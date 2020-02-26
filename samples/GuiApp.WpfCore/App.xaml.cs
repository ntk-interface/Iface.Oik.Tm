using System.Text;
using System.Windows;
using GuiApp.Shared;
using GuiApp.WpfCore.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GuiApp.WpfCore
{
  public partial class App
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // требуется для работы с кодировкой Win-1251
      
      var services = SharedStartup.StartupServices();

      services.AddSingleton<MainWindowView>();

      services.BuildServiceProvider()
              .GetService<MainWindowView>()
              .Show();
    }


    protected override void OnExit(ExitEventArgs e)
    {
      base.OnExit(e);
    }
  }
}