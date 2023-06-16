using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace CharaChatSV
{
    internal class SettingsWidget : IDrawable, IClickableElement
    {
        private readonly ConvoWindow parent;
        private bool isOpen;
        private const int IconSize = 48;
        private readonly ErsatzButton budgetQualityButton;
        private readonly ErsatzButton highQualityButton;
        
        private static readonly Rectangle IconSource = new Rectangle(367, 373, 16,16);
        private static readonly Rectangle EmptyBoxSource = new Rectangle(510, 668, 9, 9);
        private static readonly Rectangle BoxFillSource = new Rectangle(237, 725, 10, 7); //flip
        

        private Rectangle IconDestination =>
            new Rectangle(parent.SettingsWidgetAnchor.X, parent.SettingsWidgetAnchor.Y, IconSize, IconSize);

        public SettingsWidget(ConvoWindow parent, Texture2D buttonTexture)
        {
            this.parent = parent;
            budgetQualityButton = new ErsatzButton(buttonTexture, "Budget",
                new Rectangle(IconDestination.Right, IconDestination.Top, 112, IconSize),
                SetTurbo);
            highQualityButton = new ErsatzButton(buttonTexture, "High",
                new Rectangle(IconDestination.Right, IconDestination.Top + IconSize + 2, 112, IconSize),
                SetDaVinci);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Game1.mouseCursors, IconDestination, IconSource, isOpen ? Color.LightGray : Color.White);
            if (!isOpen)
            {
                spriteBatch.DrawString(Game1.smallFont, "Quality", 
                    new Vector2(IconDestination.X, IconDestination.Y + IconSize), Color.DarkGray * 0.85f);   
            }
            else
            {
                budgetQualityButton.Draw(spriteBatch);
                highQualityButton.Draw(spriteBatch);
            }
#if DEBUG
            // if (isOpen)
            // {
            //     spriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(IconDestination.Right, IconDestination.Top,
            //         160, IconSize), Color.Chartreuse * 0.25f);
            //     spriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(IconDestination.Right, IconDestination.Top + IconSize,
            //         160, IconSize), Color.LightSalmon * 0.25f);
            // }
#endif
            if (isOpen)
            {
                // spriteBatch.Draw(Game1.mouseCursors, 
                //     new Rectangle(IconDestination.Right + 1, IconDestination.Top, 
                //         Game1.smallFont.LineSpacing, Game1.smallFont.LineSpacing),
                //     EmptyBoxSource, Color.White);
                // spriteBatch.Draw(Game1.mouseCursors, 
                //     new Rectangle(IconDestination.Right + 1, IconDestination.Top + Game1.smallFont.LineSpacing,
                //         Game1.smallFont.LineSpacing, Game1.smallFont.LineSpacing),
                //     EmptyBoxSource, Color.White);
                // spriteBatch.DrawString(Game1.smallFont, "Budget", new Vector2( IconDestination.Right + EmptyBoxSource.Width + 2,
                //     IconDestination.Top), Color.DarkGray);
                // spriteBatch.DrawString(Game1.smallFont, "High", new Vector2( IconDestination.Right + EmptyBoxSource.Width + 2,
                //     IconDestination.Top + Game1.smallFont.LineSpacing + 2), Color.DarkGray);
                
            }
        }

        private void SetTurbo()
        {
            ModEntry.Log("Set turbo");
        }
        
        private void SetDaVinci()
        {
            ModEntry.Log("Set davinci");
        }

        public void DetectClick(int x, int y)
        {
            if (IconDestination.Contains(x, y))
            {
                isOpen = !isOpen;
            }
        }
    }
}