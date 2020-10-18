using Iface.Oik.Tm.Dto;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAlarmZonal : TmAlarm
  {
    public override string ThresholdName { get; }

    public override bool IsEditable => true;


    public TmAlarmZonal(TmAlarmType type,
                        short       id,
                        string      name,
                        short       importance,
                        short       isInUse,
                        bool        isActive,
                        TmAnalog    tmAnalog,
                        string      expression)
      : base(type, id, name, importance, isInUse, isActive, tmAnalog)
    {
      ThresholdName = expression;
    }


    public static TmAlarmZonal CreateTmAlarmZonalFromDto(TmAlarmDto dto)
    {
      return CreateTmAlarmZonalFromDto(dto, new TmAnalog(TmAddr.CreateFromSqlTma(TmType.Analog, dto.Tma)));
    }


    public static TmAlarmZonal CreateTmAlarmZonalFromDto(TmAlarmDto dto, TmAnalog tmAnalog)
    {
      return new TmAlarmZonal(TmAlarmType.Zonal,
                              dto.AlarmId,
                              dto.AlarmName,
                              dto.Importance,
                              dto.InUse,
                              dto.Active,
                              tmAnalog,
                              dto.Expr);
    }
  }
}