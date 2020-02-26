using System.Text.Json.Serialization;

namespace AspWebApi.Model
{
  public class TmAlarmDto
  {
    public TmAnalogDto TmAnalog { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("cmpV")]
    public float CompareValue { get; set; }

    [JsonPropertyName("isGt")]
    public bool IsCompareGreaterThan { get; set; }

    [JsonPropertyName("imp")]
    public int Importance { get; set; }

    [JsonPropertyName("isUse")]
    public bool IsInUse { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("fullN")]
    public string FullName { get; set; }
  }
}