using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace StardewChatter
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        static IMonitor monitor;
        public static NPC interlocutor = null;
        ChatWindow chatWindow;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            if (monitor == null) monitor = Monitor;
            chatWindow = new ChatWindow(helper);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.isTimePaused)
                return;

            // testing block
            if (e.Button == SButton.M)
            {
                Game1.activeClickableMenu = chatWindow;
                LogFromWeb();   //TODO make this part of the menu
                return;
            }

            if (e.Button != SButton.MouseRight) return;

            interlocutor = GetClickedNpcWhoCanChat();
            if (interlocutor == null) return;

            Game1.activeClickableMenu = chatWindow;
            Log("Activated chat window");
        }

        private NPC GetClickedNpcWhoCanChat()
        {
            return Game1.currentLocation.characters.FirstOrDefault(npc => npc.CanChat());
        }

        private static void LinusHowdy()
        {
            var linus = Game1.getCharacterFromName("Linus");
            Game1.drawDialogue(linus, "Howdy");
        }

        private void LogCursorTile()
        {
            Monitor.Log($"\nTile ({Game1.currentCursorTile}))" +
                $"\tspeech? {Game1.isSpeechAtCurrentCursorTile}\taction? {Game1.isActionAtCurrentCursorTile} \t" +
                $"insp? {Game1.isInspectionAtCurrentCursorTile}\t", LogLevel.Debug);
        }

        private void LogCharactersStatus()
        {
            foreach (var npc in Game1.currentLocation.characters)
            {
                Monitor.Log($"{npc.Name}: ({npc.getTileLocation()}) " +
                    $"{(npc.IsInConvoRange() ? " | Near" : "")}" +
                    $"{(npc.IsCursorOver() ? " | Pointing" : "")}" +
                    $"{(npc.IsDialogueEmpty() ? " | Quiet" : "")}", LogLevel.Debug);
            }
        }

        private async void LogFromWeb()
        {
            var fact = await StringFetcher.GetCatFact();
            chatWindow.npcText = fact;
            Log(fact);
        }

        public static void Log(string message)
        {
            if (monitor == null) return;
            monitor.Log(message, LogLevel.Debug);
        }
    }
}
