using System.IO;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace StardewChatter
{
    public sealed class TurboFetcher : ChatFetcher
    {
        protected override int RequestWaitTime => 3500;

        private TurboRequestBody requestBodyTemplate;
        
        public TurboFetcher(IModHelper helper) : base(helper)
        {
            requestBodyTemplate = helper.Data.ReadJsonFile<TurboRequestBody>("turbo-requestBody.json");
            if (requestBodyTemplate == null)
            {
                throw new InvalidDataException("Failed to load request body template");
            }
            if (string.IsNullOrEmpty(requestBodyTemplate.model))
            {
                throw new InvalidDataException($"Request body template loaded incorrectly.\n{requestBodyTemplate}");
            }
        }
        
        public override Task<string> Chat(string prompt)
        {
            return Task.FromResult("hello!!");
        }
    }
}