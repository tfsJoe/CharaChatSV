using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace CharaChatSV
{
    public class BackendFetcher : ChatFetcher
    {
        public static AiModel aiModel;
        private PromptParams promptParams;

        protected override int RequestWaitTime => 1500;

        protected override string CompletionsUrl => Manifest.Inst.ApiRoot + "/stardewChat";
        
        public override void SetUpChat(NPC npc)
        {
            ModEntry.Log($"NPC null? {npc == null}");
            promptParams = new PromptParams(npc);
        }

        public override async Task<BackendResponse> Chat(string userInput, Guid loginToken, Guid convoId)
        {
            userInput = Sanitize(userInput);
            var body = new BackendRequestBody(loginToken, convoId, userInput, aiModel, promptParams);
            var httpResponse = await SendChatRequest(body);
            if (!httpResponse.IsSuccessStatusCode)
            {
                return BackendResponse.FromFailure(
                    $"(Failure! {(int)httpResponse.StatusCode} {httpResponse.StatusCode}: " +
                    $"{httpResponse.ReasonPhrase})");
            }
            using var doc = await JsonDocument.ParseAsync(await httpResponse.Content.ReadAsStreamAsync());
            return BackendResponse.Parse(doc.RootElement);
        }

        public override void ReplySucceeded()
        {
            base.ReplySucceeded();
            promptParams = null;
        }

        public static ResponseSignal ParseResponseSignal(string response)
        {
            ResponseSignal signal = response switch
            {
                "ok" => ResponseSignal.ok,
                "final" => ResponseSignal.final,
                "tooLong" => ResponseSignal.tooLong,
                "underfunded" => ResponseSignal.underfunded,
                "moderated" => ResponseSignal.moderated,
                "obsolete" => ResponseSignal.obsolete,
                "rateLimited" => ResponseSignal.rateLimited,
                _ => ResponseSignal.unknown
            };
            return signal;
        }

        public enum AiModel
        {
            Default,
            davinci,
            turbo,
        }
        
        private class BackendRequestBody
        {
            // Lower camelCase to match backend API, serialized this way.
            public string t { get; set; }    // Login token. TODO: can we transmit GUIDs as a number? Worth doing?
            public string convoId { get; set; }
            public string playerLine { get; set; }
            public string modVersion => Manifest.Inst.Version;
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public AiModel aiModel { get; set; }
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public LocalizedContentManager.LanguageCode language => 
                LocalizedContentManager.CurrentLanguageCode;

            public PromptParams promptParams { get; set; }

            public BackendRequestBody(Guid loginToken, Guid convoGuid, string playerLine,
                AiModel aiModel = AiModel.Default, PromptParams promptParams = null)
            {
                t = loginToken.ToString();
                convoId = convoGuid.ToString();
                this.playerLine = playerLine;
                this.aiModel = aiModel;
                this.promptParams = promptParams;
            }
        }

        private class PromptParams
        {
            // Lower camelCase to match backend API, serialized this way.
            public int ageGroup { get; set; }
            public int manners { get; set; }
            public int socialAnxiety { get; set; }
            public int optimisim { get; set; }
            public int hearts { get; set; }
            public int dayNum { get; set; }
            public string relationshipStatus { get; set; }
            public string npcSubjPronCap { get; set; }
            public string npcPosPron { get; set; }
            public string npcName { get; set; }
            public string playerName { get; set; }
            public string playerGender { get; set; }
            public string playerPosPron { get; set; }
            public string location { get; set; }
            public string timeOfDay { get; set; }
            public string season { get; set; }
            public string loveInterest { get; set; }
            public string[] emotions { get; set; }

            public PromptParams(NPC npc)
            {
                ageGroup = npc.Age;
                manners = npc.Manners;
                socialAnxiety = npc.SocialAnxiety;
                optimisim = npc.Optimism;
                bool gotFriendship = Game1.player.friendshipData.TryGetValue(npc.Name, out var friendship);
                npcName = npc.Name;
                npcSubjPronCap = npc.Gender == 0 ? "He" : "She";
                npcPosPron = npc.Gender == 0 ? "his" : "her";
                playerName = Game1.player.Name;
                playerGender = Game1.player.IsMale ? "male" : "female";
                playerPosPron = Game1.player.IsMale ? "his" : "her";
                hearts = Game1.player.getFriendshipHeartLevelForNPC(npc.Name);
                location = Game1.currentLocation.Name;
                timeOfDay = Game1.getTimeOfDayString(Game1.timeOfDay);
                dayNum = Game1.dayOfMonth;
                season = Game1.currentSeason;
                loveInterest = npc.loveInterest;
                emotions = npc.GetEmotionNames()?.ToArray();
                if (string.IsNullOrEmpty(loveInterest)) loveInterest = "";
                relationshipStatus = !gotFriendship ? "strangers" :
                    friendship.IsMarried() ? "married" :
                    friendship.IsDating() ? "dating" :
                    friendship.IsDivorced() ? "divorced" :
                    friendship.IsRoommate() ? "roomates" :
                    friendship.IsEngaged() ? "engaged" :
                    Game1.player.getChildren().FirstOrDefault(c => c.Name == npc.Name) != null ?
                        $"that {npcName} is {playerName}'s child" :
                        "platonic";
            }
        }
    }

    public enum ResponseSignal
    {
        unknown,
        ok,
        final,
        tooLong,
        underfunded,
        moderated,
        obsolete,
        rateLimited,
    }

    public class BackendResponse
    {
        public string Reply { get; private set; }
        public ResponseSignal Signal { get; private set; }
        public int? Balance { get; private set; }
        
        public static BackendResponse Parse(JsonElement json)
        {
            var response = new BackendResponse
            {
                Reply = json.GetProperty("reply").GetString(),
                Signal = BackendFetcher.ParseResponseSignal(json.GetProperty("signal").GetString()),
            };
            if (json.TryGetProperty("balance", out var balance))
            {
                if (balance.ValueKind == JsonValueKind.Number)
                {
                    response.Balance = balance.GetInt32();
                }
                else
                {
                    response.Balance = null;
                } 
            }
            else response.Balance = null;
            return response;
        }

        public static BackendResponse FromFailure(string message)
        {
            return new BackendResponse
            {
                Reply = message,
                Signal = ResponseSignal.unknown,
                Balance = null,
            };
        }
    }
}