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
            LogAndCleanup(e.Exception);
            MessageBox.Show($"Beklenmeyen bir arayüz hatası oluştu:\n{e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", 
                            "Kritik Hata (UI)", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogAndCleanup(ex);
                MessageBox.Show($"Beklenmeyen bir arka plan hatası oluştu:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                                "Kritik Hata (Arka Plan)", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal void LogAndCleanup(Exception ex)
        {
            try
            {
                // Global Hook'un havada kalmasını önle
                if (App.Current.MainWindow is GameKeyMaster.UI.MainWindow mw)
                {
                    mw.CleanupHooks();
                }
            }
            catch { }

            LogToFile(ex);
        }

        public static void LogToFile(Exception ex)
        {
            LogToFile($"Hata: {ex.Message}\nStack Trace:\n{ex.StackTrace}\n" +
                      (ex.InnerException != null ? $"Inner Hata: {ex.InnerException.Message}\nInner Stack Trace:\n{ex.InnerException.StackTrace}\n" : ""));
        }

        public static void LogToFile(string message)
        {
            try
            {
                string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
                string logContent = $"[{DateTime.Now}] {message}\n--------------------------------------------------\n";
                System.IO.File.AppendAllText(logPath, logContent);
            }
            catch { }
        }

        public static void LogAction(string message)
        {
            try
            {
                string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "activity_log.txt");
                string logContent = $"[{DateTime.Now:HH:mm:ss}] {message}\n";
                System.IO.File.AppendAllText(logPath, logContent);

                // UI'daki son logu güncellemek için bir event tetiklenebilir veya MainWindow'a doğrudan erişilebilir
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (Application.Current.MainWindow is GameKeyMaster.UI.MainWindow mw)
                    {
                        mw.UpdateLatestLog(message);
                    }
                });
            }
            catch { }
        }
    }
}
