using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Npgsql;

namespace Iface.Oik.Tm.Helpers
{
  public class CommonOikSqlConnection : ICommonOikSqlConnection
  {
    private readonly int    _rbPort;
    private readonly string _connectionString;

    public string           Label        { get; set; }
    public NpgsqlConnection DbConnection { get; private set; }

    public bool IsOpen => DbConnection?.State == ConnectionState.Open;


    protected CommonOikSqlConnection()
    {
    }


    public CommonOikSqlConnection(int rbPort)
    {
      _rbPort = rbPort;
    }


    public CommonOikSqlConnection(string connectionString)
    {
      _connectionString = connectionString;
    }


    private void PrepareDbConnection()
    {
      DbConnection = new NpgsqlConnection(BuildConnectionString());
      WireDbConnection();
    }


    public NpgsqlCommand CreateCommand()
    {
      return new NpgsqlCommand {Connection = DbConnection};
    }


    public NpgsqlCommand CreateCommand(NpgsqlTransaction transaction)
    {
      return new NpgsqlCommand {Connection = DbConnection, Transaction = transaction};
    }


    public void Open()
    {
      PrepareDbConnection();
      DbConnection.Open();
      InitialExecute();
    }


    public async Task OpenAsync(CancellationToken cancellationToken)
    {
      PrepareDbConnection();
      await DbConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
      await InitialExecuteAsync(cancellationToken).ConfigureAwait(false);
    }


    public async Task OpenAsync()
    {
      await OpenAsync(CancellationToken.None);
    }


    protected virtual string BuildConnectionString()
    {
      return _connectionString ??
             $"Host=127.0.0.1;Port={_rbPort};Database=oikdb;Timeout=10;Username=postgres";
    }


    protected virtual void WireDbConnection()
    {
    }


    protected virtual void UnWireDbConnection()
    {
    }


    protected virtual void InitialExecute()
    {
    }


    protected virtual Task InitialExecuteAsync(CancellationToken cancellationToken)
    {
      return Task.Run(() => InitialExecute(), cancellationToken);
    }


    public void Close()
    {
      DbConnection?.Close();
    }


    public void Dispose()
    {
      ReleaseManagedResources();
      GC.SuppressFinalize(this);
    }


    protected virtual void ReleaseManagedResources()
    {
      UnWireDbConnection();
      DbConnection?.Dispose();
    }
  }
}