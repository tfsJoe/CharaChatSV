using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CharaChatSV
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        public static IMonitor monitor { get; private set; }

        public static string ModDirectory
        {
            get;
            private set;
        }
        private static NPC interlocutor = null;
        private ConvoWindow convoWindow;
        private IEnumerable<SButton> convoButtons;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Manifest.Init(helper);
            ModDirectory = helper.DirectoryPath;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            monitor ??= Monitor;
            convoWindow = new ConvoWindow(helper);
            var useButton = Game1.options.useToolButton?
                .Select(b => b.ToSButton()).Where(b => b != SButton.MouseLeft);
            var actionButton = Game1.options.actionButton
                .Select(b => b.ToSButton());
            convoButtons = useButton?.Concat(actionButton);
            if (convoButtons == null)
            {
                throw new Exception("No action/use button config to use for conversation. Check input keybinds");
            }
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
                    convoWindow.StartConversation(Game1.getCharacterFromName("Haley"));
                    return;
                case SButton.I:
                    var children = Game1.player.getChildren();
                    foreach (var child in children)
                    {
                        Log($"Child {child.Name} is {child.daysOld.Value} days old. Kissed today? {child.hasBeenKissedToday}");
                    }
                    if (children.Count > 0)
                        convoWindow.StartConversation(Game1.player.getChildren()[0]);
                    return;
            }
#endif
            
            if (!convoButtons.Contains(e.Button))
            {
                // Log($"Didn't press {string.Join(',', convoButtons)}");
                return;
            }

            interlocutor = GetNpcWhoCanChat();
            if (interlocutor == null)
            {
                // Log("Nobody can chat here");
                return;
            }
            // Log($"Starting conversation with {interlocutor}");
            convoWindow.StartConversation(interlocutor);
        }

        private NPC GetNpcWhoCanChat()
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
                            $"{(npc.IsDialogueEmpty() ? " | Quiet" : "")}", LogLevel.Debug);
            }
        }
        #endif
        
        public static void Log(string message)
        {
#if DEBUG
            monitor?.Log(message, LogLevel.Debug);
#endif
        }
        #endregion
    }
}
