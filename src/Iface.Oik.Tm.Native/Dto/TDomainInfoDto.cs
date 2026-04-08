using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

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
      PrimaryDomainSid   = TmNativeUtil.PtrToArray(domInfo.PrimaryDomainSid, 100),
      AccountDomainError = domInfo.AccountDomainError,
      AccountDomainName  = new string(domInfo.AccountDomainName),
      AccountDomainSid   = TmNativeUtil.PtrToArray(domInfo.AccountDomainSid, 100)
    };
  }
}
