using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using StardewModdingAPI;

namespace CharaChatSV
{
    internal static class NetRequestUtil
    {
        private static HttpClient client;

        public static HttpClient Client
        {
            get
            {
                if (client != null) return client;
                client = new HttpClient();
                var modVersion = Manifest.Inst?.Version ?? "";
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CharaChatSV", modVersion));
                return client;
            }
        }

        private static Guid cachedLoginToken;
        
        public static Guid GetLoginToken(IModHelper helper)
        {
            if (cachedLoginToken != Guid.Empty) return cachedLoginToken;
            var tokenString = UserSettings.Inst.LoginToken;
            if (tokenString != null)
            {
                ModEntry.Log($"Read login token: {tokenString}");
                cachedLoginToken = Guid.Parse(tokenString);
                return cachedLoginToken;
            }
            return RefreshLoginToken(helper);
        }

        public static Guid RefreshLoginToken(IModHelper helper)
        {
            cachedLoginToken = Guid.NewGuid();
            var tokenString = cachedLoginToken.ToString();
            UserSettings.Inst.LoginToken = tokenString;
            UserSettings.Write();
            ModEntry.Log($"Made new login token {tokenString}");
            return cachedLoginToken;
        }
        
        public static HttpRequestMessage RequestPostObjToUrl(object obj, string url)
        {
            if (obj == null) 
                throw new ArgumentNullException(nameof(obj), "Must not be null");
            string json;
            var options = new JsonSerializerOptions
            {
#if DEBUG
                WriteIndented = true
#endif
            };
            try { json = JsonSerializer.Serialize(obj, options); }
            catch (NotSupportedException)
            {
                ModEntry.Log($"Couldn't serialize! {obj.GetType()} {obj}");
                throw;
            }
            var requestPayload = new StringContent(json, Encoding.UTF8, "application/json" );
            var request = new HttpRequestMessage(HttpMethod.Post, url) {Content = requestPayload};
            return request;
        }
    }
}