using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GuiApp.AvaloniaUI.Views;
using GuiApp.Shared;
using GuiApp.Shared.ViewModels;
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
    var serviceProvider = SharedStartup.StartupServices()
                                       .BuildServiceProvider();

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.MainWindow = new MainWindowView
      {
        DataContext = serviceProvider.GetService<MainWindowViewModel>()
      };
    }

    base.OnFrameworkInitializationCompleted();
  }
}