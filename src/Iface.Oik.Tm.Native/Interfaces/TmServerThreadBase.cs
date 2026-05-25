using System;
using System.Buffers.Text;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class TmServerThreadBase
{
  public static T Create<T>(Span<byte> span) 
    where T : TmServerThreadBase, new()
  {
    var thread = new T();

    var sep = " • "u8;
    
    var comma = span.IndexOf((byte)',');
    if (comma < 0)
    {
      throw new FormatException("Failed to parse thread string");
    }

    if (!Utf8Parser.TryParse(span[..comma], out int id, out _))
    {
      throw new FormatException("Failed to parse thread id");
    }

    var rest = span[(comma + 2)..];
    
    var i1 = rest.IndexOf(sep);
    if (i1 < 0)
    {
      throw new FormatException("Failed to parse thread name");
    }

    var name = TmNativeUtil.BytesToString(rest[..i1]);
    rest = rest[(i1 + sep.Length)..];
    
    var i2 = rest.IndexOf(sep);
    if (i2 < 0)
    {
      throw new FormatException("Failed to parse thread uptime");
    }

    var first = rest[..i2];
    rest = rest[(i2 + sep.Length)..];
    
    
    first = first[..^2];
    var second = rest[..^2];

    if (!Utf8Parser.TryParse(first, out int uptime, out _))
    {
      throw new FormatException("Failed to parse thread uptime");
    }

    if (!Utf8Parser.TryParse(second, out float workTime, out _))
    {
      throw new FormatException("Failed to parse thread work time");
    }
    
    thread.Initialize(id, name, uptime, workTime);
    
    return thread;
  }

  protected abstract void Initialize(int id, string name, int upTime, float workTime);

}