using System.Windows;
using System.Windows.Threading;

namespace GameKeyMaster
{
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Beklenmeyen bir hata oluştu:\n{e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", 
                            "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
