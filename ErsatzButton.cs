using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.HomeRenovations;

namespace StardewChatter
{
    // I know the game already has buttons, perhaps via ClickableTextureComponent?
    // But I wasn't able to make sense of them quickly, and I figured I could get this up and running faster.
    public class ErsatzButton
    {
        public Texture2D texture;
        public string label;
        public Rectangle rect;
        public event Action onClick;

        public ErsatzButton(Texture2D texture, string label, Rectangle rect, Action onClick)
        {
            this.texture = texture;
            this.label = label;
            this.rect = rect;
            this.onClick += onClick;
        }
        
        public void DetectClick(int x, int y)
        {
            if (onClick != null && rect.Contains(x, y))
                onClick.Invoke();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var font = Game1.smallFont;
            spriteBatch.Draw(texture, rect, Color.White);
            var labelSize = font.MeasureString(label);
            var labelPos = new Vector2
            {
                X = rect.X + (rect.Width - labelSize.X) / 2,
                Y = rect.Y + (rect.Height - labelSize.Y) / 2,
            };
            spriteBatch.DrawString(font, label, labelPos,Color.Black);
        }
    }
}