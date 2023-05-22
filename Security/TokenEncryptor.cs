using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace CharaChatSV.Security
{
    internal static class TokenEncryptor
    {
        /// <summary>
        /// Asks the server what it thinks this client instance's IP address is, to get around any funky networking stuff.
        /// </summary>
        private static async Task<string> GetVisibleIpAddress()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Manifest.Inst.ApiRoot + "/echoIp");
            var response = await NetRequestUtil.Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ModEntry.monitor.Log( "Could not contact the Chara.Chat server.", LogLevel.Error);
                return null;
            }
            return await response.Content.ReadAsStringAsync();;
        }

        /// <summary>
        /// Uses IP address as visible to the server to encrypt a message. IP address is used because we don't have
        /// a way to share a password ahead of time. The goal is simply to make the login token GUID unreadable
        /// when it is opened up in plaintext in the web browser. Since players may stream the game, this would
        /// leak the token; if the server decrypts it using the IP address, an attacker would need both the token
        /// and the player's IP address at the time they authorized it to decrypt the token. They'd also
        /// need to spoof the IP address.
        /// </summary>
        public static async Task<string> GetEncryptedToken(string plaintextToken)
        {
            const string extraSauce = "g?Z,mbSoYyJ>4cvi:";
            var ip = await GetVisibleIpAddress();
            var result = Encrypt(plaintextToken, ip + extraSauce);
            return result;
        }

        private static string Encrypt(string message, string password)
        {
            // Convert the strings to bytes
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            // Encrypt the message using XOR. This is not very secure,
            // but the goal is only to avoid printing the message in plaintext in the user's browser URL bar.
            var encryptedBytes = new byte[messageBytes.Length];
            for (var i = 0; i < messageBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(messageBytes[i] ^ passwordBytes[i % passwordBytes.Length]);
            }

            // Convert the encrypted bytes to a base64 string
            return Convert.ToBase64String(encryptedBytes);
        }

#if DEBUG
        public static async Task<string> TestServerDecrypt(string encryptedMessage)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                Manifest.Inst.ApiRoot + $"/testDecrypt?message={encryptedMessage}");
            var response = await NetRequestUtil.Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ModEntry.monitor.Log( $"Test decrypt error: {response.StatusCode}", LogLevel.Error);
                return null;
            }
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> TestHello()
        {
            var encryptedMessage = await GetEncryptedToken("hello");
            ModEntry.Log($"Encrypted message: {encryptedMessage}");
            var result = await TestServerDecrypt(encryptedMessage);
            ModEntry.Log($"Server decrypted message: {result}");
            return result;
        }
        #endif
    }
}