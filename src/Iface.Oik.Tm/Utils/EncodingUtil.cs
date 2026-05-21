using System;
using System.Text;

namespace Iface.Oik.Tm.Utils
{
  public static class EncodingUtil
  {
    public static byte[] StringToBytes(string src)
    {
      if (src == null)
      {
        return null;
      }

      return Encoding.UTF8.GetBytes(src);
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
  }
}