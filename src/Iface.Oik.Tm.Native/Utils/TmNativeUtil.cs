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


    public static string[] GetStringListFromDoubleNullTerminatedChars(char[] chars)
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

      return s.Split(new[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);
    }


    public static string[] GetStringListFromDoubleNullTerminatedBytes(byte[] bytes)
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
                     .Split(new[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);
    }


    public static byte[] GetDoubleNullTerminatedBytesFromStringList(IEnumerable<string> list,
                                                                    int                 maxSize = 1024)
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


    public static IReadOnlyCollection<string> GetStringListFromDoubleNullTerminatedPointer(IntPtr ptr,
                                                                                           int    maxSize)
    {
      if (ptr == IntPtr.Zero)
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


    public static IntPtr GetDoubleNullTerminatedPointerFromStringList(IEnumerable<string> list)
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


    public static byte[] GetFixedBytesWithTrailingZero(string s, int size, string encoding)
    {
      if (s == null)
      {
        s = string.Empty;
      }

      var result = new byte[size];
      Array.Copy(Encoding.GetEncoding(encoding).GetBytes(s),
                 result,
                 Math.Min(size - 1, s.Length)); // не забыть 0 в конце
      return result;
    }


    public static TmNativeDefs.StatusData GetStatusDataFromTEvent(TmNativeDefs.TEvent tEvent)
    {
      if (tEvent.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в TEvent");
      }

      return FromBytes<TmNativeDefs.StatusData>(tEvent.Data);
    }
    
    
    public static TmNativeDefs.StatusDataEx GetStatusDataExFromTEvent(TmNativeDefs.TEvent tEvent)
    {
      if (tEvent.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в TEvent");
      }

      return FromBytes<TmNativeDefs.StatusDataEx>(tEvent.Data);
    }


    public static TmNativeDefs.AlarmData GetAlarmDataFromTEvent(TmNativeDefs.TEvent tEvent)
    {
      if (tEvent.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в TEvent");
      }

      return FromBytes<TmNativeDefs.AlarmData>(tEvent.Data);
    }


    public static TmNativeDefs.AnalogSetData GetAnalogSetDataFromTEvent(TmNativeDefs.TEvent tEvent)
    {
      if (tEvent.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в TEvent");
      }

      return FromBytes<TmNativeDefs.AnalogSetData>(tEvent.Data);
    }


    public static TmNativeDefs.ControlData GetControlDataFromTEvent(TmNativeDefs.TEvent tEvent)
    {
      if (tEvent.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в TEvent");
      }

      return FromBytes<TmNativeDefs.ControlData>(tEvent.Data);
    }


    public static TmNativeDefs.AcknowledgeData GetAcknowledgeDataFromTEvent(TmNativeDefs.TEvent tEvent)
    {
      if (tEvent.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в TEvent");
      }

      return FromBytes<TmNativeDefs.AcknowledgeData>(tEvent.Data);
    }


    public static TmNativeDefs.StrBinData GetStrBinData(TmNativeDefs.TEvent tEvent)
    {
      if (tEvent.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в TEvent");
      }

      return FromBytes<TmNativeDefs.StrBinData>(tEvent.Data);
    }


    public static TmNativeDefs.FlagsChangeData GetFlagsChangeData(TmNativeDefs.TEvent tEvent)
    {
      if (tEvent.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в TEvent");
      }

      return FromBytes<TmNativeDefs.FlagsChangeData>(tEvent.Data);
    }
    
    
    public static TmNativeDefs.FlagsChangeDataStatus GetFlagsChangeDataStatus(TmNativeDefs.TEvent tEvent)
    {
      if (tEvent.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в TEvent");
      }

      return FromBytes<TmNativeDefs.FlagsChangeDataStatus>(tEvent.Data);
    }
    
    
    public static TmNativeDefs.FlagsChangeDataAnalog GetFlagsChangeDataAnalog(TmNativeDefs.TEvent tEvent)
    {
      if (tEvent.Data == null)
      {
        throw new ArgumentException("Отсутствует Data в TEvent");
      }

      return FromBytes<TmNativeDefs.FlagsChangeDataAnalog>(tEvent.Data);
    }


    public static TmNativeDefs.TTMSEventAddData GetEventAddData(byte[] addDataBytes)
    {
      if (addDataBytes == null)
      {
        throw new ArgumentException("Массив байтов пуст");
      }

      return FromBytes<TmNativeDefs.TTMSEventAddData>(addDataBytes);
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


    public static string GetStringFromBytesWithAdditionalPart(byte[] bytes)
    {
      return Encoding.GetEncoding(1251)
                     .GetString(bytes)
                     .Split(new[] {'\0'})
                     .FirstOrDefault()?
                     .Trim('\n');
    }
    
    public static string GetStringWithUnknownLengthFromIntPtr(IntPtr ptr)
    {
      const int bufferStep = 512;
      var       bufSize    = bufferStep;

      var stringBuf    = new byte[bufSize];
      var marshalBytes = new byte[1];

      for (var i = 0; i < bufSize; i++)
      {
        Marshal.Copy(new IntPtr(ptr.ToInt64() + i), marshalBytes, 0, 1);
        if (marshalBytes[0] == 0) break;
        stringBuf[i] = marshalBytes[0];

        if (i != bufSize - 1) continue;

        bufSize += bufferStep;
        var oldStrBuffer = stringBuf;
        stringBuf = new byte[bufSize];
        Array.Copy(oldStrBuffer, stringBuf, oldStrBuffer.Length);
      }

      return Encoding.GetEncoding(1251)
                     .GetString(stringBuf)
                     .Trim('\0');
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
  }
}