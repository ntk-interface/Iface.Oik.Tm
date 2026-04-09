using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Dto;

public record TUserInfoDto
{
  public int    Id                         { get; init; }
  public bool   Valid                      { get; init; }
  public string UserName                   { get; init; } = string.Empty;
  public string UserComment                { get; init; } = string.Empty;
  public string NtUserName                 { get; init; } = string.Empty;
  public string NtUserDomain               { get; init; } = string.Empty;
  public uint   DatagramMask               { get; init; }
  public uint   AccessMask                 { get; init; }
  public uint   ConnectTime                { get; init; }
  public string UserCategory               { get; init; } = string.Empty;
  public string OldUserName                { get; init; } = string.Empty;
  public string AdditionalParametersString { get; init; } = string.Empty;

  public string KeyId           { get; init; } = string.Empty;
  public int    GroupId         { get; init; }
  public byte[] PermissionBytes { get; init; } =  [];

  internal static unsafe TUserInfoDto Create(uint id, TmNativeDefsUnsafe.TUserInfo userInfo)
  {
    return new TUserInfoDto
    {
      Id           = (int)id,
      Valid        = userInfo.Valid != 0,
      UserName     = TmNativeUtil.BytePtrToString(userInfo.UserName,     16),
      UserComment  = TmNativeUtil.BytePtrToString(userInfo.UserComment,  64),
      NtUserName   = TmNativeUtil.BytePtrToString(userInfo.NtUserName,   32),
      NtUserDomain = TmNativeUtil.BytePtrToString(userInfo.NtUserDomain, 32),
      DatagramMask = userInfo.DatagramMask,
      AccessMask   = userInfo.AccessMask,
      ConnectTime  = userInfo.ConnectTime,
      UserCategory = TmNativeUtil.BytePtrToString(userInfo.UserCategory, 64),
      OldUserName  = TmNativeUtil.BytePtrToString(userInfo.OldUserName,  16)
    };
  }

  internal static unsafe TUserInfoDto Create(uint                         id,
                                             TmNativeDefsUnsafe.TUserInfo userInfo,
                                             string                       additionalParameters)
  {
    return new TUserInfoDto
    {
      Id                         = (int)id,
      Valid                      = userInfo.Valid != 0,
      UserName                   = TmNativeUtil.BytePtrToString(userInfo.UserName,     16),
      UserComment                = TmNativeUtil.BytePtrToString(userInfo.UserComment,  64),
      NtUserName                 = TmNativeUtil.BytePtrToString(userInfo.NtUserName,   32),
      NtUserDomain               = TmNativeUtil.BytePtrToString(userInfo.NtUserDomain, 32),
      DatagramMask               = userInfo.DatagramMask,
      AccessMask                 = userInfo.AccessMask,
      ConnectTime                = userInfo.ConnectTime,
      UserCategory               = TmNativeUtil.BytePtrToString(userInfo.UserCategory, 64),
      OldUserName                = TmNativeUtil.BytePtrToString(userInfo.OldUserName,  16),
      AdditionalParametersString = additionalParameters
    };
  }

  internal static unsafe TUserInfoDto Create(TmNativeDefsUnsafe.TExtendedUserInfo userInfo)
  {
    return new TUserInfoDto
    {
      Id              = userInfo.UserId,
      UserName        = TmNativeUtil.BytePtrToString(userInfo.UserName, 16),
      UserCategory    = string.Empty,
      KeyId           = TmNativeUtil.BytePtrToString(userInfo.KeyId, 16),
      GroupId         = userInfo.Group,
      PermissionBytes = TmNativeUtil.PtrToArray(userInfo.Rights, TmNativeDefsUnsafe.TExtendedUserInfoRightsSize)
    };
  }

  internal static unsafe TUserInfoDto Create(TmNativeDefsUnsafe.TUserInfo         userInfo,
                                             TmNativeDefsUnsafe.TExtendedUserInfo extendedUserInfo)
  {
    return new TUserInfoDto
    {
      Id           = extendedUserInfo.UserId,
      UserName     = TmNativeUtil.BytePtrToString(extendedUserInfo.UserName, 16),
      UserCategory = TmNativeUtil.BytePtrToString(userInfo.UserCategory,     64),
      KeyId        = TmNativeUtil.BytePtrToString(extendedUserInfo.KeyId,    16),
      GroupId      = extendedUserInfo.Group,
      PermissionBytes = TmNativeUtil.PtrToArray(extendedUserInfo.Rights, 
                                                    TmNativeDefsUnsafe.TExtendedUserInfoRightsSize)
    };
  }
}