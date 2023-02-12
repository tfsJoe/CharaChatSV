using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Force.DeepCloner;
using StardewModdingAPI;

namespace StardewChatter
{
    public class Gpt3Fetcher
    {
        private readonly HttpClient client = new();
        private readonly Gpt3RequestBody requestBodyTemplate;
        private const string completionsUrl = "https://api.openai.com/v1/completions";

        public Gpt3Fetcher(IModHelper helper)
        {
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
            requestBodyTemplate = helper.Data.ReadJsonFile<Gpt3RequestBody>("apiKeys.json");
        }

        public async Task<string> Chat(string prompt)
        {
            var requestBody = requestBodyTemplate.ShallowClone();
            requestBody.prompt = SanitizePrompt(prompt);
            var requestPayload = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json" );
            var request = new HttpRequestMessage(HttpMethod.Post, completionsUrl) {Content = requestPayload};
            var httpResponse = await client.SendAsync(request);
            if (!httpResponse.IsSuccessStatusCode)
            {
                return $"Failure {httpResponse.StatusCode}: {httpResponse.ReasonPhrase}";
            }
            string reply;
            using (var doc = await JsonDocument.ParseAsync(await httpResponse.Content.ReadAsStreamAsync()))
            {
                reply = doc.RootElement.GetProperty("choices").GetProperty("text").GetString();
            }
            if (string.IsNullOrEmpty(reply))
            {
#if DEBUG
                ModEntry.Log(await httpResponse.Content.ReadAsStringAsync());
#endif
                return "(No response...)";
            }
            return Regex.Unescape(reply);
        }

        //TODO
        private static string SanitizePrompt(string prompt)
        {
            return prompt;
        }
    }
}