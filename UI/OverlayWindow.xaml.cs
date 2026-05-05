using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using GameKeyMaster.Core;

namespace GameKeyMaster.UI
{
    public partial class OverlayWindow : Window
    {
        private DispatcherTimer _timer;

        public OverlayWindow(string profileName)
        {
            InitializeComponent();
            MessageText.Text = $"KeyMaster Aktif: {profileName}";
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += Timer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Make the window click-through
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, extendedStyle | NativeMethods.WS_EX_TRANSPARENT);

            // Start timer to close overlay after 5 seconds
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timer.Stop();
            this.Close();
        }
    }
}
