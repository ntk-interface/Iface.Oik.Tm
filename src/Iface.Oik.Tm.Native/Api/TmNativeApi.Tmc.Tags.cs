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
    var tCommonPoints = GetTmValuesByListExUnsafe(cid,
                                                  TmNativeDefs.TmDataTypes.Analog,
                                                  new[] { new TmNativeDefs.TAdrTm
                                                  {
                                                    Ch    = ch,
                                                    RTU   = rtu,
                                                    Point = point
                                                  } });
    if (tCommonPoints.Length == 0)
    {
      throw new TmNativeException($"Ошибка получения tCommonPoint для тэга #TT:{ch}:{rtu}:{point}");
    }

    var tCommonPoint = tCommonPoints[0];

    return TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.TAnalogPoint>(tCommonPoint.Data);
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


  public static IReadOnlyList<TCommonPointDto> GetTmValuesByListEx(int                                      cid,
                                                                   TmNativeDefs.TmDataTypes                 type,
                                                                   IReadOnlyCollection<TmNativeDefs.TAdrTm> addrList)
  {
    return GetTmValuesByListExUnsafe(cid, type, addrList).Select(TCommonPointDto.Create).ToList();
  }
  
  
  private static unsafe TmNativeDefsUnsafe.TCommonPoint[] GetTmValuesByListExUnsafe(
    int                                      cid,
    TmNativeDefs.TmDataTypes                 type,
    IReadOnlyCollection<TmNativeDefs.TAdrTm> addrList)
  {
    TmNativeDefsUnsafe.TCommonPoint* ptr = null;
    
    try
    {
      ptr = TmNative.tmcTMValuesByListEx(cid,
                                         (ushort) type,
                                         0,
                                         (uint) addrList.Count, 
                                         addrList.ToArray());
      if (ptr == null)
      {
        return Array.Empty<TmNativeDefsUnsafe.TCommonPoint>();
      }

      return new ReadOnlySpan<TmNativeDefsUnsafe.TCommonPoint>(ptr, addrList.Count).ToArray();
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