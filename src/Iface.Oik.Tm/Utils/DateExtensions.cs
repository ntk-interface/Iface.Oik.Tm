using System;
using System.Text;

namespace Iface.Oik.Tm.Utils
{
  public static class DateExtensions
  {
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