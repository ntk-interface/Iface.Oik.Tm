using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<int> GetStatusFromRetro(int       ch,
                                            int       rtu,
                                            int       point,
                                            DateTime  time,
                                            PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetStatusFromRetro(ch, rtu, point, time),
                         null,
                         () => -1)
            .ConfigureAwait(false);
  }


  public async Task<ITmAnalogRetro> GetAnalogFromRetro(int       ch,
                                                       int       rtu,
                                                       int       point,
                                                       DateTime  time,
                                                       int       retroNum = 0,
                                                       PreferApi prefer   = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetAnalogFromRetro(ch, rtu, point, time, retroNum),
                         null,
                         () => TmAnalogRetro.UnreliableValue)
            .ConfigureAwait(false);
  }


  public async Task<ITmAccumRetro> GetAccumFromRetro(int       ch,
                                                     int       rtu,
                                                     int       point,
                                                     DateTime  time,
                                                     PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetAccumFromRetro(ch, rtu, point, time),
                         null,
                         () => TmAccumRetro.UnreliableValue)
            .ConfigureAwait(false);
  }


  public async Task UpdateStatusesFromRetro(IReadOnlyList<TmStatus> statuses,
                                            DateTime                time,
                                            PreferApi               prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateStatusesFromRetro(statuses, time),
                  null,
                  () => { /* todo IsInit = false; */ })
     .ConfigureAwait(false);
  }


  public async Task UpdateAnalogsFromRetro(IReadOnlyList<TmAnalog> analogs,
                                           DateTime                time,
                                           int                     retroNum = 0,
                                           PreferApi               prefer   = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateAnalogsFromRetro(analogs, time, retroNum),
                  null,
                  () => { /* todo IsInit = false; */ })
     .ConfigureAwait(false);
  }


  public async Task UpdateAccumsFromRetro(IReadOnlyList<TmAccum> accums,
                                          DateTime               time,
                                          PreferApi              prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateAccumsFromRetro(accums, time),
                  null,
                  () => { /* todo IsInit = false; */ })
     .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro[]>> GetAnalogsMicroSeries(
    IReadOnlyList<TmAnalog> analogs, 
    PreferApi               prefer = PreferApi.Auto)
  {
    if (!_serverFeatures.AreMicroSeriesEnabled)
    {
      return null;
    }
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetAnalogsMicroSeries(analogs),
                         () => _sql.GetAnalogsMicroSeries(analogs))
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmRetroInfo>> GetRetrosInfo(TmType    tmType,
                                                                    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetRetrosInfo(tmType),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetro(TmAnalog  analog,
                                                                        long      utcStartTime,
                                                                        int       count,
                                                                        int       step,
                                                                        int       retroNum = 0,
                                                                        PreferApi prefer   = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetAnalogRetro(analog, utcStartTime, count, step, retroNum),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetro(TmAnalog            analog,
                                                                        TmAnalogRetroFilter filter,
                                                                        int                 retroNum = 0,
                                                                        PreferApi           prefer   = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetAnalogRetro(analog, filter, retroNum),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveInstant(
    TmAnalog            analog,
    TmAnalogRetroFilter filter,
    PreferApi           prefer = PreferApi.Auto)
  {
    if (!_serverFeatures.IsImpulseArchiveEnabled)
    {
      return null;
    }
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetImpulseArchiveInstant(analog, filter),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveAverage(
    TmAnalog            analog,
    TmAnalogRetroFilter filter,
    PreferApi           prefer = PreferApi.Auto)
  {
    if (!_serverFeatures.IsImpulseArchiveEnabled)
    {
      return null;
    }
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetImpulseArchiveAverage(analog, filter),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveSlices(
    TmAnalog            analog,
    TmAnalogRetroFilter filter,
    PreferApi           prefer = PreferApi.Auto)
  {
    if (!_serverFeatures.IsImpulseArchiveEnabled)
    {
      return null;
    }
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetImpulseArchiveSlices(analog, filter),
                         null)
            .ConfigureAwait(false);
  }
}