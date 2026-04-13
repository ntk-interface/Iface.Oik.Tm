using System;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class TmAnalogRetroBase
{

  internal static T Create<T>(TmNativeDefsUnsafe.TAnalogPoint tAnalogPoint, 
                              DateTime dateTime) 
    where T : TmAnalogRetroBase, new()
  {
    var retro = new T();

    var serverTime = TmNative.uxgmtime2uxtime(TmNativeUtil.GetUtcTimestampFromDateTime(dateTime));
    
    retro.Intialize(tAnalogPoint.AsFloat, tAnalogPoint.Flags, serverTime, tAnalogPoint.AsCode);
    
    return retro;
  }
  
  protected abstract void Intialize(float value, short flags, long timestamp, short? code = null);
}