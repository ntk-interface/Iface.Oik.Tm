using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    
    Task UpdateStatus(TmStatus status);

    Task UpdateAnalog(TmAnalog analog);

    Task UpdateStatuses(IReadOnlyList<TmStatus> statuses);

    Task UpdateAnalogs(IReadOnlyList<TmAnalog> analogs);

    Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tags);

    Task UpdateTagPropertiesAndClassData(TmTag tag);

    Task<ReadOnlyCollection<TmEvent>> GetArchEvents(TmEventFilter filter);

    Task<(ReadOnlyCollection<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix);

    Task<ReadOnlyCollection<TmChannel>> GetTmTreeChannels();

    Task<ReadOnlyCollection<TmRtu>> GetTmTreeRtus(int channelId);

    Task<ReadOnlyCollection<TmStatus>> GetTmTreeStatuses(int channelId, int rtuId);

    Task<ReadOnlyCollection<TmAnalog>> GetTmTreeAnalogs(int channelId, int rtuId);

    Task<ReadOnlyCollection<TmStatus>> GetPresentAps();
    
    Task<ReadOnlyCollection<TmStatus>> GetUnackedAps();

    Task<ReadOnlyCollection<TmStatus>> GetAbnormalStatuses();

    Task<ReadOnlyCollection<TmAlarm>> GetPresentAlarms();

    Task<ReadOnlyCollection<TmAlarm>> GetAnalogAlarms(TmAnalog analog);
    
    Task<ReadOnlyCollection<TmStatus>> LookupStatuses(TmStatusFilter filter);
    
    Task<ReadOnlyCollection<TmAnalog>> LookupAnalogs(TmAnalogFilter filter);
    
    Task<bool> UpdateAckedEventsIfAny(IReadOnlyList<TmEvent> tmEvents);
    
    Task<bool> HasPresentAps();
    
    Task<bool> HasPresentAlarms();
  }
}