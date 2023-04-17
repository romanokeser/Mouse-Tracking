using System;
using System.Linq;
using System.Windows;
using System.IO;
using System.Drawing;
using MoreLinq;
using MoreLinq.Extensions;

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

            double[][] coordinates = File.ReadLines("MouseCoords.txt")
                             .Select(line => line.Split(',')
                                                   .Select(str => double.Parse(str))
                                                   .ToArray())
                             .ToArray();

            double[,] coordinates2D = coordinates.ToRectangularArray();

            double[,] heatmap = MakeHeatmap(coordinates2D);
            heatmap = MultiplyScalar(heatmap, 255.0 / GetMax(heatmap));
            SaveHeatmap(heatmap);
        }

        static double[,] MakeHeatmap(double[,] coordinates)
        {
            int height = 1080;
            int width = 2048;
            double[,] heatmap = new double[height, width];
            for (int i = 0; i < coordinates.GetLength(0); i++)
            {
                double[] gx = GKern(coordinates[i, 0], height);
                double[] gy = GKern(coordinates[i, 1], width);
                double[,] kernel = OuterProduct(gx, gy);
                heatmap = AddMatrices(heatmap, kernel);
            }
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

        static double[,] MultiplyScalar(double[,] matrix, double scalar)
        {
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            double[,] result = new double[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i, j] = matrix[i, j] * scalar;
                }
            }
            return result;
        }

        static double GetMax(double[,] matrix)
        {
            double max = matrix[0, 0];
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (matrix[i, j] > max)
                    {
                        max = matrix[i, j];
                    }
                }
            }
            return max;
        }

        static void SaveHeatmap(double[,] heatmap)
        {
            Bitmap bitmap = new Bitmap(heatmap.GetLength(1), heatmap.GetLength(0));
            int height = heatmap.GetLength(0);
            int width = heatmap.GetLength(1);
            Bitmap image = new Bitmap(width, height);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int value = (int)Math.Round(heatmap[i, j]);
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(value, value, value);
                    image.SetPixel(j, i, color);
                }
            }
            image.Save("Heatmap.png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
