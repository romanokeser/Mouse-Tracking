using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace MouseTracking
{
    /// <summary>
    /// Interaction logic for RegistrationMovement.xaml
    /// </summary>
    public partial class RegistrationMovement : Window
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

        public RegistrationMovement()
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


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new ToastContentBuilder()
    .AddArgument("action", "viewConversation")
    .AddArgument("conversationId", 9813)
    .AddText("Andrew sent you a picture")
    .AddText("Check this out, The Enchantments in Washington!");
    


        }

    }
}
