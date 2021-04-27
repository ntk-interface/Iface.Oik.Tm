using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Api
{
  public class OikDataApi : IOikDataApi
  {
    private readonly ITmsApi              _tms;
    private readonly IOikSqlApi           _sql;
    private readonly ICommonServerService _serverService;
    private          TmUserInfo           _userInfo;
    private          TmServerFeatures     _serverFeatures;

    public TmNativeCallback TmsCallbackDelegate      { get; }
    public TmNativeCallback EmptyTmsCallbackDelegate { get; } = delegate { };

    public event EventHandler                   UserInfoUpdated = delegate { };
    public event EventHandler                   TmEventsAcked   = delegate { };
    public event EventHandler<TobEventArgs>     TobChanged      = delegate { };
    public event EventHandler<TmAlertEventArgs> TmAlertsChanged = delegate { };

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
      if (buf[0] == 'E' &&
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


    public async Task<int> GetStatus(int       ch,
                                     int       rtu,
                                     int       point,
                                     PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetStatus(ch, rtu, point).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetStatus(ch, rtu, point).ConfigureAwait(false);
      }
      else
      {
        return -1;
      }
    }


    public async Task SetStatus(int       ch,
                                int       rtu,
                                int       point,
                                int       status,
                                PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.SetStatus(ch, rtu, point, status).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task<float> GetAnalog(int       ch,
                                       int       rtu,
                                       int       point,
                                       PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetAnalog(ch, rtu, point).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetAnalog(ch, rtu, point).ConfigureAwait(false);
      }
      else
      {
        return -1;
      }
    }


    public async Task SetAnalog(int       ch,
                                int       rtu,
                                int       point,
                                float     value,
                                PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.SetAnalog(ch, rtu, point, value).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task UpdateStatus(TmStatus  status,
                                   PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        await _tms.UpdateStatus(status).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        await _sql.UpdateStatus(status).ConfigureAwait(false);
      }
      else
      {
        status.IsInit = false;
      }
    }


    public async Task UpdateAnalog(TmAnalog  analog,
                                   PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        await _tms.UpdateAnalog(analog).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        await _sql.UpdateAnalog(analog).ConfigureAwait(false);
      }
      else
      {
        analog.IsInit = false;
      }
    }


    public async Task UpdateStatuses(IReadOnlyList<TmStatus> statuses,
                                     PreferApi               prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        await _tms.UpdateStatuses(statuses).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        await _sql.UpdateStatuses(statuses).ConfigureAwait(false);
      }
      else
      {
        // todo IsInit = false;
      }
    }


    public async Task UpdateAnalogs(IReadOnlyList<TmAnalog> analogs,
                                    PreferApi               prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        await _tms.UpdateAnalogs(analogs).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        await _sql.UpdateAnalogs(analogs).ConfigureAwait(false);
      }
      else
      {
        // todo IsInit = false;
      }
    }


    public async Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tags,
                                                       PreferApi            prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        await _tms.UpdateTagsPropertiesAndClassData(tags).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        await _sql.UpdateTagsPropertiesAndClassData(tags).ConfigureAwait(false);
      }
      else
      {
      }
    }


    public async Task UpdateTagPropertiesAndClassData(TmTag     tag,
                                                      PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        await _tms.UpdateTagPropertiesAndClassData(tag).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        await _sql.UpdateTagPropertiesAndClassData(tag).ConfigureAwait(false);
      }
      else
      {
      }
    }


    public async Task UpdateTechObjects(IReadOnlyList<Tob> techObjects,
                                        PreferApi          prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.AreTechObjectsEnabled)
      {
        return;
      }
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.UpdateTechObjectsProperties(techObjects).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        // todo IsInit = false
      }
    }


    public async Task<IReadOnlyCollection<Tob>> GetTechObjects(TobFilter filter,
                                                               PreferApi prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.AreTechObjectsEnabled)
      {
        return null;
      }
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetTechObjects(filter).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmEvent>> GetEventsArchive(TmEventFilter filter,
                                                                     PreferApi     prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetEventsArchive(filter).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetEventsArchive(filter).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<TmEventElix> GetCurrentEventsElix(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetCurrentEventsElix().ConfigureAwait(false);
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


    public async Task<(IReadOnlyCollection<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix,
                                                                                    PreferApi   prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetCurrentEvents(elix).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetCurrentEvents(elix).ConfigureAwait(false);
      }
      else
      {
        return (null, null);
      }
    }


    public async Task<bool> UpdateAckedEventsIfAny(IReadOnlyList<TmEvent> tmEvents,
                                                   PreferApi              prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.UpdateAckedEventsIfAny(tmEvents).ConfigureAwait(false);
      }
      else
      {
        return false;
      }
    }


    public async Task<IReadOnlyCollection<TmChannel>> GetTmTreeChannels(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetTmTreeChannels().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetTmTreeChannels().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmRtu>> GetTmTreeRtus(int       channelId,
                                                                PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetTmTreeRtus(channelId).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetTmTreeRtus(channelId).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmStatus>> GetTmTreeStatuses(int       channelId,
                                                                       int       rtuId,
                                                                       PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetTmTreeStatuses(channelId, rtuId).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetTmTreeStatuses(channelId, rtuId).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmAnalog>> GetTmTreeAnalogs(int       channelId,
                                                                      int       rtuId,
                                                                      PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetTmTreeAnalogs(channelId, rtuId).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetTmTreeAnalogs(channelId, rtuId).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<string> GetChannelName(int       channelId,
                                             PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetChannelName(channelId).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetChannelName(channelId).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<string> GetRtuName(int       channelId,
                                         int       rtuId,
                                         PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetRtuName(channelId, rtuId).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetRtuName(channelId, rtuId).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmClassStatus>> GetStatusesClasses(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetStatusesClasses().ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmClassAnalog>> GetAnalogsClasses(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetAnalogsClasses().ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<ITmAnalogRetro[]>> GetAnalogsMicroSeries(
      IReadOnlyList<TmAnalog> analogs,
      PreferApi               prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.AreMicroSeriesEnabled)
      {
        return null;
      }
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetAnalogsMicroSeries(analogs).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetAnalogsMicroSeries(analogs).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmRetroInfo>> GetRetrosInfo(TmType    tmType,
                                                                      PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      { 
        return await _tms.GetRetrosInfo(tmType).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetro(TmAnalog  analog,
                                                                          long      utcStartTime,
                                                                          int       count,
                                                                          int       step,
                                                                          int       retroNum = 0,
                                                                          PreferApi prefer   = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetAnalogRetro(analog, utcStartTime, count, step, retroNum).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetro(TmAnalog            analog,
                                                                          TmAnalogRetroFilter filter,
                                                                          int                 retroNum = 0,
                                                                          PreferApi           prefer   = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetAnalogRetro(analog, filter, retroNum).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveInstant(
      TmAnalog            analog,
      TmAnalogRetroFilter filter,
      PreferApi           prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.IsImpulseArchiveEnabled)
      {
        return null;
      }
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetImpulseArchiveInstant(analog, filter).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveAverage(
      TmAnalog            analog,
      TmAnalogRetroFilter filter,
      PreferApi           prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.IsImpulseArchiveEnabled)
      {
        return null;
      }
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetImpulseArchiveAverage(analog, filter).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmStatus>> GetPresentAps(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetPresentAps().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetPresentAps().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmStatus>> GetUnackedAps(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetUnackedAps().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmStatus>> GetAbnormalStatuses(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetAbnormalStatuses().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmAlert>> GetAlerts(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetAlerts().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmAlert>> GetAlertsWithAnalogMicroSeries(PreferApi prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.AreMicroSeriesEnabled)
      {
        return await GetAlerts(prefer).ConfigureAwait(false);
      }
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetAlertsWithAnalogMicroSeries().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<bool> RemoveAlert(TmAlert alert, PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.RemoveAlert(alert).ConfigureAwait(false);
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


    public async Task<bool> RemoveAlerts(IEnumerable<TmAlert> alerts, PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.RemoveAlerts(alerts).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmAlarm>> GetPresentAlarms(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetPresentAlarms().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmAlarm>> GetAnalogAlarms(TmAnalog  analog,
                                                                    PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetAnalogAlarms(analog).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmStatus>> LookupStatuses(TmStatusFilter filter,
                                                                    PreferApi      prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.LookupStatuses(filter).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IReadOnlyCollection<TmAnalog>> LookupAnalogs(TmAnalogFilter filter,
                                                                   PreferApi      prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.LookupAnalogs(filter).ConfigureAwait(false);
      }
      else
      {
        return null;
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


    public async Task<bool> HasPresentAps(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.HasPresentAps().ConfigureAwait(false);
      }
      else
      {
        return false;
      }
    }


    public async Task<bool> HasPresentAlarms(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.HasPresentAlarms().ConfigureAwait(false);
      }
      else
      {
        return false;
      }
    }


    public async Task<bool> AckTag(TmAddr    addr,
                                   PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.AckTag(addr).ConfigureAwait(false);
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


    public async Task AckAllStatuses(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.AckAllStatuses().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task AckAllAnalogs(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.AckAllAnalogs().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task<bool> AckStatus(TmStatus  status,
                                      PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.AckStatus(status).ConfigureAwait(false);
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


    public async Task<bool> AckAnalog(TmAnalog  analog,
                                      PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.AckAnalog(analog).ConfigureAwait(false);
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


    public async Task<bool> AckEvent(TmEvent   tmEvent,
                                     PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.AckEvent(tmEvent).ConfigureAwait(false);
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


    public async Task AddStringToEventLog(string    str,
                                          TmAddr    tmAddr = null,
                                          PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.AddStringToEventLog(str, tmAddr).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task SetTechObjectProperties(int                                 scheme,
                                              int                                 type,
                                              int                                 obj,
                                              IReadOnlyDictionary<string, string> properties,
                                              PreferApi                           prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.AreTechObjectsEnabled)
      {
        return;
      }
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.SetTechObjectProperties(scheme, type, obj, properties).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task ClearTechObjectProperties(int                 scheme,
                                                int                 type,
                                                int                 obj,
                                                IEnumerable<string> properties,
                                                PreferApi           prefer = PreferApi.Auto)
    {
      if (!_serverFeatures.AreTechObjectsEnabled)
      {
        return;
      }
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.ClearTechObjectProperties(scheme, type, obj, properties).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScript(
      TmStatus  status,
      PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.CheckTelecontrolScript(status).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return (false, null);
      }
    }


    public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScriptExplicitly(
      TmStatus  status,
      int       explicitNewStatus,
      PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.CheckTelecontrolScriptExplicitly(status, explicitNewStatus).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return (false, null);
      }
    }


    public async Task OverrideTelecontrolScript(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.OverrideTelecontrolScript().ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task<TmTelecontrolResult> Telecontrol(TmStatus  status,
                                                       PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.Telecontrol(status).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<TmTelecontrolResult> TelecontrolExplicitly(TmStatus  status,
                                                                 int       explicitNewStatus,
                                                                 PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.TelecontrolExplicitly(status, explicitNewStatus).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<TmTelecontrolResult> TeleregulateByStepUp(TmAnalog  analog,
                                                                PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.TeleregulateByStepUp(analog).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<TmTelecontrolResult> TeleregulateByStepDown(TmAnalog  analog,
                                                                  PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.TeleregulateByStepDown(analog).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<TmTelecontrolResult> TeleregulateByCode(TmAnalog  analog,
                                                              int       code,
                                                              PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.TeleregulateByCode(analog, code).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog  analog,
                                                               float     value,
                                                               PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.TeleregulateByValue(analog, value).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<bool> SwitchStatusManually(TmStatus  status,
                                                 bool      alsoBlockManually = false,
                                                 PreferApi prefer            = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.SwitchStatusManually(status, alsoBlockManually).ConfigureAwait(false);
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


    public async Task SetStatusNormalOn(TmStatus  status,
                                        PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.SetStatusNormalOn(status).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task SetStatusNormalOff(TmStatus  status,
                                         PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.SetStatusNormalOff(status).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task ClearStatusNormal(TmStatus  status,
                                        PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.ClearStatusNormal(status).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task<int> GetStatusNormal(TmStatus  status,
                                           PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetStatusNormal(status).ConfigureAwait(false);
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


    public async Task<bool> SetAnalogManually(TmAnalog  analog,
                                              float     value,
                                              bool      alsoBlockManually = false,
                                              PreferApi prefer            = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.SetAnalogManually(analog, value, alsoBlockManually).ConfigureAwait(false);
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


    public async Task<bool> SetAnalogTechParameters(TmAnalog  analog, TmAnalogTechParameters parameters,
                                                    PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.SetAnalogTechParameters(analog, parameters).ConfigureAwait(false);
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


    public async Task<bool> SetAlarmValue(TmAlarm   alarm,
                                          float     value,
                                          PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.SetAlarmValue(alarm, value).ConfigureAwait(false);
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


    public async Task SetTagFlags(TmTag     tag,
                                  TmFlags   flags,
                                  PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.SetTagFlags(tag, flags).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task ClearTagFlags(TmTag     tag,
                                    TmFlags   flags,
                                    PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.ClearTagFlags(tag, flags).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task SetTagsFlags(IEnumerable<TmTag> tags,
                                   TmFlags            flags,
                                   PreferApi          prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.SetTagsFlags(tags, flags).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task ClearTagsFlags(IEnumerable<TmTag> tags,
                                     TmFlags            flags,
                                     PreferApi          prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        await _tms.ClearTagsFlags(tags, flags).ConfigureAwait(false);
      }
      else if (api == ApiSelection.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
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