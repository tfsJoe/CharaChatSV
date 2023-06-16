using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace CharaChatSV
{
    internal class SettingsWidget : IDrawable, IClickableElement
    {
        public bool isOpen;
        private readonly ConvoWindow parent;
        private const int IconSize = 48;
        private readonly ErsatzButton budgetQualityButton;
        private readonly ErsatzButton highQualityButton;
        
        private static readonly Rectangle IconSource = new Rectangle(367, 373, 16,16);

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
            UserSettings.Inst.AiQuality = "budget";
            UserSettings.Write();
        }
        
        private static void SetDaVinci()
        {
            ModEntry.Log("High quality");
            BackendFetcher.aiModel = BackendFetcher.AiModel.davinci;
            UserSettings.Inst.AiQuality = "high";
            UserSettings.Write();
        }

        public bool DetectClick(int x, int y)
        {
            var result = false;
            
            if (IconDestination.Contains(x, y))
            {
                isOpen = !isOpen;
                result = true;
            }

            if (isOpen)
            {
                result = result || budgetQualityButton.DetectClick(x, y);
                result = result || highQualityButton.DetectClick(x, y);
            }

            if (!result) isOpen = false;    // Close when clicking elsewhere

            return result;
        }
    }
}