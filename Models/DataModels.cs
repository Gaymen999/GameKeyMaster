using System;
using System.Collections.Generic;

namespace GameKeyMaster.Models
{
    public class AppProfile
    {
        public List<GameProfile> Games { get; set; } = new();
    }

    public class GameProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string ExecutableName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        public List<KeyMapping> Mappings { get; set; } = new();
        public List<MacroProfile> Macros { get; set; } = new();
    }

    public class KeyMapping
    {
        public string InputKey { get; set; } = string.Empty;
        public string OutputKey { get; set; } = string.Empty;
        public bool SuppressOriginal { get; set; } = true;
    }

    public class MacroProfile
    {
        public string InputKey { get; set; } = string.Empty;
        public bool SuppressOriginal { get; set; } = true;
        public List<MacroAction> Actions { get; set; } = new();
    }

    public class MacroAction
    {
        // "keyDown", "keyUp", "keyPress", "delay"
        public string ActionType { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int DelayMs { get; set; } = 0;
    }
}
