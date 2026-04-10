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
  public async Task<IReadOnlyCollection<ITmAnalogRetro[]>> GetAnalogsMicroSeries(IReadOnlyList<TmAnalog> analogs)
  {
    if (analogs.IsNullOrEmpty())
    {
      return new [] {Array.Empty<ITmAnalogRetro>()};
    }
    try
    {
      using (var sql = _createOikSqlConnection())
      {
        await sql.OpenAsync().ConfigureAwait(false);
        var commandText = @"SELECT ms_values, ms_times, ms_sflags
                            FROM oik_cur_tt
                            RIGHT JOIN UNNEST(@TmaArray) WITH ORDINALITY t (a,i) 
                              ON tma = t.a
                            ORDER BY t.i";
        var parameters = new {TmaArray = analogs.Select(tag => tag.TmAddr.ToSqlTma()).ToArray()};
        var dtos = await sql.DbConnection
                            .QueryAsync<TmAnalogMicroSeriesDto>(commandText, parameters)
                            .ConfigureAwait(false);
        return dtos.Select(dto => dto.MapToITmAnalogRetroArray()).ToList();
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