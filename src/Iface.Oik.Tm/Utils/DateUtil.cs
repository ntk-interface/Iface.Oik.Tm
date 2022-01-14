using System;

namespace Iface.Oik.Tm.Utils
{
  public static class DateUtil
  {
    public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

    public const int Minute = 60;
    public const int Hour   = 60  * Minute;
    public const int Day    = 24  * Hour;
    public const int Week   = 7   * Day;
    public const int Year   = 365 * Day;


    public static DateTime? GetDateTime(string s)
    {
      if (!DateTime.TryParse(s, out DateTime result))
      {
        return null;
      }
      return result;
    }


    public static DateTime? GetDateTimeFromTmString(string s)
    {
      return GetDateTimeFromTmStringByFormat(s, "dd.MM.yyyy HH:mm:ss");
    }


    public static DateTime? GetDateTimeFromExtendedTmString(string s)
    {
      return GetDateTimeFromTmStringByFormat(s, "dd.MM.yyyy HH:mm:ss.fff");
    }


    public static DateTime? GetDateTimeFromReversedTmString(string s)
    {
      return GetDateTimeFromTmStringByFormat(s, "yyyy.MM.dd HH:mm:ss");
    }


    public static DateTime GetDateTimeFromTimestamp(long timestamp, int milliseconds = 0)
    {
      return Epoch.AddSeconds(timestamp).AddMilliseconds(milliseconds);
    }


    public static DateTime? GetDateTimeFromTimestampWithEpochCheck(long timestamp, int milliseconds = 0)
    {
      var dt = Epoch.AddSeconds(timestamp).AddMilliseconds(milliseconds);
      return NullIfEpoch(dt);
    }


    public static long GetUtcTimestampFromDateTime(DateTime dateTime)
    {
      return ((DateTimeOffset) dateTime).ToUnixTimeSeconds();
    }


    public static DateTime? NullIfEpoch(DateTime? dateTime)
    {
      return (dateTime == Epoch) ? null : dateTime;
    }

    private static DateTime? GetDateTimeFromTmStringByFormat(string s, string format)
    {
      if (!DateTime.TryParseExact(s, 
                                  format, 
                                  System.Globalization.CultureInfo.InvariantCulture,
                                  System.Globalization.DateTimeStyles.None,
                                  out DateTime result))
      {
        return null;
      }
      return result;
    }
  }
}