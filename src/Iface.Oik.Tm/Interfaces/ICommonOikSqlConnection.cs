using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Iface.Oik.Tm.Interfaces
{
  public interface ICommonOikSqlConnection : IDisposable
  {
    string           Label        { get; set; }
    bool             IsDebugMode  { get; set; }
    NpgsqlConnection DbConnection { get; }
    bool             IsOpen       { get; }

    void Open(); // throws Exception
    Task OpenAsync();
    Task OpenAsync(CancellationToken cancellationToken);

    NpgsqlCommand CreateCommand();

    void Close();
  }
}