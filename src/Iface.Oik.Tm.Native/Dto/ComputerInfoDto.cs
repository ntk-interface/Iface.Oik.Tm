using System;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Dto;

public record ComputerInfoDto
{
  public uint Len { get; init; }

  public string ComputerName { get; init; } = string.Empty;

  public int  NtVerMaj { get; init; }
  public int  NtVerMin { get; init; }
  public int  NtBuild  { get; init; }
  public uint Acp      { get; init; }

  public long Uptime        { get; init; }
  public byte  UptimeAbs     { get; init; }
  public byte  NtProductType { get; init; }
  public bool  Win64         { get; init; }

  public byte[] SoftwareKeyOctets { get; init; } = Array.Empty<byte>();

  public uint   Current { get; init; }
  public ushort CurrentMs  { get; init; }
  
  public uint   LocalTimeUt { get; init; }
  public ushort LocalTimeMs{ get; init; }
  
  public uint   InternalTimeUt { get; init; }
  public ushort InternalTimeMs { get; init; }

  public byte SecType    { get; init; }
  public byte Copyright { get; init; }
  public byte Endianness { get; init; }

  public int CfsVerMaj { get; init; }
  public int CfsVerMin { get; init; }

  public TDomainInfoDto DomainInfo { get; init; } = new();

  public string UserName { get; init; } = string.Empty;
  public string UserAddr { get; init; } = string.Empty;

  public uint   UserIfIp { get; init; }
  public uint[] IpAddrs  { get; init; } = Array.Empty<uint>();

  public uint AccessMask { get; init; }

  internal static unsafe ComputerInfoDto Create(TmNativeDefsUnsafe.ComputerInfoS info)
  {
    var current = (uint)TmNative.uxgmtime2uxtime(info.CurrentGMT);
    
    return new ComputerInfoDto
    {
      Len          = info.Len,
      ComputerName = TmNativeUtil.BytePtrToString(info.ComputerName, 64),

      NtVerMaj = (int)info.NtVerMaj,
      NtVerMin = (int)info.NtVerMin,
      NtBuild  = (int)info.NtBuild,
      Acp      = info.Acp,

      Uptime        = (long)info.Uptime,
      UptimeAbs     = info.UptimeAbs,
      NtProductType = info.NtProductType,
      Win64         = info.Win64 != 0,

      SoftwareKeyOctets = TmNativeUtil.PtrToArray(info.LOctet, 8),

      Current   = current,
      CurrentMs = info.CurrentMs,
      
      LocalTimeUt = info.LocalUt,
      LocalTimeMs = info.LocalMs,
      
      InternalTimeUt = info.InternalUt,
      InternalTimeMs = info.InternalMs,

      SecType    = info.SecType,
      Copyright  = info.Copyright,
      Endianness = info.Endianness,

      CfsVerMaj = (int)info.CfsVerMaj,
      CfsVerMin = (int)info.CfsVerMin,

      DomainInfo = TDomainInfoDto.Create(info.DomInfo),

      UserName = TmNativeUtil.BytePtrToString(info.UserName, 64),
      UserAddr = TmNativeUtil.BytePtrToString(info.UserAddr, 64),

      UserIfIp = info.UserIfIp,
      IpAddrs  = TmNativeUtil.PtrToArray(info.IpAddrs, 8),

      AccessMask = info.AccessMask
    };
  }
}
