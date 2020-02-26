using System;
using System.Collections.Generic;
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

    Task UpdateAnalog(TmAnalog analog);

    Task UpdateStatuses(IList<TmStatus> statuses);

    Task UpdateAnalogs(IList<TmAnalog> analogs);

    Task UpdateTagsPropertiesAndClassData(IEnumerable<TmTag> tags);

    Task UpdateTagPropertiesAndClassData(TmTag tag);

    Task UpdateTechObjectsProperties(IList<TmTechObject> techObjects);

    Task<TmEventElix> GetCurrentEventsElix();

    Task<IEnumerable<TmChannel>> GetTmTreeChannels();

    Task<IEnumerable<TmRtu>> GetTmTreeRtus(int channelId);

    Task<IEnumerable<TmClassStatus>> GetStatusesClasses();

    Task<IEnumerable<TmClassAnalog>> GetAnalogsClasses();


    Task<IEnumerable<TmAnalogRetro>> GetAnalogRetro(TmAddr addr,
                                                    long   utcStartTime,
                                                    int    count,
                                                    int    step,
                                                    int    retroNum = 0);


    Task<IEnumerable<TmAnalogRetro>> GetAnalogRetro(TmAddr   addr,
                                                    DateTime startTime,
                                                    DateTime endTime,
                                                    int      step     = 0,
                                                    int      retroNum = 0);


    Task<IEnumerable<TmAnalogRetro>> GetAnalogRetro(TmAddr addr,
                                                    string startTime,
                                                    string endTime,
                                                    int    step     = 0,
                                                    int    retroNum = 0);


    Task<IEnumerable<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                              long   utcStartTime,
                                                                              long   utcEndTime);


    Task<IEnumerable<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr   addr,
                                                                              DateTime startTime,
                                                                              DateTime endTime);


    Task<IEnumerable<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                              string startTime,
                                                                              string endTime);


    Task<IEnumerable<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
                                                                              long   utcStartTime,
                                                                              long   utcEndTime,
                                                                              int    step = 0);


    Task<IEnumerable<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr   addr,
                                                                              DateTime startTime,
                                                                              DateTime endTime,
                                                                              int      step = 0);


    Task<IEnumerable<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
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


    Task<(bool, IReadOnlyList<TmControlScriptCondition>)> CheckTelecontrolScript(TmStatus tmStatus);


    Task<(bool, IReadOnlyList<TmControlScriptCondition>)> CheckTelecontrolScriptExplicitly(TmStatus tmStatus,
                                                                                           int      explicitNewStatus);


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


    Task<IEnumerable<string>> GetFilesInDirectory(string path);


    Task<bool> DownloadFile(string remotePath,
                            string localPath);
  }
}