using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;

namespace CharaChatSV
{
    public static class Extensions
    {
        public static bool IsInConvoRange(this NPC npc)
        {
            var sqDist = Vector2.DistanceSquared(npc.Tile, Game1.player.Tile);
            return sqDist <= 2;
        }


        public static bool IsDialogueEmpty(this NPC npc)
        {
            return npc.CurrentDialogue == null || npc.CurrentDialogue.Count == 0;
        }

        private static List<Child> playerChildren = null;
        /* Possible minor bug: if the player has a new child and plays through without stopping until it
        is 57 days old, it will not be included in this list and the mod could prevent the player from
        kissing the child. The alternatives are: logic to refresh this list when a child is born (cumbersome)
        or checking the children list every time the player right clicks (inefficient,)
        so I am ok with this for now. */
        private static List<Child> PlayerChildren
        {
            get
            {
                if (playerChildren != null) return playerChildren;
                playerChildren = Game1.player.getChildren();
                return playerChildren;
            }
        }
        
        public static bool IsPlayersToddler(this NPC npc)
        {
            var child = npc as Child;
            if (child == null) return false;
            return child.daysOld.Value >= 57 && PlayerChildren.Contains(child);
        }

        public static bool CanChat(this NPC npc)
        {
            var child = npc as Child;
            bool tooYoung = false;
            if (child != null)
            {
                tooYoung = child.daysOld.Value < 57;
            }

            bool needsKiss = false;
            if (npc.Name == Game1.player.spouse)
            {
                needsKiss = !npc.hasBeenKissedToday.Value;
            }

            if (!tooYoung && child != null && PlayerChildren.Contains(child))
            {
                needsKiss = !Game1.player.hasTalkedToFriendToday(child.Name);
            }

            return Game1.player.ActiveObject == null &&
                   npc.IsInConvoRange() && npc.IsDialogueEmpty() &&
                   !npc.IsMonster && npc is not Pet &&
                   !tooYoung && !needsKiss;
        }

        public static void DrawPortrait(this NPC npc, SpriteBatch batch,
            string emotion = "neutral", Rectangle? destinationRect = null)
        {
            destinationRect ??= new Rectangle(Game1.viewport.Width - 256, 0, 128, 128);
            batch.Draw(texture: npc.Portrait, destinationRectangle: destinationRect.Value,
                sourceRectangle: EmotionUtil.EmotionToPortraitRect(npc, emotion), Color.White);
        }

        public static void DrawPortrait(this NPC npc, SpriteBatch batch,
            Rectangle emotionSourceRect, Rectangle? destinationRect = null)
        {
            if (npc.Portrait == null) return;
            destinationRect ??= new Rectangle(Game1.viewport.Width - 256, 0, 128, 128);
            batch.Draw(texture: npc.Portrait, destinationRectangle: destinationRect.Value,
                sourceRectangle: emotionSourceRect, Color.White);
        }


        /// <summary>
        /// Accepts a string and splits it into multiple lines based on the width of the box
        /// and the horizontal size of the font.
        /// </summary>
        private static List<string> GetWordWrappedLines(this SpriteFont font, string text, int lineWidth)
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

        public static void DrawWordWrappedText(this SpriteBatch spriteBatch, string text,
            Rectangle box, SpriteFont font, Color color)
        {
            var lines = font.GetWordWrappedLines(text, box.Width);
            var y = box.Y;
            foreach (var line in lines)
            {
                // spriteBatch.DrawString(font, textLine, new Vector2(box.X, y), color);
                Utility.drawTextWithShadow(spriteBatch, line, font, new Vector2(box.X, y), color);
                y += font.LineSpacing;
            }
        }

        public static void DrawAndTruncateWordWrappedText(this SpriteBatch spriteBatch, ref string text,
            Rectangle box, SpriteFont font, Color color)
        {
            var lines = font.GetWordWrappedLines(text, box.Width);
            var y = box.Y;
            for (var i = 0; i < lines.Count; ++i)
            {
                if (y > box.Height + box.Y && i > 0)
                {
                    ModEntry.Log($"Lines y got to {y} but box height was {box.Height}");
                    text = string.Join("", lines.GetRange(0, i));
                    return;
                }

                Utility.drawTextWithShadow(spriteBatch, lines[i], font, new Vector2(box.X, y), color);
                y += font.LineSpacing;
            }
        }
        
        /// <returns>X and Y coords at end of the last character of a string,
        /// given a font and text field extents.</returns>
        public static Point GetWordWrappedEnd(this SpriteFont font, string text, Rectangle box)
        {
            var lines = font.GetWordWrappedLines(text, box.Width);
            var x = lines.Count == 0 ? 0 : (int)font.MeasureString(lines[lines.Count - 1]).X;
            var lineCount = lines.Count > 0 ? lines.Count : 1;
            var y = lineCount * font.LineSpacing;
            return new Point(x, y);
        }

        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        // From https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}