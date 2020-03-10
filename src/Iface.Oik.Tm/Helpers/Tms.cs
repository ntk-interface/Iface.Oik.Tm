using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Interfaces;

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

      return tmCid;
    }


    public static bool IsConnected(int tmCid)
    {
      return tmCid != 0 &&
             !string.IsNullOrEmpty(GetSystemTimeString(tmCid));
    }


    public static string GetSystemTimeString(int tmCid)
    {
      var tmcTime = new StringBuilder(80);
      Native.TmcSystemTime(tmCid, ref tmcTime, IntPtr.Zero);
      return tmcTime.ToString();
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
      var nativeUserInfoSize = Marshal.SizeOf(typeof(TmNativeDefs.TUserInfo));
      var nativeUserInfoPtr  = Marshal.AllocHGlobal(nativeUserInfoSize);

      var cfCid = Native.TmcGetCfsHandle(tmCid);
      if (cfCid == 0)
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

      var nativeUserInfo = Marshal.PtrToStructure<TmNativeDefs.TUserInfo>(nativeUserInfoPtr);

      return new TmUserInfo(nativeUserInfo.UserId,
                            Encoding.GetEncoding(1251).GetString(nativeUserInfo.UserName).Trim('\0'),
                            Encoding.GetEncoding(1251).GetString(nativeUserInfo.KeyId).Trim('\0'),
                            nativeUserInfo.Group,
                            nativeUserInfo.Rights);
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


    public static (int tmCid, int rbCid, int rbPort, TmUserInfo userInfo) Initialize(TmInitializeOptions options)
    {
      var (tmCid, userInfo) = InitializeWithoutSql(options);

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

      return (tmCid, rbCid, rbPort, userInfo);
    }


    public static (int tmCid, TmUserInfo userInfo) InitializeWithoutSql(TmInitializeOptions options)
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

      var userInfo = GetUserInfo(tmCid, options.TmServer);
      if (userInfo == null)
      {
        //throw new Exception("Не заданы права пользователя");
      }

      return (tmCid, userInfo);
    }


    public static (int tmCid, int rbCid, int rbPort, TmUserInfo userInfo, uint stopEventHandle)
      InitializeAsTask(TmOikTaskOptions    taskOptions,
                       TmInitializeOptions options)
    {
      var (tmCid, userInfo, stopEventHandle) = InitializeAsTaskWithoutSql(taskOptions, options);

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

      return (tmCid, rbCid, rbPort, userInfo, stopEventHandle);
    }


    public static (int tmCid, TmUserInfo userInfo, uint stopEventHandle)
      InitializeAsTaskWithoutSql(TmOikTaskOptions    taskOptions,
                                 TmInitializeOptions options)
    {
      Native.CfsInitLibrary();

      var taskArgs = Environment.GetCommandLineArgs();
      taskArgs[0] = Native.GetOikTaskExecutable(taskArgs[0]);

      uint startEventHandle = 0;
      uint stopEventHandle  = 0;
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

      var userInfo = GetUserInfo(tmCid, options.TmServer);
      if (userInfo == null)
      {
        //throw new Exception("Не заданы права пользователя");
      }

      return (tmCid, userInfo, stopEventHandle);
    }


    public static bool StopEventSignalDuringWait(uint handle, uint waitMilliseconds)
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