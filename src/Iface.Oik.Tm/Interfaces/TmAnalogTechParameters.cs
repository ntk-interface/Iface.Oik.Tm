using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAnalogTechParameters
  {
    public float  Min        { get; set; }
    public float  Max        { get; set; }
    public float  Nominal    { get; set; }
    public float? MinAlarm   { get; set; }
    public float? MinWarning { get; set; }
    public float? MaxWarning { get; set; }
    public float? MaxAlarm   { get; set; }

    public bool HasAlarm     { get; set; }
    public bool IsAlarmInUse { get; set; }


    public TmAnalogTechParameters(float min,
                                  float max,
                                  float nominal,
                                  float minAlarm,
                                  float minWarning,
                                  float maxWarning,
                                  float maxAlarm,
                                  bool  hasAlarm,
                                  bool  isAlarmInUse)
    {
      Min          = min;
      Max          = max;
      Nominal      = nominal;
      MinAlarm     = NumericUtil.NullIfMaxValue(minAlarm);
      MinWarning   = NumericUtil.NullIfMaxValue(minWarning);
      MaxWarning   = NumericUtil.NullIfMaxValue(maxWarning);
      MaxAlarm     = NumericUtil.NullIfMaxValue(maxAlarm);
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
  }
}