using System;
using System.Windows;
using System.Windows.Interop;
using GameKeyMaster.Core;

using System.Windows.Media;

namespace GameKeyMaster.UI
{
    public partial class OverlayWindow : Window
    {
        public OverlayWindow(string profileName)
        {
            InitializeComponent();
            MessageText.Text = $"GKM: {profileName}";
        }

        public void UpdateStatus(string message, bool isActive)
        {
            Dispatcher.Invoke(() => {
                MessageText.Text = message;
                StatusIndicator.Fill = isActive ? new SolidColorBrush(Color.FromRgb(39, 174, 96)) : new SolidColorBrush(Color.FromRgb(192, 57, 43));
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Sağ üst köşeye konumlandır
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - 20;
            this.Top = desktopWorkingArea.Top + 20;

            // Pencereyi "click-through" (içinden geçilebilir) yap
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            IntPtr extendedStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE);
            
            // WS_EX_TRANSPARENT ve WS_EX_LAYERED bayraklarını ekle
            long newStyle = extendedStyle.ToInt64() | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_LAYERED;
            NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr(newStyle));
        }
    }
}
