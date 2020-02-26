using System;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Utils;
using Microsoft.Extensions.Hosting;

namespace ConsoleApp
{
  public class Worker : BackgroundService
  {
    private const int WorkerDelay = 1000;


    private readonly ICommonInfrastructure _infr;
    private readonly IOikDataApi           _api;


    public Worker(ICommonInfrastructure infr,
                  IOikDataApi           api)
    {
      _infr = infr;
      _api  = api;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        await DoWork();
        await Task.Delay(WorkerDelay, stoppingToken);
      }
    }


    public async Task DoWork()
    {
      Console.WriteLine(_infr.TmUserInfo?.Name);
      Console.WriteLine(await _api.GetSystemTimeString(DataApiPreference.Sql));

      var ts = new TmStatus(20, 1, 1);
      var ti = new TmAnalog(20, 1, 1);

      await _api.UpdateTagPropertiesAndClassData(ts);
      await _api.UpdateStatus(ts);

      await _api.UpdateTagPropertiesAndClassData(ti);
      await _api.UpdateAnalog(ti);

      Console.WriteLine(ts);
      Console.WriteLine(ti);

      Console.WriteLine("Активные уставки:");
      (await _api.GetPresentAlarms())?.ForEach(Console.WriteLine);

      Console.WriteLine("Активные АПС:");
      (await _api.GetPresentAps())?.ForEach(Console.WriteLine);
    }
  }
}