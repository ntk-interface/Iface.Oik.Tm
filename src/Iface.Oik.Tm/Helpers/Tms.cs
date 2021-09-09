using System;
using System.Runtime.InteropServices;
using System.Text;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Helpers
{
  public static class Tms
  {
    public static readonly ITmNative        Native                  = new TmNative();
    public static readonly TmNativeCallback EmptyTmCallbackDelegate = delegate { };


    public static void SetUserCredentials(string user,
                                          string password)
    {
      Native.CfsSetUser(user, password);
    }


    public static void SetUserCredentialsForThread(string user,
                                                   string password)
    {
      Native.CfsSetUserForThread(user, password);
    }


    public static void RegisterDatagramFlags(int tmCid, TmDatagramFlags flags)
    {
      Native.TmcSetDgrmFlags(tmCid, (uint) flags);
    }


    public static int Connect(string           host,
                              string           serverName,
                              string           applicationName,
                              TmNativeCallback callback,
                              IntPtr           callbackParameter)
    {
      var tmCid = Native.TmcConnect(host, serverName, applicationName, callback, callbackParameter);

      if (!IsConnected(tmCid))
      {
        tmCid = 0;
      }
      else
      {
        RegisterDatagramFlags(tmCid, TmDatagramFlags.NewClient);
      }

      return tmCid;
    }


    public static int ConnectExplicit(string           host,
                                      string           serverName,
                                      string           applicationName,
                                      TmNativeCallback callback,
                                      IntPtr           callbackParameter,
                                      int              propsCount,
                                      uint[]           props,
                                      uint[]           propsValues)
    {
      var tmCid = Native.TmcConnectEx(host, serverName, applicationName, callback,
                                      callbackParameter, (uint) propsCount, props, propsValues);
      if (!IsConnected(tmCid))
      {
        tmCid = 0;
      }
      else
      {
        RegisterDatagramFlags(tmCid, TmDatagramFlags.NewClient);
      }


      return tmCid;
    }


    public static int DeltaConnect(string           host,
                                   string           serverName,
                                   string           applicationName,
                                   TmNativeCallback callback,
                                   IntPtr           callbackParameter)
    {
      var props       = new uint[] {1}; //Props code 1 - datagram buffer size
      var propsValues = new uint[] {1}; //Props code 1 value = 1 MB

      return ConnectExplicit(host, serverName, applicationName, callback, callbackParameter, 1, props, propsValues);
    }


    public static bool IsConnectedSimple(int tmCid)
    {
      return tmCid != 0 &&
             Native.TmcIsConnected(tmCid) > 0;
    }


    public static bool IsConnected(int tmCid)
    {
      return tmCid != 0 &&
             !string.IsNullOrEmpty(GetSystemTimeString(tmCid));
    }


    public static string GetSystemTimeString(int tmCid)
    {
      var tmcTime = new byte[80];
      Native.TmcSystemTime(tmCid, ref tmcTime, IntPtr.Zero);
      return EncodingUtil.Win1251BytesToUtf8(tmcTime);
    }


    public static DateTime? GetSystemTime(int tmCid)
    {
      return DateTime.TryParse(GetSystemTimeString(tmCid), out var time) ? (DateTime?) time : null;
    }


    public static void Disconnect(int tmCid)
    {
      Native.TmcDisconnect(tmCid);
    }


    public static uint GetReconnectCount(int tmCid)
    {
      return Native.TmcReconnectCount(tmCid);
    }


    public static int GetLastError()
    {
      return (int) Native.TmcGetLastError();
    }


    public static bool IsServerInPassiveMode(int tmCid)
    {
      var info = new TmNativeDefs.TServerInfo();

      Native.TmcGetServerInfo(tmCid, ref info);

      return info.Description?.Contains("passive") == true;
    }


    public static TmPasswordNeedsChangeResult CheckIfPasswordNeedsChange(int tmCid)
    {
      var cfCid = Native.TmcGetCfsHandle(tmCid);
      if (cfCid == IntPtr.Zero)
      {
        return TmPasswordNeedsChangeResult.Error;
      }
      
      const int errStringLength = 1000;
      var       errString       = new byte[errStringLength];
      uint      errCode         = 0;

      Native.CfsIfpcNewUserSystemAvaliable(cfCid, out var nusFlags, out errCode, ref errString, errStringLength);
      var flags = (TmNativeDefs.NewUserSystem) nusFlags;

      if (flags.HasFlag(TmNativeDefs.NewUserSystem.AdminChangePassword))
      {
        return TmPasswordNeedsChangeResult.NeedsChangeByAdmin;
      }
      if (flags.HasFlag(TmNativeDefs.NewUserSystem.ChangePassword))
      {
        return TmPasswordNeedsChangeResult.NeedsChange;
      }
      return TmPasswordNeedsChangeResult.Ok;
    }


    public static (bool, string) ChangeUserPassword(int tmCid, string username, string password)
    {
      var cfCid = Native.TmcGetCfsHandle(tmCid);
      if (cfCid == IntPtr.Zero)
      {
        return (false, "Ошибка получения идентификатора пользователя");
      }
      
      const int errStringLength = 1000;
      var       errString       = new byte[errStringLength];
      uint      errCode         = 0;

      if (!username.StartsWith("*")) // сервер требует в начале имени звездочку
      {
        username = "*" + username;
      }

      if (Native.CfsIfpcSetUserPwd(cfCid, username, password, out errCode, ref errString, errStringLength))
      {
        return (true, string.Empty);
      }
      else
      {
        return (false, EncodingUtil.Win1251BytesToUtf8(errString));
      }
    }


    public static TmSecurityAccessFlags GetSecurityAccessFlags(int tmCid)
    {
      if (Native.RbcGetSecurity(tmCid, out var isAdmin, out var accessFlags) == 0)
      {
        return TmSecurityAccessFlags.None;
      }

      return (TmSecurityAccessFlags) accessFlags;
    }


    public static TmUserInfo GetUserInfo(int    tmCid,
                                         string serverName)
    {
      var nativeUserInfoSize = Marshal.SizeOf(typeof(TmNativeDefs.TExtendedUserInfo));
      var nativeUserInfoPtr  = Marshal.AllocHGlobal(nativeUserInfoSize);

      var cfCid = Native.TmcGetCfsHandle(tmCid);
      if (cfCid == IntPtr.Zero)
      {
        return null;
      }

      var fetchResult = Native.CfsGetExtendedUserData(cfCid,
                                                      "tms$",
                                                      serverName,
                                                      nativeUserInfoPtr,
                                                      (uint) nativeUserInfoSize);
      if (fetchResult == 0)
      {
        return null;
      }

      var extendedUserInfo = Marshal.PtrToStructure<TmNativeDefs.TExtendedUserInfo>(nativeUserInfoPtr);

      var userInfo = new TmNativeDefs.TUserInfo();
      if (!Native.TmcGetUserInfo(tmCid, 0, ref userInfo))
      {
        return null;
      }

      return new TmUserInfo(extendedUserInfo.UserId,
                            Encoding.GetEncoding(1251).GetString(extendedUserInfo.UserName).Trim('\0'),
                            Encoding.GetEncoding(1251).GetString(userInfo.UserCategory).Trim('\0'),
                            Encoding.GetEncoding(1251).GetString(extendedUserInfo.KeyId).Trim('\0'),
                            extendedUserInfo.Group,
                            extendedUserInfo.Rights);
    }


    public static TmServerFeatures GetTmServerFeatures(int tmCid)
    {
      var capabilitiesBuf = new byte[16];
      if (Native.TmcGetServerCaps(tmCid, ref capabilitiesBuf) == 0)
      {
        return TmServerFeatures.Empty;
      }

      return new TmServerFeatures(IsCapabilityEnabled(capabilitiesBuf, TmNativeDefs.ServerCap.Comtrade),
                                  IsCapabilityEnabled(capabilitiesBuf, TmNativeDefs.ServerCap.MicroSeries),
                                  IsImpulseArchiveEnabled(),
                                  AreTechObjectsEnabled());

      bool IsCapabilityEnabled(byte[] capabilities, TmNativeDefs.ServerCap capability)
      {
        var capabilityByte = (byte) capability;
        return ((capabilities[capabilityByte / 8] >> (capabilityByte % 8)) & 1) > 0;
      }

      bool IsImpulseArchiveEnabled()
      {
        var stats = new TmNativeDefs.TM_AAN_STATS();
        return Native.TmcAanGetStats(tmCid, ref stats, 0);
      }

      bool AreTechObjectsEnabled()
      {
        return true;
        var tobPtr = Native.TmcTechObjEnumValues(tmCid, uint.MaxValue, uint.MaxValue, IntPtr.Zero, out var count);
        if (tobPtr == IntPtr.Zero)
        {
          return false;
        }
        Native.TmcFreeMemory(tobPtr);
        return count > 0;
      }
    }


    public static int GetLicenseFeature(int tmCid, LicenseFeature feature)
    {
      return Native.TmcGetServerFeature(tmCid, (uint) feature);
    }


    public static int OpenSqlRedirector(int rbCid)
    {
      return Native.RbcIpgStartRedirector(rbCid, 0);
    }


    public static bool CloseSqlRedirector(int rbCid)
    {
      return Native.RbcIpgStopRedirector(rbCid, 0);
    }


    public static void PrintDebug(object message)
    {
      Print(TmPrintLevel.Debug, message);
    }


    public static void PrintMessage(object message)
    {
      Print(TmPrintLevel.Message, message);
    }


    public static void PrintError(object message)
    {
      Print(TmPrintLevel.Error, message);
    }


    public static void Print(TmPrintLevel level, object message)
    {
      var messageString = message + "\n";
      switch (level)
      {
        case TmPrintLevel.Debug:
          Native.DPrintF(messageString);
          break;
        case TmPrintLevel.Message:
          Native.MPrintF(messageString);
          break;
        case TmPrintLevel.Error:
          Native.EPrintF(messageString);
          break;
      }
    }


    public static long GetServerPseudoUnixTimestamp(long unixTimestamp)
    {
      return Native.UxGmTime2UxTime(unixTimestamp);
    }


    public static (int tmCid, int rbCid, int rbPort, TmUserInfo userInfo, TmServerFeatures serverFeatures)
      Initialize(TmInitializeOptions options)
    {
      var (tmCid, userInfo, serverFeatures) = InitializeWithoutSql(options);

      var rbCid = Connect(options.Host,
                          options.RbServer,
                          options.ApplicationName,
                          EmptyTmCallbackDelegate,
                          IntPtr.Zero);
      if (rbCid == 0)
      {
        throw new Exception("Нет связи с RB-сервером, ошибка " + GetLastError());
      }

      var rbPort = OpenSqlRedirector(rbCid);
      if (rbPort == 0)
      {
        throw new Exception("Ошибка при открытии редиректора к SQL");
      }

      return (tmCid, rbCid, rbPort, userInfo, serverFeatures);
    }


    public static (int tmCid, TmUserInfo userInfo, TmServerFeatures serverFeatures)
      InitializeWithoutSql(TmInitializeOptions options)
    {
      Native.CfsInitLibrary();

      SetUserCredentials(options.User,
                         options.Password);

      var tmCid = Connect(options.Host,
                          options.TmServer,
                          options.ApplicationName,
                          options.TmCallback,
                          options.TmCallbackParameters);
      if (tmCid == 0)
      {
        throw new Exception("Нет связи с ТМ-сервером, ошибка " + GetLastError());
      }

      return (tmCid, GetUserInfo(tmCid, options.TmServer), GetTmServerFeatures(tmCid));
    }


    public static (int tmCid, int rbCid, int rbPort, TmUserInfo userInfo, TmServerFeatures serverFeatures, IntPtr
      stopEventHandle)
      InitializeAsTask(TmOikTaskOptions taskOptions, TmInitializeOptions options)
    {
      var (tmCid, userInfo, serverFeatures, stopEventHandle) = InitializeAsTaskWithoutSql(taskOptions, options);

      var rbCid = Connect(options.Host,
                          options.RbServer,
                          options.ApplicationName,
                          EmptyTmCallbackDelegate,
                          IntPtr.Zero);
      if (rbCid == 0)
      {
        throw new Exception("Нет связи с RB-сервером, ошибка " + GetLastError());
      }

      var rbPort = OpenSqlRedirector(rbCid);
      if (rbPort == 0)
      {
        throw new Exception("Ошибка при открытии редиректора к SQL");
      }

      return (tmCid, rbCid, rbPort, userInfo, serverFeatures, stopEventHandle);
    }


    public static (int tmCid, TmUserInfo userInfo, TmServerFeatures serverFeatures, IntPtr stopEventHandle)
      InitializeAsTaskWithoutSql(TmOikTaskOptions taskOptions, TmInitializeOptions options)
    {
      Native.CfsInitLibrary();

      var taskArgs = Environment.GetCommandLineArgs();
      taskArgs[0] = Native.GetOikTaskExecutable(taskArgs[0]);

      var startEventHandle = new IntPtr();
      var stopEventHandle  = new IntPtr();
      Native.CfsPmonLocalRegisterProcess(taskArgs.Length,
                                         taskArgs,
                                         ref startEventHandle,
                                         ref stopEventHandle);
      Native.PlatformSetEvent(startEventHandle);

      SetUserCredentials(options.User,
                         options.Password);

      var tmCid = Connect(options.Host,
                          options.TmServer,
                          options.ApplicationName,
                          options.TmCallback,
                          options.TmCallbackParameters);
      if (tmCid == 0)
      {
        throw new Exception("Нет связи с ТМ-сервером, ошибка " + GetLastError());
      }

      return (tmCid, GetUserInfo(tmCid, options.TmServer), GetTmServerFeatures(tmCid), stopEventHandle);
    }
    
    
    public static (int tmCid, IntPtr stopEventHandle) InitializeAsTaskSimple(TmOikTaskOptions taskOptions, 
                                                                           TmInitializeOptions options)
    {
      Native.CfsInitLibrary();

      var taskArgs = Environment.GetCommandLineArgs();
      taskArgs[0] = Native.GetOikTaskExecutable(taskArgs[0]);

      var startEventHandle = new IntPtr();
      var stopEventHandle  = new IntPtr();
      Native.CfsPmonLocalRegisterProcess(taskArgs.Length,
                                         taskArgs,
                                         ref startEventHandle,
                                         ref stopEventHandle);
      Native.PlatformSetEvent(startEventHandle);

      SetUserCredentials(options.User,
                         options.Password);

      var tmCid = Native.TmcConnect(options.Host,
                                    options.TmServer,
                                    options.ApplicationName,
                                    options.TmCallback,
                                    options.TmCallbackParameters);
      if (tmCid == 0)
      {
        throw new Exception("Нет связи с ТМ-сервером (cid = 0), ошибка " + GetLastError());
      }

      return (tmCid, stopEventHandle);
    }


    public static bool StopEventSignalDuringWait(IntPtr handle, uint waitMilliseconds)
    {
      return Native.PlatformWaitForSingleObject(handle, waitMilliseconds) == 0;
    }


    public static void Terminate(int tmCid, int rbCid)
    {
      TerminateWithoutSql(tmCid);
      CloseSqlRedirector(rbCid);
      Disconnect(rbCid);
    }


    public static void TerminateWithoutSql(int tmCid)
    {
      Disconnect(tmCid);
    }
  }


  public class TmInitializeOptions
  {
    public string           Host                 { get; set; }
    public string           TmServer             { get; set; }
    public string           RbServer             { get; set; }
    public string           User                 { get; set; }
    public string           Password             { get; set; }
    public string           ApplicationName      { get; set; }
    public TmNativeCallback TmCallback           { get; set; }
    public IntPtr           TmCallbackParameters { get; set; } = IntPtr.Zero;
  }


  public class TmOikTaskOptions
  {
    public string TraceName    { get; set; }
    public string TraceComment { get; set; }
  }
}