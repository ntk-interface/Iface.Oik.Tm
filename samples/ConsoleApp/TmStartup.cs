using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ConsoleApp
{
  public class ServerService : CommonServerService, IHostedService
  {
  }
  
  
  public class TmStartup : IHostedService
  {
    private const string ApplicationName = "Test-Core";

    private static int              _tmCid;
    private static int              _rbCid;
    private static int              _rbPort;
    private static TmUserInfo       _userInfo;
    private static TmServerFeatures _serverFeatures;

    private readonly ICommonInfrastructure _infr;


    public TmStartup(ICommonInfrastructure infr)
    {
      _infr = infr;
    }


    public static void Connect()
    {
      var commandLineArgs = Environment.GetCommandLineArgs();

      (_tmCid, _rbCid, _rbPort, _userInfo, _serverFeatures) = Tms.Initialize(new TmInitializeOptions
      {
        ApplicationName = ApplicationName,
        TmServer        = commandLineArgs.ElementAtOrDefault(1) ?? "TMS",
        RbServer        = commandLineArgs.ElementAtOrDefault(2) ?? "RBS",
        Host            = commandLineArgs.ElementAtOrDefault(3) ?? ".",
        User            = commandLineArgs.ElementAtOrDefault(4) ?? "",
        Password        = commandLineArgs.ElementAtOrDefault(5) ?? "",
      });
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
      _infr.InitializeTm(_tmCid, _rbCid, _rbPort, _userInfo, _serverFeatures);

      return Task.CompletedTask;
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
      _infr.TerminateTm();

      Tms.Terminate(_tmCid, _rbCid);

      return Task.CompletedTask;
    }
  }
}