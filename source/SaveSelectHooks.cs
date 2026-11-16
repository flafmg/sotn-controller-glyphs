using System;
using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;
using RecompOne.Runtime.Modding;
using Sotn;

namespace ControllerGlyphs;

public static class SaveSelectHooks
{
    const uint DrawImage8x8 = 0x801B2A9Cu;
    const uint PrimBuf = 0x80086FECu;
    const uint TipsPrimIndex = 0x801BAF18u + 6 * 8;
    const uint PrimSize = 0x34;
    const uint PrimX = 0x08;
    const uint PrimY = 0x0A;
    const uint PrimU = 0x0C;
    const uint PrimV = 0x0D;
    const uint PrimTpage = 0x1A;
    const uint PrimDrawMode = 0x32;
    const uint VertexStride = 0x0C;
    const int VertexCount = 4;
    const ushort DrawHide = 0x08;
    const byte TipsU = 0xA0;
    const byte DpadV = 0x50;
    const ushort TipsTpage = 0x0C;
    const int Parked = 0x200;
    const int ParkedFrom = 0x180;
    const int IconSize = 16;
    const int CharWidth = 8;
    const int TextOffsetY = 4;
    
    [PostHook("sel", "DrawNavigationTips")]
    public static void AfterDrawTips(CpuContext c, IMemory m) => Refresh(c, m, false);
    
    [PostHook("sel", "SEL_Update")]
    public static void AfterUpdate(CpuContext c, IMemory m) => Refresh(c, m, true);
    
    public static void Restore(IMemory m)
    {
        if (!FindTips(m, out uint cross, out uint triangle, out uint start)) return;
        Park(m, cross, false);
        if (triangle != 0) Park(m, triangle, false);
        if (start != 0) Park(m, start, false);
    }
    
    static void Refresh(CpuContext c, IMemory m, bool draw)
    {
        if (!FindTips(m, out uint cross, out uint triangle, out uint start)) return;
        Relabel(c, m, cross, Bindings.Of(Bindings.Cross), draw);
        if (triangle != 0) Relabel(c, m, triangle, Bindings.Of(Bindings.Triangle), draw);
        if (start != 0) Relabel(c, m, start, Bindings.Start, draw);
    }
    
    static bool FindTips(IMemory m, out uint cross, out uint triangle, out uint start)
    {
        cross = 0;
        triangle = 0;
        start = 0;
        uint index = m.ReadU32(TipsPrimIndex);
        if (index == 0 || index >= 0x500) return false;
        uint dpad = PrimBuf + index * PrimSize;
        if (m.ReadU8(dpad + PrimU) != TipsU || m.ReadU8(dpad + PrimV) != DpadV) return false;
        if (m.ReadU16(dpad + PrimTpage) != TipsTpage) return false;
        cross = m.ReadU32(dpad);
        if (cross == 0) return false;
        triangle = m.ReadU32(cross);
        if (triangle != 0) start = m.ReadU32(triangle);
        return true;
    }
    
    static bool IsParked(IMemory m, uint prim) => (short)m.ReadU16(prim + PrimY) >= ParkedFrom;
    
    static void Park(IMemory m, uint prim, bool parked)
    {
        if (IsParked(m, prim) == parked) return;
        int delta = parked ? Parked : -Parked;
        for (uint i = 0; i < VertexCount; i++)
        {
            uint address = prim + PrimY + i * VertexStride;
            m.WriteU16(address, (ushort)((short)m.ReadU16(address) + delta));
        }
    }
    
    static void Relabel(CpuContext c, IMemory m, uint prim, Visual visual, bool draw)
    {
        Park(m, prim, !visual.OwnIcon);
        if (!draw || visual.OwnIcon) return;
        if ((m.ReadU16(prim + PrimDrawMode) & DrawHide) != 0) return;
        
        int x = (short)m.ReadU16(prim + PrimX) + IconSize - visual.Chars.Length * CharWidth;
        int y = (short)m.ReadU16(prim + PrimY) - Parked + TextOffsetY;
        
        for (int i = 0; i < visual.Chars.Length; i++)
            GameApi.Call(c, m, DrawImage8x8, visual.Chars[i], (uint)(x + i * CharWidth), (uint)y, 1);
    }
}
