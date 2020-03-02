using System;
using System.Collections.Generic;
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


    private DataApiChoice GetApiChoice(DataApiPreference userPreference,
                                       DataApiPreference defaultPreference,
                                       bool              isTmsImplemented,
                                       bool              isSqlImplemented)
    {
      if (!_serverService.IsConnected)
      {
        return DataApiChoice.None;
      }
      var prefer = (userPreference != DataApiPreference.Auto) ? userPreference : defaultPreference;
      if (prefer == DataApiPreference.Tms && isTmsImplemented)
      {
        if (IsTmsAllowed)
        {
          return DataApiChoice.Tms;
        }
        if (isSqlImplemented)
        {
          return DataApiChoice.Sql;
        }
      }
      if (prefer == DataApiPreference.Sql && isSqlImplemented)
      {
        if (IsSqlAllowed)
        {
          return DataApiChoice.Sql;
        }
        if (isTmsImplemented)
        {
          return DataApiChoice.Tms;
        }
      }
      throw new NotImplementedException();
    }


    public async Task<int> GetLastTmcError(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetLastTmcError().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return -1;
      }
    }


    public async Task<TmServerInfo> GetServerInfo(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetServerInfo().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<string> GetLastTmcErrorText(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetLastTmcErrorText().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<DateTime?> GetSystemTime(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetSystemTime().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetSystemTime().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<string> GetSystemTimeString(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetSystemTimeString().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetSystemTimeString().ConfigureAwait(false);
      }
      else
      {
        return string.Empty;
      }
    }


    public async Task<int> GetStatus(int               ch,
                                     int               rtu,
                                     int               point,
                                     DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetStatus(ch, rtu, point).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetStatus(ch, rtu, point).ConfigureAwait(false);
      }
      else
      {
        return -1;
      }
    }


    public async Task SetStatus(int               ch,
                                int               rtu,
                                int               point,
                                int               status,
                                DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.SetStatus(ch, rtu, point, status).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task<float> GetAnalog(int               ch,
                                       int               rtu,
                                       int               point,
                                       DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetAnalog(ch, rtu, point).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetAnalog(ch, rtu, point).ConfigureAwait(false);
      }
      else
      {
        return -1;
      }
    }


    public async Task SetAnalog(int               ch,
                                int               rtu,
                                int               point,
                                float             value,
                                DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.SetAnalog(ch, rtu, point, value).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task UpdateStatus(TmStatus          status,
                                   DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        await _tms.UpdateStatus(status).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        await _sql.UpdateStatus(status).ConfigureAwait(false);
      }
      else
      {
        status.IsInit = false;
      }
    }


    public async Task UpdateAnalog(TmAnalog          analog,
                                   DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        await _tms.UpdateAnalog(analog).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        await _sql.UpdateAnalog(analog).ConfigureAwait(false);
      }
      else
      {
        analog.IsInit = false;
      }
    }


    public async Task UpdateStatuses(IList<TmStatus>   statuses,
                                     DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        await _tms.UpdateStatuses(statuses).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        await _sql.UpdateStatuses(statuses).ConfigureAwait(false);
      }
      else
      {
        // todo IsInit = false;
      }
    }


    public async Task UpdateAnalogs(IList<TmAnalog>   analogs,
                                    DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        await _tms.UpdateAnalogs(analogs).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        await _sql.UpdateAnalogs(analogs).ConfigureAwait(false);
      }
      else
      {
        // todo IsInit = false;
      }
    }


    public async Task UpdateTagsPropertiesAndClassData(IEnumerable<TmTag> tags,
                                                       DataApiPreference  preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        await _tms.UpdateTagsPropertiesAndClassData(tags).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        await _sql.UpdateTagsPropertiesAndClassData(tags).ConfigureAwait(false);
      }
      else
      {
      }
    }


    public async Task UpdateTagPropertiesAndClassData(TmTag             tag,
                                                      DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        await _tms.UpdateTagPropertiesAndClassData(tag).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        await _sql.UpdateTagPropertiesAndClassData(tag).ConfigureAwait(false);
      }
      else
      {
      }
    }


    public async Task UpdateTechObjects(IList<TmTechObject> techObjects,
                                        DataApiPreference   preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.UpdateTechObjectsProperties(techObjects).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        // todo IsInit = false
      }
    }


    public async Task<IEnumerable<TmEvent>> GetEventsArchive(TmEventFilter     filter,
                                                             DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetArchEvents(filter).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<TmEventElix> GetCurrentEventsElix(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetCurrentEventsElix().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<(IEnumerable<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix,
                                                                            DataApiPreference preference =
                                                                              DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetCurrentEvents(elix).ConfigureAwait(false);
      }
      else
      {
        return (null, null);
      }
    }


    public async Task<bool> UpdateAckedEventsIfAny(IList<TmEvent>    tmEvents,
                                                   DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.UpdateAckedEventsIfAny(tmEvents).ConfigureAwait(false);
      }
      else
      {
        return false;
      }
    }


    public async Task<IEnumerable<TmChannel>> GetTmTreeChannels(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetTmTreeChannels().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetTmTreeChannels().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmRtu>> GetTmTreeRtus(int               channelId,
                                                        DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: true, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetTmTreeRtus(channelId).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetTmTreeRtus(channelId).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmStatus>> GetTmTreeStatuses(int               channelId,
                                                               int               rtuId,
                                                               DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetTmTreeStatuses(channelId, rtuId).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAnalog>> GetTmTreeAnalogs(int               channelId,
                                                              int               rtuId,
                                                              DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetTmTreeAnalogs(channelId, rtuId).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmClassStatus>> GetStatusesClasses(
      DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetStatusesClasses().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmClassAnalog>> GetAnalogsClasses(
      DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetAnalogsClasses().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAnalogRetro>> GetAnalogRetro(TmAddr            addr,
                                                                 long              utcStartTime,
                                                                 int               count,
                                                                 int               step,
                                                                 int               retroNum   = 0,
                                                                 DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetAnalogRetro(addr, utcStartTime, count, step, retroNum).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAnalogRetro>> GetAnalogRetro(TmAddr            addr,
                                                                 DateTime          startTime,
                                                                 DateTime          endTime,
                                                                 int               step       = 0,
                                                                 int               retroNum   = 0,
                                                                 DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetAnalogRetro(addr, startTime, endTime, step, retroNum).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAnalogRetro>> GetAnalogRetro(TmAddr            addr,
                                                                 string            startTime,
                                                                 string            endTime,
                                                                 int               step       = 0,
                                                                 int               retroNum   = 0,
                                                                 DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetAnalogRetro(addr, startTime, endTime, step, retroNum).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAnalogImpulseArchiveInstant>>
      GetImpulseArchiveInstant(TmAddr            addr,
                               long              utcStartTime,
                               long              utcEndTime,
                               DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetImpulseArchiveInstant(addr, utcStartTime, utcEndTime).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAnalogImpulseArchiveInstant>>
      GetImpulseArchiveInstant(TmAddr            addr,
                               DateTime          startTime,
                               DateTime          endTime,
                               DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetImpulseArchiveInstant(addr, startTime, endTime).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAnalogImpulseArchiveInstant>>
      GetImpulseArchiveInstant(TmAddr            addr,
                               string            startTime,
                               string            endTime,
                               DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetImpulseArchiveInstant(addr, startTime, endTime).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAnalogImpulseArchiveAverage>>
      GetImpulseArchiveAverage(TmAddr            addr,
                               long              utcStartTime,
                               long              utcEndTime,
                               int               step       = 0,
                               DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetImpulseArchiveAverage(addr, utcStartTime, utcEndTime, step).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAnalogImpulseArchiveAverage>>
      GetImpulseArchiveAverage(TmAddr            addr,
                               DateTime          startTime,
                               DateTime          endTime,
                               int               step       = 0,
                               DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetImpulseArchiveAverage(addr, startTime, endTime, step).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAnalogImpulseArchiveAverage>>
      GetImpulseArchiveAverage(TmAddr            addr,
                               string            startTime,
                               string            endTime,
                               int               step       = 0,
                               DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetImpulseArchiveAverage(addr, startTime, endTime, step).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmStatus>> GetPresentAps(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetPresentAps().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmStatus>> GetUnackedAps(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetUnackedAps().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmStatus>> GetAbnormalStatuses(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetAbnormalStatuses().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmStatus>> LookupStatuses(TmStatusFilter    filter,
                                                            DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.LookupStatuses(filter).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAnalog>> LookupAnalogs(TmAnalogFilter    filter,
                                                           DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.LookupAnalogs(filter).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAlarm>> GetPresentAlarms(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetPresentAlarms().ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<IEnumerable<TmAlarm>> GetAnalogAlarms(TmAnalog          analog,
                                                            DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.GetAnalogAlarms(analog).ConfigureAwait(false);
      }
      else
      {
        return null;
      }
    }


    public async Task<bool> HasPresentAps(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.HasPresentAps().ConfigureAwait(false);
      }
      else
      {
        return false;
      }
    }


    public async Task<bool> HasPresentAlarms(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Sql, isTmsImplemented: false, isSqlImplemented: true);
      if (api == DataApiChoice.Tms)
      {
        throw new NotImplementedException();
      }
      else if (api == DataApiChoice.Sql)
      {
        return await _sql.HasPresentAlarms().ConfigureAwait(false);
      }
      else
      {
        return false;
      }
    }


    public async Task<bool> AckTag(TmAddr addr, DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.AckTag(addr).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return false;
      }
    }


    public async Task AckAllStatuses(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.AckAllStatuses().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task AckAllAnalogs(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.AckAllAnalogs().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task<bool> AckStatus(TmStatus          status,
                                      DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.AckStatus(status).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return false;
      }
    }


    public async Task<bool> AckAnalog(TmAnalog          analog,
                                      DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.AckAnalog(analog).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return false;
      }
    }


    public async Task<bool> AckEvent(TmEvent tmEvent, DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.AckEvent(tmEvent).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return false;
      }
    }


    public async Task AddStringToEventLog(string            str,
                                          TmAddr            tmAddr     = null,
                                          DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.AddStringToEventLog(str, tmAddr).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
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
                                              DataApiPreference                   preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.SetTechObjectProperties(scheme, type, obj, properties).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
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
                                                DataApiPreference   preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.ClearTechObjectProperties(scheme, type, obj, properties).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task<(bool, IReadOnlyList<TmControlScriptCondition>)> CheckTelecontrolScript(
      TmStatus          status,
      DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.CheckTelecontrolScript(status).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return (false, null);
      }
    }


    public async Task<(bool, IReadOnlyList<TmControlScriptCondition>)> CheckTelecontrolScriptExplicitly(
      TmStatus          status,
      int               explicitNewStatus,
      DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.CheckTelecontrolScriptExplicitly(status, explicitNewStatus).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return (false, null);
      }
    }


    public async Task OverrideTelecontrolScript(DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.OverrideTelecontrolScript().ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task<TmTelecontrolResult> Telecontrol(TmStatus          status,
                                                       DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.Telecontrol(status).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<TmTelecontrolResult> TelecontrolExplicitly(TmStatus          status,
                                                                 int               explicitNewStatus,
                                                                 DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.TelecontrolExplicitly(status, explicitNewStatus).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<TmTelecontrolResult> TeleregulateByStepUp(TmAnalog          analog,
                                                                DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.TeleregulateByStepUp(analog).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<TmTelecontrolResult> TeleregulateByStepDown(TmAnalog          analog,
                                                                  DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.TeleregulateByStepDown(analog).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<TmTelecontrolResult> TeleregulateByCode(TmAnalog          analog,
                                                              int               code,
                                                              DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.TeleregulateByCode(analog, code).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog          analog,
                                                               float             value,
                                                               DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.TeleregulateByValue(analog, value).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
    }


    public async Task<bool> SwitchStatusManually(TmStatus          status,
                                                 bool              alsoBlockManually = false,
                                                 DataApiPreference preference        = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.SwitchStatusManually(status, alsoBlockManually).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return false;
      }
    }


    public async Task SetStatusNormalOn(TmStatus          status,
                                        DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.SetStatusNormalOn(status).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task SetStatusNormalOff(TmStatus          status,
                                         DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.SetStatusNormalOff(status).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task ClearStatusNormal(TmStatus          status,
                                        DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.ClearStatusNormal(status).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task<int> GetStatusNormal(TmStatus          status,
                                           DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetStatusNormal(status).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return -1;
      }
    }


    public async Task<bool> SetAnalogManually(TmAnalog          analog,
                                              float             value,
                                              DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.SetAnalogManually(analog, value).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return false;
      }
    }


    public async Task<bool> SetAlarmValue(TmAlarm           alarm,
                                          float             value,
                                          DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.SetAlarmValue(alarm, value).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return false;
      }
    }


    public async Task SetTagFlags(TmTag             tag,
                                  TmFlags           flags,
                                  DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.SetTagFlags(tag, flags).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task ClearTagFlags(TmTag             tag,
                                    TmFlags           flags,
                                    DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        await _tms.ClearTagFlags(tag, flags).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
      }
    }


    public async Task<IEnumerable<string>> GetFilesInDirectory(string            path,
                                                               DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.GetFilesInDirectory(path).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return null;
      }
    }


    public async Task<bool> DownloadFile(string            remotePath,
                                         string            localPath,
                                         DataApiPreference preference = DataApiPreference.Auto)
    {
      var api = GetApiChoice(preference, DataApiPreference.Tms, isTmsImplemented: true, isSqlImplemented: false);
      if (api == DataApiChoice.Tms)
      {
        return await _tms.DownloadFile(remotePath, localPath).ConfigureAwait(false);
      }
      else if (api == DataApiChoice.Sql)
      {
        throw new NotImplementedException();
      }
      else
      {
        return false;
      }
    }


    private enum DataApiChoice
    {
      None = 0,
      Tms  = 1,
      Sql  = 2,
    }
  }
}