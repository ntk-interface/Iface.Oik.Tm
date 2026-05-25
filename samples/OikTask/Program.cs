using System;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OikTask
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var app = Host.CreateDefaultBuilder(args)
                    .ConfigureServices((_, services) =>
                     {
                       // регистрация сервисов ОИК
                       services.AddSingleton<ITmsApi, TmsApi>();
                       services.AddSingleton<IOikSqlApi, OikSqlApi>();
                       services.AddSingleton<IOikDataApi, OikDataApi>();
                       services.AddSingleton<ICommonInfrastructure, CommonInfrastructure>();
                       services.AddSingleton<ServerService>();
                       services.AddSingleton<ICommonServerService>(provider => provider.GetService<ServerService>());
                       services.AddSingleton<TmStartup>();
            
                       // регистрация фоновых служб
                       services.AddSingleton<IHostedService>(provider => provider.GetService<TmStartup>());
                       services.AddSingleton<IHostedService>(provider => provider.GetService<ServerService>());
                       services.AddHostedService<Worker>();
                     })
                    .Build();
      
      using (var scope = app.Services.CreateScope())
      {
        try
        {
          scope.ServiceProvider.GetRequiredService<TmStartup>().TryConnect();
        }
        catch (Exception ex)
        {
          Tms.PrintError(ex.Message);
          Environment.Exit(-1);
        }
      }
      
      app.Run();
    }
  }
}