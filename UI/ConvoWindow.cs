using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Threading.Tasks;

namespace CharaChatSV
{
    internal class ConvoWindow : IClickableMenu
    {
        public readonly IModHelper helper; //SMAPI helper, not Stardew-native
        private readonly TextInput textInput;
        private readonly ErsatzButton clearButton, submitButton;
        private readonly PopcornReportWidget popcornWidget;
        private readonly SettingsWidget settingsWidget;
        private int x, yTop, yBottom, w, hTop, hBottom;
        private float cachedZoom = 1f;

        private NPC interlocutor;
        private Rectangle? curEmotionSpriteRect;
        private string npcReply = "";
        private readonly ChatFetcher chatApi;
        
        private Guid currentConvoId;

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
                    Game1.playSound("bigDeSelect");
                    Reset();
                    Game1.options.desiredBaseZoomLevel = cachedZoom;
                    settingsWidget.isOpen = false;
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
            width: w - 179, height: hTop - yTop - 128); 
        private Rectangle NpcTextRect => new Rectangle(x: x + 35, y: yBottom + 96,
            width: w - 70, height: hBottom - 128);
        private Rectangle NpcPortraitRect => new Rectangle(NpcTextRect.X + NpcTextRect.Width - 128,
            yBottom - 32, 128, 128);
        private Rectangle ClearButtonRect => new Rectangle(w / 2 - 200, 
            PlayerTextRect.Y + PlayerTextRect.Height + 32, 192, 48);
        private Rectangle SubmitButtonRect => new Rectangle(w /2 + 8, ClearButtonRect.Y,
            192, 48);

        public Point PopcornWidgetAnchor => new Point(PlayerTextRect.X, PlayerTextRect.Y + PlayerTextRect.Height + 24);
        public Point SettingsWidgetAnchor => new Point(SubmitButtonRect.Right + 32, SubmitButtonRect.Top);

