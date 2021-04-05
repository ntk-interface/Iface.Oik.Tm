using System;
using System.Collections.Generic;
using System.Linq;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Utils;

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
    public object    Reference          { get; }

    public List<ITmAnalogRetro> MicroSeries { get; }

    public TmAnalogTechParameters AnalogTechParameters { get; }

    public bool CanRemove => !IsActive &&
                             !IsUnacked;

    public string ImportanceName  => TmEvent.ImportanceToName(Importance);
    public string ImportanceAlias => TmEvent.ImportanceToAlias(Importance);

    public bool HasTmStatus => TmAddr.Type == TmType.Status;
    public bool HasTmAnalog => TmAddr.Type == TmType.Analog;

    public float? AlarmInitialValue => HasTmAnalog && Reference != null
      ? (float?) Reference
      : null;


    public TmAlert(byte[]                 id,
                   int                    importance,
                   bool                   isActive,
                   bool                   isUnacked,
                   DateTime?              onTime,
                   DateTime?              offTime,
                   string                 type,
                   string                 name,
                   string                 currentValueString,
                   DateTime?              currentValueTime,
                   float                  currentValue,
                   float?                 initialValue,
                   int                    tmClassId,
                   TmAddr                 tmAddr,
                   ITmAnalogRetro[]       microSeries,
                   TmAnalogTechParameters analogTechParameters)
    {
      Id                   = id;
      Importance           = importance;
      IsActive             = isActive;
      IsUnacked            = isUnacked;
      OnTime               = onTime;
      OffTime              = offTime;
      Type                 = type;
      Name                 = name;
      CurrentValueString   = currentValueString;
      CurrentValueTime     = currentValueTime;
      CurrentValue         = currentValue;
      TmClassId            = tmClassId;
      TmAddr               = tmAddr;
      MicroSeries          = microSeries.ToList();

      if (tmAddr.Type == TmType.Analog)
      {
        AnalogTechParameters = analogTechParameters;
      }
      
      if (tmAddr.Type == TmType.Analog && initialValue != null)
      {
        Reference = initialValue.Value;
      }
    }


    public static TmAlert CreateFromDto(TmAlertDto dto)
    {
      return new TmAlert(dto.AlertId,
                         dto.Importance,
                         dto.Active,
                         dto.Unack,
                         dto.OnTime,
                         dto.OffTime.NullIfEpoch(),
                         dto.TypeName,
                         dto.Name.RemoveMultipleWhitespaces(),
                         dto.ValueText.Trim(),
                         dto.CurTime,
                         dto.CurValue,
                         dto.ActValue,
                         dto.ClassId ?? 0,
                         TmAddr.CreateFromSqlTmaAndTmaType((ushort) (dto.TmType ?? 0), dto.Tma),
                         dto.MapToTmAnalogMicroSeriesDto().MapToITmAnalogRetroArray(),
                         TmAnalogTechParameters.CreateFromDto(dto.MapToTmAnalogTechParametersDto())
      );
    }
  }
}