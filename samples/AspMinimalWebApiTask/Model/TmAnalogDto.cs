using System;
using System.Text.Json.Serialization;
using Iface.Oik.Tm.Interfaces;

namespace AspMinimalWebApiTask.Model;

public class TmAnalogDto
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

  /// <summary>Значение</summary>
  [JsonPropertyName("v")]
  public float Value { get; init; }

  /// <summary>Флаги (отсутствует, если 0)</summary>
  [JsonPropertyName("f")]
  public TmFlags Flags { get; init; }

  /// <summary>Время изменения</summary>
  [JsonPropertyName("t")]
  public DateTime? ChangeTime { get; init; }

  /// <summary>Номер класса (отсутствует, если 0)</summary>
  [JsonPropertyName("cl")]
  public int ClassId { get; init; }

  /// <summary>Наименование</summary>
  [JsonPropertyName("n")]
  public string Name { get; init; } = "";

  [JsonPropertyName("u")]
  public string Unit { get; init; } = "";

  [JsonPropertyName("pr")]
  public int Precision { get; init; }

  /// <summary>Значение для отображения на экране</summary>
  [JsonPropertyName("dispV")]
  public string ValueToDisplay { get; init; } = "";
}