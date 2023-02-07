using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;

namespace AspMinimalWebApiTask;

public class ServerService : CommonServerService, IHostedService
{
}


public class TmStartup : BackgroundService
{
  private const string ApplicationName = "ASP.NET API";
  private const string TraceName       = "ASP.NET Api";
  private const string TraceComment    = "<Asp.Net.Api>";

  private static int              _tmCid;
  private static int              _rbCid;
  private static int              _rbPort;
  private static TmUserInfo?      _userInfo;
  private static TmServerFeatures _serverFeatures;
  private static IntPtr           _stopEventHandle;

  private readonly IHostApplicationLifetime _applicationLifetime;
  private readonly ICommonInfrastructure    _infr;


  public TmStartup(ICommonInfrastructure infr, IHostApplicationLifetime applicationLifetime)
  {
    _infr                = infr;
    _applicationLifetime = applicationLifetime;
  }


  public override Task StartAsync(CancellationToken cancellationToken)
  {
    var commandLineArgs = Environment.GetCommandLineArgs();

    try
    {
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

      Tms.PrintMessage("Соединение с сервером установлено");

      _infr.InitializeTm(_tmCid, _rbCid, _rbPort, _userInfo, _serverFeatures);

      return base.StartAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      Tms.PrintError(ex.Message);
      return Task.FromException(ex);
    }
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


  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    Tms.Terminate(_tmCid, _rbCid);
    _infr.TerminateTm();

    Tms.PrintMessage("Задача будет закрыта");

    await base.StopAsync(cancellationToken);
  }
}