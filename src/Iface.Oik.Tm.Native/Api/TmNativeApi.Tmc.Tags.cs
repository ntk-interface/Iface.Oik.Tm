using System;
using System.Buffers;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Dto;
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
                         TmNativeDefs.TmDataTypes.Status => TmNative.tmcGetStatusClassData(cid, 1, new [] {tmAddr}),
                         TmNativeDefs.TmDataTypes.Analog => TmNative.tmcGetAnalogClassData(cid, 1, new [] {tmAddr}),
                         _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    if (classDataPtr == nint.Zero)
    {
      return string.Empty;
    }

    var str = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(classDataPtr + sizeof(byte*));
    TmNative.tmcFreeMemory(classDataPtr);

    return str;
  }

  public static int GetStatusFromRetro(int tmCid, int ch, int rtu, int point, DateTime time)
  {
    var utcTime = TmNativeUtil.GetUtcTimestampFromDateTime(time);

    var (isSuccess, statusPoint) = GetStatusFullEx(tmCid, (short)ch, (short)rtu, (short)point, utcTime);
    
    var flags = (TmNativeDefs.Flags)statusPoint.Flags;

    if (!isSuccess || flags.HasFlag(TmNativeDefs.Flags.UnreliableHdw))
    {
      return -1;
    }

    return statusPoint.Status;
  }
  
  public static IReadOnlyCollection<T> GetStatusRetroEx<T>(GetStatusRetroExArgs args) 
    where T: TmStatusRetroBase, new()
  {
    if (args.StartTime >= args.EndTime)
    {
      return Array.Empty<T>();
    }

    var currentTime = args.StartTime;
    var retros      = new List<T>();

    while (currentTime <= args.EndTime)
    {
      var time   = TmNativeUtil.GetUtcTimestampFromDateTime(currentTime);
      var (isSuccess, point) = GetStatusFullEx(args.TmCid, 
                                   args.Ch, 
                                   args.Rtu, 
                                   args.Point, 
                                   time, 
                                   args.UserRealTelemetry);

      if (isSuccess)
      {
        retros.Add(TmStatusRetroBase.Create<T>(point, currentTime));
      }
      
      currentTime = currentTime.AddSeconds(args.Step);
    }

    return retros;
  }
  
  
  internal static string GetTagProperties(int                      cid,
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
  
  
  internal static unsafe TmNativeDefsUnsafe.TAnalogPoint GetTAnalogPoint(int   cid,
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

  
  internal static (bool isSuccess, TmNativeDefsUnsafe.TStatusPoint point) GetStatusFullEx(int cid,
    short                                                                                   ch,
    short                                                                                   rtu,
    short                                                                                   point,
    long                                                                                    currentTime,
    bool                                                                                    getRealTelemetry = false)
  {
    var tmcStatusPoint = new TmNativeDefsUnsafe.TStatusPoint();
    var time           = TmNative.uxgmtime2uxtime(currentTime);

    var result = TmNative.tmcStatusFullEx(cid,
                                          getRealTelemetry
                                            ? (short)(ch + TmNativeDefs.RealTelemetryFlag)
                                            : ch,
                                          rtu,
                                          point,
                                          ref tmcStatusPoint,
                                          (uint)time);

    return (result != TmNativeDefs.Success, tmcStatusPoint);
  }
}