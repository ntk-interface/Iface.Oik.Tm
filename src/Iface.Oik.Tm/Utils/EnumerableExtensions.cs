using System;
using System.Collections.Generic;
using System.Linq;

namespace Iface.Oik.Tm.Utils
{
  public static class EnumerableExtensions
  {
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
      if (action == null) throw new ArgumentNullException();

      foreach (var item in source)
      {
        action(item);
      }

      return source;
    }

    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
      if (action == null) throw new ArgumentNullException();

      int index = 0;
      foreach (var item in source)
      {
        action(item, index++);
      }

      return source;
    }


    public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
    {
      return source == null || 
             !source.Any();
    }
  }
}