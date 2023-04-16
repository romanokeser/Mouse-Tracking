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
        private const int ImageWidth = 1920;
        private const int ImageHeight = 1080;

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
                //int density = (255 - distance) / 4; // Scale the density to fit within 0-255
                Color color = Color.FromArgb(255, 0, (byte)(255 /*- density*/), (byte)255); // Map the density value to a color
                byte[] pixelValues = new byte[] { color.B, color.G, color.R, 0 }; // Convert the color to BGR values
                int stride = ImageWidth * 4; // 4 bytes per pixel (BGR0)
                heatmapBitmap.WritePixels(new Int32Rect(pixelX, pixelY, 1, 1), pixelValues, stride, 0); // Set the pixel value
            }
        }

        private int Distance(System.Drawing.Point p1, System.Drawing.Point p2)
        {
            int dx = p1.X - p2.X;
            int dy = p1.Y - p2.Y;
            return (int)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
