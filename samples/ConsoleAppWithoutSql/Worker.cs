using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ConsoleAppWithoutSql
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

      var tmAnalog = new TmAnalog(0, 1, 1);

      /*var ii1 = await _api.GetImpulseArchiveInstant(tmAnalog,
                                                    new TmAnalogRetroFilter(new DateTime(2026, 05, 19, 11, 50, 0),
                                                                            new DateTime(2026, 05, 19, 11, 53, 0)));

      var ii2 = await _api.GetImpulseArchiveAverage(tmAnalog,
                                                    new TmAnalogRetroFilter(new DateTime(2026, 05, 19, 11, 50, 0),
                                                                            new DateTime(2026, 05, 19, 11, 53, 0),
                                                                            1));

      var ii3 = await _api.GetImpulseArchiveSlices(tmAnalog,
                                                   new TmAnalogRetroFilter(new DateTime(2026, 05, 19, 11, 50, 0),
                                                                           new DateTime(2026, 05, 19, 11, 53, 0),
                                                                           1));*/

      // var tagsByNamePattern = await _infr.TmsApi.GetTagsByNamePattern(TmType.Status, "ОРУ 500 кВ");

      /*var tmStatuses = new List<TmStatus>(new[]
      {
        new TmStatus(0, 1, 1),
        new TmStatus(0, 1, 2),
        new TmStatus(0, 1, 3),
      });
      await _infr.TmsApi.UpdateStatuses(tmStatuses);*/

      // var tagsByFlags = await _infr.TmsApi.GetTagsByFlags(TmType.Status, TmFlags.Abnormal, TmCommonPointFlags.None);

      // var tagsByGroup = await _infr.TmsApi.GetTagsByGroup(TmType.Analog, "Сила тока");

      var ts = new TmStatus(20, 1, 1);
      var ti = new TmAnalog(20, 1, 1);

      await _api.UpdateTagPropertiesAndClassData(ts);
      await _api.UpdateStatus(ts);

      await _api.UpdateTagPropertiesAndClassData(ti);
      await _api.UpdateAnalog(ti);

      Console.WriteLine(ts);
      Console.WriteLine(ti);
    }
  }
}