using System;
using FluentAssertions;
using Iface.Oik.Tm.Native.Utils;
using Xunit;

namespace Iface.Oik.Tm.Native.Test.Utils;

public class DateExtensionsTest
{
  public class ToNativeByteArray
  {
    [Fact]
    public void ReturnsExactly24Bytes()
    {
      var dateTime = new DateTime(2026, 5, 20, 13, 28, 0);

      var result = dateTime.ToNativeByteArray();

      result.Should().HaveCount(24);
    }
    
    
    [Fact]
    public void ReturnsCorrectBytesWithTrailingNulls()
    {
      var dateTime = new DateTime(2026, 5, 20, 13, 28, 0);
      var expected = new byte[24]
      {
        (byte)'2', (byte)'0', (byte)'.', (byte)'0', (byte)'5', (byte)'.', (byte)'2', (byte)'0', (byte)'2', (byte)'6',
        (byte)' ',                                                                                                  
        (byte)'1', (byte)'3', (byte)':', (byte)'2', (byte)'8', (byte)':', (byte)'0', (byte)'0',
        0, 0, 0, 0, 0,
      };

      var result = dateTime.ToNativeByteArray();

      result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }
  }
}