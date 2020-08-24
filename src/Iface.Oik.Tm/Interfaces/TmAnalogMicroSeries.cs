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


    public TmAnalogMicroSeries(float value, TmAnalogMicroSeriesFlags flags, DateTime time)
    {
      Value = value;
      Flags = flags.HasFlag(TmAnalogMicroSeriesFlags.IsReliable) ? TmFlags.None : TmFlags.Unreliable;
      Time  = time;
    }


    public TmAnalogMicroSeries(float value, short flags, long timestamp)
      : this(value, (TmAnalogMicroSeriesFlags) flags, DateUtil.GetDateTimeFromTimestamp(timestamp))
    {
    }


    public TmAnalogMicroSeries(float value, short flags, DateTime time)
      : this(value, (TmAnalogMicroSeriesFlags) flags, time)
    {
    }
  }
}