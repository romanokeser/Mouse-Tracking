using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
    /// Interaction logic for NewRecordingWindow.xaml
    /// </summary>
    public partial class NewRecordingWindow : Window
    {
        private Timer timer;
        private StreamWriter writer;

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X;
            public int Y;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        public NewRecordingWindow()
        {
            InitializeComponent();
        }

        private void StartRecording()
        {
            string filePath = "MouseCoords.txt";
            writer = new StreamWriter(filePath);

            timer = new Timer(OnTimedEvent, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private void button_Click(object sender, EventArgs e)
        {
            var toolTip = new ToolTip();
            //toolTip.IsBalloon = true;
            //notifyIcon.ShowBalloonTip(5000, "Popup Title", "Popup Message", ToolTipIcon.Info);
        }

        private void OnTimedEvent(object state)
        {
            POINT point;
            if (GetCursorPos(out point))
            {
                writer.WriteLine($"({point.X}, {point.Y})");
            }
        }



        private void daaaaaabztt(object sender, RoutedEventArgs e)
        {

        }
    }
}
