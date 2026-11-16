using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Events;
using RecompOne.Runtime.Modding;
using RecompOne.UI;

namespace ControllerGlyphs;

public class ControllerGlyphsMod : IMod
{
    const string ModId = "controller-glyphs";
    const string LayoutKey = "mods.controller-glyphs.layout";
    const float AxisThreshold = 0.5f;
    const int RefreshInterval = 15;
    
    static volatile bool _keyboardLast;
    static volatile HashSet<int> _boundKeys = new();
    
    Layout _forced;
    Layout _active;
    string? _assets;
    string? _signature;
    int _countdown;
    
    public void OnLoad()
    {
        string? root = ModLoader.LoadedMods.FirstOrDefault(m => m.Id == ModId)?.SourcePath;
        _assets = string.IsNullOrEmpty(root) ? null : Path.Combine(root, "assets");
        
        int saved = Settings.Get(LayoutKey, 0);
        _forced = Enum.IsDefined(typeof(Layout), saved) ? (Layout)saved : Layout.Auto;
        _keyboardLast = !Layouts.PadConnected;
        _boundKeys = Bindings.BoundKeys();
        
        Event.AddListener<VSyncEvent>(OnVSync);
        Event.AddListener<KeyboardEvent>(OnKeyboard);
        Event.AddListener<ControllerEvent>(OnController);
    }
    
    public void OnUnload()
    {
        Event.RemoveListener<VSyncEvent>(OnVSync);
        Event.RemoveListener<KeyboardEvent>(OnKeyboard);
        Event.RemoveListener<ControllerEvent>(OnController);
        
        GlyphTextures.Clear();
        Bindings.Reset();
        var memory = RecompOne.Runtime.Runtime.Mem;
        if (memory == null) return;
        MenuHooks.RestoreTables(memory);
        SaveSelectHooks.Restore(memory);
    }
    
    public void DrawSettings()
    {
        int index = (int)_forced;
        if (Ui.Combo("Layout", Layouts.Name(_forced), Layouts.Names, ref index, "Auto follows the device you used last"))
        {
            _forced = (Layout)index;
            Settings.Set(LayoutKey, index);
            _countdown = 0;
        }
        
        if (_forced == Layout.Auto) Ui.Note("Detected: " + Layouts.Name(_active));
    }
    
    void OnVSync(VSyncEvent e)
    {
        var memory = RecompOne.Runtime.Runtime.Mem;
        if (memory != null) MenuHooks.PatchTables(memory);
        
        if (--_countdown > 0) return;
        _countdown = RefreshInterval;
        
        _boundKeys = Bindings.BoundKeys();
        var layout = _forced == Layout.Auto ? Layouts.Detect(_keyboardLast) : _forced;
        string signature = Bindings.Signature(layout);
        if (signature == _signature) return;
        
        _signature = signature;
        _active = layout;
        Bindings.Rebuild(layout, _assets);
    }
    
    static void OnKeyboard(KeyboardEvent e)
    {
        if (e.Pressed && _boundKeys.Contains(e.Key)) _keyboardLast = true;
    }
    
    static void OnController(ControllerEvent e)
    {
        if (e.Button >= 0 ? e.Pressed : MathF.Abs(e.Value) > AxisThreshold) _keyboardLast = false;
    }
}
