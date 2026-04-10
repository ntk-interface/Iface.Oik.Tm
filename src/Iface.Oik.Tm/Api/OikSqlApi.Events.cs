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
using Npgsql;

namespace Iface.Oik.Tm.Api;

public partial class OikSqlApi
{
  public async Task<IReadOnlyCollection<TmEvent>> GetEventsArchive(TmEventFilter filter)
  {
    if (filter == null) return null; //???

    if (filter.AreTmEventsForbidden)
    {
      return Array.Empty<TmEvent>();
    }

    var whereBeg                      = GetWhereEventStartTime(filter);
    var whereEnd                      = GetWhereEventEndTime(filter);
    var whereEventTypes               = GetWhereEventTypes(filter);
    var whereEventImportances         = GetWhereEventImportances(filter);
    var whereEventTmStatusClassIds    = GetWhereEventTmStatusClassIds(filter);
    var whereEventChannelsAndRtus     = GetWhereEventChannelsAndRtus(filter);
    var whereEventTmAddr              = GetWhereEventTmAddr(filter);
    var whereEventFromReserveExcluded = GetWhereEventFromReserveExcluded(filter);
    var limit                         = GetEventsOutputLimit(filter);

    try
    {
      var events = new List<TmEvent>();
      using (var sql = _createOikSqlConnection())
      {
        sql.Label = "ArchEvents";
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = $@"SELECT elix, update_time, 
                                rec_text, name, rec_state_text, rec_type, rec_type_name, user_name, importance, 
                                tma, tma_str, tm_type_name, tm_type, class_id, v_val, alarm_active, v_code, v_s2, flags, ts_add_flags,
                                ack_time, ack_user
            FROM oik_event_log
            WHERE 1=1 {whereBeg}{whereEnd}{whereEventTypes}{whereEventImportances}{whereEventTmStatusClassIds}{whereEventTmAddr}{whereEventChannelsAndRtus}{whereEventFromReserveExcluded}
            ORDER BY update_time
            {limit}";

        var parameters = new DynamicParameters();
        if (!whereBeg.IsNullOrEmpty())
        {
          parameters.Add("@StartTime", filter.StartTime, DbType.DateTime);
        }

        if (!whereEnd.IsNullOrEmpty())
        {
          parameters.Add("@EndTime", filter.EndTime, DbType.DateTime);
        }

        if (!whereEventTypes.IsNullOrEmpty())
        {
          parameters.Add("@Types", (short)filter.Types, DbType.Int16);
        }

        if (!whereEventImportances.IsNullOrEmpty())
        {
          parameters.Add("@Importances", filter.Importances, DbType.Int16);
        }

        if (!whereEventTmAddr.IsNullOrEmpty())
        {
          parameters.Add("@FullTmaArray", filter.TmAddrList.Select(addr => addr.ToSqlFullTma()).ToArray());
        }

        var dtos = await sql.DbConnection
                            .QueryAsync<TmEventDto>(commandText, parameters)
                            .ConfigureAwait(false);

        dtos.ForEach((dto, idx) =>
        {
          var tmEvent = TmEvent.CreateFromDto(dto);
          tmEvent.Num = idx + 1;
          events.Add(tmEvent);
        });
      }

      return events;
    }
    catch (NpgsqlException ex)
    {
      HandleNpgsqlException(ex);
      return null;
    }
    catch (Exception ex)
    {
      HandleException(ex);
      return null;
    }
  }


  private static string GetWhereEventChannelsAndRtus(TmEventFilter filter)
  {
    if (filter.ChannelAndRtuCollection.IsNullOrEmpty()) return "";

    var tmaList = new List<string>();
    foreach (var chAndRtu in filter.ChannelAndRtuCollection)
    {
      var channelId = chAndRtu.Key;
      var rtuList   = chAndRtu.Value;
      if (rtuList == null)
      {
        var (tmaStart, tmaEnd) = TmChannel.GetSqlTmaRange(channelId);
        tmaList.Add($"(tma >= {tmaStart} AND tma <= {tmaEnd})");
      }
      else
      {
        foreach (var rtuId in rtuList)
        {
          var (tmaStart, tmaEnd) = TmRtu.GetSqlTmaRange(channelId, rtuId);
          tmaList.Add($"(tma >= {tmaStart} AND tma <= {tmaEnd})");
        }
      }
    }

    return " AND (" + string.Join(" OR ", tmaList) + ")";
  }


