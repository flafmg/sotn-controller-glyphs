using System;
using System.Collections.Generic;

namespace ControllerGlyphs;

public static class Labels
{
    public const int MaxLength = 3;
    
    static readonly Dictionary<int, string> Xbox = new()
    {
        { 0, "A" }, { 1, "B" }, { 2, "X" }, { 3, "Y" }, { 4, "Bk" }, { 6, "St" }, { 7, "LS" }, { 8, "RS" },
        { 9, "LB" }, { 10, "RB" }, { 100, "LT" }, { 101, "RT" },
    };
    
    static readonly Dictionary<int, string> PlayStation = new()
    {
        { 0, "X" }, { 1, "O" }, { 2, "Sq" }, { 3, "Tr" }, { 4, "Se" }, { 6, "St" }, { 7, "L3" }, { 8, "R3" },
        { 9, "L1" }, { 10, "R1" }, { 100, "L2" }, { 101, "R2" },
    };
    
    static readonly Dictionary<int, string> Nintendo = new()
    {
        { 0, "B" }, { 1, "A" }, { 2, "Y" }, { 3, "X" }, { 4, "-" }, { 6, "+" }, { 7, "LS" }, { 8, "RS" },
        { 9, "L" }, { 10, "R" }, { 100, "ZL" }, { 101, "ZR" },
    };
    
    static readonly Dictionary<int, string> Shared = new()
    {
        { 11, "Up" }, { 12, "Dn" }, { 13, "Lf" }, { 14, "Rt" },
        { 102, "L<" }, { 103, "L>" }, { 104, "L^" }, { 105, "Lv" },
        { 106, "R<" }, { 107, "R>" }, { 108, "R^" }, { 109, "Rv" },
    };
    
    static readonly Dictionary<string, string> Keys = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Enter", "Ent" }, { "KeypadEnter", "KEn" }, { "Space", "Spc" }, { "Tab", "Tab" }, { "Escape", "Esc" },
        { "Backspace", "Bks" }, { "Insert", "Ins" }, { "Delete", "Del" }, { "Home", "Hom" }, { "End", "End" },
        { "PageUp", "PgU" }, { "PageDown", "PgD" }, { "CapsLock", "Cap" },
        { "ShiftLeft", "LSh" }, { "ShiftRight", "RSh" }, { "ControlLeft", "LCt" }, { "ControlRight", "RCt" },
        { "AltLeft", "LAl" }, { "AltRight", "RAl" }, { "SuperLeft", "LSu" }, { "SuperRight", "RSu" },
        { "Up", "Up" }, { "Down", "Dn" }, { "Left", "Lf" }, { "Right", "Rt" },
        { "Comma", "," }, { "Period", "." }, { "Slash", "/" }, { "BackSlash", "\\" }, { "Semicolon", ";" },
        { "Apostrophe", "'" }, { "Minus", "-" }, { "Equal", "=" }, { "LeftBracket", "[" }, { "RightBracket", "]" },
        { "GraveAccent", "`" }, { "KeypadAdd", "K+" }, { "KeypadSubtract", "K-" }, { "KeypadMultiply", "K*" },
        { "KeypadDivide", "K/" }, { "KeypadDecimal", "K." },
    };
    
    public static string Pad(Family family, int code)
    {
        var table = family switch
        {
            Family.Xbox => Xbox,
            Family.Nintendo => Nintendo,
            _ => PlayStation,
        };
        if (table.TryGetValue(code, out var label)) return label;
        return Shared.TryGetValue(code, out label) ? label : "?";
    }
    
    public static string Key(string? name)
    {
        if (string.IsNullOrEmpty(name)) return "?";
        if (Keys.TryGetValue(name, out var label)) return label;
        if (name.Length == 1) return name.ToUpperInvariant();
        if (name.StartsWith("Number", StringComparison.OrdinalIgnoreCase) && name.Length == 7) return name[6..];
        if (name.StartsWith("Keypad", StringComparison.OrdinalIgnoreCase) && name.Length == 7) return "K" + name[6..];
        return name.Length > MaxLength ? name[..MaxLength] : name;
    }
    
    public static byte[] Encode(string text)
    {
        var codes = new byte[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];
            codes[i] = ch is >= ' ' and <= '~' ? (byte)(ch - 0x20) : (byte)('?' - 0x20);
        }
        return codes;
    }
}
