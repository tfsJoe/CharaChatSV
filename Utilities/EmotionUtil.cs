using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewChatter
{
    internal static class EmotionUtil
    {
        /// <summary>Based on inspecting the spritesheets</summary>
        private static IReadOnlyDictionary<string, IReadOnlyList<string>> CharacterEmotions =
            new Dictionary<string, IReadOnlyList<string>>
            {
                {
                    "Abigail", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Suspicious",
                        "Love",
                        "Angry",
                        "Unimpressed",
                        "Shocked",
                        "(skip)",
                        "Suspicious"
                    }
                },
                {
                    "Alex", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Playful",
                        "Love",
                        "Angry",
                        "HulkingOut",
                        "Shocked",
                        "NakedAndSad",
                        "Annoyed",
                        "PiggingOut"
                    }
                },
                {
                    "Bear", new[]
                    {
                        "Neutral",
                        "Embarrassed",
                        "Satisfied",
                        "Happy"
                    }
                },
                {
                    "Birdie", new[]
                    {
                        "Neutral",
                        "Smrik",
                        "Sad"
                    }
                },
                {
                    "Bouncer", new[]
                    {
                        "Neutral"
                    }
                },
                {
                    "Clint", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Annoyed",
                        "Surprised",
                        "Drinking"
                    }
                },
                {
                    "Demetrius", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Smirk",
                        "Annoyed",
                        "Inquisitive",
                        "Surprised"
                    }
                },
                {
                    "Dobson", new[]
                    {
                        "Neutral",
                        "Smug",
                        "Concerned",
                        "Happy"
                    }
                },
                {
                    "Dwarf", new[]
                    {
                        "Neutral"
                    }
                },
                {
                    "Elliott", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "(skip)",
                        "Love",
                        "Angry",
                        "(skip)",
                        "Unimpressed",
                        "Shocked"
                    }
                },
                {
                    "Emily", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Attentive",
                        "Love",
                        "Angry",
                        "Shocked",
                        "Spiritual"
                    }
                },
                {
                    "Evelyn", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Wistful"
                    }
                },
                {
                    "George", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Angry"
                    }
                },
                {
                    "Gil", new[]
                    {
                        "Neutral"
                    }
                },
                {
                    "Governor", new[]
                    {
                        "Neutral",
                        "Interested",
                        "Unimpressed",
                        "Sick"
                    }
                },
                {
                    "Grandpa", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad"
                    }
                },
                {
                    "Gunther", new[]
                    {
                        "Neutral"
                    }
                },
                {
                    "Gus", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Worried",
                        "Unimpressed"
                    }
                },
                {
                    "Haley", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Uninterested",
                        "Love",
                        "Angry",
                        "(skip)",
                        "Probing",
                        "Shocked",
                        "(skip)",
                        "(skip)",
                        "Kissing"
                    }
                },
                {
                    "Harvey", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Concentrating",
                        "Love",
                        "Unimpressed",
                        "(skip)",
                        "Embarrassed",
                        "Shocked"
                    }
                },
                {
                    "Henchman", new[]
                    {
                        "Neutral",
                        "Lecherous",
                        "Sad"
                    }
                },
                {
                    "Jas", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Impressed",
                        "Explaining"
                    }
                },
                {
                    "Jodi", new[]
                    {
                        "Neutral",
                        "Laughing",
                        "Sad",
                        "Annoyed",
                        "Skeptical"
                    }
                },
                {
                    "Kent", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Unimpressed",
                        "Irritated",
                        "Shocked",
                        "Exasperated"
                    }
                },
                {
                    "Krobus", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Surprised",
                        "Love",
                        "Angry",
                        "Evil",
                        "Cute",
                        "Disguised"
                    }
                },
                {
                    "Leah", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Interested",
                        "Love",
                        "Annoyed",
                        "Shocked",
                        "Unimpressed"
                    }
                },
                {
                    "Lewis", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Annoyed",
                        "Angry",
                        "Exasperated"
                    }
                },
                {
                    "Linus", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Surprised"
                    }
                },
                {
                    "Marlon", new[]
                    {
                        "Neutral"
                    }
                },
                {
                    "Marnie", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Unimpressed",
                        "Shocked"
                    }
                },
                {
                    "Maru", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Explaining",
                        "Pleading",
                        "Angry",
                        "(skip)",
                        "(skip)",
                        "Attentive",
                        "Shocked"
                    }
                },
                {
                    "Morris", new[]
                    {
                        "Neutral",
                        "Smug",
                        "Disheveled",
                        "Annoyed"
                    }
                },
                {
                    "MrQi", new[]
                    {
                        "Happy",
                        "Neutral"
                    }
                },
                {
                    "Pam", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Angry",
                        "Unimpressed"
                    }
                },
                {
                    "ParrotBoy", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Embarrassed"
                    }
                },
                {
                    "Penny", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Shy",
                        "Love",
                        "Unimpressed",
                        "(skip)",
                        "(skip)",
                        "(skip)",
                        "Angry",
                        "Crying",
                        "Attentive"
                    }
                },
                {
                    "Pierre", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Angry",
                        "Surprised"
                    }
                },
                {
                    "Robin", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Annoyed",
                        "Flattered",
                        "Uninterested",
                        "Unimpressed"
                    }
                },
                {
                    "SafariGuy", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad"
                    }
                },
                {
                    "Sam", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "Pleased",
                        "Love",
                        "Skeptical",
                        "(skip)",
                        "Attentive",
                        "Shocked",
                        "Disappointed",
                        "Bashful"
                    }
                },
                {
                    "Sandy", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Disappointed"
                    }
                },
                {
                    "Sebastian", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad",
                        "(skip)",
                        "Love",
                        "Annoyed",
                        "(skip)",
                        "Mischievous"
                    }
                },
                {
                    "Shane", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Annoyed",
                        "Resentful",
                        "Love",
                        "Angry",
                        "Pleased",
                        "Drunk",
                        "(skip)",
                        "(skip)",
                        "Shocked"
                    }
                },
                {
                    "Vincent", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Sad"
                    }
                },
                {
                    "Willy", new[]
                    {
                        "Neutral",
                        "Happy",
                        "Disappointed",
                        "Content"
                    }
                },
                {
                    "Wizard", new[]
                    {
                        "Neutral",
                        "Happy",
                    }
                }
            };

        private static Rectangle IndexToRect(int index)
        {
            var x = index % 2 * 64;
            var y = index / 2 * 64;
            return new Rectangle(x, y, 64, 64);
        }

        public static Rectangle EmotionToPortraitRect(NPC npc, string emotion)
        {
            if (string.IsNullOrEmpty(emotion))
                return new Rectangle(0, 0, 64, 64); // Hopefully first rect is neutral
            
            if (!CharacterEmotions.ContainsKey(npc.Name))
            {
                ModEntry.Log($"Don't know emotional range of NPC {npc.Name}");
                return new Rectangle(0, 0, 64, 64); 
            }
            
            var emotions = CharacterEmotions[npc.Name];
            var index = -1;
            for (var i = 0; i < emotions.Count; ++i)
            {
                if (!string.Equals(emotions[i], emotion,
                        StringComparison.InvariantCultureIgnoreCase))
                    continue;
                index = i;
                break;
            }
            // ModEntry.Log($"{npc.Name} has emotion {emotion} at index {index}");
            return IndexToRect(index);
        }
        
        public static string ExtractEmotion(ref string text)
        {
            var pattern = @"\$([a-zA-Z]+)";
            var match = Regex.Match(text, pattern);
            if (match.Success)
                text = Regex.Replace(text, pattern, "");
            return match.Success ? match.Groups[1].Value : null;
        }

        public static IEnumerable<string> GetEmotionNames(this NPC npc)
        {
            return CharacterEmotions.ContainsKey(npc.Name) ? 
                CharacterEmotions[npc.Name].Where(e => e != "(skip)") :
                null;
        }
    }
}