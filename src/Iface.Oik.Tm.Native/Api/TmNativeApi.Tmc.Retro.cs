using System;
using System.Collections.Generic;
using System.Linq;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static IReadOnlyList<TmImpulseArchiveDto> GetImpulseArchive(int                                   cid,
                                                                     TmNativeDefs.ImpulseArchiveQueryFlags queryFlags,
                                                                     uint                                  tma,
                                                                     uint                                  uxStartTime,
                                                                     uint                                  uxEndTime,
                                                                     uint                                  step)
  {
    return GetImpulseArchiveUnsafe(cid, queryFlags, tma, uxStartTime, uxEndTime, step)
          .Select(point => new TmImpulseArchiveDto(point.Tag,
                                                   point.Ut,
                                                   point.Ms,
                                                   point.Flags,
                                                   point.Value))
          .ToList();
  }


  private static unsafe TmNativeDefsUnsafe.TMAAN_ARCH_VALUE[] GetImpulseArchiveUnsafe(int cid,
    TmNativeDefs.ImpulseArchiveQueryFlags                                                 queryFlags,
    uint                                                                                  tma,
    uint                                                                                  uxStartTime,
    uint                                                                                  uxEndTime,
    uint                                                                                  step)
  {
    TmNativeDefsUnsafe.TMAAN_ARCH_VALUE* ptr = null;

    try
    {
      ptr = TmNative.tmcAanReadArchive(cid,
                                       tma,
                                       uxStartTime,
                                       uxEndTime,
                                       step,
                                       (uint) queryFlags,
                                       out var count,
                                       null,
                                       nint.Zero);
      if (ptr == null)
      {
        return Array.Empty<TmNativeDefsUnsafe.TMAAN_ARCH_VALUE>();
      }

      return new ReadOnlySpan<TmNativeDefsUnsafe.TMAAN_ARCH_VALUE>(ptr, (int)count).ToArray();
    }
    finally
    {
      if (ptr != null)
      {
        TmNative.tmcFreeMemory((IntPtr)ptr);
      }
    }
  }
}