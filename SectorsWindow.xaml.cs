using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MouseTracking
{
    public partial class SectorsWindow : Window
    {
        private List<System.Drawing.Point> _mouseCoords = new List<System.Drawing.Point>();
        private List<DrawnRectangle> _drawnRectangles = new List<DrawnRectangle>();

        public SectorsWindow()
        {
            InitializeComponent();
            InitCoords();
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

        private void ApplyAlphaChangesToDrawnRectangles()
        {
            //iterate through the list of mouse coordinates
            //iterate through the lit of drawnRectangles and check how many points of mouse coordinate is inside
            foreach (System.Drawing.Point mouseCoord in _mouseCoords)
            {
                foreach (DrawnRectangle drawnRectangle in _drawnRectangles)
                {
                    int[,] posXY = drawnRectangle.posXY!;
                    int left = posXY[0, 0];
                    int top = posXY[0, 1];
                    int right = posXY[1, 0];
                    int bottom = posXY[1, 1];

                    if (mouseCoord.X >= left && mouseCoord.X <= right &&
                        mouseCoord.Y >= top && mouseCoord.Y <= bottom)
                    {
                        Rectangle rectangle = canvas.Children[_drawnRectangles.IndexOf(drawnRectangle)] as Rectangle;
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
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ApplyAlphaChangesToDrawnRectangles();
        }
    }

    public class DrawnRectangle
    {
        public int[,]? posXY;
    }
}
