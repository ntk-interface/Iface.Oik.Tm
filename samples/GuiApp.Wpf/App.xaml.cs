using System.Text;
using System.Windows;
using GuiApp.Wpf.ViewModels;
using GuiApp.Wpf.Views;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GuiApp.Wpf;

public partial class App
{
  protected override void OnStartup(StartupEventArgs e)
  {
    base.OnStartup(e);

    // TODO убрать после перехода кодировок
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // требуется для работы с кодировкой Win-1251
    
    Tms.InitNativeLibrary();

    var services = StartupServices();

    services.AddSingleton<MainWindowView>();

    services.BuildServiceProvider()
            .GetService<MainWindowView>()
            .Show();
  }


  private static IServiceCollection StartupServices()
  {
    var services = new ServiceCollection();

    services.AddSingleton<ITmsApi, TmsApi>();
    services.AddSingleton<IOikSqlApi, OikSqlApi>();
    services.AddSingleton<IOikDataApi, OikDataApi>();

    services.AddSingleton<ICommonServerService, CommonServerService>();
    services.AddSingleton<ICommonInfrastructure, CommonInfrastructure>();

    services.AddSingleton<MainWindowViewModel>();

    return services;
  }
}