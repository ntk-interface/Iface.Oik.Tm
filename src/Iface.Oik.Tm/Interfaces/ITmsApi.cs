using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Dto;

namespace Iface.Oik.Tm.Interfaces
{
  public interface ITmsApi
  {
    void SetCidAndUserInfo(int tmCid, TmUserInfo userInfo);

    Task<TmServerComputerInfo> GetServerComputerInfo();

    Task<int> GetLastTmcError();

    Task<string> GetLastTmcErrorText();

    string GetConnectionErrorText();

    Task<DateTime?> GetSystemTime();

    Task<string> GetSystemTimeString();

    Task<(string host, string server)> GetCurrentServerName();

    Task<(string user, string password)> GenerateTokenForExternalApp();

    Task<IntPtr> GetCfCid();

    Task<int> GetStatus(int ch, int rtu, int point);

    Task<int> GetStatusFromRetro(int ch, int rtu, int point, DateTime time);

    Task SetStatus(int ch, int rtu, int point, int status);

    Task<float> GetAnalog(int ch, int rtu, int point);

    Task<float> GetAnalogFromRetro(int ch, int rtu, int point, DateTime time, int retroNum = 0);

    Task SetAnalog(int ch, int rtu, int point, float value);

    Task UpdateTag(TmTag tag);

    Task UpdateStatus(TmStatus status);

    Task UpdateStatusExplicitly(TmStatus status, bool getRealTelemetry = false);

    Task UpdateAnalog(TmAnalog analog);


    Task UpdateAnalogExplicitly(TmAnalog analog,
                                uint     time             = 0,
                                ushort   retroNum         = 0,
                                bool     getRealTelemetry = false);


    Task UpdateStatuses(IReadOnlyList<TmStatus> statuses);

    Task UpdateStatusesExplicitly(IReadOnlyList<TmStatus> statuses, bool getRealTelemetry = false);

    Task UpdateAnalogs(IReadOnlyList<TmAnalog> analogs);


    Task UpdateAnalogsExplicitly(IReadOnlyList<TmAnalog> analogs,
                                 uint                    time             = 0,
                                 ushort                  retroNum         = 0,
                                 bool                    getRealTelemetry = false);


    Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tags);

    Task UpdateTagPropertiesAndClassData(TmTag tag);

    Task UpdateTechObjectsProperties(IReadOnlyList<Tob> techObjects);

    Task<IReadOnlyCollection<Tob>> GetTechObjects(TobFilter filter);

    Task<TmEventElix> GetCurrentEventsElix();

    Task<string> GetExpressionResult(string expression);

    Task<IReadOnlyCollection<TmChannel>> GetTmTreeChannels();

    Task<IReadOnlyCollection<TmRtu>> GetTmTreeRtus(int channelId);

    Task<IReadOnlyCollection<TmStatus>> GetTmTreeStatuses(int channelId, int rtuId);

    Task<IReadOnlyCollection<TmAnalog>> GetTmTreeAnalogs(int channelId, int rtuId);

    Task<string> GetChannelName(int channelId);

    Task<string> GetRtuName(int channelId, int rtuId);

    Task<IReadOnlyCollection<TmClassStatus>> GetStatusesClasses();

    Task<IReadOnlyCollection<TmClassAnalog>> GetAnalogsClasses();

    Task<IReadOnlyCollection<ITmAnalogRetro[]>> GetAnalogsMicroSeries(IReadOnlyList<TmAnalog> analogs);


    Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetro(TmAnalog analog,
                                                             long     utcStartTime,
                                                             int      count,
                                                             int      step,
                                                             int      retroNum = 0);


    Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetro(TmAnalog            analog,
                                                             TmAnalogRetroFilter filter,
                                                             int                 retroNum = 0);


    Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetroEx(TmAnalog            analog,
                                                               TmAnalogRetroFilter filter,
                                                               int                 retroNum         = 0,
                                                               bool                getRealTelemetry = false);


    Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveInstant(TmAnalog            analog,
                                                                       TmAnalogRetroFilter filter);


    Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveAverage(TmAnalog            analog,
                                                                       TmAnalogRetroFilter filter);

    Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveSlices(TmAnalog analog,
                                                                              TmAnalogRetroFilter filter);
    Task<bool> RemoveAlert(TmAlert alert);

    Task<bool> RemoveAlert(byte[] alertId);

    Task<bool> RemoveAlerts(IEnumerable<TmAlert> alerts);

    Task<IReadOnlyCollection<TmEvent>> GetEventsArchive(TmEventFilter filter);

    Task<(IReadOnlyCollection<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix);

    Task<bool> AckTag(TmAddr addr);

    Task AckAllStatuses();

    Task AckAllAnalogs();

    Task<bool> AckStatus(TmStatus status);

    Task<bool> AckAnalog(TmAnalog analog);

    Task<bool> AckEvent(TmEvent tmEvent);


