using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GuiApp.AvaloniaUI.Views
{
  public class MainWindowView : Window
  {
    public MainWindowView()
    {
      AvaloniaXamlLoader.Load(this);
    }
  }
}