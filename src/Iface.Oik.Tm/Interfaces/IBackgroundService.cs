using System.Threading;
using System.Threading.Tasks;

namespace Iface.Oik.Tm.Interfaces
{
  public interface IBackgroundService
  {
    Task StartAsync(CancellationToken cancellationToken);
    Task StartAsync();
    
    Task StopAsync(CancellationToken cancellationToken);
    Task StopAsync();
  }
}