using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<IReadOnlyCollection<TmEvent>> GetEventsArchive(TmEventFilter filter,
                                                                   PreferApi     prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetEventsArchiveByElix(filter).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetEventsArchive(filter).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<IReadOnlyCollection<TmUserAction>> GetUserActionsArchive(TmEventFilter filter,
                                                                             PreferApi     prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return null;
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetUserActionsArchive(filter).ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<TmEventElix> GetCurrentEventsElix(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetCurrentEventsElix().ConfigureAwait(false);
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


  public async Task<TmEventElix> GetRecentEventsElix(int          recentCount,
                                                     int          recentHours = 24,
                                                     TmEventTypes eventTypes  = TmEventTypes.Any,
                                                     PreferApi    prefer      = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetRecentEventsElix(recentCount, recentHours, eventTypes).ConfigureAwait(false);
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


  public async Task<(IReadOnlyCollection<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix,
    PreferApi                                                                                 prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.GetCurrentEvents(elix).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetCurrentEvents(elix).ConfigureAwait(false);
    }
    else
    {
      return (null, null);
    }
  }


  public async Task<bool> UpdateAckedEventsIfAny(IReadOnlyList<TmEvent> tmEvents,
                                                 PreferApi              prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      return await _tms.UpdateAckedEventsIfAny(tmEvents).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.UpdateAckedEventsIfAny(tmEvents).ConfigureAwait(false);
    }
    else
    {
      return false;
    }
  }


  public async Task<IReadOnlyCollection<TmTag>> GetTagsWithBlockedEvents(PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Sql, isTmsImplemented: false, isSqlImplemented: true);
    if (api == ApiSelection.Tms)
    {
      throw new NotImplementedException();
    }
    else if (api == ApiSelection.Sql)
    {
      return await _sql.GetTagsWithBlockedEvents().ConfigureAwait(false);
    }
    else
    {
      return null;
    }
  }


  public async Task<bool> AckEvent(TmEvent   tmEvent,
                                   PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.AckEvent(tmEvent).ConfigureAwait(false);
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


  public async Task<bool> AckEvents(IReadOnlyList<TmEvent> tmEvents,
                                    PreferApi              prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.AckEvents(tmEvents).ConfigureAwait(false);
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


  public async Task AddStringToEventLog(string    str,
                                        TmAddr    tmAddr = null,
                                        DateTime? time   = null,
                                        PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.AddStringToEventLog(str, tmAddr, time).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task AddStringToEventLogEx(DateTime?                 time,
                                          TmEventImportances        importances,
                                          TmEventLogExtendedSources source,
                                          string                    message,
                                          string                    binString = "",
                                          TmAddr                    tmAddr    = null,
                                          PreferApi                 prefer    = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.AddStringToEventLogEx(time, importances, source, message, binString, tmAddr).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task AddTmaRelatedStringToEventLog(string             message,
                                                  TmAddr             tmAddr,
                                                  TmEventImportances importances = TmEventImportances.Imp0,
                                                  DateTime?          time        = null,
                                                  PreferApi          prefer      = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.AddTmaRelatedStringToEventLog(message, tmAddr, importances, time).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }


  public async Task<bool> BlockTagEventsTemporarily(TmTag     tmTag,
                                                    int       minutesToBlock,
                                                    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.BlockTagEventsTemporarily(tmTag, minutesToBlock).ConfigureAwait(false);
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


  public async Task<bool> BlockTagEventsTemporarily(TmTag     tmTag,
                                                    DateTime  endBlockTime,
                                                    PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.BlockTagEventsTemporarily(tmTag, endBlockTime).ConfigureAwait(false);
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


  public async Task UnblockTagEvents(TmTag     tmTag,
                                     PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      await _tms.UnblockTagEvents(tmTag).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
    }
  }
}