using System;

namespace Iface.Oik.Tm.Interfaces
{
  public interface ICommonServerService : IBackgroundService
  {
    bool CheckSqlConnection    { get; set; }
    int  DelayWhenConnected    { get; set; }
    int  DelayWhenNotConnected { get; set; }

    void SetTmCid(int tmCid);

    void SetCreateOikSqlConnection(Func<ICommonOikSqlConnection> createOikSqlConnection);

    bool     IsConnected { get; }
    DateTime ServerTime  { get; }

    event EventHandler IsConnectedChanged;

    void NotifyOfAccessProblem();
  }
}