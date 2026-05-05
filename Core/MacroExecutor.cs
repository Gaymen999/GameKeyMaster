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
                    ushort vkCode = KeyHelper.GetVirtualKeyCode(action.Key);
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
    }
}
