using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MouseTracking
{
    /// <summary>
    /// Interaction logic for ThirdWindow.xaml
    /// </summary>
    public partial class ThirdWindow : Window
    {
        private const int ImageWidth = 800;
        private const int ImageHeight = 600;

        private WriteableBitmap heatmapBitmap;
        private byte[] heatmapPixels;

        public ThirdWindow()
        {
            InitializeComponent();

            // Step 1: Read the mouse coordinates from the file
            List<System.Drawing.Point> mouseCoords = new List<System.Drawing.Point>();
            string filePath = "MouseCoords.txt";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    double x = double.Parse(parts[0]);
                    double y = double.Parse(parts[1]);
                    mouseCoords.Add(new System.Drawing.Point((int)x, (int)y));
                }
            }



            heatmapBitmap = new WriteableBitmap(ImageWidth, ImageHeight, 96, 96, PixelFormats.Gray8, null);
            heatmapPixels = new byte[ImageWidth * ImageHeight];

            heatmapImage.Source = heatmapBitmap;

            // Update heatmap with stored mouse coordinates
            foreach (System.Drawing.Point mousePos in mouseCoords)
            {
                UpdateHeatmap(mousePos);
            }
        }

        private void UpdateHeatmap(System.Drawing.Point mousePos)
        {
            // Convert the mouse position to coordinates within the heatmap image
            float x = (float)mousePos.X / ImageWidth;
            float y = (float)mousePos.Y / ImageHeight;
            int pixelX = (int)(x * ImageWidth);
            int pixelY = (int)(y * ImageHeight);

            // Add a value to the heatmap at the mouse position
            if (pixelX >= 0 && pixelX < ImageWidth && pixelY >= 0 && pixelY < ImageHeight)
            {
                int distance = Distance(new System.Drawing.Point(ImageWidth / 2, ImageHeight / 2), mousePos);
                heatmapPixels[pixelY * ImageWidth + pixelX] += (byte)(255 - distance);
            }

            // Update the bitmap with the new pixels
            heatmapBitmap.WritePixels(new Int32Rect(0, 0, ImageWidth, ImageHeight), heatmapPixels, ImageWidth, 0);
        }

        private int Distance(System.Drawing.Point p1, System.Drawing.Point p2)
        {
            int dx = p1.X - p2.X;
            int dy = p1.Y - p2.Y;
            return (int)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
