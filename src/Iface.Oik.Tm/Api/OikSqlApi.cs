using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;
using Npgsql;

namespace Iface.Oik.Tm.Api
{
  public class OikSqlApi : IOikSqlApi
  {
    private Func<ICommonOikSqlConnection> _createOikSqlConnection;


    public void SetCreateOikSqlConnection(Func<ICommonOikSqlConnection> createOikSqlConnection)
    {
      _createOikSqlConnection = createOikSqlConnection;
      
      DefaultTypeMap.MatchNamesWithUnderscores = true; // Dapper
    }


    public async Task<DateTime?> GetSystemTime()
    {
      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          return await sql.DbConnection
                          .QuerySingleOrDefaultAsync<DateTime>("SELECT oik_systemtime()")
                          .ConfigureAwait(false);
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


    public async Task<string> GetSystemTimeString()
    {
      var systemTime = await GetSystemTime().ConfigureAwait(false);
      return systemTime?.ToTmString();
    }


    public async Task<int> GetStatus(int ch, int rtu, int point)
    {
      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT v_code
                              FROM oik_cur_ts
                              WHERE ch = @Ch AND rtu = @Rtu AND point = @Point";
          var parameters = new {Ch = ch, Rtu = rtu, Point = point};

          return await sql.DbConnection
                          .QueryFirstOrDefaultAsync<int>(commandText, parameters)
                          .ConfigureAwait(false);
        }
      }
      catch (NpgsqlException ex)
      {
        HandleNpgsqlException(ex);
        return -1;
      }
      catch (Exception ex)
      {
        HandleException(ex);
        return -1;
      }
    }


