using System;
using System.Linq;
using System.Windows;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace MouseTracking
{
    /// <summary>
    /// Interaction logic for HeatmapExecution.xaml
    /// </summary>
    public partial class HeatmapExecution : Window
    {
        public HeatmapExecution()
        {
            InitializeComponent();

            Task.Run(() => ProcessHeatmapAsync());
        }

        private async Task ProcessHeatmapAsync()
        {
            // Read the text file "MouseCoords.txt" asynchronously and parse each line into an array of doubles
            string[] lines = await File.ReadAllLinesAsync("MouseCoords.txt");
            List<double[]> coordinatesList = new List<double[]>(lines.Length);

            Parallel.ForEach(lines, line =>
            {
                double[] coordinate = line.Split(',')
                                          .Select(str => double.Parse(str))
                                          .ToArray();
                coordinatesList.Add(coordinate);
            });

            double[][] coordinates = coordinatesList.ToArray();
            double[,] coordinates2D = coordinates.ToRectangularArray(); // Convert the jagged array to a 2D rectangular array

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            double[,] heatmap = await Task.Run(() => MakeHeatmap(coordinates2D)); // Calculate the heatmap using the MakeHeatmap method
            stopwatch.Stop();


            TimeSpan ts = stopwatch.Elapsed;
            Console.WriteLine($"MakeHeatmap execution time: {ts.TotalMilliseconds} ms");

            heatmap = MultiplyScalar(heatmap, 255.0 / GetMax(heatmap));     // Normalize the heatmap by multiplying it with a scaling factor and save it to a file
            await SaveHeatmapAsync(heatmap);

            EnableUIToOpeningHeatmapImage(true);    //enable UI for opening the image

        }

        /// <summary>
        /// Creates a heatmap from a 2D array of mouse coordinates using Gaussian kernels
        /// Uses parallel processing
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns>2D array of doubles</returns>
        static double[,] MakeHeatmap(double[,] coordinates)
        {
            int height = 1080;
            int width = 1920;
            double[,] heatmap = new double[height, width];
            Parallel.For(0, coordinates.GetLength(0), i =>
            {
                double[] gx = GKern(coordinates[i, 0], height);
                double[] gy = GKern(coordinates[i, 1], width);
                double[,] kernel = OuterProduct(gx, gy);
                lock (heatmap)
                {
                    heatmap = AddMatrices(heatmap, kernel);
                }
            });
            return heatmap;
        }

        /// <summary>
        /// Calculates a Gaussian kernel for a given center point c and length l. 
        /// The kernel values are calculated based on a Gaussian distribution formula, 
        /// where the distance between the center point and each point in the kernel is used as the input variable to the formula. 
        /// The standard deviation of the Gaussian distribution is set to 10 by default, 
        /// but can be overridden by passing a different value for the sig parameter. 
        /// The resulting kernel values are returned as an array.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="l"></param>
        /// <param name="sig"></param>
        /// <returns></returns>
        static double[] GKern(double c, int l, double sig = 10.0)
        {
            double[] ax = Enumerable.Range(1, l).Select(i => i - c).ToArray();
            double[] kernel = ax.Select(x => Math.Exp(-0.5 * x * x / (sig * sig))).ToArray();
            return kernel;
        }

        /// <summary>
        /// This function calculates the outer product of two given vectors, a and b. 
        /// It creates a new two-dimensional array result with dimensions height and width, 
        /// which is filled with the products of each element of a with each element of b. 
        /// The resulting two-dimensional array is then returned.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static double[,] OuterProduct(double[] a, double[] b)
        {
            int height = a.Length;
            int width = b.Length;
            double[,] result = new double[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i, j] = a[i] * b[j];
                }
            }
            return result;
        }

        /// <summary>
        /// Defines a method named AddMatrices that takes in two 2D arrays (a and b) of the same size 
        /// and returns their element-wise sum in a new 2D array (result). 
        /// The method iterates over each element in the arrays using nested for loops and adds the corresponding elements in a and b. 
        /// The resulting element is then stored in the corresponding location in result. 
        /// The resulting result array is returned.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static double[,] AddMatrices(double[,] a, double[,] b)
        {
            int height = a.GetLength(0);
            int width = a.GetLength(1);
            double[,] result = new double[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// Takes a 2D double array 'arr' and a scalar value 'scalar' as inputs, 
        /// and returns the result of multiplying each element in arr by scalar in a new 2D double array result. 
        /// The height and width variables are used to determine the size of arr and result, 
        /// and two nested for loops iterate through each element of arr and calculate the corresponding element in result.
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        static double[,] MultiplyScalar(double[,] arr, double scalar)
        {
            int height = arr.GetLength(0);
            int width = arr.GetLength(1);
            double[,] result = new double[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i, j] = arr[i, j] * scalar;
                }
            }
            return result;
        }

        /// <summary>
        /// takes a two-dimensional array of doubles as input and returns the maximum value found in the array.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        static double GetMax(double[,] arr)
        {
            double max = double.MinValue;
            int height = arr.GetLength(0);
            int width = arr.GetLength(1);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (arr[i, j] > max)
                    {
                        max = arr[i, j];
                    }
                }
            }
            return max;
        }

        /// <summary>
        /// Converts the intensity value to a grayscale color, and sets the corresponding pixel in the bitmap to this color.
        /// saves bitmap as a JPEG file
        /// </summary>
        /// <param name="heatmap"></param>
        /// <returns></returns>
        private async Task SaveHeatmapAsync(double[,] heatmap)
        {
            int height = heatmap.GetLength(0);
            int width = heatmap.GetLength(1);
            Bitmap bmp = new Bitmap(width, height);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int intensity = (int)heatmap[i, j];
                    Color color = Color.FromArgb(intensity, intensity, intensity);
                    bmp.SetPixel(j, i, color);
                }
            }
            string filename = "heatmap.jpg";
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                await Task.Run(() => bmp.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg));
            }
        }

        private void OpenHeatmapImage_Btn(object sender, RoutedEventArgs e)
        {
            string outputDir = AppDomain.CurrentDomain.BaseDirectory;
            string imagePath = Path.Combine(outputDir, "heatmap.jpg");
            Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });
        }

        /// <summary>
        /// Pause progress bar and enable the button
        /// </summary>
        private void EnableUIToOpeningHeatmapImage(bool isEnable)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                openHeatmapBtn.IsEnabled = isEnable;
                progressBar.IsIndeterminate = !isEnable;
            });
        }
    }
}
