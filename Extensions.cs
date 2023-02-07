using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Accepts a string and splits it into multiple lines based on the width of the box
        /// and the horizontal size of the font.
        /// </summary>
        static List<string> GetWordWrappedLines(this SpriteFont font, string text, int lineWidth)
        {
            var lines = new List<string>();
            var line = "";
            var words = text.Split(' ');

            foreach (var word in words) // TODO: could optimize search speed w/ BST?
            {
                if (font.MeasureString(line + word).X > lineWidth)
                {
                    lines.Add(line);
                    line = "";
                }

                line += word + " ";
            }
            lines.Add(line);
            return lines;
        }

        public static void DrawWordWrappedText(this SpriteBatch spriteBatch, string text, Rectangle box,
            SpriteFont font, Color color)
        {
            var lines = font.GetWordWrappedLines(text, box.Width);
            var y = box.Y;
            foreach (var line in lines)
            {
                // spriteBatch.DrawString(font, textLine, new Vector2(box.X, y), color);
                Utility.drawTextWithShadow(spriteBatch, line, font, new Vector2(box.X, y), color);
                Utility.drawBoldText(spriteBatch, line, font, new Vector2(box.X, y), color);
                y += font.LineSpacing;
            }
        }
        
        /// <returns>X and Y coords at end of the last character of a string,
        /// given a font and text field extents.</returns>
        static Vector2 GetWordWrappedEnd(this SpriteFont font, string text, Rectangle box)
        {
            var lines = font.GetWordWrappedLines(text, box.Width);
            var x = lines.Count == 0 ? 0 : font.MeasureString(lines[lines.Count - 1]).X;
            var lineCount = lines.Count > 0 ? lines.Count : 1;
            var y = lineCount * font.LineSpacing;
            return new Vector2(x, y);
        }
    }
}
