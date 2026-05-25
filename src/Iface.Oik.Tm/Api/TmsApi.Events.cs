using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api;

public partial class TmsApi
{
  public async Task<TmEventElix> GetCurrentEventsElix()
  {
    var elix = new TmNativeDefs.TTMSElix();
    if (!await Task.Run(() => TmNative.tmcGetCurrentElix(_cid, ref elix))
                   .ConfigureAwait(false))
    {
      return null;
    }

    return new TmEventElix(elix.R, elix.M);
  }


  public async Task<TmEventElix> GetRecentEventsElix(int          recentCount,
                                                     int          recentHours = 24,
                                                     TmEventTypes eventTypes  = TmEventTypes.Any)
  {
    var utcTime       = DateUtil.GetUtcTimestampFromDateTime(DateTime.Now.Subtract(TimeSpan.FromHours(recentHours)));
    var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);

    var startElix  = new TmNativeDefs.TTMSElix(); // передаём пустой, тогда вернутся последние события
    var resultElix = new TmNativeDefs.TTMSElix();
    if (!await Task.Run(() => TmNative.tmcFindPrevElix(_cid,
                                                       ref startElix,
                                                       ref resultElix,
                                                       (uint)recentCount,
                                                       (uint)serverUtcTime,
                                                       0,
                                                       (ushort)eventTypes))
                   .ConfigureAwait(false))
    {
      return null;
    }

    if (resultElix.R == 0 && resultElix.M == 0)
    {
      return null;
    }

