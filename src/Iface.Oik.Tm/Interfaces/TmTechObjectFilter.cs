using System.Collections.Generic;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmTechObjectFilter
  {
    public uint? Scheme { get; set; }

    public uint? Type { get; set; }

    public IEnumerable<string> Properties { get; set; }
  }
}