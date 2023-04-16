using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace StardewChatter
{
    public sealed class TurboFetcher : ChatFetcher
    {
        protected override int RequestWaitTime => 3500;
        protected override string CompletionsUrl => "https://api.openai.com/v1/chat/completions";

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
                "gpt-3.5-turbo-0301 does not always pay strong attention to system messages.
                Future models will be trained to pay stronger attention to system messages."
                    https://platform.openai.com/docs/guides/chat/introduction
                Therefore, short instructions as a system message, detailed as a user message. */
                new (TurboMessage.Role.system, 
                    $"You are engaging in a roleplay as the character {npc.Name} in Stardew Valley."),
                new (TurboMessage.Role.user, ConvoParser.ParseTemplate(npc, this)),
            };
        }

        public override async Task<string> Chat(string userInput, Guid _, Guid __)
        {
            userInput = Sanitize(userInput);
            messageHistory.Add(new TurboMessage(TurboMessage.Role.user, userInput));
            requestBodyTemplate.messages = messageHistory;
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
                    var choice0 = doc.RootElement.GetProperty("choices")[0];
                    var messageElement = choice0.GetProperty("message");
                    var role = messageElement.GetProperty("role").GetString();
                    if (TurboMessage.Role.assistant.ToString() != role)
                    {
                        return $"(Failure! Unexpected message role: {role})";
                    }
                    var finishReason = choice0.GetProperty("finish_reason").GetString();
                    if (finishReason == "content_filter")
                    {
                        messageHistory.RemoveAt(messageHistory.Count - 1);
                        return "(I'm speechless...)$s";
                    }
                    reply = messageElement.GetProperty("content").GetString();
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

            reply = SanitizeReply(reply);
            messageHistory.Add(new TurboMessage(TurboMessage.Role.assistant, reply));
            return reply;
        }
    }
}