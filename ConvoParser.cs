using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewValley;
using StardewValley.Characters;

namespace CharaChatSV
{
    public class ConvoParser
    {
        public static ConvoParser Instance { get; private set; }
        private readonly PromptTemplateGroup templates;

        private ConvoParser(ChatFetcher source)
        {
            Instance = this;
            string templatePath = ResolveTemplatePath(source);
            templates = source.Helper.Data.ReadJsonFile<PromptTemplateGroup>(templatePath);
            if (templates == null)
            {
                throw new InvalidDataException($"Couldn't load prompt template: {templatePath}");
            }
            if (string.IsNullOrEmpty(templates.normal) || string.IsNullOrEmpty(templates.toddler))
            {
                throw new InvalidDataException($"Prompt template is invalid: {templatePath}");
            }
        }
        
        private string ResolveTemplatePath(ChatFetcher source)
        {
            string fileName;
            switch (source)
            {
                case DaVinciFetcher:
                    fileName = "davinci-promptTemplate.json";
                    break;
                case GptFetcher:
                    fileName = "gpt-promptTemplate.json";
                    break;
                default:
                    throw new ArgumentException($"Unknown fetcher type '{source.GetType().Name}; " +
                                                "no known corresponding prompt template file");
            }
            return fileName;
        }
        
        /// <returns>A string to be used as the first prompt / system message to instruct the
        /// AI model to roleplay as the specified character</returns>
        public static string ParseTemplate(NPC npc, ChatFetcher source)
        {
            if (npc == null) return null;
            Instance ??= new ConvoParser(source);

            string prompt = npc.IsPlayersToddler() ?
                Instance.templates.toddler :
                Instance.templates.normal;

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
            List<Child> children = Game1.player.getChildren();
            Child olderChild = children?.FirstOrDefault();
            Child youngerChild = children?.LastOrDefault();
            string relationshipStatus = !gotFriendship ? "strangers" :
                friendship.IsMarried() ? "married" :
                friendship.IsDating() ? "dating" :
                friendship.IsDivorced() ? "divorced" :
                friendship.IsRoommate() ? "roomates" :
                friendship.IsEngaged() ? "engaged" :
                olderChild?.getName() == npc.Name ? "that @npc-name is @player-name's oldest child" :
                youngerChild?.getName() == npc.Name ? "that @npc-name is @player-name's youngest child" :
                "platonic";

            prompt = prompt
                .Replace("@npc-subj-pron-cap", npc.Gender == 0 ? "He" : "She")
                .Replace("@npc-pos-pron", npc.Gender == 0 ? "his" : "her")
                .Replace("@player-name", Game1.player.Name)
                .Replace("@player-gender", Game1.player.IsMale ? "male" : "female")
                .Replace("@player-pos-pron", Game1.player.IsMale ? "his" : "her")
                .Replace("@parent-nickname", Game1.player.IsMale ? "Papa" : "Mama")
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
                .Replace("@family", DescribeFamily())
                .Replace("@relationship-status", relationshipStatus)
                .Replace("@npc-name", npc.Name)
                .Replace("@player-name", Game1.player.Name)
                .Replace("@emotion-tokens", GetEmotionOptionsPrompt(npc));
            return prompt;
        }

        private static string GetEmotionOptionsPrompt(NPC npc)
        {
            if (!EmotionUtil.characterEmotions.ContainsKey(npc.Name)) return " ";
            string result = "Each reply should end with a single $ prefixed token based on the tone and context of the reply, from the following list: ";
            foreach (var emotion in EmotionUtil.characterEmotions[npc.Name])
            {
                if (emotion == "(skip)") continue;
                result += $"${emotion}, ";
            }
            result = result[..^2] + ".";
            return result;
        }

        private static string DescribeFamily()
        {
            var spouse = (Game1.player.isMarriedOrRoommates() && !Game1.player.hasRoommate()) ?
                Game1.player.spouse : null;
            var engaged = Game1.player.isEngaged() ? Game1.player.spouse : null;
            var children = Game1.player.getChildren()?.Select(c => 
                $"{c.Name} ({(c.Gender == 0 ? "son" : "daughter")})").ToArray();
            var dating = GameplayUtil.GetAllCharacters().Where(c =>
                    Game1.player.friendshipData.ContainsKey(c.Name) &&
                    Game1.player.friendshipData[c.Name].Status == FriendshipStatus.Dating)
                .Select(lover => lover.Name).ToArray();
            var exes = GameplayUtil.GetAllCharacters().Where(c =>
                    Game1.player.friendshipData.ContainsKey(c.Name) &&
                    Game1.player.friendshipData[c.Name].IsDivorced())
                .Select(ex => ex.Name).ToArray();
            
            if (spouse == null && engaged == null && children?.Length == 0 &&
                dating.Length == 0 && exes.Length == 0)
            {
                return "The farmer has no family in Stardew Valley.";
            }

            string result = "The farmer's family details: ";

            if (!string.IsNullOrEmpty(spouse))
            {
                result += $"spouse: {spouse}, ";
            }
            if (!string.IsNullOrEmpty(engaged))
            {
                result += $"engaged to: {engaged}, ";
            }
            if (children?.Length > 0)
            {
                result += $"children: {string.Join(", ", children)}, ";
            }
            if (dating.Length > 0)
            {
                result += $"dating: {string.Join(", ", dating)}, ";
            }
            if (exes.Length > 0)
            {
                result += $"exes: {string.Join(", ", exes)}, ";
            }
            
            return result;
        }
        
        private class PromptTemplateGroup
        {
            public string normal;
            public string toddler;
        }
    }
}