using System;
using System.Net;

namespace Iface.Oik.Tm.Interfaces
{
  public abstract class MqttTopic
  {
    public string  Topic { get; protected set; }
    public MqttQoS QoS   { get; protected set; }
  }

  public class MqttSubscriptionTopic : MqttTopic
  {
    public int SubscriptionId { get; private set; }

    public MqttSubscriptionTopic(string  topic,
                                 int     subId,
                                 MqttQoS qos = MqttQoS.AtMostOnce)
    {
      SubscriptionId = subId;
      QoS           = qos;

      if (string.IsNullOrEmpty(topic))
      {
        throw new ArgumentException("Топик не может быть пустым");
      }

      var indexOfHash = topic.IndexOf("#", StringComparison.Ordinal);
      if (indexOfHash != -1 && indexOfHash != topic.Length - 1)
      {
        throw new ArgumentException("Символ '#' может быть только последним в строке");
      }

      Topic = topic;
    }
  }

  public class MqttPublishTopic : MqttTopic
  {
    public uint LifetimeSec { get; }
    
    public MqttPublishTopic(string  topic,
                            MqttQoS qos,
                            uint lifetimeSec)
    {
      QoS         = qos;
      LifetimeSec = lifetimeSec;

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

      Topic = topic;
    }
  }
}