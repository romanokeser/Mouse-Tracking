using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTracking
{
    public class ColorGradient
    {
        private List<Color> gradientColors;
        private int minValue;
        private int maxValue;

        public ColorGradient(List<Color> colors, int minValue, int maxValue)
        {
            this.gradientColors = colors;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public Color GetColor(int value)
        {
            if (value <= minValue)
                return gradientColors[0];
            if (value >= maxValue)
                return gradientColors[gradientColors.Count - 1];

            double position = (double)(value - minValue) / (double)(maxValue - minValue);
            int index = (int)(position * (gradientColors.Count - 1));

            Color color1 = gradientColors[index];
            Color color2 = gradientColors[index + 1];

            double alpha = (position - ((double)index / (gradientColors.Count - 1))) * (gradientColors.Count - 1);

            Color color = Color.FromArgb(
                (byte)(color1.A * (1 - alpha) + color2.A * alpha),
                (byte)(color1.R * (1 - alpha) + color2.R * alpha),
                (byte)(color1.G * (1 - alpha) + color2.G * alpha),
                (byte)(color1.B * (1 - alpha) + color2.B * alpha));

            return color;
        }
    }
}
