using System;
using System.Linq;
using System.Windows;
using System.IO;
using System.Drawing;
using MoreLinq;
using MoreLinq.Extensions;
using System.Threading.Tasks;
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
            using (StreamReader reader = new StreamReader("MouseCoords.txt"))
            {
                double[,] coordinates = null;
                string line;
                int row = 0;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    string[] values = line.Split(',');
                    if (coordinates == null)
                    {
                        coordinates = new double[values.Length, values.Length];
                    }
                    for (int col = 0; col < values.Length; col++)
                    {
                        coordinates[row, col] = double.Parse(values[col]);
                    }
                    row++;
                }

                double[,] heatmap = await Task.Run(() => MakeHeatmap(coordinates));
                heatmap = MultiplyScalar(heatmap, 255.0 / GetMax(heatmap));
                await SaveHeatmapAsync(heatmap);
            }
        }




        static double[,] MakeHeatmap(double[,] coordinates)
        {
            int height = 1080;
            int width = 2048;
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


        static double[] GKern(double c, int l, double sig = 10.0)
        {
            double[] ax = Enumerable.Range(1, l).Select(i => i - c).ToArray();
            double[] kernel = ax.Select(x => Math.Exp(-0.5 * x * x / (sig * sig))).ToArray();
            return kernel;
        }

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
    }
}
