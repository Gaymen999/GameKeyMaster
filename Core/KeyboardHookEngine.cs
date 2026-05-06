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
            return NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, proc,
                Marshal.GetHINSTANCE(typeof(KeyboardHookEngine).Module), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && IsActive)
            {
                int msg = wParam.ToInt32();
                bool isKeyDown = (msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN);
                bool isKeyUp = (msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP);

                if (!isKeyDown && !isKeyUp) 
                {
                    return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
                }

                var kbdStruct = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                
                // Sonsuz döngü koruması: Bizim gönderdiğimiz tuşları yoksay
                if (kbdStruct.dwExtraInfo == (IntPtr)0x1337)
                {
                    return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
                }

                var args = new HookEventArgs { KeyCode = (int)kbdStruct.vkCode, Suppress = false, IsKeyDown = isKeyDown };
                
                KeyIntercepted?.Invoke(this, args);

                if (args.Suppress)
                {
                    return (IntPtr)1; // Suppress the key (Yutma işlemi)
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
        public bool IsKeyDown { get; set; }
    }
}
