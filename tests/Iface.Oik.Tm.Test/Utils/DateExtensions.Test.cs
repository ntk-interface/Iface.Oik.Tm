using System;
using FluentAssertions;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class DateExtensionsTest
  {
    public class ToTmStringMethod
    {
      [Theory]
      [InlineData(17, 01, 2018, 00, 00, 00, "17.01.2018 00:00:00")]
      [InlineData(17, 01, 2018, 12, 23, 34, "17.01.2018 12:23:34")]
      public void ReturnsCorrectValues(int day, int month, int year, int hour, int minute, int second, 
                                       string expected)
      {
        var dateTime = new DateTime(year, month, day, hour, minute, second);

        var result = dateTime.ToTmString();

        result.Should().Be(expected);
      }
    }


    public class IsEpochMethod
    {
      [Fact]
      public void ReturnsTrue()
      {
        var dateTime = new DateTime(1970, 1, 1);

        var result = dateTime.IsEpoch();

        result.Should().BeTrue();
      }
      
      [Fact]
      public void ReturnsFalse()
      {
        var dateTime = new DateTime(2018, 01, 17);

        var result = dateTime.IsEpoch();

        result.Should().BeFalse();
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
        DateTime? dateTime = new DateTime(year, month, day, hour, minute, second);

        var result = dateTime.NullIfEpoch();

        result.Should().Be(dateTime);
      }


      [Fact]
      [UseCulture("ru-RU")]
      public void ReturnsNullForEpoch()
      {
        DateTime? dateTime = new DateTime(1970, 01, 01);
        
        var result = dateTime.NullIfEpoch();

        result.Should().BeNull();
      }
    }
  }
}