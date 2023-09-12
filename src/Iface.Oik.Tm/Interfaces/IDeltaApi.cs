using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iface.Oik.Tm.Interfaces
{
  public interface IDeltaApi
  {
    void SetCid(int cid);

    
    Task<IReadOnlyCollection<DeltaComponent>> GetComponentsTree();

    
    Task<int> GetTreeChangeValue();

    
    Task GetComponentsItems(DeltaComponent component);


    Task UpdateItemName(DeltaItem item);

    Task<bool> RegisterTracer();

    
    Task UnRegisterTracer();


    Task TraceComponent(DeltaComponent component, 
                        DeltaTraceTypes traceType, 
                        bool showDebugMessages);


    Task StopTrace();


    Task UpdateComponentsTreeLiveInfo(IReadOnlyCollection<DeltaComponent> tree);

    Task UpdatePortsStats(IReadOnlyCollection<DeltaComponent> components);
  }
}