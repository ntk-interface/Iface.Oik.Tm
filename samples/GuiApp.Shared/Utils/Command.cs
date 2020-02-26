using System;
using System.Windows.Input;

namespace GuiApp.Shared.Utils
{
  public class Command : ICommand
  {
    private readonly Func<object, bool> _canExecute;
    private readonly Action<object>     _execute;


    public Command(Action<object> execute)
    {
      _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }


    public Command(Action execute)
      : this(o => execute())
    {
      if (execute == null) throw new ArgumentNullException(nameof(execute));
    }


    public Command(Action<object> execute, Func<object, bool> canExecute)
      : this(execute)
    {
      _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
    }


    public Command(Action execute, Func<bool> canExecute)
      : this(o => execute(), o => canExecute())
    {
      if (execute == null) throw new ArgumentNullException(nameof(execute));

      if (canExecute == null) throw new ArgumentNullException(nameof(canExecute));
    }


    public bool CanExecute(object parameter)
    {
      if (_canExecute != null)
      {
        return _canExecute(parameter);
      }

      return true;
    }


    public event EventHandler CanExecuteChanged;


    public void Execute(object parameter)
    {
      _execute(parameter);
    }


    public void ChangeCanExecute()
    {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
  }
}