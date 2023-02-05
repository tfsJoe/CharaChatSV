﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            var sqDist = Vector2.DistanceSquared(npc.getTileLocation(), Game1.player.getTileLocation());
            return sqDist <= 2;
        }

        public static bool IsCursorOver(this NPC npc)
        {
            return npc.getTileLocation() == Game1.currentCursorTile;
        }

        public static bool IsDialogueEmpty(this NPC npc)
        {
            if (npc.CurrentDialogue == null) return true;
            if (npc.CurrentDialogue.Count == 0) return true;
            return false;
        }

        public static bool CanChat(this NPC npc)
        {
            return npc.IsCursorOver() && npc.IsInConvoRange() && npc.IsDialogueEmpty();
        }

        public static void DrawPortrait(this NPC npc, SpriteBatch batch,
            Emotion emotion = Emotion.Neutral, Rectangle? destinationRect = null)
        {
            destinationRect ??= new Rectangle(Game1.viewport.Width - 256, 0, 128, 128);
            batch.Draw(texture: npc.Portrait, destinationRectangle: destinationRect.Value,
                sourceRectangle: PortraitUtil.EmotionToPortraitRect(emotion), Color.White);
        }
    }
}
