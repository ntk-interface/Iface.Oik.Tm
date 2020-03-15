using System;
using Iface.Oik.Tm.Dto;

namespace Iface.Oik.Tm.Interfaces
{
  public readonly struct TmAlert
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
    public TmAddr    TmAddr             { get; }


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
                         TmAddr.CreateFromSqlTmaAndTmaType(dto.TmaType ?? 0,
                                                           dto.Tma));
    }
  }
}