using System.Collections.Generic;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Utils
{
  public static class DeltaComponentExtensions
  {
    public static DeltaComponent FindByTraceChainString(this IEnumerable<DeltaComponent> tree, DeltaComponent comparison)
    {
      if (comparison == null) return null;
      
      foreach (var root in tree)
      {
        if (root.TraceChainString == comparison.TraceChainString)
        {
          return root;
        }

        var child = root.Children.FindByTraceChainString(comparison);
        if (child != null)
        {
          return child;
        }

      }
      
      return null;
    } 
  }
}