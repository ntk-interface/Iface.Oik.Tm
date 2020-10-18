using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAnalogTechParameters
  {
    public static readonly float InvalidValue = float.MaxValue;

    public float  Min        { get; set; }
    public float  Max        { get; set; }
    public float  Nominal    { get; set; }
    public float? MinAlarm   { get; set; }
    public float? MinWarning { get; set; }
    public float? MaxWarning { get; set; }
    public float? MaxAlarm   { get; set; }

    public bool HasAlarm     { get; set; }
    public bool IsAlarmInUse { get; set; }


    public float MinAlarmOrInvalid   => MinAlarm   ?? InvalidValue;
    public float MinWarningOrInvalid => MinWarning ?? InvalidValue;
    public float MaxWarningOrInvalid => MaxWarning ?? InvalidValue;
    public float MaxAlarmOrInvalid   => MaxAlarm   ?? InvalidValue;


    public TmAnalogTechParameters(float  min,
                                  float  max,
                                  float  nominal,
                                  float? minAlarm     = null,
                                  float? minWarning   = null,
                                  float? maxWarning   = null,
                                  float? maxAlarm     = null,
                                  bool   hasAlarm     = false,
                                  bool   isAlarmInUse = false)
    {
      Min          = min;
      Max          = max;
      Nominal      = nominal;
      MinAlarm     = NullIfInvalid(minAlarm);
      MinWarning   = NullIfInvalid(minWarning);
      MaxWarning   = NullIfInvalid(maxWarning);
      MaxAlarm     = NullIfInvalid(maxAlarm);
      HasAlarm     = hasAlarm;
      IsAlarmInUse = isAlarmInUse;
    }


    public static TmAnalogTechParameters CreateFromDto(TmAnalogTechParametersDto dto)
    {
      return new TmAnalogTechParameters(dto.TprMinVal,
                                        dto.TprMaxVal,
                                        dto.TprNominal,
                                        dto.TprZoneDLow,
                                        dto.TprZoneCLow,
                                        dto.TprZoneCHigh,
                                        dto.TprZoneDHigh,
                                        dto.TprAlrPresent,
                                        dto.TprAlrInUse);
    }


    private static float? NullIfInvalid(float? value)
    {
      if (value == null ||
          value.Value.Equals(InvalidValue))
      {
        return null;
      }
      return value;
    }
  }
}