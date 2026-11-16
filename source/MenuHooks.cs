using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;
using RecompOne.Runtime.Modding;
using Sotn;

namespace ControllerGlyphs;

public static class MenuHooks
{
    const uint MenuDrawChar = 0x800F678Cu;
    const uint ChButtons = 0x800A2D70u;
    const uint ChButtons2 = 0x800A2D78u;
    
    const uint VanillaFaces = 0xEBE9E8EAu;
    const uint VanillaShoulders = 0x2C322C32u;
    const uint VanillaSuffixes = 0x12111112u;
    const uint MarkedShoulders = 0xEFEEEDECu;
    const uint MarkedSuffixes = 0xF0F0F0F0u;
    
    const byte FirstMarker = 0xE8;
    const byte LastMarker = 0xEF;
    const byte SkipMarker = 0xF0;
    const byte LetterO = 'o' - 0x20;
    const byte LetterR = 'r' - 0x20;
    const int CharWidth = 8;
    
    static readonly int[] MarkerButton =
    {
        Bindings.Circle, Bindings.Cross, Bindings.Square, Bindings.Triangle,
        Bindings.R2, Bindings.L1, Bindings.R1, Bindings.L2,
    };
    
    static bool _drawing;
    static bool _spells;
    static int _rowY;
    static int _shift;
    
    public static void PatchTables(IMemory m)
    {
        if (m.ReadU32(ChButtons) != VanillaFaces) return;
        if (m.ReadU32(ChButtons + 4) != VanillaShoulders || m.ReadU32(ChButtons2) != VanillaSuffixes) return;
        m.WriteU32(ChButtons + 4, MarkedShoulders);
        m.WriteU32(ChButtons2, MarkedSuffixes);
    }
    
    public static void RestoreTables(IMemory m)
    {
        if (m.ReadU32(ChButtons) != VanillaFaces) return;
        if (m.ReadU32(ChButtons + 4) != MarkedShoulders || m.ReadU32(ChButtons2) != MarkedSuffixes) return;
        m.WriteU32(ChButtons + 4, VanillaShoulders);
        m.WriteU32(ChButtons2, VanillaSuffixes);
    }
    
    [PreHook("dra", "MenuSpellsDraw")]
    public static void BeforeSpells(CpuContext c, IMemory m)
    {
        _spells = true;
        _rowY = int.MinValue;
        _shift = 0;
    }
    
    [PostHook("dra", "MenuSpellsDraw")]
    public static void AfterSpells(CpuContext c, IMemory m)
    {
        _spells = false;
    }
    
    [PreHook("dra", "MenuDrawChar")]
    public static bool OnDrawChar(CpuContext c, IMemory m)
    {
        if (_drawing) return true;
        
        byte ch = (byte)c.A0;
        int y = (int)c.A2;
        
        if (ch == SkipMarker) return false;
        
        if (ch < FirstMarker || ch > LastMarker)
        {
            if (_spells && y == _rowY && (ch == LetterO || ch == LetterR)) c.A1 = (uint)((int)c.A1 + _shift);
            return true;
        }
        
        int button = MarkerButton[ch - FirstMarker];
        var chars = Bindings.Of(button).Chars;
        int x = (int)c.A1;
        uint ctx = c.A3;
        
        if (_spells)
        {
            if (y != _rowY)
            {
                _rowY = y;
                _shift = 0;
            }
            x += _shift;
            int expected = button < Bindings.FaceCount ? 1 : 2;
            _shift += (chars.Length - expected) * CharWidth;
        }
        
        _drawing = true;
        try
        {
            for (int i = 0; i < chars.Length; i++)
                GameApi.Call(c, m, MenuDrawChar, chars[i], (uint)(x + i * CharWidth), (uint)y, ctx);
        }
        finally
        {
            _drawing = false;
        }
        
        return false;
    }
}
