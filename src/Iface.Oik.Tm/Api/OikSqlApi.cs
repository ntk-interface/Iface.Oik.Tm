using System;
using System.Threading.Tasks;
using Dapper;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Npgsql;

namespace Iface.Oik.Tm.Api
{
  public partial class OikSqlApi : IOikSqlApi
  {
    private Func<ICommonOikSqlConnection> _createOikSqlConnection;


    public void SetCreateOikSqlConnection(Func<ICommonOikSqlConnection> createOikSqlConnection)
    {
      _createOikSqlConnection = createOikSqlConnection;

      AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); // Npgsql 
      DefaultTypeMap.MatchNamesWithUnderscores = true;                    // Dapper
    }


    public async Task<DateTime?> GetSystemTime()
    {
      try
      {
        using var sql = _createOikSqlConnection();
        await sql.OpenAsync().ConfigureAwait(false);
        
        return await sql.DbConnection
                        .QuerySingleOrDefaultAsync<DateTime>("SELECT oik_systemtime()")
                        .ConfigureAwait(false);
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


    private static void HandleException(Exception ex)
    {
      switch (ex)
      {
        case NpgsqlException:
          Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} NpgSql Exception = {ex.Message}");
          return;
          
        default:
          Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Sql Exception = {ex.Message}");
          return;
      }
    }
  }
}