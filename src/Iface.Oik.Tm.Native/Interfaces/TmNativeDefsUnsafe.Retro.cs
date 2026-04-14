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
    public const int ElementsMaxCount = 10;

    public ushort Interval;
    public ushort Count;
    
    // max - 10
    public TMSAnalogMSeriesElement* Elements;
  }
}