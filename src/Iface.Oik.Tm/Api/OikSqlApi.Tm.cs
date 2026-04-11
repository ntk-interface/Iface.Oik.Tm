using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api;

public partial class OikSqlApi
{
  public async Task<int> GetStatus(int ch, int rtu, int point)
  {
    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT v_code
                          FROM oik_cur_ts
                          WHERE tma = @Tma";
      var parameters = new { Tma = TmAddr.EncodeTma(ch, rtu, point) };

      return await sql.DbConnection
                      .QueryFirstOrDefaultAsync<int>(commandText, parameters)
                      .ConfigureAwait(false);
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
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT v_val
                          FROM oik_cur_tt
                          WHERE tma = @Tma";
      var parameters = new { Tma = TmAddr.EncodeTma(ch, rtu, point) };

      return await sql.DbConnection
                      .QueryFirstOrDefaultAsync<float>(commandText, parameters)
                      .ConfigureAwait(false);
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
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT v_val
                          FROM oik_cur_ti
                          WHERE tma = @Tma";
      var parameters = new { Tma = TmAddr.EncodeTma(ch, rtu, point) };

      return await sql.DbConnection
                      .QueryFirstOrDefaultAsync<float>(commandText, parameters)
                      .ConfigureAwait(false);
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
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT v_load
                          FROM oik_cur_ti
                          WHERE tma = @Tma";
      var parameters = new { Tma = TmAddr.EncodeTma(ch, rtu, point) };

      return await sql.DbConnection
                      .QueryFirstOrDefaultAsync<float>(commandText, parameters)
                      .ConfigureAwait(false);
    }
    catch (Exception ex)
    {
      HandleException(ex);
      return -1;
    }
  }


  public async Task UpdateStatus(TmStatus tmStatus)
  {
    if (tmStatus == null) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT v_code, flags, v_s2, change_time
                          FROM oik_cur_ts
                          WHERE tma = @Tma";
      var parameters = new { Tma = tmStatus.TmAddr.ToTma() };
      
      var dto = await sql.DbConnection
                         .QueryFirstOrDefaultAsync<TmStatusDto>(commandText, parameters)
                         .ConfigureAwait(false);

      tmStatus.UpdateWithDto(dto);
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  public async Task UpdateAnalog(TmAnalog tmAnalog)
  {
    if (tmAnalog == null) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT v_val, flags, change_time
                          FROM oik_cur_tt
                          WHERE tma = @Tma";
      var parameters = new { Tma = tmAnalog.TmAddr.ToTma() };
      
      var dto = await sql.DbConnection
                         .QueryFirstOrDefaultAsync<TmAnalogDto>(commandText, parameters)
                         .ConfigureAwait(false);

      tmAnalog.UpdateWithDto(dto);
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  public async Task UpdateAccum(TmAccum tmAccum)
  {
    if (tmAccum == null) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT v_val, v_load, flags, change_time
                          FROM oik_cur_ti
                          WHERE tma = @Tma";
      var parameters = new { Tma = tmAccum.TmAddr.ToTma() };
      
      var dto = await sql.DbConnection
                         .QueryFirstOrDefaultAsync<TmAccumDto>(commandText, parameters)
                         .ConfigureAwait(false);

      tmAccum.UpdateWithDto(dto);
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  public async Task UpdateStatuses(IReadOnlyList<TmStatus> tmStatuses)
  {
    await UpdateStatuses(tmStatuses.ToDictionary(s => s.TmAddr.ToTma())).ConfigureAwait(false);
  }


  public async Task UpdateStatuses(IReadOnlyDictionary<int, TmStatus> tmStatuses)
  {
    if (tmStatuses.IsNullOrEmpty()) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT tma, v_code, flags, v_s2, change_time
                          FROM oik_cur_ts
                          WHERE tma = ANY(@TmaArray)";
      var parameters = new { TmaArray = tmStatuses.Keys.ToArray() };
      
      var dtos = await sql.DbConnection
                          .QueryAsync<TmStatusDto>(commandText, parameters)
                          .ConfigureAwait(false);

      foreach (var dto in dtos)
      {
        if (tmStatuses.TryGetValue(dto.Tma, out var tmStatus))
        {
          tmStatus.UpdateWithDto(dto);
        }
      }
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  public async Task UpdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs)
  {
    await UpdateAnalogs(tmAnalogs.ToDictionary(s => s.TmAddr.ToTma())).ConfigureAwait(false);
  }


  public async Task UpdateAnalogs(IReadOnlyDictionary<int, TmAnalog> tmAnalogs)
  {
    if (tmAnalogs.IsNullOrEmpty()) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT tma, v_val, flags, change_time
                          FROM oik_cur_tt
                          WHERE tma = ANY(@TmaArray)";
      var parameters = new { TmaArray = tmAnalogs.Keys.ToArray() };
      
      var dtos = await sql.DbConnection
                          .QueryAsync<TmAnalogDto>(commandText, parameters)
                          .ConfigureAwait(false);

      foreach (var dto in dtos)
      {
        if (tmAnalogs.TryGetValue(dto.Tma, out var tmAnalog))
        {
          tmAnalog.UpdateWithDto(dto);
        }
      }
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  public async Task UpdateAccums(IReadOnlyList<TmAccum> tmAccums)
  {
    await UpdateAccums(tmAccums.ToDictionary(s => s.TmAddr.ToTma())).ConfigureAwait(false);
  }


  public async Task UpdateAccums(IReadOnlyDictionary<int, TmAccum> tmAccums)
  {
    if (tmAccums.IsNullOrEmpty()) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT tma, v_val, v_load, flags, change_time
                          FROM oik_cur_ti
                          WHERE tma = ANY(@TmaArray)";
      var parameters = new { TmaArray = tmAccums.Keys.ToArray() };
      
      var dtos = await sql.DbConnection
                          .QueryAsync<TmAccumDto>(commandText, parameters)
                          .ConfigureAwait(false);


      foreach (var dto in dtos)
      {
        if (tmAccums.TryGetValue(dto.Tma, out var tmAccum))
        {
          tmAccum.UpdateWithDto(dto);
        }
      }
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  public async Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tmTags)
  {
    if (tmTags.IsNullOrEmpty())
    {
      return;
    }

    switch (tmTags.ElementAtOrDefault(0))
    {
      case TmStatus:
        await UpdateStatusesPropertiesAndClassData(tmTags.ToDictionary(t => t.TmAddr.ToTma(),
                                                                       t => t as TmStatus)).ConfigureAwait(false);
        return;
      
      case TmAnalog:
        await UpdateAnalogsPropertiesAndClassData(tmTags.ToDictionary(t => t.TmAddr.ToTma(),
                                                                      t => t as TmAnalog)).ConfigureAwait(false);
        return;
      
      case TmAccum:
        await UpdateAccumsPropertiesAndClassData(tmTags.ToDictionary(t => t.TmAddr.ToTma(),
                                                                     t => t as TmAccum)).ConfigureAwait(false);
        return;
    }
  }


  public async Task UpdateStatusesPropertiesAndClassData(IReadOnlyDictionary<int, TmStatus> tmStatuses)
  {
    if (tmStatuses.IsNullOrEmpty()) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT tma, name, v_importance, v_normalstate, class_id, provider, 
                                 cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                                 cl_fla_name, cl_flb_name, cl_flc_name, cl_fld_name,
                                 cl_fla_text0, cl_flb_text0, cl_flc_text0, cl_fld_text0, 
                                 cl_fla_text1, cl_flb_text1, cl_flc_text1, cl_fld_text1,
                                 cl_ctltext_off, cl_ctltext_on
                          FROM oik_cur_ts
                          WHERE tma = ANY(@TmaArray)";
      var parameters = new { TmaArray = tmStatuses.Keys.ToArray() };
      
      var dtos = await sql.DbConnection
                          .QueryAsync<TmStatusPropertiesDto>(commandText, parameters)
                          .ConfigureAwait(false);

      foreach (var dto in dtos)
      {
        if (tmStatuses.TryGetValue(dto.Tma, out var tmStatus))
        {
          tmStatus.UpdatePropertiesWithDto(dto);
        }
      }
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  public async Task UpdateAnalogsPropertiesAndClassData(IReadOnlyDictionary<int, TmAnalog> tmAnalogs)
  {
    if (tmAnalogs.IsNullOrEmpty()) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT tma, name, v_unit, v_format, class_id, provider,
                                 tpr_min_val, tpr_max_val, tpr_nominal, tpr_alr_present, tpr_alr_inuse,
                                 tpr_zone_d_low, tpr_zone_c_low, tpr_zone_c_high, tpr_zone_d_high
                          FROM oik_cur_tt
                          WHERE tma = ANY(@TmaArray)";
      var parameters = new { TmaArray = tmAnalogs.Keys.ToArray() };
      
      var dtos = await sql.DbConnection
                          .QueryAsync<TmAnalogPropertiesDto>(commandText, parameters)
                          .ConfigureAwait(false);

      foreach (var dto in dtos)
      {
        if (tmAnalogs.TryGetValue(dto.Tma, out var tmAnalog))
        {
          tmAnalog.UpdatePropertiesWithDto(dto);
        }
      }
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  public async Task UpdateAccumsPropertiesAndClassData(IReadOnlyDictionary<int, TmAccum> tmAccums)
  {
    if (tmAccums.IsNullOrEmpty()) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT tma, name, v_unit, v_format, v_counter_format, provider
                          FROM oik_cur_ti
                          WHERE tma = ANY(@TmaArray)";
      var parameters = new { TmaArray = tmAccums.Keys.ToArray() };
      
      var dtos = await sql.DbConnection
                          .QueryAsync<TmAccumPropertiesDto>(commandText, parameters)
                          .ConfigureAwait(false);

      foreach (var dto in dtos)
      {
        if (tmAccums.TryGetValue(dto.Tma, out var tmAccum))
        {
          tmAccum.UpdatePropertiesWithDto(dto);
        }
      }
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  public async Task UpdateTagPropertiesAndClassData(TmTag tmTag)
  {
    if (tmTag == null) return;

    switch (tmTag)
    {
      case TmStatus tmStatus:
        await UpdateStatusPropertiesAndClassData(tmStatus).ConfigureAwait(false);
        return;

      case TmAnalog tmAnalog:
        await UpdateAnalogPropertiesAndClassData(tmAnalog).ConfigureAwait(false);
        return;

      case TmAccum tmAccum:
        await UpdateAccumPropertiesAndClassData(tmAccum).ConfigureAwait(false);
        return;
    }
  }


  private async Task UpdateStatusPropertiesAndClassData(TmStatus tmStatus)
  {
    if (tmStatus == null) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT name, v_importance, v_normalstate, class_id, provider, 
                                 cl_text0, cl_text1, cl_break_text, cl_malfun_text, 
                                 cl_fla_name, cl_flb_name, cl_flc_name, cl_fld_name,
                                 cl_fla_text0, cl_flb_text0, cl_flc_text0, cl_fld_text0, 
                                 cl_fla_text1, cl_flb_text1, cl_flc_text1, cl_fld_text1,
                                 cl_ctltext_off, cl_ctltext_on
                          FROM oik_cur_ts
                          WHERE tma = @Tma";
      var parameters = new { Tma = tmStatus.TmAddr.ToTma() };
      
      var dto = await sql.DbConnection
                         .QueryFirstAsync<TmStatusPropertiesDto>(commandText, parameters)
                         .ConfigureAwait(false);

      tmStatus.UpdatePropertiesWithDto(dto);
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  private async Task UpdateAnalogPropertiesAndClassData(TmAnalog tmAnalog)
  {
    if (tmAnalog == null) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT name, v_unit, v_format, class_id, provider,
                                 tpr_min_val, tpr_max_val, tpr_nominal, tpr_alr_present, tpr_alr_inuse,
                                 tpr_zone_d_low, tpr_zone_c_low, tpr_zone_c_high, tpr_zone_d_high
                          FROM oik_cur_tt
                          WHERE tma = @Tma";
      var parameters = new { Tma = tmAnalog.TmAddr.ToTma() };
      
      var dto = await sql.DbConnection
                         .QueryFirstAsync<TmAnalogPropertiesDto>(commandText, parameters)
                         .ConfigureAwait(false);

      tmAnalog.UpdatePropertiesWithDto(dto);
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  private async Task UpdateAccumPropertiesAndClassData(TmAccum tmAccum)
  {
    if (tmAccum == null) return;

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT name, v_unit, v_format, v_counter_format, provider
                          FROM oik_cur_ti
                          WHERE tma = @Tma";
      var parameters = new { Tma = tmAccum.TmAddr.ToTma() };
      
      var dto = await sql.DbConnection
                         .QueryFirstAsync<TmAccumPropertiesDto>(commandText, parameters)
                         .ConfigureAwait(false);

      tmAccum.UpdatePropertiesWithDto(dto);
    }
    catch (Exception ex)
    {
      HandleException(ex);
    }
  }


  public async Task<IReadOnlyCollection<TmAlarm>> GetAnalogAlarms(TmAnalog tmAnalog)
  {
    if (tmAnalog == null)
    {
      return null;
    }

    try
    {
      using var sql = _createOikSqlConnection();
      await sql.OpenAsync().ConfigureAwait(false);
      
      var commandText = @"SELECT alarm_id, alarm_name, importance, in_use, active, cmp_val, cmp_sign, expr, typ
                          FROM oik_alarms
                          WHERE tma = @Tma";
      var parameters = new { Tma = tmAnalog.TmAddr.ToTma() };
      
      var dtos = await sql.DbConnection
                          .QueryAsync<TmAlarmDto>(commandText, parameters)
                          .ConfigureAwait(false);

      return dtos.Select(dto => TmAlarm.CreateFromDto(dto, tmAnalog))
                 .ToList();
    }
    catch (Exception ex)
    {
      HandleException(ex);
      return null;
    }
  }
}