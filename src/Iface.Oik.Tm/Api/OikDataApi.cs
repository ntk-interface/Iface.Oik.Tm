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
    private          TmUserInfo           _userInfo;
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


    public void SetUserInfoAndServerFeatures(TmUserInfo userInfo, TmServerFeatures features)
    {
      _userInfo = userInfo;
      UserInfoUpdated?.Invoke(this, EventArgs.Empty);

      _serverFeatures = features;
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
      var eventArgs = new TmAlertEventArgs();

      switch ((char) reason)
      {
        case 'a':
          eventArgs.Reason = TmAlertEventReason.Added;
          break;

        case 'r':
          eventArgs.Reason = TmAlertEventReason.Removed;
          break;

        default:
          eventArgs.Reason = TmAlertEventReason.Unknown;
          break;
      }

      switch ((char) importance)
      {
        case '0':
          eventArgs.Importance = TmEventImportances.Imp0;
          break;

        case '1':
          eventArgs.Importance = TmEventImportances.Imp1;
          break;

        case '2':
          eventArgs.Importance = TmEventImportances.Imp2;
          break;

        case '3':
          eventArgs.Importance = TmEventImportances.Imp3;
          break;

        default:
          eventArgs.Importance = TmEventImportances.None;
          break;
      }

      TmAlertsChanged.Invoke(this, eventArgs);
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


    public async Task<int> GetLastTmcError(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetLastTmcError().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return -1;
      }
    }


    public async Task<TmServerComputerInfo> GetServerComputerInfo(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetServerComputerInfo().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<string> GetLastTmcErrorText(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetLastTmcErrorText().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<DateTime?> GetSystemTime(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetSystemTime().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetSystemTime().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<string> GetSystemTimeString(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetSystemTimeString().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetSystemTimeString().ConfigureAwait(false);
      }
      else
      {
        return string.Empty;
      }
    }


    public async Task<(string host, string server)> GetCurrentServerName(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetCurrentServerName().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return (null, null);
      }
    }


    public async Task<(string user, string password)> GenerateTokenForExternalApp(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GenerateTokenForExternalApp().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return (null, null);
      }
    }


    public async Task<string> GetExpressionResult(string    expression,
                                                  PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetExpressionResult(expression).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public string GetExpressionResultSync(string    expression,
                                          PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return _tms.GetExpressionResultSync(expression);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<string>> GetFilesInDirectory(string    path,
                                                                       PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetFilesInDirectory(path).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<bool> DownloadFile(string    remotePath,
                                         string    localPath,
                                         PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.DownloadFile(remotePath, localPath).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return false;
      }
    }


    public async Task<IReadOnlyCollection<string>> GetComtradeDays(PreferApi prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.IsComtradeEnabled)
      {
        return null;
      }
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetComtradeDays().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<string>> GetComtradeFilesByDay(string    day,
                                                                         PreferApi prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.IsComtradeEnabled)
      {
        return null;
      }
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetComtradeFilesByDay(day).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<bool> DownloadComtradeFile(string    filename,
                                                 string    localPath,
                                                 PreferApi prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.IsComtradeEnabled)
      {
        return false;
      }
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.DownloadComtradeFile(filename, localPath).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return false;
      }
    }


    private enum ApiSelection
    {
      None = 0,
      Tms  = 1,
      Sql  = 2,
    }
  }
}