        private static readonly Rectangle CloseButtonSource = new Rectangle(338, 494, 11, 11);  // Examined spritesheet for vals
        private Rectangle CloseButtonRect => new Rectangle(PlayerTextRect.Width + PlayerTextRect.X + 25 , PlayerTextRect.Y - 25,
            CloseButtonSource.Width * 4, CloseButtonSource.Height * 4);
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
            popcornWidget = new PopcornReportWidget(this);
            settingsWidget = new SettingsWidget(this, textBoxTexture);
            chatApi = ChatFetcher.Instantiate(helper);
        }

        /// <summary>
        /// Use this, rather than setting Game1.activeClickableMenu from elsewhere.
        /// Warning: if you do not, behavior will be unpredictable. May show old convos.
        /// </summary>
        /// <param name="npc">To which NPC is the player speaking?</param>
        public void StartConversation(NPC npc)
        {
            if (npc == null)
            {
                ModEntry.Log("Tried to start conversation with nobody.");
                Status = Status.Closed;
                return;
            }
            
            CheckLoginState.Check(helper, true, (balance) => popcornWidget.popcornCount = balance)
                .ConfigureAwait(false); // Suppress warning about not awaiting async method

            currentConvoId = Guid.NewGuid();
            ModEntry.Log($"New chat ID: {currentConvoId}");
            interlocutor = npc;
            curEmotionSpriteRect = null;
            Status = Status.OpenInit;
            chatApi.SetUpChat(npc);
            textInput.lockout = false;

            cachedZoom = Game1.options.desiredBaseZoomLevel;
            Game1.options.desiredBaseZoomLevel = 1f;

            Game1.activeClickableMenu = this;
            Game1.playSound("bigSelect");
        }

        private void SubmitContent()
        {
            Game1.playSound("select");
            Status = Status.OpenWaiting;
            textInput.lockout = true;
            UpdateOnResponse(textInput.Content);
        }

        private void ShowReply(BackendResponse response)
        {
            chatApi.ReplySucceeded();
            npcReply = ChatFetcher.SanitizeReply(response.Reply);
            curEmotionSpriteRect =
                EmotionUtil.EmotionToPortraitRect(interlocutor, EmotionUtil.ExtractEmotion(ref npcReply));
        }
        
        private async void UpdateOnResponse(string nextInput)
        {
            var token = NetRequestUtil.GetLoginToken(helper);
            var response = await chatApi.Chat(nextInput, token, currentConvoId);
            if (Status == Status.Closed) return;
            switch (response.Signal)
            {
                case ResponseSignal.ok:
                    ShowReply(response);
                    textInput.UnlockAfterDelay();
                    break;
                case ResponseSignal.underfunded:
                    npcReply = "(Not enough popcorn! Shop window will open in your web browser.)";
                    textInput.lockout = false;
                    textInput.clearOnNextInput = false;
                    await Task.Delay(3000);
                    if (Status != Status.Closed)
                        Extensions.OpenUrl($"{Manifest.Inst.WebRoot}/buy");
                    break;
                case ResponseSignal.moderated:
                    Status = Status.Closed;
                    bool male = interlocutor.Gender == 0;
                    Game1.drawDialogue(interlocutor, $"({(male ? "He" : "She")} seems speechless.$s)");
                    break;
                case ResponseSignal.final:
                    ShowReply(response);
                    textInput.LockoutWithMessage("(Alright, I think it's about time we move on with our day.)");
                    break;
                case ResponseSignal.tooLong:
                    npcReply = "";
                    textInput.LockoutWithMessage("(This has been going on a while... " +
                                                 $"I'd better leave {interlocutor.Name} alone.)");
                    break;
                case ResponseSignal.rateLimited:
                    npcReply = "Too many people want to chat with us right now. Please try again later.\n" +
                               "(Service over capacity)";
                    break;
                case ResponseSignal.obsolete:
                    Status = Status.Closed;
                    Game1.drawObjectDialogue("Please update Chara.Chat mod to chat with NPCs. Opening in browser...");
                    await Task.Delay(3000);
                    // TODO: open mod page instead
                    Extensions.OpenUrl(Manifest.Inst.WebRoot + "/svModDownload");
                    break;
                case ResponseSignal.unknown:
                    npcReply = response.Reply;  // Expected to contain error message
                    textInput.UnlockAfterDelay();
                    break;
            }

            popcornWidget.popcornCount = response.Balance;
            Status = Status.OpenDisplaying;
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            Reflow();

            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
#if DEBUG
            // b.Draw(Game1.fadeToBlackRect, new Rectangle(x, yTop, w, hTop), Color.CadetBlue * 0.25f);
            // b.Draw(Game1.fadeToBlackRect, new Rectangle(x, yBottom, w, hBottom), Color.Crimson * 0.25f);
#endif
            
            // Player input portion
            drawTextureBox(b, PlayerTextRect.X - HPadding, PlayerTextRect.Y - VPadding, 
                PlayerTextRect.Width + HPadding * 2, PlayerTextRect.Height + VPadding * 2, Color.White);
            textInput.Draw(b);
            // NPC response portion
            drawTextureBox(b, NpcTextRect.X - HPadding, NpcTextRect.Y - VPadding, 
                NpcTextRect.Width + HPadding * 2, NpcTextRect.Height + VPadding * 2, Color.White);
            // Popcorn report
            popcornWidget.Draw(b);
            // Settings button
            settingsWidget.Draw(b);

#if DEBUG
            // b.Draw(Game1.fadeToBlackRect, NpcTextRect, Color.Aqua * .15f);
            // b.Draw(Game1.fadeToBlackRect, PlayerTextRect, Color.Lime * .15f);
#endif
            
            switch (status)
            {
                case Status.OpenInit:
                    b.DrawWordWrappedText($"(Say something to {interlocutor?.displayName})",
                        NpcTextRect, Game1.dialogueFont, NpcTextColor);
                    break;
                case Status.OpenWaiting:
                    b.DrawWordWrappedText(GetSpinnerString(),
                        NpcTextRect, Game1.dialogueFont, NpcTextColor);
                    break;
                case Status.OpenDisplaying:
                    if (curEmotionSpriteRect.HasValue)
                        interlocutor?.DrawPortrait(b, curEmotionSpriteRect.Value, NpcPortraitRect);

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
            if (!textInput.lockout)
            {
                clearButton.Draw(b);
                submitButton.Draw(b);
            }

            b.Draw(Game1.mouseCursors, CloseButtonRect, CloseButtonSource, Color.White);

            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (!textInput.lockout)
            {
                clearButton.DetectClick(x, y);
                submitButton.DetectClick(x, y);
            }
            settingsWidget.DetectClick(x, y);
            DetectCloseButtonClick(x, y);
        }

        private void DetectCloseButtonClick(int x, int y)
        {
            if (!CloseButtonRect.Contains(x, y)) return;
            Status = Status.Closed;
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
            npcReply = "";
            curEmotionSpriteRect = new Rectangle(0, 0,64, 64);
        }

        public static string GetSpinnerString()
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
            if (textInput != null) textInput.rect = PlayerTextRect;
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
