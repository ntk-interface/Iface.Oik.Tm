using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.IntegrationTest.Util;
using Iface.Oik.Tm.Interfaces;
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
}