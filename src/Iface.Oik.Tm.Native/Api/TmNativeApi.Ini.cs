using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static Dictionary<string, string> GetPrivateSection(nint      filePointer,
                                                             string    section,
                                                             Encoding? encoding = null)
  {
    const int maxSectionSize = 0x100000;

    encoding ??= Encoding.UTF8;

    var pool = ArrayPool<byte>.Shared;
    var buf  = pool.Rent(maxSectionSize);

    try
    {
      TmNative.ini_ReadSection(filePointer,
                               section,
                               buf,
                               maxSectionSize);

      return TmNativeUtil.GetDictionaryFromTmBytes(buf, encoding);
    }
    finally
    {
      pool.Return(buf);
    }
  }

  public static string GetIniString(nint   cfCid,
                                    string serverFilePath,
                                    string section,
                                    string key,
                                    string def,
                                    uint   bufSize)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);
    var buf    = pool.Rent((int)bufSize);

    try
    {
      var result = TmNative.cfsGetIniString(cfCid,
                                            serverFilePath,
                                            section,
                                            key,
                                            def,
                                            buf,
                                            out bufSize,
                                            out var errCode,
                                            errBuf,
                                            TmNativeDefsUnsafe.ErrorBufSize);

      if (!result)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
      
      return TmNativeUtil.BytesToString(buf, TmNativeUtil.DetectEncoding(buf));
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  public static void SetIniString(nint   cfCid,
                                  string serverFilePath,
                                  string section,
                                  string key,
                                  string value)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var result = TmNative.cfsSetIniString(cfCid,
                                            serverFilePath,
                                            section,
                                            key,
                                            value,
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