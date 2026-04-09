using System;
using System.Buffers;
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
}