  private static string GetWhereEventTmStatusClassIds(TmEventFilter filter)
  {
    if (filter.TmStatusClassIdList.IsNullOrEmpty()) return "";

    var classIdList = filter.TmStatusClassIdList.Select(classId => $"(class_id = {classId})");

    return $" AND ((tm_type != {(int)TmNativeDefs.TmDataTypes.Status}) OR {string.Join(" OR ", classIdList)})";
  }


  private static string GetWhereEventTmAddr(TmEventFilter filter)
  {
    if (filter.TmAddrList.IsNullOrEmpty()) return "";

    var tmAddrListList = filter.TmAddrList.Select(tmAddr => $"(fulltma = {tmAddr.ToSqlFullTma()})");

    return $" AND ({string.Join(" OR ", tmAddrListList)})";
  }


  private static string GetWhereEventImportances(TmEventFilter filter)
  {
    if (filter.Importances == TmEventImportances.None ||
        filter.Importances == TmEventImportances.Any)
    {
      return "";
    }

    // return " AND (1 << importance) & @Importances > 0";
    // TQI не поддерживает битовое смещение, поэтому теперь вручную заполняем
    var importances = new List<int>();
    if (filter.Importances.HasFlag(TmEventImportances.Imp0))
    {
      importances.Add(0);
    }

    if (filter.Importances.HasFlag(TmEventImportances.Imp1))
    {
      importances.Add(1);
    }

    if (filter.Importances.HasFlag(TmEventImportances.Imp2))
    {
      importances.Add(2);
    }

    if (filter.Importances.HasFlag(TmEventImportances.Imp3))
    {
      importances.Add(3);
    }

    if (importances.Count == 0)
    {
      return "";
    }

    return $" AND ({string.Join(" OR ", importances.Select(imp => $"(importance = {imp})"))})";
  }


  private static string GetWhereEventTypes(TmEventFilter filter)
  {
    if (filter.Types == TmEventTypes.None ||
        filter.Types == TmEventTypes.Any)
    {
      return "";
    }

    return " AND rec_type & @Types > 0";
  }


  private static string GetWhereEventEndTime(TmEventFilter filter)
  {
    if (!filter.EndTime.HasValue) return "";

    return " AND update_time <= @EndTime";
  }


  private static string GetWhereEventStartTime(TmEventFilter filter)
  {
    if (!filter.StartTime.HasValue) return "";

    return " AND update_time >= @StartTime";
  }


  private static string GetWhereEventFromReserveExcluded(TmEventFilter filter)
  {
    if (!filter.ExcludeFromReserve)
    {
      return "";
    }

    return " AND (ts_add_flags IS NULL OR get_bit(ts_add_flags,4) != 1)";
  }


  private static string GetEventsOutputLimit(TmEventFilter filter)
  {
    if (filter.OutputLimit <= 0) return "";

    return $" LIMIT {filter.OutputLimit}";
  }


  public async Task<IReadOnlyCollection<TmUserAction>> GetUserActionsArchive(TmEventFilter filter)
  {
    if (filter == null) return null; //???

    if (filter.AreUserActionsForbidden ||
        !filter.TmStatusClassIdList.IsNullOrEmpty())
    {
      return Array.Empty<TmUserAction>(); // TODO временно обнуляем при задании ТМ-адресов или классов
    }

    var whereBeg                   = GetWhereUserActionStartTime(filter);
    var whereEnd                   = GetWhereUserActionEndTime(filter);
    var whereActionCategories      = GetWhereUserActionCategories(filter);
    var whereActionImportances     = GetWhereUserActionImportances(filter);
    var whereActionChannelsAndRtus = GetWhereUserActionChannelsAndRtus(filter);
    var whereActionTmAddr          = GetWhereUserActionTmAddr(filter);
    var limit                      = GetEventsOutputLimit(filter);

    try
    {
      var userActions = new List<TmUserAction>();
      using (var sql = _createOikSqlConnection())
      {
        sql.Label = "ArchEvents";
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = $@"SELECT id, time, action, category, state, importance, text, user_name,
                               tma, extra_id, extra_int, extra_text,
                               ack_time, ack_user
            FROM oik_user_actions_log
            WHERE 1=1 {whereBeg}{whereEnd}{whereActionCategories}{whereActionImportances}{whereActionChannelsAndRtus}{whereActionTmAddr}
            ORDER BY time
            {limit}";

        var parameters = new DynamicParameters();
        if (!whereBeg.IsNullOrEmpty())
        {
          parameters.Add("@StartTime", filter.StartTime, DbType.DateTime);
        }

        if (!whereEnd.IsNullOrEmpty())
        {
          parameters.Add("@EndTime", filter.EndTime, DbType.DateTime);
        }

        if (!whereActionImportances.IsNullOrEmpty())
        {
          parameters.Add("@Importances", filter.Importances, DbType.Int16);
        }

        if (!whereActionTmAddr.IsNullOrEmpty())
        {
          parameters.Add("@FullTmaArray", filter.TmAddrList.Select(addr => addr.ToSqlFullTma()).ToArray());
        }

        var dtos = await sql.DbConnection
                            .QueryAsync<TmUserActionDto>(commandText, parameters)
                            .ConfigureAwait(false);

        dtos.ForEach((dto, idx) =>
        {
          var userAction = TmUserAction.CreateFromDto(dto);
          userAction.Num = idx + 1;
          userActions.Add(userAction);
        });
      }

      return userActions;
    }
    catch (NpgsqlException ex)
    {
      HandleNpgsqlException(ex);
      return null;
    }
    catch (Exception ex)
    {
      HandleException(ex);
      return null;
    }
  }


