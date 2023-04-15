using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for RecordingWindow.xaml
    /// </summary>
    public partial class RecordingWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out System.Drawing.Point lpPoint);

        private static Timer _timer;
        private static StreamWriter _streamWriter;

        public static void StartRecordingMouseCoordinates(string fileName)
        {
            _streamWriter = new StreamWriter(fileName, true);

            _timer = new Timer(1); // Set the timer interval to 1000ms (1 second)
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        public static void StopRecordingMouseCoordinates()
        {
            _timer.Enabled = false;
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();

            _streamWriter?.Flush();
            _streamWriter?.Dispose();
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            System.Drawing.Point mousePosition;
            GetCursorPos(out mousePosition);

            string coordsString = $"{mousePosition.X},{mousePosition.Y}";
            _streamWriter?.WriteLine(coordsString);
        }

        private void RecButton_Click(object sender, RoutedEventArgs e)
        {
            StartRecordingMouseCoordinates("MouseCoords.txt");
        }

        private void StopRecButton_Click(object sender, RoutedEventArgs e)
        {
            StopRecordingMouseCoordinates();
        }

        private void DiagnosticsButton_Click(object sender, RoutedEventArgs e)
        {
            ThirdWindow thirdWindow = new ThirdWindow();
            thirdWindow.Show();
        }
    }
}
