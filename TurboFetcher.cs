using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace StardewChatter
{
    public sealed class TurboFetcher : ChatFetcher
    {
        protected override int RequestWaitTime => 3500;

        private TurboRequestBody requestBodyTemplate;
        private List<TurboMessage> messageHistory = new();
        
        public TurboFetcher(IModHelper helper) : base(helper)
        {
            requestBodyTemplate = helper.Data.ReadJsonFile<TurboRequestBody>("turbo-requestTemplate.json");
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
            messageHistory = new List<TurboMessage>
            {
                /* Open AI warns:
                gpt-3.5-turbo-0301 does not always pay strong attention to system messages.
                Future models will be trained to pay stronger attention to system messages.
                https://platform.openai.com/docs/guides/chat/introduction
                Therefore, short instructions as a system message, detailed as a user message. */
                new TurboMessage(TurboMessage.Role.system, "You are engaging in a roleplay as a character in Stardew Valley."),
                new TurboMessage(TurboMessage.Role.user, ConvoParser.ParseTemplate(npc)),
            };
        }

        public override Task<string> Chat(string userInput)
        {
            // TODO
            return Task.FromResult("hello!!");
        }
    }
}