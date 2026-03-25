using GuiApp.Wpf.ViewModels;

namespace GuiApp.Wpf.Views;

public partial class MainWindowView
{
  public MainWindowView(MainWindowViewModel viewModel)
  {
    InitializeComponent();

    DataContext = viewModel;
  }
}