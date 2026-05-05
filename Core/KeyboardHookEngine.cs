using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameKeyMaster.Core
{
    public class KeyboardHookEngine : IDisposable
    {
        private NativeMethods.LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        public bool IsActive { get; set; } = false;

        public event EventHandler<HookEventArgs>? KeyIntercepted;

        public KeyboardHookEngine()
        {
            _proc = HookCallback;
        }

        public void Start()
        {
            if (_hookID == IntPtr.Zero)
            {
                _hookID = SetHook(_proc);
            }
        }

        public void Stop()
        {
            if (_hookID != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        private IntPtr SetHook(NativeMethods.LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                return NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, proc,
                    NativeMethods.GetModuleHandle(curModule.ModuleName!), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && IsActive)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var args = new HookEventArgs { KeyCode = vkCode, Suppress = false };
                
                KeyIntercepted?.Invoke(this, args);

                if (args.Suppress)
                {
                    return (IntPtr)1; // Suppress the key
                }
            }

            return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class HookEventArgs : EventArgs
    {
        public int KeyCode { get; set; }
        public bool Suppress { get; set; }
    }
}
