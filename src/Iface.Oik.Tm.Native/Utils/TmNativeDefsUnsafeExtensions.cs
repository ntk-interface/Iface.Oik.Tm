using System;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Native.Utils;

public static class TUserInfoExtensions
{
  public static TmNativeDefs.TUserInfo ToManaged(this TmNativeDefsUnsafe.TUserInfo user)
  {
    unsafe
    {
      return new TmNativeDefs.TUserInfo
      {
        Valid        = user.Valid != 0, 
        UserName     = TmNativeUtil.BytePtrToString(user.UserName,     16), 
        UserComment  = TmNativeUtil.BytePtrToString(user.UserComment,  64), 
        NtUserName   = TmNativeUtil.BytePtrToString(user.NtUserName,   32), 
        NtUserDomain = TmNativeUtil.BytePtrToString(user.NtUserDomain, 32), 
        DatagramMask = user.DatagramMask, 
        AccessMask   = user.AccessMask, 
        ConnectTime  = user.ConnectTime, 
        UserCategory = TmNativeUtil.BytePtrToString(user.UserCategory, 64),
        OldUserName  = TmNativeUtil.BytePtrToString(user.OldUserName,  16)
      };
    }
  }
  
  public static string GetUserName(this TmNativeDefsUnsafe.TUserInfo user)
  {
    unsafe
    {
      return TmNativeUtil.BytePtrToString(user.UserName, 16);
    }
  }
  
  public static string GetUserCategory(this TmNativeDefsUnsafe.TUserInfo user)
  {
    unsafe
    {
      return TmNativeUtil.BytePtrToString(user.UserCategory, 64);
    }
  }
}

public static class TExtendedUserInfoExtensions
{
  public static TmNativeDefs.TExtendedUserInfo ToManaged(this TmNativeDefsUnsafe.TExtendedUserInfo user)
  {
    unsafe
    {
      return new TmNativeDefs.TExtendedUserInfo
      {
        RecNum   = user.RecNum,
        UserId   = user.UserId,
        Group    = user.Group,
        KeyId    = TmNativeUtil.BytePtrToString(user.KeyId,    16), 
        UserName = TmNativeUtil.BytePtrToString(user.UserName, 16), 
        UserPwd  = TmNativeUtil.BytePtrToString(user.UserPwd,  8), 
        Rights   = TmNativeUtil.BytePtrToArray(user.Rights, 250),
      };
    }
  }
  
  public static string GetUserName(this TmNativeDefsUnsafe.TExtendedUserInfo user)
  {
    unsafe
    {
      return TmNativeUtil.BytePtrToString(user.UserName, 16);
    }
  }
  
  public static string GetKeyId(this TmNativeDefsUnsafe.TExtendedUserInfo user)
  {
    unsafe
    {
      return TmNativeUtil.BytePtrToString(user.KeyId, 16);
    }
  }
}