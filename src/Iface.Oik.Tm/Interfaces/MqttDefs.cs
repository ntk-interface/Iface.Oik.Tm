using System;
using System.ComponentModel;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  [Flags]
  public enum MqttQoS : byte
  {
    AtMostOnce    = TmNativeDefs.PublicationQoS.AtMostOnce,
    AtLeastOnce   = TmNativeDefs.PublicationQoS.AtLeastOnce,
    ExactlyOnce   = TmNativeDefs.PublicationQoS.ExactlyOnce,
    NoLocal       = TmNativeDefs.PublicationQoS.NoLocal,
    RetainAsPub   = TmNativeDefs.PublicationQoS.RetainAsPub,
    RetainFirst   = TmNativeDefs.PublicationQoS.RetainFirst,
    RetainNo      = TmNativeDefs.PublicationQoS.RetainNo,
    RetainDefault = TmNativeDefs.PublicationQoS.RetainDefault
  }


  public class MqttMessage
  {
    public string  Topic          { get; set; }
    public byte[]  Payload        { get; set; }
    public MqttQoS QoS            { get; set; }
    public bool    Retain         { get; set; }
    public int     SubscriptionId { get; set; }
    public uint    UserId         { get; set; }
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


    public MqttSubscriptionTopic(MqttKnownTopic knownTopic)
      : this(knownTopic.GetDescription(), (int)knownTopic)
    {
    }
  }


  public class MqttPublishTopic
  {
    public string  Topic       { get; }
    public MqttQoS QoS         { get; }
    public uint    LifetimeSec { get; }


    public MqttPublishTopic(string  topic,
                            MqttQoS qos         = MqttQoS.AtMostOnce,
                            uint    lifetimeSec = 0)
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


    public MqttPublishTopic(MqttKnownTopic knownTopic)
      : this(knownTopic.GetDescription())
    {
    }
  }
}