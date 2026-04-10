using System;
using System.Collections.Generic;
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
        var parameters = new { Ch = ch, Rtu = rtu, Point = point };

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
        var parameters = new { Ch = ch, Rtu = rtu, Point = point };

        return await sql.DbConnection
                        .QueryFirstOrDefaultAsync<float>(commandText, parameters)
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


  public async Task<float> GetAccum(int ch, int rtu, int point)
  {
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT v_val
                              FROM oik_cur_ti
                              WHERE ch = @Ch AND rtu = @Rtu AND point = @Point";
        var parameters = new { Ch = ch, Rtu = rtu, Point = point };

        return await sql.DbConnection
                        .QueryFirstOrDefaultAsync<float>(commandText, parameters)
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


  public async Task<float> GetAccumLoad(int ch, int rtu, int point)
  {
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT v_load
                              FROM oik_cur_ti
                              WHERE ch = @Ch AND rtu = @Rtu AND point = @Point";
        var parameters = new { Ch = ch, Rtu = rtu, Point = point };

        return await sql.DbConnection
                        .QueryFirstOrDefaultAsync<float>(commandText, parameters)
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
        var parameters = new { Tma = status.TmAddr.ToSqlTma() };
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
        var parameters = new { Tma = analog.TmAddr.ToSqlTma() };
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


  public async Task UpdateAccum(TmAccum accum)
  {
    if (accum == null) return;

    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT v_val, v_load, flags, change_time
                              FROM oik_cur_ti
                              WHERE tma = @Tma";
        var parameters = new { Tma = accum.TmAddr.ToSqlTma() };
        var dto = await sql.DbConnection
                           .QueryFirstOrDefaultAsync<TmAccumDto>(commandText, parameters)
                           .ConfigureAwait(false);

        accum.UpdateWithDto(dto);
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
        var parameters = new { TmaArray = statuses.Select(tag => tag.TmAddr.ToSqlTma()).ToArray() };
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
        var parameters = new { TmaArray = analogs.Select(tag => tag.TmAddr.ToSqlTma()).ToArray() };
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


  public async Task UpdateAccums(IReadOnlyList<TmAccum> accums)
  {
    if (accums.IsNullOrEmpty()) return;

    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT v_val, v_load, flags, change_time
            FROM oik_cur_ti
              RIGHT JOIN UNNEST(@TmaArray) WITH ORDINALITY t (a,i)
              ON tma = t.a
            ORDER BY t.i";
        var parameters = new { TmaArray = accums.Select(tag => tag.TmAddr.ToSqlTma()).ToArray() };
        var dtos = await sql.DbConnection
                            .QueryAsync<TmAccumDto>(commandText, parameters)
                            .ConfigureAwait(false);

        dtos.ForEach((dto, idx) => accums[idx].UpdateWithDto(dto));
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

      case IReadOnlyList<TmAccum> accums:
        await UpdateAccumsPropertiesAndClassData(accums).ConfigureAwait(false);
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
        var commandText = @"SELECT name, v_importance, v_normalstate, class_id, provider, 
                               cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                               cl_fla_name, cl_flb_name, cl_flc_name, cl_fld_name,
                               cl_fla_text0, cl_flb_text0, cl_flc_text0, cl_fld_text0, 
                               cl_fla_text1, cl_flb_text1, cl_flc_text1, cl_fld_text1,
                               cl_ctltext_off, cl_ctltext_on
              FROM oik_cur_ts
                RIGHT JOIN UNNEST(@TmaArray) WITH ORDINALITY t (a,i)
                ON tma = t.a
              ORDER BY t.i";
        var parameters = new { TmaArray = statuses.Select(tag => tag.TmAddr.ToSqlTma()).ToArray() };
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
        var commandText = @"SELECT name, v_unit, v_format, class_id, provider,
                                     tpr_min_val, tpr_max_val, tpr_nominal, tpr_alr_present, tpr_alr_inuse,
                                     tpr_zone_d_low, tpr_zone_c_low, tpr_zone_c_high, tpr_zone_d_high
                              FROM oik_cur_tt
                                RIGHT JOIN UNNEST(@TmaArray) WITH ORDINALITY t (a,i)
                                  ON tma = t.a
                              ORDER BY t.i";
        var parameters = new { TmaArray = analogs.Select(tag => tag.TmAddr.ToSqlTma()).ToArray() };
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


  private async Task UpdateAccumsPropertiesAndClassData(IReadOnlyList<TmAccum> accums)
  {
    if (accums.IsNullOrEmpty()) return;

    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT name, v_unit, v_format, v_counter_format, provider
                              FROM oik_cur_ti
                                RIGHT JOIN UNNEST(@TmaArray) WITH ORDINALITY t (a,i)
                                  ON tma = t.a
                              ORDER BY t.i";
        var parameters = new { TmaArray = accums.Select(tag => tag.TmAddr.ToSqlTma()).ToArray() };
        var dtos = await sql.DbConnection
                            .QueryAsync<TmAccumPropertiesDto>(commandText, parameters)
                            .ConfigureAwait(false);

        dtos.ForEach((dto, idx) => accums[idx].UpdatePropertiesWithDto(dto));
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

      case TmAccum accum:
        await UpdateAccumPropertiesAndClassData(accum).ConfigureAwait(false);
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
        var commandText = @"SELECT name, v_importance, v_normalstate, class_id, provider, 
                               cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                               cl_fla_name, cl_flb_name, cl_flc_name, cl_fld_name,
                               cl_fla_text0, cl_flb_text0, cl_flc_text0, cl_fld_text0, 
                               cl_fla_text1, cl_flb_text1, cl_flc_text1, cl_fld_text1,
                               cl_ctltext_off, cl_ctltext_on
              FROM oik_cur_ts
              WHERE tma = @Tma";
        var parameters = new { Tma = status.TmAddr.ToSqlTma() };
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
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT name, v_unit, v_format, class_id, provider,
                                     tpr_min_val, tpr_max_val, tpr_nominal, tpr_alr_present, tpr_alr_inuse,
                                     tpr_zone_d_low, tpr_zone_c_low, tpr_zone_c_high, tpr_zone_d_high
                              FROM oik_cur_tt
                              WHERE tma = @Tma";
        var parameters = new { Tma = analog.TmAddr.ToSqlTma() };
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


  private async Task UpdateAccumPropertiesAndClassData(TmAccum analog)
  {
    if (analog == null) return;

    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT name, v_unit, v_format, v_counter_format, provider
                              FROM oik_cur_ti
                              WHERE tma = @Tma";
        var parameters = new { Tma = analog.TmAddr.ToSqlTma() };
        var dto = await sql.DbConnection
                           .QueryFirstAsync<TmAccumPropertiesDto>(commandText, parameters)
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


  public async Task<IReadOnlyCollection<TmAlarm>> GetAnalogAlarms(TmAnalog analog)
  {
    if (analog == null)
    {
      return null;
    }

    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT alarm_id, alarm_name, importance, in_use, active, cmp_val, cmp_sign, expr, typ
                              FROM oik_alarms
                              WHERE tma = @Tma";
        var parameters = new { Tma = analog.TmAddr.ToSqlTma() };
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
}