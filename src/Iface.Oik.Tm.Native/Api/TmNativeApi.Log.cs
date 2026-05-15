using System;
using System.Buffers;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  #region TmsEvensLog

  public static List<T> GetEventsArchive<T>(int cid, TmNativeEventFilter filter)
    where T : TmEventBase, new()
  {
    var startTime = TmNative.uxgmtime2uxtime(filter.StartTime);
    var endTime   = TmNative.uxgmtime2uxtime(filter.EndTime);

    var criteria = new TmNativeDefs.TEventExCriteria
    {
      ItemsLimit = (ushort)filter.OutputLimit,
      HStop      = nint.Zero,
      EvlArch    = true
    };

    var tEventPtr = TmNative.tmcEventLogEx(cid,
                                           filter.Types,
                                           (uint)startTime,
                                           (uint)endTime,
                                           criteria);

    if (tEventPtr == nint.Zero)
    {
      return new List<T>(0);
    }

    var curPtr = tEventPtr;
    var i      = 0;

    var events = new List<T>();

    var tmTagCache = new Dictionary<int, TagPropsAndClassData>();

    while (curPtr != nint.Zero)
    {
      var (exHeader, offset) = GetEventExHeaderAndOffset(curPtr);
      var header = GetGenericEventHeader(curPtr, offset);

      var evnt = CreateEvent<T>(curPtr + offset, header, i, cid, tmTagCache);

      events.Add(evnt);
      curPtr = exHeader.Next;

      i++;
    }

    TmNative.tmcFreeMemory(tEventPtr);

    return events;
  }


  public static List<T> GetEventsArchiveByElix<T>(int cid, TmNativeEventFilter filter)
    where T : TmEventBase, new()
  {
    var startTime = TmNative.uxgmtime2uxtime(filter.StartTime);
    var endTime   = TmNative.uxgmtime2uxtime(filter.EndTime);

    var events = new List<T>();
    var elix   = new TmNativeDefsUnsafe.TTMSElix();
    var cache  = new Dictionary<int, TagPropsAndClassData>();

    while (true)
    {
      var (eventsBatchList, lastElix) = GetEventsBatchByElix<T>(cid,
                                                                elix,
                                                                filter.Types,
                                                                startTime,
                                                                endTime,
                                                                cache,
                                                                filter.Importances);

      if (eventsBatchList.Count == 0)
      {
        break;
      }

      if (filter.OutputLimit > 0 &&
          events.Count       > filter.OutputLimit)
      {
        events.RemoveRange(filter.OutputLimit, events.Count - filter.OutputLimit);
        break;
      }

      elix = lastElix;
    }

    return events;
  }

  public static (List<T>, TmNativeDefs.TTMSElix) GetCurrentEvents<T>(int cid, ulong elixR, ulong elixM)
    where T : TmEventBase, new()
  {
    var currentElix = new TmNativeDefsUnsafe.TTMSElix
    {
      M = elixM,
      R = elixR
    };

    var cache  = new Dictionary<int, TagPropsAndClassData>();
    var events = new List<T>();

    while (true)
    {
      var (eventsBatchList, lastBatchElix) = GetEventsBatchByElix<T>(cid,
                                                                     currentElix,
                                                                     0xFFFF,
                                                                     0,
                                                                     0xFFFFFFFF,
                                                                     cache);
      if (eventsBatchList.Count == 0)
      {
        break;
      }

      events.AddRange(eventsBatchList);
      currentElix = lastBatchElix;
    }

    return (events, new TmNativeDefs.TTMSElix { R = currentElix.R, M = currentElix.M });
  }


  internal static (List<T>, TmNativeDefsUnsafe.TTMSElix)
    GetEventsBatchByElix<T>(int                                   cid,
                            TmNativeDefsUnsafe.TTMSElix           elix,
                            ushort                                type,
                            long                                  startTime,
                            long                                  endTime,
                            Dictionary<int, TagPropsAndClassData> cache,
                            TmNativeEventImportances?             filter = null)
    where T : TmEventBase, new()
  {
    var lastElix = elix;

    var tmcEventsElixPtr = TmNative.tmcEventLogByElix(cid,
                                                      ref lastElix,
                                                      type,
                                                      (uint)startTime,
                                                      (uint)endTime);

    if (tmcEventsElixPtr == nint.Zero)
    {
      return (new List<T>(0), lastElix);
    }

    var curPtr = tmcEventsElixPtr;
    var events = new List<T>();
    var i      = 0;

    while (curPtr != nint.Zero)
    {
      var (elixHeader, offset) = GetEventElixHeaderAndOffset(curPtr);
      var header = GetGenericEventHeader(curPtr, offset);


      switch (filter)
      {
        case null:
        case not null when filter.Value.HasFlag(TmEventBase.ImportanceToFlag(header.Id)):
          var evnt = CreateEvent<T>(curPtr + offset, header, i, cid, cache, elixHeader.Elix);
          events.Add(evnt);
          break;
      }

      curPtr = elixHeader.Next;
      i++;
    }

    TmNative.tmcFreeMemory(tmcEventsElixPtr);

    return (events, lastElix);
  }

  internal static unsafe T CreateEvent<T>(nint                                  pTEventEx,
                                          TmNativeDefsUnsafe.GenericEventHeader header,
                                          int                                   addDataIndex,
                                          int                                   cid,
                                          Dictionary<int, TagPropsAndClassData> cache,
                                          TmNativeDefsUnsafe.TTMSElix?          elix = null)
    where T : TmEventBase, new()
  {
    var basePtr    = (byte*)pTEventEx;
    var dataOffset = basePtr + sizeof(TmNativeDefsUnsafe.GenericEventHeader);

    var addData = GetEventAddData(addDataIndex);


    T evnt;
    switch ((TmNativeDefs.EventTypes)header.Id)
    {
      case TmNativeDefs.EventTypes.StatusChange:
      {
        var classDataAndProps = GetAndCacheUpdatedEventTagData(cid,
                                                               TmNativeDefs.TmDataTypes.Status,
                                                               (short)header.Ch,
                                                               (short)header.Rtu,
                                                               (short)header.Point,
                                                               cache);

        if (header.EventSize >= TmNativeDefs.ExtendedStatusChangedEventSize)
        {
          var (statusData, operatorName) = GetStatusDataExFromBytes(dataOffset, cid);
          evnt = TmEventBase.CreateStatusChangeExtendedEvent<T>(header,
                                                                statusData,
                                                                operatorName,
                                                                addData,
                                                                classDataAndProps,
                                                                elix);
        }
        else
        {
          evnt = TmEventBase.CreateStatusChangeEvent<T>(header,
                                                        GetStatusDataFromBytes(dataOffset),
                                                        addData,
                                                        classDataAndProps,
                                                        elix);
        }

        break;
      }
      case TmNativeDefs.EventTypes.Alarm:
      {
        var alarmData = GetAlarmDataFromBytes(dataOffset);
        var alarmTypeName = GetExtendedObjectName(cid,
                                                  (short)header.Ch,
                                                  (short)header.Rtu,
                                                  (short)header.Point,
                                                  (short)alarmData.AlarmID,
                                                  TmNativeDefs.TmDataTypes.AnalogAlarm);

        var classDataAndProps = GetAndCacheUpdatedEventTagData(cid,
                                                               TmNativeDefs.TmDataTypes.Analog,
                                                               (short)header.Ch,
                                                               (short)header.Rtu,
                                                               (short)header.Point,
                                                               cache);

        evnt = TmEventBase.CreateAlarmEvent<T>(header,
                                               alarmData,
                                               addData,
                                               alarmTypeName,
                                               classDataAndProps,
                                               elix);
        break;
      }
      case TmNativeDefs.EventTypes.Control:
      {
        var (controlData, operatorName) = GetControlDataFromBytes(dataOffset, cid);
        var classDataAndProps = GetAndCacheUpdatedEventTagData(cid,
                                                               TmNativeDefs.TmDataTypes.Status,
                                                               (short)header.Ch,
                                                               (short)header.Rtu,
                                                               (short)header.Point,
                                                               cache);
        evnt = TmEventBase.CreateControlEvent<T>(header,
                                                 controlData,
                                                 addData,
                                                 operatorName,
                                                 classDataAndProps,
                                                 elix);

        break;
      }
      case TmNativeDefs.EventTypes.Acknowledge:
      {
        var (ackData, operatorName) = GetAcknowledgeDataFromBytes(dataOffset, cid);

        var propsAndClassData = new TagPropsAndClassData
        {
          Name = header.Point == 0
                   ? string.Empty
                   : GetObjectName(cid,
                                   (short)header.Ch,
                                   (short)header.Rtu,
                                   (short)header.Point,
                                   ackData.TmType)
        };

        evnt = TmEventBase.CreateAcknowledgeEventDto<T>(header,
                                                        ackData,
                                                        addData,
                                                        operatorName,
                                                        propsAndClassData,
                                                        elix);
        break;
      }
      case TmNativeDefs.EventTypes.ManualStatusSet:
      {
        var (controlData, operatorName) = GetControlDataFromBytes(dataOffset, cid);
        var classDataAndProps = GetAndCacheUpdatedEventTagData(cid,
                                                               TmNativeDefs.TmDataTypes.Status,
                                                               (short)header.Ch,
                                                               (short)header.Rtu,
                                                               (short)header.Point,
                                                               cache);

        evnt = TmEventBase.CreateManualStatusSetEvent<T>(header,
                                                         controlData,
                                                         addData,
                                                         operatorName,
                                                         classDataAndProps,
                                                         elix);
        break;
      }
      case TmNativeDefs.EventTypes.ManualAnalogSet:
      {
        var (analogSetData, operatorName) = GetAnalogSetDataFromBytes(dataOffset, cid);

        var classDataAndProps = GetAndCacheUpdatedEventTagData(cid,
                                                               TmNativeDefs.TmDataTypes.Analog,
                                                               (short)header.Ch,
                                                               (short)header.Rtu,
                                                               (short)header.Point,
                                                               cache);

        evnt = TmEventBase.CreateManualAnalogSetEvent<T>(header,
                                                         analogSetData,
                                                         addData,
                                                         operatorName,
                                                         classDataAndProps,
                                                         elix);

        break;
      }
      case TmNativeDefs.EventTypes.Extended:
      {
        var strBinData = GetStrBinDataFromBytes(dataOffset);
        evnt = TmEventBase.CreateExtendedEvent<T>(header, strBinData, addData);

        break;
      }
      case TmNativeDefs.EventTypes.FlagsChange:
      {
        var flagsChangeData = GetFlagsChangeDataFromBytes(dataOffset);
        var sourceType      = (TmNativeDefs.TmDataTypes)flagsChangeData.TmType;

        switch (sourceType)
        {
          case TmNativeDefs.TmDataTypes.Status:
          {
            var (data, operatorName) = GetStatusFlagsChangeDataFromBytes(dataOffset, cid);

            var classDataAndProps = GetAndCacheUpdatedEventTagData(cid,
                                                                   sourceType,
                                                                   (short)header.Ch,
                                                                   (short)header.Rtu,
                                                                   (short)header.Point,
                                                                   cache);

            evnt = TmEventBase.CreateStatusFlagsChangeEvent<T>(header,
                                                               data,
                                                               addData,
                                                               operatorName,
                                                               classDataAndProps,
                                                               elix);
            break;
          }
          case TmNativeDefs.TmDataTypes.Analog:
          {
            var (data, operatorName) = GetAnalogFlagsChangeDataFromBytes(dataOffset, cid);

            var classDataAndProps = GetAndCacheUpdatedEventTagData(cid,
                                                                   sourceType,
                                                                   (short)header.Ch,
                                                                   (short)header.Rtu,
                                                                   (short)header.Point,
                                                                   cache);

            evnt = TmEventBase.CreateAnalogFlagsChangeEvent<T>(header,
                                                               data,
                                                               addData,
                                                               operatorName,
                                                               classDataAndProps,
                                                               elix);
            break;
          }
          case TmNativeDefs.TmDataTypes.Accum:
            var sourceAccumName = GetObjectName(cid,
                                                (short)header.Ch,
                                                (short)header.Rtu,
                                                (short)header.Point,
                                                (ushort)sourceType);

            evnt = TmEventBase.CreateAccumFlagsChangeEvent<T>(header,
                                                              flagsChangeData,
                                                              addData,
                                                              sourceAccumName,
                                                              elix);
            break;
          default:
          {
            throw new
              TmNativeException($"Неподдерживаемый ТМ-тип события {TmNativeDefs.EventTypes.FlagsChange} - {sourceType}");
          }
        }

        break;
      }
      default:
        evnt = TmEventBase.CreateUnknownEvent<T>(header, addData);
        break;
    }

    return evnt;
  }

  internal static unsafe (TmNativeDefsUnsafe.TEventExHeader, int) GetEventExHeaderAndOffset(nint pTEventEx)
  {
    return (TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.TEventExHeader>((byte*)pTEventEx),
            sizeof(TmNativeDefsUnsafe.TEventExHeader));
  }

  internal static unsafe (TmNativeDefsUnsafe.TEventElixHeader, int) GetEventElixHeaderAndOffset(nint pTEventEx)
  {
    return (TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.TEventElixHeader>((byte*)pTEventEx),
            sizeof(TmNativeDefsUnsafe.TEventElixHeader));
  }

  internal static unsafe TmNativeDefsUnsafe.GenericEventHeader GetGenericEventHeader(nint pTEventEx,
    int                                                                                   headerOffset)
  {
    return TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.GenericEventHeader>((byte*)pTEventEx + headerOffset);
  }

  internal static unsafe TmNativeDefs.TTMSEventAddData GetEventAddData(int index)
  {
    const int  bufSize = 512;
    Span<byte> buf     = stackalloc byte[bufSize];

    TmNative.tmcEventGetAdditionalRecData((uint)index, buf, bufSize);

    fixed (byte* basePtr = buf)
    {
      var native = *(TmNativeDefsUnsafe.TTMSEventAddData*)basePtr;

      return new TmNativeDefs.TTMSEventAddData
      {
        Elix = new TmNativeDefs.TTMSElix
        {
          M = native.Elix.M,
          R = native.Elix.R
        },
        AckMs    = native.AckMs,
        AckSec   = native.AckSec,
        UserName = TmNativeUtil.GetCStringFromBytePtr(native.UserName)
      };
    }
  }

  internal static unsafe TmNativeDefsUnsafe.StatusData GetStatusDataFromBytes(byte* ptr)
  {
    return TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.StatusData>(ptr);
  }

  internal static unsafe (TmNativeDefsUnsafe.StatusDataEx, string userRef) GetStatusDataExFromBytes(byte* ptr, int cid)
  {
    var native =
      TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.StatusDataEx>(ptr);
    var userName = GetTextByRef(native.UserName, cid);

    return (native, userName);
  }

  internal static unsafe TmNativeDefsUnsafe.AlarmData GetAlarmDataFromBytes(byte* ptr)
  {
    return TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.AlarmData>(ptr);
  }

  internal static unsafe (TmNativeDefsUnsafe.ControlData data, string operatorName) GetControlDataFromBytes(
    byte* ptr, int cid)
  {
    var native       = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.ControlData>(ptr);
    var operatorName = GetTextByRef(native.UserName, cid);
    return (native, operatorName);
  }

  internal static unsafe (TmNativeDefsUnsafe.AcknowledgeData data, string operatorName) GetAcknowledgeDataFromBytes(
    byte* ptr, int cid)
  {
    var native = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.AcknowledgeData>(ptr);

    var operatorName = GetTextByRef(native.UserName, cid);

    return (native, operatorName);
  }

  internal static unsafe (TmNativeDefsUnsafe.AnalogSetData data, string operatorName) GetAnalogSetDataFromBytes(
    byte* ptr, int cid)
  {
    var native = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.AnalogSetData>(ptr);

    var operatorName = GetTextByRef(native.UserName, cid);

    return (native, operatorName);
  }

  internal static unsafe TmNativeDefsUnsafe.StrBinData GetStrBinDataFromBytes(byte* ptr)
  {
    return TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.StrBinData>(ptr);
  }

  internal static unsafe TmNativeDefsUnsafe.FlagsChangeData GetFlagsChangeDataFromBytes(byte* ptr)
  {
    return TmNativeUtil
      .FromBytesPtr<TmNativeDefsUnsafe.FlagsChangeData>(ptr);
  }

  internal static unsafe (TmNativeDefsUnsafe.FlagsChangeDataStatus data, string operatorName)
    GetStatusFlagsChangeDataFromBytes(byte* ptr, int cid)
  {
    var native       = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.FlagsChangeDataStatus>(ptr);
    var operatorName = GetTextByRef(native.UserName, cid);

    return (native, operatorName);
  }

  internal static unsafe (TmNativeDefsUnsafe.FlagsChangeDataAnalog data, string operatorName)
    GetAnalogFlagsChangeDataFromBytes(byte* ptr, int cid)
  {
    var native       = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.FlagsChangeDataAnalog>(ptr);
    var operatorName = GetTextByRef(native.UserName, cid);

    return (native, operatorName);
  }

  internal static unsafe TagPropsAndClassData GetAndCacheUpdatedEventTagData(int                      cid,
                                                                             TmNativeDefs.TmDataTypes type,
                                                                             short                    ch,
                                                                             short                    rtu,
                                                                             short                    point,
                                                                             Dictionary<int, TagPropsAndClassData>
                                                                               cache)
  {
    var tmTagHash = (cid, type, ch, rtu, point).ToTuple().GetHashCode();

    if (cache.TryGetValue(tmTagHash, out var cachedData))
    {
      return cachedData;
    }

    var classDataStr = GetTagClassData(cid, type, ch, rtu, point);
    var tagName      = TmEventBase.GetTagName(GetTagProperties(cid, type, ch, rtu, point));

    switch (type)
    {
      case TmNativeDefs.TmDataTypes.Status:
      {
        cachedData      = TmEventBase.GetStatusClassData(classDataStr);
        cachedData.Name = string.IsNullOrEmpty(tagName) ? $"#TC{ch}:{rtu}:{point}" : tagName;
        break;
      }
      case TmNativeDefs.TmDataTypes.Analog:
      {
        var tAnalogPoint = GetTAnalogPoint(cid, ch, rtu, point);

        cachedData = new TagPropsAndClassData
        {
          Name      = string.IsNullOrEmpty(tagName) ? $"#ТТ{ch}:{rtu}:{point}" : tagName,
          Units     = GetTextByRef(tAnalogPoint.Unit, cid),
          Precision = (byte)(tAnalogPoint.Format >> 4),
        };

        break;
      }
      default:
      {
        cachedData = new TagPropsAndClassData();
        break;
      }
    }

    cache.Add(tmTagHash, cachedData);

    return cachedData;
  }

  internal static string GetExtendedObjectName(int                      cid,
                                               short                    ch,
                                               short                    rtu,
                                               short                    point,
                                               short                    subItemId,
                                               TmNativeDefs.TmDataTypes tmDataType)
  {
    const int bufSize = 1024;
    var       pool    = ArrayPool<byte>.Shared;
    var       buf     = pool.Rent(bufSize);

    try
    {
      TmNative.tmcGetObjectNameEx(cid,
                                  (ushort)tmDataType,
                                  ch,
                                  rtu,
                                  point,
                                  subItemId,
                                  buf,
                                  bufSize);
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(buf);
    }


    return TmNativeUtil.BytesToString(buf);
  }

  internal static string GetObjectName(int    cid,
                                       short  ch,
                                       short  rtu,
                                       short  point,
                                       ushort tmDataType)
  {
    const int bufSize = 1024;
    var       pool    = ArrayPool<byte>.Shared;
    var       buf     = pool.Rent(bufSize);

    try
    {
      TmNative.tmcGetObjectName(cid,
                                tmDataType,
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

  internal static unsafe IReadOnlyCollection<T> ParseTmServerLogRecordsList<T>(nint      ptr,
                                                                               DateTime? startTime,
                                                                               DateTime? endTime,
                                                                               int       limit)
    where T : TmServerLogRecordBase, new()
  {
    var result = new List<T>();

    if (ptr == nint.Zero)
    {
      return result;
    }

    var p      = (byte*)ptr;
    var length = 0;

    while (true)
    {
      if (result.Count >= limit)
      {
        break;
      }

      while (p[length] != 0)
      {
        length++;
      }

      var logRecord = TmServerLogRecordBase.Create<T>(new Span<byte>(p, length));

      if (endTime != null)
      {
        if (logRecord.DateTime > endTime)
        {
          continue;
        }
      }

      if (startTime != null)
      {
        if (logRecord.DateTime < startTime)
        {
          break;
        }
      }

      result.Add(logRecord);

      length++;

      if (p[length] == 0)
      {
        break;
      }

      p      += length;
      length =  0;
    }

    return result;
  }
  
  
  public static void AddStringToEventLogEx(int       tmCid,
                                           DateTime? time,
                                           byte      importance,
                                           uint      source,
                                           string    message,
                                           uint      tmAddrInteger,
                                           string    binaryString = "")
  {
    var pool        = ArrayPool<byte>.Shared;
    var buf         = pool.Rent(binaryString.Length + 1);
    var binDataSize = TmNativeUtil.StringToLpstrBytes(binaryString, buf);

    try
    {
      AddStrBinToEventLog(tmCid, time, importance, source, message, buf, binDataSize, tmAddrInteger);
    }
    finally
    {
      pool.Return(buf);
    }
  }

  public static void AddStrBinToEventLog(int        tmCid,
                                         DateTime?  time,
                                         byte       importance,
                                         uint       source,
                                         string     message,
                                         Span<byte> binaryPayload,
                                         uint       payloadSize,
                                         uint       tmAddrInteger)
  {
    if (importance > 3)
    {
      throw new TmNativeException("Важность не поддерживается");
    }

    var sourceLongTag = tmAddrInteger != 0
                          ? (tmAddrInteger + 0x0001_0001) & 0xFFFF_0000
                          : 0;

    sourceLongTag |= source;

    var unixTime = time == null
                     ? 0
                     : TmNative.uxgmtime2uxtime(NativeDateUtil.GetUtcTimestampFromDateTime(time.Value));

    var unixTimeMs = unixTime % 1000 / 10;

    TmNative.tmcEvlogPutStrBin(tmCid,
                               (uint)unixTime,
                               (byte)unixTimeMs,
                               importance,
                               sourceLongTag,
                               message,
                               binaryPayload,
                               payloadSize);
  }

  #endregion

  #region CfsLog

  public static IReadOnlyCollection<T> GetTmServersLog<T>(nint      cfCid,
                                                          DateTime? startTime,
                                                          DateTime? endTime,
                                                          int       limit)
    where T : TmServerLogRecordBase, new()
  {
    OpenTmServerLog(cfCid);

    try
    {
      return GetTmServersLogByBatches<T>(cfCid, startTime, endTime, limit);
    }
    catch (TmNotSupportedException)
    {
      return GetTmServersLogRecords<T>(cfCid, startTime, endTime, limit);
    }
    finally
    {
      CloseTmServerLog(cfCid);
    }
  }

  internal static void OpenTmServerLog(nint cfCid)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var result = TmNative.cfsLogOpen(cfCid,
                                       out var errCode,
                                       errBuf,
                                       TmNativeDefsUnsafe.ErrorBufSize);
      if (!result)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  internal static IReadOnlyCollection<T> GetTmServersLogByBatches<T>(nint      cfCid,
                                                                     DateTime? startTime,
                                                                     DateTime? endTime,
                                                                     int       limit)
    where T : TmServerLogRecordBase, new()
  {
    var isFirst = true;

    var result = new List<T>();

    while (result.Count < limit)
    {
      var records = GetTmServersLogBatch<T>(cfCid,
                                            startTime,
                                            endTime,
                                            limit - result.Count,
                                            isFirst);
      isFirst = false;

      if (records.Count == 0)
      {
        break;
      }

      result.AddRange(records);
    }

    return result;
  }

  internal static IReadOnlyCollection<T> GetTmServersLogBatch<T>(nint      cfCid,
                                                                 DateTime? startTime,
                                                                 DateTime? endTime,
                                                                 int       limit,
                                                                 bool      isFirst)
    where T : TmServerLogRecordBase, new()
  {
    const uint notSupported = 50;

    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var batchPtr = TmNative.cfsLogGetRecordEx(cfCid,
                                                isFirst,
                                                out var errCode,
                                                errBuf,
                                                TmNativeDefsUnsafe.ErrorBufSize);

      if (batchPtr != nint.Zero)
      {
        return ParseTmServerLogRecordsList<T>(batchPtr, startTime, endTime, limit);
      }

      switch (errCode)
      {
        case 0:
          break;
        case notSupported:
          throw new TmNotSupportedException();
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      var batch = ParseTmServerLogRecordsList<T>(batchPtr, startTime, endTime, limit);
      TmNative.cfsFreeMemory(batchPtr);

      return batch;
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  internal static IReadOnlyCollection<T> GetTmServersLogRecords<T>(nint      cfCid,
                                                                   DateTime? startTime,
                                                                   DateTime? endTime,
                                                                   int       limit)
    where T : TmServerLogRecordBase, new()
  {
    var records = new List<T>();
    var isFirst = true;

    while (true)
    {
      var record = GetTmServersLogRecord<T>(cfCid, isFirst);
      isFirst = false;

      if (record == null)
      {
        break;
      }

      if (endTime != null)
      {
        if (record.DateTime > endTime)
        {
          continue;
        }
      }

      if (startTime != null)
      {
        if (record.DateTime < startTime)
        {
          break;
        }
      }

      records.Add(record);

      if (limit > 0 && records.Count >= limit)
      {
        break;
      }
    }

    return records;
  }

  internal static T? GetTmServersLogRecord<T>(nint cfCid, bool isFirst)
    where T : TmServerLogRecordBase, new()
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var logRecordPtr = TmNative.cfsLogGetRecord(cfCid,
                                                  isFirst,
                                                  out var errCode,
                                                  errBuf,
                                                  TmNativeDefsUnsafe.ErrorBufSize);

      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      if (logRecordPtr == nint.Zero)
      {
        return null;
      }

      var record = TmServerLogRecordBase.Create<T>(logRecordPtr);
      TmNative.cfsFreeMemory(logRecordPtr);

      return record;
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  internal static void CloseTmServerLog(nint cfCid)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var result = TmNative.cfsLogClose(cfCid,
                                        out var errCode,
                                        errBuf,
                                        TmNativeDefsUnsafe.ErrorBufSize);
      if (!result)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  #endregion
}