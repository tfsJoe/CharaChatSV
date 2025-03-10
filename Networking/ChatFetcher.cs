﻿using System;
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
        public IModHelper Helper { get; private set; }
        protected abstract int RequestWaitTime { get; }
        protected abstract string CompletionsUrl { get; }
        private readonly HttpClient client = new();
        private static bool waitForRateLimit;
        
        protected ChatFetcher(IModHelper helper)
        {
            Helper = helper;
            var modVersion = Manifest.Inst?.Version ?? "";

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

        public static ChatFetcher Instantiate(IModHelper helper)
        {
            ModEntry.Log($"CharaChatSV version: {Manifest.Inst.Version}");
            var modelSetting = Manifest.Inst?.AiModel;
            switch (modelSetting)
            {
                case "davinci":
                    ModEntry.monitor.Log($"This is an expensive AI model! It may also be deprecated.",
                        LogLevel.Alert);
                    return new DaVinciFetcher(helper);
                case "gpt":
                case "default":
                    return new GptFetcher(helper);
                default:
                    ModEntry.monitor.Log($"Did not understand AI model setting '{modelSetting}', using default.",
                        LogLevel.Warn);
                    return new GptFetcher(helper);
            }
        }

        public abstract void SetUpChat(NPC npc);
        
        /// <remarks>
        /// Subclass implementations are responsible for preparing the request and parsing the response,
        /// since both the input and output defined by the chat API may differ based on the AI model used.
        /// However, they should rely on SendChatRequest and SanitizeReply from this superclass.
        /// </remarks>
        /// <returns>The text reply from the chat AI, appropriately parsed and sanitized.</returns>
        public abstract Task<string> Chat(string userInput);

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
            if (requestBody == null) 
                throw new ArgumentNullException(nameof(requestBody), "Must not be null");
            string json;
            var options = new JsonSerializerOptions
            {
#if DEBUG
                WriteIndented = true
#endif
            };
            try { json = JsonSerializer.Serialize(requestBody, options); }
            catch (NotSupportedException)
            {
                ModEntry.Log($"Couldn't serialize requestBody! {requestBody.GetType()} {requestBody}");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            var requestPayload = new StringContent(json, Encoding.UTF8, "application/json" );
            var request = new HttpRequestMessage(HttpMethod.Post, CompletionsUrl) {Content = requestPayload};
            ModEntry.Log(request.ToString());
            ModEntry.Log($"(RequestBody)\n{json}");
            await RateLimit();
            waitForRateLimit = true;
            var httpResponse = await client.SendAsync(request);

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
        /// If the initial prompt failed despite a response (e.g. user input was moderated)
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