using System;
using System.Text;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.IntegrationTest.Util;
using Iface.Oik.Tm.IntegrationTest.Workers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Iface.Oik.Tm.IntegrationTest;

public class Program
{
  public static void Main(string[] args)
  {
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // требуется для работы с кодировкой Win-1251
    Tms.InitNativeLibrary();

    // Сначала проверяем статические методы
    TestStaticTms.DoWork();

    // Потом проверяем API, имитируя типовую задачу
    var app = CreateHostBuilder(args).Build();
    using (var scope = app.Services.CreateScope())
    {
      try
      {
        scope.ServiceProvider.GetRequiredService<TmStartup>().TryConnect();
      }
      catch (Exception ex)
      {
        Log.Error(ex.Message);
        Log.ExitMessage();
        Environment.Exit(-1);
      }
    }

    app.Run();
    Log.ExitMessage();
  }


  private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
          services.AddSingleton<ITmNative, TmNative>();
          services.AddSingleton<ITmsApi, TmsApi>();
          services.AddSingleton<IOikSqlApi, OikSqlApi>();
          services.AddSingleton<IOikDataApi, OikDataApi>();
          services.AddSingleton<ICommonInfrastructure, CommonInfrastructure>();
          services.AddSingleton<ServerService>();
          services.AddSingleton<ICommonServerService>(provider => provider.GetRequiredService<ServerService>());
          services.AddSingleton<TmStartup>();

          services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<TmStartup>());
          services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<ServerService>());
          services.AddHostedService<TestApi>();
        });
}