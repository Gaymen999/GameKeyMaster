using System;
using System.Windows;
using System.Windows.Interop;
using GameKeyMaster.Core;

namespace GameKeyMaster.UI
{
    public partial class OverlayWindow : Window
    {
        public OverlayWindow(string profileName)
        {
            InitializeComponent();
            // Rozet her zaman "SİSTEM AKTİF" yazacak, profil adını belki tooltip veya başka yere ekleyebiliriz ama 
            // şimdilik sadece durum göstergesi yeterli.
            MessageText.Text = "SİSTEM AKTİF";
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
