using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Native.Utils
{
  public static class TmNativeUtil
  {
    public static TmNativeDefs.TStatusPoint GetStatusPointFromCommonPoint(TmNativeDefs.TCommonPoint commonPoint)
    {
      if (commonPoint.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в CommonPoint");
      }

      return FromBytes<TmNativeDefs.TStatusPoint>(commonPoint.Data);
    }


    public static TmNativeDefs.TAnalogPoint GetAnalogPointFromCommonPoint(TmNativeDefs.TCommonPoint commonPoint)
    {
      if (commonPoint.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в CommonPoint");
      }

      return FromBytes<TmNativeDefs.TAnalogPoint>(commonPoint.Data);
    }


    public static TmNativeDefs.TAccumPoint GetAccumPointFromCommonPoint(TmNativeDefs.TCommonPoint commonPoint)
    {
      if (commonPoint.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в CommonPoint");
      }

      return FromBytes<TmNativeDefs.TAccumPoint>(commonPoint.Data);
    }


    public static string[] GetStringListFromDoubleNullTerminatedChars(char[]? chars)
    {
      if (chars == null)
      {
        return Array.Empty<string>();
      }

      var s          = new string(chars);
      int doubleNull = s.IndexOf("\0\0", StringComparison.Ordinal);
      if (doubleNull != -1)
      {
        s = s.Remove(doubleNull);
      }

      return s.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
    }


    public static string[] GetStringListFromDoubleNullTerminatedBytes(byte[]? bytes)
    {
      if (bytes == null)
      {
        return Array.Empty<string>();
      }

      int doubleNull = 0;
      for (var i = 1; i < bytes.Length; i++)
      {
        if (bytes[i] == 0 && bytes[i - 1] == 0)
        {
          doubleNull = i - 1;
          break;
        }
      }

      var significantBytes = new byte[doubleNull];
      Array.Copy(bytes, significantBytes, doubleNull);
      return Encoding.GetEncoding(1251)
                     .GetString(significantBytes)
                     .Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
    }

    public static (IEnumerable<KeyValuePair<string, string>>, byte[]) SplitMqttMessageDatagram(byte[]? datagram)
    {
      if (datagram == null || datagram[0] != 'p' && datagram[1] != 'o')
      {
        return (Enumerable.Empty<KeyValuePair<string, string>>(), Array.Empty<byte>());
      }

      var doubleNull = 0;
      for (var i = 3; i < datagram.Length; i++)
      {
        if (datagram[i] != 0 || datagram[i - 1] != 0) continue;
        doubleNull = i - 1;
        break;
      }

      var infoBytes = new byte[doubleNull];
      Array.Copy(datagram, 2, infoBytes, 0, doubleNull);
      var infoDictionary = Encoding.GetEncoding(1251)
                                   .GetString(infoBytes)
                                   .Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(x =>
                                           {
                                             var item = x.Split('=');
                                             return new KeyValuePair<string, string>(item[0], item[1]);
                                           });

      byte[] payloadBytes;
      var    payloadIndex = doubleNull + 2;

      if (payloadIndex > datagram.Length)
      {
        payloadBytes = Array.Empty<byte>();
      }
      else
      {
        var payloadLength = datagram.Length - payloadIndex;
        payloadBytes = new byte[payloadLength];
        Array.Copy(datagram,
                   payloadIndex,
                   payloadBytes,
                   0,
                   payloadLength);
      }

      return (infoDictionary, payloadBytes);
    }


    public static byte[] GetDoubleNullTerminatedBytesFromStringList(IEnumerable<string>? list,
                                                                    int                  maxSize = 1024)
    {
      if (list == null)
      {
        return Array.Empty<byte>();
      }

      var bytes  = new byte[maxSize];
      var cursor = 0;

      foreach (var str in list)
      {
        var strBytes = Encoding.GetEncoding(1251).GetBytes(str);
        Array.Copy(strBytes, 0, bytes, cursor, strBytes.Length);
        cursor += strBytes.Length;

        bytes[cursor] = 0;
        cursor++;
      }

      var result = new byte[cursor + 1]; // на конце второй ноль
      Array.Copy(bytes, result, cursor);

      return result;
    }


    public static IReadOnlyCollection<string> GetStringListFromDoubleNullTerminatedPointer(nint ptr,
      int                                                                                       maxSize)
    {
      if (ptr == nint.Zero)
      {
        return Array.Empty<string>();
      }

      var result = new List<string>();

      var marshalBytes = new byte[1];

      const int stringBytesSize = 1024;
      var       stringBytes     = new byte[stringBytesSize];

      var stringCursor = 0;
      var isNullFound  = false;
      for (var i = 0; i < maxSize; i++)
      {
        Marshal.Copy(new IntPtr(ptr.ToInt64() + i), marshalBytes, 0, 1);
        if (marshalBytes[0] == 0)
        {
          if (isNullFound) // второй ноль - выходим
          {
            break;
          }

          result.Add(Encoding.GetEncoding(1251)
                             .GetString(stringBytes)
                             .Trim('\0'));
          Array.Clear(stringBytes, 0, stringBytesSize);
          stringCursor = 0;
          isNullFound  = true;
          continue;
        }

        stringBytes[stringCursor++] = marshalBytes[0];
        isNullFound                 = false;
      }

      return result;
    }


    public static nint GetDoubleNullTerminatedPointerFromStringList(IEnumerable<string>? list)
    {
      if (list == null)
      {
        return IntPtr.Zero;
      }

      var byteList = new List<byte>();
      foreach (var item in list)
      {
        var bytes = Encoding.GetEncoding(1251)
                            .GetBytes(item);
        byteList.AddRange(bytes);
        byteList.Add(0);
      }

      byteList.Add(0);

      var handle = GCHandle.Alloc(byteList.ToArray(), GCHandleType.Pinned);
      var ptr    = handle.AddrOfPinnedObject();
      handle.Free();

      return ptr;
    }


    public static byte[] GetFixedBytesWithTrailingZero(string? s, int size, string encoding)
    {
      s ??= string.Empty;

      var result = new byte[size];
      Array.Copy(Encoding.GetEncoding(encoding).GetBytes(s),
                 result,
                 Math.Min(size - 1, s.Length)); // не забыть 0 в конце
      return result;
    }


    public static TmNativeDefs.TTMSEventAddData GetEventAddData(Span<byte> addDataBytes)
    {
      if (addDataBytes == null)
      {
        throw new ArgumentException("Массив байтов пуст");
      }

      unsafe
      {
        fixed (byte* basePtr = addDataBytes)
        {
          var native = *(TmNativeDefsUnsafe.TTMSEventAddData*)basePtr;
          var strPtr = basePtr + sizeof(TmNativeDefsUnsafe.TTMSEventAddData);

          return new TmNativeDefs.TTMSEventAddData
          {
            Elix = new TmNativeDefs.TTMSElix
            {
              M = native.Elix.M,
              R = native.Elix.R
            },
            AckMs    = native.AckMs,
            AckSec   = native.AckSec,
            UserName = GetCStringFromBytePtr(strPtr)
          };
        }
      }
    }

    public static string BytesToString(Span<byte> src, Encoding? encoding = null)
    {
      encoding ??= Encoding.UTF8;

      var len = src.IndexOf((byte)0);
      if (len < 0)
      {
        len = src.Length;
      }

      return encoding.GetString(src[..len]);
    }

    public static byte[] GetBytes<T>(T structure) where T : struct
    {
      int size   = Marshal.SizeOf(structure);
      var bytes  = new byte[size];
      var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
      try
      {
        Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);
      }
      finally
      {
        handle.Free();
      }

      return bytes;
    }


    public static string GetStringFromIntPtrWithAdditionalPart(IntPtr ptr, int size)
    {
      var bytes = new byte[size];
      Marshal.Copy(ptr, bytes, 0, size);
      return GetStringFromBytesWithAdditionalPart(bytes);
    }


    public static string GetStringFromBytesWithAdditionalPart(byte[] bytes, Encoding? encoding = null)
    {
      encoding ??= Encoding.UTF8;

      return encoding
             .GetString(bytes)
             .Split(new[] { '\0' })
             .FirstOrDefault()?
             .Trim('\n');
    }

    public static string GetStringWithUnknownLengthFromIntPtr(nint ptr, Encoding? encoding = null)
    {
      unsafe
      {
        if (ptr == nint.Zero) return string.Empty;

        encoding ??= Encoding.UTF8; // default

        var p      = (byte*)ptr.ToPointer();
        var length = 0;

        while (p[length] != 0)
        {
          length++;
        }

        return encoding.GetString(p, length);
      }
    }


    public static unsafe string GetCStringFromBytePtr(byte* ptr, Encoding? encoding = null)
    {
      if (ptr[0] == 0)
      {
        return string.Empty;
      }

      encoding ??= Encoding.UTF8; // default

      var length = 0;

      while (ptr[length] != 0)
      {
        length++;
      }

      return encoding.GetString(ptr, length);
    }


    public static unsafe IReadOnlyCollection<string> GetStringsListFromIntPtr(nint      ptr,
                                                                              Encoding? encoding = null,
                                                                              int?      limit    = null)
    {
      return ptr == nint.Zero
               ? Array.Empty<string>()
               : GetStringsListBytePtr((byte*)ptr, encoding, limit);
    }


    public static unsafe IReadOnlyCollection<string> GetStringsListFromBytes(Span<byte> bytes,
                                                                             Encoding?  encoding = null,
                                                                             int?       limit    = null)
    {
      fixed (byte* ptr = bytes)
      {
        return GetStringsListBytePtr(ptr, encoding, limit);
      }
    }


    internal static unsafe IReadOnlyCollection<string> GetStringsListBytePtr(byte*     ptr,
                                                                             Encoding? encoding = null,
                                                                             int?      limit    = null)
    {
      var result = new List<string>();

      if (ptr == null)
      {
        return result;
      }
      
      encoding ??= Encoding.UTF8; // default

      var p      = ptr;
      var length = 0;

      while (true)
      {
        if (limit is not null && limit <= result.Count)
        {
          break;
        }
        
        while (p[length] != 0)
        {
          length++;
        }

        result.Add(encoding.GetString(p, length));

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


    public static (IReadOnlyCollection<string> strings, nint nextPtr) GetStringsListWithOffsetPointer(nint ptr,
      Encoding? encoding = null)
    {
      var result = new List<string>();

      unsafe
      {
        if (ptr == nint.Zero)
        {
          return (result, nint.Zero);
        }

        encoding ??= Encoding.UTF8; // default

        var p      = (byte*)ptr.ToPointer();
        var length = 0;

        while (true)
        {
          while (p[length] != 0)
          {
            length++;
          }

          result.Add(encoding.GetString(p, length));
          length++;

          if (p[length] == 0)
          {
            break;
          }

          p      += length;
          length =  0;
        }

        return (result, (nint)p + length + 1);
      }
    }

    public static unsafe Dictionary<string, string> GetDictionaryFromTmBytes(Span<byte> bytes,
                                                                             Encoding?  encoding = null)
    {
      fixed (byte* ptr = bytes)
      {
        return GetDictionaryFromTmBytesPtr(ptr, encoding);
      }
    }

    internal static unsafe Dictionary<string, string> GetDictionaryFromTmBytesPtr(byte* ptr,
      Encoding?                                                                         encoding = null)
    {
      var result = new Dictionary<string, string>();

      if (ptr == null)
      {
        return result;
      }

      encoding ??= Encoding.UTF8;

      var p      = ptr;
      var length = 0;

      while (true)
      {
        while (p[length] != 0)
        {
          length++;
        }

        var line = new Span<byte>(p, length);
        var eq   = line.IndexOf((byte)'=');

        if (eq < 0)
        {
          break;
        }

        var key   = encoding.GetString(line[..eq]);
        var value = encoding.GetString(line[(eq + 1)..]);

        result.Add(key, value);

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

    public static Span<byte> StringToBytes(string src, Encoding? encoding = null)
    {
      encoding ??= Encoding.UTF8;

      return string.IsNullOrEmpty(src)
               ? Span<byte>.Empty
               : encoding.GetBytes(src);
    }


    public static bool PointerValueIsNull(nint ptr)
    {
      if (ptr == nint.Zero)
      {
        throw new ArgumentException("Нулевой указатель");
      }

      unsafe
      {
        var p = (byte*)ptr.ToPointer();

        return p[0] == 0;
      }
    }


    private static T FromBytes<T>(byte[] bytes) where T : struct
    {
      T        structure;
      GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
      try
      {
        structure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
      }
      finally
      {
        handle.Free();
      }

      return structure;
    }

    public static uint IpAddrToNativeDword(string ipAddrString)
    {
      var partsList = new List<uint>();

      foreach (var partStr in ipAddrString.Split('.'))
      {
        if (!uint.TryParse(partStr, out var part))
        {
          return 0;
        }

        partsList.Add(part);
      }

      if (partsList.Count != 4)
      {
        return 0;
      }

      return IpAddrToNativeDword(partsList[0], partsList[1], partsList[2], partsList[3]);
    }

    public static uint IpAddrToNativeDword(uint x1, uint x2, uint x3, uint x4)
    {
      if (x1 > 255 || x2 > 255 || x3 > 255 || x4 > 255)
      {
        return 0;
      }

      var ipAddr = (x4 << 24) + (x3 << 16) + (x2 << 8) + x1;

      if (ipAddr == 0 || ipAddr == 0xffffffff)
      {
        return 0;
      }

      return ipAddr;
    }


    internal static unsafe string BytePtrToString(byte* buffer, int size)
    {
      var span         = new ReadOnlySpan<byte>(buffer, size);
      var len          = span.IndexOf((byte)0);
      if (len < 0) len = size;

      return Encoding.UTF8.GetString(span[..len]);
    }

    internal static unsafe byte[] PtrToArray(byte* ptr, int length)
    {
      if (ptr == null || length <= 0)
      {
        return Array.Empty<byte>();
      }

      var result = new byte[length];

      fixed (byte* dest = result)
      {
        Buffer.MemoryCopy(ptr, dest, length, length);
      }

      return result;
    }


    internal static unsafe uint[] PtrToArray(uint* ptr, int length)
    {
      if (ptr == null || length <= 0)
      {
        return Array.Empty<uint>();
      }

      var result = new uint[length];

      fixed (uint* dest = result)
      {
        Buffer.MemoryCopy(ptr, dest, length, length);
      }

      return result;
    }

    internal static unsafe T FromIntPtr<T>(nint ptr)
      where T : unmanaged
    {
      return FromBytesPtr<T>((byte*)ptr);
    }

    internal static unsafe T FromBytesPtr<T>(byte* ptr)
      where T : unmanaged
    {
      if (ptr == null)
      {
        throw new ArgumentNullException(nameof(ptr));
      }

      return *(T*)ptr;
    }

    public static long GetUtcTimestampFromDateTime(DateTime dateTime)
    {
      return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }

    internal static DateTime GetTimeFromFileTime(TmNativeDefsUnsafe.FileTime fileTime)
    {
      return DateTime.FromFileTime((long)fileTime.dwHighDateTime << 32 | (uint)fileTime.dwLowDateTime);
    }
  }
}