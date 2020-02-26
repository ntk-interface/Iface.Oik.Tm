using GuiApp.Shared.ViewModels;

namespace GuiApp.WpfCore.Views
{
  public partial class MainWindowView
  {
    public MainWindowView(MainWindowViewModel viewModel)
    {
      InitializeComponent();

      DataContext = viewModel;
    }
  }
}