using System;
using System.Text;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleApp
{
  public class Program
  {
    public static void Main(string[] args)
    {
      // требуется для работы с кодировкой Win-1251
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      
      // устанавливаем соединение с сервером ОИК
      try
      {
        TmStartup.Connect();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Environment.Exit(-1);
      }
      
      // .NET Generic Host
      CreateHostBuilder(args).Build().Run();
    }


    public static IHostBuilder CreateHostBuilder(string[] args) =>
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
          });
  }
}