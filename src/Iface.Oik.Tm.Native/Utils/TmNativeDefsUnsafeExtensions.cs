using System;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Native.Utils;

internal static class TUserInfoExtensions
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


public static class TCommonPointExtensions
{
  public static TmNativeDefs.TCommonPoint ToManaged(this TmNativeDefsUnsafe.TCommonPoint point)
  {
    unsafe
    {
      var managedPoint = new TmNativeDefs.TCommonPoint
      {
        name         = point.name,
        cp_flags     = point.cp_flags,
        res1         = point.res1,
        Type         = point.Type,
        Ch           = point.Ch,
        RTU          = point.RTU,
        Point        = point.Point,
        TM_Flags     = point.TM_Flags,
        tm_s2        = point.tm_s2,
        tm_flags2    = point.tm_flags2,
        tm_local_ut  = point.tm_local_ut,
        tm_remote_ut = point.tm_remote_ut,
        tm_local_ms  = point.tm_local_ms,
        tm_remote_ms = point.tm_remote_ms,
        Data         = new byte[TmNativeDefsUnsafe.TCommonPointDataSize],
      };
      new ReadOnlySpan<byte>(point.Data, TmNativeDefsUnsafe.TCommonPointDataSize).CopyTo(managedPoint.Data);

      return managedPoint;
    }
  }
}