﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iface.Oik.Tm.Interfaces
{
  public interface IOikSqlApi
  {
    void SetCreateOikSqlConnection(Func<ICommonOikSqlConnection> createOikSqlConnection);

    Task<DateTime?> GetSystemTime();

    Task<string> GetSystemTimeString();

    Task<int> GetStatus(int ch, int rtu, int point);

    Task<float> GetAnalog(int ch, int rtu, int point);

    Task<float> GetAccum(int ch, int rtu, int point);

    Task<float> GetAccumLoad(int ch, int rtu, int point);

    Task UpdateStatus(TmStatus status);

    Task UpdateAnalog(TmAnalog analog);

    Task UpdateAccum(TmAccum accum);

    Task UpdateStatuses(IReadOnlyList<TmStatus> statuses);

    Task UpdateAnalogs(IReadOnlyList<TmAnalog> analogs);

    Task UpdateAccums(IReadOnlyList<TmAccum> accums);

    Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tags);

    Task UpdateTagPropertiesAndClassData(TmTag tag);
    
    Task UpdateTechObjectsProperties(IReadOnlyList<Tob> techObjects);

    Task<IReadOnlyCollection<ITmAnalogRetro[]>> GetAnalogsMicroSeries(IReadOnlyList<TmAnalog> analogs);

    Task<IReadOnlyCollection<TmEvent>> GetEventsArchive(TmEventFilter filter);

    Task<IReadOnlyCollection<TmUserAction>> GetUserActionsArchive(TmEventFilter filter);

    Task<(IReadOnlyCollection<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix);

    Task<IReadOnlyCollection<TmChannel>> GetTmTreeChannels();

    Task<IReadOnlyCollection<TmRtu>> GetTmTreeRtus(int channelId);

    Task<IReadOnlyCollection<TmStatus>> GetTmTreeStatuses(int channelId, int rtuId);

    Task<IReadOnlyCollection<TmAnalog>> GetTmTreeAnalogs(int channelId, int rtuId);

    Task<IReadOnlyCollection<TmAccum>> GetTmTreeAccums(int channelId, int rtuId);

    Task<string> GetChannelName(int channelId);

    Task<string> GetRtuName(int channelId, int rtuId);

    Task<IReadOnlyCollection<TmStatus>> GetPresentAps();

    Task<IReadOnlyCollection<TmStatus>> GetUnackedAps();

    Task<IReadOnlyCollection<TmStatus>> GetAbnormalStatuses();

    Task<IReadOnlyCollection<TmAlert>> GetAlerts();
    
    Task<IReadOnlyCollection<TmAlert>> GetAlertsWithAnalogMicroSeries();

    Task<IReadOnlyCollection<TmAlarm>> GetPresentAlarms();

    Task<IReadOnlyCollection<TmAlarm>> GetAnalogAlarms(TmAnalog analog);

    Task<IReadOnlyCollection<TmStatus>> LookupStatuses(TmStatusFilter filter);

    Task<IReadOnlyCollection<TmAnalog>> LookupAnalogs(TmAnalogFilter filter);

    Task<IReadOnlyCollection<TmTag>> GetTagsWithBlockedEvents();

    Task<bool> UpdateAckedEventsIfAny(IReadOnlyList<TmEvent> tmEvents);

    Task<bool> HasPresentAps();

    Task<bool> HasPresentAlarms();
  }
}