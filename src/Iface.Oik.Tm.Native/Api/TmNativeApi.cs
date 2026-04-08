using System;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial  class TmNativeApi
{
  internal static unsafe string GetTextByRef(byte* ptr, int cid)
  {
    const int bufSize = 128;

    switch (ptr[0])
    {
      case 0:
        return string.Empty;
      case 0x40:
      {
        Span<byte> buf = stackalloc byte[bufSize];
        TmNative.tmcGetTextByRef(cid, (nint)ptr, buf, bufSize);

        return TmNativeUtil.BytesToString(buf);
      }
      default:
        return TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(ptr);
    }
  }
}