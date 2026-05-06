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

            NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        public static void SendVirtualKey(ushort vkCode, bool isKeyDown)
        {
            ushort scanCode = (ushort)NativeMethods.MapVirtualKey(vkCode, 0);
            if (scanCode != 0)
            {
                // Detect extended keys (Arrows, Insert, Delete, Home, End, PageUp, PageDown)
                bool isExtended = (vkCode >= 0x21 && vkCode <= 0x2E) || // PageUp to Help
                                 (vkCode >= 0x5B && vkCode <= 0x5C) || // Windows keys
                                 (vkCode == 0x2D) || (vkCode == 0x2E);  // Insert, Delete
                
                SendKey(scanCode, isKeyDown, isExtended);
            }
        }
    }
}
