using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Color = System.Drawing.Color;

namespace MouseTracking
{
    /// <summary>
    /// Interaction logic for SecondWindow.xaml
    /// </summary>
    public partial class SecondWindow : Window
    {
        // Define constants and variables for heatmap
        private const int NumCellsX = 5000; // Number of cells in X direction
        private const int NumCellsY = 1080; // Number of cells in Y direction
        private const int CellSize = 1; // Size of each cell in pixels
        private const int MaxFrequency = 100; // Maximum frequency of mouse coordinates
        private int[,] cellFrequencies = new int[NumCellsX, NumCellsY]; // Array to store the frequency of mouse coordinates in each cell
        private Bitmap heatmapBitmap = new Bitmap(NumCellsX * CellSize, NumCellsY * CellSize); // Bitmap to store the heatmap image
        private List<Color> gradientColors;

        public SecondWindow()
        {
            InitializeComponent();



            gradientColors = new List<Color>()
            {
                Colors.Blue.ToDrawingColor(),
                Colors.Green.ToDrawingColor(),
                Colors.Red.ToDrawingColor()
            };
            ColorGradient colorGradient = new ColorGradient(gradientColors, 0, MaxFrequency);
        }

        private void AnalyzeMouseCoords()
        {
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



            // Step 2: Get the dimensions of the heatmap display area
            double heatmapWidth = heatmapImage.ActualWidth;
            double heatmapHeight = heatmapImage.ActualHeight;

            // Step 3: Create a heatmap data structure to store the mouse activity counts
            int[,] heatmap = new int[1920, 1080]; // assuming 100 x 100 cells in the heatmap

            // Step 4: Update the heatmap data structure with the mouse activity counts
            foreach (System.Drawing.Point point in mouseCoords)
            {
                int xIndex = (int)(point.X / heatmapWidth * 100);
                int yIndex = (int)(point.Y / heatmapHeight * 100);
                if (xIndex >= 0 && xIndex < 100 && yIndex >= 0 && yIndex < 100)
                {
                    heatmap[xIndex, yIndex]++;
                }
                // loop through the surrounding cells and increment their frequency
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (xIndex + i >= 0 && xIndex + i < 100 && yIndex + j >= 0 && yIndex + j < 100)
                        {
                            cellFrequencies[xIndex + i, yIndex + j]++;
                        }
                    }
                }
            }


            // Step 5: Normalize the heatmap data to a range of [0,1] for display purposes
            int maxCount = heatmap.Cast<int>().Max();
            double[,] normalizedHeatmap = new double[1920, 1080];
            for (int x = 0; x < 1920; x++)
            {
                for (int y = 0; y < 1080; y++)
                {
                    normalizedHeatmap[x, y] = (double)heatmap[x, y] / maxCount;
                }
            }

            // Step 6: Convert the heatmap data to a bitmap image for display
            BitmapSource heatmapBitmap = BitmapSource.Create(100, 100, 96, 96, PixelFormats.Gray16, null, normalizedHeatmap.Cast<double>().ToArray(), 200);

            // Step 7: Display the heatmap image in the heatmap display area
            heatmapImage.Source = heatmapBitmap;
        }

        private void ShowHeatmap()
        {
            SecondWindow heatmapWindow = new SecondWindow();
            heatmapWindow.heatmapImage.Source = heatmapImage.Source;
            heatmapWindow.Show();
        }

        private void AnalyseBtn(object sender, RoutedEventArgs e)
        {
            AnalyzeMouseCoords();
        }

        private void OpenBtn(object sender, RoutedEventArgs e)
        {
            ShowHeatmap();
        }
    }
}
