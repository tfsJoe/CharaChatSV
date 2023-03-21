using System;
using System.IO;
using System.Linq;
using StardewValley;

namespace StardewChatter
{
    public static class ConvoParser
    {
        private static string ResolveTemplatePath(ChatFetcher source)
        {
            string fileName;
            switch (source)
            {
                case DaVinciFetcher:
                    fileName = "davinci-promptTemplate.txt";
                    break;
                case TurboFetcher:
                    fileName = "turbo-promptTemplate.txt";
                    break;
                default:
                    throw new ArgumentException($"Unknown fetcher type '{source.GetType().Name}; " +
                                                "no known corresponding prompt template file");
            }
            return Path.Combine(ModEntry.ModDirectory, fileName);
        }

        public static string ParseTemplate(NPC npc, ChatFetcher source)
        {
            if (npc == null) return null;
            // TODO: cache so we aren't reading template files off disk every chat turn.
            string prompt = File.ReadAllText(ResolveTemplatePath(source));

            string ageGroup = npc.Age switch
            {
                0 => "Age: adult, ",
                1 => "Age: teen, ",
                2 => "Age: child, ",
                _ => ""
            };

            string manners = npc.Manners switch
            {
                1 => "manners: polite, ",
                2 => "manners: rude, ",
                _ => ""
            };

            string socialAnxiety = npc.SocialAnxiety switch
            {
                1 => "shy, ",
                _ => ""
            };

            string optimism = npc.Optimism switch
            {
                1 => "negative",
                _ => ""
            };
            
            bool gotFriendship = Game1.player.friendshipData.TryGetValue(npc.Name, out var friendship);
            string relationshipStatus = !gotFriendship ? "strangers" :
                friendship.IsMarried() ? "married" :
                friendship.IsDating() ? "dating" :
                friendship.IsDivorced() ? "divorced" :
                friendship.IsRoommate() ? "roomates" :
                friendship.IsEngaged() ? "engaged" :
                Game1.player.getChildren().FirstOrDefault(c => c.Name == npc.Name) != null ? "that @npc-name is @player-name's child" :
                "platonic";

            prompt = prompt
                .Replace("@npc-subj-pron-cap", npc.Gender == 0 ? "He" : "She")
                .Replace("@npc-pos-pron", npc.Gender == 0 ? "his" : "her")
                .Replace("@player-name", Game1.player.Name)
                .Replace("@player-gender", Game1.player.IsMale ? "male" : "female")
                .Replace("@player-pos-pron", Game1.player.IsMale ? "his" : "her")
                .Replace("@hearts", Game1.player.getFriendshipHeartLevelForNPC(npc.Name).ToString())
                .Replace("@location", Game1.currentLocation.Name)
                .Replace("@time-of-day", Game1.getTimeOfDayString(Game1.timeOfDay))
                .Replace("@day-num", Game1.dayOfMonth.ToString())
                .Replace("@season", Game1.currentSeason)
                .Replace("@age", ageGroup)
                .Replace("@manners", manners) 
                .Replace("@social-anxiety", socialAnxiety)
                .Replace("@optimism", optimism)
                .Replace("@love-interest", !string.IsNullOrEmpty(npc.loveInterest) && npc.loveInterest != "null"
                    ? $". If it comes up, @npc-name's love interest is {npc.loveInterest}" : "")
                .Replace("@relationship-status", relationshipStatus)
                .Replace("@npc-name", npc.Name)
                .Replace("@player-name", Game1.player.Name);
            return prompt;
        }
    }
}