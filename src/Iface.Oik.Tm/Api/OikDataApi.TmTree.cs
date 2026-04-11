using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<IReadOnlyCollection<TmChannel>> GetTmTreeChannels(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         () => _tms.GetTmTreeChannels(),
                         () => _sql.GetTmTreeChannels(),
                         () => null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmRtu>> GetTmTreeRtus(int       channelId,
                                                              PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         () => _tms.GetTmTreeRtus(channelId),
                         () => _sql.GetTmTreeRtus(channelId),
                         () => null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmStatus>> GetTmTreeStatuses(int       channelId,
                                                                     int       rtuId,
                                                                     PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         () => _tms.GetTmTreeStatuses(channelId, rtuId),
                         () => _sql.GetTmTreeStatuses(channelId, rtuId),
                         () => null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmAnalog>> GetTmTreeAnalogs(int       channelId,
                                                                    int       rtuId,
                                                                    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         () => _tms.GetTmTreeAnalogs(channelId, rtuId),
                         () => _sql.GetTmTreeAnalogs(channelId, rtuId),
                         () => null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmAccum>> GetTmTreeAccums(int       channelId,
                                                                  int       rtuId,
                                                                  PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetTmTreeAccums(channelId, rtuId).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetTmTreeAccums(channelId, rtuId).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<string> GetChannelName(int       channelId,
                                           PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetChannelName(channelId),
                         () => _sql.GetChannelName(channelId),
                         () => string.Empty)
            .ConfigureAwait(false);
  }


  public async Task<string> GetRtuName(int       channelId,
                                       int       rtuId,
                                       PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetRtuName(channelId, rtuId),
                         () => _sql.GetRtuName(channelId, rtuId),
                         () => string.Empty)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmClassStatus>> GetStatusesClasses(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetStatusesClasses(),
                         null,
                         () => null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmClassAnalog>> GetAnalogsClasses(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetAnalogsClasses(),
                         null,
                         () => null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmStatus>> LookupStatuses(TmStatusFilter filter,
                                                                  PreferApi      prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.LookupStatuses(filter),
                         () => null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmAnalog>> LookupAnalogs(TmAnalogFilter filter,
                                                                 PreferApi      prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.LookupAnalogs(filter),
                         () => null)
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmTag>> GetTagsByGroup(TmType    tmType,
                                                               string    groupName,
                                                               PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetTagsByGroup(tmType, groupName),
                         null,
                         () => null)
            .ConfigureAwait(false);
  }
}