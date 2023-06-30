using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Helpers
{
  public static class Tms
  {
    public static readonly ITmNative        Native                  = new TmNative();
    public static readonly TmNativeCallback EmptyTmCallbackDelegate = delegate { };


    public static void InitNativeLibrary()
    {
      Native.CfsInitLibrary();
    }


    public static void InitNativeLibraryAndIgnoreLinuxSignals()
    {
      Native.CfsInitLibrary(extArg: "nosig");
    }


    public static void SetUserCredentials(string user,
                                          string password)
    {
      Native.CfsSetUser(EncodingUtil.Utf8ToWin1251Bytes(user), EncodingUtil.Utf8ToWin1251Bytes(password));
    }


    public static void ClearUserCredentials()
    {
      Native.CfsSetUser(new byte[1] { 0 }, new byte[1] { 0 });
    }


    public static void SetUserCredentialsForThread(string user,
                                                   string password)
    {
      Native.CfsSetUserForThread(EncodingUtil.Utf8ToWin1251Bytes(user), EncodingUtil.Utf8ToWin1251Bytes(password));
    }


    public static void RegisterDatagramFlags(int tmCid, TmDatagramFlags flags)
    {
      Native.TmcSetDgrmFlags(tmCid, (uint)flags);
    }


    public static int Connect(string           host,
                              string           serverName,
                              string           applicationName,
                              TmNativeCallback callback,
                              IntPtr           callbackParameter,
                              bool             returnCidAnyway = false)
    {
      var tmCid = Native.TmcConnect(host, serverName, applicationName, callback, callbackParameter);

      if (!IsConnected(tmCid))
      {
        if (!returnCidAnyway)
        {
          tmCid = 0;
        }
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
                                      uint[]           propsValues,
                                      bool             returnCidAnyway = false)
    {
      var tmCid = Native.TmcConnectEx(host, serverName, applicationName, callback,
                                      callbackParameter, (uint)propsCount, props, propsValues);
      if (!IsConnected(tmCid))
      {
        if (!returnCidAnyway)
        {
          tmCid = 0;
        }
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
      var props       = new uint[] { 1 }; //Props code 1 - datagram buffer size
      var propsValues = new uint[] { 1 }; //Props code 1 value = 1 MB

      return ConnectExplicit(host, serverName, applicationName, callback, callbackParameter, 1, props, propsValues);
    }


    public static bool IsConnectedSimple(int tmCid)
    {
      return tmCid                        != 0 &&
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
      return DateUtil.GetDateTimeFromTmString(GetSystemTimeString(tmCid));
    }


    public static void Disconnect(int tmCid)
    {
      Native.TmcDisconnect(tmCid);
    }


    public static uint GetReconnectCount(int tmCid)
    {
      return Native.TmcReconnectCount(tmCid);
    }


    public static void UpdateConnection(int tmCid)
    {
      Native.TmcUpdateConnection(tmCid);
    }


    public static int GetLastError()
    {
      return (int)Native.TmcGetLastError();
    }


    public static string GetConnectionErrorText(int tmCid)
    {
      const uint bufSize = 256;
      var        buf     = new byte[bufSize];

      var result = Native.TmcGetConnectErrorText(tmCid, ref buf, bufSize);

      return result ? EncodingUtil.Win1251BytesToUtf8(buf).Trim() : "Неизвестная ошибка";
    }


    public static string GetTelecontrolResultDescription(TmTelecontrolResult result) // TODO integration test
    {
      var descriptionPtr = Native.TmcDecodeTcError((ushort)result);
      if (descriptionPtr == IntPtr.Zero)
      {
        return string.Empty;
      }
      return TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(descriptionPtr);
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

      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      Native.CfsIfpcNewUserSystemAvaliable(cfCid, out var nusFlags, out errCode, ref errBuf, errBufLength);
      var flags = (TmNativeDefs.NewUserSystem)nusFlags;

      if (flags.HasFlag(TmNativeDefs.NewUserSystem.ChangePassword) && 
          flags.HasFlag(TmNativeDefs.NewUserSystem.AdminChangePassword))
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

      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      if (!username.StartsWith("*")) // сервер требует в начале имени звездочку
      {
        username = "*" + username;
      }

      if (Native.CfsIfpcSetUserPwd(cfCid, EncodingUtil.Utf8ToWin1251Bytes(username), EncodingUtil.Utf8ToWin1251Bytes(password), out errCode, ref errBuf, errBufLength))
      {
        return (true, string.Empty);
      }
      else
      {
        return (false, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
    }


    public static string GetLinkedRbServerName(int tmCid, string tmServerName)
    {
      var cfCid = Native.TmcGetCfsHandle(tmCid);
      if (cfCid == IntPtr.Zero)
      {
        return string.Empty;
      }
      var       bufSize      = 255u;
      var       buf          = new byte[bufSize];
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = Native.CfsGetIniString(cfCid,
                                          EncodingUtil.Utf8ToWin1251Bytes("@@"),
                                          EncodingUtil.Utf8ToWin1251Bytes("LinkedServer"),
                                          EncodingUtil.Utf8ToWin1251Bytes(tmServerName),
                                          EncodingUtil.Utf8ToWin1251Bytes(string.Empty),
                                          ref buf,
                                          out bufSize,
                                          out errCode,
                                          ref errBuf,
                                          errBufLength);
      if (!result)
      {
        return string.Empty;
      }
      return EncodingUtil.Win1251BytesToUtf8(buf);
    }


    public static TmSecurityAccessFlags GetSecurityAccessFlags(int tmCid)
    {
      if (Native.RbcGetSecurity(tmCid, out var isAdmin, out var accessFlags) == 0)
      {
        return TmSecurityAccessFlags.None;
      }

      return (TmSecurityAccessFlags)accessFlags;
    }


    public static TmUserInfo GetUserInfo(int    tmCid,
                                         string serverName)
    {
      var nativeUserInfoSize = Marshal.SizeOf(typeof(TmNativeDefs.TExtendedUserInfo));

      var cfCid = Native.TmcGetCfsHandle(tmCid);
      if (cfCid == IntPtr.Zero)
      {
        return null;
      }
	 var nativeUserInfoPtr = Marshal.AllocHGlobal(nativeUserInfoSize);

	 var fetchResult = Native.CfsGetExtendedUserData(cfCid,
                                                      "tms$",
                                                      serverName,
                                                      nativeUserInfoPtr,
                                                      (uint)nativeUserInfoSize);
      if (fetchResult == 0)
      {
		Marshal.FreeHGlobal(nativeUserInfoPtr); // не забываем освобождать память из HGlobal
		return null;
      }

      var extendedUserInfo = Marshal.PtrToStructure<TmNativeDefs.TExtendedUserInfo>(nativeUserInfoPtr);
	  Marshal.FreeHGlobal(nativeUserInfoPtr); // не забываем освобождать память из HGlobal

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


    public static string GetUserName(int tmCid, int userId)
    {
      var userInfo = new TmNativeDefs.TUserInfo();
      if (!Native.TmcGetUserInfo(tmCid, (uint) userId, ref userInfo))
      {
        return string.Empty;
      }
      return Encoding.GetEncoding(1251).GetString(userInfo.UserName).Trim('\0');
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
        var capabilityByte = (byte)capability;
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
      return Native.TmcGetServerFeature(tmCid, (uint)feature);
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
          Native.DPrintF(EncodingUtil.Utf8ToWin1251Bytes(messageString));
          break;
        case TmPrintLevel.Message:
          Native.MPrintF(EncodingUtil.Utf8ToWin1251Bytes(messageString));
          break;
        case TmPrintLevel.Error:
          Native.EPrintF(EncodingUtil.Utf8ToWin1251Bytes(messageString));
          break;
      }
    }


    public static long GetServerPseudoUnixTimestamp(long unixTimestamp)
    {
      return Native.UxGmTime2UxTime(unixTimestamp);
    }


    public static bool TryDownloadTaskConfiguration(int tmCid, string applicationName, int idx, out string path)
    {
      path = string.Empty;

      var remotePathPtr = Native.TmcGetKnownxCfgPath(tmCid, applicationName, (uint)idx);
      if (remotePathPtr == IntPtr.Zero)
      {
        return false;
      }
      var remotePath = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(remotePathPtr);

      var cfCid = Native.TmcGetCfsHandle(tmCid);
      if (cfCid == IntPtr.Zero)
      {
        return false;
      }

      var localPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(remotePath));
	  var fileTime = new TmNativeDefs.FileTime();
	  const int errStringLength = 1000;
      var       errString       = new byte[errStringLength];
      uint      errCode         = 0;

      if (!Native.CfsFileGet(cfCid, EncodingUtil.Utf8ToWin1251Bytes(remotePath), EncodingUtil.Utf8ToWin1251Bytes(localPath), 30000, ref fileTime,
                             out errCode, ref errString, errStringLength))
      {
        return false;
      }

      Native.TmcFreeMemory(remotePathPtr);

      path = localPath;
      return true;
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


    public static (int tmCid, IntPtr stopEventHandle) InitializeAsTaskSimple(TmOikTaskOptions    taskOptions,
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


    public static (IntPtr startEventHandle, IntPtr stopEventHandle) RegisterOikTaskProcess()
    {
      var taskArgs = Environment.GetCommandLineArgs();
      taskArgs[0] = Native.GetOikTaskExecutable(taskArgs[0]);

      var startEventHandle = new IntPtr();
      var stopEventHandle  = new IntPtr();
      Native.CfsPmonLocalRegisterProcess(taskArgs.Length,
                                         taskArgs,
                                         ref startEventHandle,
                                         ref stopEventHandle);
      Native.PlatformSetEvent(startEventHandle);

      return (startEventHandle, stopEventHandle);
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


    public static void ClearRelay(int tmCid)
    {
      Native.TmcClrRetransInfo(tmCid);
    }


    public static void SubscribeToRelay(int tmCid, IReadOnlyList<TmAddr> tmAddrList)
    {
      const int maxSize = 128;
      
      var totalCount = tmAddrList.Count;
      for (var i = 0; i < totalCount; i += maxSize)
      {
        var batchCount  = Math.Min(totalCount - i, maxSize);
        
        var ri = new TmNativeDefs.TRetransInfo[batchCount];
        for (var j = 0; j < batchCount; j++)
        {
          ri[j] = new TmNativeDefs.TRetransInfo
          {
            Id    = (uint)(i + j + 1),
            AdrTm = tmAddrList[i + j].ToAdrTm(),
            Type  = (ushort)tmAddrList[i + j].Type.ToNativeType(),
          };
        }
        Native.TmcSetRetransInfo(tmCid, (ushort)batchCount, ri);
      }
    }


    public static void MqttSubscribe(int tmCid, MqttKnownTopic knownTopic)
    {
      var topic = new MqttSubscriptionTopic(knownTopic);

      Native.TmcPubSubscribe(tmCid,
                             topic.Topic,
                             (uint)topic.SubscriptionId,
                             (byte)topic.QoS);
    }


    public static void MqttPublish(int tmCid, MqttKnownTopic knownTopic, string payload = "")
    {
      var topic = new MqttPublishTopic(knownTopic);
      var payloadBytes = string.IsNullOrWhiteSpace(payload) 
        ? Array.Empty<byte>() 
        : EncodingUtil.Utf8ToWin1251Bytes(payload);

      Native.TmcPubPublish(tmCid,
                           topic.Topic,
                           topic.LifetimeSec,
                           (byte)topic.QoS,
                           payloadBytes,
                           (uint)payloadBytes.Length);
    }
    
    
    public static MqttMessage ParseMqttDatagram(byte[] datagram)
    {
      var result = new MqttMessage();
      
      var (infoList, payload) = TmNativeUtil.SplitMqttMessageDatagram(datagram);

      result.Payload = payload;
      foreach (var pair in infoList)
      {
        switch (pair.Key)
        {
          case "tag":
            result.Topic = pair.Value;
            break;
          case "subsid":
            result.SubscriptionId = int.Parse(pair.Value);
            break;
          case "qos":
            result.QoS = (MqttQoS)byte.Parse(pair.Value);
            break;
          case "r":
            result.Retain = int.Parse(pair.Value) == 1;
            break;
          case "pf":
            // Pub flags
            break;
          case "usid":
            result.UserId = uint.Parse(pair.Value);
            break;
        }
      }
      return result;
    }

    public static bool AckMqttPublications(int tmCid, MqttMessage message)
    {
      return Native.TmcPubAck(tmCid, message.Topic,
                              (uint)message.SubscriptionId,
                              (byte)message.QoS,
                              message.UserId,
                              message.Payload,
                              (uint)message.Payload.Length);
    }
    
    public static TmCommandLineConfiguration ParseTmCommandLineArguments()
    {
      var config = new TmCommandLineConfiguration();

      var commandLineArguments = Environment.GetCommandLineArgs();
      if (commandLineArguments.Length < 2)
      {
        return config;
      }

      config.TmServer = FixServerName(commandLineArguments.ElementAt(1));

      foreach (var arg in commandLineArguments.Skip(2))
      {
        var parts = arg.Split('=');
        if (parts.Length < 2)
        {
          continue;
        }
        switch (parts[0].TrimStart('/', '-').ToLowerInvariant())
        {
          case "cfg":
            config.ConfigPath = FixConfigPath(parts[1].Replace('`', ' '));
            break;

          case "idx":
            if (int.TryParse(parts[1], out var configIndex))
            {
              config.ConfigIndex = configIndex;
            }
            break;

          case "host":
          case "h":
          case "mach":
            config.Host = parts[1];
            break;

          case "tm":
            config.TmServer = parts[1];
            break;

          case "rb":
            config.RbServer = parts[1];
            break;

          case "user":
          case "u":
            config.User = parts[1];
            break;

          case "password":
          case "p":
          case "pwd":
            config.Password = parts[1];
            break;
        }
      }
      return config;

      string FixServerName(string source)
      {
        return (source.Contains('$')) // может добавляться номер экземпляра после доллара
          ? source.Split('$')[0]
          : source;
      }

      string FixConfigPath(string source)
      {
        return source.Replace('`', ' '); // пробелы в пути передаются как тильда
      }
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


  public class TmCommandLineConfiguration
  {
    public string Host        { get; set; } = ".";
    public string TmServer    { get; set; } = "TMS";
    public string RbServer    { get; set; } = "RBS";
    public string User        { get; set; } = "";
    public string Password    { get; set; } = "";
    public string ConfigPath  { get; set; } = "";
    public int    ConfigIndex { get; set; } = 0;
  }
}