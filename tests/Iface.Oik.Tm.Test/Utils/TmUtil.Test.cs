using System;
using FluentAssertions;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class TmUtilTest
  {
    public class GetRetrospectivePreferredStepsMethod
    {
      [Theory]
      [InlineData(1500000000, 1500000000)]
      [InlineData(1500000001, 1500000000)]
      public void ThrowsWhenStartTimeIsGreaterOrEqualToEndTime(long startTime, long endTime)
      {
        Action act = () => TmUtil.GetRetrospectivePreferredStep(startTime, endTime);

        act.Should().Throw<ArgumentException>();
      }


      [Theory]
      [InlineData(1500000000, 1500000000 + DateUtil.Hour,      30)]
      [InlineData(1500000000, 1500000000 + DateUtil.Hour * 4,  DateUtil.Minute)]
      [InlineData(1500000000, 1500000000 + DateUtil.Hour * 8,  DateUtil.Minute * 5)]
      [InlineData(1500000000, 1500000000 + DateUtil.Hour * 12, DateUtil.Minute * 5)]
      [InlineData(1500000000, 1500000000 + DateUtil.Day,       DateUtil.Minute * 10)]
      [InlineData(1500000000, 1500000000 + DateUtil.Week,      DateUtil.Hour)]
      [InlineData(1500000000, 1500000000 + DateUtil.Week * 4,  DateUtil.Hour * 4)]
      [InlineData(1500000000, 1500000000 + DateUtil.Year,      DateUtil.Hour * 12)]
      public void ReturnsCorrectValue(long startTime, long endTime, int expected)
      {
        var result = TmUtil.GetRetrospectivePreferredStep(startTime, endTime);

        result.Should().Be(expected);
      }
    }
  }
}