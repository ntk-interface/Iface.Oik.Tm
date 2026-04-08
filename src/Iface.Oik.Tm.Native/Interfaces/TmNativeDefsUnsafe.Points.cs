using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces;

internal static partial class TmNativeDefsUnsafe
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TAdrTm
  {
    public short Ch;
    public short RTU;
    public short Point;
  }

  public const int TCommonPointDataSize = 32;
  
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TCommonPoint
  {
    public nint   name;
    public byte   cp_flags;
    public byte   res1;
    public ushort Type;
    public ushort Ch;
    public ushort RTU;
    public ushort Point;
    public uint   TM_Flags;
    public ushort tm_s2;
    public ushort tm_flags2;
    public uint   tm_local_ut;
    public uint   tm_remote_ut;
    public ushort tm_local_ms;
    public ushort tm_remote_ms;

    public fixed byte Data[TCommonPointDataSize];
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TStatusPoint
  {
    public short Status;
    public short Flags;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TAnalogPoint
  {
    public float AsFloat;
    public short AsCode;
    public short Flags;

    public fixed byte Unit[7]; // в tmconn 8, в последнем байте формат

    public byte Format; // вынесли формат отдельно
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TAccumPoint
  {
    public float Value;
    public float Load;
    public short Flags;

    public fixed byte Unit[7]; // в tmconn 8, в последнем байте формат

    public byte Format; // вынесли формат отдельно
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TAnalogPointShort
  {
    public float Value;
    public short Flags;
  }
}