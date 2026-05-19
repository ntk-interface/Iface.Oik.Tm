using System;

namespace Iface.Oik.Tm.Native.Utils;

public static class TmNativeExtensions
{
  public static byte ElementAtOrDefault(this ReadOnlySpan<byte> span, int index, byte defaultValue = 0)
  {
    return (uint)index < (uint)span.Length ? span[index] : defaultValue;
  }
}