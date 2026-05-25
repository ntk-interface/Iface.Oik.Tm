using System;
using System.Buffers;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static (string user, string password) GenerateTokenForExternalApp(nint cfCid)
  {
    const int  tokenLength = 64;
    Span<byte> user        = stackalloc byte[tokenLength];
    Span<byte> password    = stackalloc byte[tokenLength];

    
    var pool = ArrayPool<byte>.Shared;
    var errBuf  = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      TmNative.cfsIfpcGetLogonToken(cfCid,
                                    user,
                                    password,
                                    out var errCode,
                                    errBuf,
                                    TmNativeDefsUnsafe.ErrorBufSize);

      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }

    return (TmNativeUtil.BytesToString(user), TmNativeUtil.BytesToString(password));
  }
  
  public static nint GetCfCid(int tmCid)
  {
    return TmNative.tmcGetCfsHandle(tmCid);
  }
  
  internal static unsafe string GetTextByRef(byte* ptr, int cid)
  {
    const int bufSize = 128;

    switch (ptr[0])
    {
      case 0:
        return string.Empty;
      case 0x40:
      {
        Span<byte> buf = stackalloc byte[bufSize];
        TmNative.tmcGetTextByRef(cid, (nint)ptr, buf, bufSize);

        return TmNativeUtil.BytesToString(buf);
      }
      default:
        return TmNativeUtil.GetCStringFromBytePtr(ptr);
    }
  }
}