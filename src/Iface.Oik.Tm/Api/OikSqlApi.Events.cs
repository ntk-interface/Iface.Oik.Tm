using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api;

public partial class OikSqlApi
{
  public async Task<IReadOnlyCollection<TmEvent>> GetEventsArchive(TmEventFilter filter)
  {
    if (filter == null) return null; // TODO throw

    if (filter.AreTmEventsForbidden)
    {
      return Array.Empty<TmEvent>();
    }

    return HasAnyNoteFilter(filter)
             ? await GetEventsArchiveWithNotesFilter(filter).ConfigureAwait(false)
             : await GetEventsArchiveWithoutNotesFilter(filter).ConfigureAwait(false);
  }


  private async Task<IReadOnlyCollection<TmEvent>> GetEventsArchiveWithoutNotesFilter(TmEventFilter filter)
  {
    var (where, parameters) = BuildTmEventsWithoutNotesWhereClauseAndParameters(filter);

    var whereClause = where.Count > 0
                        ? $"WHERE {string.Join(" AND ", where)}"
                        : string.Empty;

    var limitClause = filter.OutputLimit > 0
                        ? $" LIMIT {filter.OutputLimit}"
                        : string.Empty;

    var events = new List<TmEvent>();
    try
    {
      using var sql = _createOikSqlConnection();
      sql.Label = "ArchEvents";
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = $@"SELECT events.elix, update_time, 
                                  rec_text, name, rec_state_text, rec_type, rec_type_name, user_name, importance, 
                                  tma, tma_str, tm_type_name, tm_type, class_id, v_val, alarm_active, v_code, v_s2, flags, ts_add_flags,
                                  ack_time, ack_user,
                                  note_comment, note_time, note_tag_id
                           FROM oik_event_log AS events 
                               LEFT JOIN oik_event_log_notes AS notes on events.elix = notes.elix
                           {whereClause}
                           ORDER BY events.update_time
                           {limitClause}";

      var dtos = await sql.DbConnection
                          .QueryAsync<TmEventDto>(commandText, parameters)
                          .ConfigureAwait(false);

      dtos.ForEach((dto, idx) =>
      {
        var tmEvent = TmEvent.CreateFromDto(dto);
        tmEvent.Num = idx + 1;
        events.Add(tmEvent);
      });

      return events;
    }
    catch (Exception ex)
    {
      HandleException(ex);
      return null; // TODO throw
    }
  }


  private async Task<IReadOnlyCollection<TmEvent>> GetEventsArchiveWithNotesFilter(TmEventFilter filter)
  {
    using var sql = _createOikSqlConnection();
    sql.Label = "ArchEvents-Notes";
    await sql.OpenAsync().ConfigureAwait(false);

    var (notesWhere, notesParameters) = BuildTmEventsNotesOnlyWhereClauseAndParameters(filter);
    var notesWhereClause = notesWhere.Count > 0
                             ? $"WHERE {string.Join(" AND ", notesWhere)}"
                             : string.Empty;

    var notesCommandText = $"SELECT elix FROM oik_event_log_notes {notesWhereClause}";
    var elixList = (await sql.DbConnection
                             .QueryAsync<byte[]>(notesCommandText, notesParameters)
                             .ConfigureAwait(false)).ToList();
    if (elixList.Count == 0)
    {
      return Array.Empty<TmEvent>();
    }

    var (where, parameters) = BuildTmEventsWithoutNotesWhereClauseAndParameters(filter);
    where.Insert(0, "events.elix = @ElixFlatList");
    parameters.Add("@ElixFlatList", TmEventElix.FlattenElixList(elixList), DbType.Binary);

    var whereClause = where.Count > 0
                        ? $"WHERE {string.Join(" AND ", where)}"
                        : string.Empty;

    var limitClause = filter.OutputLimit > 0
                        ? $" LIMIT {filter.OutputLimit}"
                        : string.Empty;

    var events = new List<TmEvent>();
    try
    {
      var commandText = $@"SELECT events.elix, update_time, 
                                  rec_text, name, rec_state_text, rec_type, rec_type_name, user_name, importance, 
                                  tma, tma_str, tm_type_name, tm_type, class_id, v_val, alarm_active, v_code, v_s2, flags, ts_add_flags,
                                  ack_time, ack_user,
                                  note_comment, note_time, note_tag_id
                           FROM oik_event_log_elix AS events
                             LEFT JOIN oik_event_log_notes AS notes ON events.elix = notes.elix
                           {whereClause}
                           ORDER BY events.update_time
                           {limitClause}";

      var dtos = await sql.DbConnection
                          .QueryAsync<TmEventDto>(commandText, parameters)
                          .ConfigureAwait(false);

      dtos.ForEach((dto, idx) =>
      {
        var tmEvent = TmEvent.CreateFromDto(dto);
        tmEvent.Num = idx + 1;
        events.Add(tmEvent);
      });

      return events;
    }
    catch (Exception ex)
    {
      HandleException(ex);
      return null; // TODO throw
    }
  }


