using System.Collections.Generic;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class AccessMasksDescriptorBase<T> where T : AccessMaskBase
{
  public string                     NamePrefix  { get; set; } = string.Empty;
  public Dictionary<string, string> ObjTypeName { get; init; } = new();
  public List<T>                    AccessMasks { get; set; }  = new();
}

public abstract  class AccessMaskBase
{
  public uint                       Mask        { get; set; }
  public Dictionary<string, string> Description { get; init; } = new();
}