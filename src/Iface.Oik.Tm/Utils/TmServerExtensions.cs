using System.Collections.Generic;
using System.Linq;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Utils
{
  public static class TmServerExtensions
  {
    public static IEnumerable<TmServer> Flatten(this IEnumerable<TmServer> tree)
    {
      var queue = new Queue<TmServer>();
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

    public static TmServer FindServerByHash(this IEnumerable<TmServer> tree, TmServer comparison)
    {
      if (comparison == null) return null;
      
      foreach (var root in tree)
      {
        if (root.GetHashCode() == comparison.GetHashCode())
        {
          return root;
        }

        var child = root.Children.FindServerByHash(comparison);
        if (child != null)
        {
          return child;
        }

      }
      
      return null;
    } 
  }
}