  private static (List<string>, DynamicParameters) BuildTmEventsWithoutNotesWhereClauseAndParameters(TmEventFilter filter)
  {
    var where      = new List<string>();
    var parameters = new DynamicParameters();
    
    if (filter.StartTime != null)
    {
      where.Add("events.update_time >= @StartTime");
      parameters.Add("@StartTime", filter.StartTime, DbType.DateTime);
    }
    if (filter.EndTime != null)
    {
      where.Add("events.update_time <= @EndTime");
      parameters.Add("@EndTime", filter.EndTime, DbType.DateTime);
    }
    if (filter.Types != TmEventTypes.None && filter.Types != TmEventTypes.Any)
    {
      where.Add("events.rec_type & @Types > 0");
      parameters.Add("@Types", (short)filter.Types, DbType.Int16);
    }
    if (filter.Importances != TmEventImportances.None && filter.Importances != TmEventImportances.Any)
    {
      var importances = Enumerable.Range(0, 4)
                                  .Where(i => filter.Importances.HasFlag((TmEventImportances)(1 << i)))
                                  .ToArray();
      if (importances.Length > 0)
      {
        where.Add($"events.importance IN ({string.Join(",", importances)})");
      }
    }
    if (filter.TmStatusClassIdList?.Count > 0)
    {
      where.Add($"((events.tm_type != {(int)TmNativeDefs.TmDataTypes.Status}) OR events.class_id IN ({string.Join(",", filter.TmStatusClassIdList)}))");
    }
    if (filter.TmAddrList.Count > 0)
    {
      where.Add("events.fulltma = ANY(@FullTmaArray)");
      parameters.Add("@FullTmaArray", filter.TmAddrList.Select(t => t.ToFullTma()).ToArray());
    }
    if (filter.ChannelAndRtuCollection?.Count > 0)
    {
      var tmaConditions = filter.ChannelAndRtuCollection
                                .SelectMany(chAndRtu =>
                                 {
                                   var channelId = chAndRtu.Key;
                                   var rtuSet    = chAndRtu.Value;
                                   if (rtuSet == null) // все КП в канале
                                   {
                                     var (start, end) = TmChannel.GetTmaRange(channelId);
                                     return new[] { $"(events.tma >= {start} AND events.tma <= {end})" };
                                   }
                                   return rtuSet.Select(rtuId => // конкретный КП
                                   {
                                     var (start, end) = TmRtu.GetTmaRange(channelId, rtuId);
                                     return $"(events.tma >= {start} AND events.tma <= {end})";
                                   });
                                 })
                                .ToList();
      where.Add($"({string.Join(" OR ", tmaConditions)})");
    }
    if (filter.ExcludeFromReserve)
    {
      where.Add("(events.ts_add_flags IS NULL OR get_bit(events.ts_add_flags,4) != 1)");
    }

    return (where, parameters);
  }


  private static (List<string>, DynamicParameters) BuildTmEventsNotesOnlyWhereClauseAndParameters(TmEventFilter filter)
  {
    var where      = new List<string>();
    var parameters = new DynamicParameters();

    if (filter.HasNoteComment)
    {
      where.Add("note_comment <> ''");
    }
    if (filter.HasNoteTime)
    {
      where.Add("note_time IS NOT NULL");
    }
    if (filter.NoteTagIds.Count > 0)
    {
      where.Add("note_tag_id = ANY(@NoteTagIdArray)");
      parameters.Add("@NoteTagIdArray", filter.NoteTagIds.ToArray());
    }

    return (where, parameters);
  }


  private static bool HasAnyNoteFilter(TmEventFilter filter)
  {
    return filter.HasNoteComment || filter.HasNoteTime || filter.NoteTagIds.Count > 0;
  }


