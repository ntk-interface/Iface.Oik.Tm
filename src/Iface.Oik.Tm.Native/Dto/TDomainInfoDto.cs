using System;
using System.Runtime.InteropServices;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Native.Dto;

public record TDomainInfoDto
{
  public uint   PrimaryDomainError { get; init; }
  public string PrimaryDomainName  { get; init; } = string.Empty;
  public byte[] PrimaryDomainSid   { get; init; } = [];

  public uint   AccountDomainError { get; init; }
  public string AccountDomainName  { get; init; } = string.Empty;
  public byte[] AccountDomainSid   { get; init; } = [];

  internal static unsafe TDomainInfoDto Create(TmNativeDefsUnsafe.DomainInfoS domInfo)
  {
    return new TDomainInfoDto
    {
      PrimaryDomainError = domInfo.PrimaryDomainError,
      PrimaryDomainName  = new string(domInfo.PrimaryDomainName),
      PrimaryDomainSid   = Copy(domInfo.PrimaryDomainSid, 100),
      AccountDomainError = domInfo.AccountDomainError,
      AccountDomainName  = new string(domInfo.AccountDomainName),
      AccountDomainSid   = Copy(domInfo.AccountDomainSid, 100)
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
}
