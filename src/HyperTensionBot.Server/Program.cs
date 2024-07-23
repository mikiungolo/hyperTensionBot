using HyperTensionBot.Server.Bot;
using HyperTensionBot.Server.Bot.Extensions;
using HyperTensionBot.Server.Database;
using HyperTensionBot.Server.LLM;
using HyperTensionBot.Server.LLM.Strategy;
using HyperTensionBot.Server.ModelML;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using System.Diagnostics;
using TelegramUpdate = Telegram.Bot.Types.Update;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureTelegramBot();

builder.Services.AddSingleton<ConfigurationManager>(builder.Configuration);
builder.Services.AddSingleton<Memory>();

// Add model and llm
builder.Services.AddSingleton(new ClassificationModel());

// Change the strategy - LLM - with class Ollama or gpt
builder.Services.AddSingleton(new LLMService(await OllamaService.CreateAsync(builder))); // new GPTService(builder))

var app = builder.Build();

// Configure the bot and timer to alert patients
app.SetupTelegramBot();
app.Services.GetRequiredService<LLMService>().SetLogger(app.Services.GetRequiredService<ILogger<LLMService>>());

// Create a Telegram bot client
var botClient = app.Services.GetRequiredService<TelegramBotClient>();

await botClient.DeleteWebhookAsync();


// Configure the receiver options for polling
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
};

var cts = new CancellationTokenSource();

// Start receiving updates using polling
botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token
);

Console.WriteLine("Bot is listening for updates...");
Console.ReadLine(); // Prevents the application from exiting immediately


async Task HandleUpdateAsync(ITelegramBotClient botClient, TelegramUpdate update, CancellationToken cancellationToken)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var memory = app.Services.GetRequiredService<Memory>();
    var model = app.Services.GetRequiredService<ClassificationModel>();
    var llm = app.Services.GetRequiredService<LLMService>();
    var internalPOST = false; // Initialize flag
    TelegramBotClient localBotClient = (TelegramBotClient) botClient;

    try
    {
        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            var messageText = update.Message.Text;
            var chatId = update.Message.Chat.Id;
            var from = update.Message.From;
            var date = Time.Convert(update.Message.Date);

            // Add message to model input and predict intent
            var input = new ModelInput { Sentence = messageText };
            var result = model.Predict(input);

            memory.HandleUpdate(from, date, result, messageText);
            logger.LogInformation("Chat {0} incoming {1}", chatId, update.Type switch
            {
                UpdateType.Message => $"message with text: {messageText}",
                UpdateType.CallbackQuery => $"callback with data: {update.CallbackQuery?.Data}",
                _ => "update of unhandled type"
            });
            logger.LogInformation("Incoming message matches intent {0}", result);

            // Manage operations
            Stopwatch stopwatch = Stopwatch.StartNew();
            await Context.ControlFlow(localBotClient, llm, memory, result, messageText, update.Message.Chat, date);
            stopwatch.Stop();
            logger.LogInformation($"Tempo di elaborazione impiegato: {stopwatch.ElapsedMilliseconds / 1000} s");
        }
        else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data != null)
        {
            var chatId = update.CallbackQuery.Message.Chat.Id;
            await Context.ManageButton(update.CallbackQuery.Data, update.CallbackQuery.From, update.CallbackQuery.Message.Chat, localBotClient, memory, llm);
            if (!update.CallbackQuery.Data.StartsWith("yes") && !update.CallbackQuery.Data.StartsWith("no"))
                await Request.ModifyParameters(localBotClient, chatId, memory, update.CallbackQuery.Data, update.CallbackQuery.Message.MessageId, llm);
            // Removing inline keyboard
            await localBotClient.DeleteMessageAsync(chatId, update.CallbackQuery.Message.MessageId);
        }
        else
        {
            return;
        }
    }
    catch (Exception e)
    {
        logger.LogError(e, "Error handling update");
    }
}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(exception, "Error occurred");
    return Task.CompletedTask;
}
