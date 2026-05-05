using System;
using System.Windows;
using Microsoft.Win32;
using GameKeyMaster.Core;
using GameKeyMaster.ViewModels;
using GameKeyMaster.Models;

namespace GameKeyMaster.UI
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private KeyboardHookEngine _hookEngine;
        private ProcessMonitor _processMonitor;
        private MacroExecutor _macroExecutor;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            _hookEngine = new KeyboardHookEngine();
            _processMonitor = new ProcessMonitor();
            _macroExecutor = new MacroExecutor();

            _processMonitor.GameActiveStateChanged += ProcessMonitor_GameActiveStateChanged;
            _hookEngine.KeyIntercepted += HookEngine_KeyIntercepted;
        }

        private void AddGame_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe";
            if (openFileDialog.ShowDialog() == true)
            {
                string exePath = openFileDialog.FileName;
                string name = System.IO.Path.GetFileNameWithoutExtension(exePath);
                _viewModel.AddGame(name, System.IO.Path.GetFileName(exePath));
            }
        }

        private void AddMapping_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedGame == null)
            {
                MessageBox.Show("Lütfen önce bir oyun seçin.");
                return;
            }

            // 1. Giriş tuşunu yakala
            var inputCapture = new KeyCaptureWindow();
            inputCapture.Owner = this;
            if (inputCapture.ShowDialog() == true)
            {
                string inputKey = inputCapture.ResultKey;

                // 2. Çıkış tuşunu yakala
                var outputCapture = new KeyCaptureWindow();
                outputCapture.Owner = this;
                outputCapture.Title = "Oyun Hangi Tuşu Algılasın?";
                if (outputCapture.ShowDialog() == true)
                {
                    string outputKey = outputCapture.ResultKey;

                    // 3. Eşlemeyi ekle
                    _viewModel.SelectedGame.Mappings.Add(new KeyMapping { 
                        InputKey = inputKey, 
                        OutputKey = outputKey, 
                        SuppressOriginal = true 
                    });
                    _viewModel.Save();
                }
            }
        }

        private void AddMacro_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedGame == null)
            {
                MessageBox.Show("Lütfen önce bir oyun seçin.");
                return;
            }

            var builder = new MacroBuilderWindow { Owner = this };
            if (builder.ShowDialog() == true)
            {
                _viewModel.SelectedGame.Macros.Add(builder.ResultMacro);
                _viewModel.Save();
            }
        }

        private void StartSystem_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedGame == null)
            {
                _viewModel.StatusText = "HATA: Başlatmak için bir oyun seçmelisiniz.";
                return;
            }

            _processMonitor.SetTargetGame(_viewModel.SelectedGame.ExecutableName);
            _hookEngine.Start();
            _processMonitor.StartMonitoring();
            _viewModel.StatusText = $"{_viewModel.SelectedGame.Name} için sistem aktif. Oyun bekleniyor...";
        }

        private void StopSystem_Click(object sender, RoutedEventArgs e)
        {
            _processMonitor.StopMonitoring();
            _hookEngine.Stop();
            _viewModel.StatusText = "Sistem durduruldu.";
        }

        private void ProcessMonitor_GameActiveStateChanged(object? sender, bool isActive)
        {
            Dispatcher.Invoke(() =>
            {
                _hookEngine.IsActive = isActive;
                if (isActive)
                {
                    _viewModel.StatusText = "OYUN AKTİF: Tuşlar eşleniyor.";
                    new OverlayWindow(_viewModel.SelectedGame.Name).Show();
                }
                else
                {
                    _viewModel.StatusText = "Oyun arka planda. Sistem beklemede.";
                }
            });
        }

        private async void HookEngine_KeyIntercepted(object? sender, HookEventArgs e)
        {
            if (_viewModel.SelectedGame == null) return;

            // 1. Normal Eşleşmeler
            foreach (var mapping in _viewModel.SelectedGame.Mappings)
            {
                ushort inputVk = KeyHelper.GetVirtualKeyCode(mapping.InputKey);
                if (inputVk != 0 && e.KeyCode == inputVk)
                {
                    e.Suppress = mapping.SuppressOriginal;
                    ushort outputVk = KeyHelper.GetVirtualKeyCode(mapping.OutputKey);
                    if (outputVk != 0)
                    {
                        InputSender.SendVirtualKey(outputVk, e.IsKeyDown); // Hold Desteği
                    }
                    return;
                }
            }

            // 2. Makrolar
            foreach (var macro in _viewModel.SelectedGame.Macros)
            {
                ushort inputVk = KeyHelper.GetVirtualKeyCode(macro.InputKey);
                if (inputVk != 0 && e.KeyCode == inputVk)
                {
                    e.Suppress = macro.SuppressOriginal;
                    // Makrolar genellikle sadece tuşa basıldığında (KeyDown) tetiklenir
                    if (e.IsKeyDown)
                    {
                        _ = _macroExecutor.ExecuteMacroAsync(macro);
                    }
                    return;
                }
            }
        }

        private void DeleteMapping_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is KeyMapping mapping)
            {
                _viewModel.SelectedGame?.Mappings.Remove(mapping);
                _viewModel.Save();
            }
        }

        private void DeleteMacro_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is MacroProfile macro)
            {
                _viewModel.SelectedGame?.Macros.Remove(macro);
                _viewModel.Save();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _processMonitor.Dispose();
            _hookEngine.Dispose();
            base.OnClosed(e);
        }
    }
}
