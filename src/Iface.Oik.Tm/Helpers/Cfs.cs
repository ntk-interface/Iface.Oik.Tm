using System;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Helpers
{
  public static class Cfs
  {
    public const string ServerTypeTms = "tms$";

    public static void InitNativeLibrary(bool isUtf8 = true, bool ignoreLinuxSignals = false)
    {
      if (isUtf8)
      {
        TmNative.cfsSetUtf8Encoding(true);
      }

      TmNative.cfsInitLibrary(string.Empty,
                              ignoreLinuxSignals ? "nosig" : string.Empty);
    }

    public static void SetUserCredentials(string user,
                                          string password)
    {
      TmNative.cfsSetUser(user, password);
    }

    public static string MakeInprocCrd(string host, string user, string pwd)
    {
      var ptr = TmNative.cfsMakeInprocCrd(TmNativeUtil.StringToBytes(host),
                                          TmNativeUtil.StringToBytes(user),
                                          TmNativeUtil.StringToBytes(pwd));
      if (ptr == IntPtr.Zero)
      {
        return string.Empty;
      }

      var res = TmNativeUtil.GetCStringFromIntPtr(ptr);
      TmNative.cfsFreeMemory(ptr);
      return res;
    }

    public static (nint cfId, string errString, int errorCode) ConnectToCfs(string host)
    {
      const int  errStringLength = 1000;
      Span<byte> errBuf          = stackalloc byte[errStringLength];

      var cfId =
        TmNative.cfsConnect(TmNativeUtil.StringToBytes(host), out uint errCode, errBuf, errStringLength);

      if (cfId == nint.Zero)
      {
        Console.WriteLine($"Ошибка соединения с мастер-сервисом: {errCode} - {TmNativeUtil.BytesToString(errBuf)}");
      }

      return (cfId, TmNativeUtil.BytesToString(errBuf), Convert.ToInt32(errCode));
    }

    public static (nint, CfsDefs.InitializeConnectionResult) InitializeConnection(CfsOptions options)
    {
      SetUserCredentials(options.User, options.Password);

      var (cfsCid, errString, errorCode) = ConnectToCfs(options.Host);

      switch (errorCode)
      {
        case 0:
          return (cfsCid, CfsDefs.InitializeConnectionResult.Ok);
        case 87:
          Console.WriteLine("Соединение не установлено. " + errString);
          return (cfsCid, CfsDefs.InitializeConnectionResult.InvalidLoginOrPassword);
        default:
          Console.WriteLine("Соединение не установлено. " + errString);
          return (cfsCid, CfsDefs.InitializeConnectionResult.NonSpecifiedError);
      }
    }

    public static bool IsConnected(IntPtr cfsCid)
    {
      return cfsCid != IntPtr.Zero;
    }

    public class CfsOptions
    {
      public string Host     { get; set; }
      public string User     { get; set; }
      public string Password { get; set; }
    }

    // todo надо ли вообще здесь такую реализацию

    public static TmUserInfo GetUserInfo(nint cfCid,
                                         string serverName,
                                         string serverType)
    {
      
      var dto = TmNativeApi.GetUserInfoCfs(cfCid, serverName, serverType);

      return new TmUserInfo(dto);
    }

    public static void CloseCfsConnection(nint cfId)
    {
      TmNative.cfsDisconnect(cfId);
    }


    public static (bool hasNus, TmNativeDefs.NewUserSystem nusFlags) GetNewUserSystemFlags(nint cfCid)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var hasNus = TmNative.cfsIfpcNewUserSystemAvaliable(cfCid, out var nusFlags, out errCode, errBuf, errBufLength);
      return (hasNus, (TmNativeDefs.NewUserSystem)nusFlags);
    }
  }
}