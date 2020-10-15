using Iface.Oik.Tm.Dto;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAlarmAnalog : TmAlarm
  {
    private readonly bool _isCompareGreaterThan;

    public string Expression { get; }

    public override string ThresholdName => $"{(_isCompareGreaterThan ? ">" : "<")} {Expression}";


    public TmAlarmAnalog(TmAlarmType type,
                         short       id,
                         string      name,
                         short       importance,
                         short       isInUse,
                         bool        isActive,
                         TmAnalog    tmAnalog,
                         string      expression,
                         short       compareSign)
      : base(type, id, name, importance, isInUse, isActive, tmAnalog)
    {
      Expression            = expression;
      _isCompareGreaterThan = (compareSign == 0);
    }


    public static TmAlarmAnalog CreateTmAlarmAnalogFromDto(TmAlarmDto dto)
    {
      return CreateTmAlarmAnalogFromDto(dto, new TmAnalog(TmAddr.CreateFromSqlTma(TmType.Analog, dto.Tma)));
    }


    public static TmAlarmAnalog CreateTmAlarmAnalogFromDto(TmAlarmDto dto, TmAnalog tmAnalog)
    {
      return new TmAlarmAnalog(TmAlarmType.Analog,
                               dto.AlarmId,
                               dto.AlarmName,
                               dto.Importance,
                               dto.InUse,
                               dto.Active,
                               tmAnalog,
                               dto.Expr,
                               dto.CmpSign);
    }
  }
}