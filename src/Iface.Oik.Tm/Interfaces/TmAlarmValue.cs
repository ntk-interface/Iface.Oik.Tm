using Iface.Oik.Tm.Dto;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAlarmValue : TmAlarm
  {
    private readonly bool  _isCompareGreaterThan;

    public float CompareValue { get; }

    public override string ThresholdName => (_isCompareGreaterThan ? ">" : "<") +
                                            " "                                +
                                            TmAnalog.FakeValueWithUnitString(CompareValue);

    public override bool IsEditable => true;


    public TmAlarmValue(TmAlarmType type,
                        short       id,
                        string      name,
                        short       importance,
                        short       isInUse,
                        bool        isActive,
                        TmAnalog    tmAnalog,
                        float       compareValue,
                        short       compareSign)
      : base(type, id, name, importance, isInUse, isActive, tmAnalog)
    {
      CompareValue         = compareValue;
      _isCompareGreaterThan = (compareSign == 0);
    }


    public static TmAlarmValue CreateTmAlarmValueFromDto(TmAlarmDto dto)
    {
      return CreateTmAlarmValueFromDto(dto, new TmAnalog(TmAddr.CreateFromSqlTma(TmType.Analog, dto.Tma)));
    }


    public static TmAlarmValue CreateTmAlarmValueFromDto(TmAlarmDto dto, TmAnalog tmAnalog)
    {
      return new TmAlarmValue(TmAlarmType.Value,
                              dto.AlarmId,
                              dto.AlarmName,
                              dto.Importance,
                              dto.InUse,
                              dto.Active,
                              tmAnalog,
                              dto.CmpVal,
                              dto.CmpSign);
    }
  }
}