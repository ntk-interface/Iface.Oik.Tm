using System;
using System.Text;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OikTask
{
  public class Program
  {
    public static void Main(string[] args)
    {
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // требуется для работы с кодировкой Win-1251
      
      var app = CreateHostBuilder(args).Build();
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

    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureServices((_, services) =>
          {
            // регистрация сервисов ОИК
            services.AddSingleton<ITmNative, TmNative>();
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
          });
  }
}