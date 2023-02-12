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
                throw new InvalidDataException("OpenAI API key has not been correctly entered in apiKeys.json. Please set this value");
            }
            
            ModEntry.Log(keyManager.openAI);
        }
    }
}