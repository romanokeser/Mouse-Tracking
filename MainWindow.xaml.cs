using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace MouseTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<System.Windows.Point> mouseCoordinates;

        private int[,] screen;
        private Bitmap bitmap;

        private Timer timer;
        private StreamWriter writer;

        string fileName = "MouseCoords.txt";
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]

        static extern bool GetCursorPos(out POINT lpPoint);


        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X;
            public int Y;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Record(object sender, RoutedEventArgs e)
        {
            RecordMouseCoords();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            StartRecording();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AnalyzeMouseCoordinates(1920, 1080);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ReadMouseCoordinatesFromFile("MouseCoords.txt");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //DisplayBitmapImageInWpf(mouseHeatmap);
            //DrawHeatMap();
            GenerateBitmap(1920, 1080, 0.5f);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            StopRecording();
        }

        private void StartRecording()
        {
            string filePath = "MouseCoords.txt";
            writer = new StreamWriter(filePath);

            timer = new Timer(OnTimedEvent, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.1f));
        }

        private void StopRecording()
        {
            timer.Dispose();
            writer.Close();
        }

        private void OnTimedEvent(object state)
        {
            Dispatcher.Invoke(() =>
            {
                System.Windows.Point mousePosition = Mouse.GetPosition(null);
                string coordsString = $"{mousePosition.X},{mousePosition.Y}";
                writer.WriteLine(coordsString);
            });
        }


        private void RecordMouseCoords()
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                int count = 0;
                while (count < 1000)
                {
                    System.Windows.Point mousePosition = Mouse.GetPosition(null);
                    string coordsString = $"{mousePosition.X},{mousePosition.Y}";
                    writer.WriteLine(coordsString);
                    count++;
                }
            }
        }

        public void ReadMouseCoordinatesFromFile(string fileName)
        {
            mouseCoordinates = new List<System.Windows.Point>();
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] coordinates = line.Split(',');
                    if (coordinates.Length >= 2)
                    {
                        System.Windows.Point point = new System.Windows.Point(Convert.ToInt32(coordinates[0]), Convert.ToInt32(coordinates[1]));
                        mouseCoordinates.Add(point);
                    }
                }
            }
        }

        public void AnalyzeMouseCoordinates(int screenWidth, int screenHeight)
        {
            // Calculate frequency of occurrence of each coordinate
            Dictionary<System.Windows.Point, int> frequencyMap = new Dictionary<System.Windows.Point, int>();
            foreach (System.Windows.Point point in mouseCoordinates)
            {
                if (frequencyMap.ContainsKey(point))
                    frequencyMap[point]++;
                else
                    frequencyMap[point] = 1;
            }

            // Create two-dimensional array representing screen
            screen = new int[screenWidth, screenHeight];

            // Map mouse coordinates onto two-dimensional array and increment cell values
            foreach (KeyValuePair<System.Windows.Point, int> pair in frequencyMap)
            {
                int x = Convert.ToInt32(pair.Key.X);
                int y = Convert.ToInt32(pair.Key.Y);
                screen[x, y] += pair.Value;
            }

            // Normalize values in two-dimensional array to range 0-255
            int max = 0;
            for (int i = 0; i < screen.GetLength(0); i++)
            {
                for (int j = 0; j < screen.GetLength(1); j++)
                {
                    if (screen[i, j] > max)
                        max = screen[i, j];
                }
            }

            for (int i = 0; i < screen.GetLength(0); i++)
            {
                for (int j = 0; j < screen.GetLength(1); j++)
                {
                    screen[i, j] = (int)(255.0 * screen[i, j] / max);
                }
            }

            // Create bitmap image from two-dimensional array
            bitmap = new Bitmap(screenWidth, screenHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            for (int i = 0; i < screen.GetLength(0); i++)
            {
                for (int j = 0; j < screen.GetLength(1); j++)
                {
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(screen[i, j], screen[i, j], screen[i, j]);
                    bitmap.SetPixel(i, j, color);
                }
            }
        }


        private BitmapSource GenerateBitmap(int width, int height, double maxDistance)
        {
            // Create a new bitmap with the specified dimensions and set all pixels to white.
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(System.Drawing.Color.White);
            }

            // Calculate the maximum distance between any two points in the array.
            // This will be used to determine the color of each pixel.
            double maxDistanceSquared = maxDistance * maxDistance;

            // Loop through each pixel in the bitmap.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate the distance from this pixel to each point in the array.
                    double minDistanceSquared = double.MaxValue;
                    foreach (System.Windows.Point point in mouseCoordinates)
                    {
                        double dx = x - point.X;
                        double dy = y - point.Y;
                        double distanceSquared = dx * dx + dy * dy;
                        minDistanceSquared = Math.Min(minDistanceSquared, distanceSquared);
                    }

                    // Determine the color of this pixel based on the minimum distance to any point.
                    if (minDistanceSquared < maxDistanceSquared / 4)
                    {
                        // Close to a point, so render red.
                        bitmap.SetPixel(x, y, System.Drawing.Color.Red);
                    }
                    else if (minDistanceSquared < maxDistanceSquared / 2)
                    {
                        // Somewhat close to a point, so render orange.
                        bitmap.SetPixel(x, y, System.Drawing.Color.Orange);
                    }
                    else if (minDistanceSquared < maxDistanceSquared)
                    {
                        // Far from any point, so render blue.
                        bitmap.SetPixel(x, y, System.Drawing.Color.Blue);
                    }
                    else
                    {
                        // Very far from any point, so render white.
                        bitmap.SetPixel(x, y, System.Drawing.Color.White);
                    }
                }
            }

            // Convert the bitmap to a BitmapSource and return it.
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmapSource.Freeze();
            NativeMethods.DeleteObject(hBitmap);
            return bitmapSource;
        }



        //maybe better working
        private void DrawHeatMap()
        {
            if (mouseCoordinates.Count == 0)
                return;

            // Calculate the maximum distance between any two points
            double maxDistance = 0;
            for (int i = 0; i < mouseCoordinates.Count - 1; i++)
            {
                for (int j = i + 1; j < mouseCoordinates.Count; j++)
                {
                    double distance = Distance(mouseCoordinates[i], mouseCoordinates[j]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                    }
                }
            }

            // Create a bitmap with the same size as the canvas
            int width = (int)myCanvas.ActualWidth;
            int height = (int)myCanvas.ActualHeight;
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            // Lock the bitmap to access its pixels
            bitmap.Lock();

            // Get the pointer to the first pixel
            IntPtr backBuffer = bitmap.BackBuffer;
            int stride = bitmap.BackBufferStride;
            int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;

            // Create a color array for the heatmap
            int[] heatmapColors = new int[] {
        0xFF0000, // Red
        0xFF4000,
        0xFF8000,
        0xFFBF00,
        0xFFFF00, // Yellow
        0xBFFF00,
        0x80FF00,
        0x40FF00,
        0x00FF00, // Green
        0x00FF40,
        0x00FF80,
        0x00FFBF,
        0x00FFFF, // Cyan
        0x00BFFF,
        0x0080FF,
        0x0040FF,
        0x0000FF, // Blue
    };

            // Initialize the heatmap color and count arrays
            int[] heatmapColorCounts = new int[heatmapColors.Length];
            int[] heatmapColorSums = new int[heatmapColors.Length];

            // Iterate over each pixel of the bitmap
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Check if this pixel is close to any mouse coordinate
                    bool isClose = false;
                    foreach (System.Windows.Point point in mouseCoordinates)
                    {
                        double distance = Distance(new System.Windows.Point(x, y), point);
                        if (distance < maxDistance / 4) // If within a quarter of the max distance
                        {
                            isClose = true;
                            break;
                        }
                    }

                    if (!isClose)
                    {
                        // This pixel is not close to any mouse coordinate
                        continue;
                    }

                    // Calculate the distance to the closest mouse coordinate
                    double minDistance = double.MaxValue;
                    foreach (System.Windows.Point point in mouseCoordinates)
                    {
                        double distance = Distance(new System.Windows.Point(x, y), point);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                        }
                    }

                    // Calculate the color index based on the distance
                    int colorIndex = (int)(minDistance / maxDistance * (heatmapColors.Length - 1));

                    // Increment the color count and sum arrays
                    heatmapColorCounts[colorIndex]++;
                    heatmapColorSums[colorIndex] += colorIndex;

                    // Calculate the color for this pixel
                    int color = heatmapColors[colorIndex];

                    // Set the color of this pixel in the bitmap
                    byte[] colorBytes = BitConverter.GetBytes(color);
                    Marshal.Copy(colorBytes, 0, backBuffer + y * stride + x * bytesPerPixel, bytesPerPixel);
                }
            }

            // Unlock the bitmap
            bitmap.Unlock();

            // Normalize the color sum array to get the average color index for each heatmap color
            double[] heatmapColorAverages = new double[heatmapColors.Length];
            for (int i = 0; i < heatmapColors.Length; i++)
            {
                if (heatmapColorCounts[i] == 0)
                {
                    // No pixels with this color
                    continue;
                }

                heatmapColorAverages[i] = (double)heatmapColorSums[i] / heatmapColorCounts[i];
            }

            // Create a gradient brush with the heatmap colors
            GradientStopCollection gradientStops = new GradientStopCollection();
            for (int i = 0; i < heatmapColors.Length; i++)
            {
                double offset = i / (double)(heatmapColors.Length - 1);
                gradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb((byte)0xFF, (byte)((heatmapColors[i] >> 16) & 0xFF), (byte)((heatmapColors[i] >> 8) & 0xFF), (byte)(heatmapColors[i] & 0xFF)), offset));
            }
            LinearGradientBrush brush = new LinearGradientBrush(gradientStops, 0);

            // Create a rectangle to display the heatmap
            System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
            rectangle.Width = (int)myCanvas.ActualWidth;
            rectangle.Height = (int)myCanvas.ActualHeight;
            rectangle.Fill = brush;

            // Add the rectangle to the canvas
            myCanvas.Children.Add(rectangle);
        }

        private double Distance(System.Windows.Point p1, System.Windows.Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public void DisplayBitmapImageInWpf(System.Windows.Controls.Image imageControl)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                // Create a new window to display the image
                Window imageWindow = new Window();
                imageWindow.Content = new System.Windows.Controls.Image() { Source = bitmapImage };
                imageWindow.Show();
            }
        }

        internal static class NativeMethods
        {
            [DllImport("gdi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DeleteObject(IntPtr hObject);
        }
    }
}
