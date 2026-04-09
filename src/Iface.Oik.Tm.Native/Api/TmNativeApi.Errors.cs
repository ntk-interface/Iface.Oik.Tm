using System;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static int GetLastTmcError()
  {
    return (int)TmNative.tmcGetLastError();
  }
  
  public static unsafe string GetLastTmcErrorText(int tmCid)
  {

    byte* bufPtr;
    TmNative.tmcGetLastErrorText(tmCid, &bufPtr);

    var str = TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(bufPtr);
    TmNative.tmcFreeMemory((nint)bufPtr);

    return str;
  }
  
  public static string GetTmcConnectionErrorText(int tmCid)
  {
    const uint bufSize = 256;
    Span<byte> buf     = stackalloc byte[(int)bufSize];

    var result = TmNative.tmcGetConnectErrorText(tmCid, buf, bufSize);

    return result ? TmNativeUtil.BytesToString(buf) : "Неизвестная ошибка";
  }
}