using System;
using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces;

internal static partial class TmNativeDefsUnsafe
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TMSAnalogMSeriesElement
  {
    public float Value;
    public uint Ut;
    public byte   SFlg; // 1 - present, 2 - unreliable
  }
  
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TMSAnalogMSeries
  {
    public ushort Interval;
    public ushort Count;
    
    // TMSAnalogMSeriesElement(size = 9 bytes) variable-size array. Max - 10.  
    public fixed byte Elements[90];
  }
}