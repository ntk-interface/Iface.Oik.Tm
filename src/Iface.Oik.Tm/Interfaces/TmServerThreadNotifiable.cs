using System.ComponentModel;
using System.Runtime.CompilerServices;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Interfaces;

public abstract class TmServerThreadNotifiable : TmServerThreadBase, INotifyPropertyChanged
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


  protected virtual void SetPropertyValueAndRefresh<TValue>(ref TValue field,
                                                            TValue     value)
  {
    SetPropertyValue(ref field, value, string.Empty);
  }


  protected virtual void NotifyOfPropertyChange([CallerMemberName] string propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}