  public async Task<IReadOnlyCollection<TmUserAction>> GetUserActionsArchive(TmEventFilter filter)
  {
    if (filter == null) return null; // TODO throw

    if (filter.AreUserActionsForbidden ||
        !filter.TmStatusClassIdList.IsNullOrEmpty())
    {
      return Array.Empty<TmUserAction>(); // TODO временно обнуляем при задании ТМ-адресов или классов
    }

    var (where, parameters) = PrepareUserActionsWhereClauseAndParameters(filter);
    
    var whereClause = where.Count > 0
                        ? $"WHERE {string.Join(" AND ", where)}"
                        : string.Empty;
    
    var limitClause = filter.OutputLimit > 0
                        ? $" LIMIT {filter.OutputLimit}"
                        : string.Empty;

    var userActions = new List<TmUserAction>();
    try
    {
      using var sql = _createOikSqlConnection();
      sql.Label = "UserActions";
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = $@"SELECT log.id, time, action, category, state, importance, text, user_name,
                                  tma, extra_id, extra_int, extra_text,
                                  ack_time, ack_user,
                                  note_tag_id, note_comment, note_time
                           FROM oik_user_actions_log AS log
                             LEFT JOIN oik_user_actions_log_notes AS notes ON log.id = notes.id
                           {whereClause}
                           ORDER BY log.time
                           {limitClause}";

      var dtos = await sql.DbConnection
                          .QueryAsync<TmUserActionDto>(commandText, parameters)
                          .ConfigureAwait(false);

      dtos.ForEach((dto, idx) =>
      {
        var userAction = TmUserAction.CreateFromDto(dto);
        userAction.Num = idx + 1;
        userActions.Add(userAction);
      });

      return userActions;
    }
    catch (Exception ex)
    {
      HandleException(ex);
      return null; // TODO throw
    }
  }


  private static (List<string>, DynamicParameters) PrepareUserActionsWhereClauseAndParameters(TmEventFilter filter)
  {
    var where      = new List<string>();
    var parameters = new DynamicParameters();
    
    if (filter.StartTime != null)
    {
      where.Add("log.time >= @StartTime");
      parameters.Add("@StartTime", filter.StartTime, DbType.DateTime);
    }
    if (filter.EndTime != null)
    {
      where.Add("log.time <= @EndTime");
      parameters.Add("@EndTime", filter.EndTime, DbType.DateTime);
    }
    if (filter.Categories?.Count > 0)
    {
      where.Add("log.category = ANY(@CategoriesArray)");
      parameters.Add("@CategoriesArray", filter.Categories.Cast<int>().ToArray());
    }
    if (filter.Importances != TmEventImportances.None && filter.Importances != TmEventImportances.Any)
    {
      var importances = Enumerable.Range(0, 4)
                                  .Where(i => filter.Importances.HasFlag((TmEventImportances)(1 << i)))
                                  .ToArray();
      if (importances.Length > 0)
      {
        where.Add($"log.importance IN ({string.Join(",", importances)})");
      }
    }
    if (filter.TmAddrList.Count > 0)
    {
      where.Add("log.tma = ANY(@FullTmaArray)");
      parameters.Add("@FullTmaArray", filter.TmAddrList.Select(t => t.ToFullTma()).ToArray());
    }
    if (filter.ChannelAndRtuCollection?.Count > 0)
    {
      const string fullTmaMask = "4294967295"; // 0xFFFFFFFF
      
      var tmaConditions = filter.ChannelAndRtuCollection
                                .SelectMany(chAndRtu =>
                                 {
                                   var channelId = chAndRtu.Key;
                                   var rtuSet    = chAndRtu.Value;
                                   if (rtuSet == null) // все КП в канале
                                   {
                                     var (start, end) = TmChannel.GetTmaRange(channelId);
                                     return new[] { $"((log.tma & {fullTmaMask}) >= {start} AND (log.tma & {fullTmaMask}) <= {end})" }; // fulltma->tma
                                   }
                                   return rtuSet.Select(rtuId => // конкретный КП
                                   {
                                     var (start, end) = TmRtu.GetTmaRange(channelId, rtuId);
                                     return $"((log.tma & {fullTmaMask}) >= {start} AND (log.tma & {fullTmaMask}) <= {end})";
                                   });
                                 })
                                .ToList();
      where.Add($"({string.Join(" OR ", tmaConditions)})");
    }

    if (filter.HasNoteComment)
    {
      where.Add("notes.note_comment <> ''");
    }
    if (filter.HasNoteTime)
    {
      where.Add("notes.note_time IS NOT NULL");
    }
    if (filter.NoteTagIds.Count > 0)
    {
      where.Add("notes.note_tag_id = ANY(@NoteTagIdArray)");
      parameters.Add("@NoteTagIdArray", filter.NoteTagIds.ToArray());
    }

    return (where, parameters);
  }


