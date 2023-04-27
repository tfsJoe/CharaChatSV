using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace StardewChatter
{
    public class PopcornReportWidget : IDrawable
    {
        private readonly IModHelper helper;
        private readonly Texture2D boxTexture;
        private readonly Texture2D bagTexture;
        private readonly Rectangle bagPlacement;
        private int? popcornCount;
        
        private static readonly Rectangle EmptyBagRect = new Rectangle(0, 0, 64, 64);
        private static readonly Rectangle FullBagRect = new Rectangle(64, 0, 64, 64);

        public PopcornReportWidget(IModHelper helper, Texture2D boxTexture, int xPos, int yPos)
        {
            this.helper = helper;
            this.boxTexture = boxTexture;
            bagPlacement = new Rectangle(xPos, yPos, 128, 128);
            bagTexture = helper.ModContent.Load<Texture2D>("assets/PopcornBag.png");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var bagRect = popcornCount.HasValue ? 
                (popcornCount > 10 ? FullBagRect : EmptyBagRect) :
                EmptyBagRect;
            spriteBatch.Draw(bagTexture, bagPlacement, bagRect, Color.White);
        }
    }
}