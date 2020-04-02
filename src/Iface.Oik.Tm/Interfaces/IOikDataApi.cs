using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Interfaces
{
  public enum PreferApi
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
    event EventHandler TmAlertsChanged;
    event EventHandler UserInfoUpdated;


    void SetUserInfo(TmUserInfo userInfo);


    Task<TmServerInfo> GetServerInfo(PreferApi prefer = PreferApi.Auto);


    Task<int> GetLastTmcError(PreferApi prefer = PreferApi.Auto);


    Task<string> GetLastTmcErrorText(PreferApi prefer = PreferApi.Auto);


    Task<DateTime?> GetSystemTime(PreferApi prefer = PreferApi.Auto);


    Task<string> GetSystemTimeString(PreferApi prefer = PreferApi.Auto);


    Task<int> GetStatus(int       ch,
                        int       rtu,
                        int       point,
                        PreferApi prefer = PreferApi.Auto);


    Task SetStatus(int       ch,
                   int       rtu,
                   int       point,
                   int       status,
                   PreferApi prefer = PreferApi.Auto);


    Task<float> GetAnalog(int       ch,
                          int       rtu,
                          int       point,
                          PreferApi prefer = PreferApi.Auto);


    Task SetAnalog(int       ch,
                   int       rtu,
                   int       point,
                   float     value,
                   PreferApi prefer = PreferApi.Auto);


    Task UpdateStatus(TmStatus  status,
                      PreferApi prefer = PreferApi.Auto);


    Task UpdateAnalog(TmAnalog  analog,
                      PreferApi prefer = PreferApi.Auto);


    Task UpdateStatuses(IReadOnlyList<TmStatus> statuses,
                        PreferApi               prefer = PreferApi.Auto);


    Task UpdateAnalogs(IReadOnlyList<TmAnalog> analogs,
                       PreferApi               prefer = PreferApi.Auto);


    Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tags,
                                          PreferApi            prefer = PreferApi.Auto);


    Task UpdateTagPropertiesAndClassData(TmTag     tag,
                                         PreferApi prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<TmClassStatus>> GetStatusesClasses(PreferApi prefer = PreferApi.Auto);

    Task<IReadOnlyCollection<TmClassAnalog>> GetAnalogsClasses(PreferApi prefer = PreferApi.Auto);


    Task UpdateTechObjects(IReadOnlyList<TmTechObject> techObjects,
                           PreferApi                   prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<TmEvent>> GetEventsArchive(TmEventFilter filter,
                                                        PreferApi     prefer = PreferApi.Auto);


    Task<TmEventElix> GetCurrentEventsElix(PreferApi prefer = PreferApi.Auto);


    Task<(IReadOnlyCollection<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix,
                                                                       PreferApi   prefer = PreferApi.Auto);


    Task<bool> UpdateAckedEventsIfAny(IReadOnlyList<TmEvent> tmEvents,
                                      PreferApi              prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<TmChannel>> GetTmTreeChannels(PreferApi prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<TmRtu>> GetTmTreeRtus(int       channelId,
                                                   PreferApi prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<TmStatus>> GetTmTreeStatuses(int       channelId,
                                                          int       rtuId,
                                                          PreferApi prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<TmAnalog>> GetTmTreeAnalogs(int       channelId,
                                                         int       rtuId,
                                                         PreferApi prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr    addr,
                                                            long      utcStartTime,
                                                            int       count,
                                                            int       step,
                                                            int       retroNum = 0,
                                                            PreferApi prefer   = PreferApi.Auto);


    Task<IReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr    addr,
                                                            DateTime  startTime,
                                                            DateTime  endTime,
                                                            int       step     = 0,
                                                            int       retroNum = 0,
                                                            PreferApi prefer   = PreferApi.Auto);


    Task<IReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr    addr,
                                                            string    startTime,
                                                            string    endTime,
                                                            int       step     = 0,
                                                            int       retroNum = 0,
                                                            PreferApi prefer   = PreferApi.Auto);


    Task<IReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                                      long   utcStartTime,
                                                                                      long   utcEndTime,
                                                                                      PreferApi prefer =
                                                                                        PreferApi.Auto);


    Task<IReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr   addr,
                                                                                      DateTime startTime,
                                                                                      DateTime endTime,
                                                                                      PreferApi prefer =
                                                                                        PreferApi.Auto);


    Task<IReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                                      string startTime,
                                                                                      string endTime,
                                                                                      PreferApi prefer =
                                                                                        PreferApi.Auto);


    Task<IReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
                                                                                      long   utcStartTime,
                                                                                      long   utcEndTime,
                                                                                      int    step = 0,
                                                                                      PreferApi prefer =
                                                                                        PreferApi.Auto);


    Task<IReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr   addr,
                                                                                      DateTime startTime,
                                                                                      DateTime endTime,
                                                                                      int      step = 0,
                                                                                      PreferApi prefer =
                                                                                        PreferApi.Auto);


    Task<IReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
                                                                                      string startTime,
                                                                                      string endTime,
                                                                                      int    step = 0,
                                                                                      PreferApi prefer =
                                                                                        PreferApi.Auto);


    Task<IReadOnlyCollection<TmStatus>> GetPresentAps(PreferApi prefer = PreferApi.Auto);

    Task<IReadOnlyCollection<TmStatus>> GetUnackedAps(PreferApi prefer = PreferApi.Auto);

    Task<IReadOnlyCollection<TmStatus>> GetAbnormalStatuses(PreferApi prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<TmAlert>> GetAlerts(PreferApi prefer = PreferApi.Auto);


    Task<bool> RemoveAlert(TmAlert alert, PreferApi prefer = PreferApi.Auto);

    Task<bool> RemoveAlerts(IEnumerable<TmAlert> alerts, PreferApi api = PreferApi.Auto);


    Task<IReadOnlyCollection<TmAlarm>> GetPresentAlarms(PreferApi prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<TmAlarm>> GetAnalogAlarms(TmAnalog  analog,
                                                       PreferApi prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<TmStatus>> LookupStatuses(TmStatusFilter filter,
                                                       PreferApi      prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<TmAnalog>> LookupAnalogs(TmAnalogFilter filter,
                                                      PreferApi      prefer = PreferApi.Auto);


    Task<bool> HasPresentAps(PreferApi prefer = PreferApi.Auto);

    Task<bool> HasPresentAlarms(PreferApi prefer = PreferApi.Auto);


    Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScript(
      TmStatus  status,
      PreferApi prefer = PreferApi.Auto);


    Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScriptExplicitly(
      TmStatus  status,
      int       explicitNewStatus,
      PreferApi prefer = PreferApi.Auto);


    Task OverrideTelecontrolScript(PreferApi prefer = PreferApi.Auto);


    Task<TmTelecontrolResult> Telecontrol(TmStatus  status,
                                          PreferApi prefer = PreferApi.Auto);


    Task<TmTelecontrolResult> TelecontrolExplicitly(TmStatus  status,
                                                    int       explicitNewStatus,
                                                    PreferApi prefer = PreferApi.Auto);


    Task<TmTelecontrolResult> TeleregulateByStepUp(TmAnalog  analog,
                                                   PreferApi prefer = PreferApi.Auto);


    Task<TmTelecontrolResult> TeleregulateByStepDown(TmAnalog  analog,
                                                     PreferApi prefer = PreferApi.Auto);


    Task<TmTelecontrolResult> TeleregulateByCode(TmAnalog  analog,
                                                 int       code,
                                                 PreferApi prefer = PreferApi.Auto);


    Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog  analog,
                                                  float     value,
                                                  PreferApi prefer = PreferApi.Auto);


    Task<bool> SwitchStatusManually(TmStatus  status,
                                    bool      alsoBlockManually = false,
                                    PreferApi prefer            = PreferApi.Auto);


    Task SetStatusNormalOn(TmStatus  status,
                           PreferApi prefer = PreferApi.Auto);


    Task SetStatusNormalOff(TmStatus  status,
                            PreferApi prefer = PreferApi.Auto);


    Task ClearStatusNormal(TmStatus  status,
                           PreferApi prefer = PreferApi.Auto);


    Task<int> GetStatusNormal(TmStatus  status,
                              PreferApi prefer = PreferApi.Auto);


    Task SetTagFlags(TmTag     tag,
                     TmFlags   flags,
                     PreferApi prefer = PreferApi.Auto);


    Task ClearTagFlags(TmTag     tag,
                       TmFlags   flags,
                       PreferApi prefer = PreferApi.Auto);


    Task SetTagsFlags(IEnumerable<TmTag> tags,
                      TmFlags            flags,
                      PreferApi          prefer = PreferApi.Auto);


    Task ClearTagsFlags(IEnumerable<TmTag> tags,
                        TmFlags            flags,
                        PreferApi          prefer = PreferApi.Auto);


    Task<bool> SetAnalogManually(TmAnalog  analog,
                                 float     value,
                                 PreferApi prefer = PreferApi.Auto);


    Task<bool> SetAlarmValue(TmAlarm   tmAlarm,
                             float     value,
                             PreferApi prefer = PreferApi.Auto);


    Task<bool> AckTag(TmAddr    addr,
                      PreferApi prefer = PreferApi.Auto);


    Task AckAllStatuses(PreferApi prefer = PreferApi.Auto);

    Task AckAllAnalogs(PreferApi prefer = PreferApi.Auto);


    Task<bool> AckStatus(TmStatus  status,
                         PreferApi prefer = PreferApi.Auto);


    Task<bool> AckAnalog(TmAnalog  analog,
                         PreferApi prefer = PreferApi.Auto);


    Task<bool> AckEvent(TmEvent   tmEvent,
                        PreferApi prefer = PreferApi.Auto);


    Task AddStringToEventLog(string    str,
                             TmAddr    tmAddr = null,
                             PreferApi prefer = PreferApi.Auto);


    Task SetTechObjectProperties(int                                 scheme,
                                 int                                 type,
                                 int                                 obj,
                                 IReadOnlyDictionary<string, string> properties,
                                 PreferApi                           prefer = PreferApi.Auto);


    Task ClearTechObjectProperties(int                 scheme,
                                   int                 type,
                                   int                 obj,
                                   IEnumerable<string> properties,
                                   PreferApi           prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<string>> GetFilesInDirectory(string    path,
                                                          PreferApi prefer = PreferApi.Auto);


    Task<bool> DownloadFile(string    remotePath,
                            string    localPath,
                            PreferApi prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<string>> GetComtradeDays(PreferApi prefer = PreferApi.Auto);


    Task<IReadOnlyCollection<string>> GetComtradeFilesByDay(string    day,
                                                            PreferApi prefer = PreferApi.Auto);


    Task<bool> DownloadComtradeFile(string    filename,
                                    string    localPath,
                                    PreferApi prefer = PreferApi.Auto);
  }
}