using Iface.Oik.Tm.Dto;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAlarm
  {
    public int      Id                   { get; }
    public string   Name                 { get; }
    public float    CompareValue         { get; }
    public bool     IsCompareGreaterThan { get; }
    public int      Importance           { get; }
    public bool     IsInUse              { get; }
    public bool     IsActive             { get; }
    public TmAnalog TmAnalog             { get; }


    public string ThresholdName => (IsCompareGreaterThan ? ">" : "<") +
                                   " "                                +
                                   TmAnalog.FakeValueWithUnitString(CompareValue);

    public string FullName => Name +
                              " "  +
                              ThresholdName;


    public string StateName =>
      IsInUse
        ? IsActive
          ? "Взведена"
          : "Норма"
        : "Отключена";


    public TmAlarm(short    id,
                   string   name,
                   float    compareValue,
                   short    compareSign,
                   short    importance,
                   short    isInUse,
                   bool     isActive,
                   TmAnalog tmAnalog)
    {
      Id                   = id;
      Name                 = name;
      CompareValue         = compareValue;
      IsCompareGreaterThan = (compareSign == 0);
      Importance           = importance;
      IsInUse              = (isInUse > 0);
      IsActive             = isActive;
      TmAnalog             = tmAnalog;
    }


    public static TmAlarm CreateFromDto(TmAlarmDto dto)
    {
      return new TmAlarm(dto.AlarmId,
                         dto.AlarmName,
                         dto.CmpVal,
                         dto.CmpSign,
                         dto.Importance,
                         dto.InUse,
                         dto.Active,
                         new TmAnalog(TmAddr.CreateFromSqlTma(TmType.Analog, dto.Tma)));
    }

    
    public static TmAlarm CreateFromDto(TmAlarmDto dto, TmAnalog analog)
    {
      return new TmAlarm(dto.AlarmId,
                         dto.AlarmName,
                         dto.CmpVal,
                         dto.CmpSign,
                         dto.Importance,
                         dto.InUse,
                         dto.Active,
                         analog);
    }
  }
}