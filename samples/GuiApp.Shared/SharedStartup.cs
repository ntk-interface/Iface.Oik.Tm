using GuiApp.Shared.ViewModels;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GuiApp.Shared
{
  public static class SharedStartup
  {
    public static IServiceCollection StartupServices()
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
}