    public async Task<float> GetAnalog(int ch, int rtu, int point)
    {
      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT v_val
                              FROM oik_cur_tt
                              WHERE ch = @Ch AND rtu = @Rtu AND point = @Point";
          var parameters = new {Ch = ch, Rtu = rtu, Point = point};

          return await sql.DbConnection
                          .QueryFirstOrDefaultAsync<int>(commandText, parameters)
                          .ConfigureAwait(false);
        }
      }
      catch (NpgsqlException ex)
      {
        HandleNpgsqlException(ex);
        return -1;
      }
      catch (Exception ex)
      {
        HandleException(ex);
        return -1;
      }
    }


    public async Task UpdateStatus(TmStatus status)
    {
      if (status == null) return;

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT v_code, flags, v_s2, change_time
                              FROM oik_cur_ts
                              WHERE tma = @Tma";
          var parameters = new {Tma = status.TmAddr.ToSqlTma()};
          var dto = await sql.DbConnection
                             .QueryFirstOrDefaultAsync<TmStatusDto>(commandText, parameters)
                             .ConfigureAwait(false);

          status.UpdateWithDto(dto);
        }
      }
      catch (NpgsqlException ex)
      {
        HandleNpgsqlException(ex);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }


    public async Task UpdateAnalog(TmAnalog analog)
    {
      if (analog == null) return;

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT v_val, flags, change_time
                              FROM oik_cur_tt
                              WHERE tma = @Tma";
          var parameters = new {Tma = analog.TmAddr.ToSqlTma()};
          var dto = await sql.DbConnection
                             .QueryFirstOrDefaultAsync<TmAnalogDto>(commandText, parameters)
                             .ConfigureAwait(false);

          analog.UpdateWithDto(dto);
        }
      }
      catch (NpgsqlException ex)
      {
        HandleNpgsqlException(ex);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }


    public async Task UpdateStatuses(IReadOnlyList<TmStatus> statuses)
    {
      if (statuses.IsNullOrEmpty()) return;

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT v_code, flags, v_s2, change_time
              FROM oik_cur_ts
                RIGHT JOIN UNNEST(@TmaArray) WITH ORDINALITY t (a,i)
                ON tma = t.a
              ORDER BY t.i";
          var parameters = new {TmaArray = statuses.Select(tag => tag.TmAddr.ToSqlTma()).ToArray()};
          var dtos = await sql.DbConnection
                              .QueryAsync<TmStatusDto>(commandText, parameters)
                              .ConfigureAwait(false);

          dtos.ForEach((dto, idx) => statuses[idx].UpdateWithDto(dto));
        }
      }
      catch (NpgsqlException ex)
      {
        HandleNpgsqlException(ex);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }


    public async Task UpdateAnalogs(IReadOnlyList<TmAnalog> analogs)
    {
      if (analogs.IsNullOrEmpty()) return;

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT v_val, flags, change_time
            FROM oik_cur_tt
              RIGHT JOIN UNNEST(@TmaArray) WITH ORDINALITY t (a,i)
              ON tma = t.a
            ORDER BY t.i";
          var parameters = new {TmaArray = analogs.Select(tag => tag.TmAddr.ToSqlTma()).ToArray()};
          var dtos = await sql.DbConnection
                              .QueryAsync<TmAnalogDto>(commandText, parameters)
                              .ConfigureAwait(false);

          dtos.ForEach((dto, idx) => analogs[idx].UpdateWithDto(dto));
        }
      }
      catch (NpgsqlException ex)
      {
        HandleNpgsqlException(ex);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }


    public async Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tags)
    {
      if (tags == null) return;

      switch (tags)
      {
        case IReadOnlyList<TmStatus> statuses:
          await UpdateStatusesPropertiesAndClassData(statuses).ConfigureAwait(false);
          return;

        case IReadOnlyList<TmAnalog> analogs:
          await UpdateAnalogsPropertiesAndClassData(analogs).ConfigureAwait(false);
          return;
      }
    }


    private async Task UpdateStatusesPropertiesAndClassData(IReadOnlyList<TmStatus> statuses)
    {
      if (statuses.IsNullOrEmpty()) return;

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT name, v_importance, v_normalstate, class_id, 
                               cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                               cl_fla_name, cl_flb_name, cl_flc_name, cl_fld_name,
                               cl_fla_text0, cl_flb_text0, cl_flc_text0, cl_fld_text0, 
                               cl_fla_text1, cl_flb_text1, cl_flc_text1, cl_fld_text1
              FROM oik_cur_ts
                RIGHT JOIN UNNEST(@TmaArray) WITH ORDINALITY t (a,i)
                ON tma = t.a
              ORDER BY t.i";
          var parameters = new {TmaArray = statuses.Select(tag => tag.TmAddr.ToSqlTma()).ToArray()};
          var dtos = await sql.DbConnection
                              .QueryAsync<TmStatusPropertiesDto>(commandText, parameters)
                              .ConfigureAwait(false);

          dtos.ForEach((dto, idx) => statuses[idx].UpdatePropertiesWithDto(dto));
        }
      }
      catch (NpgsqlException ex)
      {
        HandleNpgsqlException(ex);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }


    private async Task UpdateAnalogsPropertiesAndClassData(IReadOnlyList<TmAnalog> analogs)
    {
      if (analogs.IsNullOrEmpty()) return;

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT name, v_unit, v_format, class_id, provider
            FROM oik_cur_tt
              RIGHT JOIN UNNEST(@TmaArray) WITH ORDINALITY t (a,i)
              ON tma = t.a
            ORDER BY t.i";
          var parameters = new {TmaArray = analogs.Select(tag => tag.TmAddr.ToSqlTma()).ToArray()};
          var dtos = await sql.DbConnection
                              .QueryAsync<TmAnalogPropertiesDto>(commandText, parameters)
                              .ConfigureAwait(false);

          dtos.ForEach((dto, idx) => analogs[idx].UpdatePropertiesWithDto(dto));
        }
      }
      catch (NpgsqlException ex)
      {
        HandleNpgsqlException(ex);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }


    public async Task UpdateTagPropertiesAndClassData(TmTag tag)
    {
      if (tag == null) return;

      switch (tag)
      {
        case TmStatus status:
          await UpdateStatusPropertiesAndClassData(status).ConfigureAwait(false);
          return;

        case TmAnalog analog:
          await UpdateAnalogPropertiesAndClassData(analog).ConfigureAwait(false);
          return;
      }
    }


    private async Task UpdateStatusPropertiesAndClassData(TmStatus status)
    {
      if (status == null) return;

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT name, v_importance, v_normalstate, class_id, 
                               cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                               cl_fla_name, cl_flb_name, cl_flc_name, cl_fld_name,
                               cl_fla_text0, cl_flb_text0, cl_flc_text0, cl_fld_text0, 
                               cl_fla_text1, cl_flb_text1, cl_flc_text1, cl_fld_text1
              FROM oik_cur_ts
              WHERE tma = @Tma";
          var parameters = new {Tma = status.TmAddr.ToSqlTma()};
          var dto = await sql.DbConnection
                             .QueryFirstAsync<TmStatusPropertiesDto>(commandText, parameters)
                             .ConfigureAwait(false);

          status.UpdatePropertiesWithDto(dto);
        }
      }
      catch (NpgsqlException ex)
      {
        HandleNpgsqlException(ex);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }


    private async Task UpdateAnalogPropertiesAndClassData(TmAnalog analog)
    {
      if (analog == null) return;

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          sql.Label = "UpdateAnalogPropertiesAndClassData";

          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT name, v_unit, v_format, class_id, provider
                              FROM oik_cur_tt
                              WHERE tma = @Tma";
          var parameters = new {Tma = analog.TmAddr.ToSqlTma()};
          var dto = await sql.DbConnection
                             .QueryFirstAsync<TmAnalogPropertiesDto>(commandText, parameters)
                             .ConfigureAwait(false);

          analog.UpdatePropertiesWithDto(dto);
        }
      }
      catch (NpgsqlException ex)
      {
        HandleNpgsqlException(ex);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }


    public async Task<IReadOnlyCollection<TmChannel>> GetTmTreeChannels()
    {
      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = "SELECT ch AS ChannelId, name AS Name FROM oik_chn ORDER BY ch";
          var dtos = await sql.DbConnection
                                  .QueryAsync<TmChannelDto>(commandText)
                                  .ConfigureAwait(false);

          return dtos.Select(dto => dto.MapToTmChannel())
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


    public async Task<IReadOnlyCollection<TmRtu>> GetTmTreeRtus(int channelId)
    {
      if (channelId < 0 || channelId > 254) return null;

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = "SELECT @Ch AS ChannelId, rtu AS RtuId, name AS Name FROM oik_rtu WHERE ch = @Ch";
          var parameters  = new {Ch = channelId};
          var dtos = await sql.DbConnection
                              .QueryAsync<TmRtuDto>(commandText, parameters)
                              .ConfigureAwait(false);

          return dtos.Select(dto => dto.MapToTmRtu())
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


    public async Task<IReadOnlyCollection<TmStatus>> GetTmTreeStatuses(int channelId, int rtuId)
    {
      if (channelId < 0 || channelId > 254 ||
          rtuId     < 1 || rtuId     > 255)
      {
        return null;
      }

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT @Ch AS ch, @Rtu AS rtu, point, 
                                name, v_importance, v_normalstate, class_id, 
                                cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                                cl_fla_name, cl_flb_name, cl_flc_name, cl_fld_name,
                                cl_fla_text0, cl_flb_text0, cl_flc_text0, cl_fld_text0, 
                                cl_fla_text1, cl_flb_text1, cl_flc_text1, cl_fld_text1,
                                v_code, flags, v_s2, change_time
                              FROM oik_cur_ts
                              WHERE ch = @Ch AND rtu = @Rtu";
          var parameters = new {Ch = channelId, Rtu = rtuId};
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


    public async Task<IReadOnlyCollection<TmAnalog>> GetTmTreeAnalogs(int channelId, int rtuId)
    {
      if (channelId < 0 || channelId > 254 ||
          rtuId     < 1 || rtuId     > 255)
      {
        return null;
      }

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT @Ch AS ch, @RTU AS rtu, point, 
                                name, v_unit, v_format, class_id, provider, 
                                v_val, flags, change_time
                              FROM oik_cur_tt
                              WHERE ch = @Ch AND rtu = @Rtu";
          var parameters = new {Ch = channelId, Rtu = rtuId};
          var dtos = await sql.DbConnection
                              .QueryAsync<TmAnalogTmTreeDto>(commandText, parameters)
                              .ConfigureAwait(false);

          return dtos.Select(TmAnalog.CreateFromTmTreeDto)
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
            FlagAps   = (int) TmFlags.StatusAps,
            FlagUnrel = (int) TmFlags.Unreliable,
            FlagRes   = (int) TmFlags.ResChannel,
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
            FlagAps     = (int) TmFlags.StatusAps,
            FlagUnacked = (int) TmFlags.Unacked,
            FlagUnrel   = (int) TmFlags.Unreliable,
            FlagRes     = (int) TmFlags.ResChannel,
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
          var parameters = new {FlagAbnormal = (int) TmFlags.Abnormal};
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


    public async Task<IReadOnlyCollection<TmAlert>> GetAlerts()
    {
      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT alert_id, importance, active, unack, on_time, off_time, type_name, name, tm_type, tma, class_id,
                                value_text, cur_time, cur_value 
                              FROM oik_alerts";
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


    public async Task<IReadOnlyCollection<TmAlarm>> GetPresentAlarms()
    {
      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          // сначала запрос списка уставок
          var alarmsCommandText = @"SELECT alarm_id, alarm_name, cmp_val, cmp_sign, importance, in_use, active, tma
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
          var analogsParameters = new {TmaArray = alarms.Select(alarm => alarm.TmAnalog.TmAddr.ToSqlTma()).ToArray()};
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


    public async Task<IReadOnlyCollection<TmAlarm>> GetAnalogAlarms(TmAnalog analog)
    {
      if (analog == null) return null;

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT alarm_id, alarm_name, cmp_val, cmp_sign, importance, in_use, active
                              FROM oik_alarms
                              WHERE tma = @Tma";
          var parameters = new {Tma = analog.TmAddr.ToSqlTma()};
          var dtos = await sql.DbConnection
                              .QueryAsync<TmAlarmDto>(commandText, parameters)
                              .ConfigureAwait(false);

          return dtos.Select(dto => TmAlarm.CreateFromDto(dto, analog))
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


    public async Task<IReadOnlyCollection<TmStatus>> LookupStatuses(TmStatusFilter filter)
    {
      if (filter == null) return null;

      var whereName       = GetWhereName();
      var whereFlags      = GetWhereFlags();
      var whereClasses    = GetWhereClasses();
      var whereChangeTime = GetWhereChangeTime();
      var whereUpdateTime = GetWhereUpdateTime();
      var whereStatus     = GetWhereStatus();
      var whereS2Flags    = GetWhereS2Flags();

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = $@"SELECT @Ch AS ch, @Rtu AS rtu, point, 
                                name, v_importance, v_normalstate, class_id, 
                                cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                                cl_fla_name, cl_flb_name, cl_flc_name, cl_fld_name,
                                cl_fla_text0, cl_flb_text0, cl_flc_text0, cl_fld_text0, 
                                cl_fla_text1, cl_flb_text1, cl_flc_text1, cl_fld_text1,
                                v_code, flags, v_s2, change_time
                              FROM oik_cur_ts
                              WHERE 1=1 {whereName}{whereFlags}{whereClasses}{whereChangeTime}{whereUpdateTime}{whereStatus}{whereS2Flags}
                              ORDER BY change_time DESC";
          var parameters = new DynamicParameters();
          if (!whereName.IsNullOrEmpty())
          {
            parameters.Add("@Name", filter.Name, DbType.String);
          }
          if (!whereFlags.IsNullOrEmpty())
          {
            parameters.Add("@Flags", filter.Flags, DbType.Int32);
          }
          if (!whereChangeTime.IsNullOrEmpty())
          {
            parameters.Add("@change_time_minutes", filter.ChangeTimeMinutes, DbType.Int32);
          }
          if (!whereUpdateTime.IsNullOrEmpty())
          {
            parameters.Add("@update_time_minutes", filter.UpdateTimeMinutes, DbType.Int32);
          }
          if (!whereStatus.IsNullOrEmpty())
          {
            parameters.Add("@Status", filter.Status, DbType.Int32);
          }
          if (!whereS2Flags.IsNullOrEmpty())
          {
            parameters.Add("@S2Flags", filter.S2Flags, DbType.Int32);
          }
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

      string GetWhereName()
      {
        return ""; // внимание, сейчас не работает такой поиск в SQL
        /*return !string.IsNullOrWhiteSpace(filter.Name)
          ? " AND name LIKE @Name"
          : "";*/
      }

      string GetWhereFlags()
      {
        return filter.Flags != TmFlags.None
          ? " AND flags & @Flags = @Flags"
          : "";
      }

      string GetWhereClasses()
      {
        return !filter.ClassIdList.IsNullOrEmpty()
          ? $" AND ({string.Join(" OR ", filter.ClassIdList.Select(classId => $"(class_id = {classId})"))})"
          : "";
      }

      string GetWhereChangeTime()
      {
        return GetWhereTime(filter.ChangeTimeOption, "change_time");
      }

      string GetWhereUpdateTime()
      {
        return GetWhereTime(filter.UpdateTimeOption, "update_time");
      }

      string GetWhereTime(TmTagFilterTimeOption? option, string timeColumn)
      {
        switch (option)
        {
          case TmTagFilterTimeOption.IsEarlierThan:
            return $" AND {timeColumn} > CURRENT_TIMESTAMP - MAKE_INTERVAL(mins => @{timeColumn}_minutes)";
          case TmTagFilterTimeOption.IsLaterThan:
            return $" AND {timeColumn} < CURRENT_TIMESTAMP - MAKE_INTERVAL(mins => @{timeColumn}_minutes)";
          default:
            return "";
        }
      }

      string GetWhereStatus()
      {
        switch (filter.Status)
        {
          case 0:
          case 1:
            return " AND v_code = @Status";
          default:
            return "";
        }
      }

      string GetWhereS2Flags()
      {
        switch (filter.S2Flags)
        {
          case TmS2Flags.None:
          case TmS2Flags.Break:
          case TmS2Flags.Malfunction:
            return " AND v_s2 = @S2Flags";
          default:
            return "";
        }
      }
    }


    public async Task<IReadOnlyCollection<TmAnalog>> LookupAnalogs(TmAnalogFilter filter)
    {
      if (filter == null) return null;

      var whereName       = GetWhereName();
      var whereFlags      = GetWhereFlags();
      var whereClasses    = GetWhereClasses();
      var whereChangeTime = GetWhereChangeTime();
      var whereUpdateTime = GetWhereUpdateTime();

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = $@"SELECT @Ch AS ch, @RTU AS rtu, point, 
                                name, v_unit, v_format, class_id, provider, 
                                v_val, flags, change_time
                              FROM oik_cur_tt
                              WHERE 1=1 {whereName}{whereFlags}{whereClasses}{whereChangeTime}{whereUpdateTime}
                              ORDER BY change_time DESC";
          var parameters = new DynamicParameters();
          if (!whereName.IsNullOrEmpty())
          {
            parameters.Add("@Name", filter.Name, DbType.String);
          }
          if (!whereFlags.IsNullOrEmpty())
          {
            parameters.Add("@Flags", filter.Flags, DbType.Int32);
          }
          if (!whereChangeTime.IsNullOrEmpty())
          {
            parameters.Add("@change_time_minutes", filter.ChangeTimeMinutes, DbType.Int32);
          }
          if (!whereUpdateTime.IsNullOrEmpty())
          {
            parameters.Add("@update_time_minutes", filter.UpdateTimeMinutes, DbType.Int32);
          }
          var dtos = await sql.DbConnection
                              .QueryAsync<TmAnalogTmTreeDto>(commandText, parameters)
                              .ConfigureAwait(false);

          return dtos.Select(TmAnalog.CreateFromTmTreeDto)
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

      string GetWhereName()
      {
        return ""; // внимание, сейчас не работает такой поиск в SQL
        /*return !string.IsNullOrWhiteSpace(filter.Name)
          ? " AND name LIKE @Name"
          : "";*/
      }

      string GetWhereFlags()
      {
        return filter.Flags != TmFlags.None
          ? " AND flags & @Flags = @Flags"
          : "";
      }

      string GetWhereClasses()
      {
        return !filter.ClassIdList.IsNullOrEmpty()
          ? $" AND ({string.Join(" OR ", filter.ClassIdList.Select(classId => $"(class_id = {classId})"))})"
          : "";
      }

      string GetWhereChangeTime()
      {
        return GetWhereTime(filter.ChangeTimeOption, "change_time");
      }

      string GetWhereUpdateTime()
      {
        return GetWhereTime(filter.UpdateTimeOption, "update_time");
      }

      string GetWhereTime(TmTagFilterTimeOption? option, string timeColumn)
      {
        switch (option)
        {
          case TmTagFilterTimeOption.IsEarlierThan:
            return $" AND {timeColumn} > CURRENT_TIMESTAMP - MAKE_INTERVAL(mins => @{timeColumn}_minutes)";
          case TmTagFilterTimeOption.IsLaterThan:
            return $" AND {timeColumn} < CURRENT_TIMESTAMP - MAKE_INTERVAL(mins => @{timeColumn}_minutes)";
          default:
            return "";
        }
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


    public async Task<IReadOnlyCollection<TmEvent>> GetArchEvents(TmEventFilter filter)
    {
      if (filter == null) return null; //???

      var whereBeg                   = GetArchEventWhereBeg(filter);
      var whereEnd                   = GetArchEventsWhereEnd(filter);
      var whereEventTypes            = GetArchEventsWhereEventTypes(filter);
      var whereEventImportances      = GetArchEventsWhereEventImportances(filter);
      var whereEventTmStatusClassIds = GetArchEventsWhereEventTmStatusClassIds(filter);
      var whereEventChannelsAndRtus  = GetArchEventsWhereEventChannelsAndRtus(filter); // todo al не работает, см. ниже
      var whereEventTmAddr           = GetArchEventsWhereEventTmAddr(filter);

      try
      {
        var events = new List<TmEvent>();
        using (var sql = _createOikSqlConnection())
        {
          sql.Label = "ArchEvents";
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = $@"SELECT elix, update_time, 
                                rec_text, name, rec_state_text, rec_type, rec_type_name, user_name, importance, 
                                tma, tma_str, tm_type_name, tm_type, class_id,
                                ack_time, ack_user
            FROM oik_event_log
            WHERE 1=1 {whereBeg}{whereEnd}{whereEventTypes}{whereEventImportances}{whereEventTmAddr}{whereEventTmStatusClassIds}{whereEventChannelsAndRtus}
            ORDER BY update_time";

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
            parameters.Add("@Types", filter.Types, DbType.Int16);
          }
          if (!whereEventImportances.IsNullOrEmpty())
          {
            parameters.Add("@Importances", filter.Importances, DbType.Int16);
          }

          var dtos = await sql.DbConnection
                              .QueryAsync<TmEventDto>(commandText, parameters)
                              .ConfigureAwait(false);

          dtos.ForEach((dto, idx) =>
          {
            if (IsTmaGoodForChannelAndRtuFilter(dto.Tma, filter))
            {
              var tmEvent = TmEvent.CreateFromDto(dto);
              tmEvent.Num = idx;
              events.Add(tmEvent);
            }
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


    // todo al потом когда будет работать фильтр SQL убрать это
    private static bool IsTmaGoodForChannelAndRtuFilter(int tma, TmEventFilter filter)
    {
      if (filter.ChannelAndRtuCollection.IsNullOrEmpty()) return true;

      foreach (var chAndRtu in filter.ChannelAndRtuCollection)
      {
        var channelId = chAndRtu.Key;
        var rtuList   = chAndRtu.Value;
        if (rtuList == null)
        {
          var (tmaStart, tmaEnd) = TmChannel.GetSqlTmaRange(channelId);
          if (tma >= tmaStart && tma <= tmaEnd)
          {
            return true;
          }
        }
        else
        {
          foreach (var rtuId in rtuList)
          {
            var (tmaStart, tmaEnd) = TmRtu.GetSqlTmaRange(channelId, rtuId);
            if (tma >= tmaStart && tma <= tmaEnd)
            {
              return true;
            }
          }
        }
      }

      return false;
    }


    private static string GetArchEventsWhereEventChannelsAndRtus(TmEventFilter filter)
    {
      // todo al Подход хороший, но не работает для oik_event_log конструкция с множественным OR для tma
      return "";
      /*if (filter.ChannelAndRtuCollection.IsNullOrEmpty()) return "";

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

      return " AND (" + string.Join(" OR ", tmaList) + ")";*/
    }


    private static string GetArchEventsWhereEventTmStatusClassIds(TmEventFilter filter)
    {
      if (filter.TmStatusClassIdList.IsNullOrEmpty()) return "";

      var classIdList = new List<string>();
      foreach (var classId in filter.TmStatusClassIdList)
      {
        classIdList.Add($"(class_id = {classId})");
      }
      return $" AND ((tm_type != {(int) TmNativeDefs.TmDataTypes.Status}) OR {string.Join(" OR ", classIdList)})";
    }


    private static string GetArchEventsWhereEventTmAddr(TmEventFilter filter)
    {
      if (filter.TmAddrList.IsNullOrEmpty()) return "";

      var tmaList = filter.TmAddrList.Select(tmAddr => $"(tma_str = '{tmAddr.ToSqlTmaStr()}')");
      //tmaList.Add($"((tma = {tmAddr.ToSqlTma()}) AND (tm_type = {tmAddr.GetSqlTmType()}))"); // todo JO?!

      return " AND (" + string.Join(" OR ", tmaList) + ")";
    }


    private static string GetArchEventsWhereEventImportances(TmEventFilter filter)
    {
      if (filter.Importances == TmEventImportances.None ||
          filter.Importances == TmEventImportances.Any)
      {
        return "";
      }

      return " AND (1 << importance) & @Importances > 0";
    }


    private static string GetArchEventsWhereEventTypes(TmEventFilter filter)
    {
      if (filter.Types == TmEventTypes.None ||
          filter.Types == TmEventTypes.Any)
      {
        return "";
      }

      return " AND rec_type & @Types > 0";
    }


    private static string GetArchEventsWhereEnd(TmEventFilter filter)
    {
      if (!filter.EndTime.HasValue) return "";

      return " AND update_time <= @EndTime";
    }


    private static string GetArchEventWhereBeg(TmEventFilter filter)
    {
      if (!filter.StartTime.HasValue) return "";

      return " AND update_time >= @StartTime";
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
          sql.Label = "CurrEvents";
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT elix, update_time, 
                                rec_text, name, rec_state_text, rec_type, rec_type_name, user_name, importance, 
                                tma, tma_str, tm_type_name, tm_type, class_id,  
                                ack_time, ack_user
                              FROM oik_event_log_elix
                              WHERE elix > @Elix
                              ORDER BY update_time";
          var parameters = new {Elix = elix.ToByteArray()};
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

      try
      {
        bool changesFound = false;

        using (var sql = _createOikSqlConnection())
        {
          await sql.OpenAsync().ConfigureAwait(false);
          var commandText = @"SELECT ack_time, ack_user
            FROM oik_event_log_elix
              RIGHT JOIN UNNEST(@ElixArray) WITH ORDINALITY t (e,i)
              ON elix = t.e
            ORDER BY t.i";
          var parameters = new {ElixArray = tmEvents.Select(e => e.Elix.ToByteArray()).ToArray()};
          var dtos = await sql.DbConnection
                              .QueryAsync<TmEventDto>(commandText, parameters)
                              .ConfigureAwait(false);

          dtos.ForEach((dto, idx) =>
          {
            if (DateUtil.NullIfEpoch(dto.AckTime) != null)
            {
              tmEvents[idx].AckTime = dto.AckTime;
              tmEvents[idx].AckUser = dto.AckUser;
              changesFound          = true;
            }
          });
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


    private void HandleException(Exception ex)
    {
      Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Sql Exception = {ex.Message}");
    }


    private void HandleNpgsqlException(NpgsqlException ex)
    {
      Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} NpgSql Exception = {ex.Message}");
    }


    private static void ConsoleWrite(string message)
    {
      Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {Thread.CurrentThread.ManagedThreadId} {message}");
    }
  }
}