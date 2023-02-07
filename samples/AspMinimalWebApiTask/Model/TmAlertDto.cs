using System;
using System.Text.Json.Serialization;

namespace AspMinimalWebApiTask.Model;

public class TmAlertDto
{
  /// <summary>Важность, где: 0 - оперативного состояния; 1 - предупредительные 2; 2 - предупредительные 1; 3 - тревога</summary>
  [JsonPropertyName("imp")]
  public int Importance { get; init; }

  /// <summary>Наименование</summary>
  [JsonPropertyName("n")]
  public string Name { get; init; } = "";

  /// <summary>Тип</summary>
  [JsonPropertyName("type")]
  public string Type { get; init; } = "";

  /// <summary>Время возникновения</summary>
  [JsonPropertyName("onT")]
  public DateTime? OnTime { get; init; }

  /// <summary>Время снятия</summary>
  [JsonPropertyName("offT")]
  public DateTime? OffTime { get; init; }

  /// <summary>Признак неквитированности</summary>
  [JsonPropertyName("isUnack")]
  public bool IsUnacked { get; init; }

  /// <summary>Признак активности тревоги</summary>
  [JsonPropertyName("isActive")]
  public bool IsActive { get; init; }

  /// <summary>Текущее значение сигнала или измерения, связанного с тревогой</summary>
  [JsonPropertyName("dispV")]
  public string CurrentValueString { get; init; } = "";

  /// <summary>ТМ-адрес сигнала или измерения, связанного с тревогой</summary>
  [JsonPropertyName("tm")]
  public string TmAddr { get; init; } = "";

  /// <summary>Время текущего значения</summary>
  [JsonPropertyName("t")]
  public DateTime? CurrentValueTime { get; init; }

  /// <summary>Время квитирования</summary>
  [JsonPropertyName("ackT")]
  public DateTime? AckTime { get; init; }
        
  /// <summary>Квитировавший пользователь</summary>
  [JsonPropertyName("ackU")] 
  public string? AckUser { get; init; }
  
}