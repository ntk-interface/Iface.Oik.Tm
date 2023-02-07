using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.Hosting;

namespace AspMinimalWebApiTask;

public class ServerService : CommonServerService, IHostedService
{
}


public class TmStartup : BackgroundService
{
  private const string ApplicationName = "ASP.NET API";
  private const string TraceName       = "ASP.NET Api";
  private const string TraceComment    = "<Asp.Net.Api>";

  private int              _tmCid;
  private int              _rbCid;
  private int              _rbPort;
  private TmUserInfo?      _userInfo;
  private TmServerFeatures _serverFeatures;
  private IntPtr           _stopEventHandle;

  private readonly ICommonInfrastructure    _infr;
  private readonly IHostApplicationLifetime _applicationLifetime;


  public TmStartup(ICommonInfrastructure infr, IHostApplicationLifetime applicationLifetime)
  {
    _infr                = infr;
    _applicationLifetime = applicationLifetime;
  }


  public void TryConnect()
  {
    var commandLineArgs = Environment.GetCommandLineArgs();

    (_tmCid, _rbCid, _rbPort, _userInfo, _serverFeatures, _stopEventHandle) = Tms.InitializeAsTask(
      new TmOikTaskOptions
      {
        TraceName    = TraceName,
        TraceComment = TraceComment,
      },
      new TmInitializeOptions
      {
        ApplicationName = ApplicationName,
        TmServer        = commandLineArgs.ElementAtOrDefault(1) ?? "TMS",
        RbServer        = commandLineArgs.ElementAtOrDefault(2) ?? "RBS",
        Host            = commandLineArgs.ElementAtOrDefault(3) ?? ".",
        User            = commandLineArgs.ElementAtOrDefault(4) ?? "",
        Password        = commandLineArgs.ElementAtOrDefault(5) ?? "",
      });
  }


  public override Task StartAsync(CancellationToken cancellationToken)
  {
    _infr.InitializeTm(_tmCid, _rbCid, _rbPort, _userInfo, _serverFeatures);
    
    Tms.PrintMessage("Соединение с сервером установлено");

    return base.StartAsync(cancellationToken);
  }


  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      if (await Task.Run(() => Tms.StopEventSignalDuringWait(_stopEventHandle, 1000), stoppingToken))
      {
        Tms.PrintMessage("Получено сообщение об остановке со стороны сервера");
        _applicationLifetime.StopApplication();
        break;
      }
    }
  }


  public override Task StopAsync(CancellationToken cancellationToken)
  {
    Tms.Terminate(_tmCid, _rbCid);
    _infr.TerminateTm();

    Tms.PrintMessage("Задача будет закрыта");

    return base.StopAsync(cancellationToken);
  }
}