using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GuiApp.AvaloniaUI.ViewModels;
using GuiApp.AvaloniaUI.Views;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GuiApp.AvaloniaUI;

public partial class App : Application
{
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
  }


  public override void OnFrameworkInitializationCompleted()
  {
    var serviceProvider = StartupServices().BuildServiceProvider();

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.MainWindow = new MainWindowView
      {
        DataContext = serviceProvider.GetService<MainWindowViewModel>()
      };
    }

    base.OnFrameworkInitializationCompleted();
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