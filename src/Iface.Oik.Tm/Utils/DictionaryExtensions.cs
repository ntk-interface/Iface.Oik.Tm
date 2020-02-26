using System.Collections.Generic;
using System.Linq;

namespace Iface.Oik.Tm.Utils
{
  public static class DictionaryExtensions
  {
    // важно - работает только для простых типов Value, для которых реализован EqualityComparer
    public static bool DictionaryEquals<TKey, TValue>(this IDictionary<TKey, TValue> dict1,
                                                      IDictionary<TKey, TValue>      dict2)
    {
      if (dict1 == dict2)
      {
        return true;
      }
      if (dict1 == null || dict2 == null)
      {
        return false;
      }
      if (dict1.Count != dict2.Count)
      {
        return false;
      }
      return dict1.All(kvp => dict2.TryGetValue(kvp.Key, out var value2) &&
                              (kvp.Value == null ? value2 == null : kvp.Value.Equals(value2)));
    }
  }
}