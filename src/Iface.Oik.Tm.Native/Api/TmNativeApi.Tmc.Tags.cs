using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static unsafe string GetTagClassData(int                      cid,
                                              TmNativeDefs.TmDataTypes type,
                                              short                    ch,
                                              short                    rtu,
                                              short                    point)
  {
    var tmAddr = new TmNativeDefs.TAdrTm
    {
      Ch    = ch,
      RTU   = rtu,
      Point = point
    };

    var classDataPtr = type switch
                       {
                         TmNativeDefs.TmDataTypes.Status => TmNative.tmcGetStatusClassData(cid, 1, [tmAddr]),
    TmNativeDefs.TmDataTypes.Analog => TmNative.tmcGetAnalogClassData(cid, 1, [tmAddr]),
    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    if (classDataPtr == nint.Zero)
    {
      return string.Empty;
    }

    var str = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(classDataPtr + sizeof(byte*));
    TmNative.tmcFreeMemory(classDataPtr);

    return str;
  }

  private static string GetTagProperties(int                      cid,
                                       TmNativeDefs.TmDataTypes type,
                                       short                    ch,
                                       short                    rtu,
                                       short                    point)
  {
    Span<byte> sb = stackalloc byte[1024];

    TmNative.tmcGetObjectProperties(cid,
                                    (ushort)type,
                                    ch,
                                    rtu,
                                    point,
                                    sb,
                                    1024);
    return TmNativeUtil.BytesToString(sb);
  }
}