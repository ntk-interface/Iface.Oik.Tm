using System;
using System.Text;

namespace Iface.Oik.Tm.Utils
{
  public static class DateExtensions
  {
    public static string ToTmString(this DateTime dateTime)
    {
      return dateTime.ToString("dd.MM.yyyy HH:mm:ss");
    }


    public static byte[] ToTmByteArray(this DateTime dateTime)
    {
      var result = new byte[24];
      
      Encoding.Default.GetBytes(dateTime.ToTmString()).CopyTo(result, 0);
      
      return result;
    }


    public static bool IsEpoch(this DateTime dateTime)
    {
      return dateTime == DateUtil.Epoch;
    }


    public static DateTime? NullIfEpoch(this DateTime? dateTime)
    {
      return DateUtil.NullIfEpoch(dateTime);
    }
  }
}