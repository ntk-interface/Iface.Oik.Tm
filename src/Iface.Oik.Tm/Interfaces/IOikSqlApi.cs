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
    
    Task UpdateStatus(TmStatus status);

    Task UpdateAnalog(TmAnalog analog);

    Task UpdateStatuses(IList<TmStatus> statuses);

    Task UpdateAnalogs(IList<TmAnalog> analogs);

    Task UpdateTagsPropertiesAndClassData(IEnumerable<TmTag> tags);

    Task UpdateTagPropertiesAndClassData(TmTag tag);

    Task<IEnumerable<TmEvent>> GetArchEvents(TmEventFilter filter);

    Task<(IEnumerable<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix);

    Task<IEnumerable<TmChannel>> GetTmTreeChannels();

    Task<IEnumerable<TmRtu>> GetTmTreeRtus(int channelId);

    Task<IEnumerable<TmStatus>> GetTmTreeStatuses(int channelId, int rtuId);

    Task<IEnumerable<TmAnalog>> GetTmTreeAnalogs(int channelId, int rtuId);

    Task<IEnumerable<TmStatus>> GetPresentAps();
    
    Task<IEnumerable<TmStatus>> GetUnackedAps();

    Task<IEnumerable<TmStatus>> GetAbnormalStatuses();
    
    Task<IEnumerable<TmStatus>> LookupStatuses(TmStatusFilter filter);
    
    Task<IEnumerable<TmAnalog>> LookupAnalogs(TmAnalogFilter filter);

    Task<IEnumerable<TmAlarm>> GetPresentAlarms();

    Task<IEnumerable<TmAlarm>> GetAnalogAlarms(TmAnalog analog);
    
    Task<bool> UpdateAckedEventsIfAny(IList<TmEvent> tmEvents);
    
    Task<bool> HasPresentAps();
    
    Task<bool> HasPresentAlarms();
  }
}