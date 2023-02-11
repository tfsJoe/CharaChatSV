using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewChatter
{
    public static class ConvoParser
    {
        static string TemplatePath => Path.Combine(ModEntry.ModDirectory, "promptTemplate.txt");

        public static string ParseTemplate(NPC npc)
        {
            if (npc == null) return null;
            string prompt = File.ReadAllText(TemplatePath);
            var friendship = Game1.player.friendshipData[npc.Name];
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

            prompt = prompt
                .Replace("@npc-subj-pron-cap", npc.Gender == 0 ? "He" : "She")
                .Replace("@npc-pos-pron", npc.Gender == 0 ? "his" : "her")
                .Replace("@player-name", Game1.player.Name)
                .Replace("@player-gender", Game1.player.IsMale ? "male" : "female")
                .Replace("@player-pos-pron", Game1.player.IsMale ? "his" : "her")
                .Replace("@hearts", Game1.player.getFriendshipHeartLevelForNPC(npc.Name).ToString())
                .Replace("@location", Game1.currentLocation.Name)
                .Replace("@age", ageGroup)
                .Replace("@manners", manners) 
                .Replace("@social-anxiety", socialAnxiety)
                .Replace("@optimism", optimism)
                .Replace("@love-interest", !string.IsNullOrEmpty(npc.loveInterest) && npc.loveInterest != "null"
                    ? $". If it comes up, her love interest is {npc.loveInterest}" : "")
                .Replace("@relationship-status", friendship.IsMarried() ? "married" :
                        friendship.IsDating() ? "dating" :
                        friendship.IsDivorced() ? "divorced" :
                        friendship.IsRoommate() ? "roomates" :
                        friendship.IsEngaged() ? "engaged" :
                        Game1.player.getChildren().FirstOrDefault(c => c.Name == npc.Name) != null ? "that @npc-name is @player-name's child" :
                        "platonic")
                .Replace("@npc-name", npc.Name)
                .Replace("@player-name", Game1.player.Name);
            return prompt;
        }
    }
}