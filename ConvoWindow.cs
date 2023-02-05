using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using StardewValley.GameData.HomeRenovations;

namespace StardewChatter
{
    internal class ConvoWindow : IClickableMenu
    {
        private readonly IModHelper helper; //SMAPI helper, not Stardew-native
        private int x, yTop, yBottom, w, hTop, hBottom;

        private NPC interlocutor;
        private string playerPrompt = "";   // TODO: print when appropriate
        private string npcReply = "";

        private Status status = Status.Closed;
        public Status Status
        {
            get => status;
            private set
            {
                status = value;
                if (value == Status.Closed)
                {
                    Game1.activeClickableMenu = null;
                }
            }
        }

        public ConvoWindow(IModHelper helper)
        {
            this.helper = helper;
            Recenter();
            initialize(x, yTop + yBottom, w, hTop + hBottom, true);
        }

        /// <summary>
        /// Use this rather than setting Game1.activeClickableMenu from elsewhere.
        /// Warning: if you do not, behavior will be unpredictable. May show old convos.
        /// </summary>
        /// <param name="npc">To which NPC is the player speaking?</param>
        /// <param name="prompt">What has the player said to this NPC?</param>
        public void Converse(NPC npc, string prompt = "")
        {
            if (npc == null)
            {
                ModEntry.Log("Tried to enter conversation with nobody.");
                Status = Status.Closed;
                return;
            }
            interlocutor = npc;
            Status = string.IsNullOrEmpty(prompt) ? Status.OpenInit : Status.OpenWaiting;
            playerPrompt = prompt;
            if (Status == Status.OpenWaiting)
            {
                UpdateOnReply(prompt);
            }
            Game1.activeClickableMenu = this;
        }

        private async void UpdateOnReply(string prompt)
        {
            npcReply = await CatFactFetcher.GetCatFact();  // TODO: Use prompt to get actual reply eventually
            if (Status == Status.Closed) return;
            Status = Status.OpenDisplaying;
        }

        public void SimulateWebHang()
        {
            Status = Status.OpenWaiting;
            Game1.activeClickableMenu = this;
        }
        
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            Recenter();

            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            Game1.drawDialogueBox(x, yTop, w, hTop, true, true, "top");
            Game1.drawDialogueBox(x, yBottom, w, hBottom, false, true, "bottom");

            switch (status)
            {
                case Status.OpenInit:
                    DrawWordWrappedTextBox(b, $"(Say something to {interlocutor?.Name})",
                        GetNpcTextRect(), Game1.dialogueFont, Color.Brown);
                    break;
                case Status.OpenWaiting:
                    DrawWordWrappedTextBox(b, GetSpinnerString(),
                        GetNpcTextRect(), Game1.dialogueFont, Color.Brown);
                    break;
                case Status.OpenDisplaying:
                    interlocutor?.DrawPortrait(b);

                    if (!string.IsNullOrEmpty(npcReply))
                    {
                        DrawWordWrappedTextBox(b, npcReply, 
                            GetNpcTextRect(), Game1.dialogueFont, Color.Brown);
                    }
                    break;
                case Status.Closed:
                default:
                    Game1.activeClickableMenu = null;
                    break;
            }

            drawMouse(b);
        }

        string GetSpinnerString()
        {
            int dots = ((DateTime.Now.Millisecond / 200) % 5) + 1;
            switch (dots)
            {
                case 1: return ".";
                case 2: return "..";
                case 3:
                default:
                    return "...";
            }
        }

        protected override void cleanupBeforeExit()
        {
            status = Status.Closed;
            base.cleanupBeforeExit();
        }

        private static void DrawWordWrappedTextBox(SpriteBatch spriteBatch, string text, Rectangle box, SpriteFont font, Color color)
        {
            var lines = new List<string>();
            var line = "";
            var words = text.Split(' ');

            foreach (var word in words)
            {
                if (font.MeasureString(line + word).X > box.Width)
                {
                    lines.Add(line);
                    line = "";
                }

                line += word + " ";
            }

            lines.Add(line);

            var lineHeight = font.LineSpacing;
            var y = box.Y;

            foreach (var textLine in lines)
            {
                spriteBatch.DrawString(font, textLine, new Vector2(box.X, y), color);
                y += lineHeight;
            }
            
#if DEBUG
            spriteBatch.Draw(Game1.fadeToBlackRect, box, Color.Aqua * .15f);
#endif
        }
        
        // TODO: cache to optimize?
        // TODO: use better anchoring?
        Rectangle GetNpcTextRect()
        {
            return new Rectangle(x: x + 34, y: yTop + 128, width: w - 66, height: hTop - 160);
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

    internal enum Status
    {
        Closed,
        OpenInit,       // Player wants to talk but has not said anything yet.
        OpenWaiting,    // Player has said something but web API has not responded yet
        OpenDisplaying, // Web API has responded
    }
}
