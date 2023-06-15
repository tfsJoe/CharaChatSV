using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace CharaChatSV
{
    public static class GameplayUtil
    {
        private static List<NPC> allCharacters;
        public static List<NPC> GetAllCharacters()
        {
            if (allCharacters != null && allCharacters.Count != 0)
                return allCharacters;
            allCharacters = new List<NPC>(64);
            foreach (var location in Game1.locations)
                allCharacters.AddRange(location.characters);
            ModEntry.Log($"All {allCharacters.Count} characters: {string.Join(' ', allCharacters.Select(c => c.Name))}");
            return allCharacters;
        }
    }
}