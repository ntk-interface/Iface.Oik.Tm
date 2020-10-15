using Iface.Oik.Tm.Dto;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAlarmExpression : TmAlarm
  {
    private readonly string _expression;
    private readonly bool   _isCompareGreaterThan;

    public override string ThresholdName => $"{(_isCompareGreaterThan ? ">" : "<")} {_expression}";


    public TmAlarmExpression(TmAlarmType type,
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
      _expression           = expression;
      _isCompareGreaterThan = (compareSign == 0);
    }


    public static TmAlarmExpression CreateTmAlarmExpressionFromDto(TmAlarmDto dto)
    {
      return CreateTmAlarmExpressionFromDto(dto, new TmAnalog(TmAddr.CreateFromSqlTma(TmType.Analog, dto.Tma)));
    }


    public static TmAlarmExpression CreateTmAlarmExpressionFromDto(TmAlarmDto dto, TmAnalog tmAnalog)
    {
      return new TmAlarmExpression(TmAlarmType.Expression,
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