using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<int> GetStatus(int       ch,
                                   int       rtu,
                                   int       point,
                                   PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetStatus(ch, rtu, point).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetStatus(ch, rtu, point).ConfigureAwait(false);
    }
    else
    {
      return -1;
    }
  }


  public async Task SetStatus(int       ch,
                              int       rtu,
                              int       point,
                              int       status,
                              PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.SetStatus(ch, rtu, point, status).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task<float> GetAnalog(int       ch,
                                     int       rtu,
                                     int       point,
                                     PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetAnalog(ch, rtu, point).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetAnalog(ch, rtu, point).ConfigureAwait(false);
    }
    else
    {
      return -1;
    }
  }


  public async Task SetAnalog(int       ch,
                              int       rtu,
                              int       point,
                              float     value,
                              PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.SetAnalog(ch, rtu, point, value).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task<float> GetAccum(int       ch,
                                    int       rtu,
                                    int       point,
                                    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetAccum(ch, rtu, point).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetAccum(ch, rtu, point).ConfigureAwait(false);
    }
    else
    {
      return -1;
    }
  }


  public async Task<float> GetAccumLoad(int       ch,
                                        int       rtu,
                                        int       point,
                                        PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetAccumLoad(ch, rtu, point).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetAccumLoad(ch, rtu, point).ConfigureAwait(false);
    }
    else
    {
      return -1;
    }
  }


  public async Task UpdateStatus(TmStatus  status,
                                 PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      await _tms.UpdateStatus(status).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      await _sql.UpdateStatus(status).ConfigureAwait(false);
    }
    else
    {
      status.IsInit = false;
    }
  }


  public async Task UpdateAnalog(TmAnalog  analog,
                                 PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      await _tms.UpdateAnalog(analog).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      await _sql.UpdateAnalog(analog).ConfigureAwait(false);
    }
    else
    {
      analog.IsInit = false;
    }
  }


  public async Task UpdateAccum(TmAccum   accum,
                                PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      await _tms.UpdateAccum(accum).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      await _sql.UpdateAccum(accum).ConfigureAwait(false);
    }
    else
    {
      accum.IsInit = false;
    }
  }


  public async Task UpdateStatuses(IReadOnlyList<TmStatus> statuses,
                                   PreferApi               prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      await _tms.UpdateStatuses(statuses).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      await _sql.UpdateStatuses(statuses).ConfigureAwait(false);
    }
    else
    {
      // todo IsInit = false;
    }
  }


  public async Task UpdateAnalogs(IReadOnlyList<TmAnalog> analogs,
                                  PreferApi               prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      await _tms.UpdateAnalogs(analogs).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      await _sql.UpdateAnalogs(analogs).ConfigureAwait(false);
    }
    else
    {
      // todo IsInit = false;
    }
  }


  public async Task UpdateAccums(IReadOnlyList<TmAccum> accums,
                                 PreferApi              prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      await _tms.UpdateAccums(accums).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      await _sql.UpdateAccums(accums).ConfigureAwait(false);
    }
    else
    {
      // todo IsInit = false;
    }
  }


  public async Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tags,
                                                     PreferApi            prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      await _tms.UpdateTagsPropertiesAndClassData(tags).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      await _sql.UpdateTagsPropertiesAndClassData(tags).ConfigureAwait(false);
    }
    else
    {
    }
  }


  public async Task UpdateTagPropertiesAndClassData(TmTag     tag,
                                                    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      await _tms.UpdateTagPropertiesAndClassData(tag).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      await _sql.UpdateTagPropertiesAndClassData(tag).ConfigureAwait(false);
    }
    else
    {
    }
  }


  public async Task<IReadOnlyCollection<TmAlarm>> GetAnalogAlarms(TmAnalog  analog,
                                                                  PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      throw new NotImplementedException();
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetAnalogAlarms(analog).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }
}