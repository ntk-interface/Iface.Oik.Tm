using System;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Dto;

public record struct TmStatusRecord(ushort    Ch,
                                    ushort    Rtu,
                                    ushort    Point,
                                    short     Status,
                                    TmFlags   Flags,
                                    TmS2Flags S2Flags,
                                    DateTime? ChangeTime)
{
  public static TmStatusRecord CreateFromTmcCommonPoint(TmNativeDefs.TCommonPoint tmcCommonPoint)
  {
    TmNativeDefs.TStatusPoint tmcStatusPoint;
    try
    {
      tmcStatusPoint = TmNativeUtil.GetStatusPointFromCommonPoint(tmcCommonPoint);
    }
    catch (ArgumentException)
    {
      throw new TmNativeException("Отсутствует StatusPoint внутри CommonPoint");
    }

    return new TmStatusRecord(tmcCommonPoint.Ch,
                              tmcCommonPoint.RTU,
                              tmcCommonPoint.Point,
                              tmcStatusPoint.Status,
                              (TmFlags)tmcStatusPoint.Flags,
                              (TmS2Flags)tmcCommonPoint.tm_s2,
                              DateUtil.GetDateTimeFromTimestampWithEpochCheck(tmcCommonPoint.tm_local_ut,
                                                                              tmcCommonPoint.tm_local_ms));
  }
}


public record struct TmAnalogRecord(ushort    Ch,
                                    ushort    Rtu,
                                    ushort    Point,
                                    float     Value,
                                    TmFlags   Flags,
                                    DateTime? ChangeTime)
{
  public static TmAnalogRecord CreateFromTmcCommonPoint(TmNativeDefs.TCommonPoint tmcCommonPoint)
  {
    TmNativeDefs.TAnalogPoint tmcAnalogPoint;
    try
    {
      tmcAnalogPoint = TmNativeUtil.GetAnalogPointFromCommonPoint(tmcCommonPoint);
    }
    catch (ArgumentException)
    {
      throw new TmNativeException("Отсутствует AnalogPoint внутри CommonPoint");
    }

    return new TmAnalogRecord(tmcCommonPoint.Ch,
                              tmcCommonPoint.RTU,
                              tmcCommonPoint.Point,
                              tmcAnalogPoint.AsFloat,
                              (TmFlags)tmcAnalogPoint.Flags,
                              DateUtil.GetDateTimeFromTimestampWithEpochCheck(tmcCommonPoint.tm_local_ut,
                                                                              tmcCommonPoint.tm_local_ms));
  }
}


public record struct TmAccumRecord(ushort    Ch,
                                   ushort    Rtu,
                                   ushort    Point,
                                   float     Value,
                                   float     Load,
                                   TmFlags   Flags,
                                   DateTime? ChangeTime)
{
  public static TmAccumRecord CreateFromTmcCommonPoint(TmNativeDefs.TCommonPoint tmcCommonPoint)
  {
    TmNativeDefs.TAccumPoint tmcAccumPoint;
    try
    {
      tmcAccumPoint = TmNativeUtil.GetAccumPointFromCommonPoint(tmcCommonPoint);
    }
    catch (ArgumentException)
    {
      throw new TmNativeException("Отсутствует AccumPoint внутри CommonPoint");
    }

    return new TmAccumRecord(tmcCommonPoint.Ch,
                             tmcCommonPoint.RTU,
                             tmcCommonPoint.Point,
                             tmcAccumPoint.Value,
                             tmcAccumPoint.Load,
                             (TmFlags)tmcAccumPoint.Flags,
                             DateUtil.GetDateTimeFromTimestampWithEpochCheck(tmcCommonPoint.tm_local_ut,
                                                                             tmcCommonPoint.tm_local_ms));
  }
}