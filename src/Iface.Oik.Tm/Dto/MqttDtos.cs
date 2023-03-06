using System;
using System.Collections.Generic;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Dto
{
  public class MqttUserCustomMessageDto
  {
    public TmEventImportances Importance { get; set; } = TmEventImportances.Imp0;
    public string             Message    { get; set; }
  }


  public class MqttTopologyChangeSomeDto
  {
     public Dictionary<Guid, CimTopologyStatus> Items { get; set; }
  }
  
  
  public class MqttUserActionLogDto
  {
    public Guid               Id         { get; set; }
    public DateTime           Time       { get; set; }
    public MqttKnownTopic     Action     { get; set; }
    public TmEventImportances Importance { get; set; }
    public string             Text       { get; set; }
    public string             UserName   { get; set; }
  }


  public class MqttDocumentDto
  {
    public int    Ownership { get; set; }
    public string Name      { get; set; }
  }
}