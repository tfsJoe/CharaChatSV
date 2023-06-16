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
            Color iconColor;
            if (isOpen)
            {
                budgetQualityButton.textColor = BackendFetcher.aiModel != BackendFetcher.AiModel.davinci
                    ? ErsatzButton.DefaultColor
                    : ErsatzButton.DefaultColor * 0.5f;
                highQualityButton.textColor = BackendFetcher.aiModel == BackendFetcher.AiModel.davinci
                    ? ErsatzButton.DefaultColor
                    : ErsatzButton.DefaultColor * 0.5f;
                budgetQualityButton.Draw(spriteBatch);
                highQualityButton.Draw(spriteBatch);
                iconColor = Color.LightGray;
            }
            else
            {
                spriteBatch.DrawString(Game1.smallFont, "Quality",
                    new Vector2(IconDestination.X + IconSize + 2, IconDestination.Y), Color.DarkGray * 0.85f);
                iconColor = BackendFetcher.aiModel == BackendFetcher.AiModel.davinci ? Color.LightSalmon : Color.White;
            }

            spriteBatch.Draw(Game1.mouseCursors, IconDestination, IconSource, iconColor);
        }

        private static void SetTurbo()
        {
            ModEntry.Log("Budget quality");
            BackendFetcher.aiModel = BackendFetcher.AiModel.turbo;
        }
        
        private static void SetDaVinci()
        {
            ModEntry.Log("High quality");
            BackendFetcher.aiModel = BackendFetcher.AiModel.davinci;
        }

        public void DetectClick(int x, int y)
        {
            if (IconDestination.Contains(x, y))
            {
                isOpen = !isOpen;
            }

            if (isOpen)
            {
                budgetQualityButton.DetectClick(x, y);
                highQualityButton.DetectClick(x, y);
            }
        }
    }
}