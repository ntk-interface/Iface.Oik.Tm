using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public readonly struct TmAnalogMicroSeries : ITmAnalogRetro
  {
    public float    Value { get; }
    public TmFlags  Flags { get; }
    public DateTime Time  { get; }

    public bool IsUnreliable => Flags.HasFlag(TmFlags.Unreliable);


    public TmAnalogMicroSeries(float value, TmAnalogMicroSeriesFlags flags, long timestamp)
    {
      Value = value;
      Flags = flags.HasFlag(TmAnalogMicroSeriesFlags.IsReliable) ? TmFlags.None : TmFlags.Unreliable;
      Time  = DateUtil.GetDateTimeFromTimestamp(timestamp);
    }
  }
}