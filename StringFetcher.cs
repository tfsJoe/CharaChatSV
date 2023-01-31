using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace StardewChatter
{
    public class StringFetcher
    {
        private const string catFactsUrl = "https://catfact.ninja/fact?max_length=512";
        private static StringFetcher inst;
        private readonly HttpClient client = new();

        public static async Task<string> GetCatFact()
        {
            if (inst == null) inst = new();
            var response = await inst.client.GetAsync(catFactsUrl);

            if (!response.IsSuccessStatusCode)
            {
                string err = response.StatusCode.ToString();
                if (!string.IsNullOrEmpty(response.ReasonPhrase)) err += " " + response.ReasonPhrase;
                return err;
            }

            var json = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(json);

            string fact;
            try
            {
                fact = jsonDoc.RootElement.GetProperty("fact").GetString();
            }
            catch (JsonException e)
            {
                return $"(Poorly formed response: {e.Message}";
            }
            catch (KeyNotFoundException)
            {
                return $"(Couldn't extract fact)\n{json}";
            }
            return fact;
        }
    }
}
