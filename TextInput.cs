

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StardewChatter
{
    public class TextInput : IKeyboardSubscriber
    {
        private Rectangle rect;
        private bool selected;
        private IKeyboardSubscriber previousSubscriber;
        
        private const int MAX_CHAR_COUNT = 300;
        private static readonly char[] illegalChars = new[] { '\\', '\"', ':' };

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

        public TextInput(Rectangle rect)
        {
            this.rect = rect;
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
            
            spriteBatch.DrawAndTruncateWordWrappedText(ref content, rect, Font, TextColor);
            Content = content;  // Sanitization. Necessary because ref doesn't work with setter.

            if ((DateTime.Now.Millisecond / 250) % 2 == 0)
            {
                DrawCaret(spriteBatch);
            }
        }

        void DrawCaret(SpriteBatch spriteBatch)
        {
            var textToCaret = Content.Substring(0, CaretIndex);
            var pos = Font.GetWordWrappedEnd(textToCaret, rect);
            pos.X += rect.X - 18;
            pos.Y += rect.Y;
            var caretRect = new Rectangle(pos.X, pos.Y, 2, Font.LineSpacing);
            spriteBatch.Draw(Game1.fadeToBlackRect, caretRect, Color.Chocolate * 0.75f);
        }

        void IKeyboardSubscriber.RecieveTextInput(char inputChar)
        {
            if (illegalChars.Any(illegalChar => inputChar == illegalChar)) return;
            string pre = Content.Substring(0, CaretIndex);
            string post = Content.Substring(CaretIndex, Content.Length - CaretIndex);
            Content = string.Join("", pre, inputChar, post);
            ++CaretIndex;
            // ModEntry.Log(Content);
        }

        void IKeyboardSubscriber.RecieveTextInput(string text)
        {
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
            if (command == '\b' && CaretIndex > 0 && Content.Length > 0)
            {
                Content = Content.Substring(0, CaretIndex - 1) +
                          Content.Substring(CaretIndex, Content.Length - CaretIndex);
                if (CaretIndex < Content.Length) --CaretIndex;
            }
            else
            {
                ModEntry.Log($"Command: {((int)command).ToString()}");
            }
        }

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