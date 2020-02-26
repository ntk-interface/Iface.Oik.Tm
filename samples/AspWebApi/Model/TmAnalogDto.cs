using System;
using System.Text.Json.Serialization;
using Iface.Oik.Tm.Interfaces;

namespace AspWebApi.Model
{
  public class TmAnalogDto
  {
    [JsonPropertyName("ch")]
    public int TmAddrCh { get; set; }

    [JsonPropertyName("rtu")]
    public int TmAddrRtu { get; set; }

    [JsonPropertyName("point")]
    public int TmAddrPoint { get; set; }

    [JsonPropertyName("v")]
    public float Value { get; set; }

    [JsonPropertyName("f")]
    public TmFlags Flags { get; set; }

    [JsonPropertyName("time")]
    public DateTime ChangeTime { get; set; }

    [JsonPropertyName("class")]
    public int ClassId { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("u")]
    public string Unit { get; set; }

    [JsonPropertyName("pr")]
    public int Precision { get; set; }

    [JsonPropertyName("dispV")]
    public string ValueToDisplay { get; set; }
  }
}