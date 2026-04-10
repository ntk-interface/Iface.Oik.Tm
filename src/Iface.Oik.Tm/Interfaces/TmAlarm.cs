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
      return (TmAlarmType)dto.Typ switch
             {
               TmAlarmType.Value      => TmAlarmValue.CreateTmAlarmValueFromDto(dto),
               TmAlarmType.Analog     => TmAlarmAnalog.CreateTmAlarmAnalogFromDto(dto),
               TmAlarmType.Expression => TmAlarmExpression.CreateTmAlarmExpressionFromDto(dto),
               TmAlarmType.Zonal      => TmAlarmZonal.CreateTmAlarmZonalFromDto(dto),
               _                      => null
             };
    }


    public static TmAlarm CreateFromDto(TmAlarmDto dto, TmAnalog tmAnalog)
    {
      return (TmAlarmType)dto.Typ switch
             {
               TmAlarmType.Value      => TmAlarmValue.CreateTmAlarmValueFromDto(dto, tmAnalog),
               TmAlarmType.Expression => TmAlarmExpression.CreateTmAlarmExpressionFromDto(dto, tmAnalog),
               TmAlarmType.Analog     => TmAlarmAnalog.CreateTmAlarmAnalogFromDto(dto, tmAnalog),
               TmAlarmType.Zonal      => TmAlarmZonal.CreateTmAlarmZonalFromDto(dto, tmAnalog),
               _                      => null
             };
    }
  }
}