    Task AddStringToEventLog(string    str,
                             TmAddr    tmAddr = null,
                             DateTime? time   = null);


    Task AddTmaRelatedStringToEventLog(string             message,
                                       TmAddr             tmAddr,
                                       TmEventImportances importances = TmEventImportances.Imp0,
                                       DateTime?          time        = null);


    Task AddStringToEventLogEx(DateTime?                 time,
                               TmEventImportances        importances,
                               TmEventLogExtendedSources source,
                               string                    message,
                               string                    binaryString = "",
                               TmAddr                    tmAddr       = null);


    Task AddStrBinToEventLog(DateTime?                 time,
                             TmEventImportances        importances,
                             TmEventLogExtendedSources source,
                             string                    message,
                             byte[]                    binary = null,
                             TmAddr                    tmAddr = null);


    Task SetTagFlags(TmTag tag, TmFlags flags);

    Task ClearTagFlags(TmTag tag, TmFlags flags);

    Task SetTagsFlags(IEnumerable<TmTag> tmTags, TmFlags flags);

    Task ClearTagsFlags(IEnumerable<TmTag> tmTags, TmFlags flags);

    Task<bool> SetAnalogManually(TmAnalog tmAnalog, float value, bool alsoBlockManually = false);

    Task<bool> SetAnalogTechParameters(TmAnalog analog, TmAnalogTechParameters parameters);

    Task<bool> SetAlarmValue(TmAlarm tmAlarm, float value);

    Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScript(TmStatus tmStatus);


    Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScriptExplicitly(TmStatus tmStatus,
                                                                                                 int
                                                                                                   explicitNewStatus);


    Task OverrideTelecontrolScript();

    Task<TmTelecontrolResult> Telecontrol(TmStatus tmStatus);

    Task<TmTelecontrolResult> TelecontrolExplicitly(TmStatus tmStatus, int explicitNewStatus);

    Task<TmTelecontrolResult> TeleregulateByStepUp(TmAnalog analog);

    Task<TmTelecontrolResult> TeleregulateByStepDown(TmAnalog analog);

    Task<TmTelecontrolResult> TeleregulateByCode(TmAnalog analog, int code);

    Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog analog, float value);

    Task<bool> SwitchStatusManually(TmStatus tmStatus, bool alsoBlockManually = false);

    Task SetTechObjectsProperties(IReadOnlyCollection<Tob> tobs);


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

    Task<IReadOnlyCollection<string>> GetFilesInDirectory(string path);

    Task<bool> DownloadFile(string remotePath, string localPath);

    Task<IReadOnlyCollection<string>> GetComtradeDays();

    Task<IReadOnlyCollection<string>> GetComtradeFilesByDay(string day);

    Task<bool> DownloadComtradeFile(string filename, string localPath);

    Task SetTagFlagsExplicitly(TmTag tag, TmFlags flags);

    Task ClearTagFlagsExplicitly(TmTag tag, TmFlags flags);

    Task StartTmAddrTracer(int channel, int rtu, int point, TmType tmType, TmTraceTypes filterTypes);

    Task StopTmAddrTracer(int channel, int rtu, int point, TmType tmType);


    Task<TmServerInfo> GetServerInfo();

    Task<IReadOnlyCollection<TmServerThread>> GetServerThreads();

    Task<TmAccessRights> GetAccessRights();


    Task<IReadOnlyCollection<TmUserInfo>> GetUsersInfo();

    Task<TmUserInfo> GetUserInfo(uint userId);

    Task<TmUserInfo> GetExtendedUserInfo(int userId);


    Task<IReadOnlyCollection<TmStatus>> GetPresentAps();


    Task<IReadOnlyCollection<TmTag>> GetTagsByGroup(TmType tmType,
                                                    string groupName);


    Task<bool> BlockTagEventsTemporarily(TmTag tmTag,
                                         int   minutesToBlock);


    Task UnblockTagEvents(TmTag tmTag);


    Task<IReadOnlyCollection<TmTag>> GetTagsByFlags(TmType             tmType,
                                                    TmFlags            tmFlags,
                                                    TmCommonPointFlags filterFlags);


    Task<IReadOnlyCollection<TmTag>> GetTagsByNamePattern(TmType tmType,
                                                          string pattern);


    Task<IReadOnlyCollection<TmRetroInfo>> GetRetrosInfo(TmType tmType);

    Task<bool> MqttSubscribe(MqttSubscriptionTopic topic);
    
    Task<bool> MqttSubscribe(MqttKnownTopic topic);

    Task<bool> MqttUnsubscribe(MqttSubscriptionTopic topic);

    Task<bool> MqttUnsubscribe(MqttKnownTopic topic);

    Task<bool> MqttPublish(MqttKnownTopic topic, string payload = "");

    Task<bool> MqttPublish(MqttPublishTopic topic, byte[] payload);

    Task<bool> MqttPublish(MqttPublishTopic topic, string payload);
  }
}