  public async Task<(IReadOnlyCollection<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix)
  {
    if (elix == null)
    {
      return (null, null);
    }

    var events = new List<TmEvent>();
    var newElix  = elix;
    try
    {
      using var sql = _createOikSqlConnection();
      sql.Label = "CurrEvents";
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT elix, update_time, 
                                 rec_text, name, rec_state_text, rec_type, rec_type_name, user_name, importance, 
                                 tma, tma_str, tm_type_name, tm_type, class_id, v_val, alarm_active, v_code, v_s2, flags, ts_add_flags,
                                 ack_time, ack_user
                          FROM oik_event_log_elix
                          WHERE elix > @Elix
                          ORDER BY update_time";
      var parameters = new { Elix = elix.ToByteArray() };
      
      var dtos = await sql.DbConnection
                          .QueryAsync<TmEventDto>(commandText, parameters)
                          .ConfigureAwait(false);

      dtos.ForEach(dto =>
      {
        var tmEvent = TmEvent.CreateFromDto(dto);

        if (tmEvent.Elix?.CompareTo(newElix) > 0)
        {
          newElix = tmEvent.Elix;
        }

        events.Add(tmEvent);
      });

      return (events, newElix);
    }
    catch (Exception ex)
    {
      HandleException(ex);
      return (null, elix);
    }
  }


  public async Task<bool> UpdateAckedEventsIfAny(IReadOnlyList<TmEvent> tmEvents)
  {
    if (tmEvents.IsNullOrEmpty()) return false;

    var whereQueryStringArray          = new List<string>(tmEvents.Count);
    var whereQueryParametersDictionary = new Dictionary<string, object>(tmEvents.Count);
    var tmEventsDictionary             = new Dictionary<TmEventElix, TmEvent>(tmEvents.Count);

    for (var i = 0; i < tmEvents.Count; i++)
    {
      whereQueryStringArray.Add($"(elix = @Elix{i})");
      whereQueryParametersDictionary.Add($"@Elix{i}", tmEvents[i].Elix.ToByteArray());
      tmEventsDictionary.Add(tmEvents[i].Elix, tmEvents[i]);
    }

    try
    {
      bool changesFound = false;

      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = $@"SELECT elix, ack_time, ack_user
                           FROM oik_event_log_elix
                           WHERE {string.Join(" OR ", whereQueryStringArray)}";
      var parameters = new DynamicParameters(whereQueryParametersDictionary);
      
      var dtos = await sql.DbConnection
                          .QueryAsync<TmEventDto>(commandText, parameters)
                          .ConfigureAwait(false);

      foreach (var dto in dtos)
      {
        if (DateUtil.NullIfEpoch(dto.AckTime) != null)
        {
          if (tmEventsDictionary.TryGetValue(TmEventElix.CreateFromByteArray(dto.Elix), out var initialEvent))
          {
            initialEvent.AckTime = dto.AckTime;
            initialEvent.AckUser = dto.AckUser;
            changesFound         = true;
          }
        }
      }

      return changesFound;
    }
    catch (Exception ex)
    {
      HandleException(ex);
      return false;
    }
  }
    

  public async Task<IReadOnlyCollection<TmTag>> GetTagsWithBlockedEvents()
  {
    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT unblktime, name, tm_type, ch, rtu, point
                          FROM oik_event_blocks
                          WHERE unblktime > NOW()
                          ORDER BY unblktime DESC";
      
      var dtos = await sql.DbConnection
                          .QueryAsync<TmTagWithBlockedEventsDto>(commandText)
                          .ConfigureAwait(false);
          
      return dtos.Select(dto => dto.MapToTmTag())
                 .ToList();
    }
    catch (Exception ex)
    {
      HandleException(ex);
      return null;
    }
  }
}