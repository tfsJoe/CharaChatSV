using Microsoft.Xna.Framework.Graphics;

namespace CharaChatSV
{
    public interface IClickableElement
    {
        public void DetectClick(int x, int y);
    }
}