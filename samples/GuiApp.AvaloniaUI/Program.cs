using System;
using System.Text;
using Avalonia;

namespace GuiApp.AvaloniaUI;

class Program
{
  [STAThread]
  public static void Main(string[] args)
  {
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // требуется для работы с кодировкой Win-1251

    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
  }


  public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
                                                           .UsePlatformDetect()
                                                           .LogToTrace();
}