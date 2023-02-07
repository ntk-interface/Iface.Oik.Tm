using System;
using System.Text.Json.Serialization;
using Iface.Oik.Tm.Interfaces;

namespace AspMinimalWebApiTask.Model;

public class TmStatusDto
{
  /// <summary>Номер канала</summary>
  [JsonPropertyName("ch")]
  public int TmAddrCh { get; init; }

  /// <summary>Номер КП</summary>
  [JsonPropertyName("rtu")]
  public int TmAddrRtu { get; init; }

  /// <summary>Номер объекта</summary>
  [JsonPropertyName("point")]
  public int TmAddrPoint { get; init; }

  /// <summary>Состояние</summary>
  [JsonPropertyName("s")]
  public int Status { get; init; }

  /// <summary>Флаги</summary>
  [JsonPropertyName("f")]
  public TmFlags Flags { get; init; }

  /// <summary>Флаги двухпозиционного сигнала</summary>
  [JsonPropertyName("s2")]
  public TmS2Flags S2Flags { get; init; }

  /// <summary>Время изменения</summary>
  [JsonPropertyName("t")]
  public DateTime? ChangeTime { get; init; }

  /// <summary>Номер класса</summary>
  [JsonPropertyName("cl")]
  public int ClassId { get; init; }

  /// <summary>Наименование</summary>
  [JsonPropertyName("n")]
  public string Name { get; init; } = "";
    
  /// <summary>Важность, где: 0 - оперативного состояния; 1 - предупредительные 2; 2 - предупредительные 1; 3 - тревога</summary>
  [JsonPropertyName("imp")]
  public int Importance { get; init; }

  /// <summary>Значение для отображения на экране</summary>
  [JsonPropertyName("dispV")]
  public string ValueToDisplay { get; init; } = "";
}