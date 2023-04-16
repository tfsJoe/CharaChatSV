using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace StardewChatter
{
    public class BackendFetcher : ChatFetcher
    {
        public static AiModel aiModel;
        private PromptParams promptParams;
        
        public BackendFetcher(IModHelper helper) : base(helper)
        {
        }

        protected override int RequestWaitTime => 1500;

        protected override string CompletionsUrl => Manifest.Inst.ApiUrl;
        
        public override void SetUpChat(NPC npc)
        {
            promptParams = new PromptParams(npc);
        }

        public override async Task<string> Chat(string userInput, Guid loginToken, Guid convoId)
        {
            userInput = Sanitize(userInput);
            var body = new BackendRequestBody(loginToken, convoId, userInput, aiModel, promptParams);
            promptParams = null;    // Only needed for first chat after setup
            var httpResponse = await SendChatRequest(body);
            if (!httpResponse.IsSuccessStatusCode)
            {
                return $"(Failure! {(int)httpResponse.StatusCode} {httpResponse.StatusCode}: " +
                       $"{httpResponse.ReasonPhrase})";
            }
            var reply = "(I'm having trouble thinking...)";
            try
            {
                using var doc = await JsonDocument.ParseAsync(await httpResponse.Content.ReadAsStreamAsync());
                //TODO: actual parse!!
                reply = doc.RootElement.GetProperty("reply").GetString();
            }
            catch (Exception e)
            {
                ModEntry.Log(e.Message);
            }
            return reply;
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
}