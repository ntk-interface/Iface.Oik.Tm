using System.Buffers;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static void RegisterTmServerTracer(nint cfCid,
                                            uint processId,
                                            uint threadId,
                                            bool debug,
                                            int  pause)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var result = TmNative.cfsTraceBeginTraceEx(cfCid,
                                                 processId,
                                                 threadId,
                                                 debug,
                                                 (uint)pause,
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

  public static IReadOnlyCollection<T> TraceTmServerLogRecords<T>(nint cfCid)
    where T : TmServerLogRecordBase, new()
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    var records = new List<T>();
    
    try
    {
      while (true)
      {
        var logRecordPtr = TmNative.cfsTraceGetMessage(cfCid,
                                                       out var errCode,
                                                       errBuf,
                                                       TmNativeDefsUnsafe.ErrorBufSize);
        if (logRecordPtr == nint.Zero)
        {
          break;
        }

        if (errCode != 0)
        {
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
        }
        
        var record = TmServerLogRecordBase.Create<T>(logRecordPtr);
        TmNative.cfsFreeMemory(logRecordPtr);
        
        records.Add(record);
      }

      return records;
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  public static void StopTmServerTrace(nint cfCid)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var result = TmNative.cfsTraceEndTrace(cfCid,
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
}