using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ConsoleAppWithoutSql
{
  public class ServerService : CommonServerService, IHostedService
  {
  }
  
  
  public class TmStartup : IHostedService
  {
    private const string ApplicationName = "Test-Core";

    private static int              _tmCid;
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

      (_tmCid, _userInfo, _serverFeatures) = Tms.InitializeWithoutSql(new TmInitializeOptions
      {
        ApplicationName = ApplicationName,
        TmServer        = commandLineArgs.ElementAtOrDefault(1) ?? "TMS",
        Host            = commandLineArgs.ElementAtOrDefault(2) ?? ".",
        User            = commandLineArgs.ElementAtOrDefault(3) ?? "",
        Password        = commandLineArgs.ElementAtOrDefault(4) ?? "",
      });
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
      _infr.InitializeTmWithoutSql(_tmCid, _userInfo, _serverFeatures);

      return Task.CompletedTask;
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
      Tms.TerminateWithoutSql(_tmCid);
      _infr.TerminateTm();

      return Task.CompletedTask;
    }
  }
}