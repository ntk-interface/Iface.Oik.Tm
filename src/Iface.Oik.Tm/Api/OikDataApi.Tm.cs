using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<int> GetStatus(int       ch,
                                   int       rtu,
                                   int       point,
                                   PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetStatus(ch, rtu, point),
                         () => _sql.GetStatus(ch, rtu, point),
                         () => -1)
            .ConfigureAwait(false);
  }


  public async Task SetStatus(int       ch,
                              int       rtu,
                              int       point,
                              int       status,
                              PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.SetStatus(ch, rtu, point, status),
                  null)
     .ConfigureAwait(false);
  }


  public async Task<float> GetAnalog(int       ch,
                                     int       rtu,
                                     int       point,
                                     PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetAnalog(ch, rtu, point),
                         () => _sql.GetAnalog(ch, rtu, point),
                         () => TmAnalog.InvalidValue)
            .ConfigureAwait(false);
  }


  public async Task SetAnalog(int       ch,
                              int       rtu,
                              int       point,
                              float     value,
                              PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.SetAnalog(ch, rtu, point, value),
                  null)
     .ConfigureAwait(false);
  }


  public async Task<float> GetAccum(int       ch,
                                    int       rtu,
                                    int       point,
                                    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetAccum(ch, rtu, point),
                         () => _sql.GetAccum(ch, rtu, point),
                         () => TmAccum.InvalidValue)
            .ConfigureAwait(false);
  }


  public async Task<float> GetAccumLoad(int       ch,
                                        int       rtu,
                                        int       point,
                                        PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetAccumLoad(ch, rtu, point),
                         () => _sql.GetAccumLoad(ch, rtu, point),
                         () => TmAccum.InvalidValue)
            .ConfigureAwait(false);
  }


  public async Task UpdateStatus(TmStatus  tmStatus,
                                 PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateStatus(tmStatus),
                  () => _sql.UpdateStatus(tmStatus),
                  () => tmStatus.IsInit = false)
     .ConfigureAwait(false);
  }


  public async Task UpdateAnalog(TmAnalog  tmAnalog,
                                 PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateAnalog(tmAnalog),
                  () => _sql.UpdateAnalog(tmAnalog),
                  () => tmAnalog.IsInit = false)
     .ConfigureAwait(false);
  }


  public async Task UpdateAccum(TmAccum   tmAccum,
                                PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateAccum(tmAccum),
                  () => _sql.UpdateAccum(tmAccum),
                  () => tmAccum.IsInit = false)
     .ConfigureAwait(false);
  }


  public async Task UpdateStatuses(IReadOnlyList<TmStatus> tmStatuses,
                                   PreferApi               prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateStatuses(tmStatuses),
                  () => _sql.UpdateStatuses(tmStatuses))
     .ConfigureAwait(false);
    // todo IsInit = false;
  }


  public async Task UpdateStatuses(IReadOnlyDictionary<int, TmStatus> tmStatuses,
                                   PreferApi                          prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateStatuses(tmStatuses),
                  () => _sql.UpdateStatuses(tmStatuses))
     .ConfigureAwait(false);
    // todo IsInit = false;
  }


  public async Task UpdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs,
                                  PreferApi               prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateAnalogs(tmAnalogs),
                  () => _sql.UpdateAnalogs(tmAnalogs))
     .ConfigureAwait(false);
    // todo IsInit = false;
  }


  public async Task UpdateAnalogs(IReadOnlyDictionary<int, TmAnalog> tmAnalogs,
                                  PreferApi                          prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateAnalogs(tmAnalogs),
                  () => _sql.UpdateAnalogs(tmAnalogs))
     .ConfigureAwait(false);
    // todo IsInit = false;
  }


  public async Task UpdateAccums(IReadOnlyList<TmAccum> tmAccums,
                                 PreferApi              prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateAccums(tmAccums),
                  () => _sql.UpdateAccums(tmAccums))
     .ConfigureAwait(false);
    // todo IsInit = false;
  }


  public async Task UpdateAccums(IReadOnlyDictionary<int, TmAccum> tmAccums,
                                 PreferApi                         prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UpdateAccums(tmAccums),
                  () => _sql.UpdateAccums(tmAccums))
     .ConfigureAwait(false);
    // todo IsInit = false;
  }


  public async Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tmTags,
                                                     PreferApi            prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Sql,
                  () => _tms.UpdateTagsPropertiesAndClassData(tmTags),
                  () => _sql.UpdateTagsPropertiesAndClassData(tmTags))
     .ConfigureAwait(false);
  }


  public async Task UpdateStatusesPropertiesAndClassData(IReadOnlyDictionary<int, TmStatus> statuses,
                                                         PreferApi                          prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Sql,
                  () => _tms.UpdateStatusesPropertiesAndClassData(statuses),
                  () => _sql.UpdateStatusesPropertiesAndClassData(statuses))
     .ConfigureAwait(false);
  }


  public async Task UpdateAnalogsPropertiesAndClassData(IReadOnlyDictionary<int, TmAnalog> analogs,
                                                         PreferApi                         prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Sql,
                  () => _tms.UpdateAnalogsPropertiesAndClassData(analogs),
                  () => _sql.UpdateAnalogsPropertiesAndClassData(analogs))
     .ConfigureAwait(false);
  }


  public async Task UpdateAccumsPropertiesAndClassData(IReadOnlyDictionary<int, TmAccum> accums,
                                                        PreferApi                        prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Sql,
                  () => _tms.UpdateAccumsPropertiesAndClassData(accums),
                  () => _sql.UpdateAccumsPropertiesAndClassData(accums))
     .ConfigureAwait(false);
  }


  public async Task UpdateTagPropertiesAndClassData(TmTag     tmTag,
                                                    PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Sql,
                  () => _tms.UpdateTagPropertiesAndClassData(tmTag),
                  () => _sql.UpdateTagPropertiesAndClassData(tmTag))
     .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmAlarm>> GetAnalogAlarms(TmAnalog  analog,
                                                                  PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.GetAnalogAlarms(analog))
            .ConfigureAwait(false);
  }
    
    
  public async Task<string> GetExpressionResult(string    expression,
                                                PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetExpressionResult(expression),
                         null)
            .ConfigureAwait(false);
  }
    
    
  public string GetExpressionResultSync(string    expression,
                                        PreferApi prefer = PreferApi.Auto)
  {
    return ExecuteSync(prefer,
                       PreferApi.Tms,
                       () => _tms.GetExpressionResultSync(expression),
                       null);
  }


  public async Task CreateTmTagNamedSet(string                     name,
                                        TmType                     tmType,
                                        IReadOnlyCollection<TmTag> tmTags,
                                        PreferApi                  prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.CreateTmTagNamedSet(name, tmType, tmTags),
                  null)
     .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmStatusRecord>> GetTmStatusNamedSetUpdatedValues(string name, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetTmStatusNamedSetUpdatedValues(name),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmAnalogRecord>> GetTmAnalogNamedSetUpdatedValues(string name, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetTmAnalogNamedSetUpdatedValues(name),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmAccumRecord>> GetTmAccumNamedSetUpdatedValues(string name, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetTmAccumNamedSetUpdatedValues(name),
                         null)
            .ConfigureAwait(false);
  }


  public async Task DeleteTmTagNamedSet(string    name,
                                        TmType    tmType,
                                        PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.DeleteTmTagNamedSet(name, tmType),
                  null)
     .ConfigureAwait(false);
  }
}