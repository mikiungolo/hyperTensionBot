using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using OpenAI_API;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Concurrent;
using System.CodeDom;

namespace HyperTensionBot.Server.LLM {
    public class LLMService {

        private readonly HttpClient _httpClient = new HttpClient();
        // URL dell'API del LLM
        private string? _llmApiUrl;
        // set names to different model 
        private readonly string MODEL_COMUNICATION = "nous-hermes2-mixtral";
        private readonly string MODEL_REQUEST = "nous-hermes2-mixtral";
        private readonly string MODEL_INSERT = "nous-hermes2-mixtral";

        private List<ChatMessage> analysistInsert; 
        private List<ChatMessage> analysisRequest;

        private ILogger<LLMService>? _logger; 


        // Build option for client 
        public LLMService(WebApplicationBuilder builder) {
            ConfigureUrl(builder);
            _httpClient.Timeout = TimeSpan.FromSeconds(200);

            analysisRequest = Prompt.RequestContext();
            analysistInsert = Prompt.InsertContest(); 
        }
        public void SetLogger(ILogger<LLMService> logger) { _logger = logger; }

        private void ConfigureUrl(WebApplicationBuilder builder) {
            var buildCluster = builder.Configuration.GetSection("Clusters");
            if (!buildCluster.Exists() && buildCluster["UrlLLM"] != null)
                throw new ArgumentException("Configuration Cluster: Url Cluster is not set");
            _llmApiUrl = buildCluster["UrlLLM"];
        }

        // connection and interaction with server for request to LLM 
        public async Task<string> AskLlm(TypeConversation t, string message, List<ChatMessage>? comunicationChat = null) {

            if (_llmApiUrl != "") {

                string modelName = "";
                List<ChatMessage> chatContext = new();
                AssignInput(t, ref chatContext, comunicationChat, ref modelName); 

                //modelName = (t == TypeConversation.Communication)? MODEL_COMUNICATION: MODEL_REQUEST;

                // build payload JSON
                var jsonPayload = new {
                    model = modelName,
                    prompt = chatContext!.First().Content + message,
                    messages = chatContext,
                    stream = false,
                };

                var content = new StringContent(JsonConvert.SerializeObject(jsonPayload), Encoding.UTF8, "application/json");

                // send POST request 
                try {
                    var response = await _httpClient.PostAsync(_llmApiUrl, content);

                    if (response.IsSuccessStatusCode) {

                        var jsonResponse = await response.Content.ReadAsStringAsync();

                        // extracting text by LLM response 
                        return ParserResponse(jsonResponse, t);
                    }

                    return "Si è verificato un errore nella generazione del testo.";
                }
                catch (TaskCanceledException) {
                    return "Errore dal server";
                }
            }
            return "Non è possibile rispondere a queste domande. Riprova più tardi. "; 
        }

        private void AssignInput(TypeConversation t, ref List<ChatMessage> chatContext, List<ChatMessage>? comunication, ref string modelName) {
            switch(t) {
                case TypeConversation.Request:
                    modelName = MODEL_REQUEST;
                    chatContext = analysisRequest;
                break;
                case TypeConversation.Insert:
                    modelName = MODEL_INSERT;
                    chatContext = analysistInsert;
                break;
                default:
                    modelName = MODEL_COMUNICATION;
                    chatContext = comunication!;
                break;
            }
        }

        private string ParserResponse(string response, TypeConversation t) {
            JObject jsonObj = JObject.Parse(response);

            // Estrai il valore della chiave 'response'
            var resp = jsonObj["response"]?.ToString();

            // Rimuovi i caratteri di newline e ritorna la risposta
            resp = resp!.Replace("\\n", "");
            if (t != TypeConversation.Communication && _logger is not null) _logger.LogDebug(resp); 
            return resp;
        }
    }
}
