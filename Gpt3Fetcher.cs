using System.IO;
using StardewModdingAPI;

namespace StardewChatter
{
    public class Gpt3Fetcher
    {
        public Gpt3Fetcher(IModHelper helper)
        {
            var keyManager = helper.Data.ReadJsonFile<ApiKeyManager>("apiKeys.json");
            if (keyManager == null)
            {
                throw new InvalidDataException("Failed to read apiKeys.json");
            }

            if (keyManager.openAI.Contains(' '))
            {
                throw new InvalidDataException("Open AI API key has not been correctly entered in apiKeys.json.");
            }
        }
    }
}