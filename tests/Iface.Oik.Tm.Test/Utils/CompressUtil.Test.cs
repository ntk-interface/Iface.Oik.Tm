using System;
using System.Text;
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


  public class Compress
  {
    [Fact]
    public void ReturnsEmpty_WhenBytesAreNull()
    {
      var result = CompressUtil.Compress(default(byte[]));

      result.Should().BeEmpty();
    }


    [Fact]
    public void ReturnsEmpty_WhenBytesAreEmpty()
    {
      var result = CompressUtil.Compress(Array.Empty<byte>());

      result.Should().BeEmpty();
    }


    [Fact]
    public void Roundtrips_ThroughDecompress()
    {
      var original = "Hello, World!"u8.ToArray();

      var compressed   = CompressUtil.Compress(original);
      var decompressed = CompressUtil.Decompress(compressed);

      decompressed.Should().BeEquivalentTo(original, options => options.WithStrictOrdering());
    }
  }


  public class Decompress
  {
    [Fact]
    public void ReturnsEmpty_WhenBytesAreNull()
    {
      var result = CompressUtil.Decompress(null);

      result.Should().BeEmpty();
    }


    [Fact]
    public void ReturnsEmpty_WhenBytesAreEmpty()
    {
      var result = CompressUtil.Decompress(Array.Empty<byte>());

      result.Should().BeEmpty();
    }
  }


  public class GetRawOrDecompressedString
  {
    [Fact]
    public void ReturnsEmpty_WhenBytesAreNull()
    {
      var result = CompressUtil.GetRawOrDecompressedString(null);

      result.Should().BeEmpty();
    }


    [Fact]
    public void ReturnsRawString_ForUncompressedBytes()
    {
      var bytes = "plain text"u8.ToArray();

      var result = CompressUtil.GetRawOrDecompressedString(bytes);

      result.Should().Be("plain text");
    }


    [Fact]
    public void ReturnsDecompressedString_ForCompressedBytes()
    {
      var original   = "compressed payload";
      var compressed = CompressUtil.Compress(Encoding.UTF8.GetBytes(original));

      var result = CompressUtil.GetRawOrDecompressedString(compressed);

      result.Should().Be(original);
    }
  }
}