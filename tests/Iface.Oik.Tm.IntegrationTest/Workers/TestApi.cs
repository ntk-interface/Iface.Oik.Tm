using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.IntegrationTest.Util;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Utils;
using Microsoft.Extensions.Hosting;

namespace Iface.Oik.Tm.IntegrationTest.Workers;

[SuppressMessage("Code", "CAC001:ConfigureAwaitChecker")]
public class TestApi : IHostedService
{
  private readonly IOikDataApi              _api;
  private readonly IHostApplicationLifetime _lifetime;


  public TestApi(IOikDataApi api, IHostApplicationLifetime lifetime)
  {
    _api      = api;
    _lifetime = lifetime;
  }


  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await DoWork();

    _lifetime.StopApplication();
  }


  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }


  private async Task DoWork()
  {
    await TestTime();
    await TestCurrentServer();
    await TestComputerInfo();
    await TestLoginToken();
    Log.Message("=====");

    await TestRetros();
    Log.Message("=====");
    
    await TestTmTree();
    Log.Message("=====");

    await TestTmStatus();
    await TestTmAnalog();
    Log.Message("=====");

    await TestAck();
    Log.Message("=====");

    await TestMqtt();
  }


  private async Task TestTime()
  {
    var systemTime = await _api.GetSystemTimeString();
    Log.Condition(!string.IsNullOrEmpty(systemTime), $"System time: {systemTime}");
  }


  private async Task TestCurrentServer()
  {
    var (host, server) = await _api.GetCurrentServerName();
    Log.Condition(!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(server),
                  $"Current server: {host}/{server}");
  }


  private async Task TestComputerInfo()
  {
    var info = await _api.GetServerComputerInfo();
    Log.Condition(info != null, $"Computer info: {info?.ComputerName}");
  }


  private async Task TestLoginToken()
  {
    var (user, token) = await _api.GenerateTokenForExternalApp();
    Log.Condition(!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(token),
                  $"Generated token: {user} {token}");
  }


  private async Task TestRetros()
  {
    var retros = await _api.GetRetrosInfo(TmType.RetroAnalog);
    Log.Condition(retros.Count > 0, $"Analog retros count: {retros.Count}");
    retros.ForEach(r => Log.Message($"Retro {r.Name}"));
    
    // TODO проверить получение измерений из ретро
  }


  private async Task TestTmTree()
  {
    var channels = await _api.GetTmTreeChannels();
    Log.Condition(channels.Count > 0, $"Channels count: {channels.Count}");
    channels.Take(3).ForEach(ch => Log.Message($"#{ch.ChannelId} {ch.Name}"));
    var channelId = channels.ElementAtOrDefault(0)!.ChannelId;

    var rtus = await _api.GetTmTreeRtus(channelId);
    Log.Condition(rtus.Count > 0, $"RTUs of {channelId} count: {rtus.Count}");
    rtus.Take(3).ForEach(rtu => Log.Message($"#{rtu.ChannelId}:{rtu.RtuId} {rtu.Name}"));
    var rtuId = rtus.ElementAtOrDefault(0)!.RtuId;

    var statuses = await _api.GetTmTreeStatuses(channelId, rtuId);
    Log.Condition(statuses.Count > 0, $"TmStatuses of {channelId}:{rtuId} count: {statuses.Count}");
    statuses.Take(3).ForEach(st => Log.Message($"#TC{channelId}:{rtuId}:{st.TmAddr.Point} {st.Name}"));

    var analogs = await _api.GetTmTreeAnalogs(channelId, rtuId);
    Log.Condition(analogs.Count > 0, $"TmAnalogs of {channelId}:{rtuId} count: {analogs.Count}");
    analogs.Take(3).ForEach(an => Log.Message($"#TT{channelId}:{rtuId}:{an.TmAddr.Point} {an.Name}"));
  }

  
  private async Task TestTmStatus()
  {
    Log.Message("TmStatus tests");
    var tmStatus1 = new TmStatus(169, 1, 1);
    var tmStatus2 = new TmStatus(169, 1, 2);

    var tmStatuses = new List<TmStatus> { tmStatus1, tmStatus2 };

    await _api.SetStatus(tmStatus1.TmAddr.Ch, tmStatus1.TmAddr.Rtu, tmStatus1.TmAddr.Point, 0);
    var newStatus       = await _api.GetStatus(tmStatus1.TmAddr.Ch, tmStatus1.TmAddr.Rtu, tmStatus1.TmAddr.Point);
    Log.Condition(newStatus == 0, $"{tmStatus1.TmAddr} is {newStatus}");

    await _api.SetStatus(tmStatus1.TmAddr.Ch, tmStatus1.TmAddr.Rtu, tmStatus1.TmAddr.Point, 1);
    newStatus = await _api.GetStatus(tmStatus1.TmAddr.Ch, tmStatus1.TmAddr.Rtu, tmStatus1.TmAddr.Point);
    Log.Condition(newStatus == 1, $"{tmStatus1.TmAddr} is {newStatus}");
    
    Log.Message("Set ManuallyBlocked Flag");
    await _api.SetTagFlags(tmStatus1, TmFlags.ManuallySet);
    await _api.UpdateStatus(tmStatus1);
    Log.Condition(tmStatus1.HasFlag(TmFlags.ManuallySet), $"{tmStatus1.TmAddr} flags: {tmStatus1.Flags}");
    
    Log.Message("Clear ManuallyBlocked Flag");
    await _api.ClearTagFlags(tmStatus1, TmFlags.ManuallyBlocked);
    await _api.UpdateStatus(tmStatus1);
    Log.Condition(!tmStatus1.HasFlag(TmFlags.ManuallyBlocked), $"{tmStatus1.TmAddr} flags: {tmStatus1.Flags}");
    
    Log.Message("Set ManuallyBlocked Flag by list");
    await _api.SetTagsFlags(tmStatuses, TmFlags.ManuallyBlocked);
    await _api.UpdateStatuses(tmStatuses);
    foreach (var tmStatus in tmStatuses)
    {
      Log.Condition(tmStatus.HasFlag(TmFlags.ManuallyBlocked), $"{tmStatus.TmAddr} flags: {tmStatus.Flags}");
    }
    
    Log.Message("Clear ManuallyBlocked Flag by list");
    await _api.ClearTagsFlags(tmStatuses, TmFlags.ManuallyBlocked);
    await _api.UpdateStatuses(tmStatuses);
    foreach (var tmStatus in tmStatuses)
    {
      Log.Condition(!tmStatus.HasFlag(TmFlags.ManuallyBlocked), $"{tmStatus.TmAddr} flags: {tmStatus.Flags}");
    }


    await _api.SetStatusNormalOn(tmStatus1);
    var normalStatus = await _api.GetStatusNormal(tmStatus1);
    Log.Condition(normalStatus == 1, $"{tmStatus1.TmAddr} normal status On" );
    
    await _api.SetStatusNormalOff(tmStatus1);
    normalStatus = await _api.GetStatusNormal(tmStatus1);
    Log.Condition(normalStatus == 0, $"{tmStatus1.TmAddr} normal status Off" );
    
    await _api.ClearStatusNormal(tmStatus1);
    normalStatus = await _api.GetStatusNormal(tmStatus1);
    Log.Condition(normalStatus == -1, $"{tmStatus1.TmAddr} normal status cleared" );

    await _api.SwitchStatusManually(tmStatus1, true);
    newStatus = await _api.GetStatus(tmStatus1.TmAddr.Ch, tmStatus1.TmAddr.Rtu, tmStatus1.TmAddr.Point);
    Log.Condition(newStatus == 0, $"{tmStatus1.TmAddr} switched manually. Value: {newStatus}");

    await _api.UpdateStatus(tmStatus1);
    Log.Condition(tmStatus1.IsManuallySet, $"{tmStatus1.TmAddr} is manually set");
    Log.Condition(tmStatus1.IsManuallyBlocked, $"{tmStatus1.TmAddr} is manually blocked");

    await _api.ClearTagFlags(tmStatus1, TmFlags.ManuallyBlocked | TmFlags.ManuallySet);
    await _api.UpdateStatus(tmStatus1);
    Log.Condition(tmStatus1 is {IsManuallyBlocked: false, IsManuallySet: false}, 
                  $"{tmStatus1.TmAddr} manually blocked and manually set cleared");
  }


  private async Task TestTmAnalog()
  {
    Log.Message("TmAnalog tests");
    var tmAnalog1 = new TmAnalog(169, 1, 1);
    var tmAnalog2 = new TmAnalog(169, 1, 2);
    
    var tmAnalogs = new List<TmAnalog> {tmAnalog1, tmAnalog2};

    await _api.SetAnalog(tmAnalog1.TmAddr.Ch, tmAnalog1.TmAddr.Rtu, tmAnalog1.TmAddr.Point, 6.9f);
    var newAnalogVal = await _api.GetAnalog(tmAnalog1.TmAddr.Ch, tmAnalog1.TmAddr.Rtu, tmAnalog1.TmAddr.Point);
    Log.Condition(newAnalogVal == 6.9f, $"{tmAnalog1.TmAddr} is {newAnalogVal}");
    
    await _api.SetAnalog(tmAnalog1.TmAddr.Ch, tmAnalog1.TmAddr.Rtu, tmAnalog1.TmAddr.Point, 0);
    newAnalogVal = await _api.GetAnalog(tmAnalog1.TmAddr.Ch, tmAnalog1.TmAddr.Rtu, tmAnalog1.TmAddr.Point);
    Log.Condition(newAnalogVal == 0, $"{tmAnalog1.TmAddr} is {newAnalogVal}");

    Log.Message("Set ManuallyBlocked Flag");
    await _api.SetTagFlags(tmAnalog1, TmFlags.ManuallySet);
    await _api.UpdateAnalog(tmAnalog1);
    Log.Condition(tmAnalog1.HasFlag(TmFlags.ManuallySet), $"{tmAnalog1.TmAddr} flags: {tmAnalog1.Flags}");
    
    Log.Message("Clear ManuallyBlocked Flag");
    await _api.ClearTagFlags(tmAnalog1, TmFlags.ManuallyBlocked);
    await _api.UpdateAnalog(tmAnalog1);
    Log.Condition(!tmAnalog1.HasFlag(TmFlags.ManuallyBlocked), $"{tmAnalog1.TmAddr} flags: {tmAnalog1.Flags}");
    
    Log.Message("Set ManuallyBlocked Flag by list");
    await _api.SetTagsFlags(tmAnalogs, TmFlags.ManuallyBlocked);
    await _api.UpdateAnalogs(tmAnalogs);
    foreach (var tmAnalog in tmAnalogs)
    {
      Log.Condition(tmAnalog.HasFlag(TmFlags.ManuallyBlocked), $"{tmAnalog.TmAddr} flags: {tmAnalog.Flags}");
    }
    
    Log.Message("Clear ManuallyBlocked Flag by list");
    await _api.ClearTagsFlags(tmAnalogs, TmFlags.ManuallyBlocked);
    await _api.UpdateAnalogs(tmAnalogs);
    foreach (var tmAnalog in tmAnalogs)
    {
      Log.Condition(!tmAnalog.HasFlag(TmFlags.ManuallyBlocked), $"{tmAnalog.TmAddr} flags: {tmAnalog.Flags}");
    }
    
    await _api.SetAnalogManually(tmAnalog1, 0, true);
    newAnalogVal = await _api.GetAnalog(tmAnalog1.TmAddr.Ch, tmAnalog1.TmAddr.Rtu, tmAnalog1.TmAddr.Point);
    Log.Condition(newAnalogVal == 0, $"{tmAnalog1.TmAddr} switched manually. Value: {tmAnalog1}");

    await _api.UpdateAnalog(tmAnalog1);
    Log.Condition(tmAnalog1.IsManuallySet,     $"{tmAnalog1.TmAddr} is manually set");
    Log.Condition(tmAnalog1.IsManuallyBlocked, $"{tmAnalog1.TmAddr} is manually blocked");

    await _api.ClearTagFlags(tmAnalog1, TmFlags.ManuallyBlocked | TmFlags.ManuallySet);
    await _api.UpdateAnalog(tmAnalog1);
    Log.Condition(tmAnalog1 is {IsManuallyBlocked: false, IsManuallySet: false}, 
                  $"{tmAnalog1.TmAddr} manually blocked and manually set cleared");
  }

  private async Task TestAck()
  {
    Log.Message("Ack tests");
    var tmStatus  = new TmStatus(169, 1, 1);
    var tmAnalog = new TmAnalog(169, 1, 1);
    
    await _api.SetStatus(tmStatus.TmAddr.Ch, tmStatus.TmAddr.Rtu, tmStatus.TmAddr.Point, 1);
    Log.Condition(await _api.AckStatus(tmStatus), $"{tmStatus.TmAddr} acked");
    await _api.SetStatus(tmStatus.TmAddr.Ch, tmStatus.TmAddr.Rtu, tmStatus.TmAddr.Point, 0);

    await _api.SetAnalog(tmAnalog.TmAddr.Ch, tmAnalog.TmAddr.Rtu, tmAnalog.TmAddr.Point, 6.9f);
    Log.Condition(await _api.AckAnalog(tmAnalog), $"{tmAnalog.TmAddr} acked");
    await _api.SetAnalog(tmAnalog.TmAddr.Ch, tmAnalog.TmAddr.Rtu, tmAnalog.TmAddr.Point, 0);
  }

  private async Task TestMqtt()
  {
    Log.Message("MQTT tests");
    var subscriptionTopic = new MqttSubscriptionTopic("Test", 1);
    var publishTopic      = new MqttPublishTopic("Test", MqttQoS.ExactlyOnce, 1);
    var payload            = "Test Payload";
    
    Log.Condition(await _api.MqttSubscribe(subscriptionTopic), $"MqttSubscribe: Topic - {subscriptionTopic.Topic}");
    
    Log.Condition(await _api.MqttPublish(publishTopic, payload), 
                  $"MqttPublish string: Topic - {publishTopic.Topic}, Payload - {payload}");
    
    Log.Condition(await _api.MqttPublish(publishTopic, payload.To1251Bytes()), 
                  $"MqttPublish bytes: Topic - {publishTopic.Topic}, Payload - {payload} as bytes");
    
    Log.Condition(await _api.MqttUnsubscribe(subscriptionTopic), $"MqttUnsubscribe: Topic - {subscriptionTopic.Topic}");
  }
}