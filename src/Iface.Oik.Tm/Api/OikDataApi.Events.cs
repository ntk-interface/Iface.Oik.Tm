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
    return await Execute(prefer,
                         PreferApi.Sql,
                         () => _tms.GetEventsArchiveByElix(filter),
                         () => _sql.GetEventsArchive(filter))
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmUserAction>> GetUserActionsArchive(TmEventFilter filter,
                                                                             PreferApi     prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.GetUserActionsArchive(filter))
            .ConfigureAwait(false);
  }


  public async Task<TmEventElix> GetCurrentEventsElix(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetCurrentEventsElix(),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<TmEventElix> GetRecentEventsElix(int          recentCount,
                                                     int          recentHours = 24,
                                                     TmEventTypes eventTypes  = TmEventTypes.Any,
                                                     PreferApi    prefer      = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.GetRecentEventsElix(recentCount, recentHours, eventTypes),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<(IReadOnlyCollection<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix,
    PreferApi                                                                                 prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         () => _tms.GetCurrentEvents(elix),
                         () => _sql.GetCurrentEvents(elix))
            .ConfigureAwait(false);
  }


  public async Task<bool> UpdateAckedEventsIfAny(IReadOnlyList<TmEvent> tmEvents,
                                                 PreferApi              prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.UpdateAckedEventsIfAny(tmEvents),
                         () => _sql.UpdateAckedEventsIfAny(tmEvents))
            .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmTag>> GetTagsWithBlockedEvents(PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Sql,
                         null,
                         () => _sql.GetTagsWithBlockedEvents())
            .ConfigureAwait(false);
  }


  public async Task<bool> AckEvent(TmEvent   tmEvent,
                                   PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.AckEvent(tmEvent),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> AckEvents(IReadOnlyList<TmEvent> tmEvents,
                                    PreferApi              prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.AckEvents(tmEvents),
                         null)
            .ConfigureAwait(false);
  }


  public async Task AddStringToEventLog(string    str,
                                        TmAddr    tmAddr = null,
                                        DateTime? time   = null,
                                        PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.AddStringToEventLog(str, tmAddr, time),
                  null)
     .ConfigureAwait(false);
  }


  public async Task AddStringToEventLogEx(DateTime?                 time,
                                          TmEventImportances        importances,
                                          TmEventLogExtendedSources source,
                                          string                    message,
                                          string                    binString = "",
                                          TmAddr                    tmAddr    = null,
                                          PreferApi                 prefer    = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.AddStringToEventLogEx(time, importances, source, message, binString, tmAddr),
                  null)
     .ConfigureAwait(false);
  }


  public async Task AddTmaRelatedStringToEventLog(string             message,
                                                  TmAddr             tmAddr,
                                                  TmEventImportances importances = TmEventImportances.Imp0,
                                                  DateTime?          time        = null,
                                                  PreferApi          prefer      = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.AddTmaRelatedStringToEventLog(message, tmAddr, importances, time),
                  null)
     .ConfigureAwait(false);
  }


  public async Task<bool> BlockTagEventsTemporarily(TmTag     tmTag,
                                                    int       minutesToBlock,
                                                    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.BlockTagEventsTemporarily(tmTag, minutesToBlock),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> BlockTagEventsTemporarily(TmTag     tmTag,
                                                    DateTime  endBlockTime,
                                                    PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.BlockTagEventsTemporarily(tmTag, endBlockTime),
                         null)
            .ConfigureAwait(false);
  }


  public async Task UnblockTagEvents(TmTag     tmTag,
                                     PreferApi prefer = PreferApi.Auto)
  {
    await Execute(prefer,
                  PreferApi.Tms,
                  () => _tms.UnblockTagEvents(tmTag),
                  null)
     .ConfigureAwait(false);
  }
}