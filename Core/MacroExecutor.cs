using System;
using System.Threading;
using System.Threading.Tasks;
using GameKeyMaster.Models;

namespace GameKeyMaster.Core
{
    public class MacroExecutor
    {
        private static readonly SemaphoreSlim _inputLock = new SemaphoreSlim(1, 1);

        public async Task ExecuteMacroAsync(MacroProfile macro, CancellationToken ct)
        {
            foreach (var action in macro.Actions)
            {
                if (ct.IsCancellationRequested) break;

                if (action.ActionType == "delay")
                {
                    if (action.DelayMs > 0)
                    {
                        try { await Task.Delay(action.DelayMs, ct); }
                        catch (TaskCanceledException) { break; }
                    }
                }
                else
                {
                    ushort vkCode = KeyHelper.GetVirtualKeyCode(action.Key);
                    if (vkCode == 0) continue;

                    await _inputLock.WaitAsync(ct);
                    try
                    {
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
                            await Task.Delay(20, ct); // Small delay between down and up
                            InputSender.SendVirtualKey(vkCode, false);
                        }
                    }
                    catch (TaskCanceledException) { break; }
                    finally
                    {
                        _inputLock.Release();
                    }
                }
            }
        }
    }
}
