using System;
using System.Collections.Generic;
using Iface.Oik.Tm.Dto;
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
    private TmFlags _flags = TmFlags.Invalid;
    private byte    _width;     // todo сейчас вообще не используется, а надо ли?
    private byte    _loadWidth; // todo сейчас вообще не используется, а надо ли?
    private byte    _precision     = 3;
    private byte    _loadPrecision = 3;
    private string  _unit          = "";

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

    public byte Width
    {
      get => _width;
      set => SetPropertyValueAndRefresh(ref _width, value);
    }

    public byte LoadWidth
    {
      get => _loadWidth;
      set => SetPropertyValueAndRefresh(ref _loadWidth, value);
    }

    public byte Precision
    {
      get => _precision;
      set => SetPropertyValueAndRefresh(ref _precision, value);
    }

    public byte LoadPrecision
    {
      get => _loadPrecision;
      set => SetPropertyValueAndRefresh(ref _loadPrecision, value);
    }

    public string Unit
    {
      get => _unit;
      set => SetPropertyValueAndRefresh(ref _unit, value);
    }
    
    public bool IsUnreliable      => Flags.HasFlag(TmFlags.Unreliable);
    public bool IsInvalid         => Flags.HasFlag(TmFlags.Invalid);
    public bool IsManuallyBlocked => Flags.HasFlag(TmFlags.ManuallyBlocked);
    public bool IsManuallySet     => Flags.HasFlag(TmFlags.ManuallySet);
    public bool IsRequested       => Flags.HasFlag(TmFlags.Requested);
    public bool IsResChannel      => Flags.HasFlag(TmFlags.ResChannel);
    public bool IsUnacked         => Flags.HasFlag(TmFlags.Unacked);
    public bool IsTmStreaming     => Flags.HasFlag(TmFlags.TmStreaming);

    public string ValueString => IsInit
                                  ? Value.ToString("0." + new string('0', Precision))
                                  : InvalidValueString;

    public string LoadString => IsInit
                                  ? Load.ToString("0." + new string('0', LoadPrecision))
                                  : InvalidValueString;

    public string ValueWithUnitString => (IsInit)
                                           ? ValueString + " " + Unit
                                           : InvalidValueString;

    public string LoadWithUnitString => (IsInit)
                                          ? LoadString + " " + Unit
                                          : InvalidValueString;

    public override string ValueToDisplay => (IsInit)
                                               ? $"{ValueString} ({LoadString}) {Unit}"
                                               : InvalidValueString;
    
    public string LoadToDisplay  => LoadWithUnitString;
    
    public override bool HasProblems => !IsInit || IsUnreliable || IsInvalid;

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


    public string LoadStringWithPrecision(int precision)
    {
      return (IsInit)
               ? Load.ToString("0." + new string('0', precision))
               : InvalidValueString;
    }


    public string LoadStringWithFormat(string format)
    {
      return (IsInit)
               ? Load.ToString(format)
               : InvalidValueString;
    }


    public string LoadWithUnitStringWithPrecision(int precision)
    {
      return (IsInit)
               ? LoadStringWithPrecision(precision) + " " + Unit
               : InvalidValueString;
    }


    public string LoadWithUnitStringWithFormat(string format)
    {
      return (IsInit)
               ? Load.ToString(format) + " " + Unit
               : InvalidValueString;
    }


    public string FakeLoadString(float fakeLoad, int precision = -1)
    {
      if (precision < 0)
      {
        precision = Precision;
      }

      return fakeLoad.ToString("0." + new string('0', precision));
    }


    public string FakeLoadWithUnitString(float fakeLoad, int precision = -1)
    {
      return FakeLoadString(fakeLoad, precision) + " " + Unit;
    }


    public string FakeLoadStringWithFormat(float fakeLoad, string format)
    {
      return fakeLoad.ToString(format);
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
      Width     = (byte) (tmcAccumPoint.Format & 0x0F);
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
      tmAccum.Width    = (byte) (tmcAccumPoint.Format & 0x0F);
      tmAccum.Precision = (byte) (tmcAccumPoint.Format >> 4);
      tmAccum.Unit      = EncodingUtil.Cp866BytesToUtf8String(tmcAccumPoint.Unit);

      tmAccum.Name = EncodingUtil.Win1251BytesToUtf8(tmcCommonPoint.name);

      return tmAccum;
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
        case "CounterFormat":
        {
          var formatParts = value.Split('.');
          if (formatParts.Length > 1                        &&
              byte.TryParse(formatParts[0], out byte width) &&
              byte.TryParse(formatParts[1], out byte precision))
          {
            LoadWidth     = width;
            LoadPrecision = precision;
          }

          break;
        }
      }
    }
    
    public void FromTAccumPoint(TmNativeDefs.TAccumPoint tmcAccumPoint)
    {
      if (tmcAccumPoint.Flags == -1)
      {
        Flags  |= TmFlags.Invalid;
        IsInit =  false;
        return;
      }
      
      IsInit    = true;
      Value     = tmcAccumPoint.Value;
      Load      = tmcAccumPoint.Load;
      Flags     = (TmFlags) tmcAccumPoint.Flags;
      Width     = (byte) (tmcAccumPoint.Format & 0x0F);
      Precision = (byte) (tmcAccumPoint.Format >> 4);
    }


    public void UpdateWithDto(TmAccumDto dto)
    {
      if (dto == null) return;

      UpdateWithDto(dto.VVal,
                    dto.VLoad,
                    dto.Flags,
                    dto.ChangeTime);
    }


    public void UpdateWithDto(float value, float load, int flags, DateTime? changeTime)
    {
      IsInit     = true;
      Value      = value;
      Load       = load;
      Flags      = (TmFlags) (flags & 0x0000_FFFF); // 3 и 4 байты флагов - служебные, не для клиента
      ChangeTime = changeTime.NullIfEpoch();
    }
    
    
    public void UpdatePropertiesWithDto(TmAccumPropertiesDto dto)
    {
      if (dto?.Name == null) return;

      UpdatePropertiesWithDto(dto.Name,
                              dto.VUnit,
                              dto.VFormat,
                              dto.VCounterFormat,
                              dto.Provider);
    }


    public void UpdatePropertiesWithDto(string name,
                                        string unit,
                                        string format,
                                        string loadFormat,
                                        string provider)
    {
      Name = name;
      Unit = unit.TrimEnd();

      var formatParts = string.IsNullOrEmpty(format) 
                          ? Array.Empty<string>() 
                          : format.Split('.');
      if (formatParts.Length > 1                        &&
          byte.TryParse(formatParts[0], out var width) &&
          byte.TryParse(formatParts[1], out var precision))
      {
        Width     = width;
        Precision = precision;
      }

      var loadFormatParts = string.IsNullOrEmpty(loadFormat) 
                          ? Array.Empty<string>() 
                          : loadFormat.Split('.');
      if (loadFormatParts.Length > 1                        &&
          byte.TryParse(loadFormatParts[0], out var loadWidth) &&
          byte.TryParse(loadFormatParts[1], out var loadPrecision))
      {
        LoadWidth     = loadWidth;
        LoadPrecision = loadPrecision;
      }

      HasTmProvider = !string.IsNullOrEmpty(provider);
    }


    public static TmAccum CreateFromTmTreeDto(TmAccumTmTreeDto dto)
    {
      if (dto?.Name == null) return null;

      var analog = new TmAccum(dto.Ch, dto.Rtu, dto.Point);
      analog.UpdateWithTmTreeDto(dto);

      return analog;
    }


    public void UpdateWithTmTreeDto(TmAccumTmTreeDto dto)
    {
      if (dto?.Name == null) return;

      UpdateWithDto(dto.MapToTmAccumDto());
      UpdatePropertiesWithDto(dto.MapToTmAccumPropertiesDto());
    }
  }
}