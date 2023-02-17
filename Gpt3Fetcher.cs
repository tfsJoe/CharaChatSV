using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Force.DeepCloner;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;

namespace StardewChatter
{
    public class Gpt3Fetcher
    {
        private readonly HttpClient client = new();
        private readonly Gpt3RequestBody requestBodyTemplate;
        private const string COMPLETIONS_URL = "https://api.openai.com/v1/completions";
        // For rate limiting. See limits: https://platform.openai.com/docs/guides/rate-limits/overview
        private const int REQUEST_WAIT_TIME = 3500;
        private static bool waitForRateLimit;
        private class ModVersion { public string Version { get; set; }} // Data class for parsing manifest.json

        public Gpt3Fetcher(IModHelper helper)
        {
            var modVersion = helper.Data.ReadJsonFile<ModVersion>("manifest.json")?.Version;
            if (modVersion == null) modVersion = "";
            
            var keyManager = helper.Data.ReadJsonFile<ApiKeyManager>("apiKeys.json");
            if (keyManager == null)
            {
                throw new InvalidDataException("Failed to read apiKeys.json");
            }
            if (keyManager.openAI.Contains(' '))
            {
                throw new InvalidDataException("OpenAI API key has not been correctly entered in apiKeys.json. Please set this value");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", keyManager.openAI);
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("StardewChatter", modVersion));
            requestBodyTemplate = helper.Data.ReadJsonFile<Gpt3RequestBody>("requestTemplate.json");
            if (requestBodyTemplate == null)
            {
                throw new InvalidDataException("Failed to load request body template");
            }
            if (string.IsNullOrEmpty(requestBodyTemplate.model))
            {
                throw new InvalidDataException($"Request body template loaded incorrectly.\n{requestBodyTemplate}");
            }
        }

        public async Task<string> Chat(string prompt)
        {
            requestBodyTemplate.prompt = SanitizePrompt(prompt);
            var requestPayload = new StringContent(JsonSerializer.Serialize(requestBodyTemplate), Encoding.UTF8, "application/json" );
            var request = new HttpRequestMessage(HttpMethod.Post, COMPLETIONS_URL) {Content = requestPayload};
            ModEntry.Log(request.ToString());
            ModEntry.Log(await requestPayload.ReadAsStringAsync());
            if (waitForRateLimit) await RateLimit();
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
            return reply;
        }

        //TODO
        private static string SanitizePrompt(string prompt)
        {
            return prompt;
        }

        private static async Task RateLimit()
        {
            if (waitForRateLimit) await Task.Delay(REQUEST_WAIT_TIME);
            waitForRateLimit = false;
        }
    }
}
