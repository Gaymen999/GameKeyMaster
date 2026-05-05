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

        private void CheckForegroundWindow(object? state)
        {
            if (string.IsNullOrEmpty(_targetExecutable)) return;

            IntPtr hWnd = NativeMethods.GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return;

            NativeMethods.GetWindowThreadProcessId(hWnd, out uint processId);
            
            bool isActiveNow = false;
            try
            {
                using var process = Process.GetProcessById((int)processId);
                string processName = process.ProcessName + ".exe";
                if (processName.Equals(_targetExecutable, StringComparison.OrdinalIgnoreCase))
                {
                    isActiveNow = true;
                }
            }
            catch
            {
                // Process might have exited or access denied
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
