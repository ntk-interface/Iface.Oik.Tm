using System;
using System.Text;
using Avalonia;
using Iface.Oik.Tm.Helpers;

namespace GuiApp.AvaloniaUI;

class Program
{
  [STAThread]
  public static void Main(string[] args)
  {
    // TODO убрать после перехода кодировок
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // требуется для работы с кодировкой Win-1251
    
    Tms.InitNativeLibrary();

    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
  }


  public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
                                                           .UsePlatformDetect()
                                                           .WithInterFont()
                                                           .LogToTrace();
}