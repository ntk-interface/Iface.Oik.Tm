using System.Collections.Generic;
using System.Linq;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Utils
{
  public static class TmServerExtensions
  {
    public static bool GotChildTmServer(this TmServer node, TmServer serverToFind)
    {
      foreach (var child in node.Children)
      {
        if (child.ProcessId == serverToFind.ProcessId)
        {
          return true;
        }

        var nextDepthCheck = child.GotChildTmServer(serverToFind);
        if (nextDepthCheck) return true;
      }

      return false;
    }
    
    public static IEnumerable<TmServer> Flatten(this TmServer root)
    {
      var queue = new Queue<TmServer>();
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
}