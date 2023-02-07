using System;
using System.Text.Json.Serialization;
using Iface.Oik.Tm.Interfaces;

namespace AspWebApi.Model
{
  public class TmStatusDto
  {
    [JsonPropertyName("ch")]
    public int TmAddrCh { get; set; }

    [JsonPropertyName("rtu")]
    public int TmAddrRtu { get; set; }

    [JsonPropertyName("point")]
    public int TmAddrPoint { get; set; }

    [JsonPropertyName("s")]
    public int Status { get; set; }

    [JsonPropertyName("f")]
    public TmFlags Flags { get; set; }

    [JsonPropertyName("s2")]
    public TmS2Flags S2Flags { get; set; }

    [JsonPropertyName("time")]
    public DateTime? ChangeTime { get; set; }

    [JsonPropertyName("class")]
    public int ClassId { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; } = "";

    [JsonPropertyName("imp")]
    public int Importance { get; set; }

    [JsonPropertyName("dispV")]
    public string ValueToDisplay { get; set; } = "";
  }
}