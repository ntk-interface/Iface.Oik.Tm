using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Services;

namespace Iface.Oik.Tm.Helpers
{
  public class CommonServerService : BackgroundRestartableService, ICommonServerService
  {
    private Func<ICommonOikSqlConnection> _createOikSqlConnection;
    private int                           _tmCid;

    public bool CheckSqlConnection    { get; set; } = true;
    public int  DelayWhenConnected    { get; set; } = 1000;
    public int  DelayWhenNotConnected { get; set; } = 1000;

    private bool _isConnected;

    public bool IsConnected
    {
      get => _isConnected;
      protected set
      {
        if (_isConnected == value) return;
        _isConnected = value;
        IsConnectedChanged.Invoke(this, EventArgs.Empty);
      }
    }

    public event EventHandler IsConnectedChanged = delegate { };

    public DateTime ServerTime { get; private set; }


    public void SetTmCid(int tmCid)
    {
      _tmCid = tmCid;
    }


    public void SetCreateOikSqlConnection(Func<ICommonOikSqlConnection> createOikSqlConnection)
    {
      _createOikSqlConnection = createOikSqlConnection;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        DoWork();
        await Task.Delay(IsConnected ? DelayWhenConnected : DelayWhenNotConnected, stoppingToken)
                  .ConfigureAwait(false);
      }
    }


    public void NotifyOfAccessProblem()
    {
      DoWork();
    }


    private void DoWork()
    {
      var isTmsConnected = CheckTms();
      if (!isTmsConnected)
      {
        IsConnected = false;
        return;
      }

      if (!CheckSqlConnection)
      {
        IsConnected = true;
        return;
      }

      IsConnected = CheckSql();
    }


    private bool CheckTms()
    {
      var serverTime = Tms.GetSystemTime(_tmCid);
      if (!serverTime.HasValue)
      {
        return false;
      }
      ServerTime = serverTime.Value;
      return true;
    }


    private bool CheckSql()
    {
      try
      {
        using (var sql = _createOikSqlConnection())
        {
          sql.Open();
          var serverTime = sql.DbConnection.QueryFirstOrDefault<DateTime?>("SELECT oik_systemtime()");
          if (!serverTime.HasValue)
          {
            return false;
          }
          ServerTime = serverTime.Value;
          return true;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return false;
      }
    }
  }
}