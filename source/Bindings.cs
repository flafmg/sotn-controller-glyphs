using System;
using System.Collections.Generic;
using System.Text;
using RecompOne.Runtime.Config;

namespace ControllerGlyphs;

public sealed class Visual
{
    public byte[] Chars = Array.Empty<byte>();
    public bool OwnIcon;
}

public static class Bindings
{
    public const int Square = 0;
    public const int Circle = 1;
    public const int Cross = 2;
    public const int Triangle = 3;
    public const int R2 = 4;
    public const int L1 = 5;
    public const int R1 = 6;
    public const int L2 = 7;
    public const int Count = 8;
    public const int FaceCount = 4;
    
    public static readonly byte[] FaceChars = { 0xEA, 0xE8, 0xE9, 0xEB };
    
    const int StartCode = 6;
    
    static readonly string[] Positions = { "south", "east", "west", "north" };
    
    static volatile Visual[] _visuals = Vanilla();
    static volatile Visual _start = new Visual { OwnIcon = true };
    
    public static Visual Of(int button) => _visuals[button];
    
    public static Visual Start => _start;
    
    public static void Reset()
    {
        _visuals = Vanilla();
        _start = new Visual { OwnIcon = true };
    }
    
    static Visual[] Vanilla()
    {
        var visuals = new Visual[Count];
        for (int i = 0; i < FaceCount; i++) visuals[i] = new Visual { Chars = new[] { FaceChars[i] }, OwnIcon = true };
        visuals[R2] = new Visual { Chars = Labels.Encode("R2") };
        visuals[L1] = new Visual { Chars = Labels.Encode("L1") };
        visuals[R1] = new Visual { Chars = Labels.Encode("R1") };
        visuals[L2] = new Visual { Chars = Labels.Encode("L2") };
        return visuals;
    }
    
    static string[] KeyNames()
    {
        var keys = Settings.System.Input[0].Keys;
        return new[] { keys.Square, keys.Circle, keys.Cross, keys.Triangle, keys.R2, keys.L1, keys.R1, keys.L2 };
    }
    
    static int[] PadCodes()
    {
        var pad = Settings.System.Input[0].Gamepad;
        var all = new[] { pad.Square, pad.Circle, pad.Cross, pad.Triangle, pad.R2, pad.L1, pad.R1, pad.L2 };
        var codes = new int[Count];
        for (int i = 0; i < Count; i++) codes[i] = all[i] is { Length: > 0 } list ? list[0] : -1;
        return codes;
    }
    
    static int StartPadCode()
    {
        var start = Settings.System.Input[0].Gamepad.Start;
        return start is { Length: > 0 } ? start[0] : -1;
    }
    
    public static string Signature(Layout layout)
    {
        var sb = new StringBuilder();
        sb.Append((int)layout);
        if (layout == Layout.Keyboard)
        {
            foreach (var name in KeyNames()) sb.Append('|').Append(name);
            sb.Append('|').Append(Settings.System.Input[0].Keys.Start);
        }
        else
        {
            foreach (var code in PadCodes()) sb.Append('|').Append(code);
            sb.Append('|').Append(StartPadCode());
        }
        return sb.ToString();
    }
    
    public static HashSet<int> BoundKeys()
    {
        var keys = Settings.System.Input[0].Keys;
        var names = new[]
        {
            keys.Cross, keys.Circle, keys.Square, keys.Triangle, keys.L1, keys.R1, keys.L2, keys.R2, keys.L3, keys.R3,
            keys.Start, keys.Select, keys.Up, keys.Down, keys.Left, keys.Right,
        };
        var set = new HashSet<int>();
        foreach (var name in names)
            if (Enum.TryParse<Silk.NET.Input.Key>(name, out var key))
                set.Add((int)key);
        return set;
    }
    
    public static void Rebuild(Layout layout, string? assets)
    {
        var visuals = new Visual[Count];
        
        if (layout == Layout.Keyboard)
        {
            var names = KeyNames();
            for (int i = 0; i < Count; i++) visuals[i] = new Visual { Chars = Labels.Encode(Labels.Key(names[i])) };
            _visuals = visuals;
            _start = new Visual { Chars = Labels.Encode(Labels.Key(Settings.System.Input[0].Keys.Start)) };
            GlyphTextures.Clear();
            return;
        }
        
        var family = Layouts.FamilyOf(layout);
        var codes = PadCodes();
        var faces = new string?[FaceCount];
        
        for (int i = 0; i < FaceCount; i++)
        {
            if (codes[i] is < 0 or >= FaceCount) continue;
            faces[i] = Positions[codes[i]];
            visuals[i] = new Visual { Chars = new[] { FaceChars[i] }, OwnIcon = true };
        }
        
        for (int i = 0; i < Count; i++)
        {
            if (visuals[i] != null) continue;
            int shown = codes[i] is >= 0 and < FaceCount ? Array.IndexOf(faces, Positions[codes[i]]) : -1;
            visuals[i] = shown >= 0
                ? new Visual { Chars = new[] { FaceChars[shown] } }
                : new Visual { Chars = Labels.Encode(Labels.Pad(family, codes[i])) };
        }
        
        int startCode = StartPadCode();
        bool startIcon = startCode == StartCode;
        int startShown = startCode is >= 0 and < FaceCount ? Array.IndexOf(faces, Positions[startCode]) : -1;
        
        _visuals = visuals;
        _start = startIcon
            ? new Visual { OwnIcon = true }
            : startShown >= 0
                ? new Visual { Chars = new[] { FaceChars[startShown] } }
                : new Visual { Chars = Labels.Encode(Labels.Pad(family, startCode)) };
        GlyphTextures.Apply(assets, Layouts.Folder(layout)!, faces, startIcon);
    }
}
