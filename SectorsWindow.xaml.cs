using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MouseTracking
{
    public partial class MouseAnalyser : Window
    {
        private StreamWriter _streamWriter;

        private List<System.Drawing.Point> _mouseCoords = new List<System.Drawing.Point>();
        private List<System.Drawing.Point> _mouseClicksCoords = new List<System.Drawing.Point>();
        private List<DrawnRectangle> _drawnRectangles = new List<DrawnRectangle>();
        private static readonly Random _random = new Random();

        public MouseAnalyser()
        {
            InitializeComponent();
            InitCoords();
            InitClickCoords();
            canvas.Loaded += Canvas_Loaded;
            alphaSlider.ValueChanged += AlphaSlider_ValueChanged;

        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            DrawRectangleSectors();
        }

        private void InitCoords()
        {
            //gathers all coords and store them into _mouseCoords List<> 
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
            _mouseCoords = mouseCoords;
        }

        private void InitClickCoords()
        {
            List<System.Drawing.Point> mouseClickCoords = new List<System.Drawing.Point>();
            string filePath = "MouseClicksCoords.txt";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    double x = double.Parse(parts[0]);
                    double y = double.Parse(parts[1]);
                    mouseClickCoords.Add(new System.Drawing.Point((int)x, (int)y));
                }
            }
            _mouseClicksCoords = mouseClickCoords;
        }


        private void DrawRectangleSectors()
        {
            int rectSize = 20;
            int maxX = (int)canvas.ActualWidth - rectSize;
            int maxY = (int)canvas.ActualHeight - rectSize;
            for (int x = 0; x <= maxX; x += rectSize)
            {
                for (int y = 0; y <= maxY; y += rectSize)
                {
                    Rectangle rectangle = new Rectangle()
                    {
                        Width = rectSize,
                        Height = rectSize,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)) // Default alpha value of 128
                    };
                    Canvas.SetLeft(rectangle, x);
                    Canvas.SetTop(rectangle, y);
                    canvas.Children.Add(rectangle);
                    _drawnRectangles.Add(new DrawnRectangle()
                    {
                        posXY = new int[,] { { x, y }, { x + rectSize, y + rectSize } },
                        alpha = 128 // Default alpha value of 128
                    });
                }
            }
        }


        private void AlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateDrawnRectangles();
        }

        /// <summary>
        ///  Loops through the list of mouse coordinates _mouseCoords, checks if each coordinate is within any of the drawn rectangles, 
        ///  and applies changes to the fill color of the rectangle
        ///  It also checks if any of the mouse clicks _mouseClicksCoords are within the same rectangle and applies a different fill color. 
        /// </summary>
        private void UpdateDrawnRectangles()
        {
            int left = 0;
            int top = 0;
            int right = 0;
            int bottom = 0;

            foreach (System.Drawing.Point mouseCoord in _mouseCoords)
            {
                int rectangleIndex = -1;
                for (int i = 0; i < _drawnRectangles.Count; i++)
                {
                    // Get the position coordinates and alpha value of the current rectangle
                    int[,] posXY = _drawnRectangles[i].posXY!;
                    byte alpha = _drawnRectangles[i].alpha;
                    left = posXY[0, 0];
                    top = posXY[0, 1];
                    right = posXY[1, 0];
                    bottom = posXY[1, 1];

                    // Check if the mouse coordinate is within the current rectangle
                    if (mouseCoord.X >= left && mouseCoord.X <= right &&
                        mouseCoord.Y >= top && mouseCoord.Y <= bottom)
                    {
                        rectangleIndex = i;
                        break;
                    }
                }

                // If a matching rectangle was found
                if (rectangleIndex != -1)
                {
                    // Get the matching rectangle from the canvas
                    Rectangle rectangle = canvas.Children[rectangleIndex] as Rectangle;

                    // Get the brush used to fill the rectangle
                    SolidColorBrush brush = rectangle.Fill as SolidColorBrush;
                    if (brush == null)
                    {
                        brush = new SolidColorBrush(Colors.Blue);
                        rectangle.Fill = brush;
                    }

                    // Update the alpha value of the brush
                    byte alpha = _drawnRectangles[rectangleIndex].alpha;
                    brush.Color = Color.FromArgb(alpha, brush.Color.R, brush.Color.G, brush.Color.B);

                    // Check if any of the mouse clicks occurred within the current rectangle
                    if (_mouseClicksCoords.Any(clickCoord =>
                        clickCoord.X >= left && clickCoord.X <= right &&
                        clickCoord.Y >= top && clickCoord.Y <= bottom


        ))
                    {
                        // Set the rectangle's fill to red
                        rectangle.Fill = new SolidColorBrush(Colors.Red);
                    }
                }
                else // If no matching rectangle was found
                {
                    // Create a new rectangle with a random color and alpha value
                    SolidColorBrush brush = new SolidColorBrush(RandomColor());
                    byte alpha = (byte)_random.Next(50, 200);

                    Rectangle newRectangle = new Rectangle
                    {
                        Width = 50,
                        Height = 50,
                        Fill = brush,
                        Opacity = alpha / 255.0,
                        Margin = new Thickness(mouseCoord.X - 25, mouseCoord.Y - 25, 0, 0)
                    };

                    // Add the new rectangle to the canvas and to the list of drawn rectangles
                    canvas.Children.Add(newRectangle);
                    _drawnRectangles.Add(new DrawnRectangle
                    {
                        posXY = new int[,] { { mouseCoord.X - 25, mouseCoord.Y - 25 }, { mouseCoord.X + 25, mouseCoord.Y + 25 } },
                        alpha = alpha
                    });
                }

            }
        }

        private Color RandomColor()
        {
            byte[] rgb = new byte[3];
            _random.NextBytes(rgb);
            return Color.FromRgb(rgb[0], rgb[1], rgb[2]);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }

    public class DrawnRectangle
    {
        public int[,]? posXY;
        public byte alpha { get; set; }

    }
}
