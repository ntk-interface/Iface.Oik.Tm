using System;
using System.Collections.Generic;
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
  public async Task<IReadOnlyCollection<TmAlert>> GetAlerts()
  {
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText =
          @"SELECT alert_id, importance, active, unack, on_time, off_time, type_name, name, tm_type, tma, class_id,
                   value_text, cur_time, cur_value, act_value, ack_time, ack_user
            FROM oik_alerts
            WHERE tma > 0";
        var dtos = await sql.DbConnection
                            .QueryAsync<TmAlertDto>(commandText)
                            .ConfigureAwait(false);

        return dtos.Select(TmAlert.CreateFromDto)
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


  public async Task<IReadOnlyCollection<TmAlert>> GetAlertsWithAnalogMicroSeries()
  {
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText =
          @"SELECT alert_id, importance, active, unack, on_time, off_time, type_name, al.name, al.tm_type, al.tma, al.class_id, 
                   value_text, cur_time, cur_value, act_value, ack_time, ack_user,
                   ms_values, ms_times, ms_sflags,
                   tpr_min_val, tpr_max_val, tpr_nominal, tpr_alr_present, tpr_alr_inuse,
                   tpr_zone_d_low, tpr_zone_c_low, tpr_zone_c_high, tpr_zone_d_high
          FROM oik_alerts AS al
          LEFT JOIN oik_cur_tt AS tt ON tt.tma = al.tma AND al.tm_type = @AnalogTmType
          WHERE al.tma > 0";
        
        var dtos = await sql.DbConnection.QueryAsync<TmAlertDto>(commandText,
                                                                 new
                                                                 {
                                                                   AnalogTmType = unchecked((short)TmNativeDefs.TmDataTypes.Analog)
                                                                 })
                            .ConfigureAwait(false);

        return dtos.Select(TmAlert.CreateFromDto)
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


  public async Task<IReadOnlyCollection<TmStatus>> GetPresentAps()
  {
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT ch, rtu, point, 
                                   name, v_importance, v_normalstate, class_id, cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                                   v_code, flags, v_s2, change_time
                            FROM oik_cur_ts
                            WHERE (flags & @FlagAps > 0) AND (v_code = 1)
                              AND (flags & @FlagUnrel = 0) AND (flags & @FlagRes = 0)";
        var parameters = new
        {
          FlagAps   = (int)TmFlags.StatusAps,
          FlagUnrel = (int)TmFlags.Unreliable,
          FlagRes   = (int)TmFlags.ResChannel,
        };
        var dtos = await sql.DbConnection
                            .QueryAsync<TmStatusTmTreeDto>(commandText, parameters)
                            .ConfigureAwait(false);

        return dtos.Select(TmStatus.CreateFromTmTreeDto)
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


  public async Task<IReadOnlyCollection<TmStatus>> GetUnackedAps()
  {
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT ch, rtu, point, 
                                   name, v_importance, v_normalstate, class_id, cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                                   v_code, flags, v_s2, change_time
                            FROM oik_cur_ts
                            WHERE (flags & @FlagAps > 0) AND (v_code = 0) AND (flags & @FlagUnacked > 0)
                              AND (flags & @FlagUnrel = 0) AND (flags & @FlagRes = 0)";
        var parameters = new
        {
          FlagAps     = (int)TmFlags.StatusAps,
          FlagUnacked = (int)TmFlags.Unacked,
          FlagUnrel   = (int)TmFlags.Unreliable,
          FlagRes     = (int)TmFlags.ResChannel,
        };
        var dtos = await sql.DbConnection
                            .QueryAsync<TmStatusTmTreeDto>(commandText, parameters)
                            .ConfigureAwait(false);

        return dtos.Select(TmStatus.CreateFromTmTreeDto)
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


  public async Task<IReadOnlyCollection<TmStatus>> GetAbnormalStatuses()
  {
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT ch, rtu, point, 
                                   name, v_importance, v_normalstate, class_id, cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                                   v_code, flags, v_s2, change_time
                            FROM oik_cur_ts
                            WHERE (flags & @FlagAbnormal > 0)";
        var parameters = new { FlagAbnormal = (int)TmFlags.Abnormal };
        var dtos = await sql.DbConnection
                            .QueryAsync<TmStatusTmTreeDto>(commandText, parameters)
                            .ConfigureAwait(false);

        return dtos.Select(TmStatus.CreateFromTmTreeDto)
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


  public async Task<IReadOnlyCollection<TmAlarm>> GetPresentAlarms()
  {
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        // сначала запрос списка уставок
        var alarmsCommandText =
          @"SELECT alarm_id, alarm_name, importance, in_use, active, tma, cmp_val, cmp_sign, expr, typ
            FROM oik_alarms
            WHERE active = TRUE";
        var alarmsDtos = await sql.DbConnection
                                  .QueryAsync<TmAlarmDto>(alarmsCommandText)
                                  .ConfigureAwait(false);
        var alarms = alarmsDtos.Select(TmAlarm.CreateFromDto)
                               .ToList();

        // потом запрос данных о ТИТ, если конечно есть уставки
        if (alarms.Count == 0)
        {
          return alarms;
        }

        var analogsCommandText = @"SELECT name, v_unit, v_format, class_id, provider, v_val, flags, change_time
                                   FROM oik_cur_tt
                                     RIGHT JOIN UNNEST(@TmaArray) WITH ORDINALITY t (a,i)
                                   ON tma = a
                                   ORDER BY t.i";
        var analogsParameters = new { TmaArray = alarms.Select(alarm => alarm.TmAnalog.TmAddr.ToSqlTma()).ToArray() };
        var analogsDtos = await sql.DbConnection
                                   .QueryAsync<TmAnalogTmTreeDto>(analogsCommandText, analogsParameters)
                                   .ConfigureAwait(false);

        analogsDtos.ForEach((dto, idx) => alarms[idx].TmAnalog.UpdateWithTmTreeDto(dto));

        return alarms;
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


  public async Task<bool> HasPresentAps()
  {
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT EXISTS(SELECT FROM oik_cur_ts 
                                          WHERE (flags & @FlagAps > 0) AND (v_code = 1)
                                            AND (flags & @FlagUnrel = 0) AND (flags & @FlagRes = 0))";
        var parameters = new
        {
          FlagAps   = (int) TmFlags.StatusAps,
          FlagUnrel = (int) TmFlags.Unreliable,
          FlagRes   = (int) TmFlags.ResChannel,
        };
        return await sql.DbConnection
                        .QueryFirstOrDefaultAsync<bool>(commandText, parameters)
                        .ConfigureAwait(false);
      }
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


  public async Task<bool> HasPresentAlarms()
  {
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = "SELECT EXISTS(SELECT FROM oik_alarms WHERE active = true)";

        return await sql.DbConnection
                        .QueryFirstOrDefaultAsync<bool>(commandText)
                        .ConfigureAwait(false);
      }
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
}