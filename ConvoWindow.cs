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
        private readonly TextInput textInput;
        private readonly ErsatzButton clearButton, submitButton;
        private int x, yTop, yBottom, w, hTop, hBottom;

        private NPC interlocutor;
        private Rectangle curEmotionSpriteRect;
        private string chatLog = "";
        private string npcReply = "";
        private Gpt3Fetcher chatApi;

        private Status status = Status.Closed;
        public Status Status
        {
            get => status;
            private set
            {
                status = value;
                if (value == Status.Closed)
                {
                    // ModEntry.Log("Closing ConvoWindow.");
                    Reset();
                    Game1.activeClickableMenu = null;
                }
                else
                {
                    textInput?.SubscribeAll(helper.Events);
                }
            }
        }

        private int HPadding => (int)((hTop + hBottom) * 0.025f);
        private int VPadding => (int)(w * 0.025f);
        private Rectangle PlayerTextRect => new Rectangle(x: x + 35, y: yTop + 64,
            width: w - 179, height: hTop - 160);
        private Rectangle NpcTextRect => new Rectangle(x: x + 35, y: yBottom + 128,
            width: w - 67, height: hBottom - 128);
        private Rectangle NpcPortraitRect => new Rectangle(x + w - 128, hBottom, 128, 128);
        private Rectangle ClearButtonRect => new Rectangle(PlayerTextRect.X + PlayerTextRect.Width - 128,
            PlayerTextRect.Y + 75, 192, 48);
        private Rectangle SubmitButtonRect => new Rectangle(ClearButtonRect.X, ClearButtonRect.Y + 54,
            192, 48);
        private static Color NpcTextColor => new Color(86, 22, 12, 255);
        

        public ConvoWindow(IModHelper helper)
        {
            this.helper = helper;
            Reflow();
            initialize(x, yTop + yBottom, w, hTop + hBottom, true);
            textInput = new TextInput(PlayerTextRect, SubmitContent);
            var textBoxTexture = helper.GameContent.Load<Texture2D>("LooseSprites\\textBox");
            clearButton = new ErsatzButton(textBoxTexture, "Clear (tab)", ClearButtonRect, textInput.Clear);
            submitButton = new ErsatzButton(textBoxTexture, "Say (enter)", SubmitButtonRect, SubmitContent);
            chatApi = new Gpt3Fetcher(helper);
        }

        /// <summary>
        /// Use this, rather than setting Game1.activeClickableMenu from elsewhere.
        /// Warning: if you do not, behavior will be unpredictable. May show old convos.
        /// </summary>
        /// <param name="npc">To which NPC is the player speaking?</param>
        /// <param name="prompt">What has the player said to this NPC?</param>
        public void StartConversation(NPC npc)
        {
            if (npc == null)
            {
                ModEntry.Log("Tried to start conversation with nobody.");
                Status = Status.Closed;
                return;
            }
            interlocutor = npc;
            Status = Status.OpenInit;
            chatLog = ConvoParser.ParseTemplate(npc);
            Game1.activeClickableMenu = this;
        }

        private void SubmitContent()
        {
            Status = Status.OpenWaiting;
            textInput.LockInput();
            UpdateOnReply(textInput.Content);
        }

        private async void UpdateOnReply(string nextInput)
        {
            chatLog += $"\n@human: {nextInput}\n@ai:";

            npcReply = await chatApi.Chat(chatLog);
            chatLog += npcReply;
            curEmotionSpriteRect = PortraitUtil.EmotionPortraitFromText(ref npcReply);
            ModEntry.Log(chatLog);
            textInput.UnlockAfterDelay();
            if (Status == Status.Closed) return;
            Status = Status.OpenDisplaying;
        }
        
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            Reflow();

            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
#if DEBUG
            b.Draw(Game1.fadeToBlackRect, new Rectangle(x, yTop, w, hTop), Color.CadetBlue * 0.25f);
            b.Draw(Game1.fadeToBlackRect, new Rectangle(x, yBottom, w, hBottom), Color.Crimson * 0.25f);
#endif
            
            // Player input portion
            drawTextureBox(b, PlayerTextRect.X - HPadding, PlayerTextRect.Y - VPadding, 
                PlayerTextRect.Width + HPadding * 2, PlayerTextRect.Height + VPadding * 2, Color.White);
            textInput.Draw(b);
            // NPC response portion
            drawTextureBox(b, NpcTextRect.X - HPadding, NpcTextRect.Y - VPadding, 
                NpcTextRect.Width + HPadding * 2, NpcTextRect.Height + VPadding * 2, Color.White);

#if DEBUG
            b.Draw(Game1.fadeToBlackRect, NpcTextRect, Color.Aqua * .15f);
            b.Draw(Game1.fadeToBlackRect, PlayerTextRect, Color.Lime * .45f);
#endif
            
            switch (status)
            {
                case Status.OpenInit:
                    b.DrawWordWrappedText($"(Say something to {interlocutor?.Name})",
                        NpcTextRect, Game1.dialogueFont, NpcTextColor);
                    break;
                case Status.OpenWaiting:
                    b.DrawWordWrappedText(GetSpinnerString(),
                        NpcTextRect, Game1.dialogueFont, NpcTextColor);
                    break;
                case Status.OpenDisplaying:
                    interlocutor?.DrawPortrait(b, curEmotionSpriteRect, NpcPortraitRect);

                    if (!string.IsNullOrEmpty(npcReply))
                    {
                        b.DrawWordWrappedText(npcReply,
                            NpcTextRect, Game1.dialogueFont, NpcTextColor);
                    }
                    break;
                case Status.Closed:
                default:
                    Game1.activeClickableMenu = null;
                    break;
            }
            
            // Buttons
            clearButton.Draw(b);
            submitButton.Draw(b);
            
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            clearButton.DetectClick(x, y);
            submitButton.DetectClick(x, y);
        }

        public override void receiveKeyPress(Keys key)
        {
            switch (key)
            {
                case Keys.Escape:
                    ModEntry.Log("Closing ConvoWindow");
                    Status = Status.Closed;
                    break;
                default:
                    return;
            }
        }

        protected override void cleanupBeforeExit()
        {
            // ModEntry.Log("Cleaning up ConvoWindow");
            status = Status.Closed;
            Reset();
            base.cleanupBeforeExit();
        }

        private void Reset()
        {
            textInput?.UnsubscribeAll(helper.Events);
            textInput?.Clear();
            chatLog = "";
            npcReply = "";
            curEmotionSpriteRect = PortraitUtil.EmotionToPortraitRect(Emotion.Neutral);
        }

        private static string GetSpinnerString()
        {
            int dots = ((DateTime.Now.Millisecond / 200) % 5);
            switch (dots)
            {
                case 0: return ".";
                case 1: return "..";
                default:
                    return "...";
            }
        }

        private void Reflow()
        {
            w = (int)(Game1.viewport.Width * 0.9f);
            hTop = Game1.viewport.Height / 2;
            hBottom = Game1.viewport.Height - hTop;
            x = (Game1.viewport.Width - w) / 2;
            yTop = 0;
            yBottom = hTop;
            if (clearButton != null) clearButton.rect = ClearButtonRect;
            if (submitButton != null) submitButton.rect = SubmitButtonRect;
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
