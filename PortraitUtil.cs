using Microsoft.Xna.Framework;
using System.Linq;

namespace CharaChatSV
{
    public static class PortraitUtil
    {
        public static Rectangle EmotionStringToPortraitRect(string emotion)
        {
            switch (emotion)
            {
				case "$h":
					return new Rectangle(64, 0, 64, 64);
				case "$s":
					return new Rectangle(0, 64, 64, 64);
				case "$u":
					return new Rectangle(64, 64, 64, 64);
				case "$l":
					return new Rectangle(0, 128, 64, 64);
				case "$a":
					return new Rectangle(64, 128, 64, 64);
				case "$k":
				case "$neutral":
				default:
					return new Rectangle(0, 0, 64, 64);
			}
		}

		public static Rectangle EmotionToPortraitRect(Emotion emotion)
        {
			string emoString;
			switch (emotion)
            {
				case Emotion.Happy:
					emoString = "$h";
					break;
				case Emotion.Sad:
					emoString = "$s";
					break;
				case Emotion.Unique:
					emoString = "$u";
					break;
				case Emotion.Love:
					emoString = "$l";
					break;
				case Emotion.Angry:
					emoString = "$a";
					break;
				case Emotion.Neutral:
				default:
					emoString = "$k";
					break;
			}
			return EmotionStringToPortraitRect(emoString);
        }

		/// <summary>
		/// Searches a string for a $-prefixed token denoting an emotion. Prioritizes more intense/unusual emotions.
		/// Side effect: also strips all emotion tokens out of the input string.
		/// </summary>
		/// <returns>The Rectangle representing the emotion's coordinates in the character spritesheets.</returns>
		public static Rectangle EmotionPortraitFromText(ref string text)
		{
			// Tokens are prioritized in case multiple are found. Unique is not used here.
			var tokens = new string[] { "$a", "$s", "$l", "$h", "$k" };
			var foundToken = tokens.FirstOrDefault(text.Contains);
			if (string.IsNullOrEmpty(foundToken)) foundToken = "$k";
			foreach (var token in tokens) text = text.Replace(token, "");
			return EmotionStringToPortraitRect(foundToken);
		}
    }

    public enum Emotion
    {
        Neutral,
        Happy,
        Sad,
        Unique,
        Love,
        Angry,
    }
}
