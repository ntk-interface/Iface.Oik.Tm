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
      Console.WriteLine(await _api.GetSystemTimeString());

      var ts  = new TmStatus(20, 1, 1);
      var ti  = new TmAnalog(20, 1, 1);
      var tii = new TmAccum(20, 1, 1);

      var valueTms = await _api.GetAccum(20, 1, 1, PreferApi.Tms);
      var valueSql = await _api.GetAccum(20, 1, 1, PreferApi.Sql);

      var loadTms = await _api.GetAccumLoad(20, 1, 1, PreferApi.Tms);
      var loadSql = await _api.GetAccumLoad(20, 1, 1, PreferApi.Sql);

      await _api.UpdateTagPropertiesAndClassData(ts);
      await _api.UpdateStatus(ts);

      await _api.UpdateTagPropertiesAndClassData(ti);
      await _api.UpdateAnalog(ti);

      Console.WriteLine(ts);
      Console.WriteLine(ti);

      Console.WriteLine("Активные уставки:");
      var alarms = await _api.GetPresentAlarms();
      alarms?.ForEach(alarm => Console.WriteLine($"{alarm.FullName}, {alarm.StateName}"));

      Console.WriteLine("Активные АПС:");
      var aps = await _api.GetPresentAps();
      aps?.ForEach(Console.WriteLine);

      Console.WriteLine("Монитор тревог:");
      var alerts = await _api.GetAlertsWithAnalogMicroSeries();
      alerts?.ForEach(alert => Console.WriteLine($"{alert.Name}, {alert.ImportanceAlias}, {alert.CurrentValueString}"));
    }
  }
}