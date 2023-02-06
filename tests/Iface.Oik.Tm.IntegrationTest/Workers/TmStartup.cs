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
  private static int              _tmCid;
  private static int              _rbCid;
  private static int              _rbPort;
  private static TmUserInfo?      _userInfo;
  private static TmServerFeatures _serverFeatures;

  private readonly ICommonInfrastructure _infr;


  public TmStartup(ICommonInfrastructure infr)
  {
    _infr = infr;
  }


  public Task StartAsync(CancellationToken cancellationToken)
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