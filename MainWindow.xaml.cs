using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MouseTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer timer;
        private StreamWriter writer;

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
            StartRecording();
        }

        private void Button_StopRecord(object sender, RoutedEventArgs e)
        {
            StopRecording();
        }

        private void StartRecording()
        {
            string filePath = "MouseCoords.txt";
            writer = new StreamWriter(filePath);

            timer = new Timer(OnTimedEvent, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private void StopRecording()
        {
            timer.Dispose();
            writer.Close();
        }

        private void OnTimedEvent(object state)
        {
            POINT point;
            if (GetCursorPos(out point))
            {
                writer.WriteLine($"({point.X}, {point.Y})");
            }
        }


        private void ShowHeatmap()
        {
            string filePath = "MouseCoords.txt";
            Dictionary<int, int> sectorCounts = new Dictionary<int, int>();
            int maxCount = 0;

            // Read the mouse coordinates from the file and aggregate them into sectors
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split('(', ',', ')');

                    if (parts.Length == 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                    {
                        int sector = GetSector(x, y);
                        if (sectorCounts.ContainsKey(sector))
                        {
                            sectorCounts[sector]++;
                        }
                        else
                        {
                            sectorCounts[sector] = 1;
                        }

                        maxCount = Math.Max(maxCount, sectorCounts[sector]);
                    }
                }
            }

            // Create a color palette for the sectors based on the number of recorded coordinates
            Dictionary<int, Color> sectorColors = new Dictionary<int, Color>();
            foreach (int sector in Enumerable.Range(0, 32))
            {
                if (sectorCounts.ContainsKey(sector))
                {
                    int count = sectorCounts[sector];
                    double alpha = Math.Min(1.0, count / (double)maxCount);
                    sectorColors[sector] = Color.FromArgb((byte)(alpha * 255), Colors.Red.R, Colors.Red.G, Colors.Red.B);
                }
                else
                {
                    sectorColors[sector] = Colors.Transparent;
                }
            }

            // Create a new window to display the heatmap
            HeatmapWindow heatmapWindow = new HeatmapWindow(sectorColors);
            heatmapWindow.Show();
        }

        private int GetSector(int x, int y)
        {
            const int sectorSize = 25;
            int column = x / sectorSize;
            int row = y / sectorSize;
            return row * 4 + column;
        }

        private int[,] CalculateSectorActivity(string filePath)
        {
            int[,] sectorActivity = new int[4, 8]; // 4 rows and 8 columns for the sectors

            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] coordinates = line.Split(',');

                    int x = int.Parse(coordinates[0].Trim('(', ' '));
                    int y = int.Parse(coordinates[1].Trim(')', ' '));

                    // Calculate the sector where the point falls
                    int row = y / (int)ActualHeight * 4;
                    int col = x / (int)ActualWidth * 8;

                    // Increase the activity in the corresponding sector
                    sectorActivity[row, col]++;
                }
            }

            return sectorActivity;
        }

        private void CreateSectorColorPalette(int[,] sectorActivity)
        {
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int activity = sectorActivity[row, col];

                    // Set the color based on the activity count
                    Color color = Color.FromArgb((byte)Math.Min(activity * 10, 255), 0, 0, 255);

                    // Create a rectangle for the sector and set its color
                    Rectangle rect = new Rectangle();
                    rect.Width = ActualWidth / 8;
                    rect.Height = ActualHeight / 4;
                    rect.Fill = new SolidColorBrush(color);

                    // Add the rectangle to the grid at the corresponding position
                    Grid.SetRow(rect, row);
                    Grid.SetColumn(rect, col);
                    grid.Children.Add(rect);
                }
            }
        }

        private void Button_ShowActivity(object sender, RoutedEventArgs e)
        {
            string filePath = "MouseCoords.txt";
            int[,] sectorActivity = CalculateSectorActivity(filePath);
            CreateSectorColorPalette(sectorActivity);
        }
    }
}
