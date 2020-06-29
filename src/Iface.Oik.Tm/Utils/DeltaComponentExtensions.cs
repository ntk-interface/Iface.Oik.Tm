using System.Collections.Generic;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Utils
{
  public static class DeltaComponentExtensions
  {
    public static IEnumerable<DeltaComponent> Flatten(this IEnumerable<DeltaComponent> tree)
    {
      var queue = new Queue<DeltaComponent>();
      foreach (var root in tree)
      {
        queue.Enqueue(root);
        while(queue.Count > 0)
        {
          var current = queue.Dequeue();
          yield return current;
          foreach(var child in current.Children)
            queue.Enqueue(child);
        }
      }
    }
    
    public static DeltaComponent FindNode(this IEnumerable<DeltaComponent> tree, DeltaComponent comparison)
    {
      if (comparison == null) return null;
      
      foreach (var root in tree)
      {
        if (root.TraceChainString == comparison.TraceChainString)
        {
          return root;
        }

        var child = root.Children.FindNode(comparison);
        if (child != null)
        {
          return child;
        }

      }
      
      return null;
    }
    
    public static DeltaComponent FindNode(this IEnumerable<DeltaComponent> tree, string traceChainString)
    {
      if (traceChainString == null) return null;
      
      foreach (var root in tree)
      {
        if (root.TraceChainString == traceChainString)
        {
          return root;
        }

        var child = root.Children.FindNode(traceChainString);
        if (child != null)
        {
          return child;
        }

      }
      
      return null;
    }
  }
  
}