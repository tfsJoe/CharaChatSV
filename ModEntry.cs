using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace StardewChatter
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private static IMonitor monitor;
        private static NPC interlocutor = null;
        private ConvoWindow convoWindow;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            monitor ??= Monitor;
            convoWindow = new ConvoWindow(helper);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.isTimePaused)
                return;

            
            #if DEBUG
            switch (e.Button)
            {
                case SButton.H:
                    Log("Debug: talk to Haley");
                    convoWindow.Converse(Game1.getCharacterFromName("Haley"), "Tell me a cat fact, Haley.");
                    return;
                case SButton.L:
                    Log("Debug: talk to Lewis");
                    convoWindow.Converse(Game1.getCharacterFromName("Lewis"));
                    break;
                case SButton.P:
                    Log("Simulating hanging web response");
                    convoWindow.SimulateWebHang();
                    break;
            }
            #endif

            if (e.Button != SButton.MouseRight) return;

            interlocutor = GetClickedNpcWhoCanChat();
            if (interlocutor == null) return;
        }

        private NPC GetClickedNpcWhoCanChat()
        {
            return Game1.currentLocation.characters.FirstOrDefault(npc => npc.CanChat());
        }
        
        #region debug
        #if DEBUG
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
        #endif
        
        public static void Log(string message)
        {
            if (monitor == null) return;
            monitor.Log(message, LogLevel.Debug);
        }
        #endregion
    }
}
