using Microsoft.Xna.Framework;

namespace StardewChatter
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
					emoString = "$";
					break;
				case Emotion.Neutral:
				default:
					emoString = "$k";
					break;
			}
			return EmotionStringToPortraitRect(emoString);
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
