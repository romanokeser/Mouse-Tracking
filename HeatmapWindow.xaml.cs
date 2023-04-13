using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MouseTracking
{
    /// <summary>
    /// Interaction logic for HeatmapWindow.xaml
    /// </summary>
    public partial class HeatmapWindow : Window
    {
        public HeatmapWindow(Dictionary<int, Color> sectorColors)
        {
            InitializeComponent();

            // Create rectangles for each sector and set their color
            foreach (int sector in sectorColors.Keys)
            {
                Rectangle rect = new Rectangle();
                rect.Width = 25;
                rect.Height = 25;
                rect.Fill = new SolidColorBrush(sectorColors[sector]);

                // Calculate the row and column of the sector based on its index
                int row = sector / 4;
                int col = sector % 4;

                // Add the rectangle to the grid at the corresponding position
                Grid.SetRow(rect, row);
                Grid.SetColumn(rect, col);
                grid.Children.Add(rect);
            }
        }
    }
}
