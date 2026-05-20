using System;
using System.Buffers;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Dto;
using System.Linq;
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
                         TmNativeDefs.TmDataTypes.Status => TmNative.tmcGetStatusClassData(cid, 1, new[] { tmAddr }),
                         TmNativeDefs.TmDataTypes.Analog => TmNative.tmcGetAnalogClassData(cid, 1, new[] { tmAddr }),
                         _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
                       };

    if (classDataPtr == nint.Zero)
    {
      return string.Empty;
    }

    var str = TmNativeUtil.GetCStringFromIntPtr(classDataPtr + sizeof(byte*));
    TmNative.tmcFreeMemory(classDataPtr);

    return str;
  }

  public static int GetStatusFromRetro(int tmCid, int ch, int rtu, int point, DateTime time)
  {
    var utcTime    = TmNativeUtil.GetUtcTimestampFromDateTime(time);
    var serverTime = TmNative.uxgmtime2uxtime(utcTime);

    var (isSuccess, statusPoint) = GetStatusFullEx(tmCid, (short)ch, (short)rtu, (short)point, serverTime);

    var flags = (TmNativeDefs.Flags)statusPoint.Flags;

    if (!isSuccess || flags.HasFlag(TmNativeDefs.Flags.UnreliableHdw))
    {
      return -1;
    }

    return statusPoint.Status;
  }

  public static IReadOnlyCollection<T> GetStatusRetroEx<T>(GetRetroExArgs args)
    where T : TmStatusRetroBase, new()
  {
    if (args.StartTime >= args.EndTime)
    {
      return Array.Empty<T>();
    }

    var currentTime = args.StartTime;
    var retros      = new List<T>();

    while (currentTime <= args.EndTime)
    {
      var time       = TmNativeUtil.GetUtcTimestampFromDateTime(currentTime);
      var serverTime = TmNative.uxgmtime2uxtime(time);

      var (isSuccess, point) = GetStatusFullEx(args.TmCid,
                                               args.Ch,
                                               args.Rtu,
                                               args.Point,
                                               serverTime,
                                               args.UserRealTelemetry);

      if (isSuccess)
      {
        retros.Add(TmStatusRetroBase.Create<T>(point, serverTime));
      }

      currentTime = currentTime.AddSeconds(args.Step);
    }

    return retros;
  }


  public static float GetAnalog(int       tmCid,
                                int       ch,
                                int       rtu,
                                int       point,
                                DateTime? dateTime,
                                int       retroNum)
  {
    var time = dateTime is { } dt
                 ? dt.ToNativeByteArray()
                 : Span<byte>.Empty;

    return TmNative.tmcAnalog(tmCid, (short)ch, (short)rtu, (short)point, time, (short)retroNum);
  }


  public static (bool isSuccess, TmAnalogRetroDto dto) GetAnalogFromRetro(int      tmCid,
                                                                          int      ch,
                                                                          int      rtu,
                                                                          int      point,
                                                                          DateTime dateTime,
                                                                          int      retroNum)
  {
    var (isSuccess, analogPoint) = GetAnalogFull(tmCid, 
                                                 (short)ch, 
                                                 (short)rtu, 
                                                 (short)point, 
                                                 dateTime, 
                                                 (short)retroNum);

    if (!isSuccess)
    {
      return(false, new TmAnalogRetroDto());
    }
    
    var utcTime = TmNativeUtil.GetUtcTimestampFromDateTime(dateTime);

    var dto = new TmAnalogRetroDto
    {
      Value = analogPoint.AsFloat,
      Flags = analogPoint.Flags,
      Code  = analogPoint.AsCode,
      Time  = TmNative.uxgmtime2uxtime(utcTime),
    };

    return (true, dto);
  }


  public static IReadOnlyCollection<T> GetAnalogRetroEx<T>(GetRetroExArgs args)
    where T : TmAnalogRetroBase, new()
  {
    if (args.StartTime >= args.EndTime)
    {
      return Array.Empty<T>();
    }

    var currentTime = args.StartTime;
    var retros      = new List<T>();

    while (currentTime <= args.EndTime)
    {
      var time = currentTime;

      var (isSuccess, tAnalogPoint) = GetAnalogFull(args.TmCid,
                                                    args.Ch,
                                                    args.Rtu,
                                                    args.Point,
                                                    time,
                                                    (short)args.RetroNum,
                                                    args.UserRealTelemetry);

      if (isSuccess)
      {
        retros.Add(TmAnalogRetroBase.Create<T>(tAnalogPoint, time));
      }

      currentTime = currentTime.AddSeconds(args.Step);
    }

    return retros;
  }


  public static unsafe IReadOnlyCollection<T[]> GetAnalogMicroseries<T>(int cid, Span<TmNativeDefs.TAdrTm> tmAddrs) 
    where T: TmAnalogMicroSeriesBase, new()
  {
    var count      = tmAddrs.Length;
    
    var pool    = ArrayPool<nint>.Shared;
    var buf     = pool.Rent(count);

    try
    {
      var fetchResult = TmNative.tmcAnalogMicroSeries(cid, (uint)count, tmAddrs, buf);

      if (fetchResult != TmNativeDefs.Success)
      {
        foreach (var ptr in buf)
        {
          TmNative.tmcFreeMemory(ptr);
        }
        return new[] { Array.Empty<T>() };
      }

      var result = new List<T[]>(count);
      
      foreach (var ptr in buf)
      {
        if (ptr == nint.Zero)
        {
          break;
        }
        
        var series   = TmNativeUtil.FromIntPtr<TmNativeDefsUnsafe.TMSAnalogMSeries>(ptr);
        
        var elements = new Span<TmNativeDefsUnsafe.TMSAnalogMSeriesElement>(series.Elements, series.Count);

        var tSeries = new T[elements.Length];

        
        for (var i = 0; i < elements.Length; i++)
        {
          tSeries[i] = TmAnalogMicroSeriesBase.Create<T>(elements[i].Value,
                                                         elements[i].SFlg,
                                                         elements[i].Ut);
        }

        result.Add(tSeries);
        
        TmNative.tmcFreeMemory(ptr);
      }
      
      return result;
    }
    finally
    {
      ArrayPool<nint>.Shared.Return(buf);
    }
  }
  

  public static float GetAccumValue(int tmCid, int ch, int rtu, int point)
  {
    return TmNative.tmcAccumValue(tmCid, (short)ch, (short)rtu, (short)point, Span<byte>.Empty);
  }

  public static float GetAccumLoad(int tmCid, int ch, int rtu, int point)
  {
    return TmNative.tmcAccumLoad(tmCid, (short)ch, (short)rtu, (short)point, Span<byte>.Empty);
  }

  public static (bool isSuccess, TmAccumRetroDto dto) GetAccumFromRetro(int      tmCid,
                                                                        int      ch,
                                                                        int      rtu,
                                                                        int      point,
                                                                        DateTime dateTime)
  {
    var (isSuccess, accumPoint) = GetAccumFull(tmCid, (short)ch, (short)rtu, (short)point, dateTime);

    if (!isSuccess)
    {
      return(false, new TmAccumRetroDto());
    }

    var utcTime = TmNativeUtil.GetUtcTimestampFromDateTime(dateTime);
    var dto = new TmAccumRetroDto
    {
      Value = accumPoint.Value,
      Load = accumPoint.Load,
      Flags = accumPoint.Flags,
      Time = TmNative.uxgmtime2uxtime(utcTime)
    };

    return (true, dto);
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
                                                  new[]
                                                  {
                                                    new TmNativeDefs.TAdrTm
                                                    {
                                                      Ch    = ch,
                                                      RTU   = rtu,
                                                      Point = point
                                                    }
                                                  });
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
    return GetTmTagNamedSetUpdatedValuesUnsafe(cid, type, name)
          .Select(commonPoint => TCommonPointDto.Create(commonPoint, queryUnit: false))
          .ToList();
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
        TmNative.tmcFreeMemory((IntPtr)ptr);
      }
    }
  }


  public static IReadOnlyList<TCommonPointDto> GetTmValuesByListEx(int                                      cid,
                                                                   TmNativeDefs.TmDataTypes                 type,
                                                                   IReadOnlyCollection<TmNativeDefs.TAdrTm> addrList)
  {
    return GetTmValuesByListExUnsafe(cid, type, addrList)
          .Select(commonPoint => TCommonPointDto.Create(commonPoint, queryUnit: false))
          .ToList();
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
                                         (ushort)type,
                                         0,
                                         (uint)addrList.Count,
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
        TmNative.tmcFreeMemory((IntPtr)ptr);
      }
    }
  }


  public static IReadOnlyList<TCommonPointDto> GetValuesByFlagMask(int                      cid,
                                                                   TmNativeDefs.TmDataTypes tmType,
                                                                   TmNativeDefs.Flags       tmFlags,
                                                                   TmNativeDefs.TmCpf       queryFlags)
  {
    return GetValuesByFlagMaskUnsafe(cid, tmType, tmFlags, queryFlags)
          .Select(commonPoint => TCommonPointDto.Create(commonPoint, queryUnit: true, cid))
          .ToList();
  }


  private static unsafe TmNativeDefsUnsafe.TCommonPoint[] GetValuesByFlagMaskUnsafe(int cid,
    TmNativeDefs.TmDataTypes                                                            tmType,
    TmNativeDefs.Flags                                                                  tmFlags,
    TmNativeDefs.TmCpf                                                                  queryFlags)
  {
    TmNativeDefsUnsafe.TCommonPoint* ptr = null;

    try
    {
      ptr = TmNative.tmcGetValuesByFlagMask(cid,
                                            (ushort)tmType,
                                            (uint) tmFlags,
                                            (byte) (queryFlags | TmNativeDefs.TmCpf.Name),
                                            out var count);
      if (ptr == null)
      {
        return Array.Empty<TmNativeDefsUnsafe.TCommonPoint>();
      }

      return new ReadOnlySpan<TmNativeDefsUnsafe.TCommonPoint>(ptr, (int) count).ToArray();
    }
    finally
    {
      if (ptr != null)
      {
        TmNative.tmcFreeMemory((IntPtr)ptr);
      }
    }
  }


  public static IReadOnlyList<TCommonPointDto> GetTagsByGroupName(int                      cid,
                                                                  TmNativeDefs.TmDataTypes tmType,
                                                                  string                   groupName)
  {
    return GetTagsByGroupNameUnsafe(cid, tmType, groupName)
          .Select(commonPoint => TCommonPointDto.Create(commonPoint, queryUnit: true, cid))
          .ToList();
  }


  private static unsafe TmNativeDefsUnsafe.TCommonPoint[] GetTagsByGroupNameUnsafe(int cid,
    TmNativeDefs.TmDataTypes                                                           tmType,
    string                                                                             groupName)
  {
    TmNativeDefsUnsafe.TCommonPoint* ptr = null;

    try
    {
      ptr = TmNative.tmcGetValuesEx(cid,
                                    (ushort)tmType,
                                    0,
                                    0,
                                    (byte)TmNativeDefs.TmCpf.Name,
                                    TmNativeUtil.StringToBytes(groupName),
                                    0,
                                    out var count);
      if (ptr == null)
      {
        return Array.Empty<TmNativeDefsUnsafe.TCommonPoint>();
      }

      return new ReadOnlySpan<TmNativeDefsUnsafe.TCommonPoint>(ptr, (int) count).ToArray();
    }
    finally
    {
      if (ptr != null)
      {
        TmNative.tmcFreeMemory((IntPtr)ptr);
      }
    }
  }


  public static IReadOnlyList<TAdrTmDto> GetTagsByNamePattern(int                      cid,
                                                              TmNativeDefs.TmDataTypes tmType,
                                                              string                   pattern)
  {
    return GetTagsByNamePatternUnsafe(cid, tmType, pattern)
          .Select(adrTm => new TAdrTmDto(adrTm.Ch, adrTm.RTU, adrTm.Point))
          .ToList();
  }


  private static unsafe TmNativeDefsUnsafe.TAdrTm[] GetTagsByNamePatternUnsafe(int                      cid,
                                                                               TmNativeDefs.TmDataTypes tmType,
                                                                               string                   pattern)
  {
    TmNativeDefsUnsafe.TAdrTm* ptr = null;

    try
    {
      ptr = TmNative.tmcTextSearch(cid,
                                   (ushort)tmType,
                                   TmNativeUtil.StringToBytes(pattern),
                                   out var count);
      if (ptr == null)
      {
        return Array.Empty<TmNativeDefsUnsafe.TAdrTm>();
      }

      return new ReadOnlySpan<TmNativeDefsUnsafe.TAdrTm>(ptr, (int) count).ToArray();
    }
    finally
    {
      if (ptr != null)
      {
        TmNative.tmcFreeMemory((IntPtr)ptr);
      }
    }
  }


  public static IReadOnlyList<TAdrTmDto> GetPresentAps(int cid)
  {
    return GetPresentApsUnsafe(cid)
          .Select(adrTm => new TAdrTmDto(adrTm.Ch, adrTm.RTU, adrTm.Point))
          .ToList();
  }


  private static unsafe TmNativeDefsUnsafe.TAdrTm[] GetPresentApsUnsafe(int cid)
  {
    TmNativeDefsUnsafe.TAdrTm* ptr = null;

    try
    {
      ptr = TmNative.tmcTakeAPS(cid);
      if (ptr == null)
      {
        return Array.Empty<TmNativeDefsUnsafe.TAdrTm>();
      }

      var count      = 0;
      var currentPtr = ptr;
      while (currentPtr->Point != 0)
      {
        count++;
        currentPtr++;
      }

      return new ReadOnlySpan<TmNativeDefsUnsafe.TAdrTm>(ptr, count).ToArray();
    }
    finally
    {
      if (ptr != null)
      {
        TmNative.tmcFreeMemory((IntPtr)ptr);
      }
    }
  }


  internal static (bool isSuccess, TmNativeDefsUnsafe.TStatusPoint point) GetStatusFullEx(int tmCid,
    short                                                                                     ch,
    short                                                                                     rtu,
    short                                                                                     point,
    long                                                                                      time,
    bool                                                                                      getRealTelemetry = false)
  {
    var tmcStatusPoint = new TmNativeDefsUnsafe.TStatusPoint();

    var result = TmNative.tmcStatusFullEx(tmCid,
                                          getRealTelemetry ? (short)(ch + TmNativeDefs.RealTelemetryFlag) : ch,
                                          rtu,
                                          point,
                                          ref tmcStatusPoint,
                                          (uint)time);

    return (result == TmNativeDefs.Success, tmcStatusPoint);
  }

  internal static (bool isSuccess, TmNativeDefsUnsafe.TAnalogPoint point) GetAnalogFull(int tmCid,
    short                                                                                   ch,
    short                                                                                   rtu,
    short                                                                                   point,
    DateTime                                                                                time,
    short                                                                                   retroNum         = 0,
    bool                                                                                    getRealTelemetry = false)
  {
    var tmcAnalogPoint = new TmNativeDefsUnsafe.TAnalogPoint();

    var result = TmNative.tmcAnalogFull(tmCid,
                                        getRealTelemetry ? (short)(ch + TmNativeDefs.RealTelemetryFlag) : ch,
                                        rtu,
                                        point,
                                        ref tmcAnalogPoint,
                                        time.ToNativeByteArray(),
                                        retroNum);

    return (result == TmNativeDefs.Success, tmcAnalogPoint);
  }

  internal static (bool isSuccess, TmNativeDefsUnsafe.TAccumPoint point) GetAccumFull(int tmCid,
    short                                                                                 ch,
    short                                                                                 rtu,
    short                                                                                 point,
    DateTime                                                                              time)
  {
    var accumPoint = new TmNativeDefsUnsafe.TAccumPoint();

    var result = TmNative.tmcAccumFull(tmCid,
                                          ch,
                                          rtu,
                                          point,
                                          ref accumPoint,
                                          time.ToNativeByteArray());

    return (result == TmNativeDefs.Success, accumPoint);
  }
}