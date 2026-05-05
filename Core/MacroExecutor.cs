using System;
using System.Threading.Tasks;
using GameKeyMaster.Models;

namespace GameKeyMaster.Core
{
    public class MacroExecutor
    {
        public async Task ExecuteMacroAsync(MacroProfile macro)
        {
            foreach (var action in macro.Actions)
            {
                if (action.ActionType == "delay")
                {
                    if (action.DelayMs > 0)
                    {
                        await Task.Delay(action.DelayMs);
                    }
                }
                else
                {
                    ushort vkCode = ParseKeyToVirtualKeyCode(action.Key);
                    if (vkCode == 0) continue;

                    if (action.ActionType == "keyDown")
                    {
                        InputSender.SendVirtualKey(vkCode, true);
                    }
                    else if (action.ActionType == "keyUp")
                    {
                        InputSender.SendVirtualKey(vkCode, false);
                    }
                    else if (action.ActionType == "keyPress")
                    {
                        InputSender.SendVirtualKey(vkCode, true);
                        await Task.Delay(20); // Small delay between down and up for game engine detection
                        InputSender.SendVirtualKey(vkCode, false);
                    }
                }
            }
        }

        private ushort ParseKeyToVirtualKeyCode(string key)
        {
            // Simple mapping for demonstration. In a real app, KeyInterop is better.
            if (Enum.TryParse<ConsoleKey>(key, true, out var consoleKey))
            {
                return (ushort)consoleKey;
            }
            
            // Handle special cases
            return key.ToUpperInvariant() switch
            {
                "SHIFT" => 0x10,
                "CTRL" => 0x11,
                "ALT" => 0x12,
                _ => 0
            };
        }
    }
}
