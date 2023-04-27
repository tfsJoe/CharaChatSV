using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewChatter
{
    internal class PopcornReportWidget : IDrawable
    {
        private readonly Texture2D bagTexture;
        private readonly ConvoWindow parent;

        public int? popcornCount;
        
        private static readonly Rectangle EmptyBagRect = new Rectangle(0, 0, 48, 48);
        private static readonly Rectangle FullBagRect = new Rectangle(48, 0, 48, 48);
        
        private Rectangle BagDestination => new(parent.PopcornWidgetAnchor.X, parent.PopcornWidgetAnchor.Y, 96, 96);

        public PopcornReportWidget(ConvoWindow parent)
        {
            this.parent = parent;
            bagTexture = parent.helper.ModContent.Load<Texture2D>("assets/PopcornBag.png");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var bagDest = BagDestination;
            var bagRect = popcornCount.HasValue ? 
                (popcornCount > 10 ? FullBagRect : EmptyBagRect) :
                EmptyBagRect;
            spriteBatch.Draw(bagTexture, bagDest, bagRect, Color.White);
            var stringPlacement = new Vector2(bagDest.X + bagDest.Width + 16, bagDest.Y + 32);
            spriteBatch.DrawString(Game1.dialogueFont, popcornCount?.ToString() ?? ConvoWindow.GetSpinnerString(), 
                stringPlacement, Color.Moccasin);
        }
    }
}