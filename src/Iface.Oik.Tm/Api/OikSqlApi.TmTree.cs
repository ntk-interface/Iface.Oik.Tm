using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Utils;
using Npgsql;

namespace Iface.Oik.Tm.Api;

public partial class OikSqlApi
{
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
        var parameters  = new { Ch = channelId };
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
                                name, v_importance, v_normalstate, class_id, provider, 
                                cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                                cl_fla_name, cl_flb_name, cl_flc_name, cl_fld_name,
                                cl_fla_text0, cl_flb_text0, cl_flc_text0, cl_fld_text0, 
                                cl_fla_text1, cl_flb_text1, cl_flc_text1, cl_fld_text1,
                                v_code, flags, v_s2, change_time
                              FROM oik_cur_ts
                              WHERE ch = @Ch AND rtu = @Rtu";
        var parameters = new { Ch = channelId, Rtu = rtuId };
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
        var parameters = new { Ch = channelId, Rtu = rtuId };
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


  public async Task<IReadOnlyCollection<TmAccum>> GetTmTreeAccums(int channelId, int rtuId)
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
                                name, v_unit, v_format, v_counter_format, provider, 
                                v_val, v_load, flags, change_time
                              FROM oik_cur_ti
                              WHERE ch = @Ch AND rtu = @Rtu";
        var parameters = new { Ch = channelId, Rtu = rtuId };
        var dtos = await sql.DbConnection
                            .QueryAsync<TmAccumTmTreeDto>(commandText, parameters)
                            .ConfigureAwait(false);

        return dtos.Select(TmAccum.CreateFromTmTreeDto)
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


  public async Task<string> GetChannelName(int channelId)
  {
    if (channelId < 0 || channelId > 254) return null;

    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = "SELECT name FROM oik_chn WHERE ch = @Ch";
        return await sql.DbConnection.QueryFirstOrDefaultAsync<string>(commandText,
                                                                       new { Ch = channelId })
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


  public async Task<string> GetRtuName(int channelId, int rtuId)
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
        var commandText = "SELECT name FROM oik_rtu WHERE ch = @Ch AND rtu = @Rtu";
        return await sql.DbConnection.QueryFirstOrDefaultAsync<string>(commandText,
                                                                       new { Ch = channelId, Rtu = rtuId })
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
                                name, v_importance, v_normalstate, class_id, provider, 
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
            parameters.Add("@Flags", (int)filter.Flags, DbType.Int32);
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
            parameters.Add("@Flags", (int)filter.Flags, DbType.Int32);
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
}