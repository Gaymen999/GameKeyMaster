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
            IntPtr extendedStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE);
            
            // x64 compatibility: Use ToInt64() and then create a new IntPtr
            long newStyle = extendedStyle.ToInt64() | NativeMethods.WS_EX_TRANSPARENT;
            NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr(newStyle));

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
