using System;
using Iface.Oik.Tm.Dto;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAlert
  {
    public byte[]    Id                 { get; }
    public int       Importance         { get; }
    public bool      IsActive           { get; }
    public bool      IsUnacked          { get; }
    public DateTime? OnTime             { get; }
    public DateTime? OffTime            { get; }
    public string    Type               { get; }
    public string    Name               { get; }
    public string    CurrentValueString { get; }
    public DateTime? CurrentValueTime   { get; }
    public float     CurrentValue       { get; }
    public int       TmClassId          { get; }
    public TmAddr    TmAddr             { get; }

    public bool CanRemove => !IsActive &&
                             !IsUnacked;

    public string ImportanceName  => TmEvent.ImportanceToName(Importance);
    public string ImportanceAlias => TmEvent.ImportanceToAlias(Importance);

    public bool HasTmStatus => TmAddr.Type == TmType.Status;
    public bool HasTmAnalog => TmAddr.Type == TmType.Analog;


    public TmAlert(byte[]    id,
                   int       importance,
                   bool      isActive,
                   bool      isUnacked,
                   DateTime? onTime,
                   DateTime? offTime,
                   string    type,
                   string    name,
                   string    currentValueString,
                   DateTime? currentValueTime,
                   float     currentValue,
                   int       tmClassId,
                   TmAddr    tmAddr)
    {
      Id                 = id;
      Importance         = importance;
      IsActive           = isActive;
      IsUnacked          = isUnacked;
      OnTime             = onTime;
      OffTime            = offTime;
      Type               = type;
      Name               = name;
      CurrentValueString = currentValueString;
      CurrentValueTime   = currentValueTime;
      CurrentValue       = currentValue;
      TmClassId          = tmClassId;
      TmAddr             = tmAddr;
    }


    public static TmAlert CreateFromDto(TmAlertDto dto)
    {
      return new TmAlert(dto.AlertId,
                         dto.Importance,
                         dto.Active,
                         dto.Unack,
                         dto.OnTime,
                         dto.OffTime,
                         dto.TypeName,
                         dto.Name,
                         dto.ValueText,
                         dto.CurTime,
                         dto.CurValue,
                         dto.ClassId ?? 0,
                         TmAddr.CreateFromSqlTmaAndTmaType(dto.TmaType ?? 0,
                                                           dto.Tma));
    }
  }
}