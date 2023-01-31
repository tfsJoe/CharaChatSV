using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace StardewChatter
{
    public static class Extensions
    {
        public static bool IsInConvoRange(this NPC npc)
        {
            var npcLoc = npc.getTileLocation();
            var playerLoc = Game1.player.getTileLocation();
            var xDist = Math.Abs(npcLoc.X - playerLoc.X);
            var yDist = Math.Abs(npcLoc.Y - playerLoc.Y);
            return xDist < 1f && yDist < 1f;
        }

        public static bool IsCursorOver(this NPC npc)
        {
            return npc.getTileLocation() == Game1.currentCursorTile;
        }
    }
}
