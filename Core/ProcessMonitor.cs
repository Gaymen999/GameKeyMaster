using System;
using System.Diagnostics;
using System.Threading;

namespace GameKeyMaster.Core
{
    public class ProcessMonitor : IDisposable
    {
        private Timer? _timer;
        private string _targetExecutable = string.Empty;
        private bool _isGameActive = false;

        public event EventHandler<bool>? GameActiveStateChanged;

        public void SetTargetGame(string executableName)
        {
            _targetExecutable = executableName?.ToLowerInvariant() ?? string.Empty;
        }

        public void StartMonitoring(int intervalMs = 1000)
        {
            _timer = new Timer(CheckForegroundWindow, null, 0, intervalMs);
        }

        public void StopMonitoring()
        {
            _timer?.Change(Timeout.Infinite, 0);
        }

        private uint _lastProcessId = 0;
        private string _lastProcessName = string.Empty;

        private void CheckForegroundWindow(object? state)
        {
            if (string.IsNullOrEmpty(_targetExecutable)) return;

            IntPtr hWnd = NativeMethods.GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return;

            NativeMethods.GetWindowThreadProcessId(hWnd, out uint processId);
            
            bool isActiveNow = false;

            if (processId != _lastProcessId)
            {
                _lastProcessId = processId;
                try
                {
                    using var process = Process.GetProcessById((int)processId);
                    _lastProcessName = process.ProcessName + ".exe";
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    _lastProcessName = string.Empty; // Access denied
                }
                catch (ArgumentException)
                {
                    _lastProcessName = string.Empty; // Process is not running
                }
                catch (InvalidOperationException)
                {
                    _lastProcessName = string.Empty;
                }
                catch (Exception)
                {
                    _lastProcessName = string.Empty;
                }
            }

            if (_lastProcessName.Equals(_targetExecutable, StringComparison.OrdinalIgnoreCase))
            {
                isActiveNow = true;
            }

            if (isActiveNow != _isGameActive)
            {
                _isGameActive = isActiveNow;
                GameActiveStateChanged?.Invoke(this, _isGameActive);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
