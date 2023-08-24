using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public readonly struct TmAnalogImpulseArchiveInstant : ITmAnalogRetro
  {
    public float    Value { get; }
    public TmFlags  Flags { get; }
    public DateTime Time  { get; }
    public int?     Code  { get; }

    public bool IsUnreliable => Flags.HasFlag(TmFlags.Unreliable);


    public TmAnalogImpulseArchiveInstant(float value, uint flags, uint timestamp, ushort ms)
    {
      Value = value;
      Flags = (TmFlags) (flags & 0xFF_FF);
      Time  = DateUtil.GetDateTimeFromTimestamp(timestamp, ms);
      Code  = null;
    }
  }


  public readonly struct TmAnalogImpulseArchiveAverage : ITmAnalogRetro
  {
    public float    AvgValue { get; }
    public float    MinValue { get; }
    public float    MaxValue { get; }
    public TmFlags  Flags    { get; }
    public DateTime Time     { get; }
    public int?     Code     { get; }

    public float Value => AvgValue;

    public bool IsUnreliable => Flags.HasFlag(TmFlags.Unreliable);


    public TmAnalogImpulseArchiveAverage(float avg, float min, float max, uint flags, uint timestamp, ushort ms)
    {
      AvgValue = avg;
      MinValue = min;
      MaxValue = max;
      Flags    = (TmFlags) (flags & 0xFF_FF);
      Time     = DateUtil.GetDateTimeFromTimestamp(timestamp, ms);
      Code     = null;
    }
  }
}