using Microsoft.Xna.Framework.Graphics;

namespace CharaChatSV
{
    public interface IClickableElement
    {
        public bool DetectClick(int x, int y);
    }
}