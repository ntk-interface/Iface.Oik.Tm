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
}