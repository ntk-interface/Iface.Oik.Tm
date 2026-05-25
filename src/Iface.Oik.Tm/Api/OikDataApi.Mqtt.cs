using System;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<bool> MqttSubscribe(MqttSubscriptionTopic topic, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.MqttSubscribe(topic),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> MqttSubscribe(MqttKnownTopic topic, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.MqttSubscribe(topic),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> MqttUnsubscribe(MqttSubscriptionTopic topic, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.MqttUnsubscribe(topic),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> MqttUnsubscribe(MqttKnownTopic topic, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.MqttUnsubscribe(topic),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> MqttPublish(MqttKnownTopic topic, byte[] payload, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.MqttPublish(topic, payload),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> MqttPublish(MqttKnownTopic topic, string payload = "", PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.MqttPublish(topic, payload),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> MqttPublish(MqttPublishTopic topic, byte[] payload, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.MqttPublish(topic, payload),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> MqttPublish(MqttPublishTopic topic, string payload, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.MqttPublish(topic, payload),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<bool> MqttPublish(string topic, byte[] payload, PreferApi prefer = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.MqttPublish(topic, payload),
                         null)
            .ConfigureAwait(false);
  }


  public async Task<byte[]> MqttInvokeRpc(MqttPublishTopic requestTopic,
                                          byte[]           requestPayload,
                                          int              timeoutSeconds = 5,
                                          PreferApi        prefer         = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.MqttInvokeRpc(requestTopic, requestPayload, timeoutSeconds),
                         null,
                         () => Array.Empty<byte>())
            .ConfigureAwait(false);
  }


  public async Task<byte[]> MqttInvokeRpc(MqttKnownTopic requestTopic,
                                          byte[]         requestPayload,
                                          int            timeoutSeconds = 5,
                                          PreferApi      prefer         = PreferApi.Auto)
  {
    return await Execute(prefer,
                         PreferApi.Tms,
                         () => _tms.MqttInvokeRpc(requestTopic, requestPayload, timeoutSeconds),
                         null,
                         () => Array.Empty<byte>())
            .ConfigureAwait(false);
  }
}