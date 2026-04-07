using System;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Api;

public partial class OikDataApi
{
  public async Task<bool> MqttSubscribe(MqttSubscriptionTopic topic, PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.MqttSubscribe(topic).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> MqttSubscribe(MqttKnownTopic topic, PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.MqttSubscribe(topic).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> MqttUnsubscribe(MqttSubscriptionTopic topic, PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.MqttUnsubscribe(topic).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> MqttUnsubscribe(MqttKnownTopic topic, PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.MqttUnsubscribe(topic).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> MqttPublish(MqttKnownTopic topic, byte[] payload, PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.MqttPublish(topic, payload).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> MqttPublish(MqttKnownTopic topic, string payload = "", PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.MqttPublish(topic, payload).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> MqttPublish(MqttPublishTopic topic, byte[] payload, PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.MqttPublish(topic, payload).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> MqttPublish(MqttPublishTopic topic, string payload, PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.MqttPublish(topic, payload).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<bool> MqttPublish(string topic, byte[] payload, PreferApi prefer = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.MqttPublish(topic, payload).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return false;
    }
  }


  public async Task<byte[]> MqttInvokeRpc(MqttPublishTopic requestTopic,
                                          byte[]           requestPayload,
                                          int              timeoutSeconds = 5,
                                          PreferApi        prefer         = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.MqttInvokeRpc(requestTopic, requestPayload, timeoutSeconds).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return Array.Empty<byte>();
    }
  }


  public async Task<byte[]> MqttInvokeRpc(MqttKnownTopic requestTopic,
                                          byte[]         requestPayload,
                                          int            timeoutSeconds = 5,
                                          PreferApi      prefer         = PreferApi.Auto)
  {
    var api = SelectApi(prefer, PreferApi.Tms, isTmsImplemented: true, isSqlImplemented: false);
    if (api == ApiSelection.Tms)
    {
      return await _tms.MqttInvokeRpc(requestTopic, requestPayload, timeoutSeconds).ConfigureAwait(false);
    }
    else if (api == ApiSelection.Sql)
    {
      throw new NotImplementedException();
    }
    else
    {
      return Array.Empty<byte>();
    }
  }
}