  private static string GetWhereUserActionEndTime(TmEventFilter filter)
  {
    if (!filter.EndTime.HasValue) return "";

    return " AND time <= @EndTime";
  }


  private static string GetWhereUserActionStartTime(TmEventFilter filter)
  {
    if (!filter.StartTime.HasValue) return "";

    return " AND time >= @StartTime";
  }


  private static string GetWhereUserActionCategories(TmEventFilter filter)
  {
    if (filter.Categories.IsNullOrEmpty()) return "";

    var categoriesList = filter.Categories.Select(category => $"(category = {(int)category})");

    return $" AND ({string.Join(" OR ", categoriesList)})";
  }


  private static string GetWhereUserActionImportances(TmEventFilter filter)
  {
    if (filter.Importances == TmEventImportances.None ||
        filter.Importances == TmEventImportances.Any)
    {
      return "";
    }

    return " AND (1 << importance) & @Importances > 0";
  }


  private static string GetWhereUserActionTmAddr(TmEventFilter filter)
  {
    if (filter.TmAddrList.IsNullOrEmpty()) return "";

    return " AND tma = ANY(@FullTmaArray)";
  }


  private static string GetWhereUserActionChannelsAndRtus(TmEventFilter filter)
  {
    if (filter.ChannelAndRtuCollection.IsNullOrEmpty()) return "";

    var tmaList = new List<string>();
    foreach (var chAndRtu in filter.ChannelAndRtuCollection)
    {
      var channelId = chAndRtu.Key;
      var rtuList   = chAndRtu.Value;
      if (rtuList == null)
      {
        var (tmaStart, tmaEnd) = TmChannel.GetSqlTmaRange(channelId);
        tmaList.Add($"((tma & {uint.MaxValue}) >= {tmaStart} AND (tma & {uint.MaxValue}) <= {tmaEnd})");
      }
      else
      {
        foreach (var rtuId in rtuList)
        {
          var (tmaStart, tmaEnd) = TmRtu.GetSqlTmaRange(channelId, rtuId);
          tmaList.Add($"((tma & {uint.MaxValue}) >= {tmaStart} AND (tma & {uint.MaxValue}) <= {tmaEnd})");
        }
      }
    }

    return " AND (" + string.Join(" OR ", tmaList) + ")";
  }


  public async Task<(IReadOnlyCollection<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix)
  {
    if (elix == null) return (null, null);

    var tmEvents = new List<TmEvent>();
    var newElix  = elix;
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        sql.Label       = "CurrEvents";
        sql.IsDebugMode = true;
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

          tmEvents.Add(tmEvent);
        });
      }

      return (tmEvents, newElix);
    }
    catch (NpgsqlException ex)
    {
      HandleNpgsqlException(ex);
      return (null, elix);
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

      using (var sql = _createOikSqlConnection())
      {
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
      }

      return changesFound;
    }
    catch (NpgsqlException ex)
    {
      HandleNpgsqlException(ex);
      return false;
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
      using (var sql = _createOikSqlConnection())
      {
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
    }
    catch (NpgsqlException ex)
    {
      HandleNpgsqlException(ex);
      return null;
    }
    catch (Exception ex)
    {
      HandleException(ex);
      return null;
    }
  }
}