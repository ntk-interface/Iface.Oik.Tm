using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
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


    private event EventHandler<MqttMessage> MqttMessageReceived = delegate { };


    public TmsApi(ITmNative native)
    {
      _native = native;
    }


    public void SetCidAndUserInfo(int cid, TmUserInfo userInfo)
    {
      _cid      = cid;
      SetUserInfo(userInfo);
    }


    public void SetUserInfo(TmUserInfo userInfo)
    {
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
        Len = (uint)Marshal.SizeOf(typeof(TmNativeDefs.ComputerInfoS))
      };
      var  errString = new byte[errStringLength];
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
                                      (int)cis.CfsVerMaj,
                                      (int)cis.CfsVerMin,
                                      (int)cis.NtVerMaj,
                                      (int)cis.NtVerMin,
                                      (int)cis.NtBuild,
                                      (long)cis.Uptime);
    }


    public async Task<int> GetLastTmcError()
    {
      return (int)await Task.Run(() => _native.TmcGetLastError())
                            .ConfigureAwait(false);
    }


    public async Task<string> GetLastTmcErrorText()
    {
      var bufPtr = Marshal.AllocHGlobal(1024);
      await Task.Run(() => _native.TmcGetLastErrorText(_cid, bufPtr))
                .ConfigureAwait(false);

      var singleBufPtr = Marshal.PtrToStructure<IntPtr>(bufPtr); // массив строк, а не просто строка
  	  Marshal.FreeHGlobal(bufPtr); // не забываем освобождать память из HGlobal
	  var str          = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(singleBufPtr);
      _native.TmcFreeMemory(singleBufPtr);

      return str;
    }


    public string GetConnectionErrorText()
    {
      const uint bufSize = 256;
      var        buf     = new byte[bufSize];

      var result = _native.TmcGetConnectErrorText(_cid, ref buf, bufSize);

      return result ? EncodingUtil.Win1251BytesToUtf8(buf) : "Неизвестная ошибка";
    }


    public async Task<DateTime?> GetSystemTime()
    {
      return DateUtil.GetDateTimeFromTmString(await GetSystemTimeString().ConfigureAwait(false));
    }


    public async Task<string> GetSystemTimeString()
    {
      var tmcTime = new byte[80];
      await Task.Run(() => _native.TmcSystemTime(_cid, ref tmcTime, IntPtr.Zero))
                .ConfigureAwait(false);
      return EncodingUtil.Win1251BytesToUtf8(tmcTime);
    }


    public async Task<(string host, string server)> GetCurrentServerName()
    {
      const int bufSize = 255;
      var       host    = new byte[bufSize];
      var       server  = new byte[bufSize];

      // todo al сейчас всегда приходит 0
      /*if (!await Task.Run(() => _native.TmcGetCurrentServer(_cid, ref host, bufSize, ref server, bufSize))
                     .ConfigureAwait(false))
      {
        return (null, null);
      }*/
      await Task.Run(() => _native.TmcGetCurrentServer(_cid, ref host, bufSize, ref server, bufSize))
                .ConfigureAwait(false);
      return (EncodingUtil.Win1251BytesToUtf8(host), EncodingUtil.Win1251BytesToUtf8(server));
    }


    public async Task<(string user, string password)> GenerateTokenForExternalApp()
    {
      const int tokenLength = 64;
      var       user        = new byte[tokenLength];
      var       password    = new byte[tokenLength];

      const int errStringLength = 1000;
      var       errString       = new byte[errStringLength];
      uint      errCode         = 0;

      var cfCid = await GetCfCid().ConfigureAwait(false);

      await Task.Run(() => _native.CfsIfpcGetLogonToken(cfCid, ref user, ref password,
                                                        out errCode, ref errString, errStringLength))
                .ConfigureAwait(false);

      return (EncodingUtil.Win1251BytesToUtf8(user), EncodingUtil.Win1251BytesToUtf8(password));
    }


    public async Task<IntPtr> GetCfCid()
    {
      return await Task.Run(() => _native.TmcGetCfsHandle(_cid))
                       .ConfigureAwait(false);
    }


    public async Task<int> GetStatus(int ch, int rtu, int point)
    {
      return await Task.Run(() => _native.TmcStatus(_cid, (short)ch, (short)rtu, (short)point))
                       .ConfigureAwait(false);
    }


    public async Task<int> GetStatusFromRetro(int ch, int rtu, int point, DateTime time)
    {
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = _native.UxGmTime2UxTime(utcTime);
      
      var statusPoint = new TmNativeDefs.TStatusPoint();

      var isSuccess = await Task.Run(() => _native.TmcStatusFullEx(_cid,
                                                                   (short)ch,
                                                                   (short)rtu,
                                                                   (short)point,
                                                                   ref statusPoint,
                                                                   (uint)serverUtcTime))
                                .ConfigureAwait(false);

      var flags = (TmFlags)statusPoint.Flags;
      if (isSuccess == 0 || flags.HasFlag(TmFlags.Unreliable))
      {
        return -1;
      }
      return statusPoint.Status;
    }


    public async Task<IReadOnlyCollection<TmStatusRetro>> GetStatusRetroEx(TmStatus            status,
                                                                          TmStatusRetroFilter filter,
                                                                          bool                getRealTelemetry = false)
    {
      if (filter.StartTime >= filter.EndTime)
      {
        return Array.Empty<TmStatusRetro>();
      }

      var (ch, rtu, point) = status.TmAddr.GetTupleShort();

      var tmcStatusPoint = new TmNativeDefs.TStatusPoint();
      var statusRetros   = new List<TmStatusRetro>();
      
      var currentTime = filter.StartTime;

      while (currentTime <= filter.EndTime)
      {
        var time = _native.UxGmTime2UxTime(DateUtil.GetUtcTimestampFromDateTime(currentTime));
        var result = await Task.Run(() => _native.TmcStatusFullEx(_cid,
                                                                getRealTelemetry
                                                                  ? (short)(ch + TmNativeDefs.RealTelemetryFlag)
                                                                  : ch,
                                                                rtu,
                                                                point,
                                                                ref tmcStatusPoint,
                                                                (uint) time))
                               .ConfigureAwait(false);

        currentTime = currentTime.AddSeconds(filter.Step);

        if (result != TmNativeDefs.Success)
        {
          continue;
        }
        
        statusRetros.Add(new TmStatusRetro(tmcStatusPoint.Status, tmcStatusPoint.Flags, time));
      }

      return statusRetros;
    }
    

    public async Task<float> GetAnalog(int ch, int rtu, int point)
    {
      return await Task.Run(() => _native.TmcAnalog(_cid, (short)ch, (short)rtu, (short)point, null, 0))
                       .ConfigureAwait(false);
    }


    public async Task<ITmAnalogRetro> GetAnalogFromRetro(int ch, int rtu, int point, DateTime time, int retroNum = 0)
    {
      var analogPoint = new TmNativeDefs.TAnalogPoint();

      var isSuccess = await Task.Run(() => _native.TmcAnalogFull(_cid,
                                                                 (short)ch,
                                                                 (short)rtu,
                                                                 (short)point,
                                                                 ref analogPoint,
                                                                 time.ToTmByteArray(),
                                                                 (short)retroNum))
                                .ConfigureAwait(false);
      if (isSuccess == 0)
      {
        return TmAnalogRetro.UnreliableValue;
      }
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = _native.UxGmTime2UxTime(utcTime);
      return new TmAnalogRetro(analogPoint.AsFloat, analogPoint.Flags, serverUtcTime);
    }
    

    public async Task<float> GetAccum(int ch, int rtu, int point)
    {
      return await Task.Run(() => TmNative.tmcAccumValue(_cid, (short)ch, (short)rtu, (short)point, null))
                       .ConfigureAwait(false);
    }
    

    public async Task<float> GetAccumLoad(int ch, int rtu, int point)
    {
      return await Task.Run(() => TmNative.tmcAccumLoad(_cid, (short)ch, (short)rtu, (short)point, null))
                       .ConfigureAwait(false);
    }


    public async Task<ITmAccumRetro> GetAccumFromRetro(int ch, int rtu, int point, DateTime time)
    {
      var accumPoint = new TmNativeDefs.TAccumPoint();

      var isSuccess = await Task.Run(() => TmNative.tmcAccumFull(_cid,
                                                                 (short)ch,
                                                                 (short)rtu,
                                                                 (short)point,
                                                                 ref accumPoint,
                                                                 time.ToTmByteArray()))
                                .ConfigureAwait(false);
      if (isSuccess == 0)
      {
        return TmAccumRetro.UnreliableValue;
      }
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = _native.UxGmTime2UxTime(utcTime);
      return new TmAccumRetro(accumPoint.Value, accumPoint.Load, accumPoint.Flags, serverUtcTime);
    }


    public async Task<IReadOnlyCollection<ITmAnalogRetro[]>> GetAnalogsMicroSeries(IReadOnlyList<TmAnalog> analogs)
    {
      if (analogs.IsNullOrEmpty())
      {
        return new[] { Array.Empty<ITmAnalogRetro>() };
      }

      var count      = analogs.Count;
      var addrList   = new TmNativeDefs.TAdrTm[count];
      var bufPtrList = new IntPtr[count];
      for (var i = 0; i < count; i++)
      {
        addrList[i] = analogs[i].TmAddr.ToAdrTm();
      }

      var fetchResult = await Task.Run(() => _native.TmcAnalogMicroSeries(_cid, (uint)count, addrList, bufPtrList))
                                  .ConfigureAwait(false);
      if (fetchResult != TmNativeDefs.Success)
      {
        bufPtrList.ForEach(_native.TmcFreeMemory);
        return new[] { Array.Empty<ITmAnalogRetro>() };
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
                                                   (uint)startTime,
                                                   (ushort)retroNum,
                                                   (ushort)count,
                                                   (ushort)step,
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


    public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetAnalogRetroEx(TmAnalog analog,
                                                                            TmAnalogRetroFilter filter,
                                                                            int retroNum = 0,
                                                                            bool getRealTelemetry = false)
    {
      if (filter.StartTime >= filter.EndTime)
      {
        return null;
      }

      var (ch, rtu, point) = analog.TmAddr.GetTupleShort();

      var tmcAnalogPoint = new TmNativeDefs.TAnalogPoint();
      var analogRetros   = new List<ITmAnalogRetro>();

      var currentTime = filter.StartTime;

      while (currentTime <= filter.EndTime)
      {
        var time = currentTime;
        var result = await Task.Run(() => _native.TmcAnalogFull(_cid,
                                                                getRealTelemetry
                                                                  ? (short)(ch + TmNativeDefs.RealTelemetryFlag)
                                                                  : ch,
                                                                rtu,
                                                                point,
                                                                ref tmcAnalogPoint,
                                                                time.ToTmByteArray(),
                                                                (short)retroNum))
                               .ConfigureAwait(false);

        currentTime = currentTime.AddSeconds(filter.Step);

        if (result != TmNativeDefs.Success)
        {
          continue;
        }

        analogRetros.Add(new TmAnalogRetro(tmcAnalogPoint.AsFloat, tmcAnalogPoint.Flags,
                                           _native.UxGmTime2UxTime(DateUtil.GetUtcTimestampFromDateTime(time)),
                                           tmcAnalogPoint.AsCode));
      }

      return analogRetros;
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

      int pointsCount = (int)((endTime - startTime) / step) + 1;

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

      const uint queryFlags = (uint)(TmNativeDefs.ImpulseArchiveQueryFlags.Mom);
      const uint step       = 1;            

      uint count = 0;
      var tmcImpulseArchivePtr = await Task.Run(() => _native.TmcAanReadArchive(_cid,
                                                                                analog.TmAddr
                                                                                      .ToIntegerWithoutPadding(),
                                                                                (uint)_native.UxGmTime2UxTime(
                                                                                  startTime),
                                                                                (uint)_native
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

      const uint queryFlags = (uint)(TmNativeDefs.ImpulseArchiveQueryFlags.Avg |
                                     TmNativeDefs.ImpulseArchiveQueryFlags.Min |
                                     TmNativeDefs.ImpulseArchiveQueryFlags.Max);

      var step = filter.Step;

      uint count = 0;
      var tmcImpulseArchivePtr = await Task.Run(() => _native.TmcAanReadArchive(_cid,
                                                                                analog.TmAddr
                                                                                      .ToIntegerWithoutPadding(),
                                                                                (uint)_native.UxGmTime2UxTime(
                                                                                  startTime),
                                                                                (uint)_native
                                                                                  .UxGmTime2UxTime(endTime),
                                                                                (uint)step,
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
        var avgTime         = 0u;
        var avgValue        = 0f;
        var minValue        = 0f;
        var maxValue        = 0f;
        var internalCounter = 0;
        var structSize      = Marshal.SizeOf(typeof(TmNativeDefs.TMAAN_ARCH_VALUE));
        for (var i = 0; i < count; i++)
        {
          var currentPtr             = new IntPtr(tmcImpulseArchivePtr.ToInt64() + i * structSize);
          var tmcImpulseArchivePoint = Marshal.PtrToStructure<TmNativeDefs.TMAAN_ARCH_VALUE>(currentPtr);
          switch ((TmNativeDefs.ImpulseArchiveFlags)tmcImpulseArchivePoint.Tag)
          {
            case TmNativeDefs.ImpulseArchiveFlags.Avg:
              avgValue = tmcImpulseArchivePoint.Value;
              avgTime  = tmcImpulseArchivePoint.Ut;
              internalCounter++;
              break;

            case TmNativeDefs.ImpulseArchiveFlags.Max:
              maxValue = tmcImpulseArchivePoint.Value;
              internalCounter++;
              break;

            case TmNativeDefs.ImpulseArchiveFlags.Min:
              minValue = tmcImpulseArchivePoint.Value;
              internalCounter++;
              break;
          }
          if (internalCounter == 3)
          {
            result.Add(new TmAnalogImpulseArchiveAverage(avgValue,
                                                         minValue,
                                                         maxValue,
                                                         tmcImpulseArchivePoint.Flags,
                                                         avgTime + (uint)step, // прошлый период
                                                         tmcImpulseArchivePoint.Ms));
            internalCounter = 0;
            minValue        = 0;
            maxValue        = 0;
            avgValue        = 0;
          }
        }
      }
      finally
      {
        _native.TmcFreeMemory(tmcImpulseArchivePtr);
      }

      return result;
    }

    public async Task<IReadOnlyCollection<ITmAnalogRetro>> GetImpulseArchiveSlices(
      TmAnalog analog,
      TmAnalogRetroFilter filter)
        {
            var startTime = DateUtil.GetUtcTimestampFromDateTime(filter.StartTime);
            var endTime = DateUtil.GetUtcTimestampFromDateTime(filter.EndTime);
            if (endTime <= startTime) 
            {
                return null;
            }

            const uint queryFlags = (uint)(TmNativeDefs.ImpulseArchiveQueryFlags.Avg |
                                           TmNativeDefs.ImpulseArchiveQueryFlags.Min |
                                           TmNativeDefs.ImpulseArchiveQueryFlags.Max);

            var step = - filter.Step;

            uint count = 0;
            var tmcImpulseArchivePtr = await Task.Run(() => _native.TmcAanReadArchive(_cid,
                                                                                      analog.TmAddr
                                                                                            .ToIntegerWithoutPadding(),
                                                                                      (uint)_native.UxGmTime2UxTime(
                                                                                        startTime),
                                                                                      (uint)_native
                                                                                        .UxGmTime2UxTime(endTime),
                                                                                      (uint)step,
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
                    var currentPtr = new IntPtr(tmcImpulseArchivePtr.ToInt64() + i * structSize);
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


    public async Task UpdateAnalog(TmAnalog analog)
    {
      await UpdateAnalogs(new List<TmAnalog> { analog }).ConfigureAwait(false);
    }


    public async Task UpdateAnalogExplicitly(TmAnalog analog, uint time, ushort retroNum, bool getRealTelemetry)
    {
      await UpdateAnalogsExplicitly(new List<TmAnalog> { analog }, time, retroNum, getRealTelemetry)
        .ConfigureAwait(false);
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

      await Task.Run(() => _native.TmcStatusByList(_cid, (ushort)count, tmcAddrList, statusPointsList))
                .ConfigureAwait(false);

      for (var i = 0; i < count; i++)
      {
        statuses[i].FromTStatusPoint(statusPointsList[i]);
      }
    }


    public async Task UpdateStatusesFromRetro(IReadOnlyList<TmStatus> statuses, DateTime time)
    {
      if (statuses.IsNullOrEmpty()) return;
      
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = _native.UxGmTime2UxTime(utcTime);

      var count            = statuses.Count;
      var tmcAddrList      = new TmNativeDefs.TAdrTm[count];
      var statusPointsList = new TmNativeDefs.TStatusPoint[count];

      for (var i = 0; i < count; i++)
      {
        tmcAddrList[i] = statuses[i].TmAddr.ToAdrTm();
      }

      await Task.Run(() => _native.TmcStatusByListEx(_cid, 
                                                     (ushort)count, 
                                                     tmcAddrList, 
                                                     statusPointsList, 
                                                     (uint) serverUtcTime))
                .ConfigureAwait(false);

      for (var i = 0; i < count; i++)
      {
        statuses[i].FromTStatusPoint(statusPointsList[i]);
        statuses[i].ChangeTime = time;
      }
    }


    public async Task UpdateAnalogs(IReadOnlyList<TmAnalog> analogs)
    {
      await Task.Run(() => UpdateAnalogsSynchronously(analogs)).ConfigureAwait(false);
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

      await Task.Run(() => _native.TmcAnalogByList(_cid, (ushort)count, tmcAddrList, analogPointsList, time, retroNum))
                .ConfigureAwait(false);

      for (var i = 0; i < count; i++)
      {
        analogs[i].FromTAnalogPoint(analogPointsList[i]);
      }
    }


    public async Task UpdateAnalogsFromRetro(IReadOnlyList<TmAnalog> analogs,
                                             DateTime                time,
                                             int                     retroNum = 0)
    {
      if (analogs.IsNullOrEmpty()) return;
      
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = _native.UxGmTime2UxTime(utcTime);

      var count            = analogs.Count;
      var tmcAddrList      = new TmNativeDefs.TAdrTm[count];
      var analogPointsList = new TmNativeDefs.TAnalogPoint[count];

      for (var i = 0; i < count; i++)
      {
        tmcAddrList[i] = analogs[i].TmAddr.ToAdrTm();
      }

      await Task.Run(() => _native.TmcAnalogByList(_cid, 
                                                   (ushort)count, 
                                                   tmcAddrList, 
                                                   analogPointsList, 
                                                   (uint) serverUtcTime, 
                                                   (ushort) retroNum))
                .ConfigureAwait(false);

      for (var i = 0; i < count; i++)
      {
        analogs[i].FromTAnalogPoint(analogPointsList[i]);
        analogs[i].ChangeTime = time;
      }
    }


    public async Task UpdateAccums(IReadOnlyList<TmAccum> accums)
    {
      await Task.Run(() => UpdateAccumsSynchronously(accums)).ConfigureAwait(false);
    }

    
    public async Task UpdateAccumsExplicitly(IReadOnlyList<TmAccum> accums, 
                                             uint                   time,
                                             bool                   getRealTelemetry = false)
    {
      if (accums.IsNullOrEmpty()) return;

      var count            = accums.Count;
      var tmcAddrList      = new TmNativeDefs.TAdrTm[count];
      var accumPointsList = new TmNativeDefs.TAccumPoint[count];

      for (var i = 0; i < count; i++)
      {
        tmcAddrList[i] = accums[i].TmAddr.ToAdrTm();
        if (getRealTelemetry)
        {
          tmcAddrList[i].Ch += TmNativeDefs.RealTelemetryFlag;
        }
      }

      await Task.Run(() => _native.TmcAccumByList(_cid, (ushort)count, tmcAddrList, accumPointsList, time))
                .ConfigureAwait(false);
      
      for (var i = 0; i < count; i++)
      {
        accums[i].FromTAccumPoint(accumPointsList[i]);
      }
    }


    public async Task UpdateAccumsFromRetro(IReadOnlyList<TmAccum> accums,
                                            DateTime               time)
    {
      if (accums.IsNullOrEmpty()) return;
      
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = _native.UxGmTime2UxTime(utcTime);

      var count           = accums.Count;
      var tmcAddrList     = new TmNativeDefs.TAdrTm[count];
      var accumPointsList = new TmNativeDefs.TAccumPoint[count];

      for (var i = 0; i < count; i++)
      {
        tmcAddrList[i] = accums[i].TmAddr.ToAdrTm();
      }

      await Task.Run(() => _native.TmcAccumByList(_cid, 
                                                  (ushort)count, 
                                                  tmcAddrList, 
                                                  accumPointsList, 
                                                  (uint) serverUtcTime))
                .ConfigureAwait(false);

      for (var i = 0; i < count; i++)
      {
        accums[i].FromTAccumPoint(accumPointsList[i]);
        accums[i].ChangeTime = time;
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


    public async Task UpdateTagProperties(TmTag tag)
    {
      await Task.Run(() => UpdateTagPropertiesSynchronously(tag)).ConfigureAwait(false);
    }


    private void UpdateTagPropertiesSynchronously(TmTag tag)
    {
      var sb = new byte[1024];
      var (ch, rtu, point) = tag.TmAddr.GetTupleShort();
      _native.TmcGetObjectProperties(_cid,
                                     tag.NativeType,
                                     ch,
                                     rtu,
                                     point,
                                     ref sb,
                                     1024);
      tag.SetTmcObjectProperties(EncodingUtil.Win1251BytesToUtf8(sb));
    }
    
    
    private async Task<TmTag> FindTmTagReserveTag(TmTag tmTag)
    {
      var sb = new byte[1024];
      var (ch, rtu, point) = tmTag.TmAddr.GetTupleShort();
      await Task.Run(() => _native.TmcGetObjectProperties(_cid,
                                                          (ushort)tmTag.Type.ToNativeType(),
                                                          ch,
                                                          rtu,
                                                          point,
                                                          ref sb,
                                                          1024)).ConfigureAwait(false);
      
      var props = EncodingUtil.Win1251BytesToUtf8(sb).Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
      foreach (var prop in props)
      {
        var kvp = prop.Split('=');
        if (kvp.Length != 2)
        {
          continue;
        }
        if (kvp[0] == "Reserve" && TmAddr.TryParse(kvp[1], out var tmAddrReserve, tmTag.Type))
        {
          return TmTag.Create(tmAddrReserve);
        }
        if (kvp[0] == "Reserving" && TmAddr.TryParse(kvp[1], out var tmAddrReserving, tmTag.Type))
        {
          return TmTag.Create(tmAddrReserving);
        }
      }
      
      return null;
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
          classDataPtr = _native.TmcGetStatusClassData(_cid, 1, new[] { tmcAddr });
          break;
        case TmType.Analog:
          classDataPtr = _native.TmcGetAnalogClassData(_cid, 1, new[] { tmcAddr });
          break;
        default:
          return;
      }

      if (classDataPtr == IntPtr.Zero)
      {
        return;
      }

      var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr); // у нас массив строк, а не просто строка
      var str                = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(singleClassDataPtr);
      _native.TmcFreeMemory(classDataPtr);

      tag.SetTmcClassData(str);
    }

    
    public async Task UpdateTagsClassDataExplicitly(IReadOnlyList<TmTag> tags)
    {
      var analogs = new List<TmAnalog>();
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
        var chunk        = source.Take(128).ToList();
        var classDataPtr = _native.TmcGetAnalogClassData(_cid, (uint) chunk.Count, chunk.Select(x => x.TmAddr.ToAdrTm()).ToArray());

        if (classDataPtr == IntPtr.Zero)
        {
          return;
        }

        var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr); 
        
        foreach (var analog in chunk)
        {
          var str = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(singleClassDataPtr);

          analog.SetTmcClassData(str);

          if (str == string.Empty)
          {
            singleClassDataPtr = IntPtr.Add(singleClassDataPtr, 1);
            continue;
          }
          
          singleClassDataPtr = IntPtr.Add(singleClassDataPtr, str.Length + 1);
        }

        source = source.Skip(128).ToList();
        _native.TmcFreeMemory(classDataPtr);
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
        var chunk        = source.Take(128).ToList();
        var classDataPtr = _native.TmcGetStatusClassData(_cid, (uint) chunk.Count, chunk.Select(x => x.TmAddr.ToAdrTm()).ToArray());

        if (classDataPtr == IntPtr.Zero)
        {
          return;
        }
        
        
        var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr);


        foreach (var status in chunk)
        {
          var str = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(singleClassDataPtr);

          status.SetTmcClassData(str);

          if (str == string.Empty)
          {
            singleClassDataPtr = IntPtr.Add(singleClassDataPtr, 1);
            continue;
          }
          
          singleClassDataPtr = IntPtr.Add(singleClassDataPtr, str.Length + 1);
        }

        source = source.Skip(128).ToArray();
        
        _native.TmcFreeMemory(classDataPtr);
      }
    }
    
    
    private async Task UpdateAnalogTechParameters(TmTag tag)
    {
      await Task.Run(() => UpdateAnalogTechParametersSynchronously(tag)).ConfigureAwait(false);
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
      const int tmcAddrsLimit = 127;
      
      var tmClasses     = new List<TmClassStatus>();

      var tmcAddrs = Enumerable.Range(1, tmcAddrsLimit).Select(x => new TmNativeDefs.TAdrTm
      {
        Ch    = -1,
        RTU   = -1,
        Point = (short)x
      }).ToArray();
      
      var classDataPtr = await Task.Run(() => _native.TmcGetStatusClassData(_cid, tmcAddrsLimit, tmcAddrs))
                                   .ConfigureAwait(false);
      
      var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr);

      if (singleClassDataPtr == IntPtr.Zero)
      {
        return Array.Empty<TmClassStatus>();
      }

      for (var i = 0; i <= tmcAddrsLimit; i++)
      {
        var tmcClassDataStr = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(singleClassDataPtr);

        if (tmcClassDataStr == string.Empty)
        {
          singleClassDataPtr = IntPtr.Add(singleClassDataPtr, 1);
          continue;
        }
        
        var tmClassId    = 0;
        var tmClassName  = "";
        var tmClassFlags = 0;
        
        tmcClassDataStr.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
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
        
        singleClassDataPtr = IntPtr.Add(singleClassDataPtr, tmcClassDataStr.Length + 1);
      }

      return tmClasses;
    }


    public async Task<IReadOnlyCollection<TmClassAnalog>> GetAnalogsClasses()
    {
      const int tmcAddrsLimit = 127;
      var       tmAnalogs     = new List<TmClassAnalog>();
      
      var tmcAddrs = Enumerable.Range(1, tmcAddrsLimit).Select(x => new TmNativeDefs.TAdrTm
      {
        Ch    = -1,
        RTU   = -1,
        Point = (short)x
      }).ToArray();
      
      var classDataPtr = await Task.Run(() => _native.TmcGetAnalogClassData(_cid, tmcAddrsLimit, tmcAddrs))
                                   .ConfigureAwait(false);
      var singleClassDataPtr = Marshal.PtrToStructure<IntPtr>(classDataPtr);

      if (singleClassDataPtr == IntPtr.Zero)
      {
        return Array.Empty<TmClassAnalog>();
      }
      
      for (var i = 0; i <= tmcAddrsLimit; i++)
      {
        
        var tmcClassDataStr    = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(singleClassDataPtr);
        
        if (tmcClassDataStr == string.Empty)
        {
          singleClassDataPtr = IntPtr.Add(singleClassDataPtr, 1);
          continue;
        }
        
        var tmClassId          = 0;
        var tmClassName        = "";
        
        tmcClassDataStr.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
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
        
        singleClassDataPtr = IntPtr.Add(singleClassDataPtr, tmcClassDataStr.Length + 1);
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

      var tmcTechObjPropsPtr = await Task.Run(() => _native.TmcTechObjReadValues(_cid, nativeTobList, (uint)count))
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

      return await RemoveAlert(alert.Id).ConfigureAwait(false);
    }


    public async Task<bool> RemoveAlert(byte[] alertId)
    {
      if (alertId.IsNullOrEmpty()) return false;

      return await Task.Run(() => _native.TmcAlertListRemove(_cid,
                                                             new[]
                                                             {
                                                               new TmNativeDefs.TAlertListId { IData = alertId }
                                                             }))
                       .ConfigureAwait(false);
    }


    public async Task<bool> RemoveAlerts(IEnumerable<TmAlert> alerts)
    {
      if (alerts == null) return false;

      var nativeAlertList = alerts.Select(a => new TmNativeDefs.TAlertListId { IData = a.Id })
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
                                                                              (short)explicitNewStatus))
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
        ?.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
        .ForEach(condition =>
        {
          var isConditionMet = condition[0] == '1';
          var text           = condition.Substring(1);
          conditions.Add(new TmControlScriptCondition(isConditionMet, text));
        });

      return (scriptResult == 1, conditions);
    }


    public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTeleregulationScript(TmAnalog tmAnalog)
    {
      if (tmAnalog == null) return (false, null);

      await UpdateAnalog(tmAnalog).ConfigureAwait(false);
      
      var (ch, rtu, point) = tmAnalog.TmAddr.GetTupleShort();
      
      TmNativeDefs.AnalogRegulationType command;
      if (tmAnalog.HasTeleregulationByCode)
      {
        command = TmNativeDefs.AnalogRegulationType.Code;
      }
      else if (tmAnalog.HasTeleregulationByValue)
      {
        command = TmNativeDefs.AnalogRegulationType.Value;
      }
      else if (tmAnalog.HasTeleregulationByStep)
      {
        command = TmNativeDefs.AnalogRegulationType.Step;
      }
      else
      {
        return (false, new List<TmControlScriptCondition> { 
                   new TmControlScriptCondition(false, "Не определено регулирование") 
                 });
      }

      var handle = GCHandle.Alloc(0, GCHandleType.Pinned);

      int scriptResult;
      try
      {
        scriptResult = await Task.Run(() => _native.TmcExecuteRegulationScript(_cid,
                                                                               ch,
                                                                               rtu,
                                                                               point,
                                                                               (byte)command,
                                                                               handle.AddrOfPinnedObject()))
                                 .ConfigureAwait(false);
      }
      finally
      {
        handle.Free();
      }

      var conditions = new List<TmControlScriptCondition>();

      if (tmAnalog.IsUnreliable ||
          tmAnalog.IsInvalid    ||
          tmAnalog.IsManuallyBlocked)
      {
        scriptResult = 0;
        conditions.Add(new TmControlScriptCondition(false, "Нет достоверной информации о состоянии"));
      }

      (await GetLastTmcErrorText().ConfigureAwait(false))
      ?.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
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
        Id    = (ushort)TmNativeDefs.EventTypes.Control,
        Imp   = 0,
        Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Cmd = (byte)explicitNewStatus,
          UserName =
            TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16,
                                                       "cp866"),
        }),
      };
      await Task.Run(() => _native.TmcRegEvent(_cid, ev))
                .ConfigureAwait(false);

      // телеуправление
      var result = await Task.Run(() => _native.TmcControlByStatus(_cid,
                                                                   (short)ch,
                                                                   (short)rtu,
                                                                   (short)point,
                                                                   (short)explicitNewStatus))
                             .ConfigureAwait(false);
      if (result <= 0) // если не прошло, регистрируем событие
      {
        ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Result = (byte)result,
          Cmd    = (byte)explicitNewStatus,
          UserName =
            TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"),
        });
      }

      return (TmTelecontrolResult)result;
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
      if (analog == null)
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
      return await TeleregulateByValueOrCode(analog, null, code).ConfigureAwait(false);
    }


    public async Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog analog, float value)
    {
      if (analog == null)
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }
      return await TeleregulateByValueOrCode(analog, value, null).ConfigureAwait(false);
    }


    public async Task InputTelecontrolPassword(string password)
    {
      await Task.Run(() => _native.TmcSetTcPwd(_cid, EncodingUtil.Utf8ToWin1251Bytes(password))).ConfigureAwait(false);
    }


    private async Task<TmTelecontrolResult> TeleregulateByValueOrCode(TmAnalog analog, float? value, int? code)
    {
      var (ch, rtu, point) = analog.TmAddr.GetTupleShort();

      GCHandle                          handle;
      TmNativeDefs.AnalogRegulationType command;
      if (value.HasValue)
      {
        handle  = GCHandle.Alloc(value.Value, GCHandleType.Pinned);
        command = TmNativeDefs.AnalogRegulationType.Value;
      }
      else if (code.HasValue)
      {
        handle  = GCHandle.Alloc((short)code.Value, GCHandleType.Pinned);
        command = TmNativeDefs.AnalogRegulationType.Code;
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
                                                                        (byte) command,
                                                                        handle.AddrOfPinnedObject()))
                               .ConfigureAwait(false);
        return (TmTelecontrolResult)result;
      }
      finally
      {
        handle.Free();
      }
    }


    private async Task<TmTelecontrolResult> TeleregulateByStepUpOrDown(TmAnalog analog, bool isStepUp)
    {
      if (analog == null)
      {
        return TmTelecontrolResult.CommandNotSentToServer;
      }

      var (ch, rtu, point) = analog.TmAddr.GetTupleShort();

      var stepValue = (short)(isStepUp ? 1 : -1);
      var handle    = GCHandle.Alloc(stepValue, GCHandleType.Pinned);
      var command   = TmNativeDefs.AnalogRegulationType.Step;
      try
      {
        var result = await Task.Run(() => _native.TmcRegulationByAnalog(_cid,
                                                                        ch,
                                                                        rtu,
                                                                        point,
                                                                        (byte) command,
                                                                        handle.AddrOfPinnedObject()))
                               .ConfigureAwait(false);
        return (TmTelecontrolResult)result;
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
      await Task.Run(() => _native.TmcDriverCall(_cid, 0, (short)TmNativeDefs.DriverCall.Acknowledge, 0))
                .ConfigureAwait(false);
    }


    public async Task AckAllAnalogs()
    {
      await Task.Run(() => _native.TmcDriverCall(_cid, 0, (short)TmNativeDefs.DriverCall.AckAnalog, 0))
                .ConfigureAwait(false);
    }


    public async Task<bool> AckStatus(TmStatus status)
    {
      if (status == null) return false;

      var result = await Task.Run(() => _native.TmcDriverCall(_cid,
                                                              status.TmAddr.ToInteger(),
                                                              (short)TmNativeDefs.DriverCall.Acknowledge,
                                                              1))
                             .ConfigureAwait(false);
      return result == TmNativeDefs.Success;
    }


    public async Task<bool> AckAnalog(TmAnalog analog)
    {
      if (analog == null) return false;

      var result = await Task.Run(() => _native.TmcDriverCall(_cid,
                                                              analog.TmAddr.ToInteger(),
                                                              (short)TmNativeDefs.DriverCall.AckAnalog,
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
                                TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, EncodingUtil.Cp1251), 
                                tmAddr)
        .ConfigureAwait(false);
    }
    
    


    public async Task AddTmaRelatedStringToEventLog(string             message,
                                                    TmAddr             tmAddr,
                                                    TmEventImportances importances = TmEventImportances.Imp0,
                                                    DateTime?          time        = null)
    {
      var binStr = $"pt={tmAddr.Point};t={(uint)tmAddr.Type.ToNativeType()}";
      var bin    = TmNativeUtil.GetFixedBytesWithTrailingZero(binStr, binStr.Length + 1, EncodingUtil.Cp1251);
      
      await AddStrBinToEventLog(time,
                                importances,
                                TmEventLogExtendedSources.TmaRelated,
                                message,
                                bin,
                                tmAddr).ConfigureAwait(false);
    }


    public async Task AddStringToEventLogEx(DateTime?                 time,
                                            TmEventImportances        importances,
                                            TmEventLogExtendedSources source,
                                            string                    message,
                                            string                    binaryString = "",
                                            TmAddr                    tmAddr       = null)
    {
      var bin = string.IsNullOrEmpty(binaryString)
        ? Array.Empty<byte>()
        : TmNativeUtil.GetFixedBytesWithTrailingZero(binaryString, binaryString.Length + 1, EncodingUtil.Cp1251);

      await AddStrBinToEventLog(time, importances, source, message, bin, tmAddr).ConfigureAwait(false);
    }


    public async Task AddStrBinToEventLog(DateTime?                 time,
                                          TmEventImportances        importances,
                                          TmEventLogExtendedSources source,
                                          string                    message,
                                          byte[]                    binary = null,
                                          TmAddr                    tmAddr = null)
    {
      byte importance;
      switch (importances)
      {
        case TmEventImportances.Imp0:
          importance = 0;
          break;
        case TmEventImportances.Imp1:
          importance = 1;
          break;
        case TmEventImportances.Imp2:
          importance = 2;
          break;
        case TmEventImportances.Imp3:
          importance = 3;
          break;
        default:
          throw new Exception("Важность не поддерживается");
      }

      var sourceLongTag = tmAddr != null
        ? (tmAddr.ToInteger() + 0x0001_0001) & 0xFFFF_0000
        : 0;

      sourceLongTag |= (uint)source;

      var unixTime = time == null
        ? 0
        : _native.UxGmTime2UxTime(DateUtil.GetUtcTimestampFromDateTime(time.Value));

      var unixTimeMs = unixTime % 1000 / 10;

      var binaryPayload = binary ?? Array.Empty<byte>();

      await Task.Run(() =>
      {
        _native.TmcEvlogPutStrBin(_cid,
                                  (uint)unixTime,
                                  (byte)unixTimeMs,
                                  importance,
                                  sourceLongTag,
                                  EncodingUtil.Utf8ToWin1251Bytes(message),
                                  binaryPayload,
                                  (uint)binaryPayload.Length);
      }).ConfigureAwait(false);
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
          timedValueType = (byte)TmNativeDefs.VfType.Status;
          break;
        case TmAnalog _:
          timedValueType = (byte)TmNativeDefs.VfType.AnalogFloat;
          break;
        case TmAccum _:
          timedValueType = (byte)TmNativeDefs.VfType.AccumFloat;
          break;
        default:
          return;
      }

      if (isSet)
      {
        timedValueType += (byte)TmNativeDefs.VfType.FlagSet;
      }
      else
      {
        timedValueType += (byte)TmNativeDefs.VfType.FlagClear;
      }

      var tvf = new TmNativeDefs.TTimedValueAndFlags
      {
        Vf =
        {
          Adr   = tmTag.TmAddr.ToAdrTm(),
          Type  = timedValueType,
          Flags = (byte)flags,
          Bits  = 0,
        },
        Xt =
        {
          Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
        }
      };
      await Task.Run(() => _native.TmcSetTimedValues(_cid, 1, new[] { tvf }))
                .ConfigureAwait(false);
      
      // переключаем также флаги на резерве, если есть
      var resTmTag = await FindTmTagReserveTag(tmTag).ConfigureAwait(false);
      if (resTmTag != null)
      {
        tvf.Vf.Adr = resTmTag.TmAddr.ToAdrTm();
        await Task.Run(() => _native.TmcSetTimedValues(_cid, 1, new[] { tvf }))
                  .ConfigureAwait(false);
      }
      

      // изменения флагов 1-4 для ТС записываются в журнал событий
      if (tmTag is TmStatus &&
          (flags.HasFlag(TmFlags.LevelA) ||
           flags.HasFlag(TmFlags.LevelB) ||
           flags.HasFlag(TmFlags.LevelC) ||
           flags.HasFlag(TmFlags.LevelD)))
      {
        var (ch, rtu, point) = tmTag.TmAddr.GetTuple();
        var evCh       = (byte)0xFF;
        var evCommand  = (byte)(isSet ? 1 : 0);
        var evOperator = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866");
        var ev = new TmNativeDefs.TEvent
        {
          Ch    = ch,
          Rtu   = rtu,
          Point = point,
          Id    = (ushort)TmNativeDefs.EventTypes.ManualStatusSet,
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
            timedValueType = (byte)TmNativeDefs.VfType.Status;
            break;
          case TmAnalog _:
            timedValueType = (byte)TmNativeDefs.VfType.AnalogFloat;
            break;
          default:
            continue;
        }

        if (isSet)
        {
          timedValueType += (byte)TmNativeDefs.VfType.FlagSet;
        }
        else
        {
          timedValueType += (byte)TmNativeDefs.VfType.FlagClear;
        }

        timedValuesAndFlags.Add(new TmNativeDefs.TTimedValueAndFlags
        {
          Vf =
          {
            Adr   = tmTag.TmAddr.ToAdrTm(),
            Type  = timedValueType,
            Flags = (byte)flags,
            Bits  = 0,
          },
          Xt =
          {
            Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
          }
        });
      }

      await Task.Run(() => _native.TmcSetTimedValues(_cid,
                                                     (uint)timedValuesAndFlags.Count,
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
        Id    = (ushort)TmNativeDefs.EventTypes.ManualStatusSet,
        Imp   = 0,
        Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Cmd = (byte)newStatus,
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
        newStatus ^= 1;
      }

      byte flags = (byte)TmNativeDefs.Flags.ManuallySet;
      if (alsoBlockManually)
      {
        flags += (byte)TmNativeDefs.Flags.UnreliableManu;
      }

      var tvf = new TmNativeDefs.TTimedValueAndFlags
      {
        Vf =
        {
          Adr = tmStatus.TmAddr.ToAdrTm(),
          Type = (byte)TmNativeDefs.VfType.Status + 
                 (byte)TmNativeDefs.VfType.FlagSet + 
                 (byte)TmNativeDefs.VfType.AlwaysSetValue,
          Flags = flags,
          Bits  = 1,
          Value = (uint)newStatus,
        },
        Xt =
        {
          Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
        }
      };
      await Task.Run(() => _native.TmcSetTimedValues(_cid, 1, new[] { tvf }))
                .ConfigureAwait(false);
      
      // переключаем также резерв, если есть
      var resStatus = await FindTmTagReserveTag(tmStatus).ConfigureAwait(false);
      if (resStatus != null)
      {
        tvf.Vf.Adr = resStatus.TmAddr.ToAdrTm();
        await Task.Run(() => _native.TmcSetTimedValues(_cid, 1, new[] { tvf }))
                  .ConfigureAwait(false);
      }

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
        TobFlg = (byte)TmNativeDefs.TofWr.Addt,
        Scheme = (uint)scheme,
        Type   = (ushort)type,
        Object = (uint)obj,
        Props  = propsPtr,
      };
      await Task.Run(() =>
      {
        _native.TmcTechObjBeginUpdate(_cid);
        _native.TmcTechObjWriteValues(_cid, new[] { tmcProps }, 1);
        _native.TmcTechObjEndUpdate(_cid);
      }).ConfigureAwait(false);

      Marshal.FreeHGlobal(propsPtr);
    }


    public async Task SetTechObjectsProperties(IReadOnlyCollection<Tob> tobs)
    {
      var tmcProps = new List<TmNativeDefs.TTechObjProps>(tobs.Count);

      foreach (var tob in tobs)
      {
        var propsList  = tob.Properties.Select(p => $"{p.Key}={p.Value}");
        var propsBytes = TmNativeUtil.GetDoubleNullTerminatedBytesFromStringList(propsList);
        var propsPtr   = Marshal.AllocHGlobal(propsBytes.Length);
        Marshal.Copy(propsBytes, 0, propsPtr, propsBytes.Length);

        tmcProps.Add(new TmNativeDefs.TTechObjProps
        {
          TobFlg = (byte)TmNativeDefs.TofWr.Addt,
          Scheme = tob.Scheme,
          Type   = tob.Type,
          Object = tob.Object,
          Props  = propsPtr,
        });
      }

      await Task.Run(() =>
      {
        _native.TmcTechObjBeginUpdate(_cid);
        _native.TmcTechObjWriteValues(_cid, tmcProps.ToArray(), (uint)tmcProps.Count);
        _native.TmcTechObjEndUpdate(_cid);
      }).ConfigureAwait(false);

      foreach (var tmcProp in tmcProps)
      {
        Marshal.FreeHGlobal(tmcProp.Props);
      }
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
        TobFlg = (byte)TmNativeDefs.TofWr.Addt,
        Scheme = (uint)scheme,
        Type   = (ushort)type,
        Object = (uint)obj,
        Props  = propsPtr,
      };
      await Task.Run(() =>
      {
        _native.TmcTechObjBeginUpdate(_cid);
        _native.TmcTechObjWriteValues(_cid, new[] { tmcProps }, 1);
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

      await Task.Run(() => _native.TmcSetStatusNormal(_cid, ch, rtu, point, (ushort)normalValue))
                .ConfigureAwait(false);
                
      // переключаем также нормальное состояние резерва, если есть
      var resStatus = await FindTmTagReserveTag(status).ConfigureAwait(false);
      if (resStatus != null)
      {
        var (resCh, resRtu, resPoint) = resStatus.TmAddr.GetTupleShort();
        await Task.Run(() => _native.TmcSetStatusNormal(_cid, resCh, resRtu, resPoint, (ushort)normalValue))
                  .ConfigureAwait(false);
      }
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

      await Task.Run(() => _native.TmcSetStatus(_cid, (short)ch, (short)rtu, (short)point, (byte)status, null, 0))
                .ConfigureAwait(false);
    }


    public async Task SetAnalog(int ch, int rtu, int point, float value)
    {
      await Task.Run(() => _native.TmcSetAnalog(_cid, (short)ch, (short)rtu, (short)point, value, null))
                .ConfigureAwait(false);
    }


    public async Task SetAnalogByCode(int ch, int rtu, int point, int code)
    {
      await Task.Run(() => _native.TmcSetAnalogByCode(_cid, (short)ch, (short)rtu, (short)point, (short)code))
                .ConfigureAwait(false);
    }


    public async Task<bool> BackdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs,
                                            IReadOnlyList<float>    values,
                                            DateTime                time)
    {
      if (tmAnalogs.IsNullOrEmpty() || values.IsNullOrEmpty())
      {
        return false;
      }
      if (tmAnalogs.Count != values.Count)
      {
        return false;
      }
      
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = _native.UxGmTime2UxTime(utcTime);
      
      var tvf           = new TmNativeDefs.TTimedValueAndFlags[tmAnalogs.Count];
      for (var i = 0; i < tmAnalogs.Count; i++)
      {
        tvf[i] = new TmNativeDefs.TTimedValueAndFlags
        {
          Vf =
          {
            Adr = tmAnalogs[i].TmAddr.ToAdrTm(),
            Type = (byte)TmNativeDefs.VfType.AnalogFloat +
                   (byte)TmNativeDefs.VfType.AlwaysSetValue,
            Bits  = 32,
            Value = BitConverter.ToUInt32(BitConverter.GetBytes(values[i]), 0), // функция требует значение DWORD
          },
          Xt =
          {
            Sec = (uint)serverUtcTime,
          }
        };
      }

      var result = await Task.Run(() => _native.TmcSetTimedValues(_cid, (uint) tvf.Length, tvf))
                             .ConfigureAwait(false);

      return result > 0;
    }


    public async Task<bool> PostdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs,
                                            IReadOnlyList<float>    values,
                                            DateTime                time)
    {
      if (tmAnalogs.IsNullOrEmpty() || values.IsNullOrEmpty())
      {
        return false;
      }
      if (tmAnalogs.Count != values.Count)
      {
        return false;
      }
      
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = _native.UxGmTime2UxTime(utcTime);

      return await Task.Run(() => _native.TmcPerspPutAnalogs(_cid,
                                                             (uint)serverUtcTime,
                                                             (uint)tmAnalogs.Count,
                                                             tmAnalogs.Select(a => a.TmAddr.ToAdrTm()).ToArray(),
                                                             values.ToArray()))
                       .ConfigureAwait(false);
    }


    public async Task<bool> SetAnalogManually(TmAnalog tmAnalog, float value, bool alsoBlockManually = false)
    {
      if (tmAnalog == null) return false;

      // установка нового значения
      var uintValue = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0); // функция требует значение DWORD

      byte flags = (byte)TmNativeDefs.Flags.ManuallySet;
      if (alsoBlockManually)
      {
        flags += (byte)TmNativeDefs.Flags.UnreliableManu;
      }

      var tvf = new TmNativeDefs.TTimedValueAndFlags
      {
        Vf =
        {
          Adr = tmAnalog.TmAddr.ToAdrTm(),
          Type = (byte)TmNativeDefs.VfType.AnalogFloat +
                 (byte)TmNativeDefs.VfType.FlagSet     +
                 (byte)TmNativeDefs.VfType.AlwaysSetValue,
          Flags = flags,
          Bits  = 32,
          Value = uintValue,
        },
        Xt =
        {
          Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
        }
      };
      await Task.Run(() => _native.TmcSetTimedValues(_cid, 1, new[] { tvf })).ConfigureAwait(false);
      
      // выставляем также значение резерву, если есть
      var resAnalogs = await FindTmTagReserveTag(tmAnalog).ConfigureAwait(false);
      if (resAnalogs != null)
      {
        tvf.Vf.Adr = resAnalogs.TmAddr.ToAdrTm();
        await Task.Run(() => _native.TmcSetTimedValues(_cid, 1, new[] { tvf }))
                  .ConfigureAwait(false);
      }

      // регистрируем событие
      var ev = new TmNativeDefs.TEvent
      {
        Id  = (ushort)TmNativeDefs.EventTypes.ManualAnalogSet,
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


    public async Task<bool> SetAnalogBackdateManually(TmAnalog tmAnalog, float value, DateTime time)
    {
      if (tmAnalog == null) return false;
      
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = _native.UxGmTime2UxTime(utcTime);

      // установка нового значения
      var uintValue = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0); // функция требует значение DWORD

      var tvf = new TmNativeDefs.TTimedValueAndFlags
      {
        Vf =
        {
          Adr = tmAnalog.TmAddr.ToAdrTm(),
          Type = (byte)TmNativeDefs.VfType.AnalogFloat +
                 (byte)TmNativeDefs.VfType.FlagSet     +
                 (byte)TmNativeDefs.VfType.AlwaysSetValue,
          Flags = (byte)TmNativeDefs.Flags.ManuallySet,
          Bits  = 32,
          Value = uintValue,
        },
        Xt =
        {
          Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
          Sec   = (uint)serverUtcTime,
        }
      };
      await Task.Run(() => _native.TmcSetTimedValues(_cid, 1, new[] { tvf })).ConfigureAwait(false);
      
      // выставляем также значение резерву, если есть
      var resAnalogs = await FindTmTagReserveTag(tmAnalog).ConfigureAwait(false);
      if (resAnalogs != null)
      {
        tvf.Vf.Adr = resAnalogs.TmAddr.ToAdrTm();
        await Task.Run(() => _native.TmcSetTimedValues(_cid, 1, new[] { tvf }))
                  .ConfigureAwait(false);
      }

      // регистрируем событие
      var ev = new TmNativeDefs.TEvent
      {
        Id       = (ushort)TmNativeDefs.EventTypes.ManualAnalogSet,
        Imp      = 0,
        DateTime = time.ToTmByteArray(),
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


    public async Task<bool> SetStatusBackdateManually(TmStatus tmStatus,
                                                     int      status,
                                                     DateTime time)
    {
      if (tmStatus == null) return false;

      var (ch, rtu, point) = tmStatus.TmAddr.GetTuple();
      
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = _native.UxGmTime2UxTime(utcTime);

      // регистрируем событие переключения (в старом клиенте такой порядок - сначала событие, потом само переключение)
      var ev = new TmNativeDefs.TEvent
      {
        Ch       = ch,
        Rtu      = rtu,
        Point    = point,
        Id       = (ushort)TmNativeDefs.EventTypes.ManualStatusSet,
        Imp      = 0,
        DateTime = time.ToTmByteArray(),
        Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Cmd = (byte)status,
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
        status ^= 1;
      }

      var tvf = new TmNativeDefs.TTimedValueAndFlags
      {
        Vf =
        {
          Adr = tmStatus.TmAddr.ToAdrTm(),
          Type = (byte)TmNativeDefs.VfType.Status + 
                 (byte)TmNativeDefs.VfType.FlagSet + 
                 (byte)TmNativeDefs.VfType.AlwaysSetValue,
          Flags = (byte)TmNativeDefs.Flags.ManuallySet,
          Bits  = 1,
          Value = (uint)status,
        },
        Xt =
        {
          Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
          Sec   = (uint)serverUtcTime,
        }
      };
      await Task.Run(() => _native.TmcSetTimedValues(_cid, 1, new[] { tvf }))
                .ConfigureAwait(false);
      
      // переключаем также резерв, если есть
      var resStatus = await FindTmTagReserveTag(tmStatus).ConfigureAwait(false);
      if (resStatus != null)
      {
        tvf.Vf.Adr = resStatus.TmAddr.ToAdrTm();
        await Task.Run(() => _native.TmcSetTimedValues(_cid, 1, new[] { tvf }))
                  .ConfigureAwait(false);
      }

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
        _native.TmcPeekAlarm(_cid, ch, rtu, point, (short)tmAlarm.Id, ref nativeAlarm);

        // установка нового значения
        nativeAlarm.Value = value;
        _native.TmcPokeAlarm(_cid, ch, rtu, point, (short)tmAlarm.Id, ref nativeAlarm);
      }).ConfigureAwait(false);

      // регистрируем событие
      var message = $"Изменена уставка \"{tmAlarm.Name}\" на \"{tmAlarm.TmAnalog.Name}\"" +
                    $", новое значение {tmAlarm.TmAnalog.FakeValueWithUnitString(value)}";
      await AddStringToEventLog(message, tmAlarm.TmAnalog.TmAddr).ConfigureAwait(false);

      return true;
    }
    
    
    public async Task SetAccum(int ch, int rtu, int point, float value)
    {
      await Task.Run(() => _native.TmcSetAccumValue(_cid, (short)ch, (short)rtu, (short)point, value, null))
                .ConfigureAwait(false);
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
      var        errString       = new byte[errStringLength];
      uint       errCode         = 0;
      if (!await Task.Run(() => _native.CfsDirEnum(cfCid,
												   EncodingUtil.Utf8ToWin1251Bytes(path),
                                                   ref buf,
                                                   bufLength,
                                                   out errCode,
                                                   ref errString,
                                                   errStringLength))
                     .ConfigureAwait(false))
      {
        Console.WriteLine(
          $"Ошибка при запросе списка файлов: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errString)}");
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
	  var fileTime = new TmNativeDefs.FileTime();
	  const int errStringLength = 1000;
      var       errString       = new byte[errStringLength];
      uint      errCode         = 0;
      if (!await Task.Run(() => _native.CfsFileGet(cfCid,
                                                   EncodingUtil.Utf8ToWin1251Bytes(remotePath),
                                                   EncodingUtil.Utf8ToWin1251Bytes(localPath),
                                                   60000,
                                                   ref fileTime,
                                                   out errCode,
                                                   ref errString,
                                                   errStringLength))
                     .ConfigureAwait(false))
      {
        Console.WriteLine($"Ошибка при скачивании файла: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errString)}");
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
      var ptr = await Task.Run(() => _native.TmcComtradeEnumFiles(_cid, EncodingUtil.Utf8ToWin1251Bytes(day)))
                          .ConfigureAwait(false);

      return TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptr, 8192);
    }


    public async Task<bool> DownloadComtradeFile(string filename, string localPath)
    {
      if (!await Task.Run(() => _native.TmcComtradeGetFile(_cid, 
                                                           EncodingUtil.Utf8ToWin1251Bytes(filename), 
                                                             EncodingUtil.Utf8ToWin1251Bytes(localPath)))
                     .ConfigureAwait(false))
      {
        Console.WriteLine($"Ошибка при скачивании файла: {GetLastTmcError()}");
        return false;
      }

      return true;
    }


    public async Task<string> GetExpressionResult(string expression)
    {
      const int bufSize = 1024;

      var buf = new byte[bufSize];
      await Task.Run(() => _native.TmcEvaluateExpression(_cid, EncodingUtil.Utf8ToWin1251Bytes(expression), buf, bufSize))
                .ConfigureAwait(false);

      return EncodingUtil.Win1251BytesToUtf8(buf);
    }


    public string GetExpressionResultSync(string expression)
    {
      const int bufSize = 1024;

      var buf = new byte[bufSize];
      _native.TmcEvaluateExpression(_cid, EncodingUtil.Utf8ToWin1251Bytes(expression), buf, bufSize);

      return EncodingUtil.Win1251BytesToUtf8(buf);
    }


    public async Task<IReadOnlyCollection<TmChannel>> GetTmTreeChannels()
    {
      var result = new List<TmChannel>();

      await Task.Run(() =>
      {
        var itemsIndexes = new ushort[255];
        var count = _native.TmcEnumObjects(_cid, (ushort)TmNativeDefs.TmDataTypes.Channel, 255,
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
        var count = _native.TmcEnumObjects(_cid, (ushort)TmNativeDefs.TmDataTypes.Rtu, 255,
                                           ref itemsIndexes, (short)channelId, 0, 0);

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
                                                                (ushort)TmNativeDefs.TmDataTypes.Status,
                                                                255,
                                                                ref itemsIndexes,
                                                                (short)channelId,
                                                                (short)rtuId,
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
        
        startIndex = (short)(itemsIndexes[count - 1] + 1);
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
                                                                (ushort)TmNativeDefs.TmDataTypes.Analog,
                                                                255,
                                                                ref itemsIndexes,
                                                                (short)channelId,
                                                                (short)rtuId,
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

        startIndex = (short)(itemsIndexes[count - 1] + 1);
        // todo name, properties?
      }

      return result;
    }

    
    public async Task<IReadOnlyCollection<TmAccum>> GetTmTreeAccums(int channelId, int rtuId)
    {
      if (channelId < 0 || channelId > 254 ||
          rtuId     < 1 || rtuId     > 255)
      {
        return null;
      }

      var   result     = new List<TmAccum>();
      short startIndex = 0;
      while (true)
      {
        var itemsIndexes = new ushort[255];
        var count = await Task.Run(() => _native.TmcEnumObjects(_cid,
                                                                (ushort)TmNativeDefs.TmDataTypes.Accum,
                                                                255,
                                                                ref itemsIndexes,
                                                                (short)channelId,
                                                                (short)rtuId,
                                                                startIndex))
                              .ConfigureAwait(false);
        if (count == 0)
        {
          break;
        }

        for (var i = 0; i < count; i++)
        {
          result.Add(new TmAccum(channelId, rtuId, itemsIndexes[i]));
        }

        startIndex = (short)(itemsIndexes[count - 1] + 1);
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

      var buf = new byte[1024];
      _native.TmcGetObjectName(_cid, (ushort)TmNativeDefs.TmDataTypes.Channel, (short)channelId, 0, 0,
                               ref buf, 1024);
      return EncodingUtil.Win1251BytesToUtf8(buf);
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

      var buf = new byte[1024];
      _native.TmcGetObjectName(_cid, (ushort)TmNativeDefs.TmDataTypes.Rtu, (short)channelId, (short)rtuId, 0,
                               ref buf, 1024);

      return EncodingUtil.Win1251BytesToUtf8(buf);
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
          await Task.Run(() => _native.TmcSetStatusFlags(_cid, ch, rtu, point, (short)flags))
                    .ConfigureAwait(false);
          return;

        case TmAnalog _:
          await Task.Run(() => _native.TmcSetAnalogFlags(_cid, ch, rtu, point, (short)flags))
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
          await Task.Run(() => _native.TmcClrStatusFlags(_cid, ch, rtu, point, (short)flags))
                    .ConfigureAwait(false);
          return;

        case TmAnalog _:
          await Task.Run(() => _native.TmcClrAnalogFlags(_cid, ch, rtu, point, (short)flags))
                    .ConfigureAwait(false);
          return;
      }
    }


    public async Task<IReadOnlyCollection<TmEvent>> GetEventsArchive(TmEventFilter filter)
    {
      if (filter.StartTime == null)
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
      var endTime = _native.UxGmTime2UxTime(DateUtil.GetUtcTimestampFromDateTime(filter.EndTime ?? 
                                              DateTime.Now.AddDays(1)));

      var events = new List<TmEvent>();
      var cache  = new Dictionary<string, TmTag>();

      var criteria = new TmNativeDefs.TEventExCriteria
      {
        ItemsLimit = (uint) filter.OutputLimit,
        HStop = IntPtr.Zero,
        EvlArch = true
      };
      
      const int bufSize      = 1000;
      var       addDataBytes = new byte[bufSize];
      var       i            = 0;

      await Task.Run(() =>
                     {
                       var tEventPtr = _native.TmcEventLogEx(_cid,
                                                             (ushort)filterTypes,
                                                             (uint)startTime,
                                                             (uint)endTime, 
                                                             criteria);

                         if (tEventPtr == IntPtr.Zero)
                         {
                           return;
                         }

                         var curPtr = tEventPtr;

                         while (curPtr != IntPtr.Zero)
                         {
                           var tEventEx = TmNativeUtil.TEventExFromIntPtr(curPtr);
                           
                           _native.TmcEventGetAdditionalRecData((uint)i, ref addDataBytes, bufSize);
                           var addData = TmNativeUtil.GetEventAddData(addDataBytes);

                           var tmEvent = CreateEvent(tEventEx.Event,
                                                     addData,
                                                     tEventEx.EventSize,
                                                     cache);

                           if (filterImportances.HasFlag(tmEvent.ImportanceFlag))
                           {
                             events.Add(tmEvent);
                           }
                           
                           curPtr = tEventEx.Next;
                           i++;
                         }
                         
                         _native.TmcFreeMemory(tEventPtr);

                     }).ConfigureAwait(false);
      
      
      return events;
    }
    

    public async Task<IReadOnlyCollection<TmEvent>> GetEventsArchiveByElix(TmEventFilter filter) // TODO unit test
    {
      if (filter.StartTime == null)
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
      var endTime   = _native.UxGmTime2UxTime(DateUtil.GetUtcTimestampFromDateTime(filter.EndTime ?? 
                                                                                   DateTime.Now.AddDays(1)));

      var events = new List<TmEvent>();
      var elix   = new TmNativeDefs.TTMSElix();
      var cache  = new Dictionary<string, TmTag>();

      while (true)
      {
        var (eventsBatchList, lastElix) = await GetEventsBatchByElix(elix, filterTypes, startTime, endTime, cache)
          .ConfigureAwait(false);

        if (eventsBatchList.IsNullOrEmpty())
        {
          break;
        }

        events.AddRange(eventsBatchList.Where(e => filterImportances.HasFlag(e.ImportanceFlag)));
        if (filter.OutputLimit > 0 &&
            events.Count > filter.OutputLimit)
        {
          events.RemoveRange(filter.OutputLimit, events.Count - filter.OutputLimit);
          break;
        }
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
        var (eventsBatchList, lastBatchElix) = await GetEventsBatchByElix(currentElix,
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
      await Task.Run(() => _native.TmcSetTracer(_cid,
                                                  (short)channel,
                                                  (short)rtu,
                                                  (short)point,
                                                  (ushort)tmType.ToNativeType(),
                                                  (ushort)filterTypes))
                  .ConfigureAwait(false);
    }


    public async Task StopTmAddrTracer(int channel, int rtu, int point, TmType tmType)
    {
        await Task.Run(() => _native.TmcSetTracer(_cid,
                                                  (short)channel,
                                                  (short)rtu,
                                                  (short)point,
                                                  (ushort)tmType.ToNativeType(),
                                                  (ushort)TmTraceTypes.None))
                  .ConfigureAwait(false);
      
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
        Console.WriteLine(
          $"Ошибка при получении списка потоков сервера: {GetLastTmcErrorText().ConfigureAwait(false)}");
        return null;
      }

      var threadList = TmNativeUtil.GetUnknownLengthStringListFromDoubleNullTerminatedPointer(threadsPtr).Select(x =>
                                   {
                                     var regex =
                                       new Regex(@"([0-9]*), (.*?) • ([-+]?[0-9]*) s • ([-+]?[0-9]*\.?[0-9]+) s");
                                     var mc     = regex.Match(x);
                                     var id     = int.Parse(mc.Groups[1].Value);
                                     var name   = mc.Groups[2].Value;
                                     var upTime = int.Parse(mc.Groups[3].Value);
                                     var workTime =
                                       float.Parse(mc.Groups[4].Value, CultureInfo.InvariantCulture);
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

      return (TmAccessRights)access;
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
        return new TmUserInfo((int)userId, tUserInfo, string.Empty);
      }

      Console.WriteLine($"Ошибка получения информации о пользователе с ID {userId}");
      return null;
    }


    public async Task<TmUserInfo> GetExtendedUserInfo(int userId)
    {
      const int bufSize          = 1000;
      var       extendedInfoBuff = new byte[bufSize];
      var       tUserInfo        = new TmNativeDefs.TUserInfo();

      if (await Task.Run(() => _native.TmcGetUserInfoEx(_cid, (uint)userId, ref tUserInfo, ref extendedInfoBuff,
                                                        bufSize)).ConfigureAwait(false))
      {
        return new TmUserInfo(userId, tUserInfo, EncodingUtil.Win1251BytesToUtf8(extendedInfoBuff));
      }

      Console.WriteLine($"Ошибка получения расширенной информации о пользователе с ID {userId}");
      return null;
    }


    public async Task<IReadOnlyCollection<TmStatus>> GetPresentAps()
    {
      var apsTAdrTmListPointer = await Task.Run(() => _native.TmcTakeAPS(_cid)).ConfigureAwait(false);

      if (apsTAdrTmListPointer == IntPtr.Zero)
      {
        Console.WriteLine("Ошибка получения списка взведённых АПС");
        return null;
      }


      var currentPointer = apsTAdrTmListPointer;
      var apsList        = new List<TmStatus>();

      while (true)
      {
        var apsTAdrTm = Marshal.PtrToStructure<TmNativeDefs.TAdrTm>(currentPointer);

        if (apsTAdrTm.Point == 0) break;

        var aps = new TmStatus(apsTAdrTm.Ch, apsTAdrTm.RTU, apsTAdrTm.Point);
        await UpdateStatus(aps).ConfigureAwait(false);
        await UpdateTagPropertiesAndClassData(aps).ConfigureAwait(false);
        apsList.Add(aps);

        currentPointer = IntPtr.Add(currentPointer, Marshal.SizeOf(typeof(TmNativeDefs.TAdrTm)));
      }

      _native.TmcFreeMemory(apsTAdrTmListPointer);

      return apsList;
    }


    public async Task<IReadOnlyCollection<TmTag>> GetTagsByGroup(TmType tmType,
                                                                 string groupName)
    {
      uint count = 0;

      var tmcCommonPointsPtr = await Task.Run(() => _native.TmcGetValuesEx(_cid,
                                                                           (ushort)tmType.ToNativeType(),
                                                                           0,
                                                                           0,
                                                                           0,
                                                                           EncodingUtil.Utf8ToWin1251Bytes(groupName),
                                                                           0,
                                                                           out count))
                                         .ConfigureAwait(false);

      if (tmcCommonPointsPtr == IntPtr.Zero)
      {
        return Array.Empty<TmTag>();
      }

      var tagsList   = new List<TmTag>();
      var structSize = Marshal.SizeOf(typeof(TmNativeDefs.TCommonPoint));

      for (var i = 0; i < count; i++)
      {
        var currentPtr     = new IntPtr(tmcCommonPointsPtr.ToInt64() + i * structSize);
        var tmcCommonPoint = Marshal.PtrToStructure<TmNativeDefs.TCommonPoint>(currentPtr);

        var tag = TmTag.CreateFromTmcCommonPoint(tmcCommonPoint);
        if (tag == null)
        {
          continue;
        }
        tagsList.Add(tag);
      }

      _native.TmcFreeMemory(tmcCommonPointsPtr);

      return tagsList;
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
      var result = await Task.Run(() => _native.TmcSetObjectProperties(_cid, 
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
      await Task.Run(() => _native.TmcSetObjectProperties(_cid,
                                                          tmTag.NativeType,
                                                          ch,
                                                          rtu,
                                                          point,
                                                          propsBytes,
                                                          out _))
                .ConfigureAwait(false);
    }



    public async Task<IReadOnlyCollection<TmTag>> GetTagsByFlags(TmType             tmType,
                                                                 TmFlags            tmFlags,
                                                                 TmCommonPointFlags filterFlags)
    {
      uint count = 0;

      var tmcCommonPointsPtr = await Task.Run(() => _native.TmcGetValuesByFlagMask(_cid,
                                                                                   (ushort)tmType.ToNativeType(),
                                                                                   (uint)tmFlags,
                                                                                   (byte)filterFlags,
                                                                                   out count))
                                         .ConfigureAwait(false);

      if (tmcCommonPointsPtr == IntPtr.Zero)
      {
        return null;
      }

      var tagsList   = new List<TmTag>();
      var structSize = Marshal.SizeOf(typeof(TmNativeDefs.TCommonPoint));

      for (var i = 0; i < count; i++)
      {
        var currentPtr     = new IntPtr(tmcCommonPointsPtr.ToInt64() + i * structSize);
        var tmcCommonPoint = Marshal.PtrToStructure<TmNativeDefs.TCommonPoint>(currentPtr);

        var tag = TmTag.CreateFromTmcCommonPoint(tmcCommonPoint);

        if (tag == null)
        {
          continue;
        }

        tagsList.Add(tag);
      }

      _native.TmcFreeMemory(tmcCommonPointsPtr);

      return tagsList;
    }


    public async Task<IReadOnlyCollection<TmTag>> GetTagsByNamePattern(TmType tmType,
                                                                       string pattern)
    {
      if (pattern.IsNullOrEmpty())
      {
        return Array.Empty<TmTag>();
      }

      uint count = 0;
      var tagTAdrTmListPointer = await Task.Run(() => _native.TmcTextSearch(_cid,
                                                                            (ushort)tmType.ToNativeType(),
                                                                            EncodingUtil.Utf8ToWin1251Bytes(pattern),
                                                                            out count))
                                           .ConfigureAwait(false);

      if (tagTAdrTmListPointer == IntPtr.Zero)
      {
        Console.WriteLine("Ошибка получения списка тэгов по имени");
        return Array.Empty<TmTag>();
      }

      var tags           = new List<TmTag>();
      var currentPointer = tagTAdrTmListPointer;

      for (var i = 0; i < count; i++)
      {
        var tAdrTm = Marshal.PtrToStructure<TmNativeDefs.TAdrTm>(currentPointer);

        if (tAdrTm.Point == 0) continue;

        TmTag tag;

        switch (tmType)
        {
          case TmType.Accum:
            tag = new TmAccum(tAdrTm.Ch, tAdrTm.RTU, tAdrTm.Point);
            break;
          case TmType.Analog:
            tag = new TmAnalog(tAdrTm.Ch, tAdrTm.RTU, tAdrTm.Point);
            break;
          case TmType.Status:
            tag = new TmStatus(tAdrTm.Ch, tAdrTm.RTU, tAdrTm.Point);
            break;
          default:
            tag = null;
            break;
        }

        if (tag != null)
        {
          await UpdateTag(tag).ConfigureAwait(false);
          await UpdateTagPropertiesAndClassData(tag).ConfigureAwait(false);
          tags.Add(tag);
        }

        currentPointer = IntPtr.Add(currentPointer, Marshal.SizeOf(typeof(TmNativeDefs.TAdrTm)));
      }

      _native.TmcFreeMemory(tagTAdrTmListPointer);

      return tags;
    }


    public async Task<IReadOnlyCollection<TmRetroInfo>> GetRetrosInfo(TmType tmType)
    {
      var retrosInfo = new List<TmRetroInfo>();

      await Task.Run(() =>
                {
                  var itemsIndexes = new ushort[64];
                  var count = _native.TmcEnumObjects(_cid, (ushort)tmType.ToNativeType(), 64,
                                                     ref itemsIndexes, 0, 0, 0);

                  for (var i = 0; i < count; i++)
                  {
                    var info = new TmNativeDefs.TRetroInfoEx();
                    if (_native.TmcRetroInfoEx(_cid, itemsIndexes[i], ref info) == TmNativeDefs.Success)
                    {
                      retrosInfo.Add(TmRetroInfo.CreateFromTRetroInfoEx(info));
                    }
                  }
                })
                .ConfigureAwait(false);

      return retrosInfo;
    }


    private async Task<(IReadOnlyList<TmEvent>, TmNativeDefs.TTMSElix)> GetEventsBatchByElix(TmNativeDefs.TTMSElix elix, 
      TmEventTypes type, long startTime, long endTime, Dictionary<string, TmTag> tmTagsCache)
    {
      var lastElix   = elix;
      var eventsList = new List<TmEvent>();
      await Task.Run(() =>
                  {
                    var tmcEventsElixPtr = _native.TmcEventLogByElix(_cid,
                                                                     ref lastElix,
                                                                     (ushort)type,
                                                                     (uint)startTime,
                                                                     (uint)endTime);
                    var i = 0;

                    if (tmcEventsElixPtr == IntPtr.Zero) return;
                    var currentPtr = tmcEventsElixPtr;

                    const int bufSize      = 1000;
                    var       addDataBytes = new byte[bufSize];

                    while (currentPtr != IntPtr.Zero)
                    {
                      var tmcEventElix = TmNativeUtil.EventElixFromIntPtr(currentPtr);

                      _native.TmcEventGetAdditionalRecData((uint)i, ref addDataBytes, bufSize);
                      var addData = TmNativeUtil.GetEventAddData(addDataBytes);

                      var tmEvent = CreateEvent(tmcEventElix.Event,
                                                addData,
                                                tmcEventElix.EventSize,
                                                tmTagsCache,
                                                tmcEventElix.Elix);
                      
                      eventsList.Add(tmEvent);

                      currentPtr = tmcEventElix.Next;
                      i++;
                    }

                    _native.TmcFreeMemory(tmcEventsElixPtr);
                  }
                )
                .ConfigureAwait(false);

      return (eventsList, lastElix);
    }


    private TmEvent CreateEvent(TmNativeDefs.TEvent           tEvent,
                                TmNativeDefs.TTMSEventAddData addData,
                                uint                          eventSize,
                                IDictionary<string, TmTag>    tmTagsCache,
                                TmNativeDefs.TTMSElix?        elix = null)
    {
      TmEvent tmEvent;

      switch ((TmEventTypes)tEvent.Id) 
      {
        case TmEventTypes.StatusChange:
        {
          var tmAddr = new TmAddr(TmType.Status, tEvent.Ch, tEvent.Rtu, tEvent.Point);
          var tag    = GetAndCacheUpdatedTmTagSynchronously(tmAddr, tmTagsCache);

          if (eventSize >= TmNativeDefs.ExtendedStatusChangedEventSize)
          { 
            var data = TmNativeUtil.GetStatusDataExFromTEvent(tEvent);
            tmEvent = TmEvent.CreateStatusChangeExtendedEvent(tEvent, addData, (TmStatus)tag, data, elix);
          }
          else
          {
            var data = TmNativeUtil.GetStatusDataFromTEvent(tEvent);
            tmEvent = TmEvent.CreateStatusChangeEvent(tEvent, addData, (TmStatus)tag, data, elix);
          }
          break;
        }
        case TmEventTypes.Alarm:
        {
          var tmAddr = new TmAddr(TmType.Analog, tEvent.Ch, tEvent.Rtu, tEvent.Point);

          var data          = TmNativeUtil.GetAlarmDataFromTEvent(tEvent);
          var alarmTypeName = GetExtendedObjectName(tmAddr, data.AlarmID, TmNativeDefs.TmDataTypes.AnalogAlarm);

          var tag = GetAndCacheUpdatedTmTagSynchronously(tmAddr, tmTagsCache);

          tmEvent = TmEvent.CreateAlarmTmEvent(tEvent, addData, alarmTypeName, (TmAnalog)tag, data, elix);
          break;
        }
        case TmEventTypes.Control:
        {
          var tmAddr = new TmAddr(TmType.Status, tEvent.Ch, tEvent.Rtu, tEvent.Point);

          var data = TmNativeUtil.GetControlDataFromTEvent(tEvent);

          var tag = GetAndCacheUpdatedTmTagSynchronously(tmAddr, tmTagsCache);

          tmEvent = TmEvent.CreateControlEvent(tEvent, addData, (TmStatus)tag, data, elix);
          break;
        }
        case TmEventTypes.Acknowledge:
        {
          var data       = TmNativeUtil.GetAcknowledgeDataFromTEvent(tEvent);
          var ackTargetName = "";
          if (tEvent.Point != 0)
          {
            var tmAddr = new TmAddr(((TmNativeDefs.TmDataTypes)data.TmType).ToTmType(), 
                                    tEvent.Ch,
                                    tEvent.Rtu,
                                    tEvent.Point);
            ackTargetName = GetObjectName(tmAddr);
          }

          tmEvent = TmEvent.CreateAcknowledgeEvent(tEvent, addData, ackTargetName, data, elix);
          break;
        }
        case TmEventTypes.ManualStatusSet:
        {
          var tmAddr = new TmAddr(TmType.Status, tEvent.Ch, tEvent.Rtu, tEvent.Point);
          var data   = TmNativeUtil.GetControlDataFromTEvent(tEvent);

          var tag = GetAndCacheUpdatedTmTagSynchronously(tmAddr, tmTagsCache);

          tmEvent = TmEvent.CreateManualStatusSetEvent(tEvent, addData, (TmStatus)tag, data, elix);
          break;
        }
        case TmEventTypes.ManualAnalogSet:
        {
          var tmAddr = new TmAddr(TmType.Analog, tEvent.Ch, tEvent.Rtu, tEvent.Point);

          var data = TmNativeUtil.GetAnalogSetDataFromTEvent(tEvent);

          var tag = GetAndCacheUpdatedTmTagSynchronously(tmAddr, tmTagsCache);

          tmEvent = TmEvent.CreateManualAnalogSetEvent(tEvent, addData, (TmAnalog)tag, data, elix);
          break;
        }
        case TmEventTypes.Extended:
        {
          var data = TmNativeUtil.GetStrBinData(tEvent);
          tmEvent = TmEvent.CreateExtendedEvent(tEvent, addData, data, elix);
          break; 
        }
        case TmEventTypes.FlagsChange:
        {
          var flagsChangeData = TmNativeUtil.GetFlagsChangeData(tEvent);
          var sourceType      = (TmNativeDefs.TmDataTypes)flagsChangeData.TmType;

          var tmAddr = new TmAddr(sourceType.ToTmType(), tEvent.Ch, tEvent.Rtu, tEvent.Point);

          switch (sourceType)
          {
            case TmNativeDefs.TmDataTypes.Status:
            {
              var data = TmNativeUtil.GetFlagsChangeDataStatus(tEvent);

              var tag = GetAndCacheUpdatedTmTagSynchronously(tmAddr, tmTagsCache);

              tmEvent = TmEvent.CreateStatusFlagsChangeEvent(tEvent, addData, (TmStatus)tag, data, elix);
              break;
            }
            case TmNativeDefs.TmDataTypes.Analog:
            {
              var data = TmNativeUtil.GetFlagsChangeDataAnalog(tEvent);

              var tag = GetAndCacheUpdatedTmTagSynchronously(tmAddr, tmTagsCache);

              tmEvent = TmEvent.CreateAnalogFlagsChangeEvent(tEvent, addData, (TmAnalog)tag, data, elix);
              break;
            }
            default:
            {
              var sourceAccumName = GetObjectName(tmAddr);

              tmEvent = TmEvent.CreateAccumFlagsChangeEvent(tEvent, addData, sourceAccumName, flagsChangeData, elix);
              break;
            }
          }

          break;
        }
        default:
        {
          tmEvent = TmEvent.CreateFromTEvent(tEvent, addData, "???", elix);
          break; 
        }
      }
      
      return tmEvent;
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

      await Task.Run(() => _native.TmcEventLogAdditionalDataByElixList(_cid, elixList, (uint) elixList.Length))
                .ConfigureAwait(false);

      const int bufSize      = 1000;
      var       extraDataBytes = new byte[bufSize];

      var changesFound = false;
      for (var i = 0; i < tmEvents.Count; i++)
      {
        _native.TmcEventGetAdditionalRecData((uint)i, ref extraDataBytes, bufSize);
        var extraData = TmNativeUtil.GetEventAddData(extraDataBytes);
        
        if (extraData.AckSec != 0)
        {
          tmEvents[i].AckTime = DateUtil.GetDateTimeFromTimestamp(extraData.AckSec, extraData.AckMs);
          // сервер возвращает мусор после первого нуля в имени, нужно обрезать
          tmEvents[i].AckUser = EncodingUtil.Win1251ToUtf8(TmNativeUtil.GetStringFromBytesWithAdditionalPart(extraData.UserName));
          changesFound        = true;
        }
      }

      return changesFound;
    }



    private string GetObjectName(TmAddr tmAddr)
    {
      const int bufSize = 1024;
      var       buf     = new byte[bufSize];

      _native.TmcGetObjectName(_cid,
                               (ushort)tmAddr.Type.ToNativeType(),
                               (short)tmAddr.Ch,
                               (short)tmAddr.Rtu,
                               (short)tmAddr.Point,
                               ref buf,
                               bufSize);

      return EncodingUtil.Win1251BytesToUtf8(buf);
    }


    private string GetExtendedObjectName(TmAddr                   tmAddr,
                                         ushort                   subItemId,
                                         TmNativeDefs.TmDataTypes tmDataType)
    {
      const int bufSize = 1024;
      var       buf     = new byte[bufSize];

      _native.TmcGetObjectNameEx(_cid,
                                 (ushort)tmDataType,
                                 (short)tmAddr.Ch,
                                 (short)tmAddr.Rtu,
                                 (short)tmAddr.Point,
                                 (short)subItemId,
                                 ref buf,
                                 bufSize);

      return EncodingUtil.Win1251BytesToUtf8(buf);
    }


    private void UpdateStatusSynchronously(TmStatus status)
    {
      UpdateStatusesSynchronously(new List<TmStatus> { status });
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

      var tmcCommonPointsPtr = _native.TmcTmValuesByListEx(_cid, (ushort)TmNativeDefs.TmDataTypes.Status, 0,
                                                           (uint)count,
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
      UpdateAnalogsSynchronously(new List<TmAnalog> { analog });
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

      var tmcCommonPointsPtr = _native.TmcTmValuesByListEx(_cid, (ushort)TmNativeDefs.TmDataTypes.Analog, 0,
                                                           (uint)count,
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


    private void UpdateAccumSynchronously(TmAccum accum)
    {
      UpdateAccumsSynchronously(new List<TmAccum> { accum });
    }


    private void UpdateAccumsSynchronously(IReadOnlyList<TmAccum> accums)
    {
      if (accums.IsNullOrEmpty()) return;

      var count       = accums.Count;
      var tmcAddrList = new TmNativeDefs.TAdrTm[count];

      for (var i = 0; i < count; i++)
      {
        tmcAddrList[i] = accums[i].TmAddr.ToAdrTm();
      }

      var tmcCommonPointsPtr = _native.TmcTmValuesByListEx(_cid, (ushort)TmNativeDefs.TmDataTypes.Accum, 0,
                                                           (uint)count,
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
        accums[i].FromTmcCommonPoint(tmcCommonPoint);
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
        UpdateStatusSynchronously((TmStatus)newTag);
      }
      else
      {
        newTag = new TmAnalog(tagTmAddr);
        UpdateAnalogSynchronously((TmAnalog)newTag);
      }

      UpdateTagPropertiesAndClassDataSynchronously(newTag);

      cache.Add(tagTmAddr.ToString(), newTag);

      return newTag;
    }


    private bool MqttPublishSync(MqttPublishTopic topic, byte[] payload)
    {
      if (topic.VariableHeader.IsNullOrEmpty())
      {
        return _native.TmcPubPublish(_cid,
                                     EncodingUtil.Utf8ToWin1251Bytes(topic.Topic),
                                     topic.LifetimeSec,
                                     (byte)topic.QoS,
                                     payload,
                                     (uint)payload.Length);
      }
      else
      {
        var addListPtr = TmNativeUtil.GetDoubleNullTerminatedPointerFromStringList(topic.VariableHeader.Select(
          x => $"{x.Key}={x.Value}"));
        return _native.TmcPubPublishEx(_cid,
                                       EncodingUtil.Utf8ToWin1251Bytes(topic.Topic),
                                       topic.LifetimeSec,
                                       (byte)topic.QoS,
                                       payload,
                                       (uint)payload.Length,
                                       addListPtr);
      }
    }


    private bool MqttSubscribeSync(MqttSubscriptionTopic topic)
    {
      return _native.TmcPubSubscribe(_cid,
                                     EncodingUtil.Utf8ToWin1251Bytes(topic.Topic),
                                     (uint)topic.SubscriptionId,
                                     (byte)topic.QoS);
    }


    private bool MqttUnsubscribeSync(MqttSubscriptionTopic topic)
    {
      return _native.TmcPubUnsubscribe(_cid,
                                       EncodingUtil.Utf8ToWin1251Bytes(topic.Topic),
                                       (uint)topic.SubscriptionId);
    }


    public async Task<bool> MqttSubscribe(MqttSubscriptionTopic topic)
    {
      return await Task.Run(() => MqttSubscribeSync(topic)).ConfigureAwait(false);
    }


    public async Task<bool> MqttSubscribe(MqttKnownTopic topic)
    {
      return await MqttSubscribe(new MqttSubscriptionTopic(topic)).ConfigureAwait(false);
    }


    public async Task<bool> MqttUnsubscribe(MqttSubscriptionTopic topic)
    {
      return await Task.Run(() => MqttUnsubscribeSync(topic)).ConfigureAwait(false);
    }


    public async Task<bool> MqttUnsubscribe(MqttKnownTopic topic)
    {
      return await MqttUnsubscribe(new MqttSubscriptionTopic(topic)).ConfigureAwait(false);
    }
    

    public async Task<bool> MqttPublish(MqttKnownTopic topic, byte[] payload)
    {
      return await MqttPublish(new MqttPublishTopic(topic), payload).ConfigureAwait(false);
    }
    

    public async Task<bool> MqttPublish(MqttKnownTopic topic, string payload = "")
    {
      return await MqttPublish(topic,
                               string.IsNullOrWhiteSpace(payload)
                                 ? Array.Empty<byte>()
                                 : EncodingUtil.Utf8ToWin1251Bytes(payload))
        .ConfigureAwait(false);
    }
    

    public async Task<bool> MqttPublish(MqttPublishTopic topic, string payload)
    {
      return await MqttPublish(topic, EncodingUtil.Utf8ToWin1251Bytes(payload)).ConfigureAwait(false);
    }


    public async Task<bool> MqttPublish(string topic, byte[] payload)
    {
      return await MqttPublish(new MqttPublishTopic(topic), payload).ConfigureAwait(false);
    }
    
    
    public async Task<bool> MqttPublish(MqttPublishTopic topic, byte[] payload)
    {
      return await Task.Run(() => MqttPublishSync(topic, payload)).ConfigureAwait(false);
    }


    public Task<byte[]> MqttInvokeRpc(MqttPublishTopic requestTopic,
                                      byte[]           requestPayload,
                                      int              timeoutSeconds = 5)
    {
      const int responseId    = 656667;                             // случайное число
      var       responseTopic = $"rpc/{Guid.NewGuid().ToString()}"; // случайная строка, ловим ответы только сюда
      
      var tcs = new TaskCompletionSource<byte[]>();

      void WrapperFunc(object sender, MqttMessage message)
      {
        if (message.Payload.IsNullOrEmpty() ||
            string.IsNullOrEmpty(message.Topic) ||
            !message.Topic.Equals(responseTopic, StringComparison.Ordinal))
        {
          return;
        }
        MqttMessageReceived -= WrapperFunc;
        MqttUnsubscribeSync(new MqttSubscriptionTopic(responseTopic, responseId));
        tcs.TrySetResult(message.Payload);
      }
      
      MqttSubscribeSync(new MqttSubscriptionTopic(responseTopic, responseId)); 
      MqttMessageReceived += WrapperFunc;

      var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
      cancellationToken.Token.Register(() =>
                                       {
                                         if (tcs.TrySetCanceled())
                                         {
                                           MqttMessageReceived -= WrapperFunc;
                                           MqttUnsubscribeSync(new MqttSubscriptionTopic(responseTopic, responseId)); 
                                         }
                                       }, useSynchronizationContext: false);

      requestTopic.AddResponseTopic(responseTopic);
      MqttPublishSync(requestTopic, requestPayload);

      return tcs.Task;
    }


    public async Task<byte[]> MqttInvokeRpc(MqttKnownTopic requestTopic,
                                            byte[]         requestPayload,
                                            int            timeoutSeconds = 5)
    {
      return await MqttInvokeRpc(new MqttPublishTopic(requestTopic), requestPayload, timeoutSeconds)
              .ConfigureAwait(false);
    }


    public void NotifyOfMqttMessage(MqttMessage message)
    {
      MqttMessageReceived.Invoke(this, message);
    }
  }
}