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
    public partial class SectorsWindow : Window
    {
        private StreamWriter _streamWriter;

        private List<System.Drawing.Point> _mouseCoords = new List<System.Drawing.Point>();
        private List<System.Drawing.Point> _mouseClicksCoords = new List<System.Drawing.Point>();
        private List<DrawnRectangle> _drawnRectangles = new List<DrawnRectangle>();

        public SectorsWindow()
        {
            InitializeComponent();
            InitCoords();
            InitClickCoords();
            canvas.Loaded += Canvas_Loaded;
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
                        StrokeThickness = 1
                    };
                    Canvas.SetLeft(rectangle, x);
                    Canvas.SetTop(rectangle, y);
                    canvas.Children.Add(rectangle);
                    _drawnRectangles.Add(new DrawnRectangle()
                    {
                        posXY = new int[,] { { x, y }, { x + rectSize, y + rectSize } }
                    });
                }
            }
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
                    // Get the position coordinates of the current rectangle
                    int[,] posXY = _drawnRectangles[i].posXY!;
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
                    double alpha = brush.Color.A - 10;
                    if (alpha < 0)
                    {
                        alpha = 0;
                    }
                    brush.Color = Color.FromArgb((byte)alpha, brush.Color.R, brush.Color.G, brush.Color.B);

                    // Check if any of the mouse clicks occurred within the current rectangle
                    if (_mouseClicksCoords.Any(clickCoord =>
                        clickCoord.X >= left && clickCoord.X <= right &&
                        clickCoord.Y >= top && clickCoord.Y <= bottom))
                    {
                        brush.Color = Colors.Red;
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateDrawnRectangles();
        }
    }

    public class DrawnRectangle
    {
        public int[,]? posXY;
    }
}
