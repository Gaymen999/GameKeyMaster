using System;
using System.Runtime.InteropServices;

namespace GameKeyMaster.Core
{
    public static class InputSender
    {
        public static void SendKey(ushort scanCode, bool isKeyDown, bool isExtended = false)
        {
            uint flags = NativeMethods.KEYEVENTF_SCANCODE;
            if (!isKeyDown) flags |= NativeMethods.KEYEVENTF_KEYUP;
            if (isExtended) flags |= NativeMethods.KEYEVENTF_EXTENDEDKEY;

            var input = new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                u = new NativeMethods.InputUnion
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = scanCode,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = (IntPtr)0x1337 // Sonsuz döngü koruma bayrağı
                    }
                }
            };

            uint result = NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
            if (result == 0)
            {
                App.LogAction($"HATA: Tuş gönderilemedi (SendInput başarısız: {Marshal.GetLastWin32Error()})");
            }
        }

        public static void SendVirtualKey(ushort vkCode, bool isKeyDown)
        {
            ushort scanCode = (ushort)NativeMethods.MapVirtualKey(vkCode, 0);
            
            // Detect extended keys (Arrows, Insert, Delete, Home, End, PageUp, PageDown, R-Alt, R-Ctrl)
            bool isExtended = false;
            if (vkCode == 0x21 || vkCode == 0x22 || // Page Up, Page Down
                vkCode == 0x23 || vkCode == 0x24 || // End, Home
                vkCode == 0x25 || vkCode == 0x26 || vkCode == 0x27 || vkCode == 0x28 || // Arrows
                vkCode == 0x2D || vkCode == 0x2E || // Insert, Delete
                vkCode == 0xA5 || vkCode == 0xA3 || // R-Alt, R-Ctrl
                vkCode == 0x6F || vkCode == 0x0D)    // Num / , Num Enter (some cases)
            {
                isExtended = true;
            }

            SendKey(scanCode, isKeyDown, isExtended);
        }
    }
}
