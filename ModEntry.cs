using System;
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
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            if (e.Button == SButton.P && Context.IsPlayerFree && !Game1.isTimePaused)
            {
                LogCursorTile();
                var npc = GetTalkableNpc();
                if (npc != null)
                {
                    foreach (var dialogue in npc.CurrentDialogue)
                    {
                        Monitor.Log(dialogue.getCurrentDialogue(), LogLevel.Debug);
                    }
                }
            }
        }

        private static void LinusHowdy()
        {
            var linus = Game1.getCharacterFromName("Linus");
            Game1.drawDialogue(linus, "Howdy");
        }

        private static NPC GetTalkableNpc()
        {
           return Game1.currentLocation.isCharacterAtTile(Game1.GetPlacementGrabTile());
        }

        private void LogCursorTile()
        {
            Monitor.Log($"Tile ({Game1.currentCursorTile}))" +
                $"\tspeech? {Game1.isSpeechAtCurrentCursorTile}\taction? {Game1.isActionAtCurrentCursorTile} \t" +
                $"insp? {Game1.isInspectionAtCurrentCursorTile}\t", LogLevel.Debug);
        }
    }
}
