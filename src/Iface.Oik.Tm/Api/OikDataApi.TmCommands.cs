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
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.CheckTelecontrolScript(status).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return (false, null);
    }
  }


  public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScriptExplicitly(
    TmStatus  status,
    int       explicitNewStatus,
    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.CheckTelecontrolScriptExplicitly(status, explicitNewStatus).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return (false, null);
    }
  }


  public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTeleregulationScript(
    TmAnalog  tmAnalog,
    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.CheckTeleregulationScript(tmAnalog).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return (false, null);
    }
  }


  public async Task OverrideTelecontrolScript(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.OverrideTelecontrolScript().ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task<TmTelecontrolResult> Telecontrol(TmStatus  status,
                                                     PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.Telecontrol(status).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return TmTelecontrolResult.CommandNotSentToServer;
    }
  }


  public async Task<TmTelecontrolResult> TelecontrolExplicitly(TmStatus  status,
                                                               int       explicitNewStatus,
                                                               PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.TelecontrolExplicitly(status, explicitNewStatus).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return TmTelecontrolResult.CommandNotSentToServer;
    }
  }


  public async Task<TmTelecontrolResult> TeleregulateByStepUp(TmAnalog  analog,
                                                              PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.TeleregulateByStepUp(analog).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return TmTelecontrolResult.CommandNotSentToServer;
    }
  }


  public async Task<TmTelecontrolResult> TeleregulateByStepDown(TmAnalog  analog,
                                                                PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.TeleregulateByStepDown(analog).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return TmTelecontrolResult.CommandNotSentToServer;
    }
  }


  public async Task<TmTelecontrolResult> TeleregulateByCode(TmAnalog  analog,
                                                            int       code,
                                                            PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.TeleregulateByCode(analog, code).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return TmTelecontrolResult.CommandNotSentToServer;
    }
  }


  public async Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog  analog,
                                                             float     value,
                                                             PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.TeleregulateByValue(analog, value).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return TmTelecontrolResult.CommandNotSentToServer;
    }
  }


  public async Task InputTelecontrolPassword(string    password,
                                             PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.InputTelecontrolPassword(password).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task<bool> SwitchStatusManually(TmStatus  status,
                                               bool      alsoBlockManually = false,
                                               PreferApi prefer            = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.SwitchStatusManually(status, alsoBlockManually).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task SetStatusNormalOn(TmStatus  status,
                                      PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.SetStatusNormalOn(status).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task SetStatusNormalOff(TmStatus  status,
                                       PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.SetStatusNormalOff(status).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task ClearStatusNormal(TmStatus  status,
                                      PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.ClearStatusNormal(status).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task<int> GetStatusNormal(TmStatus  status,
                                         PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetStatusNormal(status).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return -1;
    }
  }


  public async Task<bool> BackdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs,
                                          IReadOnlyList<float>    values,
                                          DateTime                time,
                                          PreferApi               prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.BackdateAnalogs(tmAnalogs, values, time).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> PostdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs,
                                          IReadOnlyList<float>    values,
                                          DateTime                time,
                                          PreferApi               prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.PostdateAnalogs(tmAnalogs, values, time).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> SetAnalogManually(TmAnalog  analog,
                                            float     value,
                                            bool      alsoBlockManually = false,
                                            PreferApi prefer            = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.SetAnalogManually(analog, value, alsoBlockManually).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> SetAnalogBackdateManually(TmAnalog  tmAnalog,
                                                    float     value,
                                                    DateTime  time,
                                                    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.SetAnalogBackdateManually(tmAnalog, value, time).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> SetStatusBackdateManually(TmStatus  analog,
                                                    int       status,
                                                    DateTime  time,
                                                    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.SetStatusBackdateManually(analog, status, time).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> SetAnalogTechParameters(TmAnalog  analog, TmAnalogTechParameters parameters,
                                                  PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.SetAnalogTechParameters(analog, parameters).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> SetAlarmValue(TmAlarm   alarm,
                                        float     value,
                                        PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.SetAlarmValue(alarm, value).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task SetTagFlags(TmTag     tag,
                                TmFlags   flags,
                                PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.SetTagFlags(tag, flags).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task ClearTagFlags(TmTag     tag,
                                  TmFlags   flags,
                                  PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.ClearTagFlags(tag, flags).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task SetTagFlagsExplicitly(TmTag     tag,
                                          TmFlags   flags,
                                          PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.SetTagFlagsExplicitly(tag, flags).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task ClearTagFlagsExplicitly(TmTag     tag,
                                            TmFlags   flags,
                                            PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.ClearTagFlagsExplicitly(tag, flags).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task SetTagsFlags(IEnumerable<TmTag> tags,
                                 TmFlags            flags,
                                 PreferApi          prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.SetTagsFlags(tags, flags).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task ClearTagsFlags(IEnumerable<TmTag> tags,
                                   TmFlags            flags,
                                   PreferApi          prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.ClearTagsFlags(tags, flags).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task<bool> AckTag(TmAddr    addr,
                                 PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.AckTag(addr).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task AckAllStatuses(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.AckAllStatuses().ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task AckAllAnalogs(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.AckAllAnalogs().ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task<bool> AckStatus(TmStatus  status,
                                    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.AckStatus(status).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> AckAnalog(TmAnalog  analog,
                                    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.AckAnalog(analog).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }
}