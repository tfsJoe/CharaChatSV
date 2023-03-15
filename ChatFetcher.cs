using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace StardewChatter
{
    public abstract class ChatFetcher
    {
        protected readonly HttpClient client = new();
        private class ModVersion { public string Version { get; set; }} // Data class for parsing manifest.json
        protected abstract int RequestWaitTime { get; }
        protected const string COMPLETIONS_URL = "https://api.openai.com/v1/completions";
        protected static bool waitForRateLimit;

        public ChatFetcher(IModHelper helper)
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
        }

        public abstract Task<string> Chat(string prompt);
        
        //TODO
        protected static string SanitizePrompt(string prompt)
        {
            return prompt;
        }
        
        protected async Task RateLimit()
        {
            if (waitForRateLimit) await Task.Delay(RequestWaitTime);
            waitForRateLimit = false;
        }
    }
}