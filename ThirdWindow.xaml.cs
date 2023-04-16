using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            //// Convert the mouse position to coordinates within the heatmap image
            //float x = (float)mousePos.X / ImageWidth;
            //float y = (float)mousePos.Y / ImageHeight;
            //int pixelX = (int)(x * ImageWidth);
            //int pixelY = (int)(y * ImageHeight);

            //// Add a value to the heatmap at the mouse position
            //if (pixelX >= 0 && pixelX < ImageWidth && pixelY >= 0 && pixelY < ImageHeight)
            //{
            //    int colorIntensity = 255; // Scale the density to fit within 0-255
            //    Color color = Color.FromArgb(255, 0, (byte)(255 - colorIntensity), (byte)colorIntensity); // Map the density value to a color
            //    byte[] pixelValues = new byte[] { color.B, color.G, color.R, 0 }; // Convert the color to BGR values
            //    int stride = ImageWidth * 4; // 4 bytes per pixel (BGR0)
            //    heatmapBitmap.WritePixels(new Int32Rect(pixelX, pixelY, 1, 1), pixelValues, stride, 0); // Set the pixel value
            //}

            // Define a radius around the mouse position
            float radius = 0f; // Change this value as needed

            // Convert the mouse position to coordinates within the heatmap image
            float x = (float)mousePos.X / ImageWidth;
            float y = (float)mousePos.Y / ImageHeight;
            int centerPixelX = (int)(x * ImageWidth);
            int centerPixelY = (int)(y * ImageHeight);

            // Define a color gradient to map density values to colors
            Color[] gradient = new Color[]
            {   Color.FromArgb(255, 0, 0, 255),
                Color.FromArgb(255, 0, 255, 0),
                Color.FromArgb(255, 255, 255, 0),
                Color.FromArgb(255, 255, 0, 0)
            };
            // Iterate over all pixels within the radius around the mouse position
            for (int pixelX = centerPixelX - (int)radius; pixelX <= centerPixelX + (int)radius; pixelX++)
            {
                for (int pixelY = centerPixelY - (int)radius; pixelY <= centerPixelY + (int)radius; pixelY++)
                {
                    // Calculate the distance between the current pixel and the mouse position
                    float distance = (float)Math.Sqrt(Math.Pow(pixelX - centerPixelX, 2) + Math.Pow(pixelY - centerPixelY, 2));

                    // If the distance is within the radius, update the pixel color
                    if (distance <= radius && pixelX >= 0 && pixelX < ImageWidth && pixelY >= 0 && pixelY < ImageHeight)
                    {
                        // Calculate the color density based on the distance from the center pixel
                        int maxDensity = 255;
                        int density = maxDensity - (int)((distance / radius) * maxDensity);

                        // Map the density value to a color using the gradient
                        Color color;
                        if (density <= 0)
                        {
                            color = gradient[0];
                        }
                        else if (density >= maxDensity)
                        {
                            color = gradient[gradient.Length - 1];
                        }
                        else
                        {
                            float t = (float)density / maxDensity;
                            int index = (int)Math.Floor((gradient.Length - 1) * t);
                            float fraction = ((gradient.Length - 1) * t) - index;
                            color = InterpolateColors(gradient[index], gradient[index + 1], fraction);
                        }

                        // Convert the color to BGR values
                        byte[] pixelValues = new byte[] { color.B, color.G, color.R, 0 };

                        // Set the pixel value
                        int stride = ImageWidth * 4; // 4 bytes per pixel (BGR0)
                        heatmapBitmap.WritePixels(new Int32Rect(pixelX, pixelY, 1, 1), pixelValues, stride, 0);
                    }
                }
            }
        }

        private Color InterpolateColors(Color color1, Color color2, float fraction)
        {
            int r = (int)((1 - fraction) * color1.R + fraction * color2.R);
            int g = (int)((1 - fraction) * color1.G + fraction * color2.G);
            int b = (int)((1 - fraction) * color1.B + fraction * color2.B);
            int a = (int)((1 - fraction) * color1.A + fraction * color2.A);
            return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }
    }
}
