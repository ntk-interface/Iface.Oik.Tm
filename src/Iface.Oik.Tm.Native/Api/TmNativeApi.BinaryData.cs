using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
    internal static unsafe int IfpcGetBinInt(nint   cfCid,
                                          string uName,
                                          string oName,
                                          string binName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var ptr    = (byte*)binPtr;
          var length = 0;

          while (ptr[length] != 0)
          {
            length++;
          }

          if (!Utf8Parser.TryParse(new ReadOnlySpan<byte>(ptr, length), out int value, out _))
          {
            throw new FormatException($"Wrong response format for {uName}{oName}{binName}");
          }

          TmNative.cfsFreeMemory(binPtr);

          return value;
        }
        case 2:
          return 0;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  internal static unsafe uint IfpcGetBinUint(nint   cfCid,
                                          string uName,
                                          string oName,
                                          string binName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var ptr    = (byte*)binPtr;
          var length = 0;

          while (ptr[length] != 0)
          {
            length++;
          }

          if (!Utf8Parser.TryParse(new ReadOnlySpan<byte>(ptr, length), out uint value, out _))
          {
            throw new FormatException($"Wrong response format for {uName}{oName}{binName}");
          }

          TmNative.cfsFreeMemory(binPtr);

          return value;
        }
        case 2:
          return 0;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  
  internal static unsafe long IfpcGetBinLong(nint   cfCid,
                                            string uName,
                                            string oName,
                                            string binName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var ptr    = (byte*)binPtr;
          var length = 0;

          while (ptr[length] != 0)
          {
            length++;
          }

          if (!Utf8Parser.TryParse(new ReadOnlySpan<byte>(ptr, length), out long value, out _))
          {
            throw new FormatException($"Wrong response format for {uName}{oName}{binName}");
          }

          TmNative.cfsFreeMemory(binPtr);

          return value;
        }
        case 2:
          return 0;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  internal static string IfpcBinString(nint   cfCid,
                                         string uName,
                                         string oName,
                                         string binName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var value = TmNativeUtil.GetCStringFromIntPtrAutoEncoding(binPtr);

          TmNative.cfsFreeMemory(binPtr);

          return value;
        }
        case 2:
          return string.Empty;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  internal static unsafe bool IfpcGetBinBool(nint   cfCid,
                                            string uName,
                                            string oName,
                                            string binName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var ptr = (byte*)binPtr;

          var value = ptr[0] == (byte)'1';

          TmNative.cfsFreeMemory(binPtr);

          return value;
        }
        case 2:
          return false;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  internal static unsafe string IfpcGetBinMacList(nint   cfCid,
                                                 string uName,
                                                 string oName,
                                                 string binName)
  {
    var                pool   = ArrayPool<byte>.Shared;
    var                errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);
    ReadOnlySpan<char> hex    = "0123456789ABCDEF";

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out var length,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var        value  = new StringBuilder();
          Span<char> buffer = stackalloc char[18];

          for (var i = 0; i < length; i += 6)
          {
            var mac = new ReadOnlySpan<byte>((byte*)binPtr, 6);

            var pos = 0;

            for (var j = 0; j < 6; j++)
            {
              var b = mac[j];

              buffer[pos++] = hex[b >> 4];
              buffer[pos++] = hex[b & 0xF];

              if (j != 5)
              {
                buffer[pos++] = ':';
              }
            }

            buffer[pos + 1] = '\n';

            value.Append(buffer);
          }

          TmNative.cfsFreeMemory(binPtr);

          return value.ToString();
        }
        case 2:
          return string.Empty;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }
}