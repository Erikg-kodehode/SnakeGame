using System.Windows.Forms;

namespace SnakeGame
{
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
        }
    }
}
