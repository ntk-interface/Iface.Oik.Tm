using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces;

internal static partial class TmNativeDefsUnsafe
{
  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
  public unsafe struct ComputerInfoS
  {
    public uint Len;

    public fixed byte ComputerName[64];

    public uint  NtVerMaj;
    public uint  NtVerMin;
    public uint  NtBuild;
    public uint  Acp;
    public ulong Uptime;
    public byte  UptimeAbs;
    public byte  NtProductType;
    public byte  Win64;
    public byte  _bres1;

    public fixed byte LOctet[8];

    public uint   CurrentGMT;
    public ushort CurrentMs;

    public byte SecType;
    public byte Copyright;
    public byte Endianness;

    public fixed byte Res[3];

    public uint CfsVerMaj;
    public uint CfsVerMin;

    public DomainInfoS DomInfo;

    public fixed byte UserName[64];

    public uint   InternalUt;
    public ushort InternalMs;

    public uint   LocalUt;
    public ushort LocalMs;

    public fixed byte Res2[52];

    public fixed byte UserAddr[64];

    public uint UserIfIp;

    public fixed byte Res3[28];

    public fixed uint IpAddrs[8];

    public uint AccessMask;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
  public unsafe struct DomainInfoS
  {
    public uint PrimaryDomainError;

    public fixed char PrimaryDomainName[100];
    public fixed byte PrimaryDomainSid[100];

    public uint AccountDomainError;

    public fixed char AccountDomainName[100];
    public fixed byte AccountDomainSid[100];
  }
}