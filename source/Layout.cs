using System;
using System.Linq;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Host;

namespace ControllerGlyphs;

public enum Layout
{
    Auto,
    Keyboard,
    Xbox,
    PlayStation5,
    PlayStation4,
    PlayStation3,
    PlayStation1,
    Nintendo,
}

public enum Family
{
    Keyboard,
    Xbox,
    PlayStation,
    Nintendo,
}

public static class Layouts
{
    public static readonly string[] Names =
    {
        "Auto", "Keyboard", "Xbox", "PlayStation 5", "PlayStation 4", "PlayStation 2 / 3", "PlayStation", "Nintendo",
    };
    
    public static string Name(Layout layout) => Names[(int)layout];
    
    //todo: add steamdeck
    public static string? Folder(Layout layout) => layout switch
    {
        Layout.Xbox => "xbox",
        Layout.PlayStation5 => "ps5",
        Layout.PlayStation4 => "ps4",
        Layout.PlayStation3 => "ps3",
        Layout.PlayStation1 => "ps1",
        Layout.Nintendo => "nintendo",
        _ => null,
    };
    
    public static Family FamilyOf(Layout layout) => layout switch
    {
        Layout.Xbox => Family.Xbox,
        Layout.Nintendo => Family.Nintendo,
        Layout.Keyboard or Layout.Auto => Family.Keyboard,
        _ => Family.PlayStation,
    };
    
    public static bool PadConnected => InputManager.IsPadConnected(0) || InputManager.IsPadConnected(1);
    
    public static Layout Detect(bool keyboardLast)
    {
        if (keyboardLast || !PadConnected) return Layout.Keyboard;
        return InputManager.MenuPadStyle switch
        {
            PadStyle.PlayStation5 => Layout.PlayStation5,
            PadStyle.PlayStation => IsPlayStation3() ? Layout.PlayStation3 : Layout.PlayStation4,
            PadStyle.Nintendo => Layout.Nintendo,
            _ => Layout.Xbox,
        };
    }
    
    static bool IsPlayStation3()
    {
        var devices = InputManager.Devices;
        if (devices.Count == 0) return false;
        string id = Settings.System.Input[0].Device;
        var device = devices.FirstOrDefault(d => d.Id == id);
        string name = device.Name ?? devices[0].Name ?? "";

        return name.Contains("PS3", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("PLAYSTATION(R)3", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("PS2", StringComparison.OrdinalIgnoreCase);
    }
}
