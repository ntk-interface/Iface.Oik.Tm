using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Interfaces
{
  public interface ITmsApi
  {
    void SetCidAndUserInfo(int tmCid, TmUserInfo userInfo);


    Task<TmServerInfo> GetServerInfo();

    Task<int> GetLastTmcError();

    Task<string> GetLastTmcErrorText();

    Task<DateTime?> GetSystemTime();

    Task<string> GetSystemTimeString();

    Task<uint> GetCfCid();

    Task<int> GetStatus(int ch, int rtu, int point);

    Task SetStatus(int ch, int rtu, int point, int status);

    Task<float> GetAnalog(int ch, int rtu, int point);

    Task SetAnalog(int ch, int rtu, int point, float value);


    Task UpdateStatus(TmStatus status);


    Task UpdateStatusExplicitly(TmStatus status,
                                bool     getRealTelemetry = false);


    Task UpdateAnalog(TmAnalog analog);


    Task UpdateAnalogExplicitly(TmAnalog analog,
                                bool     getRealTelemetry = false);


    Task UpdateStatuses(IReadOnlyList<TmStatus> statuses);


    Task UpdateStatusesExplicitly(IList<TmStatus> statuses,
                                  bool            getRealTelemetry = false);


    Task UpdateAnalogs(IReadOnlyList<TmAnalog> analogs);


    Task UpdateAnalogsExplicitly(IList<TmAnalog> analogs,
                                 bool            getRealTelemetry = false);


    Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tags);

    Task UpdateTagPropertiesAndClassData(TmTag tag);

    Task UpdateTechObjectsProperties(IReadOnlyList<TmTechObject> techObjects);

    Task<TmEventElix> GetCurrentEventsElix();

    Task<ReadOnlyCollection<TmChannel>> GetTmTreeChannels();

    Task<ReadOnlyCollection<TmRtu>> GetTmTreeRtus(int channelId);

    Task<ReadOnlyCollection<TmTag>> GetTmsPoints(TmType tmType, int channelId, int rtuId);

    Task<ReadOnlyCollection<TmClassStatus>> GetStatusesClasses();

    Task<ReadOnlyCollection<TmClassAnalog>> GetAnalogsClasses();


    Task<ReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr addr,
                                                           long   utcStartTime,
                                                           int    count,
                                                           int    step,
                                                           int    retroNum = 0);


    Task<ReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr   addr,
                                                           DateTime startTime,
                                                           DateTime endTime,
                                                           int      step     = 0,
                                                           int      retroNum = 0);


    Task<ReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr addr,
                                                           string startTime,
                                                           string endTime,
                                                           int    step     = 0,
                                                           int    retroNum = 0);


    Task<ReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                                     long   utcStartTime,
                                                                                     long   utcEndTime);


    Task<ReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr   addr,
                                                                                     DateTime startTime,
                                                                                     DateTime endTime);


    Task<ReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                                     string startTime,
                                                                                     string endTime);


    Task<ReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
                                                                                     long   utcStartTime,
                                                                                     long   utcEndTime,
                                                                                     int    step = 0);


    Task<ReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr   addr,
                                                                                     DateTime startTime,
                                                                                     DateTime endTime,
                                                                                     int      step = 0);


    Task<ReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
                                                                                     string startTime,
                                                                                     string endTime,
                                                                                     int    step = 0);


    Task<bool> AckTag(TmAddr addr);

    Task AckAllStatuses();

    Task AckAllAnalogs();

    Task<bool> AckStatus(TmStatus status);

    Task<bool> AckAnalog(TmAnalog analog);

    Task<bool> AckEvent(TmEvent tmEvent);


    Task AddStringToEventLog(string str,
                             TmAddr tmAddr = null);


    Task SetTagFlags(TmTag   tag,
                     TmFlags flags);


    Task ClearTagFlags(TmTag   tag,
                       TmFlags flags);


    Task<bool> SetAnalogManually(TmAnalog tmAnalog,
                                 float    value);


    Task<bool> SetAlarmValue(TmAlarm tmAlarm,
                             float   value);


    Task<(bool, ReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScript(TmStatus tmStatus);


    Task<(bool, ReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScriptExplicitly(TmStatus tmStatus,
                                                                                                int
                                                                                                  explicitNewStatus);


    Task OverrideTelecontrolScript();

    Task<TmTelecontrolResult> Telecontrol(TmStatus tmStatus);

    Task<TmTelecontrolResult> TelecontrolExplicitly(TmStatus tmStatus, int explicitNewStatus);

    Task<TmTelecontrolResult> TeleregulateByStepUp(TmAnalog analog);

    Task<TmTelecontrolResult> TeleregulateByStepDown(TmAnalog analog);

    Task<TmTelecontrolResult> TeleregulateByCode(TmAnalog analog, int code);

    Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog analog, float value);


    Task<bool> SwitchStatusManually(TmStatus tmStatus,
                                    bool     alsoBlockManually = false);


    Task SetTechObjectProperties(int                                 scheme,
                                 int                                 type,
                                 int                                 obj,
                                 IReadOnlyDictionary<string, string> properties);


    Task ClearTechObjectProperties(int                 scheme,
                                   int                 type,
                                   int                 obj,
                                   IEnumerable<string> properties);


    Task SetStatusNormalOn(TmStatus status);


    Task SetStatusNormalOff(TmStatus status);


    Task ClearStatusNormal(TmStatus status);


    Task<int> GetStatusNormal(TmStatus status);


    Task<ReadOnlyCollection<string>> GetFilesInDirectory(string path);


    Task<bool> DownloadFile(string remotePath,
                            string localPath);


    Task SetMultipleTagsFlags(IEnumerable<TmTag> tmTags,
                              TmFlags            flags);


    Task ClearMultipleTagsFlags(IEnumerable<TmTag> tmTags,
                                TmFlags            flags);


    Task<bool> SetTagFlagsExplicitly(TmTag tag, TmFlags flags);


    Task<bool> ClearTagFlagsExplicitly(TmTag tag, TmFlags flags);
  }
}