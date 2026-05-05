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
                    "SHIFT" => 0x10,
                    "CTRL" => 0x11,
                    "ALT" => 0x12,
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
