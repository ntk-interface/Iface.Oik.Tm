using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
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

          var value = 0;

          if (length > 0 && !Utf8Parser.TryParse(new ReadOnlySpan<byte>(ptr, length), out value, out _))
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
                                                  string uName)
  {
    var                pool   = ArrayPool<byte>.Shared;
    var                errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);
    ReadOnlySpan<char> hex    = "0123456789ABCDEF";

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          ".",
                                          "mac_list",
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
            var pos = 0;
            var mac = new ReadOnlySpan<byte>((byte*)(binPtr + i), 6);

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

            buffer[pos] = '\n';

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

  internal static unsafe bool IfpcGetBinPwd(nint   cfCid,
                                            string uName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          ".",
                                          "pwd",
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var ptr = (byte*)binPtr;

          var value = ptr[0] != 0;

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

  internal static void IfpcSetBinBool(nint   cfCid,
                                      string uName,
                                      string oName,
                                      string binName,
                                      bool   value)
  {
    IfpcSetBinString(cfCid, uName, oName, binName, value ? "1" : "0");
  }

  internal static void IfpcSetBinInt(nint   cfCid,
                                     string uName,
                                     string oName,
                                     string binName,
                                     int    value)
  {
    IfpcSetBinString(cfCid, uName, oName, binName, $"{value}");
  }

  internal static void IfpcSetBinTimestamp(nint     cfCid,
                                           string   uName,
                                           string   oName,
                                           string   binName,
                                           DateTime value)
  {
    var timestampString = value == DateTime.MinValue
                            ? string.Empty
                            : $"{TmNative.uxgmtime2uxtime(NativeDateUtil.GetUtcTimestampFromDateTime(value))}";

    IfpcSetBinString(cfCid, uName, oName, binName, timestampString);
  }

  internal static void IfpcSetBinMacs(nint               cfCid,
                                      string             uName,
                                      ReadOnlySpan<char> value)
  {
    var pool = ArrayPool<byte>.Shared;

    var buf    = pool.Rent((value.Length / 12 + 1) * 6);
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);
    var pos    = 0;

    var        i   = 0;
    Span<byte> mac = stackalloc byte[6];

    try
    {
      while (i < value.Length)
      {
        var macPos = 0;
        var valid  = true;

        while (i < value.Length && value[i] != '\n')
        {
          if (macPos >= 6 || i + 1 >= value.Length)
          {
            valid = false;
            break;
          }

          var hi = TmNativeUtil.Hex(value[i]);
          var lo = TmNativeUtil.Hex(value[i + 1]);

          if (hi < 0 || lo < 0)
          {
            valid = false;
            break;
          }

          mac[macPos++] =  (byte)((hi << 4) | lo);
          i             += 2;

          if (i < value.Length && (value[i] == ':' || value[i] == '-'))
          {
            i++;
          }
        }

        if (i < value.Length && value[i] == '\n')
        {
          i++;
        }

        if (!valid || macPos != 6)
        {
          continue;
        }

        mac.CopyTo(buf.AsSpan(pos));
        pos += 6;
      }

      TmNative.cfsIfpcSetBin(cfCid,
                             uName,
                             ".",
                             "mac_list",
                             buf,
                             (uint)pos,
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
      pool.Return(buf);
      pool.Return(errBuf);
    }
  }

  internal static unsafe void IfpcSetBinString(nint      cfCid,
                                               string    uName,
                                               string    oName,
                                               string    binName,
                                               string    value,
                                               Encoding? encoding = null)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);
    encoding ??= Encoding.UTF8;


    var byteCount = encoding.GetByteCount(value);
    var buf       = pool.Rent(byteCount + 1);

    try
    {
      int written;
      fixed (char* pStr = value)
      fixed (byte* pBuf = buf)
      {
        written = Encoding.UTF8.GetBytes(pStr, value.Length, pBuf, byteCount);
      }

      buf[written] = 0;

      TmNative.cfsIfpcSetBin(cfCid,
                             uName,
                             oName,
                             binName,
                             buf,
                             (uint)written + 1,
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
      ArrayPool<byte>.Shared.Return(buf);
    }
  }

  internal static void IfpcSetBinStrings(nint                        cfCid,
                                         string                      uName,
                                         string                      oName,
                                         string                      binName,
                                         IReadOnlyCollection<string> values,
                                         Encoding?                   encoding = null)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);
    encoding ??= Encoding.UTF8;


    var total = 1; // final null

    foreach (var s in values)
    {
      if (string.IsNullOrEmpty(s))
      {
        continue;
      }
      
      total += encoding.GetByteCount(s) + 1;
    }
    
    var buf       = pool.Rent(total);

    try
    {
      var written = TmNativeUtil.StringsToLpstrListBytes(values, buf, encoding);

      TmNative.cfsIfpcSetBin(cfCid,
                             uName,
                             oName,
                             binName,
                             buf,
                             written,
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
      ArrayPool<byte>.Shared.Return(buf);
    }
  }
}