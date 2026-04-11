using System;
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

    Task UpdateStatus(TmStatus tmStatus);

    Task UpdateAnalog(TmAnalog tmAnalog);

    Task UpdateAccum(TmAccum tmAccum);

    Task UpdateStatuses(IReadOnlyList<TmStatus> tmStatuses);

    Task UpdateStatuses(IReadOnlyDictionary<int, TmStatus> tmStatuses);

    Task UpdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs);

    Task UpdateAnalogs(IReadOnlyDictionary<int, TmAnalog> tmAnalogs);

    Task UpdateAccums(IReadOnlyList<TmAccum> tmAccums);

    Task UpdateAccums(IReadOnlyDictionary<int, TmAccum> tmAccums);

    Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tmTags);

    Task UpdateStatusesPropertiesAndClassData(IReadOnlyDictionary<int, TmStatus> tmStatuses);

    Task UpdateAnalogsPropertiesAndClassData(IReadOnlyDictionary<int, TmAnalog> tmAnalogs);

    Task UpdateAccumsPropertiesAndClassData(IReadOnlyDictionary<int, TmAccum> tmAccums);

    Task UpdateTagPropertiesAndClassData(TmTag tmTag);

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

    Task<IReadOnlyCollection<TmAlarm>> GetAnalogAlarms(TmAnalog tmAnalog);

    Task<IReadOnlyCollection<TmStatus>> LookupStatuses(TmStatusFilter filter);

    Task<IReadOnlyCollection<TmAnalog>> LookupAnalogs(TmAnalogFilter filter);

    Task<IReadOnlyCollection<TmTag>> GetTagsWithBlockedEvents();

    Task<bool> UpdateAckedEventsIfAny(IReadOnlyList<TmEvent> tmEvents);

    Task<bool> HasPresentAps();

    Task<bool> HasPresentAlarms();
  }
}