using System;
using System.Text;

namespace Iface.Oik.Tm.Native.Utils;

public static class DateExtensions
{
  public static string ToTmString(this DateTime dateTime)
  {
    return dateTime.ToString("dd.MM.yyyy HH:mm:ss");
  }

    
  public static byte[] ToNativeByteArray(this DateTime dateTime)
  {
    var result = new byte[24];

    Encoding.UTF8.GetBytes(dateTime.ToTmString(), result.AsSpan());

    return result;
  }
}