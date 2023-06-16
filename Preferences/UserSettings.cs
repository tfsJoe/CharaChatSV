using System;
using StardewModdingAPI;

namespace CharaChatSV
{
    public class UserSettings
    {
        public string AiQuality { get; set; } = "default";
        public string LoginToken { get; set; } = Guid.Empty.ToString();


        private static UserSettings _inst;
        public static UserSettings Inst
        {
            get
            {
                if (_inst == null) _inst = new UserSettings();
                return _inst;
            }
            private set => _inst = value;
        }
        private const string Filename = "ccsv_settings.json";

        public static void Read()
        {
            _inst = ModEntry.modHelper.Data.ReadJsonFile<UserSettings>(Filename);
        }

        public static void Write()
        {
            ModEntry.modHelper.Data.WriteJsonFile(Filename, Inst);
        }
    }
}