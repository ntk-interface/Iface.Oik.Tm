using Iface.Oik.Tm.Dto;

namespace Iface.Oik.Tm.Interfaces
{
  public abstract class TmAlarm
  {
    public TmAlarmType Type       { get; }
    public int         Id         { get; }
    public string      Name       { get; }
    public int         Importance { get; }
    public bool        IsInUse    { get; }
    public bool        IsActive   { get; }
    public TmAnalog    TmAnalog   { get; }

    public virtual bool IsEditable => false;

    public abstract string ThresholdName { get; }

    public string FullName => $"{Name} {ThresholdName}";


    public string StateName =>
      IsInUse
        ? IsActive
          ? "Взведена"
          : "Норма"
        : "Отключена";


    public TmAlarm(TmAlarmType type,
                   short       id,
                   string      name,
                   short       importance,
                   short       isInUse,
                   bool        isActive,
                   TmAnalog    tmAnalog)
    {
      Type       = type;
      Id         = id;
      Name       = name;
      Importance = importance;
      IsInUse    = (isInUse > 0);
      IsActive   = isActive;
      TmAnalog   = tmAnalog;
    }


    public static TmAlarm CreateFromDto(TmAlarmDto dto)
    {
      switch ((TmAlarmType) dto.Typ)
      {
        case TmAlarmType.Value:
          return TmAlarmValue.CreateTmAlarmValueFromDto(dto);

        case TmAlarmType.Analog:
          return TmAlarmAnalog.CreateTmAlarmAnalogFromDto(dto);

        case TmAlarmType.Expression:
          return TmAlarmExpression.CreateTmAlarmExpressionFromDto(dto);

        case TmAlarmType.Zonal:
          return TmAlarmZonal.CreateTmAlarmZonalFromDto(dto);

        default:
          return null;
      }
    }


    public static TmAlarm CreateFromDto(TmAlarmDto dto, TmAnalog tmAnalog)
    {
      switch ((TmAlarmType) dto.Typ)
      {
        case TmAlarmType.Value:
          return TmAlarmValue.CreateTmAlarmValueFromDto(dto, tmAnalog);
        
        case TmAlarmType.Expression:
          return TmAlarmExpression.CreateTmAlarmExpressionFromDto(dto, tmAnalog);

        case TmAlarmType.Analog:
          return TmAlarmAnalog.CreateTmAlarmAnalogFromDto(dto, tmAnalog);

        case TmAlarmType.Zonal:
          return TmAlarmZonal.CreateTmAlarmZonalFromDto(dto, tmAnalog);

        default:
          return null;
      }
    }
  }
}