using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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


    public async Task<TmServerComputerInfo> GetServerComputerInfo()
    {
      var cfCid = await GetCfCid().ConfigureAwait(false);
      if (cfCid == IntPtr.Zero)
      {
        Console.WriteLine("Ошибка при получении cfCid"); // todo
        return null;
      }

      const int errStringLength = 1000;
      var cis = new TmNativeDefs.ComputerInfoS
      {
        Len = (uint) Marshal.SizeOf(typeof(TmNativeDefs.ComputerInfoS))
      };
      var  errString = new StringBuilder(errStringLength);
      uint errCode   = 0;

      if (!await Task.Run(() => _native.CfsGetComputerInfo(cfCid,
                                                           ref cis,
                                                           out errCode,
                                                           ref errString,
                                                           errStringLength))
                     .ConfigureAwait(false))
      {
        return null;
      }

      return new TmServerComputerInfo(cis.ComputerName,
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


    public async Task<(string host, string server)> GetCurrentServerName()
    {
      const int bufSize = 255;
      var       host    = new StringBuilder(bufSize);
      var       server  = new StringBuilder(bufSize);

      // todo al сейчас всегда приходит 0
      /*if (!await Task.Run(() => _native.TmcGetCurrentServer(_cid, ref host, bufSize, ref server, bufSize))
                     .ConfigureAwait(false))
      {
        return (null, null);
      }*/
      await Task.Run(() => _native.TmcGetCurrentServer(_cid, ref host, bufSize, ref server, bufSize))
                .ConfigureAwait(false);
      return (host.ToString(), server.ToString());
    }


    public async Task<(string user, string password)> GenerateTokenForExternalApp()
    {
      const int tokenLength = 64;
      var       user        = new StringBuilder(tokenLength);
      var       password    = new StringBuilder(tokenLength);

      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;

      var cfCid = await GetCfCid().ConfigureAwait(false);

      await Task.Run(() => _native.CfsIfpcGetLogonToken(cfCid, ref user, ref password,
                                                        out errCode, ref errString, errStringLength))
                .ConfigureAwait(false);

      return (user.ToString(), password.ToString());
    }


    public async Task<IntPtr> GetCfCid()
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


    public async Task<IReadOnlyCollection<ITmAnalogRetro[]>> GetAnalogsMicroSeries(IReadOnlyList<TmAnalog> analogs)
    {
      if (analogs.IsNullOrEmpty())
      {
        return new[] {Array.Empty<ITmAnalogRetro>()};
      }

      var count      = analogs.Count;
      var addrList   = new TmNativeDefs.TAdrTm[count];
      var bufPtrList = new IntPtr[count];
      for (var i = 0; i < count; i++)
      {
        addrList[i]   = analogs[i].TmAddr.ToAdrTm();
        bufPtrList[i] = Marshal.AllocHGlobal(1024);
      }

      var fetchResult = await Task.Run(() => _native.TmcAnalogMicroSeries(_cid, (uint) count, addrList, bufPtrList))
                                  .ConfigureAwait(false);
      if (fetchResult != TmNativeDefs.Success)
      {
        bufPtrList.ForEach(_native.TmcFreeMemory);
        return new[] {Array.Empty<ITmAnalogRetro>()};
      }

      var result = new List<ITmAnalogRetro[]>(count);
      for (var i = 0; i < count; i++)
      {
        var analogSeries = Marshal.PtrToStructure<TmNativeDefs.TMSAnalogMSeries>(bufPtrList[i]);
        result.Add(analogSeries.Elements
                               .Take(analogSeries.Count)
                               .Select(el => new TmAnalogMicroSeries(el.Value, el.SFlg, el.Ut))
                               .Cast<ITmAnalogRetro>()
                               .ToArray());

        _native.TmcFreeMemory(bufPtrList[i]);
      }

      return result;
    }


    public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetro(TmAnalog analog,
                                                                          long     utcStartTime,
                                                                          int      count,
                                                                          int      step,
                                                                          int      retroNum = 0)
    {
      var result = new List<ITmAnalogRetro>();

      var (ch, rtu, point) = analog.TmAddr.GetTupleShort();
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

      var step = filter.Step ?? TmUtil.GetRetrospectivePreferredStep(startTime, endTime);

      int pointsCount = (int) ((endTime - startTime) / step) + 1;

      return await GetAnalogRetro(analog, startTime, pointsCount, step, retroNum).ConfigureAwait(false);
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

      const uint queryFlags = (uint) (TmNativeDefs.ImpulseArchiveQueryFlags.Mom);
      const uint step       = 1;

      uint count = 0;
      var tmcImpulseArchivePtr = await Task.Run(() => _native.TmcAanReadArchive(_cid,
                                                                                  analog.TmAddr
                                                                                    .ToIntegerWithoutPadding(),
                                                                                  (uint) _native.UxGmTime2UxTime(
                                                                                   startTime),
                                                                                  (uint) _native
                                                                                    .UxGmTime2UxTime(endTime),
                                                                                  step,
                                                                                  queryFlags,
                                                                                  out count,
                                                                                  null, IntPtr.Zero))
                                           .ConfigureAwait(false);
      if (tmcImpulseArchivePtr == IntPtr.Zero)
      {
        return null;
      }

      var result = new List<ITmAnalogRetro>();
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

      const uint queryFlags = (uint) (TmNativeDefs.ImpulseArchiveQueryFlags.Avg |
                                      TmNativeDefs.ImpulseArchiveQueryFlags.Min |
                                      TmNativeDefs.ImpulseArchiveQueryFlags.Max);

      var step = filter.Step ?? TmUtil.GetRetrospectivePreferredStep(startTime, endTime);

      uint count = 0;
      var tmcImpulseArchivePtr = await Task.Run(() => _native.TmcAanReadArchive(_cid,
                                                                                  analog.TmAddr
                                                                                    .ToIntegerWithoutPadding(),
                                                                                  (uint) _native.UxGmTime2UxTime(
                                                                                   startTime),
                                                                                  (uint) _native
                                                                                    .UxGmTime2UxTime(endTime),
                                                                                  (uint) step,
                                                                                  queryFlags,
                                                                                  out count,
                                                                                  null, IntPtr.Zero))
                                           .ConfigureAwait(false);
      if (tmcImpulseArchivePtr == IntPtr.Zero)
      {
        return null;
      }

      var result = new List<ITmAnalogRetro>();
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
      await Task.Run(() => UpdateStatusesSynchronously(statuses)).ConfigureAwait(false);
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
      await Task.Run(() => UpdateAnalogsSynchronously(analogs)).ConfigureAwait(false);
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


    private async Task UpdateTagProperties(TmTag tag)
    {
      await Task.Run(() => UpdateTagPropertiesSynchronously(tag)).ConfigureAwait(true);
    }


    private void UpdateTagPropertiesSynchronously(TmTag tag)
    {
      var sb = new StringBuilder(1024);
      var (ch, rtu, point) = tag.TmAddr.GetTupleShort();
      _native.TmcGetObjectProperties(_cid,
                                     tag.NativeType,
                                     ch,
                                     rtu,
                                     point,
                                     ref sb,
                                     1024);
      tag.SetTmcObjectProperties(sb);
    }


    private async Task UpdateTagClassData(TmTag tag)
    {
      await Task.Run(() => UpdateTagClassDataSynchronously(tag)).ConfigureAwait(true);
    }


    private void UpdateTagClassDataSynchronously(TmTag tag)
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

      var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr); // у нас массив строк, а не просто строка
      var str                = Marshal.PtrToStringAnsi(singleClassDataPtr);
      _native.TmcFreeMemory(classDataPtr);

      tag.SetTmcClassData(str);
    }


    private async Task UpdateAnalogTechParameters(TmTag tag)
    {
      await Task.Run(() => UpdateAnalogTechParametersSynchronously(tag)).ConfigureAwait(true);
    }


    private void UpdateAnalogTechParametersSynchronously(TmTag tag)
    {
      if (!(tag is TmAnalog tmAnalog))
      {
        return;
      }

      var tmcAddr = tag.TmAddr.ToAdrTm();
      var techParams = new TmNativeDefs.TAnalogTechParms
      {
        ZoneLim  = new float[TmNativeDefs.TAnalogTechParmsAlarmSize],
        Reserved = new uint[TmNativeDefs.TAnalogTechParamsReservedSize],
      };
      if (!_native.TmcGetAnalogTechParms(_cid, ref tmcAddr, ref techParams))
      {
        return;
      }

      tmAnalog.SetTmcTechParameters(techParams);
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


    public async Task UpdateTechObjectsProperties(IReadOnlyList<Tob> techObjects)
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


    public async Task<IReadOnlyCollection<Tob>> GetTechObjects(TobFilter filter)
    {
      uint count            = 0;
      var  filterProperties = TmNativeUtil.GetDoubleNullTerminatedPointerFromStringList(filter?.Properties);
      var tmcTechObjPropsPtr = await Task.Run(() => _native.TmcTechObjEnumValues(_cid,
                                                filter?.Scheme ?? uint.MaxValue,
                                                filter?.Type   ?? uint.MaxValue,
                                                filterProperties,
                                                out count))
                                         .ConfigureAwait(false);
      if (tmcTechObjPropsPtr == IntPtr.Zero)
      {
        return Array.Empty<Tob>();
      }

      var tobs       = new List<Tob>();
      var structSize = Marshal.SizeOf(typeof(TmNativeDefs.TTechObjProps));
      for (var i = 0; i < count; i++)
      {
        var currentPtr      = new IntPtr(tmcTechObjPropsPtr.ToInt64() + i * structSize);
        var tmcTechObjProps = Marshal.PtrToStructure<TmNativeDefs.TTechObjProps>(currentPtr);
        if (tmcTechObjProps.Props == IntPtr.Zero)
        {
          continue;
        }

        var tob = new Tob(tmcTechObjProps.Scheme, tmcTechObjProps.Type, tmcTechObjProps.Object);
        tob.SetPropertiesFromTmc(TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(tmcTechObjProps.Props,
                                   1024));
        tobs.Add(tob);
      }

      _native.TmcFreeMemory(tmcTechObjPropsPtr);

      return tobs;
    }


    public async Task<bool> RemoveAlert(TmAlert alert)
    {
      if (alert == null) return false;

      return await Task.Run(() => _native.TmcAlertListRemove(_cid,
                                                             new[] {new TmNativeDefs.TAlertListId {IData = alert.Id}}))
                       .ConfigureAwait(false);
    }


    public async Task<bool> RemoveAlerts(IEnumerable<TmAlert> alerts)
    {
      if (alerts == null) return false;

      var nativeAlertList = alerts.Select(a => new TmNativeDefs.TAlertListId {IData = a.Id})
                                  .ToArray();
      if (nativeAlertList.Length == 0)
      {
        return false;
      }

      return await Task.Run(() => _native.TmcAlertListRemove(_cid, nativeAlertList))
                       .ConfigureAwait(false);
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
          Cmd = (byte) explicitNewStatus,
          UserName =
            TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16,
                                                       "cp866"),
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
          Result = (byte) result,
          Cmd    = (byte) explicitNewStatus,
          UserName =
            TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"),
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
      return result == TmNativeDefs.Success;
    }


    public async Task<bool> AckAnalog(TmAnalog analog)
    {
      if (analog == null) return false;

      var result = await Task.Run(() => _native.TmcDriverCall(_cid,
                                                              analog.TmAddr.ToInteger(),
                                                              (short) TmNativeDefs.DriverCall.AckAnalog,
                                                              1))
                             .ConfigureAwait(false);
      return result == TmNativeDefs.Success;
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
          Cmd = (byte) newStatus,
          UserName =
            TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16,
                                                       "cp866"),
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
          Cmd   = 1, // флаг ручной установки
          Value = value,
          UserName =
            TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16,
                                                       "cp866"),
        }),
      };
      (ev.Ch, ev.Rtu, ev.Point) = tmAnalog.TmAddr.GetTuple();

      await Task.Run(() => _native.TmcRegEvent(_cid, ev))
                .ConfigureAwait(false);

      return true;
    }


    public async Task<bool> SetAnalogTechParameters(TmAnalog analog, TmAnalogTechParameters parameters)
    {
      if (analog == null || parameters == null)
      {
        return false;
      }

      var tmcAddr = analog.TmAddr.ToAdrTm();
      var techParams = new TmNativeDefs.TAnalogTechParms
      {
        ZoneLim  = new float[TmNativeDefs.TAnalogTechParmsAlarmSize],
        Reserved = new uint[TmNativeDefs.TAnalogTechParamsReservedSize],
      };
      if (!await Task.Run(() => _native.TmcGetAnalogTechParms(_cid, ref tmcAddr, ref techParams))
                     .ConfigureAwait(false))
      {
        return false;
      }

      techParams.MinVal  = parameters.Min;
      techParams.MaxVal  = parameters.Max;
      techParams.Nominal = parameters.Nominal;
      if (techParams.AlrPresent > 0)
      {
        techParams.ZoneLim[0] = parameters.MinAlarmOrInvalid;
        techParams.ZoneLim[1] = parameters.MinWarningOrInvalid;
        techParams.ZoneLim[2] = parameters.MaxWarningOrInvalid;
        techParams.ZoneLim[3] = parameters.MaxAlarmOrInvalid;
      }

      return await Task.Run(() => _native.TmcSetAnalogTechParms(_cid, ref tmcAddr, ref techParams))
                       .ConfigureAwait(false);
    }


    public async Task<bool> SetAlarmValue(TmAlarm tmAlarm, float value)
    {
      if (tmAlarm.TmAnalog == null) return false;

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
      if (cfCid == IntPtr.Zero)
      {
        Console.WriteLine("Ошибка при получении cfCid"); // todo
        return null;
      }

      const uint bufLength       = 8192;
      const int  errStringLength = 1000;
      var        buf             = new char[bufLength];
      var        errString       = new StringBuilder(errStringLength);
      uint       errCode         = 0;
      if (!await Task.Run(() => _native.CfsDirEnum(cfCid,
                                                   path,
                                                   ref buf,
                                                   bufLength,
                                                   out errCode,
                                                   ref errString,
                                                   errStringLength))
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
      if (cfCid == IntPtr.Zero)
      {
        Console.WriteLine("Ошибка при получении cfCid");
        return false;
      }

      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;
      if (!await Task.Run(() => _native.CfsFileGet(cfCid,
                                                   remotePath,
                                                   localPath,
                                                   60000,
                                                   IntPtr.Zero,
                                                   out errCode,
                                                   ref errString,
                                                   errStringLength))
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


    public async Task<IReadOnlyCollection<string>> GetComtradeDays()
    {
      var ptr = await Task.Run(() => _native.TmcComtradeEnumDays(_cid)).ConfigureAwait(false);

      return TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptr, 8192);
    }


    public async Task<IReadOnlyCollection<string>> GetComtradeFilesByDay(string day)
    {
      var ptr = await Task.Run(() => _native.TmcComtradeEnumFiles(_cid, day)).ConfigureAwait(false);

      return TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptr, 8192);
    }


    public async Task<bool> DownloadComtradeFile(string filename, string localPath)
    {
      if (!await Task.Run(() => _native.TmcComtradeGetFile(_cid, filename, localPath)).ConfigureAwait(false))
      {
        Console.WriteLine($"Ошибка при скачивании файла: {GetLastTmcError()}");
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
                         var channelId = itemsIndexes[i];
                         result.Add(new TmChannel(channelId,
                                                  GetChannelNameSync(channelId)));
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
                         var rtuId = itemsIndexes[i];
                         result.Add(new TmRtu(channelId,
                                              rtuId,
                                              GetRtuNameSync(channelId, rtuId)));
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


    public async Task<string> GetChannelName(int channelId)
    {
      return await Task.Run(() => GetChannelNameSync(channelId)).ConfigureAwait(false);
    }


    private string GetChannelNameSync(int channelId)
    {
      if (channelId < 0 || channelId > 254) return null;

      var buf = new StringBuilder(1024);
      _native.TmcGetObjectName(_cid, (ushort) TmNativeDefs.TmDataTypes.Channel, (short) channelId, 0, 0,
                               ref buf, 1024);
      return buf.ToString();
    }


    public async Task<string> GetRtuName(int channelId, int rtuId)
    {
      return await Task.Run(() => GetRtuNameSync(channelId, rtuId)).ConfigureAwait(false);
    }


    private string GetRtuNameSync(int channelId, int rtuId)
    {
      if (channelId < 0 || channelId > 254 ||
          rtuId     < 1 || rtuId     > 255)
      {
        return null;
      }

      var buf = new StringBuilder(1024);
      _native.TmcGetObjectName(_cid, (ushort) TmNativeDefs.TmDataTypes.Rtu, (short) channelId, (short) rtuId, 0,
                               ref buf, 1024);
      return buf.ToString();
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


    public async Task SetTagFlagsExplicitly(TmTag tag, TmFlags flags)
    {
      var (ch, rtu, point) = tag.TmAddr.GetTupleShort();

      switch (tag)
      {
        case TmStatus _:
          await Task.Run(() => _native.TmcSetStatusFlags(_cid, ch, rtu, point, (short) flags))
                    .ConfigureAwait(false);
          return;

        case TmAnalog _:
          await Task.Run(() => _native.TmcSetAnalogFlags(_cid, ch, rtu, point, (short) flags))
                    .ConfigureAwait(false);
          return;
      }
    }


    public async Task ClearTagFlagsExplicitly(TmTag tag, TmFlags flags)
    {
      var (ch, rtu, point) = tag.TmAddr.GetTupleShort();

      switch (tag)
      {
        case TmStatus _:
          await Task.Run(() => _native.TmcClrStatusFlags(_cid, ch, rtu, point, (short) flags))
                    .ConfigureAwait(false);
          return;

        case TmAnalog _:
          await Task.Run(() => _native.TmcClrAnalogFlags(_cid, ch, rtu, point, (short) flags))
                    .ConfigureAwait(false);
          return;
      }
    }


    public async Task<IReadOnlyCollection<TmEvent>> GetEventsArchive(TmEventFilter filter) // TODO unit test
    {
      if (filter.StartTime == null || filter.EndTime == null)
      {
        throw new Exception("Не задано время начала и конца архива событий");
      }

      var filterTypes = (filter.Types != TmEventTypes.None)
                          ? filter.Types
                          : TmEventTypes.Any;
      var filterImportances = (filter.Importances != TmEventImportances.None)
                                ? filter.Importances
                                : TmEventImportances.Any;

      var startTime = _native.UxGmTime2UxTime(DateUtil.GetUtcTimestampFromDateTime(filter.StartTime.Value));
      var endTime   = _native.UxGmTime2UxTime(DateUtil.GetUtcTimestampFromDateTime(filter.EndTime.Value));

      var events = new List<TmEvent>();
      var elix   = new TmNativeDefs.TTMSElix();
      var cache  = new Dictionary<string, TmTag>();

      while (true)
      {
        var (eventsBatchList, lastElix) = await GetEventsBatch(elix, filterTypes, startTime, endTime, cache)
                                            .ConfigureAwait(false);

        if (eventsBatchList.IsNullOrEmpty())
        {
          break;
        }

        events.AddRange(eventsBatchList.Where(e => filterImportances.HasFlag(e.ImportanceFlag)));
        elix = lastElix;
      }

      return events;
    }


    public async Task<(IReadOnlyCollection<TmEvent>, TmEventElix)> GetCurrentEvents(TmEventElix elix)
    {
      if (elix == null) return (null, null);

      var cache = new Dictionary<string, TmTag>();
      var currentElix = new TmNativeDefs.TTMSElix
      {
        R = elix.R,
        M = elix.M
      };

      var events = new List<TmEvent>();

      while (true)
      {
        var (eventsBatchList, lastBatchElix) = await GetEventsBatch(currentElix,
                                                                    TmEventTypes.Any,
                                                                    0,
                                                                    0xFFFFFFFF,
                                                                    cache)
                                                 .ConfigureAwait(false);
        if (eventsBatchList.IsNullOrEmpty())
        {
          break;
        }

        events.AddRange(eventsBatchList);
        currentElix = lastBatchElix;
      }

      return (events, new TmEventElix(currentElix.R, currentElix.M));
    }


    public async Task StartTmAddrTracer(int channel, int rtu, int point, TmType tmType, TmTraceTypes filterTypes)
    {
      var result =
        await Task.Run(() => _native.TmcSetTracer(_cid,
                                                  (short) channel,
                                                  (short) rtu,
                                                  (short) point,
                                                  (ushort) tmType.ToNativeType(),
                                                  (ushort) filterTypes))
                  .ConfigureAwait(false);

      Console.WriteLine($"Start tmc trace result: {result}");
    }


    public async Task StopTmAddrTracer(int channel, int rtu, int point, TmType tmType)
    {
      var result =
        await Task.Run(() => _native.TmcSetTracer(_cid,
                                                  (short) channel,
                                                  (short) rtu,
                                                  (short) point,
                                                  (ushort) tmType.ToNativeType(),
                                                  (ushort) TmTraceTypes.None))
                  .ConfigureAwait(false);

      Console.WriteLine($"Stop tmc trace result: {result}");
    }


    public async Task<TmServerInfo> GetServerInfo()
    {
      var info = new TmNativeDefs.TServerInfo();

      var result = await Task.Run(() => _native.TmcGetServerInfo(_cid, ref info)).ConfigureAwait(false);

      if (result != TmNativeDefs.Success)
      {
        Console.WriteLine(await GetLastTmcErrorText().ConfigureAwait(false));
        return null;
      }

      var (host, server) = await GetCurrentServerName().ConfigureAwait(false);
      return new TmServerInfo($"{host}\\{server}", info);
    }


    public async Task<IReadOnlyCollection<TmServerThread>> GetServerThreads()
    {
      var threadsPtr = await Task.Run(() => _native.TmcGetServerThreads(_cid)).ConfigureAwait(false);

      if (threadsPtr == IntPtr.Zero)
      {
        Console.WriteLine($"Ошибка при получении списка потоков сервера: {GetLastTmcErrorText().ConfigureAwait(false)}");
        return null;
      }

      var threadList = TmNativeUtil.GetUnknownLengthStringListFromDoubleNullTerminatedPointer(threadsPtr).Select(x =>
                          {
                            var regex =
                              new Regex(@"([0-9]*), (.*?) • ([-+]?[0-9]*) s • ([-+]?[0-9]*\.?[0-9]+) s");
                            var mc       = regex.Match(x);
                            var id       = int.Parse(mc.Groups[1].Value);
                            var name     = mc.Groups[2].Value;
                            var upTime   = int.Parse(mc.Groups[3].Value);
                            var workTime = float.Parse(mc.Groups[4].Value, CultureInfo.InvariantCulture);
                            return new TmServerThread(id, name, upTime, workTime);
                          })
                          .ToList();
      
      _native.CfsFreeMemory(threadsPtr);
      
      return threadList;
    }


    public async Task<TmAccessRights> GetAccessRights()
    {
      uint access = 0;

      await Task.Run(() => _native.TmcGetGrantedAccess(_cid, out access)).ConfigureAwait(false);

      return (TmAccessRights) access;
    }


    public async Task<IReadOnlyCollection<TmUserInfo>> GetUsersInfo()
    {
      var usersIdPtr = await Task.Run(() => _native.TmcGetUserList(_cid)).ConfigureAwait(false);
      
      var tmUsersInfo = new List<TmUserInfo>();
      
      if (usersIdPtr == IntPtr.Zero)
      {
        Console.WriteLine("Ошибка получения списка пользователей ТМС");
        return tmUsersInfo;
      }

      var ptrWithOffset = usersIdPtr;
      
      while (true)
      {
        var id = Marshal.PtrToStructure<uint>(ptrWithOffset);
        if (id == 0)
        {
          break;
        }

        var user = await GetUserInfo(id).ConfigureAwait(false);

        if (user != null)
        {
          tmUsersInfo.Add(user);
        }

        ptrWithOffset = IntPtr.Add(ptrWithOffset, sizeof(uint));
      }

      return tmUsersInfo;
    }


    public async Task<TmUserInfo> GetUserInfo(uint userId)
    {
      var tUserInfo = new TmNativeDefs.TUserInfo();

      if (await Task.Run(() => _native.TmcGetUserInfo(_cid, userId, ref tUserInfo)).ConfigureAwait(false))
      {
        return new TmUserInfo((int) userId, tUserInfo, string.Empty);
      }
      
      Console.WriteLine($"Ошибка получения информации о пользователе с ID {userId}");
      return null;

    }


    public async Task<TmUserInfo> GetExtendedUserInfo(int userId)
    {
      const int bufSize          = 1000;
      var       extendedInfoBuff = new StringBuilder(bufSize);
      var       tUserInfo        = new TmNativeDefs.TUserInfo();
      
      if (await Task.Run(() => _native.TmcGetUserInfoEx(_cid, (uint) userId, ref tUserInfo, ref extendedInfoBuff, bufSize)).ConfigureAwait(false))
      {
        return new TmUserInfo(userId, tUserInfo, extendedInfoBuff.ToString());
      }
      
      Console.WriteLine($"Ошибка получения расширенной информации о пользователе с ID {userId}");
      return null;
    }
    

    private async Task<(IReadOnlyList<TmEvent>, TmNativeDefs.TTMSElix)> GetEventsBatch(TmNativeDefs.TTMSElix elix,
      TmEventTypes                                                                                           type,
      long                                                                                                   startTime,
      long                                                                                                   endTime,
      Dictionary<string, TmTag>
        tmTagsCache)
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
                         const int bufSize      = 1000;
                         var       addDataBytes = new byte[bufSize];

                         var tmcEventElix = Marshal.PtrToStructure<TmNativeDefs.TEventElix>(currentPtr);

                         _native.TmcEventGetAdditionalRecData((uint) i, ref addDataBytes, bufSize);
                         var addData = TmNativeUtil.GetEventAddData(addDataBytes);


                         TmEvent tmEvent;
                         switch ((TmEventTypes) tmcEventElix.Event.Id)
                         {
                           case TmEventTypes.StatusChange:
                             var statusTmAddr = new TmAddr(TmType.Status,
                                                           tmcEventElix.Event.Ch,
                                                           tmcEventElix.Event.Rtu,
                                                           tmcEventElix.Event.Point);
                             var status = GetAndCacheUpdatedTmTagSynchronously(statusTmAddr, tmTagsCache);

                             if (tmcEventElix.EventSize >= TmNativeDefs.ExtendedStatusChangedEventSize)
                             {
                               var statusDataEx = TmNativeUtil.GetStatusDataExFromTEvent(tmcEventElix.Event);
                               tmEvent = TmEvent.CreateStatusChangeExtendedEvent(
                                tmcEventElix, addData, (TmStatus) status, statusDataEx);
                             }
                             else
                             {
                               var statusData = TmNativeUtil.GetStatusDataFromTEvent(tmcEventElix.Event);
                               tmEvent = TmEvent.CreateStatusChangeEvent(tmcEventElix, addData, (TmStatus) status,
                                                                         statusData);
                             }

                             break;

                           case TmEventTypes.Alarm:
                             var alarmSourceTmAddr = new TmAddr(TmType.Analog,
                                                                tmcEventElix.Event.Ch,
                                                                tmcEventElix.Event.Rtu,
                                                                tmcEventElix.Event.Point);

                             var alarmData = TmNativeUtil.GetAlarmDataFromTEvent(tmcEventElix.Event);
                             var alarmTypeName = GetExtendedObjectName(alarmSourceTmAddr,
                                                                       alarmData.AlarmID,
                                                                       TmNativeDefs.TmDataTypes.AnalogAlarm);

                             var alarmSourceAnalog =
                               GetAndCacheUpdatedTmTagSynchronously(alarmSourceTmAddr, tmTagsCache);

                             tmEvent = TmEvent.CreateAlarmTmEvent(tmcEventElix,
                                                                  addData,
                                                                  alarmTypeName,
                                                                  (TmAnalog) alarmSourceAnalog,
                                                                  alarmData);
                             break;

                           case TmEventTypes.Control:
                             var controlStatusTmAddr = new TmAddr(TmType.Status,
                                                                  tmcEventElix.Event.Ch,
                                                                  tmcEventElix.Event.Rtu,
                                                                  tmcEventElix.Event.Point);

                             var controlData = TmNativeUtil.GetControlDataFromTEvent(tmcEventElix.Event);

                             var controlStatus = GetAndCacheUpdatedTmTagSynchronously(controlStatusTmAddr, tmTagsCache);

                             tmEvent = TmEvent.CreateControlEvent(tmcEventElix,
                                                                  addData,
                                                                  (TmStatus) controlStatus,
                                                                  controlData);
                             break;

                           case TmEventTypes.Acknowledge:
                             var ackData       = TmNativeUtil.GetAcknowledgeDataFromTEvent(tmcEventElix.Event);
                             var ackTargetName = "";
                             if (tmcEventElix.Event.Point != 0)
                             {
                               var ackTargetTmAddr = new TmAddr(((TmNativeDefs.TmDataTypes) ackData.TmType).ToTmType(),
                                                                tmcEventElix.Event.Ch,
                                                                tmcEventElix.Event.Rtu,
                                                                tmcEventElix.Event.Point);
                               ackTargetName = GetObjectName(ackTargetTmAddr);
                             }

                             tmEvent = TmEvent.CreateAcknowledgeEvent(tmcEventElix, addData, ackTargetName, ackData);
                             break;

                           case TmEventTypes.ManualStatusSet:
                             var setStatusTmAddr = new TmAddr(TmType.Status,
                                                              tmcEventElix.Event.Ch,
                                                              tmcEventElix.Event.Rtu,
                                                              tmcEventElix.Event.Point);
                             var setStatusData = TmNativeUtil.GetControlDataFromTEvent(tmcEventElix.Event);

                             var setStatus = GetAndCacheUpdatedTmTagSynchronously(setStatusTmAddr, tmTagsCache);

                             tmEvent = TmEvent.CreateManualStatusSetEvent(tmcEventElix,
                                                                          addData,
                                                                          (TmStatus) setStatus,
                                                                          setStatusData);
                             break;

                           case TmEventTypes.ManualAnalogSet:
                             var setAnalogTmAddr = new TmAddr(TmType.Analog,
                                                              tmcEventElix.Event.Ch,
                                                              tmcEventElix.Event.Rtu,
                                                              tmcEventElix.Event.Point);

                             var setAnalogData = TmNativeUtil.GetAnalogSetDataFromTEvent(tmcEventElix.Event);

                             var setAnalog = GetAndCacheUpdatedTmTagSynchronously(setAnalogTmAddr, tmTagsCache);

                             tmEvent = TmEvent.CreateManualAnalogSetEvent(tmcEventElix, addData,
                                                                          (TmAnalog) setAnalog,
                                                                          setAnalogData);
                             break;

                           case TmEventTypes.Extended:
                             var strBinData = TmNativeUtil.GetStrBinData(tmcEventElix.Event);
                             tmEvent = TmEvent.CreateExtendedEvent(tmcEventElix, addData, strBinData);
                             break;

                           case TmEventTypes.FlagsChange:
                             var flagsChangeData       = TmNativeUtil.GetFlagsChangeData(tmcEventElix.Event);
                             var flagsChangeSourceType = (TmNativeDefs.TmDataTypes) flagsChangeData.TmType;

                             var flagsChangeSourceTmAddr = new TmAddr(flagsChangeSourceType.ToTmType(),
                                                                      tmcEventElix.Event.Ch,
                                                                      tmcEventElix.Event.Rtu,
                                                                      tmcEventElix.Event.Point);

                             if (flagsChangeSourceType == TmNativeDefs.TmDataTypes.Status)
                             {
                               var flagsChangeDataStatus = TmNativeUtil.GetFlagsChangeDataStatus(tmcEventElix.Event);

                               var flagsChangeSourceStatus =
                                 GetAndCacheUpdatedTmTagSynchronously(flagsChangeSourceTmAddr, tmTagsCache);

                               tmEvent = TmEvent.CreateStatusFlagsChangeEvent(tmcEventElix,
                                                                              addData,
                                                                              (TmStatus) flagsChangeSourceStatus,
                                                                              flagsChangeDataStatus);
                             }
                             else if (flagsChangeSourceType == TmNativeDefs.TmDataTypes.Analog)
                             {
                               var flagsChangeDataAnalog = TmNativeUtil.GetFlagsChangeDataAnalog(tmcEventElix.Event);

                               var flagsChangeSourceAnalog =
                                 GetAndCacheUpdatedTmTagSynchronously(flagsChangeSourceTmAddr, tmTagsCache);

                               tmEvent = TmEvent.CreateAnalogFlagsChangeEvent(tmcEventElix,
                                                                              addData,
                                                                              (TmAnalog) flagsChangeSourceAnalog,
                                                                              flagsChangeDataAnalog);
                             }
                             else
                             {
                               var sourceAccumName = GetObjectName(flagsChangeSourceTmAddr);
                               tmEvent = TmEvent.CreateAccumFlagsChangeEvent(tmcEventElix,
                                                                             addData,
                                                                             sourceAccumName,
                                                                             flagsChangeData);
                             }

                             break;

                           default:
                             tmEvent = TmEvent.CreateFromTEventElix(tmcEventElix, addData, "???");
                             break;
                         }

                         eventsList.Add(tmEvent);

                         currentPtr = tmcEventElix.Next;
                         i++;
                       }

                       _native.TmcFreeMemory(tmcEventsElixPtr);
                     })
                .ConfigureAwait(false);

      return (eventsList, lastElix);
    }


    private string GetObjectName(TmAddr tmAddr)
    {
      const int bufSize = 1024;
      var       buf     = new StringBuilder(bufSize);

      _native.TmcGetObjectName(_cid,
                               (ushort) tmAddr.Type.ToNativeType(),
                               (short) tmAddr.Ch,
                               (short) tmAddr.Rtu,
                               (short) tmAddr.Point,
                               ref buf,
                               bufSize);
      return buf.ToString();
    }


    private string GetExtendedObjectName(TmAddr                   tmAddr,
                                         ushort                   subItemId,
                                         TmNativeDefs.TmDataTypes tmDataType)
    {
      const int bufSize = 1024;
      var       buf     = new StringBuilder(bufSize);

      _native.TmcGetObjectNameEx(_cid,
                                 (ushort) tmDataType,
                                 (short) tmAddr.Ch,
                                 (short) tmAddr.Rtu,
                                 (short) tmAddr.Point,
                                 (short) subItemId,
                                 ref buf,
                                 bufSize);
      return buf.ToString();
    }


    private void UpdateStatusSynchronously(TmStatus status)
    {
      UpdateStatusesSynchronously(new List<TmStatus> {status});
    }


    private void UpdateStatusesSynchronously(IReadOnlyList<TmStatus> statuses)
    {
      if (statuses.IsNullOrEmpty()) return;

      var count       = statuses.Count;
      var tmcAddrList = new TmNativeDefs.TAdrTm[count];

      for (var i = 0; i < count; i++)
      {
        tmcAddrList[i] = statuses[i].TmAddr.ToAdrTm();
      }

      var tmcCommonPointsPtr = _native.TmcTmValuesByListEx(_cid, (ushort) TmNativeDefs.TmDataTypes.Status, 0,
                                                           (uint) count,
                                                           tmcAddrList);
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


    private void UpdateAnalogSynchronously(TmAnalog analog)
    {
      UpdateAnalogsSynchronously(new List<TmAnalog> {analog});
    }


    private void UpdateAnalogsSynchronously(IReadOnlyList<TmAnalog> analogs)
    {
      if (analogs.IsNullOrEmpty()) return;

      var count       = analogs.Count;
      var tmcAddrList = new TmNativeDefs.TAdrTm[count];

      for (var i = 0; i < count; i++)
      {
        tmcAddrList[i] = analogs[i].TmAddr.ToAdrTm();
      }

      var tmcCommonPointsPtr = _native.TmcTmValuesByListEx(_cid, (ushort) TmNativeDefs.TmDataTypes.Analog, 0,
                                                           (uint) count,
                                                           tmcAddrList);
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


    private TmTag GetAndCacheUpdatedTmTagSynchronously(TmAddr tagTmAddr, IDictionary<string, TmTag> cache)
    {
      if (cache.TryGetValue(tagTmAddr.ToString(), out var tmTag)) return tmTag;

      TmTag newTag;
      if (tagTmAddr.Type == TmType.Status)
      {
        newTag = new TmStatus(tagTmAddr);
        UpdateStatusSynchronously((TmStatus) newTag);
      }
      else
      {
        newTag = new TmAnalog(tagTmAddr);
        UpdateAnalogSynchronously((TmAnalog) newTag);
      }

      UpdateTagPropertiesAndClassDataSynchronously(newTag);

      cache.Add(tagTmAddr.ToString(), newTag);

      return newTag;
    }
  }
}