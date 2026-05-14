using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Iface.Oik.Tm.Utils
{
  public static class EncodingUtil
  {
    public const string Cp1251 = "windows-1251";


    public static byte[] StringToBytes(string src)
    {
      if (src == null)
      {
        return null;
      }

      return Encoding.UTF8.GetBytes(src);
    }


    public static string Win1251ToUtf8(string src) // TODO кодировка удалить
    {
      if (src == null)
      {
        return null;
      }
      var win1251 = Encoding.GetEncoding(1251);
      return BytesToString(win1251.GetBytes(src));
    }


    public static string BytesToString(Span<byte> src, Encoding encoding = null)
    {
      encoding ??= Encoding.UTF8;
      
      if (src == null)
      {
        return null;
      }

      var len = src.IndexOf((byte)0);
      if (len < 0)
      {
        len = src.Length;
      }

      return encoding.GetString(src[..len]);
    }

    public static string Win1251IntPtrToUtf8(IntPtr stringPtr) // TODO кодировка
    {
      var win1251Str = Marshal.PtrToStringAnsi(stringPtr);
      if (win1251Str == null)
      {
        return null;
      }
      return Win1251ToUtf8(win1251Str);
    }


    public static string Cp866ToUtf8(string src) // TODO кодировка удалить
    {
      if (src == null)
      {
        return null;
      }
      var cp866 = Encoding.GetEncoding(866);
      return Cp866BytesToUtf8(cp866.GetBytes(src));
    }


    public static string Cp866BytesToUtf8(byte[] src) // TODO кодировка
    {
      if (src == null)
      {
        return null;
      }
      var utf8 = Encoding.UTF8;
      var cp866 = Encoding.GetEncoding(866);
      return utf8.GetString(Encoding.Convert(cp866, utf8, src)).Trim('\0');
    }
  }
}