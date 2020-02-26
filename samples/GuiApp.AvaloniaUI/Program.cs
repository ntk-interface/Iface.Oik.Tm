using Avalonia;

namespace GuiApp.AvaloniaUI
{
  class Program
  {
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);


    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
                                                             .UsePlatformDetect();
  }
}