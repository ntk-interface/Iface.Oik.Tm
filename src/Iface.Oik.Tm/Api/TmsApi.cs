using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api
{
  public class TmsApi : ITmsApi
  {
    private int _cid;

    private readonly ITmNative  _native;
    private          TmUserInfo _userInfo;


    public TmsApi(ITmNative native)
    {
      _native = native;
    }


    public void SetCidAndUserInfo(int cid, TmUserInfo userInfo)
    {
      _cid      = cid;
      _userInfo = userInfo;
    }


    public async Task<TmServerInfo> GetServerInfo()
    {
      var cfCid = await GetCfCid().ConfigureAwait(false);
      if (cfCid == 0)
      {
        Console.WriteLine("Ошибка при получении cfCid"); // todo
        return null;
      }

      const int errStringLenth = 1000;
      var cis = new TmNativeDefs.ComputerInfoS
      {
        Len = (uint) Marshal.SizeOf(typeof(TmNativeDefs.ComputerInfoS))
      };
      var  errString = new StringBuilder(errStringLenth);
      uint errCode   = 0;

      if (!await Task.Run(() => _native.CfsGetComputerInfo(cfCid,
                                                           ref cis,
                                                           out errCode,
                                                           ref errString,
                                                           errStringLenth))
                     .ConfigureAwait(false))
      {
        return null;
      }

      return new TmServerInfo(cis.ComputerName,
                              (int) cis.CfsVerMaj,
                              (int) cis.CfsVerMin,
                              (int) cis.NtVerMaj,
                              (int) cis.NtVerMin,
                              (int) cis.NtBuild,
                              (long) cis.Uptime);
    }


    public async Task<int> GetLastTmcError()
    {
      return (int) await Task.Run(() => _native.TmcGetLastError())
                             .ConfigureAwait(false);
    }


    public async Task<string> GetLastTmcErrorText()
    {
      var bufPtr = Marshal.AllocHGlobal(1024);
      await Task.Run(() => _native.TmcGetLastErrorText(_cid, bufPtr))
                .ConfigureAwait(false);

      var singleBufPtr = Marshal.PtrToStructure<IntPtr>(bufPtr); // массив строк, а не просто строка
      var str          = Marshal.PtrToStringAnsi(singleBufPtr);
      _native.TmcFreeMemory(singleBufPtr);

      return str;
    }


    public async Task<DateTime?> GetSystemTime()
    {
      return DateUtil.GetDateTimeFromTmString(await GetSystemTimeString().ConfigureAwait(false));
    }


    public async Task<string> GetSystemTimeString()
    {
      var tmcTime = new StringBuilder(80);
      await Task.Run(() => _native.TmcSystemTime(_cid, ref tmcTime, IntPtr.Zero))
                .ConfigureAwait(false);
      return tmcTime.ToString();
    }


    public async Task<uint> GetCfCid()
    {
      return await Task.Run(() => _native.TmcGetCfsHandle(_cid))
                       .ConfigureAwait(false);
    }


    public async Task<int> GetStatus(int ch, int rtu, int point)
    {
      return await Task.Run(() => _native.TmcStatus(_cid, (short) ch, (short) rtu, (short) point))
                       .ConfigureAwait(false);
    }


    public async Task<float> GetAnalog(int ch, int rtu, int point)
    {
      return await Task.Run(() => _native.TmcAnalog(_cid, (short) ch, (short) rtu, (short) point, null, 0))
                       .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr addr,
                                                                         long   utcStartTime,
                                                                         int    count,
                                                                         int    step,
                                                                         int    retroNum = 0)
    {
      var result = new List<TmAnalogRetro>();

      var (ch, rtu, point) = addr.GetTupleShort();
      long startTime = _native.UxGmTime2UxTime(utcStartTime);

      var tmcAnalogShortList = new TmNativeDefs.TAnalogPointShort[count];
      await Task.Run(() => _native.TmcTakeRetroTit(_cid,
                                                   ch, rtu, point,
                                                   (uint) startTime,
                                                   (ushort) retroNum,
                                                   (ushort) count,
                                                   (ushort) step,
                                                   ref tmcAnalogShortList))
                .ConfigureAwait(false);

      for (var i = 0; i < count; i++)
      {
        result.Add(new TmAnalogRetro(tmcAnalogShortList[i].Value,
                                     tmcAnalogShortList[i].Flags,
                                     startTime + i * step));
      }

      return result;
    }


    public async Task<IReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr addr,
                                                                         long   utcStartTime,
                                                                         long   utcEndTime,
                                                                         int    step     = 0,
                                                                         int    retroNum = 0)
    {
      if (utcEndTime <= utcStartTime)
      {
        return null;
      }

      if (step == 0)
      {
        step = TmUtil.GetRetrospectivePreferredStep(utcStartTime, utcEndTime);
      }

      int pointsCount = (int) ((utcEndTime - utcStartTime) / step) + 1;

      return await GetAnalogRetro(addr, utcStartTime, pointsCount, step, retroNum).ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr   addr,
                                                                         DateTime startTime,
                                                                         DateTime endTime,
                                                                         int      step     = 0,
                                                                         int      retroNum = 0)
    {
      return await GetAnalogRetro(addr,
                                  DateUtil.GetUtcTimestampFromDateTime(startTime),
                                  DateUtil.GetUtcTimestampFromDateTime(endTime),
                                  step,
                                  retroNum)
               .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<TmAnalogRetro>> GetAnalogRetro(TmAddr addr,
                                                                         string startTime,
                                                                         string endTime,
                                                                         int    step     = 0,
                                                                         int    retroNum = 0)
    {
      return await GetAnalogRetro(addr,
                                  DateUtil.GetDateTime(startTime) ?? throw new ArgumentException(),
                                  DateUtil.GetDateTime(endTime)   ?? throw new ArgumentException(),
                                  step,
                                  retroNum)
               .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                                                   long   utcStartTime,
                                                                                                   long   utcEndTime)
    {
      var result = new List<TmAnalogImpulseArchiveInstant>();

      if (utcEndTime <= utcStartTime)
      {
        return null;
      }

      const uint queryFlags = (uint) (TmNativeDefs.ImpulseArchiveQueryFlags.Mom);
      const uint step       = 1;

      long startTime = _native.UxGmTime2UxTime(utcStartTime);
      long endTime   = _native.UxGmTime2UxTime(utcEndTime);

      uint count = 0;
      var tmcImpulseArchivePtr = await Task.Run(() => _native.TmcAanReadArchive(_cid,
                                                                                addr.ToIntegerWithoutPadding(),
                                                                                (uint) startTime,
                                                                                (uint) endTime,
                                                                                step,
                                                                                queryFlags,
                                                                                out count,
                                                                                null, IntPtr.Zero))
                                           .ConfigureAwait(false);
      if (tmcImpulseArchivePtr == IntPtr.Zero)
      {
        return null;
      }

      try
      {
        var structSize = Marshal.SizeOf(typeof(TmNativeDefs.TMAAN_ARCH_VALUE));
        for (var i = 0; i < count; i++)
        {
          var currentPtr             = new IntPtr(tmcImpulseArchivePtr.ToInt64() + i * structSize);
          var tmcImpulseArchivePoint = Marshal.PtrToStructure<TmNativeDefs.TMAAN_ARCH_VALUE>(currentPtr);
          result.Add(new TmAnalogImpulseArchiveInstant(tmcImpulseArchivePoint.Value,
                                                       tmcImpulseArchivePoint.Flags,
                                                       tmcImpulseArchivePoint.Ut,
                                                       tmcImpulseArchivePoint.Ms));
        }
      }
      finally
      {
        _native.TmcFreeMemory(tmcImpulseArchivePtr);
      }

      return result;
    }


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr   addr,
                                                                                                   DateTime startTime,
                                                                                                   DateTime endTime)
    {
      return await GetImpulseArchiveInstant(addr,
                                            DateUtil.GetUtcTimestampFromDateTime(startTime),
                                            DateUtil.GetUtcTimestampFromDateTime(endTime))
               .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveInstant>> GetImpulseArchiveInstant(TmAddr addr,
                                                                                                   string startTime,
                                                                                                   string endTime)
    {
      return await GetImpulseArchiveInstant(addr,
                                            DateUtil.GetDateTime(startTime) ?? throw new ArgumentException(),
                                            DateUtil.GetDateTime(endTime)   ?? throw new ArgumentException())
               .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
                                                                                                   long   utcStartTime,
                                                                                                   long   utcEndTime,
                                                                                                   int    step = 0)
    {
      var result = new List<TmAnalogImpulseArchiveAverage>();

      if (utcEndTime <= utcStartTime)
      {
        return null;
      }

      const uint queryFlags = (uint) (TmNativeDefs.ImpulseArchiveQueryFlags.Avg |
                                      TmNativeDefs.ImpulseArchiveQueryFlags.Min |
                                      TmNativeDefs.ImpulseArchiveQueryFlags.Max);
      if (step == 0)
      {
        step = TmUtil.GetRetrospectivePreferredStep(utcStartTime, utcEndTime);
      }

      long startTime = _native.UxGmTime2UxTime(utcStartTime);
      long endTime   = _native.UxGmTime2UxTime(utcEndTime);

      uint count = 0;
      var tmcImpulseArchivePtr = await Task.Run(() => _native.TmcAanReadArchive(_cid,
                                                                                addr.ToIntegerWithoutPadding(),
                                                                                (uint) startTime,
                                                                                (uint) endTime,
                                                                                (uint) step,
                                                                                queryFlags,
                                                                                out count,
                                                                                null, IntPtr.Zero))
                                           .ConfigureAwait(false);
      if (tmcImpulseArchivePtr == IntPtr.Zero)
      {
        return null;
      }

      try
      {
        float minValue   = 0;
        float maxValue   = 0;
        var   structSize = Marshal.SizeOf(typeof(TmNativeDefs.TMAAN_ARCH_VALUE));
        for (var i = 0; i < count; i++)
        {
          var currentPtr             = new IntPtr(tmcImpulseArchivePtr.ToInt64() + i * structSize);
          var tmcImpulseArchivePoint = Marshal.PtrToStructure<TmNativeDefs.TMAAN_ARCH_VALUE>(currentPtr);
          switch ((TmNativeDefs.ImpulseArchiveFlags) tmcImpulseArchivePoint.Tag)
          {
            case TmNativeDefs.ImpulseArchiveFlags.Max:
              maxValue = tmcImpulseArchivePoint.Value;
              break;

            case TmNativeDefs.ImpulseArchiveFlags.Min:
              minValue = tmcImpulseArchivePoint.Value;
              break;

            case TmNativeDefs.ImpulseArchiveFlags.Avg:
              float avgValue = tmcImpulseArchivePoint.Value;
              var point = new TmAnalogImpulseArchiveAverage(avgValue, minValue, maxValue,
                                                            tmcImpulseArchivePoint.Flags,
                                                            tmcImpulseArchivePoint.Ut + (uint) step, // прошлый пер.
                                                            tmcImpulseArchivePoint.Ms);
              minValue = 0;
              maxValue = 0;
              result.Add(point);
              break;
          }
        }
      }
      finally
      {
        _native.TmcFreeMemory(tmcImpulseArchivePtr);
      }

      return result;
    }


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr   addr,
                                                                                                   DateTime startTime,
                                                                                                   DateTime endTime,
                                                                                                   int      step = 0)
    {
      return await GetImpulseArchiveAverage(addr,
                                            DateUtil.GetUtcTimestampFromDateTime(startTime),
                                            DateUtil.GetUtcTimestampFromDateTime(endTime),
                                            step)
               .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<TmAnalogImpulseArchiveAverage>> GetImpulseArchiveAverage(TmAddr addr,
                                                                                                   string startTime,
                                                                                                   string endTime,
                                                                                                   int    step = 0)
    {
      return await GetImpulseArchiveAverage(addr,
                                            DateUtil.GetDateTime(startTime) ?? throw new ArgumentException(),
                                            DateUtil.GetDateTime(endTime)   ?? throw new ArgumentException(),
                                            step)
               .ConfigureAwait(false);
    }


    public async Task UpdateStatus(TmStatus status)
    {
      await UpdateStatuses(new List<TmStatus> {status}).ConfigureAwait(false);
    }


    public async Task UpdateStatusExplicitly(TmStatus status, bool getRealTelemetry)
    {
      await UpdateStatusesExplicitly(new List<TmStatus> {status}, getRealTelemetry).ConfigureAwait(false);
    }


    public async Task UpdateAnalog(TmAnalog analog)
    {
      await UpdateAnalogs(new List<TmAnalog> {analog}).ConfigureAwait(false);
    }


    public async Task UpdateAnalogExplicitly(TmAnalog analog, bool getRealTelemetry)
    {
      await UpdateAnalogsExplicitly(new List<TmAnalog> {analog}, getRealTelemetry).ConfigureAwait(false);
    }


    public async Task UpdateStatuses(IReadOnlyList<TmStatus> statuses)
    {
      if (statuses.IsNullOrEmpty()) return;

      var count       = statuses.Count;
      var tmcAddrList = new TmNativeDefs.TAdrTm[count];

      for (var i = 0; i < count; i++)
      {
        tmcAddrList[i] = statuses[i].TmAddr.ToAdrTm();
      }

      var tmcCommonPointsPtr =
        await Task.Run(() => _native.TmcTmValuesByListEx(_cid, (ushort) TmNativeDefs.TmDataTypes.Status, 0,
                                                         (uint) count,
                                                         tmcAddrList))
                  .ConfigureAwait(false);
      if (tmcCommonPointsPtr == IntPtr.Zero)
      {
        return;
      }

      var structSize = Marshal.SizeOf(typeof(TmNativeDefs.TCommonPoint));
      for (var i = 0; i < count; i++)
      {
        var currentPtr     = new IntPtr(tmcCommonPointsPtr.ToInt64() + i * structSize);
        var tmcCommonPoint = Marshal.PtrToStructure<TmNativeDefs.TCommonPoint>(currentPtr);
        statuses[i].FromTmcCommonPoint(tmcCommonPoint);
      }

      _native.TmcFreeMemory(tmcCommonPointsPtr);
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

      await Task.Run(() => _native.TmcStatusByList(_cid, (ushort) count, tmcAddrList, statusPointsList))
                .ConfigureAwait(false);

      for (var i = 0; i < count; i++)
      {
        statuses[i].FromTStatusPoint(statusPointsList[i]);
      }
    }


    public async Task UpdateAnalogs(IReadOnlyList<TmAnalog> analogs)
    {
      if (analogs.IsNullOrEmpty()) return;

      var count       = analogs.Count;
      var tmcAddrList = new TmNativeDefs.TAdrTm[count];

      for (var i = 0; i < count; i++)
      {
        tmcAddrList[i] = analogs[i].TmAddr.ToAdrTm();
      }

      var tmcCommonPointsPtr =
        await Task.Run(() => _native.TmcTmValuesByListEx(_cid, (ushort) TmNativeDefs.TmDataTypes.Analog, 0,
                                                         (uint) count,
                                                         tmcAddrList))
                  .ConfigureAwait(false);
      if (tmcCommonPointsPtr == IntPtr.Zero)
      {
        return;
      }

      var structSize = Marshal.SizeOf(typeof(TmNativeDefs.TCommonPoint));
      for (var i = 0; i < count; i++)
      {
        var currentPtr     = new IntPtr(tmcCommonPointsPtr.ToInt64() + i * structSize);
        var tmcCommonPoint = Marshal.PtrToStructure<TmNativeDefs.TCommonPoint>(currentPtr);
        analogs[i].FromTmcCommonPoint(tmcCommonPoint);
      }

      _native.TmcFreeMemory(tmcCommonPointsPtr);
    }


    public async Task UpdateAnalogsExplicitly(IReadOnlyList<TmAnalog> analogs, bool getRealTelemetry = false)
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

      await Task.Run(() => _native.TmcAnalogByList(_cid, (ushort) count, tmcAddrList, analogPointsList))
                .ConfigureAwait(false);

      for (var i = 0; i < count; i++)
      {
        analogs[i].FromTAnalogPoint(analogPointsList[i]);
      }
    }


    public async Task UpdateTagsPropertiesAndClassData(IReadOnlyList<TmTag> tags)
    {
      if (tags.IsNullOrEmpty()) return;

      var taskProperties = Task.Run(() =>
                                    {
                                      foreach (var tag in tags)
                                      {
                                        var sb = new StringBuilder(1024);
                                        var (ch, rtu, point) = tag.TmAddr.GetTupleShort();
                                        _native.TmcGetObjectProperties(_cid, tag.NativeType, ch, rtu, point,
                                                                       ref sb, 1024);
                                        tag.SetTmcObjectProperties(sb);
                                      }
                                    });

      var taskClassData = Task.Run(() => // TODO попробовать через один запрос завернуть, возможны проблемы с маршалом
                                   {
                                     foreach (var tag in tags)
                                     {
                                       var    tmcAddr = tag.TmAddr.ToAdrTm();
                                       IntPtr classDataPtr;

                                       switch (tag.Type)
                                       {
                                         case TmType.Status:
                                           classDataPtr = _native.TmcGetStatusClassData(_cid, 1, new[] {tmcAddr});
                                           break;
                                         case TmType.Analog:
                                           classDataPtr = _native.TmcGetAnalogClassData(_cid, 1, new[] {tmcAddr});
                                           break;
                                         default:
                                           return;
                                       }

                                       if (classDataPtr == IntPtr.Zero)
                                       {
                                         return;
                                       }

                                       var singleClassDataPtr =
                                         Marshal
                                           .PtrToStructure<IntPtr>(classDataPtr); // массив строк, а не просто строка
                                       var str = Marshal.PtrToStringAnsi(singleClassDataPtr);
                                       _native.TmcFreeMemory(classDataPtr);

                                       tag.SetTmcClassData(str);
                                     }
                                   });

      await Task.WhenAll(taskProperties, taskClassData)
                .ConfigureAwait(false);
    }


    public async Task UpdateTagPropertiesAndClassData(TmTag tag)
    {
      var taskProperties = UpdateTagProperties(tag);
      var taskClassData  = UpdateTagClassData(tag);

      await Task.WhenAll(taskProperties, taskClassData)
                .ConfigureAwait(false);
    }


    private async Task UpdateTagProperties(TmTag tag)
    {
      var sb = new StringBuilder(1024);
      var (ch, rtu, point) = tag.TmAddr.GetTupleShort();
      await Task.Run(() => _native.TmcGetObjectProperties(_cid,
                                                          tag.NativeType,
                                                          ch,
                                                          rtu,
                                                          point,
                                                          ref sb,
                                                          1024))
                .ConfigureAwait(false);
      tag.SetTmcObjectProperties(sb);
    }


    private async Task UpdateTagClassData(TmTag tag)
    {
      var    tmcAddr = tag.TmAddr.ToAdrTm();
      IntPtr classDataPtr;

      switch (tag.Type)
      {
        case TmType.Status:
          classDataPtr = await Task.Run(() => _native.TmcGetStatusClassData(_cid, 1, new[] {tmcAddr}))
                                   .ConfigureAwait(false);
          break;
        case TmType.Analog:
          classDataPtr = await Task.Run(() => _native.TmcGetAnalogClassData(_cid, 1, new[] {tmcAddr}))
                                   .ConfigureAwait(false);
          break;
        default:
          return;
      }

      if (classDataPtr == IntPtr.Zero)
      {
        return;
      }

      var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr); // у нас массив строк, а не просто строка
      var str                = Marshal.PtrToStringAnsi(singleClassDataPtr);
      _native.TmcFreeMemory(classDataPtr);

      tag.SetTmcClassData(str);
    }


    public async Task<IReadOnlyCollection<TmClassStatus>> GetStatusesClasses()
    {
      var tmClasses = new List<TmClassStatus>();
      var tmcAddr = new TmNativeDefs.TAdrTm
      {
        Ch  = -1,
        RTU = -1,
      };

      for (var i = 1; i <= 50; i++)
      {
        tmcAddr.Point = (short) i;
        var classDataPtr = await Task.Run(() => _native.TmcGetStatusClassData(_cid, 1, new[] {tmcAddr}))
                                     .ConfigureAwait(false);
        if (classDataPtr == IntPtr.Zero)
        {
          continue;
        }

        var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr); // у нас массив строк, а не просто строка
        var tmcClassDataStr    = Marshal.PtrToStringAnsi(singleClassDataPtr);

        var tmClassId    = 0;
        var tmClassName  = "";
        var tmClassFlags = 0;
        tmcClassDataStr?.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)
                       .ForEach(property =>
                                {
                                  var kvp = property.Split('=');
                                  if (kvp.Length != 2)
                                  {
                                    return;
                                  }

                                  if (kvp[0] == "ClassNumber")
                                  {
                                    int.TryParse(kvp[1], out tmClassId);
                                  }

                                  if (kvp[0] == "ClassName")
                                  {
                                    tmClassName = kvp[1];
                                  }

                                  if (kvp[0] == "ClassFlags")
                                  {
                                    int.TryParse(kvp[1], NumberStyles.HexNumber, null, out tmClassFlags);
                                  }
                                });
        if (tmClassId != 0)
        {
          tmClasses.Add(new TmClassStatus(tmClassId, tmClassName, tmClassFlags));
        }
      }

      return tmClasses;
    }


    public async Task<IReadOnlyCollection<TmClassAnalog>> GetAnalogsClasses()
    {
      var tmAnalogs = new List<TmClassAnalog>();
      var tmcAddr = new TmNativeDefs.TAdrTm
      {
        Ch  = -1,
        RTU = -1,
      };

      for (var i = 1; i <= 50; i++)
      {
        tmcAddr.Point = (short) i;
        var classDataPtr = await Task.Run(() => _native.TmcGetAnalogClassData(_cid, 1, new[] {tmcAddr}))
                                     .ConfigureAwait(false);
        if (classDataPtr == IntPtr.Zero)
        {
          continue;
        }

        var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr); // у нас массив строк, а не просто строка
        var tmcClassDataStr    = Marshal.PtrToStringAnsi(singleClassDataPtr);
        var tmClassId          = 0;
        var tmClassName        = "";
        tmcClassDataStr?.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)
                       .ForEach(property =>
                                {
                                  var kvp = property.Split('=');
                                  if (kvp.Length != 2)
                                  {
                                    return;
                                  }

                                  if (kvp[0] == "ClassNumber")
                                  {
                                    int.TryParse(kvp[1], out tmClassId);
                                  }

                                  if (kvp[0] == "ClassName")
                                  {
                                    tmClassName = kvp[1];
                                  }
                                });
        if (tmClassId != 0)
        {
          tmAnalogs.Add(new TmClassAnalog(tmClassId, tmClassName));
        }
      }

      return tmAnalogs;
    }


    public async Task UpdateTechObjectsProperties(IReadOnlyList<TmTechObject> techObjects)
    {
      if (techObjects.IsNullOrEmpty()) return;

      var count         = techObjects.Count;
      var nativeTobList = new TmNativeDefs.TTechObj[count];

      for (var i = 0; i < count; i++)
      {
        nativeTobList[i] = techObjects[i].ToNativeTechObj();
      }

      var tmcTechObjPropsPtr = await Task.Run(() => _native.TmcTechObjReadValues(_cid, nativeTobList, (uint) count))
                                         .ConfigureAwait(false);
      if (tmcTechObjPropsPtr == IntPtr.Zero)
      {
        return;
      }

      var structSize = Marshal.SizeOf(typeof(TmNativeDefs.TTechObjProps));
      for (var i = 0; i < count; i++)
      {
        var currentPtr      = new IntPtr(tmcTechObjPropsPtr.ToInt64() + i * structSize);
        var tmcTechObjProps = Marshal.PtrToStructure<TmNativeDefs.TTechObjProps>(currentPtr);
        if (tmcTechObjProps.Props == IntPtr.Zero)
        {
          continue;
        }

        techObjects[i].SetPropertiesFromTmc(
                                            TmNativeUtil
                                              .GetStringListFromDoubleNullTerminatedPointer(tmcTechObjProps.Props,
                                                                                            1024));
      }

      _native.TmcFreeMemory(tmcTechObjPropsPtr);
    }


    public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScript(TmStatus tmStatus)
    {
      if (tmStatus == null) return (false, null);

      await UpdateStatus(tmStatus).ConfigureAwait(false);
      var newStatus = tmStatus.Status ^ 1;

      return await CheckTelecontrolScriptExplicitly(tmStatus, newStatus).ConfigureAwait(false);
    }


    public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScriptExplicitly(
      TmStatus tmStatus,
      int      explicitNewStatus)
    {
      if (tmStatus == null) return (false, null);

      await UpdateStatus(tmStatus).ConfigureAwait(false);
      var (ch, rtu, point) = tmStatus.TmAddr.GetTupleShort();

      var scriptResult = await Task.Run(() => _native.TmcExecuteControlScript(_cid,
                                                                              ch,
                                                                              rtu,
                                                                              point,
                                                                              (short) explicitNewStatus))
                                   .ConfigureAwait(false);

      var conditions = new List<TmControlScriptCondition>();

      if (tmStatus.IsUnreliable ||
          tmStatus.IsInvalid    ||
          tmStatus.IsS2Failure  ||
          tmStatus.IsManuallyBlocked)
      {
        scriptResult = 0;
        conditions.Add(new TmControlScriptCondition(false, "Нет достоверной информации о состоянии"));
      }

      (await GetLastTmcErrorText().ConfigureAwait(false))
        ?.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)
        .ForEach(condition =>
                 {
                   var isConditionMet = condition[0] == '1';
                   var text           = condition.Substring(1);
                   conditions.Add(new TmControlScriptCondition(isConditionMet, text));
                 });

      return (scriptResult == 1, conditions);
    }


    public async Task OverrideTelecontrolScript()
    {
      await Task.Run(() => _native.TmcOverrideControlScript(_cid, true))
                .ConfigureAwait(false);
    }


    public async Task<TmTelecontrolResult> Telecontrol(TmStatus tmStatus)
    {
      if (tmStatus == null) return TmTelecontrolResult.CommandNotSentToServer;

      var (ch, rtu, point) = tmStatus.TmAddr.GetTuple();
      int currentStatus = await GetStatus(ch, rtu, point).ConfigureAwait(false);

      return await TelecontrolExplicitly(tmStatus, currentStatus ^ 1).ConfigureAwait(false);
    }


    public async Task<TmTelecontrolResult> TelecontrolExplicitly(TmStatus tmStatus, int explicitNewStatus)
    {
      if (tmStatus == null) return TmTelecontrolResult.CommandNotSentToServer;

      var (ch, rtu, point) = tmStatus.TmAddr.GetTuple();

      // регистрируем событие переключения (в старом клиенте такой порядок - сначала событие, потом само переключение)
      var ev = new TmNativeDefs.TEvent
      {
        Ch    = ch,
        Rtu   = rtu,
        Point = point,
        Id    = (ushort) TmNativeDefs.EventTypes.Control,
        Imp   = 0,
        Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Cmd      = (byte) explicitNewStatus,
          UserName = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"),
        }),
      };
      await Task.Run(() => _native.TmcRegEvent(_cid, ev))
                .ConfigureAwait(false);

      // телеуправление
      var result = await Task.Run(() => _native.TmcControlByStatus(_cid,
                                                                   (short) ch,
                                                                   (short) rtu,
                                                                   (short) point,
                                                                   (short) explicitNewStatus))
                             .ConfigureAwait(false);
      if (result <= 0) // если не прошло, регистрируем событие
      {
        ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Result   = (byte) result,
          Cmd      = (byte) explicitNewStatus,
          UserName = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"),
        });
      }

      return (TmTelecontrolResult) result;
    }


    public async Task<TmTelecontrolResult> TeleregulateByStepUp(TmAnalog analog)
    {
      return await TeleregulateByStepUpOrDown(analog, true).ConfigureAwait(false);
    }


    public async Task<TmTelecontrolResult> TeleregulateByStepDown(TmAnalog analog)
    {
      return await TeleregulateByStepUpOrDown(analog, false).ConfigureAwait(false);
    }


    public async Task<TmTelecontrolResult> TeleregulateByCode(TmAnalog analog, int code)
    {
      if (analog == null || !analog.HasTeleregulationByCode)
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }

      return await TeleregulateByValueOrCode(analog, null, code).ConfigureAwait(false);
    }


    public async Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog analog, float value)
    {
      if (analog == null || !analog.HasTeleregulationByValue)
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }

      return await TeleregulateByValueOrCode(analog, value, null).ConfigureAwait(false);
    }


    private async Task<TmTelecontrolResult> TeleregulateByValueOrCode(TmAnalog analog, float? value, int? code)
    {
      var (ch, rtu, point) = analog.TmAddr.GetTupleShort();

      GCHandle handle;
      if (value.HasValue)
      {
        handle = GCHandle.Alloc(value.Value, GCHandleType.Pinned);
      }
      else if (code.HasValue)
      {
        handle = GCHandle.Alloc((short) code.Value, GCHandleType.Pinned);
      }
      else
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }

      try
      {
        var result = await Task.Run(() => _native.TmcRegulationByAnalog(_cid,
                                                                        ch,
                                                                        rtu,
                                                                        point,
                                                                        analog.TmcRegulationType,
                                                                        handle.AddrOfPinnedObject()))
                               .ConfigureAwait(false);
        return (TmTelecontrolResult) result;
      }
      finally
      {
        handle.Free();
      }
    }


    private async Task<TmTelecontrolResult> TeleregulateByStepUpOrDown(TmAnalog analog, bool isStepUp)
    {
      if (analog == null ||
          !analog.HasTeleregulationByStep)
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }

      var (ch, rtu, point) = analog.TmAddr.GetTupleShort();

      var stepValue = (short) (isStepUp ? 1 : -1);
      var handle    = GCHandle.Alloc(stepValue, GCHandleType.Pinned);
      try
      {
        var result = await Task.Run(() => _native.TmcRegulationByAnalog(_cid,
                                                                        ch,
                                                                        rtu,
                                                                        point,
                                                                        analog.TmcRegulationType,
                                                                        handle.AddrOfPinnedObject()))
                               .ConfigureAwait(false);
        return (TmTelecontrolResult) result;
      }
      finally
      {
        handle.Free();
      }
    }


    public async Task<bool> AckTag(TmAddr addr)
    {
      switch (addr.Type)
      {
        case TmType.Status:
          return await AckStatus(new TmStatus(addr)).ConfigureAwait(false);
        case TmType.Analog:
          return await AckAnalog(new TmAnalog(addr)).ConfigureAwait(false);
        default:
          return false;
      }
    }


    public async Task AckAllStatuses()
    {
      await Task.Run(() => _native.TmcDriverCall(_cid, 0, (short) TmNativeDefs.DriverCall.Acknowledge, 0))
                .ConfigureAwait(false);
    }


    public async Task AckAllAnalogs()
    {
      await Task.Run(() => _native.TmcDriverCall(_cid, 0, (short) TmNativeDefs.DriverCall.AckAnalog, 0))
                .ConfigureAwait(false);
    }


    public async Task<bool> AckStatus(TmStatus status)
    {
      if (status == null) return false;

      var result = await Task.Run(() => _native.TmcDriverCall(_cid,
                                                              status.TmAddr.ToInteger(),
                                                              (short) TmNativeDefs.DriverCall.Acknowledge,
                                                              1))
                             .ConfigureAwait(false);
      if (result != TmNativeDefs.Success)
      {
        // TODO мы всегда в этой ветке, хотя квитируется
        Console.WriteLine("ACK NOT OK");
        return false;
      }

      Console.WriteLine("ACK OK");
      return true;
    }


    public async Task<bool> AckAnalog(TmAnalog analog)
    {
      if (analog == null) return false;

      var result = await Task.Run(() => _native.TmcDriverCall(_cid,
                                                              analog.TmAddr.ToInteger(),
                                                              (short) TmNativeDefs.DriverCall.AckAnalog,
                                                              1))
                             .ConfigureAwait(false);
      if (result != TmNativeDefs.Success)
      {
        // TODO мы всегда в этой ветке, хотя квитируется
        Console.WriteLine("ACK NOT OK");
        return false;
      }

      Console.WriteLine("ACK OK");
      return true;
    }


    public async Task<bool> AckEvent(TmEvent tmEvent)
    {
      if (tmEvent == null) return false;

      var nativeElix = new TmNativeDefs.TTMSElix
      {
        M = tmEvent.Elix.M,
        R = tmEvent.Elix.R,
      };
      var result = await Task.Run(() => _native.TmcEventLogAckRecords(_cid, ref nativeElix, 1))
                             .ConfigureAwait(false);

      return result;
    }


    public async Task AddStringToEventLog(string str,
                                          TmAddr tmAddr = null)
    {
      if (str == null)
      {
        return;
      }

      var eventLogAddr = (tmAddr != null)
                           ? (tmAddr.ToInteger() + 0x0001_0001) & 0xFFFF_0000
                           : 0;
      await Task.Run(() => _native.TmcEvlogPutStrBin(_cid,
                                                     0,
                                                     0,
                                                     0,
                                                     eventLogAddr,
                                                     str,
                                                     TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name,
                                                                                                16,
                                                                                                "windows-1251"),
                                                     16))
                .ConfigureAwait(false);
    }


    public async Task SetTagFlags(TmTag   tag,
                                  TmFlags flags)
    {
      await ToggleTagFlags(tag, flags, true).ConfigureAwait(false);
    }


    public async Task ClearTagFlags(TmTag   tag,
                                    TmFlags flags)
    {
      await ToggleTagFlags(tag, flags, false).ConfigureAwait(false);
    }


    private async Task ToggleTagFlags(TmTag   tmTag,
                                      TmFlags flags,
                                      bool    isSet)
    {
      byte timedValueType;

      switch (tmTag)
      {
        case TmStatus _:
          timedValueType = (byte) TmNativeDefs.VfType.Status;
          break;
        case TmAnalog _:
          timedValueType = (byte) TmNativeDefs.VfType.AnalogFloat;
          break;
        default:
          return;
      }

      if (isSet)
      {
        timedValueType += (byte) TmNativeDefs.VfType.FlagSet;
      }
      else
      {
        timedValueType += (byte) TmNativeDefs.VfType.FlagClear;
      }

      await Task.Run(() => _native.TmcSetTimedValues(_cid,
                                                     1,
                                                     new[]
                                                     {
                                                       new TmNativeDefs.TTimedValueAndFlags
                                                       {
                                                         Vf =
                                                         {
                                                           Adr   = tmTag.TmAddr.ToAdrTm(),
                                                           Type  = timedValueType,
                                                           Flags = (byte) flags,
                                                           Bits  = 0,
                                                         },
                                                         Xt =
                                                         {
                                                           Flags = (ushort) TmNativeDefs.TMXTimeFlags.User,
                                                         }
                                                       }
                                                     }))
                .ConfigureAwait(false);

      // изменения флагов 1-4 для ТС записываются в журнал событий
      if (tmTag is TmStatus &&
          (flags.HasFlag(TmFlags.LevelA) ||
           flags.HasFlag(TmFlags.LevelB) ||
           flags.HasFlag(TmFlags.LevelC) ||
           flags.HasFlag(TmFlags.LevelD)))
      {
        var (ch, rtu, point) = tmTag.TmAddr.GetTuple();
        var evCh       = (byte) 0xFF;
        var evCommand  = (byte) (isSet ? 1 : 0);
        var evOperator = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866");
        var ev = new TmNativeDefs.TEvent
        {
          Ch    = ch,
          Rtu   = rtu,
          Point = point,
          Id    = (ushort) TmNativeDefs.EventTypes.ManualStatusSet,
          Imp   = 0,
        };
        if (flags.HasFlag(TmFlags.LevelA))
        {
          ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
          {
            Ch       = evCh,
            Rtu      = 1,
            Cmd      = evCommand,
            UserName = evOperator,
          });
          await Task.Run(() => _native.TmcRegEvent(_cid, ev))
                    .ConfigureAwait(false);
        }

        if (flags.HasFlag(TmFlags.LevelB))
        {
          ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
          {
            Ch       = evCh,
            Rtu      = 2,
            Cmd      = evCommand,
            UserName = evOperator,
          });
          await Task.Run(() => _native.TmcRegEvent(_cid, ev))
                    .ConfigureAwait(false);
        }

        if (flags.HasFlag(TmFlags.LevelC))
        {
          ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
          {
            Ch       = evCh,
            Rtu      = 3,
            Cmd      = evCommand,
            UserName = evOperator,
          });
          await Task.Run(() => _native.TmcRegEvent(_cid, ev))
                    .ConfigureAwait(false);
        }

        if (flags.HasFlag(TmFlags.LevelD))
        {
          ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
          {
            Ch       = evCh,
            Rtu      = 4,
            Cmd      = evCommand,
            UserName = evOperator,
          });
          await Task.Run(() => _native.TmcRegEvent(_cid, ev))
                    .ConfigureAwait(false);
        }
      }
    }


    public async Task SetTagsFlags(IEnumerable<TmTag> tmTags,
                                   TmFlags            flags)
    {
      await ToggleTagsFlags(tmTags, flags, isSet: true).ConfigureAwait(false);
    }


    public async Task ClearTagsFlags(IEnumerable<TmTag> tmTags,
                                     TmFlags            flags)
    {
      await ToggleTagsFlags(tmTags, flags, isSet: false).ConfigureAwait(false);
    }


    private async Task ToggleTagsFlags(IEnumerable<TmTag> tmTags,
                                       TmFlags            flags,
                                       bool               isSet)
    {
      var timedValuesAndFlags = new List<TmNativeDefs.TTimedValueAndFlags>();

      foreach (var tmTag in tmTags)
      {
        byte timedValueType;
        switch (tmTag)
        {
          case TmStatus _:
            timedValueType = (byte) TmNativeDefs.VfType.Status;
            break;
          case TmAnalog _:
            timedValueType = (byte) TmNativeDefs.VfType.AnalogFloat;
            break;
          default:
            continue;
        }

        if (isSet)
        {
          timedValueType += (byte) TmNativeDefs.VfType.FlagSet;
        }
        else
        {
          timedValueType += (byte) TmNativeDefs.VfType.FlagClear;
        }

        timedValuesAndFlags.Add(new TmNativeDefs.TTimedValueAndFlags
        {
          Vf =
          {
            Adr   = tmTag.TmAddr.ToAdrTm(),
            Type  = timedValueType,
            Flags = (byte) flags,
            Bits  = 0,
          },
          Xt =
          {
            Flags = (ushort) TmNativeDefs.TMXTimeFlags.User,
          }
        });
      }

      await Task.Run(() => _native.TmcSetTimedValues(_cid,
                                                     (uint) timedValuesAndFlags.Count,
                                                     timedValuesAndFlags.ToArray()))
                .ConfigureAwait(false);
    }


    public async Task<bool> SwitchStatusManually(TmStatus tmStatus,
                                                 bool     alsoBlockManually = false)
    {
      if (tmStatus == null) return false;

      var (ch, rtu, point) = tmStatus.TmAddr.GetTuple();

      int currentStatus = await GetStatus(ch, rtu, point).ConfigureAwait(false);
      int newStatus     = currentStatus ^ 1;

      // регистрируем событие переключения (в старом клиенте такой порядок - сначала событие, потом само переключение)
      var ev = new TmNativeDefs.TEvent
      {
        Ch    = ch,
        Rtu   = rtu,
        Point = point,
        Id    = (ushort) TmNativeDefs.EventTypes.ManualStatusSet,
        Imp   = 0,
        Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Cmd      = (byte) newStatus,
          UserName = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"),
        }),
      };
      await Task.Run(() => _native.TmcRegEvent(_cid, ev))
                .ConfigureAwait(false);

      // выставляем новое состояние с флагом ручной установки
      if (tmStatus.IsInverted) // при инверсии нужно инвертировать команду
      {
        newStatus = newStatus ^ 1;
      }

      byte flags = (byte) TmNativeDefs.Flags.ManuallySet;
      if (alsoBlockManually)
      {
        flags += (byte) TmNativeDefs.Flags.UnreliableManu;
      }

      await Task.Run(() => _native.TmcSetTimedValues(_cid,
                                                     1,
                                                     new[]
                                                     {
                                                       new TmNativeDefs.TTimedValueAndFlags
                                                       {
                                                         Vf =
                                                         {
                                                           Adr = tmStatus.TmAddr.ToAdrTm(),
                                                           Type = (byte) TmNativeDefs.VfType.Status  +
                                                                  (byte) TmNativeDefs.VfType.FlagSet +
                                                                  (byte) TmNativeDefs.VfType.AlwaysSetValue,
                                                           Flags = flags,
                                                           Bits  = 1,
                                                           Value = (uint) newStatus,
                                                         },
                                                         Xt =
                                                         {
                                                           Flags = (ushort) TmNativeDefs.TMXTimeFlags.User,
                                                         }
                                                       }
                                                     }))
                .ConfigureAwait(false);

      return true;
    }


    public async Task SetTechObjectProperties(int                                 scheme,
                                              int                                 type,
                                              int                                 obj,
                                              IReadOnlyDictionary<string, string> properties)
    {
      var propsList  = properties.Select(p => $"{p.Key}={p.Value}");
      var propsBytes = TmNativeUtil.GetDoubleNullTerminatedBytesFromStringList(propsList);
      var propsPtr   = Marshal.AllocHGlobal(propsBytes.Length);
      Marshal.Copy(propsBytes, 0, propsPtr, propsBytes.Length);

      var tmcProps = new TmNativeDefs.TTechObjProps
      {
        TobFlg = (byte) TmNativeDefs.TofWr.Addt,
        Scheme = (uint) scheme,
        Type   = (ushort) type,
        Object = (uint) obj,
        Props  = propsPtr,
      };
      await Task.Run(() =>
                     {
                       _native.TmcTechObjBeginUpdate(_cid);
                       _native.TmcTechObjWriteValues(_cid, new[] {tmcProps}, 1);
                       _native.TmcTechObjEndUpdate(_cid);
                     }).ConfigureAwait(false);

      Marshal.FreeHGlobal(propsPtr);
    }


    public async Task ClearTechObjectProperties(int                 scheme,
                                                int                 type,
                                                int                 obj,
                                                IEnumerable<string> properties)
    {
      var propsList  = properties.Select(p => $"{p}=");
      var propsBytes = TmNativeUtil.GetDoubleNullTerminatedBytesFromStringList(propsList);
      var propsPtr   = Marshal.AllocHGlobal(propsBytes.Length);
      Marshal.Copy(propsBytes, 0, propsPtr, propsBytes.Length);

      var tmcProps = new TmNativeDefs.TTechObjProps
      {
        TobFlg = (byte) TmNativeDefs.TofWr.Addt,
        Scheme = (uint) scheme,
        Type   = (ushort) type,
        Object = (uint) obj,
        Props  = propsPtr,
      };
      await Task.Run(() =>
                     {
                       _native.TmcTechObjBeginUpdate(_cid);
                       _native.TmcTechObjWriteValues(_cid, new[] {tmcProps}, 1);
                       _native.TmcTechObjEndUpdate(_cid);
                     }).ConfigureAwait(false);

      Marshal.FreeHGlobal(propsPtr);
    }


    public async Task SetStatusNormalOn(TmStatus status)
    {
      await SetStatusNormal(status, 1).ConfigureAwait(false);
    }


    public async Task SetStatusNormalOff(TmStatus status)
    {
      await SetStatusNormal(status, 0).ConfigureAwait(false);
    }


    public async Task ClearStatusNormal(TmStatus status)
    {
      await SetStatusNormal(status, -1).ConfigureAwait(false);
    }


    private async Task SetStatusNormal(TmStatus status, int normalValue)
    {
      if (status == null) return;

      var (ch, rtu, point) = status.TmAddr.GetTupleShort();

      await Task.Run(() => _native.TmcSetStatusNormal(_cid, ch, rtu, point, (ushort) normalValue))
                .ConfigureAwait(false);
    }


    public async Task<int> GetStatusNormal(TmStatus status)
    {
      if (status == null) return -1;

      var (ch, rtu, point) = status.TmAddr.GetTupleShort();

      ushort normalValue = 0xFFFF;
      await Task.Run(() => _native.TmcGetStatusNormal(_cid, ch, rtu, point, out normalValue))
                .ConfigureAwait(false);

      return (normalValue == 0 || normalValue == 1) ? normalValue : -1;
    }


    public async Task SetStatus(int ch, int rtu, int point, int status)
    {
      if (status != 0 && status != 1) return;

      await Task.Run(() => _native.TmcSetStatus(_cid, (short) ch, (short) rtu, (short) point, (byte) status, null, 0))
                .ConfigureAwait(false);
    }


    public async Task SetAnalog(int ch, int rtu, int point, float value)
    {
      await Task.Run(() => _native.TmcSetAnalog(_cid, (short) ch, (short) rtu, (short) point, value, null))
                .ConfigureAwait(false);
    }


    public async Task<bool> SetAnalogManually(TmAnalog tmAnalog, float value)
    {
      if (tmAnalog == null) return false;

      // установка нового значения
      var uintValue = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0); // функция требует значение DWORD
      await Task.Run(() => _native.TmcSetTimedValues(_cid,
                                                     1,
                                                     new[]
                                                     {
                                                       new TmNativeDefs.TTimedValueAndFlags
                                                       {
                                                         Vf =
                                                         {
                                                           Adr = tmAnalog.TmAddr.ToAdrTm(),
                                                           Type = (byte) TmNativeDefs.VfType.AnalogFloat +
                                                                  (byte) TmNativeDefs.VfType.FlagSet     +
                                                                  (byte) TmNativeDefs.VfType.AlwaysSetValue,
                                                           Flags = (byte) TmFlags.ManuallySet,
                                                           Bits  = 32,
                                                           Value = uintValue,
                                                         },
                                                         Xt =
                                                         {
                                                           Flags = (ushort) TmNativeDefs.TMXTimeFlags.User,
                                                         }
                                                       }
                                                     })).ConfigureAwait(false);

      // регистрируем событие
      var ev = new TmNativeDefs.TEvent
      {
        Id  = (ushort) TmNativeDefs.EventTypes.ManualAnalogSet,
        Imp = 0,
        Data = TmNativeUtil.GetBytes(new TmNativeDefs.AnalogSetData
        {
          Cmd      = 1, // флаг ручной установки
          Value    = value,
          UserName = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"),
        }),
      };
      (ev.Ch, ev.Rtu, ev.Point) = tmAnalog.TmAddr.GetTuple();

      await Task.Run(() => _native.TmcRegEvent(_cid, ev))
                .ConfigureAwait(false);

      return true;
    }


    public async Task<bool> SetAlarmValue(TmAlarm tmAlarm, float value)
    {
      if (tmAlarm?.TmAnalog == null) return false;

      await Task.Run(() =>
                     {
                       // получение структуры уставки
                       var (ch, rtu, point) = tmAlarm.TmAnalog.TmAddr.GetTupleShort();
                       var nativeAlarm = new TmNativeDefs.TAlarm();
                       _native.TmcPeekAlarm(_cid, ch, rtu, point, (short) tmAlarm.Id, ref nativeAlarm);

                       // установка нового значения
                       nativeAlarm.Value = value;
                       _native.TmcPokeAlarm(_cid, ch, rtu, point, (short) tmAlarm.Id, ref nativeAlarm);
                     }).ConfigureAwait(false);

      // регистрируем событие
      var message = $"Изменена уставка \"{tmAlarm.Name}\" на \"{tmAlarm.TmAnalog.Name}\"" +
                    $", новое значение {tmAlarm.TmAnalog.FakeValueWithUnitString(value)}";
      await AddStringToEventLog(message, tmAlarm.TmAnalog.TmAddr).ConfigureAwait(false);

      return true;
    }


    public async Task<IReadOnlyCollection<string>> GetFilesInDirectory(string path)
    {
      var cfCid = await GetCfCid().ConfigureAwait(false);
      if (cfCid == 0)
      {
        Console.WriteLine("Ошибка при получении cfCid"); // todo
        return null;
      }

      const uint bufLength      = 8192;
      const int  errStringLenth = 1000;
      var        buf            = new char[bufLength];
      var        errString      = new StringBuilder(errStringLenth);
      uint       errCode        = 0;
      if (!await Task.Run(() => _native.CfsDirEnum(cfCid,
                                                   path,
                                                   ref buf,
                                                   bufLength,
                                                   out errCode,
                                                   ref errString,
                                                   errStringLenth))
                     .ConfigureAwait(false))
      {
        Console.WriteLine($"Ошибка при запросе списка файлов: {errCode} - {errString}");
        return null;
      }

      return TmNativeUtil.GetStringListFromDoubleNullTerminatedChars(buf);
    }


    public async Task<bool> DownloadFile(string remotePath, string localPath)
    {
      var cfCid = await GetCfCid().ConfigureAwait(false);
      if (cfCid == 0)
      {
        Console.WriteLine("Ошибка при получении cfCid");
        return false;
      }

      const int errStringLenth = 1000;
      var       errString      = new StringBuilder(errStringLenth);
      uint      errCode        = 0;
      if (!await Task.Run(() => _native.CfsFileGet(cfCid,
                                                   remotePath,
                                                   localPath,
                                                   60000,
                                                   IntPtr.Zero,
                                                   out errCode,
                                                   ref errString,
                                                   errStringLenth))
                     .ConfigureAwait(false))
      {
        Console.WriteLine($"Ошибка при скачивании файла: {errCode} - {errString}");
        return false;
      }

      if (!File.Exists(localPath))
      {
        Console.WriteLine("Ошибка при сохранении файла в файловую систему");
        return false;
      }

      return true;
    }


    public async Task<IReadOnlyCollection<TmChannel>> GetTmTreeChannels()
    {
      var result = new List<TmChannel>();

      await Task.Run(() =>
                     {
                       var itemsIndexes = new ushort[255];
                       var count = _native.TmcEnumObjects(_cid, (ushort) TmNativeDefs.TmDataTypes.Channel, 255,
                                                          ref itemsIndexes, 0, 0, 0);

                       for (int i = 0; i < count; i++)
                       {
                         var channel = new TmChannel {ChannelId = itemsIndexes[i]};
                         UpdateChannelName(channel);
                         result.Add(channel);
                       }
                     }).ConfigureAwait(false);

      return result;
    }


    public async Task<IReadOnlyCollection<TmRtu>> GetTmTreeRtus(int channelId)
    {
      if (channelId < 0 || channelId > 254) return null;

      var result = new List<TmRtu>();

      await Task.Run(() =>
                     {
                       var itemsIndexes = new ushort[255];
                       var count = _native.TmcEnumObjects(_cid, (ushort) TmNativeDefs.TmDataTypes.Rtu, 255,
                                                          ref itemsIndexes, (short) channelId, 0, 0);

                       for (int i = 0; i < count; i++)
                       {
                         var rtu = new TmRtu
                         {
                           ChannelId = channelId,
                           RtuId     = itemsIndexes[i],
                         };
                         UpdateRtuName(rtu);
                         result.Add(rtu);
                       }
                     }).ConfigureAwait(false);

      return result;
    }


    public async Task<IReadOnlyCollection<TmStatus>> GetTmTreeStatuses(int channelId, int rtuId)
    {
      if (channelId < 0 || channelId > 254 ||
          rtuId     < 1 || rtuId     > 255)
      {
        return null;
      }

      var   result     = new List<TmStatus>();
      short startIndex = 0;
      while (true)
      {
        var itemsIndexes = new ushort[255];
        var count = await Task.Run(() => _native.TmcEnumObjects(_cid,
                                                                (ushort) TmNativeDefs.TmDataTypes.Status,
                                                                255,
                                                                ref itemsIndexes,
                                                                (short) channelId,
                                                                (short) rtuId,
                                                                startIndex))
                              .ConfigureAwait(false);
        if (count == 0)
        {
          break;
        }

        for (var i = 0; i < count; i++)
        {
          result.Add(new TmStatus(channelId, rtuId, itemsIndexes[i]));
        }

        startIndex += (short) (count + 1);
        // todo name, properties?
      }

      return result;
    }


    public async Task<IReadOnlyCollection<TmAnalog>> GetTmTreeAnalogs(int channelId, int rtuId)
    {
      if (channelId < 0 || channelId > 254 ||
          rtuId     < 1 || rtuId     > 255)
      {
        return null;
      }

      var   result     = new List<TmAnalog>();
      short startIndex = 0;
      while (true)
      {
        var itemsIndexes = new ushort[255];
        var count = await Task.Run(() => _native.TmcEnumObjects(_cid,
                                                                (ushort) TmNativeDefs.TmDataTypes.Analog,
                                                                255,
                                                                ref itemsIndexes,
                                                                (short) channelId,
                                                                (short) rtuId,
                                                                startIndex))
                              .ConfigureAwait(false);
        if (count == 0)
        {
          break;
        }

        for (var i = 0; i < count; i++)
        {
          result.Add(new TmAnalog(channelId, rtuId, itemsIndexes[i]));
        }

        startIndex += (short) (count + 1);
        // todo name, properties?
      }

      return result;
    }


    public async Task<TmEventElix> GetCurrentEventsElix()
    {
      var elix = new TmNativeDefs.TTMSElix();
      if (!await Task.Run(() => _native.TmcGetCurrentElix(_cid, ref elix))
                     .ConfigureAwait(false))
      {
        return null;
      }

      return new TmEventElix(elix.R, elix.M);
    }


    private void UpdateChannelName(TmChannel channel)
    {
      var buf = new StringBuilder(1024);
      _native.TmcGetObjectName(_cid, (ushort) TmNativeDefs.TmDataTypes.Channel, (short) channel.ChannelId, 0, 0,
                               ref buf, 1024);
      channel.Name = buf.ToString();
    }


    private void UpdateRtuName(TmRtu rtu)
    {
      var buf = new StringBuilder(1024);
      _native.TmcGetObjectName(_cid, (ushort) TmNativeDefs.TmDataTypes.Rtu, (short) rtu.ChannelId, (short) rtu.RtuId,
                               0,
                               ref buf, 1024);
      rtu.Name = buf.ToString();
    }


    public async Task<bool> SetTagFlagsExplicitly(TmTag tag, TmFlags flags)
    {
      var (ch, rtu, point) = tag.TmAddr.GetTupleShort();

      switch (tag)
      {
        case TmStatus _:
          return await Task.Run(() => _native.TmcSetStatusFlags(_cid, ch, rtu, point, (short) flags))
                           .ConfigureAwait(false)
                 == TmNativeDefs.Success;

        case TmAnalog _:
          return await Task.Run(() => _native.TmcSetAnalogFlags(_cid, ch, rtu, point, (short) flags))
                           .ConfigureAwait(false)
                 == TmNativeDefs.Success;

        default:
          return false;
      }
    }


    public async Task<bool> ClearTagFlagsExplicitly(TmTag tag, TmFlags flags)
    {
      var (ch, rtu, point) = tag.TmAddr.GetTupleShort();

      switch (tag)
      {
        case TmStatus _:
          return await Task.Run(() => _native.TmcClrStatusFlags(_cid, ch, rtu, point, (short) flags))
                           .ConfigureAwait(false)
                 == TmNativeDefs.Success;

        case TmAnalog _:
          return await Task.Run(() => _native.TmcClrAnalogFlags(_cid, ch, rtu, point, (short) flags))
                           .ConfigureAwait(false)
                 == TmNativeDefs.Success;

        default:
          return false;
      }
    }

    public async Task<IReadOnlyList<TmEvent>> GetEventsByElix(TmEventFilter filter) // TODO unit test
    {
      if (filter.StartTime == null || filter.EndTime == null)
      {
        throw new Exception("Не задано время начало и конца архива событий");
      }

      var startTime = _native.UxGmTime2UxTime(DateUtil.GetUtcTimestampFromDateTime(filter.StartTime.Value));
      var endTime   = _native.UxGmTime2UxTime(DateUtil.GetUtcTimestampFromDateTime(filter.EndTime.Value));

      var events = new List<TmEvent>();
      var elix   = new TmNativeDefs.TTMSElix();

      while (true)
      {
        var (eventsBatchList, lastElix) = await GetEventsBatch(elix, filter.Types, startTime, endTime);
        if (eventsBatchList.IsNullOrEmpty()) break;
        events.AddRange(eventsBatchList);
        elix = lastElix;
      }

      return events;
    }


    private async Task<(IReadOnlyList<TmEvent>, TmNativeDefs.TTMSElix)> GetEventsBatch(TmNativeDefs.TTMSElix elix,
                                                                                       TmEventTypes          type,
                                                                                       long                  startTime,
                                                                                       long                  endTime)
    {
      var lastElix   = elix;
      var eventsList = new List<TmEvent>();
      await Task.Run(() =>
                     {
                       var tmcEventsElixPtr = _native.TmcEventLogByElix(_cid,
                                                                        ref lastElix,
                                                                        (ushort) type,
                                                                        (uint) startTime,
                                                                        (uint) endTime);

                       var i = 0;

                       if (tmcEventsElixPtr == IntPtr.Zero) return;
                       var currentPtr = tmcEventsElixPtr;

                       while (currentPtr != IntPtr.Zero)
                       {
                         var addDataBytes = new byte[255];
                         var buf          = new StringBuilder(1024);


                         var tmcEventElix = Marshal.PtrToStructure<TmNativeDefs.TEventElix>(currentPtr);

                         _native.TmcEventGetAdditionalRecData((uint) i, ref addDataBytes, 255);
                         var addData = TmNativeUtil.GetEventAddData(addDataBytes);

                         var sourceObjectName = GetEventSourceObjectName(tmcEventElix.Event);

                         eventsList.Add(TmEvent.CreateFromTEventElix(tmcEventElix, addData, sourceObjectName));

                         currentPtr = tmcEventElix.Next;
                         i++;
                       }

                       _native.TmcFreeMemory(tmcEventsElixPtr);
                     });

      return (eventsList, lastElix);
    }


    private string GetEventSourceObjectName(TmNativeDefs.TEvent tEvent)
    {
      var buf       = new StringBuilder(1024);
      var eventType = (TmEventTypes) tEvent.Id;

      switch (eventType)
      {
        case TmEventTypes.StatusChange:
        case TmEventTypes.ManualStatusSet:
        case TmEventTypes.Control:
          _native.TmcGetObjectName(_cid,
                                   (ushort) TmNativeDefs.TmDataTypes.Status,
                                   (short) tEvent.Ch,
                                   (short) tEvent.Rtu,
                                   (short) tEvent.Point,
                                   ref buf,
                                   1024);
          return buf.ToString();

        case TmEventTypes.Alarm:
        case TmEventTypes.ManualAnalogSet:
          _native.TmcGetObjectName(_cid,
                                   (ushort) TmNativeDefs.TmDataTypes.Analog,
                                   (short) tEvent.Ch,
                                   (short) tEvent.Rtu,
                                   (short) tEvent.Point,
                                   ref buf,
                                   1024);
          return buf.ToString();

        case TmEventTypes.Acknowledge:
          var ackData = TmNativeUtil.GetAcknowledgeDataFromTEvent(tEvent);
          _native.TmcGetObjectName(_cid,
                                   ackData.TmType,
                                   (short) tEvent.Ch,
                                   (short) tEvent.Rtu,
                                   (short) tEvent.Point,
                                   ref buf,
                                   1024);
          return buf.ToString();

        default:
          return "";
      }
    }
  }
}