using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using Iface.Oik.Tm.Native.Dto;
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
    const int bufSize = 1024;
    var       pool    = ArrayPool<byte>.Shared;
    var       buf     = pool.Rent(bufSize);

    try
    {
      TmNative.tmcGetObjectProperties(cid,
                                      (ushort)type,
                                      ch,
                                      rtu,
                                      point,
                                      buf,
                                      bufSize);
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(buf);
    }

    return TmNativeUtil.BytesToString(buf);
  }

  private static unsafe TmNativeDefsUnsafe.TAnalogPoint GetTAnalogPoint(int   cid,
                                                                        short ch,
                                                                        short rtu,
                                                                        short point)
  {
    var tmAddr = new TmNativeDefs.TAdrTm
    {
      Ch    = ch,
      RTU   = rtu,
      Point = point
    };

    var tmcCommonPointsPtr = TmNative.tmcTMValuesByListEx(cid,
                                                          (ushort)TmNativeDefs.TmDataTypes.Analog, 0,
                                                          1,
                                                          new[] { tmAddr });

    if (tmcCommonPointsPtr == nint.Zero)
    {
      throw new TmNativeException($"Ошибка получения tCommonPoint для тэга #TT:{ch}:{rtu}:{point}");
    }
    
    var tCommonPoint = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.TCommonPoint>((byte*)tmcCommonPointsPtr);
    var tAnalogPoint = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.TAnalogPoint>(tCommonPoint.Data);
    
    TmNative.tmcFreeMemory(tmcCommonPointsPtr);

    return tAnalogPoint;
  }


  public static IReadOnlyList<TCommonPointDto> GetTmTagNamedSetUpdatedValues(int                      cid,
                                                                             TmNativeDefs.TmDataTypes type,
                                                                             Span<byte>               name)
  {
    return GetTmTagNamedSetUpdatedValuesUnsafe(cid, type, name).Select(TCommonPointDto.Create).ToList();
  }
  
  
  private static unsafe TmNativeDefsUnsafe.TCommonPoint[] GetTmTagNamedSetUpdatedValuesUnsafe(
    int                      cid,
    TmNativeDefs.TmDataTypes type,
    Span<byte>               name)
  {
    TmNativeDefsUnsafe.TCommonPoint* ptr = null;
    try
    {
      ptr = TmNative.tmcTmvUserSetGet(cid,
                                      (ushort)type,
                                      changesOnly: true,
                                      name,
                                      out var count);
      if (count == 0 || ptr == null)
      {
        return Array.Empty<TmNativeDefsUnsafe.TCommonPoint>();
      }

      return new ReadOnlySpan<TmNativeDefsUnsafe.TCommonPoint>(ptr, (int)count).ToArray();
    }
    finally
    {
      if (ptr != null)
      {
        TmNative.tmcFreeMemory((IntPtr) ptr);
      }
    }
  }
}