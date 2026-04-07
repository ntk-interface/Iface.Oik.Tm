using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<IReadOnlyCollection<TmChannel>> GetTmTreeChannels(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetTmTreeChannels().ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetTmTreeChannels().ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmRtu>> GetTmTreeRtus(int       channelId,
                                                              PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetTmTreeRtus(channelId).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetTmTreeRtus(channelId).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmStatus>> GetTmTreeStatuses(int       channelId,
                                                                     int       rtuId,
                                                                     PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetTmTreeStatuses(channelId, rtuId).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetTmTreeStatuses(channelId, rtuId).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmAnalog>> GetTmTreeAnalogs(int       channelId,
                                                                    int       rtuId,
                                                                    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetTmTreeAnalogs(channelId, rtuId).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetTmTreeAnalogs(channelId, rtuId).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
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
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetChannelName(channelId).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetChannelName(channelId).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<string> GetRtuName(int       channelId,
                                       int       rtuId,
                                       PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetRtuName(channelId, rtuId).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetRtuName(channelId, rtuId).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmClassStatus>> GetStatusesClasses(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetStatusesClasses().ConfigureAwait(false);
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


  public async Task<IReadOnlyCollection<TmClassAnalog>> GetAnalogsClasses(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetAnalogsClasses().ConfigureAwait(false);
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


  public async Task<IReadOnlyCollection<TmStatus>> LookupStatuses(TmStatusFilter filter,
                                                                  PreferApi      prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      throw new NotImplementedException();
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.LookupStatuses(filter).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmAnalog>> LookupAnalogs(TmAnalogFilter filter,
                                                                 PreferApi      prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      throw new NotImplementedException();
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.LookupAnalogs(filter).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmTag>> GetTagsByGroup(TmType    tmType,
                                                               string    groupName,
                                                               PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetTagsByGroup(tmType, groupName).ConfigureAwait(false);
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