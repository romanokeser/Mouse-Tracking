using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for MouseClicksSaver.xaml
    /// </summary>
    public partial class MouseClicksSaver : Window
    {
        private StreamWriter _streamWriter;

        public MouseClicksSaver()
        {
            InitializeComponent();

            // Open the file for writing
            _streamWriter = new StreamWriter("MouseClicksCoords.txt");
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Write the coordinates to the file
            _streamWriter.WriteLine($"{e.GetPosition(this).X}, {e.GetPosition(this).Y}");
            _streamWriter.Flush(); // Flush the buffer to ensure data is written to the file immediately
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Close the file when the app is closing
            _streamWriter.Close();
        }
    }
}
