using System;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class TmStatusRetroBase
{
  internal static T Create<T>(TmNativeDefsUnsafe.TStatusPoint point, DateTime time)
    where T : TmStatusRetroBase, new()
  {
    var retro = new T();
    
    


    return retro;
  }

  protected abstract void Initialize(short    status,
                                  short    flags,
                                  DateTime time);
}