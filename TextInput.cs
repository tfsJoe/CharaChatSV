

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
        
        public TextInput(Rectangle rect)
        {
            this.rect = rect;
            
        }

        public void SubscribeAllEvents(IModEvents events)
        {
            UnsubscribeAllEvents(events);
            
        }

        public void UnsubscribeAllEvents(IModEvents events)
        {
            events.Input.
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            #if DEBUG
            spriteBatch.Draw(Game1.fadeToBlackRect, rect, Color.MediumOrchid * 0.15f);
            #endif
        }
        
        public void RecieveTextInput(char inputChar)
        {
            ModEntry.Log($"Got input char {inputChar}");
        }

        public void RecieveTextInput(string text)
        {
            ModEntry.Log($"Got input string {text}");
        }

        public void RecieveCommandInput(char command)
        {
            ModEntry.Log($"Got input command {command}");
        }

        public void RecieveSpecialInput(Keys key)
        {
            ModEntry.Log($"Got special input {key.ToString()}");
        }

        public bool Selected { get; set; }
    }
}