using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Iface.Oik.Tm.Utils
{
  public class TmNotifyPropertyChanged : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;


    public virtual void Refresh()
    {
      NotifyOfPropertyChange(string.Empty);
    }


    protected virtual void SetPropertyValue<TValue>(ref TValue                field,
                                                    TValue                    value,
                                                    [CallerMemberName] string propertyName = null)
    {
      if (Equals(field, value)) return;
      field = value;
      NotifyOfPropertyChange(propertyName);
    }


    protected virtual void NotifyOfPropertyChange([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}