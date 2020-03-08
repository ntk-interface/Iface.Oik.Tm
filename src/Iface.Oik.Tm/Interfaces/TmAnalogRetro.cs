using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public readonly struct TmAnalogRetro : ITmAnalogRetro
  {
    private readonly bool _invalidFlags;

    public float    Value { get; }
    public TmFlags  Flags { get; }
    public DateTime Time  { get; }

    public bool IsValid => Flags >= 0 && 
                           !Value.Equals(float.MaxValue) && 
                           !_invalidFlags;


    public TmAnalogRetro(float value, short flags, long timestamp)
    {
      Value         = value;
      Flags         = (TmFlags) flags;
      Time          = DateUtil.GetDateTimeFromTimestamp(timestamp);
      _invalidFlags = (flags < 0);
    }
  }
}