using StardewModdingAPI;

namespace CharaChatSV
{
    /// <summary>Data class for parsing manifest.json</summary>
    public class Manifest
    {
        public string Version { get; set; }
        public string AiModel { get; set; }
        public string ApiUrl { get; set; }

        public static Manifest Inst { get; private set; }

        public static void Init(IModHelper helper)
        {
            Inst = helper.Data.ReadJsonFile<Manifest>("manifest.json");
        }
    }
}