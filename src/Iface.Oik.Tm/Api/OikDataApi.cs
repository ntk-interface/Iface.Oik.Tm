using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api
{
  public class OikDataApi : IOikDataApi
  {
    private readonly ITmsApi              _tms;
    private readonly IOikSqlApi           _sql;
    private readonly ICommonServerService _serverService;
    private          TmUserInfo           _userInfo;

    public TmNativeCallback TmsCallbackDelegate      { get; }
    public TmNativeCallback EmptyTmsCallbackDelegate { get; } = delegate { };

    public event EventHandler TmEventsAcked   = delegate { };
    public event EventHandler UserInfoUpdated = delegate { };

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


    public void SetUserInfo(TmUserInfo userInfo)
    {
      _userInfo = userInfo;
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
      if (buf[0] == 'E' &&
          buf[1] == 'L' &&
          buf[2] == 'A')
      {
        TmEventsAcked?.Invoke(this, EventArgs.Empty);
      }
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


    public async Task<TmServerInfo> GetServerInfo(PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetServerInfo().ConfigureAwait(false);
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


    public async Task UpdateTechObjects(IReadOnlyList<TmTechObject> techObjects,
                                        PreferApi                   prefer = PreferApi.Auto)
    {
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


    public async Task<IReadOnlyCollection<TmEvent>> GetEventsArchive(TmEventFilter filter,
                                                                     PreferApi     prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == ApiSelection.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == ApiSelection.Sql)
      {
        return await _sql.GetArchEvents(filter).ConfigureAwait(false);
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
      var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
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


    public async Task<IReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr    addr,
                                                                         long      utcStartTime,
                                                                         int       count,
                                                                         int       step,
                                                                         int       retroNum = 0,
                                                                         PreferApi prefer   = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetAnalogRetro(addr, utcStartTime, count, step, retroNum).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr    addr,
                                                                         DateTime  startTime,
                                                                         DateTime  endTime,
                                                                         int       step     = 0,
                                                                         int       retroNum = 0,
                                                                         PreferApi prefer   = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetAnalogRetro(addr, startTime, endTime, step, retroNum).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr    addr,
                                                                         string    startTime,
                                                                         string    endTime,
                                                                         int       step     = 0,
                                                                         int       retroNum = 0,
                                                                         PreferApi prefer   = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetAnalogRetro(addr, startTime, endTime, step, retroNum).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                                                   long   utcStartTime,
                                                                                                   long   utcEndTime,
                                                                                                   PreferApi prefer =
                                                                                                     PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetImpulseArchiveInstant(addr, utcStartTime, utcEndTime).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr   addr,
                                                                                                   DateTime startTime,
                                                                                                   DateTime endTime,
                                                                                                   PreferApi prefer =
                                                                                                     PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetImpulseArchiveInstant(addr, startTime, endTime).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                                                   string startTime,
                                                                                                   string endTime,
                                                                                                   PreferApi prefer =
                                                                                                     PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetImpulseArchiveInstant(addr, startTime, endTime).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
                                                                                                   long   utcStartTime,
                                                                                                   long   utcEndTime,
                                                                                                   int    step = 0,
                                                                                                   PreferApi prefer =
                                                                                                     PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetImpulseArchiveAverage(addr, utcStartTime, utcEndTime, step).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr   addr,
                                                                                                   DateTime startTime,
                                                                                                   DateTime endTime,
                                                                                                   int      step = 0,
                                                                                                   PreferApi prefer =
                                                                                                     PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetImpulseArchiveAverage(addr, startTime, endTime, step).ConfigureAwait(false);
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


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
                                                                                                   string startTime,
                                                                                                   string endTime,
                                                                                                   int    step = 0,
                                                                                                   PreferApi prefer =
                                                                                                     PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.GetImpulseArchiveAverage(addr, startTime, endTime, step).ConfigureAwait(false);
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
        throw new NotImplementedException();
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
                                              PreferApi prefer = PreferApi.Auto)
    {
      var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == ApiSelection.Tms)
      {
        return await _tms.SetAnalogManually(analog, value).ConfigureAwait(false);
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


    private enum ApiSelection
    {
      None = 0,
      Tms  = 1,
      Sql  = 2,
    }
  }
}