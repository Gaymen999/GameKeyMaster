using System.Windows;
using System.Windows.Input;

namespace GameKeyMaster.UI
{
    public partial class KeyCaptureWindow : Window
    {
        public string ResultKey { get; private set; } = string.Empty;

        public KeyCaptureWindow()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Yakalanan tuşu al (WPF Key enum'dan string'e)
            ResultKey = e.Key.ToString();
            
            // Eğer tuş 'System' ise (Alt gibi), gerçek tuşu al
            if (e.Key == Key.System)
            {
                ResultKey = e.SystemKey.ToString();
            }

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
