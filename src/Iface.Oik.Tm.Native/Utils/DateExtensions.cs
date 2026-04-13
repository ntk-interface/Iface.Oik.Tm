using System;

namespace Iface.Oik.Tm.Native.Utils;

public static class DateExtensions
{
  public static string ToTmString(this DateTime dateTime)
  {
    return dateTime.ToString("dd.MM.yyyy HH:mm:ss");
  }

    
  public static Span<byte> ToNativeByteArray(this DateTime dateTime)
  {
    var result = new byte[24];
    TmNativeUtil.StringToBytes(dateTime.ToTmString())[..24].CopyTo(result);
    return result;
  }
}