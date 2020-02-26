using System;
using System.Text;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleApp
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }


    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureServices((hostContext, services) =>
          {
            // регистрация сервисов ОИК
            services.AddSingleton<ITmNative, TmNative>();
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
          });
  }
}