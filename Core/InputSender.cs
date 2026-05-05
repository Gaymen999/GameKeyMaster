using System;
using System.Runtime.InteropServices;

namespace GameKeyMaster.Core
{
    public static class InputSender
    {
        public static void SendKey(ushort scanCode, bool isKeyDown)
        {
            var input = new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                u = new NativeMethods.InputUnion
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = scanCode,
                        dwFlags = NativeMethods.KEYEVENTF_SCANCODE | (isKeyDown ? 0 : NativeMethods.KEYEVENTF_KEYUP),
                        time = 0,
                        dwExtraInfo = (IntPtr)0x1337 // Sonsuz döngü koruma bayrağı
                    }
                }
            };

            NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        public static void SendVirtualKey(ushort vkCode, bool isKeyDown)
        {
            ushort scanCode = (ushort)NativeMethods.MapVirtualKey(vkCode, 0);
            if (scanCode != 0)
            {
                SendKey(scanCode, isKeyDown);
            }
        }
    }
}
