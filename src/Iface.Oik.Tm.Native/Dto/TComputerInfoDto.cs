using System;
using System.Runtime.InteropServices;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Dto;

public record TComputerInfoDto
{
  public uint Len { get; init; }

  public string ComputerName { get; init; } = string.Empty;

  public uint NtVerMaj { get; init; }
  public uint NtVerMin { get; init; }
  public uint NtBuild  { get; init; }
  public uint Acp      { get; init; }

  public ulong Uptime        { get; init; }
  public byte  UptimeAbs     { get; init; }
  public byte  NtProductType { get; init; }
  public bool  Win64         { get; init; }

  public byte[] SoftwareKeyOctets { get; init; } = [];

  public uint   CurrentGMT { get; init; }
  public ushort CurrentMs  { get; init; }

  public byte SecType    { get; init; }
  public byte Copyright { get; init; }
  public byte Endianness { get; init; }

  public uint CfsVerMaj { get; init; }
  public uint CfsVerMin { get; init; }

  public TDomainInfoDto DomainInfo { get; init; } = new();

  public string UserName { get; init; } = string.Empty;
  public string UserAddr { get; init; } = string.Empty;

  public uint   UserIfIp { get; init; }
  public uint[] IpAddrs  { get; init; } = [];

  public uint AccessMask { get; init; }

  internal static unsafe TComputerInfoDto Create(TmNativeDefsUnsafe.ComputerInfoS info)
  {
    return new TComputerInfoDto
    {
      Len          = info.Len,
      ComputerName = TmNativeUtil.BytePtrToString(info.ComputerName, 64),

      NtVerMaj = info.NtVerMaj,
      NtVerMin = info.NtVerMin,
      NtBuild  = info.NtBuild,
      Acp      = info.Acp,

      Uptime        = info.Uptime,
      UptimeAbs     = info.UptimeAbs,
      NtProductType = info.NtProductType,
      Win64         = info.Win64 != 0,

      SoftwareKeyOctets = Copy(info.LOctet, 8),

      CurrentGMT = info.CurrentGMT,
      CurrentMs  = info.CurrentMs,

      SecType    = info.SecType,
      Copyright  = info.Copyright,
      Endianness = info.Endianness,

      CfsVerMaj = info.CfsVerMaj,
      CfsVerMin = info.CfsVerMin,

      DomainInfo = TDomainInfoDto.Create(info.DomInfo),

      UserName = TmNativeUtil.BytePtrToString(info.UserName, 64),
      UserAddr = TmNativeUtil.BytePtrToString(info.UserAddr, 64),

      UserIfIp = info.UserIfIp,
      IpAddrs  = Copy(info.IpAddrs, 8),

      AccessMask = info.AccessMask
    };
  }

  private static unsafe byte[] Copy(byte* ptr, int length)
  {
    if (ptr == null || length <= 0)
    {
      return Array.Empty<byte>();
    }

    var result = new byte[length];
    Marshal.Copy((nint)ptr, result, 0, length);
    return result;
  }

  private static unsafe uint[] Copy(uint* ptr, int length)
  {
    if (ptr == null || length <= 0)
    {
      return Array.Empty<uint>();
    }

    var result = new uint[length];
    fixed (uint* dest = result)
    {
      Buffer.MemoryCopy(ptr, dest, length * sizeof(uint), length * sizeof(uint));
    }

    return result;
  }
}
