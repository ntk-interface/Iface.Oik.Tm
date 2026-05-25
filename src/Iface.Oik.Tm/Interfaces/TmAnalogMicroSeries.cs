using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAnalogMicroSeries : TmAnalogMicroSeriesBase, ITmAnalogRetro
  {
    public float    Value { get; private set; }
    public TmFlags  Flags { get; private set; }
    public DateTime Time  { get; private set; }
    public int?     Code  { get; private set; }

    public bool IsUnreliable => Flags.HasFlag(TmFlags.Unreliable);


    public TmAnalogMicroSeries(){}
    
    public TmAnalogMicroSeries(float value, TmAnalogMicroSeriesFlags flags, DateTime time)
    {
      Value = value;
      Flags = flags.HasFlag(TmAnalogMicroSeriesFlags.IsReliable) ? TmFlags.None : TmFlags.Unreliable;
      Time  = time;
      Code  = null;
    }


    public TmAnalogMicroSeries(float value, short flags, long timestamp)
      : this(value, (TmAnalogMicroSeriesFlags)flags, DateUtil.GetDateTimeFromTimestamp(timestamp))
    {
    }


    public TmAnalogMicroSeries(float value, short flags, DateTime time)
      : this(value, (TmAnalogMicroSeriesFlags)flags, time)
    {
    }

    protected override void Initialize(float value, short flag, long timestamp)
    {
      var flags = (TmAnalogMicroSeriesFlags)flag;
      
      Value = value;
      Flags = flags.HasFlag(TmAnalogMicroSeriesFlags.IsReliable) ? TmFlags.None : TmFlags.Unreliable;
      Time  = DateUtil.GetDateTimeFromTimestamp(timestamp);
      Code  = null;
    }
  }
}