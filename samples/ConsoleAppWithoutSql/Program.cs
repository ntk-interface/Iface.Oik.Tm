using System;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleAppWithoutSql
{
  public class Program
  {
    public static void Main(string[] args)
    {
      try
      {
        TmStartup.Connect();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Environment.Exit(-1);
      }

      Host.CreateDefaultBuilder(args)
          .ConfigureServices((_, services) =>
           {
             // регистрация сервисов ОИК
             services.AddSingleton<ITmsApi, TmsApi>();
             services.AddSingleton<IOikSqlApi, OikSqlApi>();
             services.AddSingleton<IOikDataApi, OikDataApi>();
             services.AddSingleton<ICommonInfrastructure, CommonInfrastructure>();
             services.AddSingleton<ServerService>();
             services.AddSingleton<ICommonServerService>(provider => provider.GetService<ServerService>());

             // регистрация фоновых служб
             services.AddHostedService<TmStartup>();
             services.AddSingleton<IHostedService>(provider => provider.GetService<ServerService>());
             services.AddHostedService<Worker>();
           })
          .Build()
          .Run();
    }
  }
}