using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace StardewChatter
{
    internal enum LoginState
    {
        noRecord,
        authorized,
        revoked,
    }
    
    internal static class CheckLoginState
    {
        public static async Task<LoginState> Check(IModHelper helper, 
            bool openAuthPage = false, System.Action<int> updateBalance = null)
        {
            var token = NetRequestUtil.GetLoginToken(helper);
            var reqPayload = new { t = token.ToString() };
            var request = NetRequestUtil.RequestPostObjToUrl(reqPayload,
                $"{Manifest.Inst.ApiRoot}/checkClientLoginState");
            HttpResponseMessage httpResponse;
            try { httpResponse = await NetRequestUtil.Client.SendAsync(request); }
            catch (HttpRequestException e)
            {
                ModEntry.Log($"Login state check failed: {e.Message}");
                return LoginState.noRecord;
            }
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                ModEntry.Log($"Login state check failed: {httpResponse.StatusCode} {httpResponse.ReasonPhrase}");
                return LoginState.noRecord;
            }
            
            var response = await httpResponse.Content.ReadAsStringAsync();
            LoginState state;
            int? balance = null;
            switch (response)
            {
                case "revoked":
                    state = LoginState.revoked;
                    break;
                case "noRecord":
                    state = LoginState.noRecord;
                    break;
                default:
                    bool success = int.TryParse(response, out var bal);
                    if (!success)
                    {
                        ModEntry.Log($"Unexpected response from login state check: {response}");
                        state = LoginState.noRecord;
                        break;
                    }
                    balance = bal;
                    state = LoginState.authorized;
                    break;
            }

            if (openAuthPage && state != LoginState.authorized)
            {
                ModEntry.Log($"Opening auth page: {Manifest.Inst.WebRoot}/authorize?t={token}");
                var startInfo = new ProcessStartInfo
                {
                    FileName = $"{Manifest.Inst.WebRoot}/authorize?t={token}",
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(startInfo);
            }

            if (balance.HasValue && updateBalance != null) updateBalance(balance.Value);

            return state;
        }
    }
}