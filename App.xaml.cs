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
            LogToFile(e.Exception);
            MessageBox.Show($"Beklenmeyen bir arayüz hatası oluştu:\n{e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", 
                            "Kritik Hata (UI)", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogToFile(ex);
                MessageBox.Show($"Beklenmeyen bir arka plan hatası oluştu:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                                "Kritik Hata (Arka Plan)", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogToFile(Exception ex)
        {
            try
            {
                string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
                string logContent = $"[{DateTime.Now}] Hata: {ex.Message}\nStack Trace:\n{ex.StackTrace}\n" +
                                   (ex.InnerException != null ? $"Inner Hata: {ex.InnerException.Message}\nInner Stack Trace:\n{ex.InnerException.StackTrace}\n" : "") +
                                   "--------------------------------------------------\n";
                System.IO.File.AppendAllText(logPath, logContent);
            }
            catch { }
        }
    }
}
