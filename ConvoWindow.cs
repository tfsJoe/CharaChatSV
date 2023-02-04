using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;

namespace StardewChatter
{
    class ConvoWindow : IClickableMenu
    {
        private readonly IModHelper helper;
        int x, yTop, yBottom, w, hTop, hBottom;
        public string npcText = "";

        public ConvoWindow(IModHelper helper)
        {
            this.helper = helper;
            Recenter();
            initialize(x, yTop + yBottom, w, hTop + hBottom, true);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            Recenter();

            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            Game1.drawDialogueBox(x, yTop, w, hTop, true, true, "top");
            Game1.drawDialogueBox(x, yBottom, w, hBottom, false, true, "bottom");

            var npc = Game1.getCharacterFromName("Haley");
            npc.DrawPortrait(b);

            if (!string.IsNullOrEmpty(npcText))
            {
                Game1.drawWithBorder(npcText, Color.Orange, Color.MediumPurple, new(x, yTop));
            }

            drawMouse(b);
        }

        public void PrintNPCString(string message)
        {
            if (!IsActive())
            {
                ModEntry.Log($"Message received but ChatWindow was not active. Message:\n{message}");
            }
            Game1.drawWithBorder(message, Color.Orange, Color.MediumPurple, new (x, yTop));
        }

        private void Recenter()
        {
            float scale = 1f / Game1.options.uiScale;
            w = (int)(Game1.viewport.Width * scale * 0.9f);
            hTop = Game1.viewport.Height / 2;
            hBottom = Game1.viewport.Height / 2;
            x = (Game1.viewport.Width - w) / 2;
            yTop = 0;
            yBottom = hTop;
        }
    }
}
