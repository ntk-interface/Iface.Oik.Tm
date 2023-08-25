using System;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAccum : TmTag
  {
    public static readonly string InvalidValueString = "???";

    private short   _code;
    private float   _value;
    private float   _load;
    private TmFlags _flags     = TmFlags.Invalid;
    private byte    _precision = 3;
    private string  _unit      = "";
    private byte    _width;

    public short Code
    {
      get => _code;
      set => SetPropertyValue(ref _code, value);
    }
    
    public float Value
    {
      get => _value;
      set => SetPropertyValue(ref _value, value);
    }

    public float Load
    {
      get => _load;
      set => SetPropertyValue(ref _load, value);
    }

    public TmFlags Flags
    {
      get => _flags;
      set => SetPropertyValueAndRefresh(ref _flags, value);
    }

    public byte Precision
    {
      get => _precision;
      set => SetPropertyValueAndRefresh(ref _precision, value);
    }

    public string Unit
    {
      get => _unit;
      set => SetPropertyValueAndRefresh(ref _unit, value);
    }

    public byte Width
    {
      get => _width;
      set => SetPropertyValueAndRefresh(ref _width, value);
    }
    
    public bool IsUnreliable      => Flags.HasFlag(TmFlags.Unreliable);
    public bool IsInvalid         => Flags.HasFlag(TmFlags.Invalid);
    public bool IsManuallyBlocked => Flags.HasFlag(TmFlags.ManuallyBlocked);
    public bool IsManuallySet     => Flags.HasFlag(TmFlags.ManuallySet);
    public bool IsRequested       => Flags.HasFlag(TmFlags.Requested);
    public bool IsResChannel      => Flags.HasFlag(TmFlags.ResChannel);
    public bool IsUnacked         => Flags.HasFlag(TmFlags.Unacked);
    public bool IsTmStreaming     => Flags.HasFlag(TmFlags.TmStreaming);

    public string ValueString => (IsInit)
                                   ? Value.ToString("0." + new string('0', Precision))
                                   : InvalidValueString;


    public string ValueWithUnitString => (IsInit)
                                           ? ValueString + " " + Unit
                                           : InvalidValueString;

    public string LoadString => (IsInit)
                                  ? Load.ToString("0." + new string('0', Precision))
                                  : InvalidValueString;


    public string LoadWithUnitString => (IsInit)
                                          ? LoadString + " " + Unit
                                          : InvalidValueString;

    public override string ValueToDisplay => ValueWithUnitString;
    public          string LoadToDisplay  => LoadWithUnitString;

    public override List<string> FlagsToDisplay
    {
      get
      {
        var flagsToDisplay = new List<string>();

        if (IsUnreliable)
        {
          flagsToDisplay.Add("Неактуальное значение (NT)");
        }

        if (IsInvalid)
        {
          flagsToDisplay.Add("Недействительное значение (IV)");
        }

        if (IsResChannel)
        {
          flagsToDisplay.Add("Взят с резерва");
        }

        if (IsRequested)
        {
          flagsToDisplay.Add("Идет опрос");
        }

        if (IsManuallyBlocked)
        {
          flagsToDisplay.Add("Заблокировано оператором");
        }

        if (IsManuallySet)
        {
          flagsToDisplay.Add("Установлено вручную");
        }

        return flagsToDisplay;
      }
    }

    public TmAccum(int ch, int rtu, int point)
      : base(TmType.Accum, ch, rtu, point)
    {
    }

    public TmAccum(TmAddr addr)
      : base(addr)
    {
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }


    public override bool Equals(object obj)
    {
      return Equals(obj as TmAccum);
    }


    public bool Equals(TmAccum comparison)
    {
      if (ReferenceEquals(comparison, null))
      {
        return false;
      }

      if (ReferenceEquals(this, comparison))
      {
        return true;
      }

      return TmAddr == comparison.TmAddr    &&
             Value.Equals(comparison.Value) &&
             Load.Equals(comparison.Load)   &&
             Flags == comparison.Flags;
    }


    public static bool operator ==(TmAccum left, TmAccum right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }

      return left.Equals(right);
    }


    public static bool operator !=(TmAccum left, TmAccum right)
    {
      return !(left == right);
    }


    public bool HasFlag(TmFlags flags)
    {
      return Flags.HasFlag(flags);
    }


    public void FromTmcCommonPoint(TmNativeDefs.TCommonPoint tmcCommonPoint)
    {
      TmNativeDefs.TAccumPoint tmcAccumPoint;
      try
      {
        tmcAccumPoint = TmNativeUtil.GetAccumPointFromCommonPoint(tmcCommonPoint);
      }
      catch (ArgumentException)
      {
        return;
      }

      IsInit = (tmcCommonPoint.TM_Flags != 0xFFFF);
      Value  = tmcAccumPoint.Value;
      Load   = tmcAccumPoint.Load;
      Flags  = (TmFlags) tmcAccumPoint.Flags;
      ChangeTime = DateUtil.GetDateTimeFromTimestampWithEpochCheck(tmcCommonPoint.tm_local_ut,
                                                                   tmcCommonPoint.tm_local_ms);
      Precision = (byte) (tmcAccumPoint.Format >> 4);
    }

    public static TmAccum CreateFromTmcCommonPointEx(TmNativeDefs.TCommonPoint tmcCommonPoint)
    {
      var tmAccum = new TmAccum(tmcCommonPoint.Ch, tmcCommonPoint.RTU, tmcCommonPoint.Point);

      TmNativeDefs.TAccumPoint tmcAccumPoint;
      try
      {
        tmcAccumPoint = TmNativeUtil.GetAccumPointFromCommonPoint(tmcCommonPoint);
      }
      catch (ArgumentException)
      {
        return tmAccum;
      }

      tmAccum.IsInit = tmcCommonPoint.TM_Flags != 0xFFFF;
      tmAccum.Value  = tmcAccumPoint.Value;
      tmAccum.Load   = tmcAccumPoint.Load;
      tmAccum.Flags  = (TmFlags) tmcAccumPoint.Flags;
      tmAccum.ChangeTime = DateUtil.GetDateTimeFromTimestampWithEpochCheck(tmcCommonPoint.tm_local_ut,
                                                                           tmcCommonPoint.tm_local_ms);
      tmAccum.Precision = (byte) (tmcAccumPoint.Format >> 4);
      tmAccum.Unit      = EncodingUtil.Cp866BytesToUtf8String(tmcAccumPoint.Unit);

      tmAccum.Name = tmcCommonPoint.name;

      return tmAccum;
    }
    
    public void FromTAccumPoint(TmNativeDefs.TAccumPoint tmcAccumPoint)
    {
      IsInit    = true;
      Value     = tmcAccumPoint.Value;
      Flags     = (TmFlags) tmcAccumPoint.Flags;
      Width     = (byte) (tmcAccumPoint.Format & 0x0F);
      Precision = (byte) (tmcAccumPoint.Format >> 4);
    }
  }
}