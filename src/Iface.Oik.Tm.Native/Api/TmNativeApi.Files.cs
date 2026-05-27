using System;
using System.Buffers;
using System.IO;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static DateTime GetFile(nint   cfCid,
                                 string localFilePath,
                                 string remoteFilePath,
                                 uint   timeout = 20000)
  {
    if (string.IsNullOrEmpty(localFilePath))
    {
      throw new ArgumentException(nameof(localFilePath));
    }

    if (string.IsNullOrEmpty(remoteFilePath))
    {
      throw new ArgumentException(nameof(remoteFilePath));
    }

    var fileTime = new TmNativeDefsUnsafe.FileTime();
    var pool     = ArrayPool<byte>.Shared;
    var errBuf   = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var result = TmNative.cfsFileGet(cfCid,
                                       remoteFilePath,
                                       localFilePath,
                                       timeout | TmNativeDefsUnsafe.FailIfNoConnect,
                                       ref fileTime,
                                       out var errCode,
                                       errBuf,
                                       TmNativeDefsUnsafe.ErrorBufSize);

      if (!result)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      if (!File.Exists(localFilePath))
      {
        throw new FileNotFoundException(localFilePath);
      }

      return NativeDateUtil.GetDateTimeFromCustomFileTime(fileTime);
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  public static string ReadFile(string filePath)
  {
    if (!File.Exists(filePath))
    {
      throw new FileNotFoundException();
    }

    var bytes    = File.ReadAllBytes(filePath);
    var encoding = TmNativeUtil.DetectEncoding(bytes);

    return TmNativeUtil.BytesToString(bytes, encoding);
  }
}