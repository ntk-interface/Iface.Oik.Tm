using System;
using System.Buffers;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static ComputerInfoDto GetServerComputerInfo(int tmCid)
  {
    var cfCid = TmNative.tmcGetCfsHandle(tmCid);

    if (cfCid == nint.Zero)
    {
      throw new TmNativeException("Не удалось получить cfsHandle");
    }

    return GetServerComputerInfo(cfCid);
  }

  public static ComputerInfoDto GetServerComputerInfo(nint cfCid)
  {
    return ComputerInfoDto.Create(GetComputerInfoS(cfCid));
  }

  public static string GetSystemTimeString(int tmCid)
  {
    Span<byte> tmcTime = stackalloc byte[80];
    TmNative.tmcSystemTime(tmCid, tmcTime, nint.Zero);
    return TmNativeUtil.BytesToString(tmcTime);
  }
  
  public static (string host, string server) GetCurrentTmServerName(int tmCid)
  {
    const int  bufSize = 255;
    Span<byte> host    = stackalloc byte[bufSize];
    Span<byte> server  = stackalloc byte[bufSize];

    // todo al сейчас всегда приходит 0
    /*if (!TmNative.tmcGetCurrentServer(_cid, ref host, bufSize, ref server, bufSize))
    {
      return (string.Empty, string.Empty);
    }*/
    TmNative.tmcGetCurrentServer(tmCid, host, bufSize, server, bufSize);
    
    return (TmNativeUtil.BytesToString(host), TmNativeUtil.BytesToString(server));
  }

  public static IReadOnlyCollection<T> GetTmServersThreads<T>(nint cfCid)
    where T : TmServerThreadBase, new()
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var ptr = TmNative.cfsEnumThreads(cfCid, out var errCode, errBuf, TmNativeDefsUnsafe.ErrorBufSize);

      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      var threads = GetTmServersThreadsFromPtr<T>(ptr);
      TmNative.cfsFreeMemory(ptr);

      return threads;
    }
    finally
    {
      pool.Return(errBuf);
    }
  }
  
  public static IReadOnlyCollection<T> GetTmServersThreads<T>(int tmCid)
    where T : TmServerThreadBase, new()
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var ptr = TmNative.tmcGetServerThreads(tmCid);

      if (ptr == nint.Zero)
      {
        throw new TmNativeException(GetLastTmcErrorText(tmCid));
      }

      var threads = GetTmServersThreadsFromPtr<T>(ptr);
      TmNative.cfsFreeMemory(ptr);

      return threads;
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  public static string GetBasePath(nint cfCid)
  {
    const int pathBufSize = 1000;
    
    var pool    = ArrayPool<byte>.Shared;
    var pathBuf = pool.Rent(pathBufSize);
    var errBuf  = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var result = TmNative.cfsGetBasePath(cfCid,
                                           pathBuf,
                                           pathBufSize,
                                           out var errCode,
                                           errBuf,
                                           TmNativeDefsUnsafe.ErrorBufSize);
      
      
      if (!result)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      return TmNativeUtil.BytesToString(pathBuf);
    }
    finally
    {
      pool.Return(pathBuf);
      pool.Return(errBuf);
    }
  }
  
  internal static unsafe TmNativeDefsUnsafe.ComputerInfoS GetComputerInfoS(nint cfCid)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    var cis = new TmNativeDefsUnsafe.ComputerInfoS
    {
      Len = (uint)sizeof(TmNativeDefsUnsafe.ComputerInfoS)
    };

    try
    {
      if (!TmNative.cfsGetComputerInfo(cfCid,
                                       ref cis,
                                       out var errCode,
                                       errBuf,
                                       TmNativeDefsUnsafe.ErrorBufSize))
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }

    return cis;
  }
  
  internal static unsafe IReadOnlyCollection<T> GetTmServersThreadsFromPtr<T>(nint listPtr)
    where T : TmServerThreadBase, new()
  {
    var result = new List<T>();
    

    if (listPtr == nint.Zero)
    {
      return result;
    }

    var p      = (byte*)listPtr;
    var length = 0;

    while (true)
    {
      while (p[length] != 0)
      {
        length++;
      }

      var span = new Span<byte>(p, length);

      if (span.IndexOf((byte)',') != -1)
      {
        result.Add(TmServerThreadBase.Create<T>(span)); 
      }

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
}