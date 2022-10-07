using System;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Interfaces
{
  public enum MqttQoS : byte
  {
    AtMostOnce  = TmNativeDefs.PublicationQoS.AtMostOnce,
    AtLeastOnce = TmNativeDefs.PublicationQoS.AtLeastOnce,
    ExactlyOnce = TmNativeDefs.PublicationQoS.ExactlyOnce
  }


  public class MqttMessage
  {
    public string  Topic          { get; set; }
    public byte[]  Payload        { get; set; }
    public MqttQoS QoS            { get; set; }
    public bool    Retain         { get; set; }
    public int     SubscriptionId { get; set; }
  }


  public class MqttSubscriptionTopic
  {
    public string  Topic          { get; }
    public MqttQoS QoS            { get; }
    public int     SubscriptionId { get; }


    public MqttSubscriptionTopic(string  topic,
                                 int     subscriptionId,
                                 MqttQoS qos = MqttQoS.AtMostOnce)
    {
      if (string.IsNullOrEmpty(topic))
      {
        throw new ArgumentException("Топик не может быть пустым");
      }
      var indexOfHash = topic.IndexOf("#", StringComparison.Ordinal);
      if (indexOfHash != -1 && indexOfHash != topic.Length - 1)
      {
        throw new ArgumentException("Символ '#' может быть только последним в строке");
      }

      Topic          = topic;
      SubscriptionId = subscriptionId;
      QoS            = qos;
    }
  }


  public class MqttPublishTopic
  {
    public string  Topic       { get; }
    public MqttQoS QoS         { get; }
    public uint    LifetimeSec { get; }


    public MqttPublishTopic(string  topic,
                            MqttQoS qos         = MqttQoS.AtMostOnce,
                            uint    lifetimeSec = 60)
    {
      if (string.IsNullOrEmpty(topic))
      {
        throw new ArgumentException("Топик не может быть пустым");
      }
      foreach (var @char in topic)
      {
        switch (@char)
        {
          case '+':
            throw new ArgumentException("Символ '+' недопустим в имени топика");
          case '#':
            throw new ArgumentException("Символ '#' недопустим в имени топика");
        }
      }
      Topic       = topic;
      QoS         = qos;
      LifetimeSec = lifetimeSec;
    }
  }
}