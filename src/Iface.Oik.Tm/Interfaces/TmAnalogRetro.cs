using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces;

public class TmAnalogRetro : TmAnalogRetroBase, ITmAnalogRetro
{
  private bool _invalidFlags;

  public float    Value { get; private set; }
  public int?     Code  { get; private set; }
  public TmFlags  Flags { get; private set; }
  public DateTime Time  { get; private set; }

  public bool IsValid => Flags >= 0                         && 
                         !Flags.HasFlag(TmFlags.Unreliable) &&
                         !Value.Equals(float.MaxValue)      && 
                         !_invalidFlags;

  public bool IsUnreliable => !IsValid;

  public TmAnalogRetro(){}
  
  public TmAnalogRetro(float value, short flags, long timestamp, short? code = null)
  {
    Value         = value;
    Flags         = (TmFlags) flags;
    Time          = DateUtil.GetDateTimeFromTimestamp(timestamp);
    Code          = code;
    _invalidFlags = (flags < 0);
  }

  
  public static      TmAnalogRetro UnreliableValue = new TmAnalogRetro(float.MaxValue, (short)TmFlags.Unreliable, 0);
  protected override void          Intialize(float value, short flags, long timestamp, short? code = null)
  {
    
    Value         = value;
    Flags         = (TmFlags) flags;
    Time          = DateUtil.GetDateTimeFromTimestamp(timestamp);
    Code          = code;
    _invalidFlags = flags < 0;
  }
}