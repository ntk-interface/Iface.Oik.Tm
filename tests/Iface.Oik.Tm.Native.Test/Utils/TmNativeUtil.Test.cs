using System;
using FluentAssertions;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Xunit;

namespace Iface.Oik.Tm.Native.Test.Utils
{
  public class TmNativeUtilTest
  {
    public class GetStatusPointFromCommonPointMethod
    {
      [Fact]
      public void ThrowsArgumentExceptionForDataNull()
      {
        var tmcCommonPoint = new TmNativeDefs.TCommonPoint();

        Action act = () => TmNativeUtil.GetStatusPointFromCommonPoint(tmcCommonPoint);

        act.Should().Throw<ArgumentException>();
      }
    }


    public class GetAnalogPointFromCommonPointMethod
    {
      [Fact]
      public void ThrowsArgumentExceptionForDataNull()
      {
        var tmcCommonPoint = new TmNativeDefs.TCommonPoint();

        Action act = () => TmNativeUtil.GetStatusPointFromCommonPoint(tmcCommonPoint);

        act.Should().Throw<ArgumentException>();
      }
    }


    public class GetFixedBytesWithTrailingZeroMethod
    {
      [Fact]
      public void ReturnsCorrectBytesFromShortString()
      {
        string s = "Aa";

        var result = TmNativeUtil.GetFixedBytesWithTrailingZero(s, 4, "utf-8");

        result.Should().Equal(65, 97, 0, 0); // Aa00
      }


      [Fact]
      public void ReturnsCorrectBytesTrimmingLongString()
      {
        string s = "Aaaaaa";

        var result = TmNativeUtil.GetFixedBytesWithTrailingZero(s, 4, "utf-8");

        result.Should().Equal(65, 97, 97, 0); // Aaa0
      }
    }


    public class GetStringListFromDoubleNullTerminatedChars
    {
      [Fact]
      public void ReturnsEmptyListForNull()
      {
        var result = TmNativeUtil.GetStringListFromDoubleNullTerminatedChars(null);

        result.Should().BeEmpty();
      }
      
      [Fact]
      public void ReturnsEmptyListForNullChars()
      {
        var chars = new char[0];

        var result = TmNativeUtil.GetStringListFromDoubleNullTerminatedChars(chars);

        result.Should().BeEmpty();
      }
      
      [Fact]
      public void ReturnsEmptyListForEmptyChars()
      {
        char[] chars = {'\0'};

        var result = TmNativeUtil.GetStringListFromDoubleNullTerminatedChars(chars);

        result.Should().BeEmpty();
      }
      
      [Fact]
      public void ReturnsCorrectForSingleNotTerminatedChars()
      {
        char[] chars = {'D', 'u', 'm', 'm', 'y'};

        var result = TmNativeUtil.GetStringListFromDoubleNullTerminatedChars(chars);

        result.Should().Equal("Dummy");
      }
      
      [Fact]
      public void ReturnsCorrectForComplexChars()
      {
        char[] chars =
        {
          'T', 'h', 'i', 's', '\0',
          'i', 's', '\0',
          't', 'e', 's', 't', '\0', '\0',
          't', 'r', 'a', 's', 'h',
        };

        var result = TmNativeUtil.GetStringListFromDoubleNullTerminatedChars(chars);

        result.Should().Equal("This", "is", "test");
      }
    }
  }
}