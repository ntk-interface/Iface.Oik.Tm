using FluentAssertions;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils;

public class CompressUtilTest
{
  public class IsProbablyCompressed
  {
    [Fact]
    public void ReturnsTrue_ForValidGzipHeader()
    {
      var compressedData = new byte[] { 0x1f, 0x8b, 0x08, 1, 2, 3 }; // GZIP-заголовок и какой-то мусор

      var result = CompressUtil.IsProbablyCompressed(compressedData);

      result.Should().BeTrue();
    }

    [Fact]
    public void ReturnsFalse_ForPlainString()
    {
      var plainData = "Test"u8.ToArray();

      var result = CompressUtil.IsProbablyCompressed(plainData);

      result.Should().BeFalse();
    }
  }
}