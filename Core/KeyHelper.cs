using System;
using System.Windows.Input;

namespace GameKeyMaster.Core
{
    public static class KeyHelper
    {
        public static ushort GetVirtualKeyCode(string keyString)
        {
            if (string.IsNullOrWhiteSpace(keyString)) return 0;

            try
            {
                // First try to parse as WPF Key
                if (Enum.TryParse<Key>(keyString, true, out Key wpfKey))
                {
                    return (ushort)KeyInterop.VirtualKeyFromKey(wpfKey);
                }

                // If it fails, fallback to some basic checks
                return keyString.ToUpperInvariant() switch
                {
                    "SHIFT" or "LEFTSHIFT" or "LSHIFT" => 0x10,
                    "RIGHTSHIFT" or "RSHIFT" => 0x10, // General Shift is usually safer
                    "CTRL" or "CONTROL" or "LEFTCTRL" or "LCTRL" => 0x11,
                    "RIGHTCTRL" or "RCTRL" => 0x11,
                    "ALT" or "MENU" or "LEFTALT" or "LALT" => 0x12,
                    "RIGHTALT" or "RALT" or "RMENU" => 0x12,
                    "SPACE" or "SPACEBAR" => 0x20,
                    "ENTER" or "RETURN" => 0x0D,
                    "ESC" or "ESCAPE" => 0x1B,
                    "BACK" or "BACKSPACE" => 0x08,
                    "TAB" => 0x09,
                    _ => 0
                };
            }
            catch
            {
                return 0;
            }
        }
    }
}
