using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Interfaces
{
  public enum DataApiPreference
  {
    Auto = 0,
    Tms  = 1,
    Sql  = 2,
  }


  public interface IOikDataApi
  {
    bool IsTmsAllowed { get; set; }
    bool IsSqlAllowed { get; set; }

    TmNativeCallback TmsCallbackDelegate      { get; }
    TmNativeCallback EmptyTmsCallbackDelegate { get; }

    event EventHandler TmEventsAcked;
    event EventHandler UserInfoUpdated;


    void SetUserInfo(TmUserInfo userInfo);


    Task<TmServerInfo> GetServerInfo(DataApiPreference preference = DataApiPreference.Auto);


    Task<int> GetLastTmcError(DataApiPreference preference = DataApiPreference.Auto);


    Task<string> GetLastTmcErrorText(DataApiPreference preference = DataApiPreference.Auto);


    Task<DateTime?> GetSystemTime(DataApiPreference preference = DataApiPreference.Auto);


    Task<string> GetSystemTimeString(DataApiPreference preference = DataApiPreference.Auto);


    Task<int> GetStatus(int               ch,
                        int               rtu,
                        int               point,
                        DataApiPreference preference = DataApiPreference.Auto);


    Task SetStatus(int               ch,
                   int               rtu,
                   int               point,
                   int               status,
                   DataApiPreference preference = DataApiPreference.Auto);


    Task<float> GetAnalog(int               ch,
                          int               rtu,
                          int               point,
                          DataApiPreference preference = DataApiPreference.Auto);


    Task SetAnalog(int               ch,
                   int               rtu,
                   int               point,
                   float             value,
                   DataApiPreference preference = DataApiPreference.Auto);


    Task UpdateStatus(TmStatus          status,
                      DataApiPreference preference = DataApiPreference.Auto);


    Task UpdateAnalog(TmAnalog          analog,
                      DataApiPreference preference = DataApiPreference.Auto);


    Task UpdateStatuses(IList<TmStatus>   statuses,
                        DataApiPreference preference = DataApiPreference.Auto);


    Task UpdateAnalogs(IList<TmAnalog>   analogs,
                       DataApiPreference preference = DataApiPreference.Auto);


    Task UpdateTagsPropertiesAndClassData(IEnumerable<TmTag> tags,
                                          DataApiPreference  preference = DataApiPreference.Auto);


    Task UpdateTagPropertiesAndClassData(TmTag             tag,
                                         DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmClassStatus>> GetStatusesClasses(DataApiPreference preference = DataApiPreference.Auto);

    Task<IEnumerable<TmClassAnalog>> GetAnalogsClasses(DataApiPreference preference = DataApiPreference.Auto);


    Task UpdateTechObjects(IList<TmTechObject> techObjects,
                           DataApiPreference   preference = DataApiPreference.Auto);


    Task<IEnumerable<TmEvent>> GetEventsArchive(TmEventFilter     filter,
                                                DataApiPreference preference = DataApiPreference.Auto);


    Task<TmEventElix> GetCurrentEventsElix(DataApiPreference preference = DataApiPreference.Auto);


    Task<(IEnumerable<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix       elix,
                                                               DataApiPreference preference = DataApiPreference.Auto);


    Task<bool> UpdateAckedEventsIfAny(IList<TmEvent>    tmEvents,
                                      DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmChannel>> GetTmTreeChannels(DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmRtu>> GetTmTreeRtus(int               channelId,
                                           DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmStatus>> GetTmTreeStatuses(int               channelId,
                                                  int               rtuId,
                                                  DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmAnalog>> GetTmTreeAnalogs(int               channelId,
                                                 int               rtuId,
                                                 DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmAnalogRetro>> GetAnalogRetro(TmAddr            addr,
                                                    long              utcStartTime,
                                                    int               count,
                                                    int               step,
                                                    int               retroNum   = 0,
                                                    DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmAnalogRetro>> GetAnalogRetro(TmAddr            addr,
                                                    DateTime          startTime,
                                                    DateTime          endTime,
                                                    int               step       = 0,
                                                    int               retroNum   = 0,
                                                    DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmAnalogRetro>> GetAnalogRetro(TmAddr            addr,
                                                    string            startTime,
                                                    string            endTime,
                                                    int               step       = 0,
                                                    int               retroNum   = 0,
                                                    DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                              long   utcStartTime,
                                                                              long   utcEndTime,
                                                                              DataApiPreference preference =
                                                                                DataApiPreference.Auto);


    Task<IEnumerable<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr   addr,
                                                                              DateTime startTime,
                                                                              DateTime endTime,
                                                                              DataApiPreference preference =
                                                                                DataApiPreference.Auto);


    Task<IEnumerable<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                              string startTime,
                                                                              string endTime,
                                                                              DataApiPreference preference =
                                                                                DataApiPreference.Auto);


    Task<IEnumerable<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
                                                                              long   utcStartTime,
                                                                              long   utcEndTime,
                                                                              int    step = 0,
                                                                              DataApiPreference preference =
                                                                                DataApiPreference.Auto);


    Task<IEnumerable<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr   addr,
                                                                              DateTime startTime,
                                                                              DateTime endTime,
                                                                              int      step = 0,
                                                                              DataApiPreference preference =
                                                                                DataApiPreference.Auto);


    Task<IEnumerable<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
                                                                              string startTime,
                                                                              string endTime,
                                                                              int    step = 0,
                                                                              DataApiPreference preference =
                                                                                DataApiPreference.Auto);


    Task<IEnumerable<TmStatus>> GetPresentAps(DataApiPreference preference = DataApiPreference.Auto);

    Task<IEnumerable<TmStatus>> GetUnackedAps(DataApiPreference preference = DataApiPreference.Auto);

    Task<IEnumerable<TmStatus>> GetAbnormalStatuses(DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmStatus>> LookupStatuses(TmStatusFilter    filter,
                                               DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmAnalog>> LookupAnalogs(TmAnalogFilter    filter,
                                              DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmAlarm>> GetPresentAlarms(DataApiPreference preference = DataApiPreference.Auto);


    Task<IEnumerable<TmAlarm>> GetAnalogAlarms(TmAnalog          analog,
                                               DataApiPreference preference = DataApiPreference.Auto);


    Task<bool> HasPresentAps(DataApiPreference preference = DataApiPreference.Auto);

    Task<bool> HasPresentAlarms(DataApiPreference preference = DataApiPreference.Auto);


    Task<(bool, IReadOnlyList<TmControlScriptCondition>)> CheckTelecontrolScript(
      TmStatus          status,
      DataApiPreference preference = DataApiPreference.Auto);


    Task<(bool, IReadOnlyList<TmControlScriptCondition>)> CheckTelecontrolScriptExplicitly(
      TmStatus          status,
      int               explicitNewStatus,
      DataApiPreference preference = DataApiPreference.Auto);


    Task OverrideTelecontrolScript(DataApiPreference preference = DataApiPreference.Auto);


    Task<TmTelecontrolResult> Telecontrol(TmStatus          status,
                                          DataApiPreference preference = DataApiPreference.Auto);


    Task<TmTelecontrolResult> TelecontrolExplicitly(TmStatus          status,
                                                    int               explicitNewStatus,
                                                    DataApiPreference preference = DataApiPreference.Auto);


    Task<TmTelecontrolResult> TeleregulateByStepUp(TmAnalog          analog,
                                                   DataApiPreference preference = DataApiPreference.Auto);


    Task<TmTelecontrolResult> TeleregulateByStepDown(TmAnalog          analog,
                                                     DataApiPreference preference = DataApiPreference.Auto);


    Task<TmTelecontrolResult> TeleregulateByCode(TmAnalog          analog,
                                                 int               code,
                                                 DataApiPreference preference = DataApiPreference.Auto);


    Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog          analog,
                                                  float             value,
                                                  DataApiPreference preference = DataApiPreference.Auto);


    Task<bool> SwitchStatusManually(TmStatus          status,
                                    bool              alsoBlockManually = false,
                                    DataApiPreference preference        = DataApiPreference.Auto);


    Task SetStatusNormalOn(TmStatus          status,
                           DataApiPreference preference = DataApiPreference.Auto);


    Task SetStatusNormalOff(TmStatus          status,
                            DataApiPreference preference = DataApiPreference.Auto);


    Task ClearStatusNormal(TmStatus          status,
                           DataApiPreference preference = DataApiPreference.Auto);


    Task<int> GetStatusNormal(TmStatus          status,
                              DataApiPreference preference = DataApiPreference.Auto);


    Task SetTagFlags(TmTag             tag,
                     TmFlags           flags,
                     DataApiPreference preference = DataApiPreference.Auto);


    Task ClearTagFlags(TmTag             tag,
                       TmFlags           flags,
                       DataApiPreference preference = DataApiPreference.Auto);


    Task<bool> SetAnalogManually(TmAnalog          analog,
                                 float             value,
                                 DataApiPreference preference = DataApiPreference.Auto);


    Task<bool> SetAlarmValue(TmAlarm           tmAlarm,
                             float             value,
                             DataApiPreference preference = DataApiPreference.Auto);


    Task<bool> AckTag(TmAddr            addr,
                      DataApiPreference preference = DataApiPreference.Auto);


    Task AckAllStatuses(DataApiPreference preference = DataApiPreference.Auto);

    Task AckAllAnalogs(DataApiPreference preference = DataApiPreference.Auto);


    Task<bool> AckStatus(TmStatus          status,
                         DataApiPreference preference = DataApiPreference.Auto);


    Task<bool> AckAnalog(TmAnalog          analog,
                         DataApiPreference preference = DataApiPreference.Auto);


    Task<bool> AckEvent(TmEvent           tmEvent,
                        DataApiPreference preference = DataApiPreference.Auto);


    Task AddStringToEventLog(string            str,
                             TmAddr            tmAddr     = null,
                             DataApiPreference preference = DataApiPreference.Auto);


    Task SetTechObjectProperties(int                                 scheme,
                                 int                                 type,
                                 int                                 obj,
                                 IReadOnlyDictionary<string, string> properties,
                                 DataApiPreference                   preference = DataApiPreference.Auto);


    Task ClearTechObjectProperties(int                 scheme,
                                   int                 type,
                                   int                 obj,
                                   IEnumerable<string> properties,
                                   DataApiPreference   preference = DataApiPreference.Auto);


    Task<IEnumerable<string>> GetFilesInDirectory(string            path,
                                                  DataApiPreference preference = DataApiPreference.Auto);


    Task<bool> DownloadFile(string            remotePath,
                            string            localPath,
                            DataApiPreference preference = DataApiPreference.Auto);
  }
}