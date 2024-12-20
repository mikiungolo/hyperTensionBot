/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace HyperTensionBot.Server.LLM.Strategy {
    // Gpt service: configuration and manage request to models
    public class GPTService : ILLMService {
        private OpenAIAPI? api;
        private string? gptKey;
        private readonly List<ChatMessage> analisysRequest = new();
        private ILogger<LLMService>? _logger;

        // Configuration
        public GPTService(WebApplicationBuilder builder) {
            ConfigureKey(builder);
            analisysRequest = Prompt.RequestContext();
        }

        private void ConfigureKey(WebApplicationBuilder builder) {
            var confGpt = builder.Configuration.GetSection("OpenAI");
            if (!confGpt.Exists() && confGpt["OpenKey"] != null)
                throw new ArgumentException("Configuration Gpt: OpenAi Key is not set");
            gptKey = confGpt["Openkey"];
            api = new OpenAIAPI(gptKey);
        }

        // Send text messages to gpt model. It's pèossibile choice model, tokens, temperature ecc..
        public async Task<string> AskLLM(TypeConversation t, string message, List<ChatMessage>? conversation = null) {
            if (api is not null) {
                /*
                if (t == TypeConversation.Communication)
                    conversation!.Add(new ChatMessage(ChatMessageRole.User, userMessage));
                else
                    analisysRequest.Add(new ChatMessage(ChatMessageRole.User, userMessage));
                */
                var response = await api.Chat.CreateChatCompletionAsync(
                    model: Model.ChatGPTTurbo,
                    messages: (IList<ChatMessage>?)(t == TypeConversation.Communication ? conversation : analisysRequest),
                    max_tokens: 200);

                if (t != TypeConversation.Communication && _logger is not null) _logger.LogDebug(response.ToString());
                return response.ToString();
            }
            return "Error Service";
        }

        public void SetLogger(ILogger<LLMService> logger) {
            _logger = logger;
        }
    }
}
