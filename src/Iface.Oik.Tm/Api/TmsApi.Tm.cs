using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api;

public partial class TmsApi
{
  public async Task<int> GetStatus(int ch, int rtu, int point)
  {
    return await Task.Run(() => TmNative.tmcStatus(_cid, (short)ch, (short)rtu, (short)point))
                     .ConfigureAwait(false);
  }


  public async Task<float> GetAnalog(int ch, int rtu, int point)
  {
    return await Task.Run(() => TmNativeApi.GetAnalog(_cid, ch, rtu, point, null, 0))
                     .ConfigureAwait(false);
  }


  public async Task<float> GetAccum(int ch, int rtu, int point)
  {
    return await Task.Run(() => TmNativeApi.GetAccumValue(_cid, ch, rtu, point))
                     .ConfigureAwait(false);
  }


  public async Task<float> GetAccumLoad(int ch, int rtu, int point)
  {
    return await Task.Run(() => TmNativeApi.GetAccumLoad(_cid, ch, rtu, point))
                     .ConfigureAwait(false);
  }


  public async Task UpdateTag(TmTag tag)
  {
    switch (tag)
    {
      case TmAccum accum:
        await UpdateAccum(accum).ConfigureAwait(false);
        break;
      case TmAnalog analog:
        await UpdateAnalog(analog).ConfigureAwait(false);
        break;
      case TmStatus status:
        await UpdateStatus(status).ConfigureAwait(false);
        break;
    }
  }


  public async Task UpdateStatus(TmStatus status)
  {
    await UpdateStatuses(new List<TmStatus> { status }).ConfigureAwait(false);
  }


  public async Task UpdateStatusExplicitly(TmStatus status, bool getRealTelemetry)
  {
    await UpdateStatusesExplicitly(new List<TmStatus> { status }, getRealTelemetry).ConfigureAwait(false);
  }


  public async Task UpdateStatuses(IReadOnlyList<TmStatus> tmStatuses)
  {
    await Task.Run(() => UpdateStatusesSynchronously(tmStatuses)).ConfigureAwait(false);
  }


  public async Task UpdateStatuses(IReadOnlyDictionary<int, TmStatus> tmStatuses)
  {
    await Task.Run(() => UpdateStatuses(tmStatuses.Values.ToList())).ConfigureAwait(false);
  }


  public async Task UpdateStatusesExplicitly(IReadOnlyList<TmStatus> statuses, bool getRealTelemetry = false)
  {
    if (statuses.IsNullOrEmpty()) return;

    var count            = statuses.Count;
    var tmcAddrList      = new TmNativeDefs.TAdrTm[count];
    var statusPointsList = new TmNativeDefs.TStatusPoint[count];

    for (var i = 0; i < count; i++)
    {
      tmcAddrList[i] = statuses[i].TmAddr.ToAdrTm();
      if (getRealTelemetry)
      {
        tmcAddrList[i].Ch += TmNativeDefs.RealTelemetryFlag;
      }
    }

    await Task.Run(() => TmNative.tmcStatusByList(_cid, (ushort)count, tmcAddrList, statusPointsList))
              .ConfigureAwait(false);

    for (var i = 0; i < count; i++)
    {
      statuses[i].UpdateValueFromTStatusPoint(statusPointsList[i]);
    }
  }


  private void UpdateStatusSynchronously(TmStatus status)
  {
    UpdateStatusesSynchronously(new List<TmStatus> { status });
  }


  private void UpdateStatusesSynchronously(IReadOnlyList<TmStatus> statuses)
  {
    if (statuses.IsNullOrEmpty()) return;

    var tmcAddrList = new TmNativeDefs.TAdrTm[statuses.Count];
    for (var i = 0; i < statuses.Count; i++)
    {
      tmcAddrList[i] = statuses[i].TmAddr.ToAdrTm();
    }

    var commonPoints = TmNativeApi.GetTmValuesByListEx(_cid,
                                                       TmNativeDefs.TmDataTypes.Status,
                                                       tmcAddrList);
    for (var i = 0; i < statuses.Count; i++)
    {
      statuses[i].UpdateValueFromCommonPointDto(commonPoints[i]);
    }
  }


  public async Task UpdateAnalog(TmAnalog analog)
  {
    await UpdateAnalogs(new List<TmAnalog> { analog }).ConfigureAwait(false);
  }


