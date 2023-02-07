using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.IntegrationTest.Util;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Iface.Oik.Tm.IntegrationTest.Workers;

public class ServerService : CommonServerService, IHostedService
{
}


public class TmStartup : IHostedService
{
  private int              _tmCid;
  private int              _rbCid;
  private int              _rbPort;
  private TmUserInfo?      _userInfo;
  private TmServerFeatures _serverFeatures;

  private readonly ICommonInfrastructure _infr;


  public TmStartup(ICommonInfrastructure infr)
  {
    _infr = infr;
  }


  public void TryConnect()
  {
    var (host, tmServer, rbServer, username, password) = CommonUtil.ParseCommandLineArgs();

    (_tmCid, _rbCid, _rbPort, _userInfo, _serverFeatures) = Tms.Initialize(new TmInitializeOptions
    {
      ApplicationName = CommonUtil.TaskName,
      Host            = host,
      TmServer        = tmServer,
      RbServer        = rbServer,
      User            = username,
      Password        = password,
    });
  }


  public Task StartAsync(CancellationToken cancellationToken)
  {
    _infr.InitializeTm(_tmCid, _rbCid, _rbPort, _userInfo, _serverFeatures);

    return Task.CompletedTask;
  }


  public Task StopAsync(CancellationToken cancellationToken)
  {
    Tms.Terminate(_tmCid, _rbCid);
    _infr.TerminateTm();

    return Task.CompletedTask;
  }
}