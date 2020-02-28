using System;
using System.Globalization;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAnalog : TmTag
  {
    public static readonly string InvalidValueString = "???";


    public short   Code      { get; set; }
    public float   Value     { get; set; }
    public TmFlags Flags     { get; set; } = TmFlags.Invalid;
    public byte    Width     { get; set; } // todo сейчас вообще не используется, а надо ли?
    public byte    Precision { get; set; } = 3;
    public string  Unit      { get; set; } = "";

    public TmTeleregulation Teleregulation { get; set; }

    public bool IsUnreliable      => Flags.HasFlag(TmFlags.Unreliable);
    public bool IsInvalid         => Flags.HasFlag(TmFlags.Invalid);
    public bool IsManuallyBlocked => Flags.HasFlag(TmFlags.ManuallyBlocked);
    public bool IsManuallySet     => Flags.HasFlag(TmFlags.ManuallySet);
    public bool IsRequested       => Flags.HasFlag(TmFlags.Requested);
    public bool IsResChannel      => Flags.HasFlag(TmFlags.ResChannel);
    public bool HasTeleregulation => Flags.HasFlag(TmFlags.HasTeleregulation);
    public bool IsUnacked         => Flags.HasFlag(TmFlags.Unacked);
    public bool HasAlarm          => Flags.HasFlag(TmFlags.HasAlarm);
    public bool IsAlarmLevel1     => Flags.HasFlag(TmFlags.LevelA);
    public bool IsAlarmLevel2     => Flags.HasFlag(TmFlags.LevelB);
    public bool IsAlarmLevel3     => Flags.HasFlag(TmFlags.LevelC);
    public bool IsAlarmLevel4     => Flags.HasFlag(TmFlags.LevelD);

    public bool HasTeleregulationByStep  => Teleregulation == TmTeleregulation.Step;
    public bool HasTeleregulationByCode  => Teleregulation == TmTeleregulation.Code;
    public bool HasTeleregulationByValue => Teleregulation == TmTeleregulation.Value;

    public string ValueString => (IsInit)
                                   ? Value.ToString("0." + new string('0', Precision))
                                   : InvalidValueString;


    public string ValueWithUnitString => (IsInit)
                                           ? ValueString + " " + Unit
                                           : InvalidValueString;


    public byte TmcRegulationType
    {
      get
      {
        switch (Teleregulation)
        {
          case TmTeleregulation.Step:
            return (byte) TmNativeDefs.AnalogRegulationType.Step;
          case TmTeleregulation.Code:
            return (byte) TmNativeDefs.AnalogRegulationType.Code;
          case TmTeleregulation.Value:
            return (byte) TmNativeDefs.AnalogRegulationType.Value;
          default:
            return 0;
        }
      }
    }


    public override string ValueToDisplay => ValueWithUnitString;


    public TmAnalog(int ch, int rtu, int point)
      : base(TmType.Analog, ch, rtu, point)
    {
    }


    public TmAnalog(TmAddr addr)
      : base(addr)
    {
    }


    public override bool Equals(object obj)
    {
      return Equals(obj as TmAnalog);
    }


    public bool Equals(TmAnalog comparison)
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
             Flags == comparison.Flags;
    }


    public static bool operator ==(TmAnalog left, TmAnalog right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }

      return left.Equals(right);
    }


    public static bool operator !=(TmAnalog left, TmAnalog right)
    {
      return !(left == right);
    }


    public override string ToString()
    {
      return $"{Name} = {ValueString} {Unit}";
    }


    public bool HasFlag(TmFlags flags)
    {
      return Flags.HasFlag(flags);
    }


    public string ValueStringWithPrecision(int precision)
    {
      return (IsInit)
               ? Value.ToString("0." + new string('0', precision))
               : InvalidValueString;
    }


    public string ValueStringWithFormat(string format)
    {
      return (IsInit)
               ? Value.ToString(format)
               : InvalidValueString;
    }


    public string ValueWithUnitStringWithPrecision(int precision)
    {
      return (IsInit)
               ? ValueStringWithPrecision(precision) + " " + Unit
               : InvalidValueString;
    }


    public string ValueWithUnitStringWithFormat(string format)
    {
      return (IsInit)
               ? Value.ToString(format) + " " + Unit
               : InvalidValueString;
    }


    public string FakeValueString(float fakeValue, int precision = -1)
    {
      if (precision < 0)
      {
        precision = Precision;
      }

      return fakeValue.ToString("0." + new string('0', precision));
    }


    public string FakeValueWithUnitString(float fakeValue, int precision = -1)
    {
      return FakeValueString(fakeValue, precision) + " " + Unit;
    }


    public string FakeValueStringWithFormat(float fakeValue, string format)
    {
      return fakeValue.ToString(format);
    }


    public static TmTeleregulation GetRegulationFromNativeFlag(string nativeValue)
    {
      if (!short.TryParse(nativeValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out short flag))
      {
        return TmTeleregulation.None;
      }

      switch ((TmNativeDefs.AnalogRegulationFlag) flag)
      {
        case TmNativeDefs.AnalogRegulationFlag.Step:
          return TmTeleregulation.Step;
        case TmNativeDefs.AnalogRegulationFlag.Code:
          return TmTeleregulation.Code;
        case TmNativeDefs.AnalogRegulationFlag.Value:
          return TmTeleregulation.Value;
        default:
          return TmTeleregulation.None;
      }
    }


    public void FromTmcCommonPoint(TmNativeDefs.TCommonPoint tmcCommonPoint)
    {
      TmNativeDefs.TAnalogPoint tmcAnalogPoint;
      try
      {
        tmcAnalogPoint = TmNativeUtil.GetAnalogPointFromCommonPoint(tmcCommonPoint);
      }
      catch (ArgumentException)
      {
        return;
      }

      IsInit = (tmcCommonPoint.TM_Flags != 0xFFFF);
      Value  = tmcAnalogPoint.AsFloat;
      Flags  = (TmFlags) tmcAnalogPoint.Flags;
      ChangeTime = DateUtil.GetDateTimeFromTimestampWithEpochCheck(tmcCommonPoint.tm_local_ut,
                                                                   tmcCommonPoint.tm_local_ms);
      Width     = (byte) (tmcAnalogPoint.Format & 0x0F);
      Precision = (byte) (tmcAnalogPoint.Format >> 4);
    }
    
    
    public void FromTAnalogPoint(TmNativeDefs.TAnalogPoint tmcAnalogPoint)
    {
      IsInit    = true;
      Code      = tmcAnalogPoint.AsCode;
      Value     = tmcAnalogPoint.AsFloat;
      Flags     = (TmFlags) tmcAnalogPoint.Flags;
      Width     = (byte) (tmcAnalogPoint.Format & 0x0F);
      Precision = (byte) (tmcAnalogPoint.Format >> 4);
    }


    public void UpdateWithDto(TmAnalogDto dto)
    {
      if (dto == null) return;

      UpdateWithDto(dto.VVal,
                    dto.Flags,
                    dto.ChangeTime);
    }


    public void UpdateWithDto(float value, int flags, DateTime? changeTime)
    {
      IsInit     = true;
      Value      = value;
      Flags      = (TmFlags) (flags & 0x0000_FFFF); // 3 и 4 байты флагов - служебные, не для клиента
      ChangeTime = changeTime.NullIfEpoch();
    }


    public void UpdatePropertiesWithDto(TmAnalogPropertiesDto dto)
    {
      if (dto?.Name == null) return;

      UpdatePropertiesWithDto(dto.Name,
                              dto.VUnit,
                              dto.VFormat,
                              dto.ClassId,
                              dto.Provider);
    }


    public void UpdatePropertiesWithDto(string name,
                                        string unit,
                                        string format,
                                        short  classId,
                                        string provider)
    {
      Name = name;
      Unit = unit.TrimEnd();

      var formatParts = format.Split('.');
      if (formatParts.Length > 1                        &&
          byte.TryParse(formatParts[0], out byte width) &&
          byte.TryParse(formatParts[1], out byte precision))
      {
        Width     = width;
        Precision = precision;
      }

      if (!string.IsNullOrEmpty(provider))
      {
        provider.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)
                .ForEach(property =>
                         {
                           var kvp = property.Split('=');
                           if (kvp.Length != 2)
                           {
                             return;
                           }

                           if (kvp[0] == "FBFlagsC")
                           {
                             Teleregulation = GetRegulationFromNativeFlag(kvp[1]);
                           }
                         });
      }

      if (classId < 0)
      {
        return;
      }

      ClassId = (byte) classId;
    }


    public static TmAnalog CreateFromTmTreeDto(TmAnalogTmTreeDto dto)
    {
      if (dto?.Name == null) return null;

      var analog = new TmAnalog(dto.Ch, dto.Rtu, dto.Point);
      analog.UpdateWithTmTreeDto(dto);

      return analog;
    }


    public void UpdateWithTmTreeDto(TmAnalogTmTreeDto dto)
    {
      if (dto?.Name == null) return;

      UpdateWithDto(dto.AdaptToTmAnalogDto());
      UpdatePropertiesWithDto(dto.AdaptToTmAnalogPropertiesDto());
    }
  }
}