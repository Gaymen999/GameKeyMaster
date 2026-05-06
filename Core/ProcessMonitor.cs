using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameKeyMaster.Core
{
    public class ProcessMonitor : IDisposable
    {
        private string _targetExecutable = string.Empty;
        private bool _isGameActive = false;
        private IntPtr _hWinEventHook = IntPtr.Zero;
        private NativeMethods.WinEventDelegate? _winEventProc;

        public event EventHandler<bool>? GameActiveStateChanged;

        public void SetTargetGame(string executableName)
        {
            _targetExecutable = executableName?.ToLowerInvariant() ?? string.Empty;
            // Check immediately when target is set
            CheckForegroundWindow();
        }

        public void StartMonitoring(int intervalMs = 1000)
        {
            if (_hWinEventHook == IntPtr.Zero)
            {
                _winEventProc = new NativeMethods.WinEventDelegate(WinEventCallback);
                _hWinEventHook = NativeMethods.SetWinEventHook(
                    NativeMethods.EVENT_SYSTEM_FOREGROUND,
                    NativeMethods.EVENT_SYSTEM_FOREGROUND,
                    IntPtr.Zero,
                    _winEventProc,
                    0,
                    0,
                    NativeMethods.WINEVENT_OUTOFCONTEXT);
                    
                CheckForegroundWindow();
            }
        }

        public void StopMonitoring()
        {
            if (_hWinEventHook != IntPtr.Zero)
            {
                NativeMethods.UnhookWinEvent(_hWinEventHook);
                _hWinEventHook = IntPtr.Zero;
                _winEventProc = null;
            }
        }

        private void WinEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == NativeMethods.EVENT_SYSTEM_FOREGROUND)
            {
                CheckForegroundWindow(hwnd);
            }
        }

        private void CheckForegroundWindow(IntPtr? hwnd = null)
        {
            if (string.IsNullOrEmpty(_targetExecutable)) return;

            IntPtr hWndToCheck = hwnd ?? NativeMethods.GetForegroundWindow();
            if (hWndToCheck == IntPtr.Zero)
            {
                UpdateState(false);
                return;
            }

            NativeMethods.GetWindowThreadProcessId(hWndToCheck, out uint processId);
            
            bool isActiveNow = false;
            string processName = string.Empty;

            if (processId > 0)
            {
                try
                {
                    using var process = Process.GetProcessById((int)processId);
                    processName = process.ProcessName + ".exe";
                }
                catch (Exception)
                {
                    // Access denied or process not found
                }
            }

            if (processName.Equals(_targetExecutable, StringComparison.OrdinalIgnoreCase))
            {
                isActiveNow = true;
            }

            UpdateState(isActiveNow);
        }

        private void UpdateState(bool isActive)
        {
            if (isActive != _isGameActive)
            {
                _isGameActive = isActive;
                GameActiveStateChanged?.Invoke(this, _isGameActive);
            }
        }

        public void Dispose()
        {
            StopMonitoring();
        }
    }
}
