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
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetStatusFromRetro(ch, rtu, point, time).ConfigureAwait(false);
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


  public async Task<ITmAnalogRetro> GetAnalogFromRetro(int       ch,
                                                       int       rtu,
                                                       int       point,
                                                       DateTime  time,
                                                       int       retroNum = 0,
                                                       PreferApi prefer   = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetAnalogFromRetro(ch, rtu, point, time, retroNum).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return TmAnalogRetro.UnreliableValue;
    }
  }


  public async Task<ITmAccumRetro> GetAccumFromRetro(int       ch,
                                                     int       rtu,
                                                     int       point,
                                                     DateTime  time,
                                                     PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetAccumFromRetro(ch, rtu, point, time).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return TmAccumRetro.UnreliableValue;
    }
  }


  public async Task UpdateStatusesFromRetro(IReadOnlyList<TmStatus> statuses,
                                            DateTime                time,
                                            PreferApi               prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.UpdateStatusesFromRetro(statuses, time).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
    }
    else
    {
      // todo IsInit = false;
    }
  }


  public async Task UpdateAnalogsFromRetro(IReadOnlyList<TmAnalog> analogs,
                                           DateTime                time,
                                           int                     retroNum = 0,
                                           PreferApi               prefer   = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.UpdateAnalogsFromRetro(analogs, time, retroNum).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
    }
    else
    {
      // todo IsInit = false;
    }
  }


  public async Task UpdateAccumsFromRetro(IReadOnlyList<TmAccum> accums,
                                          DateTime               time,
                                          PreferApi              prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.UpdateAccumsFromRetro(accums, time).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
    }
    else
    {
      // todo IsInit = false;
    }
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro[]>> GetAnalogsMicroSeries(
    IReadOnlyList<TmAnalog> analogs,
    PreferApi               prefer = PreferApi.Auto)
  {
    if (!_serverFeatures.AreMicroSeriesEnabled)
    {
      return null;
    }

    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetAnalogsMicroSeries(analogs).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetAnalogsMicroSeries(analogs).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmRetroInfo>> GetRetrosInfo(TmType    tmType,
                                                                    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetRetrosInfo(tmType).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetro(TmAnalog  analog,
                                                                        long      utcStartTime,
                                                                        int       count,
                                                                        int       step,
                                                                        int       retroNum = 0,
                                                                        PreferApi prefer   = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetAnalogRetro(analog, utcStartTime, count, step, retroNum).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetro(TmAnalog            analog,
                                                                        TmAnalogRetroFilter filter,
                                                                        int                 retroNum = 0,
                                                                        PreferApi           prefer   = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetAnalogRetro(analog, filter, retroNum).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return null;
    }
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

    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetImpulseArchiveInstant(analog, filter).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return null;
    }
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

    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetImpulseArchiveAverage(analog, filter).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return null;
    }
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

    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetImpulseArchiveSlices(analog, filter).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return null;
    }
  }
}