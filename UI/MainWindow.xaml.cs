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

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            _hookEngine = new KeyboardHookEngine();
            _processMonitor = new ProcessMonitor();

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

        private void HookEngine_KeyIntercepted(object? sender, HookEventArgs e)
        {
            // Bu kısım dinamik mapping listesinden kontrol edilir
            if (_viewModel.SelectedGame == null) return;

            foreach (var mapping in _viewModel.SelectedGame.Mappings)
            {
                // KeyCode -> String dönüşümü yapılmalı (örnek: 86 = V)
                if (e.KeyCode.ToString() == mapping.InputKey || (mapping.InputKey == "V" && e.KeyCode == 86))
                {
                    e.Suppress = mapping.SuppressOriginal;
                    
                    // OutputKey gönder (örnek: F)
                    InputSender.SendVirtualKey(70, true);
                    InputSender.SendVirtualKey(70, false);
                }
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
