using System;
using FluentAssertions;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class DateUtilTest
  {
    public class EpochProperty
    {
      [Fact]
      public void IsEpoch()
      {
        DateUtil.Epoch.Should().Be(new DateTime(1970, 1, 1));
      }
    }


    public class MinuteProperty
    {
      [Fact]
      public void IsCorrectValueInSeconds()
      {
        DateUtil.Minute.Should().Be(60);
      }
    }


    public class HourProperty
    {
      [Fact]
      public void IsCorrectValueInSeconds()
      {
        DateUtil.Hour.Should().Be(3_600);
      }
    }


    public class DayProperty
    {
      [Fact]
      public void IsCorrectValueInSeconds()
      {
        DateUtil.Day.Should().Be(86_400);
      }
    }


    public class WeekProperty
    {
      [Fact]
      public void IsCorrectValueInSeconds()
      {
        DateUtil.Week.Should().Be(604_800);
      }
    }


    public class YearProperty
    {
      [Fact]
      public void IsCorrectValueInSeconds()
      {
        DateUtil.Year.Should().Be(31_536_000);
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

        result.Should().Be(new DateTime(year, month, day, hour, minute, second, ms));
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

        result.Should().BeNull();
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

        result.Should().Be(new DateTime(year, month, day, hour, minute, second));
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

        result.Should().BeNull();
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

        result.Should().Be(new DateTime(year, month, day, hour, minute, second, ms));
      }


      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(1516191814, 567, 17, 01, 2018, 12, 23, 34, 567)]
      public void ReturnsCorrectValuesWithMs(long timestamp,
                                             int  timestampMs,
                                             int  day, int month, int year, int hour, int minute, int second, int ms)
      {
        var result = DateUtil.GetDateTimeFromTimestamp(timestamp, timestampMs);

        result.Should().Be(new DateTime(year, month, day, hour, minute, second, ms));
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

        result.Should().Be(new DateTime(year, month, day, hour, minute, second, ms));
      }


      [Fact]
      [UseCulture("ru-RU")]
      public void ReturnsNullForEpoch()
      {
        var result = DateUtil.GetDateTimeFromTimestampWithEpochCheck(0, 0);

        result.Should().BeNull();
      }
    }


    public class GetUtcTimestampFromDateTimeMethod
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(17, 01, 2018, 00, 00, 00, 1516147200)]
      [InlineData(17, 01, 2018, 12, 23, 34, 1516191814)]
      public void ReturnsCorrectValues(int  day, int month, int year, int hour, int minute, int second,
                                       long expected)
      {
        var dateTime = DateTime.SpecifyKind(new DateTime(year, month, day, hour, minute, second), DateTimeKind.Utc);
        
        var result = DateUtil.GetUtcTimestampFromDateTime(dateTime);

        result.Should().Be(expected);
      }
    }


    public class NullIfEpochMethod
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(17, 01, 2018, 00, 00, 00)]
      [InlineData(17, 01, 2018, 12, 23, 34)]
      public void ReturnsDateTime(int day, int month, int year, int hour, int minute, int second)
      {
        var dateTime = new DateTime(year, month, day, hour, minute, second);

        var result = DateUtil.NullIfEpoch(dateTime);

        result.Should().Be(dateTime);
      }


      [Fact]
      [UseCulture("ru-RU")]
      public void ReturnsNullForEpoch()
      {
        var dateTime = new DateTime(1970, 01, 01);
        
        var result = DateUtil.NullIfEpoch(dateTime);

        result.Should().BeNull();
      }
    }
  }
}