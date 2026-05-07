using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScript(
    TmStatus  status,
    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.CheckTelecontrolScript(status),
                         null,
                         () => (false, null))
            .ConfigureAwait(false);
  }


  public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScriptExplicitly(
    TmStatus  status,
    int       explicitNewStatus,
    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.CheckTelecontrolScriptExplicitly(status, explicitNewStatus),
                         null,
                         () => (false, null))
            .ConfigureAwait(false);
  }


  public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTeleregulationScript(
    TmAnalog  tmAnalog,
    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.CheckTeleregulationScript(tmAnalog),
                         null,
                         () => (false, null))
            .ConfigureAwait(false);
  }


  public async Task OverrideTelecontrolScript(PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.OverrideTelecontrolScript(),
                  null)
     .ConfigureAwait(false);
  }


  public async Task<TmTelecontrolResult> Telecontrol(TmStatus  status,
                                                     PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.Telecontrol(status),
                         null,
                         () => TmTelecontrolResult.CommandNotSentToServer)
            .ConfigureAwait(false);
  }


  public async Task<TmTelecontrolResult> TelecontrolExplicitly(TmStatus  status,
                                                               int       explicitNewStatus,
                                                               PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.TelecontrolExplicitly(status, explicitNewStatus),
                         null,
                         () => TmTelecontrolResult.CommandNotSentToServer)
            .ConfigureAwait(false);
  }


  public async Task<TmTelecontrolResult> TeleregulateByStepUp(TmAnalog  analog,
                                                              PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.TeleregulateByStepUp(analog),
                         null,
                         () => TmTelecontrolResult.CommandNotSentToServer)
            .ConfigureAwait(false);
  }


  public async Task<TmTelecontrolResult> TeleregulateByStepDown(TmAnalog  analog,
                                                                PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.TeleregulateByStepDown(analog),
                         null,
                         () => TmTelecontrolResult.CommandNotSentToServer)
            .ConfigureAwait(false);
  }


  public async Task<TmTelecontrolResult> TeleregulateByCode(TmAnalog  analog,
                                                            int       code,
                                                            PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.TeleregulateByCode(analog, code),
                         null,
                         () => TmTelecontrolResult.CommandNotSentToServer)
            .ConfigureAwait(false);
  }


  public async Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog  analog,
                                                             float     value,
                                                             PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.TeleregulateByValue(analog, value),
                         null,
                         () => TmTelecontrolResult.CommandNotSentToServer)
            .ConfigureAwait(false);
  }


  public async Task InputTelecontrolPassword(string    password,
                                             PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.InputTelecontrolPassword(password),
                  null)
     .ConfigureAwait(false);
  }


  public async Task<bool> SwitchStatusManually(TmStatus  status,
                                               bool      alsoBlockManually = false,
                                               PreferApi prefer            = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.SwitchStatusManually(status, alsoBlockManually),
                         null)
            .ConfigureAwait(false);
  }


  public async Task SetStatusNormalOn(TmStatus  status,
                                      PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.SetStatusNormalOn(status),
                  null)
     .ConfigureAwait(false);
  }


  public async Task SetStatusNormalOff(TmStatus  status,
                                       PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.SetStatusNormalOff(status),
                  null)
     .ConfigureAwait(false);
  }


  public async Task ClearStatusNormal(TmStatus  status,
                                      PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.ClearStatusNormal(status),
                  null)
     .ConfigureAwait(false);
  }


  public async Task<int> GetStatusNormal(TmStatus  status,
                                         PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetStatusNormal(status),
                         null,
                         () => -1)
            .ConfigureAwait(false);
  }


  public async Task<bool> BackdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs,
                                          IReadOnlyList<float>    values,
                                          DateTime                time,
                                          PreferApi               prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.BackdateAnalogs(tmAnalogs, values, time),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> PostdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs,
                                          IReadOnlyList<float>    values,
                                          DateTime                time,
                                          PreferApi               prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.PostdateAnalogs(tmAnalogs, values, time),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> SetAnalogManually(TmAnalog  analog,
                                            float     value,
                                            bool      alsoBlockManually = false,
                                            PreferApi prefer            = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.SetAnalogManually(analog, value, alsoBlockManually),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> SetAnalogBackdateManually(TmAnalog  tmAnalog,
                                                    float     value,
                                                    DateTime  time,
                                                    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.SetAnalogBackdateManually(tmAnalog, value, time),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> SetStatusBackdateManually(TmStatus  analog,
                                                    int       status,
                                                    DateTime  time,
                                                    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.SetStatusBackdateManually(analog, status, time),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> SetAnalogTechParameters(TmAnalog  analog, TmAnalogTechParameters parameters,
                                                  PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.SetAnalogTechParameters(analog, parameters),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> SetAlarmValue(TmAlarm   alarm,
                                        float     value,
                                        PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.SetAlarmValue(alarm, value),
                         null)
            .ConfigureAwait(false);
  }


  public async Task SetTagFlags(TmTag     tag,
                                TmFlags   flags,
                                PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.SetTagFlags(tag, flags),
                  null)
     .ConfigureAwait(false);
  }


  public async Task ClearTagFlags(TmTag     tag,
                                  TmFlags   flags,
                                  PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.ClearTagFlags(tag, flags),
                  null)
     .ConfigureAwait(false);
  }


  public async Task SetTagFlagsExplicitly(TmTag     tag,
                                          TmFlags   flags,
                                          PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.SetTagFlagsExplicitly(tag, flags),
                  null)
     .ConfigureAwait(false);
  }


  public async Task ClearTagFlagsExplicitly(TmTag     tag,
                                            TmFlags   flags,
                                            PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.ClearTagFlagsExplicitly(tag, flags),
                  null)
     .ConfigureAwait(false);
  }


  public async Task SetTagsFlags(IEnumerable<TmTag> tags,
                                 TmFlags            flags,
                                 PreferApi          prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.SetTagsFlags(tags, flags),
                  null)
     .ConfigureAwait(false);
  }


  public async Task ClearTagsFlags(IEnumerable<TmTag> tags,
                                   TmFlags            flags,
                                   PreferApi          prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.ClearTagsFlags(tags, flags),
                  null)
     .ConfigureAwait(false);
  }


  public async Task<bool> AckTag(TmAddr    addr,
                                 PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.AckTag(addr),
                         null)
            .ConfigureAwait(false);
  }


  public async Task AckAllStatuses(PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.AckAllStatuses(),
                  null)
     .ConfigureAwait(false);
  }


  public async Task AckAllAnalogs(PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.AckAllAnalogs(),
                  null)
     .ConfigureAwait(false);
  }


  public async Task<bool> AckStatus(TmStatus  status,
                                    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.AckStatus(status),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> AckAnalog(TmAnalog  analog,
                                    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.AckAnalog(analog),
                         null)
            .ConfigureAwait(false);
  }
}