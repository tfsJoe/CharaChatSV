using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace CharaChatSV
{
    public abstract class ChatFetcher
    {
        protected abstract int RequestWaitTime { get; }
        protected abstract string CompletionsUrl { get; }
        protected static bool waitForRateLimit;

        public static ChatFetcher Instantiate(IModHelper helper)
        {
            /* There used to be several subclasses (one for each AI model) but since we always go through the
             backend now, this is no longer needed, and there's no need to decide in this factory method.
             The AI model setting is a static member of BackendFetcher now.*/
            return new BackendFetcher();
        }

        public abstract void SetUpChat(NPC npc);
        
        /// <remarks>
        /// Subclass implementations are responsible for preparing the request and parsing the response,
        /// since both the input and output defined by the chat API may differ based on the AI model used.
        /// However, they should rely on SendChatRequest and SanitizeReply from this superclass.
        /// </remarks>
        /// <returns>A tuple containing a signal describing any issues encountered while generating response,
        /// and the text reply from the chat AI, appropriately parsed and sanitized.</returns>
        public abstract Task<BackendResponse> Chat(string userInput, Guid loginToken, Guid convoId);

        protected static string Sanitize(string userInput)
        {
            if (userInput.Length > TextInput.MAX_CHAR_COUNT)
            {
                ModEntry.Log($"Prompt too long ({userInput.Length} chars), truncating to {TextInput.MAX_CHAR_COUNT} chars");
                userInput = userInput.Substring(0, TextInput.MAX_CHAR_COUNT);
            }

            return userInput;
        }
        
        protected async Task RateLimit()
        {
            if (waitForRateLimit) await Task.Delay(RequestWaitTime);
            waitForRateLimit = false;
        }
        
        protected async Task<HttpResponseMessage> SendChatRequest(object requestBody)
        {
            HttpRequestMessage request;
            try
            {
                request = NetRequestUtil.RequestPostObjToUrl(requestBody, CompletionsUrl);
            }
            catch (NotSupportedException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            ModEntry.Log(request.ToString());
            await RateLimit();
            waitForRateLimit = true;
            HttpResponseMessage httpResponse = null;
            try { httpResponse = await NetRequestUtil.Client.SendAsync(request); }
            catch (HttpRequestException e)
            {
                httpResponse = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    ReasonPhrase = e.Message
                };
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                ModEntry.monitor.Log($"{(int)httpResponse.StatusCode} {httpResponse.StatusCode}:" +
                                     $"{httpResponse.ReasonPhrase}\n{httpResponse.Content}");
            }
            RateLimit();    // Not awaited so we can reset countown if user takes longer than wait time (very likely.)
            return httpResponse;
        }
        
        /// <summary>
        /// Some subclasses may need to clean up the initial prompt, but only if the reply was successful.
        /// If the initial prompt failed despite a response (e.g. out of popcorn, user input was moderated, etc,
        /// then the prompt needs to be retained for the next attempt.
        /// </summary>
        public virtual void ReplySucceeded() {}

        public static string SanitizeReply(string reply)
        {
            reply = Regex.Unescape(reply);
            if (reply.Length > 1 && reply[0] == ' ')
                reply = reply.Substring(1, reply.Length - 1);
            reply = reply.Replace("@", ""); // AI sometimes uses @ before player char's name.
            //AI also sometimes surrounds text with quotes.
            if (reply.Length > 1 && reply[0] == '"' && reply[^1] == '"')
                reply = reply.Substring(1, reply.Length - 2);
            return reply;
        }
    }
}