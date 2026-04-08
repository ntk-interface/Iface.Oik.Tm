using System.Buffers;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static TUserInfoDto GetUserInfo(int cid, uint userId)
  {
    var tUserInfo = GetTUserInfo(cid, userId);
    return TUserInfoDto.Create(userId, tUserInfo);
  }

  public static TUserInfoDto GetExtendedUserInfo(int cid, uint userId)
  {
    const int bufSize   = 1024;
    var       pool      = ArrayPool<byte>.Shared;
    var       buf       = pool.Rent(bufSize);
    var       tUserInfo = new TmNativeDefsUnsafe.TUserInfo();

    try
    {
      if (!TmNative.tmcGetUserInfoEx(cid, userId, ref tUserInfo, buf, bufSize))
      {
        throw new TmNativeException($"Ошибка получения расширенной информации о пользователе {userId}");
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(buf);
    }


    return TUserInfoDto.Create(userId, tUserInfo, TmNativeUtil.BytesToString(buf));
  }
  
  public static TUserInfoDto GetUserInfoCfs(nint   cid,
                                            string serverName,
                                            string serverType)
  {
    var tExtendedUserInfo = GetTExtendedUserInfo(cid, serverName, serverType);
    return TUserInfoDto.Create(tExtendedUserInfo);
  }
  
  public static TUserInfoDto GetUserInfo(int tmCid, string severName)
  {
    var cfCid = TmNative.tmcGetCfsHandle(tmCid);
    
    if (cfCid == nint.Zero)
    {
      throw new TmNativeException("Не удалось получить cfsHandle");
    }

    var extendedInfo = GetTExtendedUserInfo(cfCid, severName, "tms$");
    var tUserInfo    = GetTUserInfo(tmCid, 0);
    
    return TUserInfoDto.Create(tUserInfo, extendedInfo);
  }

  public static string GetUserName(int tmCid, int userId)
  {
    var userInfo = GetTUserInfo(tmCid, (uint)userId);

    unsafe
    {
      return TmNativeUtil.BytePtrToString(userInfo.UserName, 16); 
    }
  }

  internal static TmNativeDefsUnsafe.TUserInfo GetTUserInfo(int tmCid, uint userId)
  {
    var tUserInfo = new TmNativeDefsUnsafe.TUserInfo();

    if (!TmNative.tmcGetUserInfo(tmCid, userId, ref tUserInfo))
    {
      throw new TmNativeException($"Ошибка получения информации о пользователе {userId}");
    }

    return tUserInfo;
  }
  
  internal static unsafe TmNativeDefsUnsafe.TExtendedUserInfo GetTExtendedUserInfo(nint cfCid,
    string                                                                              serverName, string serverType)
  {
    var tExtendedUserInfo = new TmNativeDefsUnsafe.TExtendedUserInfo();

    var fetchResult = TmNative.cfsGetExtendedUserData(cfCid,
                                                      serverType,
                                                      serverName,
                                                      ref tExtendedUserInfo,
                                                      (uint)sizeof(TmNativeDefsUnsafe.TExtendedUserInfo));

    if (fetchResult == 0)
    {
      throw new TmNativeException($"Ошибка получения TExtendedUserInfo. Server name: {serverName}");
    }

    return tExtendedUserInfo;
  }
}