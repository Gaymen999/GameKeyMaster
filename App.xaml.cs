using System;
using System.Windows;
using System.Windows.Threading;

namespace GameKeyMaster
{
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Beklenmeyen bir arayüz hatası oluştu:\n{e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", 
                            "Kritik Hata (UI)", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Beklenmeyen bir arka plan hatası oluştu:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                                "Kritik Hata (Arka Plan)", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
