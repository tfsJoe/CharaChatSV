using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace StardewChatter
{
    internal class ConvoWindow : IClickableMenu
    {
        private readonly IModHelper helper; //SMAPI helper, not Stardew-native
        private int x, yTop, yBottom, w, hTop, hBottom;
        private TextInput textInput;
        private InputTextBox tb;

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
                    textInput.UnsubscribeAllEvents(helper.Events);
                    Game1.activeClickableMenu = null;
                }
                else
                {
                    textInput.SubscribeAllEvents(helper.Events);
                }
            }
        }
        
        private Rectangle NpcTextRect => new Rectangle(x: x + 35, y: yTop + 128, width: w - 67, height: hTop - 160);
        private Rectangle PlayerTextRect => new Rectangle(x: x + 35, y: yBottom + 32, width: w - 179, height: hBottom);
        private static Color NpcTextColor => new Color(86, 22, 12, 255);
        

        public ConvoWindow(IModHelper helper)
        {
            this.helper = helper;
            Recenter();
            initialize(x, yTop + yBottom, w, hTop + hBottom, true);
            textInput = new TextInput(PlayerTextRect);
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
            
            // Player input portion
            Game1.drawDialogueBox(x, yBottom, w, hBottom, false, true, "bottom");
            textInput.Draw(b);
            
            // NPC response portion
            Game1.drawDialogueBox(x, yTop, w, hTop, true, true, "top");

            switch (status)
            {
                case Status.OpenInit:
                    DrawWordWrappedTextBox(b, $"(Say something to {interlocutor?.Name})",
                        NpcTextRect, Game1.dialogueFont, NpcTextColor);
                    break;
                case Status.OpenWaiting:
                    DrawWordWrappedTextBox(b, GetSpinnerString(),
                        NpcTextRect, Game1.dialogueFont, NpcTextColor);
                    break;
                case Status.OpenDisplaying:
                    interlocutor?.DrawPortrait(b);

                    if (!string.IsNullOrEmpty(npcReply))
                    {
                        DrawWordWrappedTextBox(b, npcReply,
                            NpcTextRect, Game1.dialogueFont, NpcTextColor);
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
                // spriteBatch.DrawString(font, textLine, new Vector2(box.X, y), color);
                Utility.drawTextWithShadow(spriteBatch, textLine, font, new Vector2(box.X, y), color);
                Utility.drawBoldText(spriteBatch, textLine, font, new Vector2(box.X, y), color);
                y += lineHeight;
            }
            
#if DEBUG
            spriteBatch.Draw(Game1.fadeToBlackRect, box, Color.Aqua * .15f);
#endif
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
