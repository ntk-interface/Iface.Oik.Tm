using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static List<T> GetEventsArchive<T>(int cid, TmNativeEventFilter filter)
    where T : TmEventBase, new()
  {
    var startTime = TmNative.uxgmtime2uxtime(filter.StartTime);
    var endTime   = TmNative.uxgmtime2uxtime(filter.EndTime);

    var criteria = new TmNativeDefs.TEventExCriteria
    {
      ItemsLimit = filter.OutputLimit,
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
      return []
      ;
    }

    var curPtr = tEventPtr;
    var i      = 0;

    var events = new List<T>();

    var tmTagCache = new Dictionary<int, TagPropsAndClassData>();

    while (curPtr != nint.Zero)
    {
      (var evnt, curPtr) = CreateEvent<T>(curPtr, i, cid, tmTagCache);
      events.Add(evnt);
      i++;
    }

    TmNative.tmcFreeMemory(tEventPtr);

    return events;
  }

  internal static unsafe (T evnt, nint next) CreateEvent<T>(nint                                  pTEventEx,
                                                            int                                   addDataIndex,
                                                            int                                   cid,
                                                            Dictionary<int, TagPropsAndClassData> cache)
    where T : TmEventBase, new()
  {
    var basePtr = (byte*)pTEventEx;

    var eventHeaderSize = sizeof(TmNativeDefsUnsafe.TEventHeader);
    var header          = *(TmNativeDefsUnsafe.TEventExHeader*)basePtr;

    var tEventOffset = sizeof(TmNativeDefsUnsafe.TEventExHeader);
    var eventHeader  = *(TmNativeDefsUnsafe.TEventHeader*)(basePtr + tEventOffset);

    var dataOffset = tEventOffset + eventHeaderSize;

    var addData = GetEventAddData(addDataIndex);


    T evnt;
    switch ((TmNativeDefs.EventTypes)eventHeader.Id)
    {
      case TmNativeDefs.EventTypes.StatusChange:
      {
        var classDataAndProps = GetAndCacheUpdatedEventTagData(cid,
                                                               TmNativeDefs.TmDataTypes.Status,
                                                               (short)eventHeader.Ch,
                                                               (short)eventHeader.Rtu,
                                                               (short)eventHeader.Point,
                                                               cache);

        if (header.EventSize >= TmNativeDefs.ExtendedStatusChangedEventSize)
        {
          var (statusData, operatorName) = GetStatusDataExFromBytes(basePtr + dataOffset, cid);
          evnt = TmEventBase.CreateStatusChangeExtendedEvent<T>(eventHeader,
                                                                statusData,
                                                                operatorName,
                                                                addData,
                                                                classDataAndProps);
        }
        else
        {
          evnt = TmEventBase.CreateStatusChangeEvent<T>(eventHeader,
                                                        GetStatusDataFromBytes(basePtr + dataOffset),
                                                        addData,
                                                        classDataAndProps);
        }

        break;
      }
      case TmNativeDefs.EventTypes.Alarm:
      {
        var alarmData = GetAlarmDataFromBytes(basePtr + dataOffset);
        var alarmTypeName = GetExtendedObjectName(cid,
                                                  (short)eventHeader.Ch,
                                                  (short)eventHeader.Rtu,
                                                  (short)eventHeader.Point,
                                                  (short)alarmData.AlarmID,
                                                  TmNativeDefs.TmDataTypes.AnalogAlarm);

        var classDataAndProps = GetAndCacheUpdatedEventTagData(cid,
                                                               TmNativeDefs.TmDataTypes.Analog,
                                                               (short)eventHeader.Ch,
                                                               (short)eventHeader.Rtu,
                                                               (short)eventHeader.Point,
                                                               cache);

        evnt = TmEventBase.CreateAlarmEvent<T>(eventHeader,
                                               alarmData,
                                               addData,
                                               alarmTypeName,
                                               classDataAndProps);
        break;
      }
      case TmNativeDefs.EventTypes.Control:
      {
        var (controlData, operatorName) = GetControlDataFromBytes(basePtr + dataOffset, cid);
        var classDataAndProps = GetAndCacheUpdatedEventTagData(cid,
                                                               TmNativeDefs.TmDataTypes.Status,
                                                               (short)eventHeader.Ch,
                                                               (short)eventHeader.Rtu,
                                                               (short)eventHeader.Point,
                                                               cache);
        evnt = TmEventBase.CreateControlEvent<T>(eventHeader,
                                                 controlData,
                                                 addData,
                                                 operatorName,
                                                 classDataAndProps);

        break;
      }
      case TmNativeDefs.EventTypes.Acknowledge:
      {
        var (ackData, operatorName) = GetAcknowledgeDataFromBytes(basePtr + dataOffset, cid);

        var propsAndClassData = new TagPropsAndClassData
        {
          Name = eventHeader.Point == 0
                   ? string.Empty
                   : GetObjectName(cid,
                                   (short)eventHeader.Ch,
                                   (short)eventHeader.Rtu,
                                   (short)eventHeader.Point,
                                   ackData.TmType)
        };
        
        evnt = TmEventBase.CreateAcknowledgeEventDto<T>(eventHeader,
                                                        ackData,
                                                        addData,
                                                        operatorName,
                                                        propsAndClassData);
        break;
      }
      default:
        evnt = new T();
        break;
    }

    return (evnt, header.Next);
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
        UserName = TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(native.UserName)
      };
    }
  }

  internal static unsafe TmNativeDefsUnsafe.StatusData GetStatusDataFromBytes(byte* ptr)
  {
    return TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.StatusData>(ptr,
                                                                    sizeof(TmNativeDefsUnsafe.StatusDataEx));
  }

  internal static unsafe (TmNativeDefsUnsafe.StatusDataEx, string userRef) GetStatusDataExFromBytes(byte* ptr, int cid)
  {
    var native =
      TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.StatusDataEx>(ptr, sizeof(TmNativeDefsUnsafe.StatusDataEx));
    var userName = GetTextByRef(native.UserName, cid);

    return (native, userName);
  }

  internal static unsafe TmNativeDefsUnsafe.AlarmData GetAlarmDataFromBytes(byte* ptr)
  {
    return TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.AlarmData>(ptr,
                                                                   sizeof(TmNativeDefsUnsafe.AlarmData));
  }

  internal static unsafe (TmNativeDefsUnsafe.ControlData data, string operatorName) GetControlDataFromBytes(
    byte* ptr, int cid)
  {
    var native = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.ControlData>(ptr,
                                                                           sizeof(TmNativeDefsUnsafe.ControlData));
    var operatorName = GetTextByRef(native.UserName, cid);
    return (native, operatorName);
  }

  internal static unsafe (TmNativeDefsUnsafe.AcknowledgeData data, string operatorName) GetAcknowledgeDataFromBytes(
    byte* ptr, int cid)
  {
    var native = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.AcknowledgeData>(ptr,
                                                                               sizeof(
                                                                                 TmNativeDefsUnsafe.AcknowledgeData));

    var operatorName = GetTextByRef(native.UserName, cid);

    return (native, operatorName);
  }

  internal static unsafe string GetTextByRef(byte* ptr, int cid)
  {
    const int bufSize = 128;

    switch (ptr[0])
    {
      case 0:
        return string.Empty;
      case 0x40:
      {
        Span<byte> buf = stackalloc byte[bufSize];
        TmNative.tmcGetTextByRef(cid, (nint)ptr, buf, bufSize);

        return TmNativeUtil.BytesToString(buf);
      }
      default:
        return TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(ptr);
    }
  }

  private static TagPropsAndClassData GetAndCacheUpdatedEventTagData(int                                   cid,
                                                                     TmNativeDefs.TmDataTypes              type,
                                                                     short                                 ch,
                                                                     short                                 rtu,
                                                                     short                                 point,
                                                                     Dictionary<int, TagPropsAndClassData> cache)
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
        cachedData = new TagPropsAndClassData
        {
          Name = string.IsNullOrEmpty(tagName) ? $"#ТТ{ch}:{rtu}:{point}" : tagName
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

  private static string GetExtendedObjectName(int                      cid,
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

  private static string GetObjectName(int    cid,
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
                                (ushort)tmDataType,
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
}