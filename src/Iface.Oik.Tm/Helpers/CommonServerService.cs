using System;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Services;
using Iface.Oik.Tm.Utils;

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
      private set
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
      var tmsSystemTime = Tms.GetSystemTime(_tmCid);
      if (tmsSystemTime == null)
      {
        IsConnected = false;
        return;
      }

      if (!CheckSqlConnection)
      {
        ServerTime  = tmsSystemTime.Value;
        IsConnected = true;
        return;
      }

      try
      {
        using (var sql = _createOikSqlConnection())
        {
          sql.Open();
          using (var cmd = sql.CreateCommand())
          {
            cmd.CommandText = "SELECT oik_systemtime()";
            using (var reader = cmd.ExecuteReaderSeq())
            {
              if (!reader.Read())
              {
                IsConnected = false;
                return;
              }
              ServerTime = reader.GetDateTime(0);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        IsConnected = false;
        return;
      }

      IsConnected = true;
    }
  }
}