using System;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Services
{
  public abstract class BackgroundRestartableService : IBackgroundService, IDisposable
  {
    private Task                    _executingTask;
    private CancellationTokenSource _stoppingCts;


    protected abstract Task ExecuteAsync(CancellationToken stoppingToken);


    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
      _stoppingCts   = new CancellationTokenSource();
      _executingTask = ExecuteAsync(_stoppingCts.Token);

      if (_executingTask.IsCompleted)
      {
        return _executingTask;
      }

      return Task.CompletedTask;
    }


    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
      if (_executingTask == null)
      {
        return;
      }

      try
      {
        _stoppingCts.Cancel();
      }
      finally
      {
        await Task.WhenAny(_executingTask,
                           Task.Delay(Timeout.Infinite, cancellationToken));
        _stoppingCts.Dispose();
        _executingTask.Dispose();
      }
    }


    public virtual Task StartAsync()
    {
      return StartAsync(CancellationToken.None);
    }


    public virtual async Task StopAsync()
    {
      await StopAsync(CancellationToken.None);
    }


    public virtual void Dispose()
    {
      try
      {
        _stoppingCts?.Cancel();
      }
      catch (ObjectDisposedException)
      {
      }
    }
  }
}