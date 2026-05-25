using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api;

public partial class TmsApi
{
  private event EventHandler<MqttMessage> MqttMessageReceived = delegate { };
  
  
  public async Task<bool> MqttSubscribe(MqttSubscriptionTopic topic)
  {
    return await Task.Run(() => MqttSubscribeSync(topic)).ConfigureAwait(false);
  }


  public async Task<bool> MqttSubscribe(MqttKnownTopic topic)
  {
    return await MqttSubscribe(new MqttSubscriptionTopic(topic)).ConfigureAwait(false);
  }


  public async Task<bool> MqttUnsubscribe(MqttSubscriptionTopic topic)
  {
    return await Task.Run(() => MqttUnsubscribeSync(topic)).ConfigureAwait(false);
  }


  public async Task<bool> MqttUnsubscribe(MqttKnownTopic topic)
  {
    return await MqttUnsubscribe(new MqttSubscriptionTopic(topic)).ConfigureAwait(false);
  }


  public async Task<bool> MqttPublish(MqttKnownTopic topic, byte[] payload)
  {
    return await MqttPublish(new MqttPublishTopic(topic), payload).ConfigureAwait(false);
  }


  public async Task<bool> MqttPublish(MqttKnownTopic topic, string payload = "")
  {
    return await MqttPublish(topic,
                             string.IsNullOrWhiteSpace(payload)
                               ? Array.Empty<byte>()
                               : TmNativeUtil.StringToBytes(payload))
            .ConfigureAwait(false);
  }


  public async Task<bool> MqttPublish(MqttPublishTopic topic, string payload)
  {
    return await MqttPublish(topic, TmNativeUtil.StringToBytes(payload)).ConfigureAwait(false);
  }


  public async Task<bool> MqttPublish(string topic, byte[] payload)
  {
    return await MqttPublish(new MqttPublishTopic(topic), payload).ConfigureAwait(false);
  }


  public async Task<bool> MqttPublish(MqttPublishTopic topic, byte[] payload)
  {
    return await Task.Run(() => MqttPublishSync(topic, payload)).ConfigureAwait(false);
  }


  public Task<byte[]> MqttInvokeRpc(MqttPublishTopic requestTopic,
                                    byte[]           requestPayload,
                                    int              timeoutSeconds = 5)
  {
    const int responseId    = 656667;                             // случайное число
    var       responseTopic = $"rpc/{Guid.NewGuid().ToString()}"; // случайная строка, ловим ответы только сюда

    var tcs = new TaskCompletionSource<byte[]>();

    void WrapperFunc(object sender, MqttMessage message)
    {
      if (message.Payload.IsNullOrEmpty()     ||
          string.IsNullOrEmpty(message.Topic) ||
          !message.Topic.Equals(responseTopic, StringComparison.Ordinal))
      {
        return;
      }

      MqttMessageReceived -= WrapperFunc;
      MqttUnsubscribeSync(new MqttSubscriptionTopic(responseTopic, responseId));
      tcs.TrySetResult(message.Payload);
    }

    MqttSubscribeSync(new MqttSubscriptionTopic(responseTopic, responseId));
    MqttMessageReceived += WrapperFunc;

    var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
    cancellationToken.Token.Register(() =>
    {
      if (tcs.TrySetCanceled())
      {
        MqttMessageReceived -= WrapperFunc;
        MqttUnsubscribeSync(new MqttSubscriptionTopic(responseTopic, responseId));
      }
    }, useSynchronizationContext: false);

    requestTopic.AddResponseTopic(responseTopic);
    MqttPublishSync(requestTopic, requestPayload);

    return tcs.Task;
  }


  public async Task<byte[]> MqttInvokeRpc(MqttKnownTopic requestTopic,
                                          byte[]         requestPayload,
                                          int            timeoutSeconds = 5)
  {
    return await MqttInvokeRpc(new MqttPublishTopic(requestTopic), requestPayload, timeoutSeconds)
            .ConfigureAwait(false);
  }


  private bool MqttPublishSync(MqttPublishTopic topic, byte[] payload)
  {
    if (topic.VariableHeader.IsNullOrEmpty())
    {
      return TmNative.tmcPubPublish(_cid,
                                    topic.Topic,
                                    topic.LifetimeSec,
                                    (byte)topic.QoS,
                                    payload,
                                    (uint)payload.Length);
    }
    else
    {
      var headerPtr = TmNativeUtil.AllocateDoubleNullTerminatedPointerFromStringList(
        topic.VariableHeader.Select(h => $"{h.Key}={h.Value}").ToArray());

      try
      {
        return TmNative.tmcPubPublishEx(_cid,
                                        topic.Topic,
                                        topic.LifetimeSec,
                                        (byte)topic.QoS,
                                        payload,
                                        (uint)payload.Length,
                                        headerPtr);
      }
      finally
      {
        TmNativeUtil.FreeAllocatedPointer(headerPtr);
      }
    }
  }


  private bool MqttSubscribeSync(MqttSubscriptionTopic topic)
  {
    return TmNative.tmcPubSubscribe(_cid,
                                    topic.Topic,
                                    (uint)topic.SubscriptionId,
                                    (byte)topic.QoS);
  }


  private bool MqttUnsubscribeSync(MqttSubscriptionTopic topic)
  {
    return TmNative.tmcPubUnsubscribe(_cid,
                                      topic.Topic,
                                      (uint)topic.SubscriptionId);
  }


  public void NotifyOfMqttMessage(MqttMessage message)
  {
    MqttMessageReceived.Invoke(this, message);
  }
}