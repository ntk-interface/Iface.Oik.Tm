using System;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class DateUtilTest
  {
    public class EpochProperty
    {
      [Fact]
      [UseCulture("ru-RU")]
      public void IsEpoch()
      {
        Assert.Equal(new DateTime(1970, 1, 1), DateUtil.Epoch);
      }
    }


    public class MinuteProperty
    {
      [Fact]
      [UseCulture("ru-RU")]
      public void IsCorrectValueInSeconds()
      {
        Assert.Equal(60, DateUtil.Minute);
      }
    }


    public class HourProperty
    {
      [Fact]
      [UseCulture("ru-RU")]
      public void IsCorrectValueInSeconds()
      {
        Assert.Equal(3_600, DateUtil.Hour);
      }
    }


    public class DayProperty
    {
      [Fact]
      [UseCulture("ru-RU")]
      public void IsCorrectValueInSeconds()
      {
        Assert.Equal(86_400, DateUtil.Day);
      }
    }


    public class WeekProperty
    {
      [Fact]
      [UseCulture("ru-RU")]
      public void IsCorrectValueInSeconds()
      {
        Assert.Equal(604_800, DateUtil.Week);
      }
    }


    public class YearProperty
    {
      [Fact]
      [UseCulture("ru-RU")]
      public void IsCorrectValueInSeconds()
      {
        Assert.Equal(31_536_000, DateUtil.Year);
      }
    }


    public class GetDateTimeMethod
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData("17.01.2018 00:00:00",     17, 01, 2018, 00, 00, 00, 00)]
      [InlineData("17.01.2018 12:23:34.567", 17, 01, 2018, 12, 23, 34, 567)]
      public void ReturnsCorrectValues(string s,
                                       int    day, int month, int year, int hour, int minute, int second, int ms)
      {
        var result = DateUtil.GetDateTime(s);

        Assert.Equal(new DateTime(year, month, day, hour, minute, second, ms),
                     result);
      }


      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("aaa")]
      [InlineData("177.01.2018 00:00:00")]
      public void ReturnsNullForInvalidStrings(string s)
      {
        var result = DateUtil.GetDateTime(s);

        Assert.Null(result);
      }
    }


    public class GetDateTimeFromTmStringMethod
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData("17.01.2018 00:00:00", 17, 01, 2018, 00, 00, 00)]
      [InlineData("17.01.2018 12:23:34", 17, 01, 2018, 12, 23, 34)]
      public void ReturnsCorrectValues(string s,
                                       int    day, int month, int year, int hour, int minute, int second)
      {
        var result = DateUtil.GetDateTimeFromTmString(s);

        Assert.Equal(new DateTime(year, month, day, hour, minute, second),
                     result);
      }


      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("aaa")]
      [InlineData("17/01/2018 00:00:00")]
      [InlineData("177.01.2018 00:00:00")]
      public void ReturnsNullForInvalidStrings(string s)
      {
        var result = DateUtil.GetDateTimeFromTmString(s);

        Assert.Null(result);
      }
    }


    public class GetDateTimeFromTimestampMethod
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(1516147200, 17, 01, 2018, 00, 00, 00, 00)]
      public void ReturnsCorrectValuesWithNoMs(long timestamp,
                                               int  day, int month, int year, int hour, int minute, int second, int ms)
      {
        var result = DateUtil.GetDateTimeFromTimestamp(timestamp);

        Assert.Equal(new DateTime(year, month, day, hour, minute, second, ms),
                     result);
      }


      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(1516191814, 567, 17, 01, 2018, 12, 23, 34, 567)]
      public void ReturnsCorrectValuesWithMs(long timestamp,
                                             int  timestampMs,
                                             int  day, int month, int year, int hour, int minute, int second, int ms)
      {
        var result = DateUtil.GetDateTimeFromTimestamp(timestamp, timestampMs);

        Assert.Equal(new DateTime(year, month, day, hour, minute, second, ms),
                     result);
      }
    }


    public class GetDateTimeFromTimestampWithEpochCheckMethod
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(1516191814, 567, 17, 01, 2018, 12, 23, 34, 567)]
      public void ReturnsCorrectValue(long timestamp,
                                      int  timestampMs,
                                      int  day, int month, int year, int hour, int minute, int second, int ms)
      {
        var result = DateUtil.GetDateTimeFromTimestampWithEpochCheck(timestamp, timestampMs);

        Assert.Equal(new DateTime(year, month, day, hour, minute, second, ms),
                     result);
      }


      [Fact]
      [UseCulture("ru-RU")]
      public void ReturnsNullForEpoch()
      {
        var result = DateUtil.GetDateTimeFromTimestampWithEpochCheck(0, 0);
        
        Assert.Null(result);
      }
    }


    public class GetUtcTimestampFromDateTimeMethod
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(17, 01, 2018, 00, 00, 00, 00,  1516129200)]
      [InlineData(17, 01, 2018, 12, 23, 34, 567, 1516173814)]
      public void ReturnsCorrectValues(int  day, int month, int year, int hour, int minute, int second, int ms,
                                       long expected)
      {
        var result = DateUtil.GetUtcTimestampFromDateTime(new DateTime(year, month, day, hour, minute, second));

        Assert.Equal(expected, result);
      }
    }


    public class NullIfEpochMethod
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(17, 01, 2018, 00, 00, 00, 00)]
      [InlineData(17, 01, 2018, 12, 23, 34, 567)]
      public void ReturnsDateTime(int day, int month, int year, int hour, int minute, int second, int ms)
      {
        var dateTime = new DateTime(year, month, day, hour, minute, second);

        var result = DateUtil.NullIfEpoch(dateTime);

        Assert.Equal(dateTime, result);
      }


      [Fact]
      [UseCulture("ru-RU")]
      public void ReturnsNullForEpoch()
      {
        var result = DateUtil.NullIfEpoch(new DateTime(1970, 01, 01));

        Assert.Null(result);
      }
    }
  }
}