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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RecordNewCoordinates_Btn(object sender, RoutedEventArgs e)
        {
            
        }

        private void ShowGridResults_Btn(object sender, RoutedEventArgs e)
        {
            SectorsWindow sectorsWindow = new SectorsWindow();
            sectorsWindow.Show();
        }

        private void ShowHeatmapJPEG_Btn(object sender, RoutedEventArgs e)
        {
            HeatmapExecution heatmapExecution = new HeatmapExecution();
            heatmapExecution.Show();    
        }
    }
}
