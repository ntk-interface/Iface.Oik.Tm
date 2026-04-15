using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static Dictionary<string, string> GetPrivateSection(nint      filePointer,
                                                             string    section,
                                                             Encoding? encoding = null)
  {
    const int maxSectionSize = 0x100000;

    encoding ??= Encoding.UTF8;

    var pool = ArrayPool<byte>.Shared;
    var buf  = pool.Rent(maxSectionSize);

    try
    {
      TmNative.ini_ReadSection(filePointer,
                               section,
                               buf,
                               maxSectionSize);

      return TmNativeUtil.GetDictionaryFromTmBytes(buf, encoding);
    }
    finally
    {
      pool.Return(buf);
    }
  }
}