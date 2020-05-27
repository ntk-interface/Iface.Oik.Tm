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
  }
}