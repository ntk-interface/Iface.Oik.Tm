using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api
{
  public class TmsApi : ITmsApi
  {
    private int        _cid;
    private TmUserInfo _userInfo;


    private event EventHandler<MqttMessage> MqttMessageReceived = delegate { };


    public void SetCidAndUserInfo(int cid, TmUserInfo userInfo)
    {
      _cid = cid;
      SetUserInfo(userInfo);
    }


    public void SetUserInfo(TmUserInfo userInfo)
    {
      _userInfo = userInfo;
    }
    

    public async Task<TmServerComputerInfo> GetServerComputerInfo()
    {
      var dto = await Task.Run(() => TmNativeApi.GetServerComputerInfo(_cid))
                          .ConfigureAwait(false);
      
      return new TmServerComputerInfo(dto);
    }


    public async Task<int> GetLastTmcError()
    {
      return await Task.Run(TmNativeApi.GetLastTmcError)
                       .ConfigureAwait(false);
    }


    public async Task<string> GetLastTmcErrorText()
    {
      return await Task.Run(() => TmNativeApi.GetLastTmcErrorText(_cid))
                       .ConfigureAwait(false);
    }

    
    public string GetConnectionErrorText()
    {
      return TmNativeApi.GetTmcConnectionErrorText(_cid);
    }


    public async Task<DateTime?> GetSystemTime()
    {
      return DateUtil.GetDateTimeFromTmString(await GetSystemTimeString().ConfigureAwait(false));
    }


    public async Task<string> GetSystemTimeString()
    {
      return await Task.Run(() => TmNativeApi.GetSystemTimeString(_cid))
                       .ConfigureAwait(false);
    }

    public async Task<(string host, string server)> GetCurrentServerName()
    {
      return await Task.Run(() => TmNativeApi.GetCurrentTmServerName(_cid))
                       .ConfigureAwait(false);
    }


    public async Task<(string user, string password)> GenerateTokenForExternalApp()
    {
      var cfCid = await GetCfCid().ConfigureAwait(false);

      return await Task.Run(() => GenerateTokenForExternalAppSync(cfCid))
                       .ConfigureAwait(false);
    }

    public (string user, string password) GenerateTokenForExternalAppSync(nint cfCid)
    {
      const int tokenLength = 64;
      Span<byte>       user        = stackalloc byte[tokenLength];
      Span<byte>       password    = stackalloc byte[tokenLength];

      const int errStringLength  = 1000;
      Span<byte>       errString = stackalloc byte[errStringLength];

      TmNative.cfsIfpcGetLogonToken(cfCid,
                                    user,
                                    password,
                                    out uint errCode,
                                    errString,
                                    errStringLength);

      return (EncodingUtil.BytesToString(user), EncodingUtil.BytesToString(password));
    }


    public async Task<IntPtr> GetCfCid()
    {
      return await Task.Run(() => TmNative.tmcGetCfsHandle(_cid))
                       .ConfigureAwait(false);
    }


    public async Task<int> GetStatus(int ch, int rtu, int point)
    {
      return await Task.Run(() => TmNative.tmcStatus(_cid, (short)ch, (short)rtu, (short)point))
                       .ConfigureAwait(false);
    }


    public async Task<int> GetStatusFromRetro(int ch, int rtu, int point, DateTime time)
    {
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);
      
      var statusPoint = new TmNativeDefs.TStatusPoint();

      var isSuccess = await Task.Run(() => TmNative.tmcStatusFullEx(_cid,
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
        var time = TmNative.uxgmtime2uxtime(DateUtil.GetUtcTimestampFromDateTime(currentTime));
        var result = await Task.Run(() => TmNative.tmcStatusFullEx(_cid,
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
      return await Task.Run(() => TmNative.tmcAnalog(_cid, (short)ch, (short)rtu, (short)point, null, 0))
                       .ConfigureAwait(false);
    }


    public async Task<ITmAnalogRetro> GetAnalogFromRetro(int ch, int rtu, int point, DateTime time, int retroNum = 0)
    {
      var analogPoint = new TmNativeDefs.TAnalogPoint();

      var isSuccess = await Task.Run(() => TmNative.tmcAnalogFull(_cid,
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
      var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);
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
      var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);
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

      var fetchResult = await Task.Run(() => TmNative.tmcAnalogMicroSeries(_cid, (uint)count, addrList, bufPtrList))
                                  .ConfigureAwait(false);
      if (fetchResult != TmNativeDefs.Success)
      {
        bufPtrList.ForEach(TmNative.tmcFreeMemory);
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

        TmNative.tmcFreeMemory(bufPtrList[i]);
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
      long startTime = TmNative.uxgmtime2uxtime(utcStartTime);

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
        var result = await Task.Run(() => TmNative.tmcAnalogFull(_cid,
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
                                           TmNative.uxgmtime2uxtime(DateUtil.GetUtcTimestampFromDateTime(time)),
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
      var tmcImpulseArchivePtr = await Task.Run(() => TmNative.tmcAanReadArchive(_cid,
                                                                                analog.TmAddr.ToIntegerWithoutPadding(),
                                                                                (uint)TmNative.uxgmtime2uxtime(startTime),
                                                                                (uint)TmNative.uxgmtime2uxtime(endTime),
                                                                                step,
                                                                                queryFlags,
                                                                                out count,
                                                                                null, 
                                                                                IntPtr.Zero))
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
        TmNative.tmcFreeMemory(tmcImpulseArchivePtr);
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
      var tmcImpulseArchivePtr = await Task.Run(() => TmNative.tmcAanReadArchive(_cid,
                                                                                analog.TmAddr.ToIntegerWithoutPadding(),
                                                                                (uint)TmNative.uxgmtime2uxtime(startTime),
                                                                                (uint)TmNative.uxgmtime2uxtime(endTime),
                                                                                (uint)step,
                                                                                queryFlags,
                                                                                out count,
                                                                                null, 
                                                                                IntPtr.Zero))
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
        TmNative.tmcFreeMemory(tmcImpulseArchivePtr);
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
            var tmcImpulseArchivePtr = await Task.Run(() => TmNative.tmcAanReadArchive(_cid,
                                                                                      analog.TmAddr.ToIntegerWithoutPadding(),
                                                                                      (uint)TmNative.uxgmtime2uxtime(startTime),
                                                                                      (uint)TmNative.uxgmtime2uxtime(endTime),
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
              TmNative.tmcFreeMemory(tmcImpulseArchivePtr);
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

      await Task.Run(() => TmNative.tmcStatusByList(_cid, (ushort)count, tmcAddrList, statusPointsList))
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

      await Task.Run(() => TmNative.tmcAnalogByList(_cid, (ushort)count, tmcAddrList, analogPointsList, time, retroNum))
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
        accums[i].FromTAccumPoint(accumPointsList[i]);
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
      Span<byte> sb = stackalloc byte[1024];
      var (ch, rtu, point) = tag.TmAddr.GetTupleShort();
      TmNative.tmcGetObjectProperties(_cid,
                                      tag.NativeType,
                                      ch,
                                      rtu,
                                      point,
                                      sb,
                                      1024);
      tag.SetTmcObjectProperties(EncodingUtil.BytesToString(sb));
    }


    public async Task CreateTmTagNamedSet(string                     name,
                                          TmType                     tmType,
                                          IReadOnlyCollection<TmTag> tmTags)
    {
      var r = await Task.Run(() => TmNative.tmcTmvUserSetDefine(_cid,
                                                        (ushort)tmType.ToNativeType(),
                                                        EncodingUtil.StringToBytes(name),
                                                        tmTags.Select(t => t.TmAddr.ToIntegerWithoutPadding()).ToArray(),
                                                        (uint) tmTags.Count)).ConfigureAwait(false);
      Console.WriteLine(r);
    }


    public async Task<IReadOnlyCollection<TmStatusRecord>> GetTmStatusNamedSetUpdatedValues(string name)
    {
      var count = 0u;
      var tmcCommonPointsPtr = await Task.Run(() => TmNative.tmcTmvUserSetGet(_cid,
                                                                              (ushort) TmNativeDefs.TmDataTypes.Status,
                                                                              changesOnly: true,
                                                                              EncodingUtil.StringToBytes(name),
                                                                              out count))
                                         .ConfigureAwait(false);
      try
      {
        return TmNativeUtil.ParsePointsFromTmcCommonPointPtr(tmcCommonPointsPtr, (int)count)
                           .Select(TmStatusRecord.CreateFromTmcCommonPoint)
                           .ToList();
      }
      finally
      {
        TmNative.tmcFreeMemory(tmcCommonPointsPtr);
      }
    }


    public async Task<IReadOnlyCollection<TmAnalogRecord>> GetTmAnalogNamedSetUpdatedValues(string name)
    {
      var count = 0u;
      var tmcCommonPointsPtr = await Task.Run(() => TmNative.tmcTmvUserSetGet(_cid,
                                                                              (ushort) TmNativeDefs.TmDataTypes.Analog,
                                                                              changesOnly: true,
                                                                              EncodingUtil.StringToBytes(name),
                                                                              out count))
                                         .ConfigureAwait(false);
      try
      {
        return TmNativeUtil.ParsePointsFromTmcCommonPointPtr(tmcCommonPointsPtr, (int)count)
                           .Select(TmAnalogRecord.CreateFromTmcCommonPoint)
                           .ToList();
      }
      finally
      {
        TmNative.tmcFreeMemory(tmcCommonPointsPtr);
      }
    }


    public async Task<IReadOnlyCollection<TmAccumRecord>> GetTmAccumNamedSetUpdatedValues(string name)
    {
      var count = 0u;
      var tmcCommonPointsPtr = await Task.Run(() => TmNative.tmcTmvUserSetGet(_cid,
                                                                              (ushort) TmNativeDefs.TmDataTypes.Accum,
                                                                              changesOnly: true,
                                                                              EncodingUtil.StringToBytes(name),
                                                                              out count))
                                         .ConfigureAwait(false);
      try
      {
        return TmNativeUtil.ParsePointsFromTmcCommonPointPtr(tmcCommonPointsPtr, (int)count)
                           .Select(TmAccumRecord.CreateFromTmcCommonPoint)
                           .ToList();
      }
      finally
      {
        TmNative.tmcFreeMemory(tmcCommonPointsPtr);
      }
    }


    public async Task DeleteTmTagNamedSet(string name,
                                          TmType tmType)
    {
      var r = await Task.Run(() => TmNative.tmcTmvUserSetDelete(_cid,
                                                                (ushort)tmType.ToNativeType(),
                                                                EncodingUtil.StringToBytes(name))).ConfigureAwait(false);
      Console.WriteLine(r);
    }
    
    
    private async Task<TmTag> FindTmTagReserveTag(TmTag tmTag)
    {
      return await Task.Run(() => FindTmTagReserveTagSync(tmTag))
                       .ConfigureAwait(false);
    }

    private TmTag FindTmTagReserveTagSync(TmTag tmTag)
    {
      Span<byte> sb = stackalloc byte[1024];
      var (ch, rtu, point) = tmTag.TmAddr.GetTupleShort();
      TmNative.tmcGetObjectProperties(_cid,
                                      (ushort)tmTag.Type.ToNativeType(),
                                      ch,
                                      rtu,
                                      point,
                                      sb,
                                      1024);
      
      var props = EncodingUtil.BytesToString(sb).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
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
      var str                = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(singleClassDataPtr);
      TmNative.tmcFreeMemory(classDataPtr);

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
        var classDataPtr = TmNative.tmcGetAnalogClassData(_cid, (uint) chunk.Count, chunk.Select(x => x.TmAddr.ToAdrTm()).ToArray());

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
        var chunk        = source.Take(128).ToList();
        var classDataPtr = TmNative.tmcGetStatusClassData(_cid, (uint) chunk.Count, chunk.Select(x => x.TmAddr.ToAdrTm()).ToArray());

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
        
        TmNative.tmcFreeMemory(classDataPtr);
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
      if (!TmNative.tmcGetAnalogTechParms(_cid, ref tmcAddr, ref techParams))
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
      
      var classDataPtr = await Task.Run(() => TmNative.tmcGetStatusClassData(_cid, tmcAddrsLimit, tmcAddrs))
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
      
      var classDataPtr = await Task.Run(() => TmNative.tmcGetAnalogClassData(_cid, tmcAddrsLimit, tmcAddrs))
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

      var tmcTechObjPropsPtr = await Task.Run(() => TmNative.tmcTechObjReadValues(_cid, nativeTobList, (uint)count))
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

      TmNative.tmcFreeMemory(tmcTechObjPropsPtr);
    }


    public async Task<IReadOnlyCollection<Tob>> GetTechObjects(TobFilter filter)
    {
      uint count            = 0;
      var  filterProperties = TmNativeUtil.GetDoubleNullTerminatedPointerFromStringList(filter?.Properties);
      var tmcTechObjPropsPtr = await Task.Run(() => TmNative.tmcTechObjEnumValues(_cid,
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

      TmNative.tmcFreeMemory(tmcTechObjPropsPtr);

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

      return await Task.Run(() => TmNative.tmcAlertListRemove(_cid,
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

      return await Task.Run(() => TmNative.tmcAlertListRemove(_cid, nativeAlertList))
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

      var scriptResult = await Task.Run(() => TmNative.tmcExecuteControlScript(_cid,
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
        scriptResult = await Task.Run(() => TmNative.tmcExecuteRegulationScript(_cid,
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
      await Task.Run(() => TmNative.tmcOverrideControlScript(_cid, true))
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
          // UserName = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"), // TODO проверить
        }),
      };
      await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
                .ConfigureAwait(false);

      // телеуправление
      var result = await Task.Run(() => TmNative.tmcControlByStatus(_cid,
                                                                    (short)ch,
                                                                    (short)rtu,
                                                                    (short)point,
                                                                    (short)explicitNewStatus))
                             .ConfigureAwait(false);
      if (result <= 0) // если не прошло, регистрируем событие
      {
        ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Result   = (byte)result,
          Cmd      = (byte)explicitNewStatus,
          // UserName = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"), // TODO проверить
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
      await Task.Run(() => TmNative.tmcSetTcPwd(_cid, EncodingUtil.StringToBytes(password))).ConfigureAwait(false);
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
        var result = await Task.Run(() => TmNative.tmcRegulationByAnalog(_cid,
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
        var result = await Task.Run(() => TmNative.tmcRegulationByAnalog(_cid,
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
      await Task.Run(() => TmNative.tmcDriverCall(_cid, 0, (short)TmNativeDefs.DriverCall.Acknowledge, 0))
                .ConfigureAwait(false);
    }


    public async Task AckAllAnalogs()
    {
      await Task.Run(() => TmNative.tmcDriverCall(_cid, 0, (short)TmNativeDefs.DriverCall.AckAnalog, 0))
                .ConfigureAwait(false);
    }


    public async Task<bool> AckStatus(TmStatus status)
    {
      if (status == null) return false;

      var result = await Task.Run(() => TmNative.tmcDriverCall(_cid,
                                                               status.TmAddr.ToInteger(),
                                                               (short)TmNativeDefs.DriverCall.Acknowledge,
                                                               1))
                             .ConfigureAwait(false);
      return result == TmNativeDefs.Success;
    }


    public async Task<bool> AckAnalog(TmAnalog analog)
    {
      if (analog == null) return false;

      var result = await Task.Run(() => TmNative.tmcDriverCall(_cid,
                                                               analog.TmAddr.ToInteger(),
                                                               (short)TmNativeDefs.DriverCall.AckAnalog,
                                                               1))
                             .ConfigureAwait(false);
      return result == TmNativeDefs.Success;
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
      var result = await Task.Run(() => TmNative.tmcEventLogAckRecords(_cid, nativeElixes, (uint) tmEvents.Count))
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
        : TmNative.uxgmtime2uxtime(DateUtil.GetUtcTimestampFromDateTime(time.Value));

      var unixTimeMs = unixTime % 1000 / 10;

      var binaryPayload = binary ?? Array.Empty<byte>();

      await Task.Run(() =>
      {
        TmNative.tmcEvlogPutStrBin(_cid,
                                   (uint)unixTime,
                                   (byte)unixTimeMs,
                                   importance,
                                   sourceLongTag,
                                   EncodingUtil.StringToBytes(message),
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
      await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
                .ConfigureAwait(false);
      
      // переключаем также флаги на резерве, если есть
      var resTmTag = await FindTmTagReserveTag(tmTag).ConfigureAwait(false);
      if (resTmTag != null)
      {
        tvf.Vf.Adr = resTmTag.TmAddr.ToAdrTm();
        await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
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
        // var evOperator = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"); // TODO проверить
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
            // UserName = evOperator, // TODO проверить
          });
          await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
                    .ConfigureAwait(false);
        }

        if (flags.HasFlag(TmFlags.LevelB))
        {
          ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
          {
            Ch       = evCh,
            Rtu      = 2,
            Cmd      = evCommand,
            // UserName = evOperator, // TODO проверить
          });
          await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
                    .ConfigureAwait(false);
        }

        if (flags.HasFlag(TmFlags.LevelC))
        {
          ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
          {
            Ch       = evCh,
            Rtu      = 3,
            Cmd      = evCommand,
            // UserName = evOperator, // TODO проверить
          });
          await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
                    .ConfigureAwait(false);
        }

        if (flags.HasFlag(TmFlags.LevelD))
        {
          ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
          {
            Ch       = evCh,
            Rtu      = 4,
            Cmd      = evCommand,
            // UserName = evOperator, // TODO проверить
          });
          await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
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

      await Task.Run(() => TmNative.tmcSetTimedValues(_cid,
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
          Cmd      = (byte)newStatus,
          // UserName = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"), // TODO проверить
        }),
      };
      await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
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
          Type = (byte)TmNativeDefs.VfType.Status  + 
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
      await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
                .ConfigureAwait(false);
      
      // переключаем также резерв, если есть
      var resStatus = await FindTmTagReserveTag(tmStatus).ConfigureAwait(false);
      if (resStatus != null)
      {
        tvf.Vf.Adr = resStatus.TmAddr.ToAdrTm();
        await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
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
        TmNative.tmcTechObjBeginUpdate(_cid);
        TmNative.tmcTechObjWriteValues(_cid, new[] { tmcProps }, 1);
        TmNative.tmcTechObjEndUpdate(_cid);
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
        TmNative.tmcTechObjBeginUpdate(_cid);
        TmNative.tmcTechObjWriteValues(_cid, tmcProps.ToArray(), (uint)tmcProps.Count);
        TmNative.tmcTechObjEndUpdate(_cid);
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
        TmNative.tmcTechObjBeginUpdate(_cid);
        TmNative.tmcTechObjWriteValues(_cid, new[] { tmcProps }, 1);
        TmNative.tmcTechObjEndUpdate(_cid);
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

      await Task.Run(() => TmNative.tmcSetStatusNormal(_cid, ch, rtu, point, (ushort)normalValue))
                .ConfigureAwait(false);
                
      // переключаем также нормальное состояние резерва, если есть
      var resStatus = await FindTmTagReserveTag(status).ConfigureAwait(false);
      if (resStatus != null)
      {
        var (resCh, resRtu, resPoint) = resStatus.TmAddr.GetTupleShort();
        await Task.Run(() => TmNative.tmcSetStatusNormal(_cid, resCh, resRtu, resPoint, (ushort)normalValue))
                  .ConfigureAwait(false);
      }
    }


    public async Task<int> GetStatusNormal(TmStatus status)
    {
      if (status == null) return -1;

      var (ch, rtu, point) = status.TmAddr.GetTupleShort();

      ushort normalValue = 0xFFFF;
      await Task.Run(() => TmNative.tmcGetStatusNormal(_cid, ch, rtu, point, out normalValue))
                .ConfigureAwait(false);

      return (normalValue == 0 || normalValue == 1) ? normalValue : -1;
    }


    public async Task SetStatus(int ch, int rtu, int point, int status)
    {
      if (status != 0 && status != 1) return;

      await Task.Run(() => TmNative.tmcSetStatus(_cid, (short)ch, (short)rtu, (short)point, (byte)status, null, 0))
                .ConfigureAwait(false);
    }


    public async Task SetAnalog(int ch, int rtu, int point, float value)
    {
      await Task.Run(() => TmNative.tmcSetAnalog(_cid, (short)ch, (short)rtu, (short)point, value, null))
                .ConfigureAwait(false);
    }


    public async Task SetAnalogByCode(int ch, int rtu, int point, int code)
    {
      await Task.Run(() => TmNative.tmcSetAnalogByCode(_cid, (short)ch, (short)rtu, (short)point, (short)code))
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
      var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);
      
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

      var result = await Task.Run(() => TmNative.tmcSetTimedValues(_cid, (uint) tvf.Length, tvf))
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
      var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);

      return await Task.Run(() => TmNative.tmcPerspPutAnalogs(_cid,
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
      await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf })).ConfigureAwait(false);
      
      // выставляем также значение резерву, если есть
      var resAnalogs = await FindTmTagReserveTag(tmAnalog).ConfigureAwait(false);
      if (resAnalogs != null)
      {
        tvf.Vf.Adr = resAnalogs.TmAddr.ToAdrTm();
        await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
                  .ConfigureAwait(false);
      }

      // регистрируем событие
      var ev = new TmNativeDefs.TEvent
      {
        Id  = (ushort)TmNativeDefs.EventTypes.ManualAnalogSet,
        Imp = 0,
        Data = TmNativeUtil.GetBytes(new TmNativeDefs.AnalogSetData
        {
          Cmd      = 1, // флаг ручной установки
          Value    = value,
          // UserName = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"), // TODO проверить
        }),
      };
      (ev.Ch, ev.Rtu, ev.Point) = tmAnalog.TmAddr.GetTuple();

      await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
                .ConfigureAwait(false);

      return true;
    }


    public async Task<bool> SetAnalogBackdateManually(TmAnalog tmAnalog, float value, DateTime time)
    {
      if (tmAnalog == null) return false;
      
      var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
      var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);

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
      await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf })).ConfigureAwait(false);
      
      // выставляем также значение резерву, если есть
      var resAnalogs = await FindTmTagReserveTag(tmAnalog).ConfigureAwait(false);
      if (resAnalogs != null)
      {
        tvf.Vf.Adr = resAnalogs.TmAddr.ToAdrTm();
        await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
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
          // UserName = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"), // TODO проверить
        }),
      };
      (ev.Ch, ev.Rtu, ev.Point) = tmAnalog.TmAddr.GetTuple();

      await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
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
      var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);

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
          Cmd      = (byte)status,
          // UserName = TmNativeUtil.GetFixedBytesWithTrailingZero(_userInfo?.Name, 16, "cp866"), // TODO проверить
        }),
      };
      await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
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
          Type = (byte)TmNativeDefs.VfType.Status  + 
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
      await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
                .ConfigureAwait(false);
      
      // переключаем также резерв, если есть
      var resStatus = await FindTmTagReserveTag(tmStatus).ConfigureAwait(false);
      if (resStatus != null)
      {
        tvf.Vf.Adr = resStatus.TmAddr.ToAdrTm();
        await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
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
      if (!await Task.Run(() => TmNative.tmcGetAnalogTechParms(_cid, ref tmcAddr, ref techParams))
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

      return await Task.Run(() => TmNative.tmcSetAnalogTechParms(_cid, ref tmcAddr, ref techParams))
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
        TmNative.tmcPeekAlarm(_cid, ch, rtu, point, (short)tmAlarm.Id, ref nativeAlarm);

        // установка нового значения
        nativeAlarm.Value = value;
        TmNative.tmcPokeAlarm(_cid, ch, rtu, point, (short)tmAlarm.Id, ref nativeAlarm);
      }).ConfigureAwait(false);

      // регистрируем событие
      var message = $"Изменена уставка \"{tmAlarm.Name}\" на \"{tmAlarm.TmAnalog.Name}\"" +
                    $", новое значение {tmAlarm.TmAnalog.FakeValueWithUnitString(value)}";
      await AddStringToEventLog(message, tmAlarm.TmAnalog.TmAddr).ConfigureAwait(false);

      return true;
    }
    
    
    public async Task SetAccum(int ch, int rtu, int point, float value)
    {
      await Task.Run(() => TmNative.tmcSetAccumValue(_cid, (short)ch, (short)rtu, (short)point, value, null))
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
      if (!await Task.Run(() => TmNative.cfsDirEnum(cfCid,
                                                    EncodingUtil.StringToBytes(path),
                                                    buf,
                                                    bufLength,
                                                    out errCode,
                                                    errString,
                                                    errStringLength))
                     .ConfigureAwait(false))
      {
        Console.WriteLine(
          $"Ошибка при запросе списка файлов: {errCode} - {EncodingUtil.BytesToString(errString)}");
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
	    var       fileTime        = new TmNativeDefs.FileTime();
	    const int errStringLength = 1000;
      var       errString       = new byte[errStringLength];
      uint      errCode         = 0;
      if (!await Task.Run(() => TmNative.cfsFileGet(cfCid,
                                                    EncodingUtil.StringToBytes(remotePath),
                                                    EncodingUtil.StringToBytes(localPath),
                                                    60000,
                                                    ref fileTime,
                                                    out errCode,
                                                    errString,
                                                    errStringLength))
                     .ConfigureAwait(false))
      {
        Console.WriteLine($"Ошибка при скачивании файла: {errCode} - {EncodingUtil.BytesToString(errString)}");
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
      var ptr = await Task.Run(() => TmNative.tmcComtradeEnumDays(_cid)).ConfigureAwait(false);

      return TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptr, 8192);
    }


    public async Task<IReadOnlyCollection<string>> GetComtradeFilesByDay(string day)
    {
      var ptr = await Task.Run(() => TmNative.tmcComtradeEnumFiles(_cid, EncodingUtil.StringToBytes(day)))
                          .ConfigureAwait(false);

      return TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptr, 8192);
    }


    public async Task<bool> DownloadComtradeFile(string filename, string localPath)
    {
      if (!await Task.Run(() => TmNative.tmcComtradeGetFile(_cid, 
                                                            EncodingUtil.StringToBytes(filename), 
                                                            EncodingUtil.StringToBytes(localPath)))
                     .ConfigureAwait(false))
      {
        Console.WriteLine($"Ошибка при скачивании файла: {GetLastTmcError()}");
        return false;
      }

      return true;
    }


    public async Task<string> GetExpressionResult(string expression)
    {
      return await Task.Run(() => GetExpressionResultSync(expression))
                .ConfigureAwait(false);
    }


    public string GetExpressionResultSync(string expression)
    {
      const int bufSize = 1024;

      Span<byte> buf = stackalloc byte[bufSize];
      TmNative.tmcEvaluateExpression(_cid, EncodingUtil.StringToBytes(expression), buf, bufSize);

      return EncodingUtil.BytesToString(buf);
    }


    public async Task<IReadOnlyCollection<TmChannel>> GetTmTreeChannels()
    {
      var result = new List<TmChannel>();

      await Task.Run(() =>
      {
        var itemsIndexes = new ushort[255];
        var count = TmNative.tmcEnumObjects(_cid, (ushort)TmNativeDefs.TmDataTypes.Channel, 255,
                                            itemsIndexes, 0, 0, 0);

        for (int i = 0; i < count; i++)
        {
          var channelId = itemsIndexes[i];
          result.Add(new TmChannel(channelId, GetChannelNameSync(channelId)));
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
        var count = TmNative.tmcEnumObjects(_cid, (ushort)TmNativeDefs.TmDataTypes.Rtu, 255,
                                            itemsIndexes, (short)channelId, 0, 0);

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
        var count = await Task.Run(() => TmNative.tmcEnumObjects(_cid,
                                                                 (ushort)TmNativeDefs.TmDataTypes.Status,
                                                                 255,
                                                                 itemsIndexes,
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
        var count = await Task.Run(() => TmNative.tmcEnumObjects(_cid,
                                                                 (ushort)TmNativeDefs.TmDataTypes.Analog,
                                                                 255,
                                                                 itemsIndexes,
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
        var count = await Task.Run(() => TmNative.tmcEnumObjects(_cid,
                                                                 (ushort)TmNativeDefs.TmDataTypes.Accum,
                                                                 255,
                                                                 itemsIndexes,
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

      Span<byte> buf = stackalloc byte[1024];
      TmNative.tmcGetObjectName(_cid, (ushort)TmNativeDefs.TmDataTypes.Channel, (short)channelId, 0, 0,
                                buf, buf.Length);
      
      return EncodingUtil.BytesToString(buf); 
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

      Span<byte> buf = stackalloc byte[1024];
      TmNative.tmcGetObjectName(_cid, (ushort)TmNativeDefs.TmDataTypes.Rtu, (short)channelId, (short)rtuId, 0,
                                buf, buf.Length);

      return EncodingUtil.BytesToString(buf); 
    }


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


    public async Task SetTagFlagsExplicitly(TmTag tag, TmFlags flags)
    {
      var (ch, rtu, point) = tag.TmAddr.GetTupleShort();

      switch (tag)
      {
        case TmStatus _:
          await Task.Run(() => TmNative.tmcSetStatusFlags(_cid, ch, rtu, point, (short)flags))
                    .ConfigureAwait(false);
          return;

        case TmAnalog _:
          await Task.Run(() => TmNative.tmcSetAnalogFlags(_cid, ch, rtu, point, (short)flags))
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
          await Task.Run(() => TmNative.tmcClrStatusFlags(_cid, ch, rtu, point, (short)flags))
                    .ConfigureAwait(false);
          return;

        case TmAnalog _:
          await Task.Run(() => TmNative.tmcClrAnalogFlags(_cid, ch, rtu, point, (short)flags))
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

      return await Task.Run(() => TmNativeApi.GetEventsArchive<TmEvent>(_cid, filter.ToNative()))
                       .ConfigureAwait(false);
    }

    public TmNativeDefs.TTMSEventAddData GetEventAddRecData(int index)
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


    public async Task StartTmAddrTracer(int channel, int rtu, int point, TmType tmType, TmTraceTypes filterTypes)
    {
      await Task.Run(() => TmNative.tmcSetTracer(_cid,
                                                 (short)channel,
                                                 (short)rtu,
                                                 (short)point,
                                                 (ushort)tmType.ToNativeType(),
                                                 (ushort)filterTypes))
                  .ConfigureAwait(false);
    }


    public async Task StopTmAddrTracer(int channel, int rtu, int point, TmType tmType)
    {
        await Task.Run(() => TmNative.tmcSetTracer(_cid,
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

      var result = await Task.Run(() => TmNative.tmcGetServerInfo(_cid, ref info)).ConfigureAwait(false);

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
      var threadsPtr = await Task.Run(() => TmNative.tmcGetServerThreads(_cid)).ConfigureAwait(false);

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

      TmNative.cfsFreeMemory(threadsPtr);

      return threadList;
    }


    public async Task<TmAccessRights> GetAccessRights()
    {
      uint access = 0;

      await Task.Run(() => TmNative.tmcGetGrantedAccess(_cid, out access)).ConfigureAwait(false);

      return (TmAccessRights)access;
    }


    public async Task<IReadOnlyCollection<TmUserInfo>> GetUsersInfo()
    {
      var usersIdPtr = await Task.Run(() => TmNative.tmcGetUserList(_cid)).ConfigureAwait(false);

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

      var dto = await Task.Run(() => TmNativeApi.GetUserInfo(_cid, userId))
                                .ConfigureAwait(false);

      return new TmUserInfo(dto);
    }


    public async Task<TmUserInfo> GetExtendedUserInfo(int userId)
    {
      var dto = await Task.Run(() => TmNativeApi.GetExtendedUserInfo(_cid, (uint)userId))
                          .ConfigureAwait(false);

       return new TmUserInfo(dto);
    }



    public async Task<IReadOnlyCollection<TmStatus>> GetPresentAps()
    {
      var apsTAdrTmListPointer = await Task.Run(() => TmNative.tmcTakeAPS(_cid)).ConfigureAwait(false);

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

      TmNative.tmcFreeMemory(apsTAdrTmListPointer);

      return apsList;
    }


    public async Task<IReadOnlyCollection<TmTag>> GetTagsByGroup(TmType tmType,
                                                                 string groupName)
    {
      uint count = 0;

      var tmcCommonPointsPtr = await Task.Run(() => TmNative.tmcGetValuesEx(_cid,
                                                                            (ushort)tmType.ToNativeType(),
                                                                            0,
                                                                            0,
                                                                            0,
                                                                            EncodingUtil.StringToBytes(groupName),
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

      TmNative.tmcFreeMemory(tmcCommonPointsPtr);

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



    public async Task<IReadOnlyCollection<TmTag>> GetTagsByFlags(TmType             tmType,
                                                                 TmFlags            tmFlags,
                                                                 TmCommonPointFlags filterFlags)
    {
      uint count = 0;

      var tmcCommonPointsPtr = await Task.Run(() => TmNative.tmcGetValuesByFlagMask(_cid,
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

      TmNative.tmcFreeMemory(tmcCommonPointsPtr);

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
      var tagTAdrTmListPointer = await Task.Run(() => TmNative.tmcTextSearch(_cid,
                                                                             (ushort)tmType.ToNativeType(),
                                                                             EncodingUtil.StringToBytes(pattern),
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

      TmNative.tmcFreeMemory(tagTAdrTmListPointer);

      return tags;
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

      await Task.Run(() => TmNative.tmcEventLogAdditionalDataByElixList(_cid, elixList, (uint) elixList.Length))
                .ConfigureAwait(false);

      const int bufSize      = 1000;
      var       extraDataBytes = new byte[bufSize];

      var changesFound = false;
      for (var i = 0; i < tmEvents.Count; i++)
      {
        //TmNative.tmcEventGetAdditionalRecData((uint)i, ref extraDataBytes, bufSize);
        var extraData = GetEventAddRecData(i);
        
        if (extraData.AckSec != 0)
        {
          tmEvents[i].AckTime = DateUtil.GetDateTimeFromTimestamp(extraData.AckSec, extraData.AckMs);
          // сервер возвращает мусор после первого нуля в имени, нужно обрезать
          tmEvents[i].AckUser = extraData.UserName;
          changesFound        = true;
        }
      }

      return changesFound;
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

      var tmcCommonPointsPtr = TmNative.tmcTMValuesByListEx(_cid, (ushort)TmNativeDefs.TmDataTypes.Status, 0,
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

      TmNative.tmcFreeMemory(tmcCommonPointsPtr);
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

      var tmcCommonPointsPtr = TmNative.tmcTMValuesByListEx(_cid, (ushort)TmNativeDefs.TmDataTypes.Analog, 0,
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

      TmNative.tmcFreeMemory(tmcCommonPointsPtr);
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

      var tmcCommonPointsPtr = TmNative.tmcTMValuesByListEx(_cid, (ushort)TmNativeDefs.TmDataTypes.Accum, 0,
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

      TmNative.tmcFreeMemory(tmcCommonPointsPtr);
    }


    private bool MqttPublishSync(MqttPublishTopic topic, byte[] payload)
    {
      if (topic.VariableHeader.IsNullOrEmpty())
      {
        return TmNative.tmcPubPublish(_cid,
                                      EncodingUtil.StringToBytes(topic.Topic),
                                      topic.LifetimeSec,
                                      (byte)topic.QoS,
                                      payload,
                                      (uint)payload.Length);
      }
      else
      {
        var addListPtr = TmNativeUtil.GetDoubleNullTerminatedPointerFromStringList(topic.VariableHeader.Select(
          x => $"{x.Key}={x.Value}"));
        return TmNative.tmcPubPublishEx(_cid,
                                        EncodingUtil.StringToBytes(topic.Topic),
                                        topic.LifetimeSec,
                                        (byte)topic.QoS,
                                        payload,
                                        (uint)payload.Length,
                                        addListPtr);
      }
    }


    private bool MqttSubscribeSync(MqttSubscriptionTopic topic)
    {
      return TmNative.tmcPubSubscribe(_cid,
                                      EncodingUtil.StringToBytes(topic.Topic),
                                      (uint)topic.SubscriptionId,
                                      (byte)topic.QoS);
    }


    private bool MqttUnsubscribeSync(MqttSubscriptionTopic topic)
    {
      return TmNative.tmcPubUnsubscribe(_cid,
                                        EncodingUtil.StringToBytes(topic.Topic),
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
                                 : EncodingUtil.StringToBytes(payload))
        .ConfigureAwait(false);
    }
    

    public async Task<bool> MqttPublish(MqttPublishTopic topic, string payload)
    {
      return await MqttPublish(topic, EncodingUtil.StringToBytes(payload)).ConfigureAwait(false);
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