

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace CharaChatSV
{
    public class TextInput : IKeyboardSubscriber
    {
        public Rectangle rect;
        private bool selected;
        private IKeyboardSubscriber previousSubscriber;
        private readonly Action enterKeyAction;
        
        public const int MAX_CHAR_COUNT = 300;
        private static readonly char[] illegalChars = new[] { '\\', '\"', ':', '@', '\n', '\r', '\t'};
        private bool clearOnNextInput;
        private bool lockout;
        private const int LOCKOUT_TIME = 1500;

        private string content = "";
        public string Content
        {
            get
            {
                if (content == null) content = "";
                else if (content.Length >= MAX_CHAR_COUNT)
                {
                    content = content.Substring(0, MAX_CHAR_COUNT);
                }
                return content;
            }
            private set
            {
                if (value == null) content = "";
                else if (value.Length > MAX_CHAR_COUNT)
                {
                    content = value.Substring(0, MAX_CHAR_COUNT);
                }
                else content = value;
            }
        }
        private static SpriteFont Font => Game1.dialogueFont;
        private static Color TextColor => Color.Sienna;
        private static Color LockedTextColor => TextColor * 0.6f;
        
        private int caretIndex = 0;
        private int CaretIndex
        {
            get
            {
                if (caretIndex < 0) caretIndex = 0;
                if (caretIndex > Content.Length) caretIndex = Content.Length;
                return caretIndex;
            }
            set
            {
                if (value < 0) caretIndex = 0;
                else if (value > Content.Length) caretIndex = Content.Length;
                else caretIndex = value;
            }
        }

        public TextInput(Rectangle rect, Action enterKeyAction)
        {
            this.rect = rect;
            this.enterKeyAction = enterKeyAction;
        }

        /// <summary>
        /// Will subscribe to any necessary events to capture user typing. Will not subscribe to any more than once.
        /// </summary>
        public void SubscribeAll(IModEvents events)
        {
            // ModEntry.Log($"Keyboard Dispatcher was: {Game1.keyboardDispatcher.Subscriber?.GetType().FullName}");
            if (Game1.keyboardDispatcher.Subscriber == this) return;
            previousSubscriber = Game1.keyboardDispatcher.Subscriber;
            Game1.keyboardDispatcher.Subscriber = this;
            events.Input.ButtonPressed -= OnButtonPressed;
            events.Input.ButtonPressed += OnButtonPressed;
            // ModEntry.Log($"Keyboard Dispatcher now: {Game1.keyboardDispatcher.Subscriber.GetType().FullName}.");
        }
        
        public void UnsubscribeAll(IModEvents events)
        {
            // ModEntry.Log($"Restoring previous keyboard dispatcher subscriber: {previousSubscriber?.GetType().FullName}");
            Game1.keyboardDispatcher.Subscriber = previousSubscriber;
            events.Input.ButtonPressed -= OnButtonPressed;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            #if DEBUG
            // spriteBatch.Draw(Game1.fadeToBlackRect, rect, Color.MediumOrchid * 0.15f);
            #endif

            var textColor = lockout ? LockedTextColor : TextColor;
            spriteBatch.DrawAndTruncateWordWrappedText(ref content, rect, Font, textColor);
            Content = content;  // Sanitization. Necessary because ref doesn't work with setter.

            if (!lockout && (DateTime.Now.Millisecond / 250) % 2 == 0)
            {
                DrawCaret(spriteBatch);
            }
        }

        void DrawCaret(SpriteBatch spriteBatch)
        {
            var textToCaret = Content.Substring(0, CaretIndex);
            var pos = Font.GetWordWrappedEnd(textToCaret, rect);
            pos.X += rect.X - 18;
            pos.Y += rect.Y - Font.LineSpacing;
            var caretRect = new Rectangle(pos.X, pos.Y, 2, Font.LineSpacing);
            var color = Content.Length < MAX_CHAR_COUNT ? Color.Chocolate * 0.75f : Color.Crimson * 0.75f;
            spriteBatch.Draw(Game1.fadeToBlackRect, caretRect, color);
        }

        void IKeyboardSubscriber.RecieveTextInput(char inputChar)
        {
            if (lockout) return;
            if (clearOnNextInput)
            {
                Clear();
                clearOnNextInput = false;
            }
            if (illegalChars.Any(illegalChar => inputChar == illegalChar)) return;
            string pre = Content.Substring(0, CaretIndex);
            string post = Content.Substring(CaretIndex, Content.Length - CaretIndex);
            Content = string.Join("", pre, inputChar, post);
            ++CaretIndex;
            // ModEntry.Log(Content);
        }

        void IKeyboardSubscriber.RecieveTextInput(string text)
        {
            if (lockout) return;
            if (clearOnNextInput)
            {
                Clear();
                clearOnNextInput = false;
            }
            // ModEntry.Log($"Received string: {text}");
            foreach (var illegalChar in illegalChars)
            {
                text = text.Replace(illegalChar.ToString(), "");
            }
            if (Content.Length >= MAX_CHAR_COUNT) return;
            if (Content.Length + text.Length > MAX_CHAR_COUNT)
            {
                var capacity = MAX_CHAR_COUNT - Content.Length;
                if (capacity < 0) return;
                text = text.Substring(0, capacity);
            }

            string pre = Content.Substring(0, CaretIndex);
            string post = Content.Substring(CaretIndex, Content.Length - CaretIndex);
            Content = string.Join("", pre, text, post);
            CaretIndex += text.Length;
        }

        void IKeyboardSubscriber.RecieveCommandInput(char command)
        {
            if (lockout) return;
            if (clearOnNextInput)
            {
                Clear();
                clearOnNextInput = false;
            }
            switch (command)
            {
                case '\b' when CaretIndex > 0 && Content.Length != 0:
                {
                    Content = Content.Substring(0, CaretIndex - 1) +
                              Content.Substring(CaretIndex, Content.Length - CaretIndex);
                    if (CaretIndex < Content.Length) --CaretIndex;
                    break;
                }
                case '\n' or '\r' when Content.Length != 0:
                    enterKeyAction?.Invoke();
                    break;
                case '\t':
                    Clear();
                    break;
                default:
                    ModEntry.Log($"Command: {((int)command).ToString()}");
                    break;
            }
        }

        // So far have not seen this method called.
        // Implement lockout and clearing if necessary.
        void IKeyboardSubscriber.RecieveSpecialInput(Keys key)
        {
            ModEntry.Log($"Received key: {key.ToString()}");
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            switch (e.Button)
            {
                case SButton.Left:
                    --CaretIndex;
                    break;
                case SButton.Right:
                    ++CaretIndex;
                    break;
                default:
                    break;
            }
        }

        public void Clear()
        {
            Content = "";
            CaretIndex = 0;
            Game1.playSound("cancel");
        }

        public void LockInput() => lockout = true;

        public async void UnlockAfterDelay()
        {
            await Task.Delay(LOCKOUT_TIME);
            lockout = false;
            clearOnNextInput = true;
        }

        bool IKeyboardSubscriber.Selected
        {
            get => selected;
            set
            {
                selected = value;
                if (selected)
                {
                    //...
                }
            }
        }
    }
}