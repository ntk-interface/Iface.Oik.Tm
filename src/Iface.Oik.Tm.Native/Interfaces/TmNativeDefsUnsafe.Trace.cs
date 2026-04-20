using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces;

internal static partial class TmNativeDefsUnsafe
{
  public const int IfaceUserServerStringsSize = 64;
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct IfaceUser
  {
    public uint Signature;
    public uint Unique;
    public uint Thid;
    public uint Pid;
    public uint Flags;
    public uint DbgCnt;
    public uint LoudCnt;
    
    public fixed byte Name[IfaceUserServerStringsSize];
    public fixed byte Comment[IfaceUserServerStringsSize];

    public ulong BytesIn;
    public ulong BytesOut;
    public uint Handle;

    public int CreationTime;
  }
  
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct IfaceServer
  {
    public uint Signature;
    public uint Unique;
    public uint Pid;
    public uint Ppid;
    public uint Flags;
    public uint DbgCnt;
    public uint LoudCnt;
    
    public fixed byte Name[IfaceUserServerStringsSize];
    public fixed byte Comment[IfaceUserServerStringsSize];
    
    public ulong BytesIn;
    public ulong BytesOut;
    public uint State;
    public int  CreationTime;
    public uint ResState;
  }
}