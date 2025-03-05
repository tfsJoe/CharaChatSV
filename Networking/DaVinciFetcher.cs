using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace CharaChatSV
{
    public sealed class DaVinciFetcher : ChatFetcher
    {
        private readonly DaVinciRequestBody requestBodyTemplate;
        protected override int RequestWaitTime => 3500;
        protected override string CompletionsUrl => "https://api.openai.com/v1/completions";
        private string chatLog = "";

        public DaVinciFetcher(IModHelper helper) : base(helper)
        {
            requestBodyTemplate = helper.Data.ReadJsonFile<DaVinciRequestBody>("davinci-requestTemplate.json");
            if (requestBodyTemplate == null)
            {
                throw new InvalidDataException("Failed to load request body template");
            }
            if (string.IsNullOrEmpty(requestBodyTemplate.model))
            {
                throw new InvalidDataException($"Request body template loaded incorrectly.\n{requestBodyTemplate}");
            }
        }

        public override void SetUpChat(NPC npc)
        {
            chatLog = ConvoParser.ParseTemplate(npc, this);
        }

        public override async Task<string> Chat(string userInput)
        {
            userInput = Sanitize(userInput);
            chatLog += $"\n@human: {userInput}\n@ai: ";
            requestBodyTemplate.prompt = chatLog;
            
            var httpResponse = await SendChatRequest(requestBodyTemplate);
            if (!httpResponse.IsSuccessStatusCode)
            {
                return $"(Failure! {(int)httpResponse.StatusCode} {httpResponse.StatusCode}: " +
                       $"{httpResponse.ReasonPhrase})";
            }
            
            string reply;
            using (var doc = await JsonDocument.ParseAsync(await httpResponse.Content.ReadAsStreamAsync()))
            {
                try
                {
                    reply = doc.RootElement.GetProperty("choices")[0].GetProperty("text").GetString();
                }
                catch (InvalidOperationException e)
                {
                    ModEntry.Log(e.Message);
                    return $"(Failure! Couldn't understand response.)";
                }
            }
            if (string.IsNullOrEmpty(reply))
            {
#if DEBUG
                ModEntry.Log(await httpResponse.Content.ReadAsStringAsync());
#endif
                return "(No response...)";
            }

            reply = Sanitize(reply);
            chatLog += reply;
            return reply;
        }
    }
}