    return new TmEventElix(resultElix.R, resultElix.M);
  }


  public async Task<IReadOnlyCollection<TmEvent>> GetEventsArchive(TmEventFilter filter)
  {
    if (filter.StartTime == null)
    {
      throw new Exception("Не задано время начала и конца архива событий");
    }

    return await Task.Run(() => TmNativeApi.GetEventsArchive<TmEvent>(_cid, filter.ToNative()))
                     .ConfigureAwait(false);
  }


  private static TmNativeDefs.TTMSEventAddData GetEventAddRecData(int index)
  {
    const int  bufSize = 512;
    Span<byte> buf     = stackalloc byte[bufSize];

    TmNative.tmcEventGetAdditionalRecData((uint)index, buf, bufSize);

    return TmNativeUtil.GetEventAddData(buf);
  }


  public async Task<IReadOnlyCollection<TmEvent>> GetEventsArchiveByElix(TmEventFilter filter) // TODO unit test
  {
    return await Task.Run(() => TmNativeApi.GetEventsArchiveByElix<TmEvent>(_cid, filter.ToNative()))
                     .ConfigureAwait(false);
  }


  public async Task<(IReadOnlyCollection<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix)
  {
    if (elix == null) return (null, null);

    var (events, nativeElix) =
      await Task.Run(() => TmNativeApi.GetCurrentEvents<TmEvent>(_cid, elix.R, elix.M))
                .ConfigureAwait(false);

    return (events, new TmEventElix(nativeElix.R, nativeElix.M));
  }

  
  public async Task<bool> UpdateAckedEventsIfAny(IReadOnlyList<TmEvent> tmEvents)
  {
    if (tmEvents.IsNullOrEmpty()) return false;

    var elixList = new TmNativeDefs.TTMSElix[tmEvents.Count];
    for (var i = 0; i < tmEvents.Count; i++)
    {
      elixList[i] = new TmNativeDefs.TTMSElix
      {
        R = tmEvents[i].Elix.R,
        M = tmEvents[i].Elix.M
      };
    }

    await Task.Run(() => TmNative.tmcEventLogAdditionalDataByElixList(_cid, elixList, (uint)elixList.Length))
              .ConfigureAwait(false);
    
    var changesFound = false;
    for (var i = 0; i < tmEvents.Count; i++)
    {
      var extraData = GetEventAddRecData(i);

      if (extraData.AckSec != 0)
      {
        tmEvents[i].AckTime = DateUtil.GetDateTimeFromTimestamp(extraData.AckSec, extraData.AckMs);
        tmEvents[i].AckUser = extraData.UserName;
        changesFound        = true;
      }
    }

    return changesFound;
  }


  public async Task<bool> AckEvent(TmEvent tmEvent)
  {
    if (tmEvent == null)
    {
      return false;
    }

    var nativeElix = new[]
    {
      new TmNativeDefs.TTMSElix
      {
        M = tmEvent.Elix.M,
        R = tmEvent.Elix.R,
      }
    };
    var result = await Task.Run(() => TmNative.tmcEventLogAckRecords(_cid, nativeElix, 1))
                           .ConfigureAwait(false);
    return result;
  }


  public async Task<bool> AckEvents(IReadOnlyList<TmEvent> tmEvents)
  {
    if (tmEvents.IsNullOrEmpty())
    {
      return false;
    }

    var nativeElixes = new TmNativeDefs.TTMSElix[tmEvents.Count];
    for (var i = 0; i < tmEvents.Count; i++)
    {
      nativeElixes[i] = new TmNativeDefs.TTMSElix
      {
        M = tmEvents[i].Elix.M,
        R = tmEvents[i].Elix.R,
      };
    }

    var result = await Task.Run(() => TmNative.tmcEventLogAckRecords(_cid, nativeElixes, (uint)tmEvents.Count))
                           .ConfigureAwait(false);
    return result;
  }


  public async Task AddStringToEventLog(string    message,
                                        TmAddr    tmAddr = null,
                                        DateTime? time   = null)
  {
    if (message == null)
    {
      return;
    }

    await AddStrBinToEventLog(time,
                              TmEventImportances.Imp0,
                              0,
                              message,
                              Array.Empty<byte>(), // TODO проверить работу без пользователя
                              tmAddr)
     .ConfigureAwait(false);
  }


  public async Task AddTmaRelatedStringToEventLog(string             message,
                                                  TmAddr             tmAddr,
                                                  TmEventImportances importances = TmEventImportances.Imp0,
                                                  DateTime?          time        = null)
  {
    var binStr = $"pt={tmAddr.Point};t={(uint)tmAddr.Type.ToNativeType()}";

    await AddStringToEventLogEx(time, importances, TmEventLogExtendedSources.TmaRelated, message, binStr)
     .ConfigureAwait(false);
  }


  public async Task AddStringToEventLogEx(DateTime?                 time,
                                          TmEventImportances        importances,
                                          TmEventLogExtendedSources source,
                                          string                    message,
                                          string                    binaryString = "",
                                          TmAddr                    tmAddr       = null)
  {
    await Task.Run(() => TmNativeApi.AddStringToEventLogEx(_cid,
                                                           time,
                                                           importances.ToEventLogImportanceByte(),
                                                           (uint)source,
                                                           message,
                                                           tmAddr?.ToInteger() ?? 0,
                                                           binaryString))
              .ConfigureAwait(false);
  }


  public async Task AddStrBinToEventLog(DateTime?                 time,
                                        TmEventImportances        importances,
                                        TmEventLogExtendedSources source,
                                        string                    message,
                                        byte[]                    binary = null,
                                        TmAddr                    tmAddr = null)
  {
    var (binData, size) = binary is null
                            ? (Array.Empty<byte>(), 0)
                            : (binary, (uint)binary.Length);

    await Task.Run(() => TmNativeApi.AddStrBinToEventLog(_cid,
                                                         time,
                                                         importances.ToEventLogImportanceByte(),
                                                         (uint)source,
                                                         message,
                                                         binData,
                                                         size,
                                                         tmAddr?.ToInteger() ?? 0))
              .ConfigureAwait(false);
  }


  public async Task<bool> BlockTagEventsTemporarily(TmTag tmTag, int minutesToBlock)
  {
    return await BlockTagEventsTemporarily(tmTag, DateTime.Now.AddMinutes(minutesToBlock)).ConfigureAwait(false);
  }


  public async Task<bool> BlockTagEventsTemporarily(TmTag tmTag, DateTime endBlockTime)
  {
    if (tmTag == null)
    {
      return false;
    }

    var (ch, rtu, point) = tmTag.TmAddr.GetTupleShort();
    var propsBytes = TmNativeUtil.GetDoubleNullTerminatedBytesFromStringList(new[]
    {
      $"EvUnblkTime={endBlockTime:yyyy.MM.dd HH:mm:00}"
    });
    var propsChanged = 0u;
    var result = await Task.Run(() => TmNative.tmcSetObjectProperties(_cid,
                                                                      tmTag.NativeType,
                                                                      ch,
                                                                      rtu,
                                                                      point,
                                                                      propsBytes,
                                                                      out propsChanged))
                           .ConfigureAwait(false);

    return result > 0 && propsChanged > 0;
  }


  public async Task UnblockTagEvents(TmTag tmTag)
  {
    if (tmTag == null)
    {
      return;
    }

    var (ch, rtu, point) = tmTag.TmAddr.GetTupleShort();
    var propsBytes = TmNativeUtil.GetDoubleNullTerminatedBytesFromStringList(new[]
    {
      "EvUnblkTime="
    });
    await Task.Run(() => TmNative.tmcSetObjectProperties(_cid,
                                                         tmTag.NativeType,
                                                         ch,
                                                         rtu,
                                                         point,
                                                         propsBytes,
                                                         out _))
              .ConfigureAwait(false);
  }
}