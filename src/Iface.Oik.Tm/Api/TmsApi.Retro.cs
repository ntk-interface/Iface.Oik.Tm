using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api;

public partial class TmsApi
{
  public async Task<int> GetStatusFromRetro(int ch, int rtu, int point, DateTime time)
  {
    return await Task.Run(() => TmNativeApi.GetStatusFromRetro(_cid, ch, rtu, point, time))
                     .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<TmStatusRetro>> GetStatusRetroEx(TmStatus            status,
                                                                         TmStatusRetroFilter filter,
                                                                         bool                getRealTelemetry = false)
  {
    var (ch, rtu, point) = status.TmAddr.GetTupleShort();

    var args = new GetRetroExArgs
    {
      TmCid             = _cid,
      Ch                = ch,
      Rtu               = rtu,
      Point             = point,
      StartTime         = filter.StartTime,
      EndTime           = filter.EndTime,
      Step              = filter.Step,
      UserRealTelemetry = getRealTelemetry
    };

    return await Task.Run(() => TmNativeApi.GetStatusRetroEx<TmStatusRetro>(args))
                     .ConfigureAwait(false);
  }


  public async Task<ITmAnalogRetro> GetAnalogFromRetro(int ch, int rtu, int point, DateTime time, int retroNum = 0)
  {
    var (isSuccess, dto) = await Task.Run(() => TmNativeApi.GetAnalogFromRetro(_cid, ch, rtu, point, time, 0))
                                     .ConfigureAwait(false);

    return isSuccess
             ? new TmAnalogRetro(dto.Value, dto.Flags, dto.Time)
             : TmAnalogRetro.UnreliableValue;
  }


  public async Task<ITmAccumRetro> GetAccumFromRetro(int ch, int rtu, int point, DateTime time)
  {
    var (isSuccess, dto) = await Task.Run(() => TmNativeApi.GetAccumFromRetro(_cid, ch, rtu, point, time))
                                     .ConfigureAwait(false);

    return isSuccess
             ? new TmAccumRetro(dto.Value, dto.Load, dto.Flags, dto.Time)
             : TmAccumRetro.UnreliableValue;
  }


  public async Task UpdateStatusesFromRetro(IReadOnlyList<TmStatus> statuses, DateTime time)
  {
    if (statuses.IsNullOrEmpty()) return;

    var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
    var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);

    var count            = statuses.Count;
    var tmcAddrList      = new TmNativeDefs.TAdrTm[count];
    var statusPointsList = new TmNativeDefs.TStatusPoint[count];

    for (var i = 0; i < count; i++)
    {
      tmcAddrList[i] = statuses[i].TmAddr.ToAdrTm();
    }

    await Task.Run(() => TmNative.tmcStatusByListEx(_cid,
                                                    (ushort)count,
                                                    tmcAddrList,
                                                    statusPointsList,
                                                    (uint)serverUtcTime))
              .ConfigureAwait(false);

    for (var i = 0; i < count; i++)
    {
      statuses[i].UpdateValueFromTStatusPoint(statusPointsList[i]);
      statuses[i].ChangeTime = time;
    }
  }


  public async Task UpdateAnalogsFromRetro(IReadOnlyList<TmAnalog> analogs,
                                           DateTime                time,
                                           int                     retroNum = 0)
  {
    if (analogs.IsNullOrEmpty()) return;

    var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
    var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);

    var count            = analogs.Count;
    var tmcAddrList      = new TmNativeDefs.TAdrTm[count];
    var analogPointsList = new TmNativeDefs.TAnalogPoint[count];

    for (var i = 0; i < count; i++)
    {
      tmcAddrList[i] = analogs[i].TmAddr.ToAdrTm();
    }

    await Task.Run(() => TmNative.tmcAnalogByList(_cid,
                                                  (ushort)count,
                                                  tmcAddrList,
                                                  analogPointsList,
                                                  (uint)serverUtcTime,
                                                  (ushort)retroNum))
              .ConfigureAwait(false);

