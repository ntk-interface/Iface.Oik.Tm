using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Api
{
  public partial class OikDataApi : IOikDataApi
  {
    private readonly ITmsApi              _tms;
    private readonly IOikSqlApi           _sql;
    private readonly ICommonServerService _serverService;
    private          TmServerFeatures     _serverFeatures;

    public TmNativeCallback TmsCallbackDelegate      { get; }
    public TmNativeCallback EmptyTmsCallbackDelegate { get; } = delegate { };

    public event EventHandler                   TmStatusChanged     = delegate { };
    public event EventHandler                   UserInfoUpdated     = delegate { };
    public event EventHandler                   TmEventsAcked       = delegate { };
    public event EventHandler<TobEventArgs>     TobChanged          = delegate { };
    public event EventHandler<TmAlertEventArgs> TmAlertsChanged     = delegate { };
    public event EventHandler<MqttMessage>      MqttMessageReceived = delegate { };

    public bool IsTmsAllowed { get; set; } = true;
    public bool IsSqlAllowed { get; set; } = true;


    public OikDataApi(ITmsApi              tms,
                      IOikSqlApi           sql,
                      ICommonServerService serverService)
    {
      _tms           = tms;
      _sql           = sql;
      _serverService = serverService;

      TmsCallbackDelegate = OnTmsCallback;
    }


    public void SetServerFeatures(TmServerFeatures features)
    {
      _serverFeatures = features;
      
      UserInfoUpdated?.Invoke(this, EventArgs.Empty);
    }


    private void OnTmsCallback(int callbackSize, IntPtr callbackBuf, IntPtr callbackParam)
    {
      var buf = new byte[callbackSize];
      Marshal.Copy(callbackBuf, buf, 0, callbackSize);
      HandleTmsCallback(buf);
    }


    private void HandleTmsCallback(byte[] buf)
    {
      if (buf[0] == 'T' &&
          buf[1] == 'S')
      {
        HandleTmsCallbackTmStatusChanged();
      }
      else if (buf[0] == 'E' &&
               buf[1] == 'L' &&
               buf[2] == 'A')
      {
        HandleTmsCallbackEventsAcked();
      }
      else if (buf[0] == 'A' &&
               buf[1] == 'L' &&
               buf[2] == 'R' &&
               buf[3] == 'T')
      {
        HandleTmsCallbackAlerts(buf.ElementAtOrDefault(4),
                                buf.ElementAtOrDefault(5));
      }
      else if (buf[0] == 'T' &&
               buf[1] == 'O' &&
               buf[2] == 'B')
      {
        HandleTmsCallbackTob(buf.ElementAtOrDefault(3));
      }
      else if (buf[0] == 'p' &&
               buf[1] == 'o')
      {
        HandleTmsCallbackMqtt(buf);
      }
    }


    private void HandleTmsCallbackTmStatusChanged()
    {
      TmStatusChanged.Invoke(this, EventArgs.Empty);
    }


    private void HandleTmsCallbackEventsAcked()
    {
      TmEventsAcked.Invoke(this, EventArgs.Empty);
    }


    private void HandleTmsCallbackAlerts(byte reason, byte importance)
    {
      TmAlertsChanged.Invoke(this, new TmAlertEventArgs
      {
        Reason = (char)reason switch
                 {
                   'a' => TmAlertEventReason.Added,
                   'r' => TmAlertEventReason.Removed,
                   _   => TmAlertEventReason.Unknown
                 },
        
        Importance = (char)importance switch
                     {
                       '0' => TmEventImportances.Imp0,
                       '1' => TmEventImportances.Imp1,
                       '2' => TmEventImportances.Imp2,
                       '3' => TmEventImportances.Imp3,
                       _   => TmEventImportances.None
                     }
      });
    }


    private void HandleTmsCallbackTob(byte reason)
    {
      var eventArgs = new TobEventArgs();

      switch ((char) reason)
      {
        case '$':
          eventArgs.Reason = TobEventReason.Topology;
          break;

        case '^':
          eventArgs.Reason = TobEventReason.Placards;
          break;

        case (char) 0xFF:
          eventArgs.Reason = TobEventReason.Global;
          break;

        default:
          return;
      }
      TobChanged.Invoke(this, eventArgs);
    }


    private void HandleTmsCallbackMqtt(byte[] datagram)
    {
      var message = Tms.ParseMqttDatagram(datagram);
      MqttMessageReceived.Invoke(this, message);
      _tms.NotifyOfMqttMessage(message);
    }


    private ApiSelection SelectApi(PreferApi userPreference,
                                   PreferApi defaultPreference,
                                   bool      isTmsImplemented,
                                   bool      isSqlImplemented)
    {
      if (!_serverService.IsConnected)
      {
        return ApiSelection.None;
      }
      var prefer = (userPreference != PreferApi.Auto) ? userPreference : defaultPreference;
      if (prefer == PreferApi.Tms && isTmsImplemented)
      {
        if (IsTmsAllowed)
        {
          return ApiSelection.Tms;
        }
        if (isSqlImplemented)
        {
          return ApiSelection.Sql;
        }
      }
      if (prefer == PreferApi.Sql && isSqlImplemented)
      {
        if (IsSqlAllowed)
        {
          return ApiSelection.Sql;
        }
        if (isTmsImplemented)
        {
          return ApiSelection.Tms;
        }
      }
      throw new NotImplementedException();
    }


    private T ExecuteSync<T>(PreferApi userPreference,
                             PreferApi defaultPreference,
                             Func<T>   tmsAction,
                             Func<T>   sqlAction,
                             Func<T>   errorAction = null)
    {
      var api = SelectApi(userPreference, defaultPreference, tmsAction != null, sqlAction != null);

      if (api == ApiSelection.Tms && tmsAction != null)
      {
        return tmsAction();
      }
      if (api == ApiSelection.Sql && sqlAction != null)
      {
        return sqlAction();
      }

      if (errorAction != null)
      {
        return errorAction();
      }
      return default; // TODO throw exception
    }


    private async Task<T> Execute<T>(PreferApi     userPreference,
                                     PreferApi     defaultPreference,
                                     Func<Task<T>> tmsAction,
                                     Func<Task<T>> sqlAction,
                                     Func<T>       errorAction = null)
    {
      var api = SelectApi(userPreference, defaultPreference, tmsAction != null, sqlAction != null);

      if (api == ApiSelection.Tms && tmsAction != null)
      {
        return await tmsAction().ConfigureAwait(false);
      }
      if (api == ApiSelection.Sql && sqlAction != null)
      {
        return await sqlAction().ConfigureAwait(false);
      }
      
      if (errorAction != null)
      {
        return errorAction();
      }
      return default; // TODO throw exception
    }


    private async Task Execute(PreferApi  userPreference,
                               PreferApi  defaultPreference,
                               Func<Task> tmsAction,
                               Func<Task> sqlAction,
                               Action     errorAction = null)
    {
      var api = SelectApi(userPreference, defaultPreference, tmsAction != null, sqlAction != null);

      if (api == ApiSelection.Tms && tmsAction != null)
      {
        await tmsAction().ConfigureAwait(false);
      }
      if (api == ApiSelection.Sql && sqlAction != null)
      {
        await sqlAction().ConfigureAwait(false);
      }
      if (errorAction != null)
      {
        errorAction();
      }
      // TODO throw exception
    }


    public async Task<int> GetLastTmcError(PreferApi prefer = PreferApi.Auto)
    {
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.GetLastTmcError(),
                           null,
                           () => -1)
              .ConfigureAwait(false);
    }


    public async Task<TmServerComputerInfo> GetServerComputerInfo(PreferApi prefer = PreferApi.Auto)
    {
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.GetServerComputerInfo(),
                           null)
              .ConfigureAwait(false);
    }


    public async Task<string> GetLastTmcErrorText(PreferApi prefer = PreferApi.Auto)
    {
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.GetLastTmcErrorText(),
                           null)
              .ConfigureAwait(false);
    }


    public async Task<DateTime?> GetSystemTime(PreferApi prefer = PreferApi.Auto)
    {
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.GetSystemTime(),
                           () => _sql.GetSystemTime())
              .ConfigureAwait(false);
    }


    public async Task<string> GetSystemTimeString(PreferApi prefer = PreferApi.Auto)
    {
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.GetSystemTimeString(),
                           () => _sql.GetSystemTimeString())
              .ConfigureAwait(false);
    }


    public async Task<(string host, string server)> GetCurrentServerName(PreferApi prefer = PreferApi.Auto)
    {
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.GetCurrentServerName(),
                           null)
              .ConfigureAwait(false);
    }
    
    
    public async Task<(string user, string password)> GenerateTokenForExternalApp(PreferApi prefer = PreferApi.Auto)
    {
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.GenerateTokenForExternalApp(),
                           null)
              .ConfigureAwait(false);
    }
    
    
    public async Task<string> GetExpressionResult(string    expression,
                                                  PreferApi prefer = PreferApi.Auto)
    {
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.GetExpressionResult(expression),
                           null)
              .ConfigureAwait(false);
    }
    
    
    public string GetExpressionResultSync(string    expression,
                                          PreferApi prefer = PreferApi.Auto)
    {
      return ExecuteSync(prefer,
                         PreferApi.Tms,
                         () => _tms.GetExpressionResultSync(expression),
                         null);
    }


    public async Task<IReadOnlyCollection<string>> GetFilesInDirectory(string    path,
                                                                       PreferApi prefer = PreferApi.Auto)
    {
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.GetFilesInDirectory(path),
                           null)
              .ConfigureAwait(false);
    }


    public async Task<bool> DownloadFile(string    remotePath,
                                         string    localPath,
                                         PreferApi prefer = PreferApi.Auto)
    {
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.DownloadFile(remotePath, localPath),
                           null)
              .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<string>> GetComtradeDays(PreferApi prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.IsComtradeEnabled)
      {
        return null;
      }
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.GetComtradeDays(),
                           null)
              .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<string>> GetComtradeFilesByDay(string    day,
                                                                         PreferApi prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.IsComtradeEnabled)
      {
        return null;
      }
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.GetComtradeFilesByDay(day),
                           null)
              .ConfigureAwait(false);
    }


    public async Task<bool> DownloadComtradeFile(string    filename,
                                                 string    localPath,
                                                 PreferApi prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.IsComtradeEnabled)
      {
        return false;
      }
      return await Execute(prefer,
                           PreferApi.Tms,
                           () => _tms.DownloadComtradeFile(filename, localPath),
                           null)
              .ConfigureAwait(false);
    }


    private enum ApiSelection
    {
      None = 0,
      Tms  = 1,
      Sql  = 2,
    }
  }
}