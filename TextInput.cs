

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewChatter
{
    public class TextInput : IKeyboardSubscriber
    {
        private Rectangle rect;
        private bool selected;
        private IKeyboardSubscriber previousSubscriber;
        
        private const int MAX_CHAR_COUNT = 300;

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
            // ModEntry.Log($"Keyboard Dispatcher now: {Game1.keyboardDispatcher.Subscriber.GetType().FullName}.");
        }
        
        public void UnsubscribeAll(IModEvents events)
        {
            // ModEntry.Log($"Restoring previous keyboard dispatcher subscriber: {previousSubscriber?.GetType().FullName}");
            Game1.keyboardDispatcher.Subscriber = previousSubscriber;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            #if DEBUG
            spriteBatch.Draw(Game1.fadeToBlackRect, rect, Color.MediumOrchid * 0.15f);
            #endif
            
            spriteBatch.DrawAndTruncateWordWrappedText(ref content, rect, Font, TextColor);
            Content = content;  // Sanitization. Necessary because of "ref"
        }

        void IKeyboardSubscriber.RecieveTextInput(char inputChar)
        {
            Content += inputChar;
            // ModEntry.Log(Content);
        }

        void IKeyboardSubscriber.RecieveTextInput(string text)
        {
            // ModEntry.Log($"Received string: {text}");
            if (Content.Length >= MAX_CHAR_COUNT) return;
            if (Content.Length + text.Length > MAX_CHAR_COUNT)
            {
                text = text.Substring(0, MAX_CHAR_COUNT - Content.Length);
            }
            Content += text;
        }

        void IKeyboardSubscriber.RecieveCommandInput(char command)
        {
            if (command == '\b' && Content.Length > 0)
            {
                Content = Content.Substring(0, Content.Length - 1); // TODO acct for caret
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