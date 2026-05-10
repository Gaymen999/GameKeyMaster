using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using GameKeyMaster.Core;
using GameKeyMaster.ViewModels;
using GameKeyMaster.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GameKeyMaster;

namespace GameKeyMaster.UI
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private KeyboardHookEngine _hookEngine;
        private ProcessMonitor _processMonitor;
        private MacroExecutor _macroExecutor;
        private OverlayWindow? _currentOverlay;
        private readonly Dictionary<MacroProfile, CancellationTokenSource> _runningMacros = new();

        // Fast lookups for the hook thread
        private readonly Dictionary<int, KeyMapping> _mappingLookup = new();
        private readonly Dictionary<int, MacroProfile> _macroLookup = new();
        private readonly object _lookupLock = new object();

        public MainWindow()
        {
            try
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
            catch (Exception ex)
            {
                // En ham haliyle startup_crash.txt'ye yaz
                System.IO.File.WriteAllText("startup_crash.txt", ex.ToString());
                // Ayrıca merkezi loga da ekle
                App.LogToFile(ex);
                throw;
            }
        }

        public void CleanupHooks()
        {
            _hookEngine?.Stop();
            _processMonitor?.StopMonitoring();
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
                    var game = _viewModel.SelectedGame;
                    if (game != null)
                    {
                        game.Mappings.Add(new KeyMapping { 
                            InputKey = inputKey, 
                            OutputKey = outputKey, 
                            SuppressOriginal = true 
                        });
                        _viewModel.Save();
                    }
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
            if (builder.ShowDialog() == true && builder.ResultMacro != null)
            {
                var game = _viewModel.SelectedGame;
                if (game != null)
                {
                    game.Macros.Add(builder.ResultMacro);
                    _viewModel.Save();
                }
            }
        }

        private void DeleteGame_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedGame != null)
            {
                if (MessageBox.Show($"'{_viewModel.SelectedGame.Name}' oyun profilini silmek istediğinize emin misiniz?", "Oyun Sil", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _viewModel.Games.Remove(_viewModel.SelectedGame);
                    _viewModel.Save();
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için listeden bir oyun seçin.");
            }
        }

        private void StartSystem_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedGame == null)
            {
                _viewModel.StatusText = "HATA: Başlatmak için bir oyun seçmelisiniz.";
                return;
            }

            lock (_lookupLock)
            {
                _mappingLookup.Clear();
                _macroLookup.Clear();

                foreach (var m in _viewModel.SelectedGame.Mappings)
                {
                    ushort vk = KeyHelper.GetVirtualKeyCode(m.InputKey);
                    if (vk != 0) _mappingLookup[vk] = m;
                }

                foreach (var m in _viewModel.SelectedGame.Macros)
                {
                    ushort vk = KeyHelper.GetVirtualKeyCode(m.InputKey);
                    if (vk != 0) _macroLookup[vk] = m;
                }
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

            lock (_lookupLock)
            {
                _mappingLookup.Clear();
                _macroLookup.Clear();
            }

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
                    GameStatusText.Text = "BAĞLANDI";
                    GameStatusText.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                    
                    if (_currentOverlay == null)
                    {
                        string gameName = _viewModel.SelectedGame?.Name ?? "Oyun";
                        _currentOverlay = new OverlayWindow(gameName);
                        _currentOverlay.Closed += (s, e) => _currentOverlay = null;
                        _currentOverlay.Show();
                    }
                    else
                    {
                        _currentOverlay.UpdateStatus("SİSTEM AKTİF", true);
                    }
                }
                else
                {
                    _viewModel.StatusText = "Oyun arka planda. Sistem beklemede.";
                    GameStatusText.Text = "BEKLEMEDE";
                    GameStatusText.Foreground = new SolidColorBrush(Color.FromRgb(241, 196, 15));
                    
                    if (_currentOverlay != null)
                    {
                        _currentOverlay.Close();
                        _currentOverlay = null;
                    }
                }
            });
        }

        private void HookEngine_KeyIntercepted(object? sender, HookEventArgs e)
        {
            try
            {
                // 1. Global Toggle Check (Scroll Lock = 0x91)
                if (e.KeyCode == 0x91 && e.IsKeyDown)
                {
                    _hookEngine.IsActive = !_hookEngine.IsActive;
                    string statusMsg = _hookEngine.IsActive ? "SİSTEM AKTİF" : "SİSTEM PASİF";
                    _currentOverlay?.UpdateStatus(statusMsg, _hookEngine.IsActive);
                    
                    Dispatcher.Invoke(() => {
                        _viewModel.StatusText = _hookEngine.IsActive ? "SİSTEM AKTİF (Manuel)" : "SİSTEM DURDURULDU (Manuel)";
                    });
                    return;
                }

                if (!_hookEngine.IsActive) return;

                KeyMapping? mapping = null;
                MacroProfile? macro = null;

                lock (_lookupLock)
                {
                    if (_mappingLookup.TryGetValue(e.KeyCode, out var m)) mapping = m;
                    else if (_macroLookup.TryGetValue(e.KeyCode, out var mac)) macro = mac;
                }

                if (mapping != null)
                {
                    e.Suppress = mapping.SuppressOriginal;
                    ushort outputVk = KeyHelper.GetVirtualKeyCode(mapping.OutputKey);
                    if (outputVk != 0)
                    {
                        // Send input on a background thread to keep the hook thread responsive
                        Task.Run(() => InputSender.SendVirtualKey(outputVk, e.IsKeyDown));
                    }
                }
                else if (macro != null)
                {
                    e.Suppress = macro.SuppressOriginal;
                    if (e.IsKeyDown)
                    {
                        lock (_runningMacros)
                        {
                            if (!_runningMacros.ContainsKey(macro))
                            {
                                var cts = new CancellationTokenSource();
                                _runningMacros[macro] = cts;
                                _ = ExecuteMacroWithTrackingAsync(macro, cts.Token);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogToFile(ex);
            }
        }

        public void UpdateLatestLog(string message)
        {
            if (StatusLogText != null)
            {
                StatusLogText.Text = message;
            }
        }

        private void OpenLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "activity_log.txt");
                if (System.IO.File.Exists(logPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(logPath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Henüz bir etkinlik kaydı oluşturulmadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Log dosyası açılamadı: {ex.Message}");
            }
        }

        private async Task ExecuteMacroWithTrackingAsync(MacroProfile macro, CancellationToken ct)
        {
            try
            {
                await _macroExecutor.ExecuteMacroAsync(macro, ct);
            }
            catch (Exception ex)
            {
                App.LogToFile(ex);
            }
            finally
            {
                lock (_runningMacros)
                {
                    if (_runningMacros.TryGetValue(macro, out var cts))
                    {
                        cts.Dispose();
                        _runningMacros.Remove(macro);
                    }
                }
            }
        }

        private void DeleteMapping_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is KeyMapping mapping)
            {
                if (MessageBox.Show("Bu eşlemeyi silmek istediğinize emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _viewModel.SelectedGame?.Mappings.Remove(mapping);
                    _viewModel.Save();
                }
            }
        }

        private void DeleteMacro_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is MacroProfile macro)
            {
                if (MessageBox.Show("Bu makroyu silmek istediğinize emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _viewModel.SelectedGame?.Macros.Remove(macro);
                    _viewModel.Save();
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
