using System;
using System.Collections.Generic;
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

    private short                  _code;
    private float                  _value;
    private TmFlags                _flags = TmFlags.Invalid;
    private byte                   _width; // todo сейчас вообще не используется, а надо ли?
    private byte                   _precision = 3;
    private string                 _unit      = "";
    private TmTeleregulation       _teleregulation;
    private TmAnalogTechParameters _techParameters;


    public short Code
    {
      get => _code;
      set => SetPropertyValueAndRefresh(ref _code, value);
    }

    public float Value
    {
      get => _value;
      set => SetPropertyValueAndRefresh(ref _value, value);
    }

    public TmFlags Flags
    {
      get => _flags;
      set => SetPropertyValueAndRefresh(ref _flags, value);
    }

    public byte Width
    {
      get => _width;
      set => SetPropertyValueAndRefresh(ref _width, value);
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

    public TmTeleregulation Teleregulation
    {
      get => _teleregulation;
      private set => SetPropertyValueAndRefresh(ref _teleregulation, value);
    }

    public TmAnalogTechParameters TechParameters
    {
      get => _techParameters;
      private set => SetPropertyValueAndRefresh(ref _techParameters, value);
    }

    public bool IsUnreliable      => Flags.HasFlag(TmFlags.Unreliable);
    public bool IsInvalid         => Flags.HasFlag(TmFlags.Invalid);
    public bool IsManuallyBlocked => Flags.HasFlag(TmFlags.ManuallyBlocked);
    public bool IsManuallySet     => Flags.HasFlag(TmFlags.ManuallySet);
    public bool IsRequested       => Flags.HasFlag(TmFlags.Requested);
    public bool IsResChannel      => Flags.HasFlag(TmFlags.ResChannel);
    public bool IsAbnormal        => Flags.HasFlag(TmFlags.Abnormal);
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


    public override string ValueToDisplay => ValueWithUnitString;
    
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
        if (IsAbnormal)
        {
          flagsToDisplay.Add("Несоответствие нормальному режиму");
        }
        if (IsManuallyBlocked)
        {
          flagsToDisplay.Add("Заблокировано оператором");
        }
        if (IsManuallySet)
        {
          flagsToDisplay.Add("Установлено вручную");
        }
        if (IsAlarmLevel1)
        {
          flagsToDisplay.Add("Уставка оперативного состояния");
        }
        if (IsAlarmLevel2)
        {
          flagsToDisplay.Add("Уставка предупредительная 2");
        }
        if (IsAlarmLevel3)
        {
          flagsToDisplay.Add("Уставка предупредительная 1");
        }
        if (IsAlarmLevel4)
        {
          flagsToDisplay.Add("Уставка аварийная");
        }

        return flagsToDisplay;
      }
    }


    public TmAnalog(int ch, int rtu, int point)
      : base(TmType.Analog, ch, rtu, point)
    {
    }


    public TmAnalog(TmAddr addr)
      : base(addr)
    {
    }


    public override int GetHashCode()
    {
      return base.GetHashCode();
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
    

    protected override void SetTmcObjectProperties(string key, string value)
    {
      base.SetTmcObjectProperties(key, value);
      
      switch (key)
      {
        case "Units":
          Unit = value;
          break;
        case "Format":
        {
          var formatParts = value.Split('.');
          if (formatParts.Length > 1                        &&
              byte.TryParse(formatParts[0], out byte width) &&
              byte.TryParse(formatParts[1], out byte precision))
          {
            Width     = width;
            Precision = precision;
          }

          break;
        }
        case "FBFlagsC":
          Teleregulation = GetRegulationFromNativeFlag(value);
          break;
        case "Class":
        {
          if (byte.TryParse(value, out var classId))
          {
            ClassId = classId;
          }

          break;
        }
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


    public void FromDatagram(byte[] buf)
    {
      IsInit = true;
      Value  = BitConverter.ToSingle(buf, 14);
      Flags  = (TmFlags)BitConverter.ToInt16(buf, 18);
    }


    public void SetTmcTechParameters(TmNativeDefs.TAnalogTechParms parameters)
    {
      TechParameters = new TmAnalogTechParameters(parameters.MinVal,
                                                  parameters.MaxVal,
                                                  parameters.Nominal,
                                                  parameters.ZoneLim[0],
                                                  parameters.ZoneLim[1],
                                                  parameters.ZoneLim[2],
                                                  parameters.ZoneLim[3],
                                                  parameters.AlrPresent > 0,
                                                  parameters.AlrInUse   > 0);
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

      UpdateTechParametersWithDto(dto.MapToTmAnalogTechParametersDto());
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

      var formatParts = string.IsNullOrEmpty(format) 
                          ? Array.Empty<string>() 
                          : format.Split('.');
      
      if (formatParts.Length > 1                        &&
          byte.TryParse(formatParts[0], out byte width) &&
          byte.TryParse(formatParts[1], out byte precision))
      {
        Width     = width;
        Precision = precision;
      }

      if (!string.IsNullOrEmpty(provider))
      {
        HasTmProvider = true;
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
      else
      {
        HasTmProvider = false;
      }

      if (classId < 0)
      {
        return;
      }

      ClassId = (byte) classId;
    }


    public void UpdateTechParametersWithDto(TmAnalogTechParametersDto dto)
    {
      TechParameters = TmAnalogTechParameters.CreateFromDto(dto);
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

      UpdateWithDto(dto.MapToTmAnalogDto());
      UpdatePropertiesWithDto(dto.MapToTmAnalogPropertiesDto());
    }
    
    
    public static TmAnalog CreateFromTmcCommonPointEx(TmNativeDefs.TCommonPoint tmcCommonPoint)
    {
      var tmAnalog = new TmAnalog(tmcCommonPoint.Ch, tmcCommonPoint.RTU, tmcCommonPoint.Point);
      
      TmNativeDefs.TAnalogPoint tmcAnalogPoint;
      try
      {
        tmcAnalogPoint = TmNativeUtil.GetAnalogPointFromCommonPoint(tmcCommonPoint);
      }
      catch (ArgumentException)
      {
        return tmAnalog;
      }
      
      tmAnalog.IsInit = (tmcCommonPoint.TM_Flags != 0xFFFF);
      tmAnalog.Value  = tmcAnalogPoint.AsFloat;
      tmAnalog.Code   = tmcAnalogPoint.AsCode;
      tmAnalog.Flags  = (TmFlags) tmcAnalogPoint.Flags;
      tmAnalog.ChangeTime = DateUtil.GetDateTimeFromTimestampWithEpochCheck(tmcCommonPoint.tm_local_ut,
                                                                            tmcCommonPoint.tm_local_ms);
      tmAnalog.Width     = (byte) (tmcAnalogPoint.Format & 0x0F);
      tmAnalog.Precision = (byte) (tmcAnalogPoint.Format >> 4);

      tmAnalog.Unit = EncodingUtil.Cp866BytesToUtf8String(tmcAnalogPoint.Unit);

      tmAnalog.Name = tmcCommonPoint.name;

      return tmAnalog;
    }
  }
}