  public async Task UpdateAnalogExplicitly(TmAnalog analog, uint time, ushort retroNum, bool getRealTelemetry)
  {
    await UpdateAnalogsExplicitly(new List<TmAnalog> { analog }, time, retroNum, getRealTelemetry)
     .ConfigureAwait(false);
  }


  private void UpdateAnalogSynchronously(TmAnalog analog)
  {
    UpdateAnalogsSynchronously(new List<TmAnalog> { analog });
  }


  private void UpdateAnalogsSynchronously(IReadOnlyList<TmAnalog> analogs)
  {
    if (analogs.IsNullOrEmpty()) return;

    var tmcAddrList = new TmNativeDefs.TAdrTm[analogs.Count];
    for (var i = 0; i < analogs.Count; i++)
    {
      tmcAddrList[i] = analogs[i].TmAddr.ToAdrTm();
    }

    var commonPoints = TmNativeApi.GetTmValuesByListEx(_cid,
                                                       TmNativeDefs.TmDataTypes.Analog,
                                                       tmcAddrList);
    for (var i = 0; i < analogs.Count; i++)
    {
      analogs[i].UpdateValueFromCommonPointDto(commonPoints[i]);
    }
  }


  public async Task UpdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs)
  {
    await Task.Run(() => UpdateAnalogsSynchronously(tmAnalogs)).ConfigureAwait(false);
  }


  public async Task UpdateAnalogs(IReadOnlyDictionary<int, TmAnalog> tmAnalogs)
  {
    await Task.Run(() => UpdateAnalogs(tmAnalogs.Values.ToList())).ConfigureAwait(false);
  }


  public async Task UpdateAnalogsExplicitly(IReadOnlyList<TmAnalog> analogs,
                                            uint                    time,
                                            ushort                  retroNum,
                                            bool                    getRealTelemetry = false)
  {
    if (analogs.IsNullOrEmpty()) return;

    var count            = analogs.Count;
    var tmcAddrList      = new TmNativeDefs.TAdrTm[count];
    var analogPointsList = new TmNativeDefs.TAnalogPoint[count];

    for (var i = 0; i < count; i++)
    {
      tmcAddrList[i] = analogs[i].TmAddr.ToAdrTm();
      if (getRealTelemetry)
      {
        tmcAddrList[i].Ch += TmNativeDefs.RealTelemetryFlag;
      }
    }

    await Task.Run(() => TmNative.tmcAnalogByList(_cid, (ushort)count, tmcAddrList, analogPointsList, time, retroNum))
              .ConfigureAwait(false);

    for (var i = 0; i < count; i++)
    {
      analogs[i].UpdateValueFromTAnalogPoint(analogPointsList[i]);
    }
  }


  public async Task UpdateAccumExplicitly(TmAccum accum, uint time, bool getRealTelemetry)
  {
    await UpdateAccumsExplicitly(new List<TmAccum> { accum }, time, getRealTelemetry)
     .ConfigureAwait(false);
  }


  public async Task UpdateAccum(TmAccum accum)
  {
    await UpdateAccums(new List<TmAccum> { accum }).ConfigureAwait(false);
  }


  public async Task UpdateAccums(IReadOnlyList<TmAccum> tmAccums)
  {
    await Task.Run(() => UpdateAccumsSynchronously(tmAccums)).ConfigureAwait(false);
  }


  public async Task UpdateAccums(IReadOnlyDictionary<int, TmAccum> tmAccums)
  {
    await Task.Run(() => UpdateAccums(tmAccums.Values.ToList())).ConfigureAwait(false);
  }


  public async Task UpdateAccumsExplicitly(IReadOnlyList<TmAccum> accums,
                                           uint                   time,
                                           bool                   getRealTelemetry = false)
  {
    if (accums.IsNullOrEmpty()) return;

    var count           = accums.Count;
    var tmcAddrList     = new TmNativeDefs.TAdrTm[count];
    var accumPointsList = new TmNativeDefs.TAccumPoint[count];

    for (var i = 0; i < count; i++)
    {
      tmcAddrList[i] = accums[i].TmAddr.ToAdrTm();
      if (getRealTelemetry)
      {
        tmcAddrList[i].Ch += TmNativeDefs.RealTelemetryFlag;
      }
    }

    await Task.Run(() => TmNative.tmcAccumByList(_cid, (ushort)count, tmcAddrList, accumPointsList, time))
              .ConfigureAwait(false);

    for (var i = 0; i < count; i++)
    {
      accums[i].UpdateValueFromTAccumPoint(accumPointsList[i]);
    }
  }


  private void UpdateAccumSynchronously(TmAccum accum)
  {
    UpdateAccumsSynchronously(new List<TmAccum> { accum });
  }


  private void UpdateAccumsSynchronously(IReadOnlyList<TmAccum> accums)
  {
    if (accums.IsNullOrEmpty()) return;

    var tmcAddrList = new TmNativeDefs.TAdrTm[accums.Count];
    for (var i = 0; i < accums.Count; i++)
    {
      tmcAddrList[i] = accums[i].TmAddr.ToAdrTm();
    }

    var commonPoints = TmNativeApi.GetTmValuesByListEx(_cid,
                                                       TmNativeDefs.TmDataTypes.Accum,
                                                       tmcAddrList);
    for (var i = 0; i < accums.Count; i++)
    {
      accums[i].UpdateValueFromCommonPointDto(commonPoints[i]);
    }
  }


  public async Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tags)
  {
    if (tags.IsNullOrEmpty()) return;

    var taskProperties = Task.Run(() =>
    {
      foreach (var tag in tags)
      {
        UpdateTagPropertiesSynchronously(tag);
      }
    });

    var taskClassData = Task.Run(() => // TODO попробовать через один запрос завернуть, возможны проблемы с маршалом
    {
      foreach (var tag in tags)
      {
        UpdateTagClassDataSynchronously(tag);
      }
    });

    var taskAnalogTechParameters = Task.Run(() =>
    {
      foreach (var tag in tags)
      {
        UpdateAnalogTechParametersSynchronously(tag);
      }
    });

    await Task.WhenAll(taskProperties, taskClassData, taskAnalogTechParameters)
              .ConfigureAwait(false);
  }


  public async Task UpdateStatusesPropertiesAndClassData(IReadOnlyDictionary<int, TmStatus> statuses)
  {
    await UpdateTagsPropertiesAndClassData(statuses.Values.ToList()).ConfigureAwait(false);
  }


  public async Task UpdateAnalogsPropertiesAndClassData(IReadOnlyDictionary<int, TmAnalog> analogs)
  {
    await UpdateTagsPropertiesAndClassData(analogs.Values.ToList()).ConfigureAwait(false);
  }


  public async Task UpdateAccumsPropertiesAndClassData(IReadOnlyDictionary<int, TmAccum> accums)
  {
    await UpdateTagsPropertiesAndClassData(accums.Values.ToList()).ConfigureAwait(false);
  }


  public async Task UpdateTagPropertiesAndClassData(TmTag tag)
  {
    var taskProperties           = UpdateTagProperties(tag);
    var taskClassData            = UpdateTagClassData(tag);
    var taskAnalogTechParameters = UpdateAnalogTechParameters(tag);

    await Task.WhenAll(taskProperties, taskClassData, taskAnalogTechParameters)
              .ConfigureAwait(false);
  }


  private void UpdateTagPropertiesAndClassDataSynchronously(TmTag tag)
  {
    UpdateTagPropertiesSynchronously(tag);
    UpdateTagClassDataSynchronously(tag);
  }


  public async Task UpdateTagProperties(TmTag tag)
  {
    await Task.Run(() => UpdateTagPropertiesSynchronously(tag)).ConfigureAwait(false);
  }


  private void UpdateTagPropertiesSynchronously(TmTag tag)
  {
    Span<byte> sb = stackalloc byte[1024];
    var (ch, rtu, point) = tag.TmAddr.GetTupleShort();
    TmNative.tmcGetObjectProperties(_cid,
                                    tag.NativeType,
                                    ch,
                                    rtu,
                                    point,
                                    sb,
                                    1024);
    tag.UpdatePropertiesFromTmcObject(EncodingUtil.BytesToString(sb));
  }


  private async Task UpdateTagClassData(TmTag tag)
  {
    await Task.Run(() => UpdateTagClassDataSynchronously(tag)).ConfigureAwait(false);
  }


  private void UpdateTagClassDataSynchronously(TmTag tag)
  {
    var    tmcAddr = tag.TmAddr.ToAdrTm();
    IntPtr classDataPtr;

    switch (tag.Type)
    {
      case TmType.Status:
        classDataPtr = TmNative.tmcGetStatusClassData(_cid, 1, new[] { tmcAddr });
        break;
      case TmType.Analog:
        classDataPtr = TmNative.tmcGetAnalogClassData(_cid, 1, new[] { tmcAddr });
        break;
      default:
        return;
    }

    if (classDataPtr == IntPtr.Zero)
    {
      return;
    }

    var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr); // у нас массив строк, а не просто строка
    var str                = TmNativeUtil.GetCStringFromIntPtr(singleClassDataPtr);
    TmNative.tmcFreeMemory(classDataPtr);

    tag.UpdateClassDataFromTmcClassData(str);
  }


  public async Task UpdateTagsClassDataExplicitly(IReadOnlyList<TmTag> tags)
  {
    var analogs  = new List<TmAnalog>();
    var statuses = new List<TmStatus>();
    foreach (var tag in tags)
    {
      switch (tag)
      {
        case TmAnalog analog:
          analogs.Add(analog);
          break;
        case TmStatus status:
          statuses.Add(status);
          break;
        default:
          continue;
      }
    }

    var analogsTask = UpdateAnalogsClassDataExplicitly(analogs);
    var statusTask  = UpdateStatusesClassDataExplicitly(statuses);

    await Task.WhenAll(analogsTask, statusTask).ConfigureAwait(false);
  }


  public async Task UpdateAnalogsClassDataExplicitly(IReadOnlyList<TmAnalog> tmAnalogs)
  {
    await Task.Run(() => UpdateAnalogsClassDataExplicitlySynchronously(tmAnalogs)).ConfigureAwait(false);
  }


  private void UpdateAnalogsClassDataExplicitlySynchronously(IReadOnlyList<TmAnalog> tmAnalogs)
  {
    var source = tmAnalogs;

    while (source.Any())
    {
      var chunk = source.Take(128).ToList();
      var classDataPtr =
        TmNative.tmcGetAnalogClassData(_cid, (uint)chunk.Count, chunk.Select(x => x.TmAddr.ToAdrTm()).ToArray());

      if (classDataPtr == IntPtr.Zero)
      {
        return;
      }

      var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr);

      foreach (var analog in chunk)
      {
        var str = TmNativeUtil.GetCStringFromIntPtr(singleClassDataPtr);

        analog.UpdateClassDataFromTmcClassData(str);

        if (str == string.Empty)
        {
          singleClassDataPtr = IntPtr.Add(singleClassDataPtr, 1);
          continue;
        }

        singleClassDataPtr = IntPtr.Add(singleClassDataPtr, str.Length + 1);
      }

      source = source.Skip(128).ToList();
      TmNative.tmcFreeMemory(classDataPtr);
    }
  }


  public async Task UpdateStatusesClassDataExplicitly(IReadOnlyList<TmStatus> tmStatus)
  {
    await Task.Run(() => UpdateStatusesClassDataExplicitlySynchronously(tmStatus)).ConfigureAwait(false);
  }


  private void UpdateStatusesClassDataExplicitlySynchronously(IReadOnlyList<TmStatus> tmStatus)
  {
    var source = tmStatus;

    while (source.Any())
    {
      var chunk = source.Take(128).ToList();
      var classDataPtr =
        TmNative.tmcGetStatusClassData(_cid, (uint)chunk.Count, chunk.Select(x => x.TmAddr.ToAdrTm()).ToArray());

      if (classDataPtr == IntPtr.Zero)
      {
        return;
      }


      var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr);


      foreach (var status in chunk)
      {
        var str = TmNativeUtil.GetCStringFromIntPtr(singleClassDataPtr);

        status.UpdateClassDataFromTmcClassData(str);

        if (str == string.Empty)
        {
          singleClassDataPtr = IntPtr.Add(singleClassDataPtr, 1);
          continue;
        }

        singleClassDataPtr = IntPtr.Add(singleClassDataPtr, str.Length + 1);
      }

      source = source.Skip(128).ToArray();

      TmNative.tmcFreeMemory(classDataPtr);
    }
  }
}