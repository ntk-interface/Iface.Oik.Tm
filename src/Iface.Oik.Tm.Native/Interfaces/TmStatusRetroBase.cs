using System;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class TmStatusRetroBase
{
  internal static T Create<T>(TmNativeDefsUnsafe.TStatusPoint point, long timestamp)
    where T : TmStatusRetroBase, new()
  {
    var retro = new T();
    
    retro.Initialize(point.Status, point.Status, timestamp);
    
    return retro;
  }

  protected abstract void Initialize(short status,
                                     short flags,
                                     long  timestamp);
}