using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Dto
{
  public class MqttMessageDto
  {
    public string  Topic          { get; set; }
    public byte[]  Payload        { get; set; }
    public MqttQoS QoS            { get; set; }
    public bool    Retain         { get; set; }
    public int     SubscriptionId { get; set; }
  }
}