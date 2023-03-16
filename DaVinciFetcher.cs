using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace StardewChatter
{
    public sealed class DaVinciFetcher : ChatFetcher
    {
        private readonly DaVinciRequestBody requestBodyTemplate;
        protected override int RequestWaitTime => 3500;
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
            chatLog = ConvoParser.ParseTemplate(npc);
        }

        public override async Task<string> Chat(string userInput)
        {
            userInput = SanitizePrompt(userInput);
            chatLog += $"\n@human: {userInput}\n@ai: ";
            requestBodyTemplate.prompt = chatLog;
            var requestPayload = new StringContent(JsonSerializer.Serialize(requestBodyTemplate), Encoding.UTF8, "application/json" );
            var request = new HttpRequestMessage(HttpMethod.Post, COMPLETIONS_URL) {Content = requestPayload};
            ModEntry.Log(request.ToString());
            ModEntry.Log(await requestPayload.ReadAsStringAsync());
            await RateLimit();
            waitForRateLimit = true;
            var httpResponse = await client.SendAsync(request);
            if (!httpResponse.IsSuccessStatusCode)
            {
                ModEntry.monitor.Log($"{(int)httpResponse.StatusCode} {httpResponse.StatusCode}:" +
                                     $"{httpResponse.ReasonPhrase}\n{httpResponse.Content}");
                return $"(Failure! {(int)httpResponse.StatusCode} {httpResponse.StatusCode}: {httpResponse.ReasonPhrase})";
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

            reply = Regex.Unescape(reply);
            if (reply.Length > 1 && reply[0] == ' ')
                reply = reply.Substring(1, reply.Length - 1);
            reply = reply.Replace("@", ""); // AI sometimes uses @ before player char's name.
            chatLog += reply;
            return reply;
        }
    }
}
