namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class TmAnalogMicroSeriesBase
{
  public static T Create<T>(float value, short flags, long timestamp) 
    where T: TmAnalogMicroSeriesBase, new()
  {
    var element = new T();

    element.Initialize(value, flags, timestamp);
    
    return element;
  }

  protected abstract void Initialize(float value, short flag, long timestamp);
}