    for (var i = 0; i < count; i++)
    {
      analogs[i].UpdateValueFromTAnalogPoint(analogPointsList[i]);
      analogs[i].ChangeTime = time;
    }
  }


  public async Task UpdateAccumsFromRetro(IReadOnlyList<TmAccum> accums,
                                          DateTime               time)
  {
    if (accums.IsNullOrEmpty()) return;

    var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
    var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);

    var count           = accums.Count;
    var tmcAddrList     = new TmNativeDefs.TAdrTm[count];
    var accumPointsList = new TmNativeDefs.TAccumPoint[count];

    for (var i = 0; i < count; i++)
    {
      tmcAddrList[i] = accums[i].TmAddr.ToAdrTm();
    }

    await Task.Run(() => TmNative.tmcAccumByList(_cid,
                                                 (ushort)count,
                                                 tmcAddrList,
                                                 accumPointsList,
                                                 (uint)serverUtcTime))
              .ConfigureAwait(false);

    for (var i = 0; i < count; i++)
    {
      accums[i].UpdateValueFromTAccumPoint(accumPointsList[i]);
      accums[i].ChangeTime = time;
    }
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro[]>> GetAnalogsMicroSeries(IReadOnlyList<TmAnalog> analogs)
  {
    if (analogs.IsNullOrEmpty())
    {
      return new[] { Array.Empty<ITmAnalogRetro>() };
    }

    var address = analogs.Select(x => x.TmAddr.ToAdrTm()).ToArray();

    return await Task.Run(() => TmNativeApi.GetAnalogMicroseries<TmAnalogMicroSeries>(_cid, address))
                     .ConfigureAwait(false);
  }
    
    
  public async Task<IReadOnlyCollection<TmRetroInfo>> GetRetrosInfo(TmType tmType)
  {
    var retrosInfo = new List<TmRetroInfo>();

    await Task.Run(() =>
               {
                 var itemsIndexes = new ushort[64];
                 var count = TmNative.tmcEnumObjects(_cid, (ushort)tmType.ToNativeType(), 64,
                                                     itemsIndexes, 0, 0, 0);

                 for (var i = 0; i < count; i++)
                 {
                   var info = new TmNativeDefs.TRetroInfoEx();
                   if (TmNative.tmcRetroInfoEx(_cid, itemsIndexes[i], ref info) == TmNativeDefs.Success)
                   {
                     retrosInfo.Add(TmRetroInfo.CreateFromTRetroInfoEx(info));
                   }
                 }
               })
              .ConfigureAwait(false);

    return retrosInfo;
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetro(TmAnalog analog,
                                                                        long     utcStartTime,
                                                                        int      count,
                                                                        int      step,
                                                                        int      retroNum = 0)
  {
    var result = new List<ITmAnalogRetro>();

    var (ch, rtu, point) = analog.TmAddr.GetTupleShort();
    var startTime = TmNative.uxgmtime2uxtime(utcStartTime);

    var tmcAnalogShortList = new TmNativeDefs.TAnalogPointShort[count];
    await Task.Run(() => TmNative.tmcTakeRetroTit(_cid,
                                                  ch, rtu, point,
                                                  (uint)startTime,
                                                  (ushort)retroNum,
                                                  (ushort)count,
                                                  (ushort)step,
                                                  tmcAnalogShortList))
              .ConfigureAwait(false);

    for (var i = 0; i < count; i++)
    {
      result.Add(new TmAnalogRetro(tmcAnalogShortList[i].Value,
                                   tmcAnalogShortList[i].Flags,
                                   startTime + i * step));
    }

    return result;
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetro(TmAnalog            analog,
                                                                        TmAnalogRetroFilter filter,
                                                                        int                 retroNum = 0)
  {
    var startTime = DateUtil.GetUtcTimestampFromDateTime(filter.StartTime);
    var endTime   = DateUtil.GetUtcTimestampFromDateTime(filter.EndTime);
    if (endTime <= startTime)
    {
      return null;
    }

    var step = filter.Step;

    var pointsCount = (int)((endTime - startTime) / step) + 1;

    return await GetAnalogRetro(analog, startTime, pointsCount, step, retroNum).ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetroEx(TmAnalog            analog,
                                                                          TmAnalogRetroFilter filter,
                                                                          int                 retroNum         = 0,
                                                                          bool                getRealTelemetry = false)
  {
    var (ch, rtu, point) = analog.TmAddr.GetTupleShort();

    var args = new GetRetroExArgs
    {
      TmCid             = _cid,
      Ch                = ch,
      Rtu               = rtu,
      Point             = point,
      StartTime         = filter.StartTime,
      EndTime           = filter.EndTime,
      Step              = filter.Step,
      RetroNum          = retroNum,
      UserRealTelemetry = getRealTelemetry
    };

    return await Task.Run(() => TmNativeApi.GetAnalogRetroEx<TmAnalogRetro>(args))
                     .ConfigureAwait(false);
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveInstant(
    TmAnalog            analog,
    TmAnalogRetroFilter filter)
  {
    var startTime = DateUtil.GetUtcTimestampFromDateTime(filter.StartTime);
    var endTime   = DateUtil.GetUtcTimestampFromDateTime(filter.EndTime);
    if (endTime <= startTime)
    {
      return null;
    }

    var points = await Task.Run(() => TmNativeApi.GetImpulseArchive(_cid,
                                                                    TmNativeDefs.ImpulseArchiveQueryFlags.Mom,
                                                                    (uint)analog.TmAddr.ToTma(),
                                                                    (uint)TmNative.uxgmtime2uxtime(startTime),
                                                                    (uint)TmNative.uxgmtime2uxtime(endTime),
                                                                    step: 1))
                           .ConfigureAwait(false);

    return points.Select(p => new TmAnalogImpulseArchiveInstant(p.Value,
                                                                p.Flags,
                                                                p.UnixTime,
                                                                p.Milliseconds) as ITmAnalogRetro)
                 .ToList();
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveSlices(
    TmAnalog            analog,
    TmAnalogRetroFilter filter)
  {
    var startTime = DateUtil.GetUtcTimestampFromDateTime(filter.StartTime);
    var endTime   = DateUtil.GetUtcTimestampFromDateTime(filter.EndTime);
    if (endTime <= startTime)
    {
      return null;
    }

    var points = await Task.Run(() => TmNativeApi.GetImpulseArchive(_cid,
                                                                    TmNativeDefs.ImpulseArchiveQueryFlags.Avg |
                                                                    TmNativeDefs.ImpulseArchiveQueryFlags.Min |
                                                                    TmNativeDefs.ImpulseArchiveQueryFlags.Max,
                                                                    (uint)analog.TmAddr.ToTma(),
                                                                    (uint)TmNative.uxgmtime2uxtime(startTime),
                                                                    (uint)TmNative.uxgmtime2uxtime(endTime),
                                                                    (uint)-filter.Step)) // минус тут важен!
                           .ConfigureAwait(false);

    return points.Select(p => new TmAnalogImpulseArchiveInstant(p.Value,
                                                                p.Flags,
                                                                p.UnixTime,
                                                                p.Milliseconds) as ITmAnalogRetro)
                 .ToList();
  }


  public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveAverage(
    TmAnalog            analog,
    TmAnalogRetroFilter filter)
  {
    var startTime = DateUtil.GetUtcTimestampFromDateTime(filter.StartTime);
    var endTime   = DateUtil.GetUtcTimestampFromDateTime(filter.EndTime);
    if (endTime <= startTime)
    {
      return null;
    }

    var points = await Task.Run(() => TmNativeApi.GetImpulseArchive(_cid,
                                                                    TmNativeDefs.ImpulseArchiveQueryFlags.Avg |
                                                                    TmNativeDefs.ImpulseArchiveQueryFlags.Min |
                                                                    TmNativeDefs.ImpulseArchiveQueryFlags.Max,
                                                                    (uint)analog.TmAddr.ToTma(),
                                                                    (uint)TmNative.uxgmtime2uxtime(startTime),
                                                                    (uint)TmNative.uxgmtime2uxtime(endTime),
                                                                    (uint)filter.Step))
                           .ConfigureAwait(false);

    // в списке точек три значения (макс, мин, среднее), которые объединяются в одно со всеми значениями
    var result = new List<ITmAnalogRetro>(points.Count / 3);
    for (var i = 0; i < points.Count; i += 3)
    {
      var avgIndex = FindIndexByType(i, TmNativeDefs.ImpulseArchiveFlags.Avg);
      var minIndex = FindIndexByType(i, TmNativeDefs.ImpulseArchiveFlags.Min);
      var maxIndex = FindIndexByType(i, TmNativeDefs.ImpulseArchiveFlags.Max);
    
      result.Add(new TmAnalogImpulseArchiveAverage(points[avgIndex].Value,
                                                   points[minIndex].Value,
                                                   points[maxIndex].Value,
                                                   points[avgIndex].Flags,
                                                   points[avgIndex].UnixTime + (uint)filter.Step, // прошлый период
                                                   points[avgIndex].Milliseconds));
    }

    return result;

    int FindIndexByType(int i, TmNativeDefs.ImpulseArchiveFlags type)
    {
      if (points[i + 1].Type == (byte)type)
      {
        return i + 1;
      }
      if (points[i + 2].Type == (byte)type)
      {
        return i + 2;
      }
      return i;
    }
  }
}