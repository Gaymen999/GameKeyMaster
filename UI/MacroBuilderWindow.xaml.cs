using System.Collections.ObjectModel;
using System.Windows;
using GameKeyMaster.Models;

namespace GameKeyMaster.UI
{
    public partial class MacroBuilderWindow : Window
    {
        public MacroProfile? ResultMacro { get; private set; }
        private ObservableCollection<MacroActionDisplay> _actions;

        public MacroBuilderWindow()
        {
            InitializeComponent();
            _actions = new ObservableCollection<MacroActionDisplay>();
            ActionsListView.ItemsSource = _actions;
        }

        private void SelectTriggerKey_Click(object sender, RoutedEventArgs e)
        {
            var capture = new KeyCaptureWindow { Owner = this, Title = "Tetikleyici Tuşu Basın" };
            if (capture.ShowDialog() == true)
            {
                TriggerKeyTextBox.Text = capture.ResultKey;
            }
        }

        private void AddKeyDown_Click(object sender, RoutedEventArgs e)
        {
            var capture = new KeyCaptureWindow { Owner = this, Title = "Basılacak Tuşu Seçin" };
            if (capture.ShowDialog() == true)
            {
                _actions.Add(new MacroActionDisplay { ActionType = "keyDown", Key = capture.ResultKey });
            }
        }

        private void AddKeyUp_Click(object sender, RoutedEventArgs e)
        {
            var capture = new KeyCaptureWindow { Owner = this, Title = "Bırakılacak Tuşu Seçin" };
            if (capture.ShowDialog() == true)
            {
                _actions.Add(new MacroActionDisplay { ActionType = "keyUp", Key = capture.ResultKey });
            }
        }

        private void AddKeyPress_Click(object sender, RoutedEventArgs e)
        {
            var capture = new KeyCaptureWindow { Owner = this, Title = "Tıklanacak (Bas-Çek) Tuşu Seçin" };
            if (capture.ShowDialog() == true)
            {
                _actions.Add(new MacroActionDisplay { ActionType = "keyPress", Key = capture.ResultKey });
            }
        }

        private void AddDelay_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DelayInput.Text, out int ms) && ms > 0)
            {
                _actions.Add(new MacroActionDisplay { ActionType = "delay", DelayMs = ms });
            }
            else
            {
                MessageBox.Show("Lütfen geçerli bir pozitif milisaniye (örnek: 50) değeri girin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoveAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is MacroActionDisplay action)
            {
                _actions.Remove(action);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (TriggerKeyTextBox.Text == "Belirlenmedi" || _actions.Count == 0)
            {
                MessageBox.Show("Lütfen bir tetikleyici tuş belirleyin ve en az 1 işlem ekleyin.");
                return;
            }

            ResultMacro = new MacroProfile
            {
                InputKey = TriggerKeyTextBox.Text,
                SuppressOriginal = SuppressCheckBox.IsChecked ?? true
            };

            foreach (var act in _actions)
            {
                ResultMacro.Actions.Add(new MacroAction
                {
                    ActionType = act.ActionType,
                    Key = act.Key,
                    DelayMs = act.DelayMs
                });
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

    public class MacroActionDisplay
    {
        public string ActionType { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int DelayMs { get; set; } = 0;

        public string KeyOrDelay => ActionType == "delay" ? $"{DelayMs} ms" : Key;
    }
}
