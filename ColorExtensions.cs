using System.Drawing;
using System.Windows.Media;

public static class ColorExtensions
{
    public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color color)
    {
        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}