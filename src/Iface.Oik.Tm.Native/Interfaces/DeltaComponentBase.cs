using System;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class DeltaComponentBase
{
  private static readonly char[] СharArray = { ' ', '\t' };

  internal static T Create<T>(ReadOnlySpan<char> line)
    where T : DeltaComponentBase, new()
  {
    var components = new T();

    var (name, type, traceChain) = ParseComponentString(line, СharArray);
    
    components.Initialize(name, type, traceChain);
    
    return components;
  }

  protected abstract void Initialize(string name, string type, uint[] traceChain);
  
  internal static (string name, string type, uint[] traceChain) ParseComponentString(ReadOnlySpan<char> span,
    char[]                                                                                              charsToTrim)
  {
    var sep = span.IndexOf(';');

    if (sep < 0)
    {
      throw new FormatException("Missing ';'");
    }

    var left  = span[..sep];
    var right = span[(sep + 1)..];

    var leftCount  = CountCommas(left)  + 1;
    var rightCount = CountCommas(right) + 1;

    var traceChain  = new uint[leftCount];
    var nameAndType = new string[rightCount];

    // Parse trace chain
    var index            = 0;
    while (!left.IsEmpty)
    {
      var comma = left.IndexOf(',');

      ReadOnlySpan<char> item;
      if (comma >= 0)
      {
        item = left[..comma];
        left = left[(comma + 1)..];
      }
      else
      {
        item = left;
        left = default;
      }

      item = item.Trim(charsToTrim);

      var chainLink = uint.Parse(item);
      
      traceChain[index++] = chainLink;
    }

    // Parse name and type
    index = 0;
    while (!right.IsEmpty)
    {
      var comma = right.IndexOf(',');

      ReadOnlySpan<char> item;
      if (comma >= 0)
      {
        item  = right[..comma];
        right = right[(comma + 1)..];
      }
      else
      {
        item  = right;
        right = default;
      }

      item = item.Trim(charsToTrim);

      nameAndType[index++] = item.ToString();
    }

    return (nameAndType[1], nameAndType[0], traceChain);
  }

  internal static int CountCommas(ReadOnlySpan<char> span)
  {
    var count = 0;
    while (true)
    {
      var i = span.IndexOf(',');
      if (i < 0)
        break;

      count++;
      span = span[(i + 1)..];
    }

    return count;
  }
}