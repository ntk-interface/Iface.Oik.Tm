using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using FluentAssertions;
using Iface.Oik.Tm.Native.Utils;
using Xunit;

namespace Iface.Oik.Tm.Native.Test.Utils
{
  public class TmNativeUtilTest
  {
    public class GetDoubleNullTerminatedBytesFromStringList
    {
      [Fact]
      public void ReturnsEmptyBytes_WhenSourceIsEmpty()
      {
        var result = TmNativeUtil.GetDoubleNullTerminatedBytesFromStringList(Array.Empty<string>());

        result.Should().BeEmpty();
      }
      
      
      [Fact]
      public void ReturnsCorrectBytes_ForSingleString()
      {
        var list     = new[] { "Test" };
        var expected = new byte[] { (byte)'T', (byte)'e', (byte)'s', (byte)'t', 0, 0 };
        
        var result = TmNativeUtil.GetDoubleNullTerminatedBytesFromStringList(list);

        result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
      }
      
      
      [Fact]
      public void ReturnsCorrectBytes_ForMultipleStrings()
      {
        var list = new[] { "A=12", "BC=3" };
        var expected = new byte[]
        {
          (byte)'A', (byte)'=', (byte)'1', (byte)'2', 0,
          (byte)'B', (byte)'C', (byte)'=', (byte)'3', 0, 0
        };
        
        var result = TmNativeUtil.GetDoubleNullTerminatedBytesFromStringList(list);

        result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
      }
    }


    public class AllocateDoubleNullTerminatedPointerFromStringList
    {
      [Fact]
      public void ReturnsZeroPointer_WhenSourceIsEmpty()
      {
        var result = TmNativeUtil.AllocateDoubleNullTerminatedPointerFromStringList(Array.Empty<string>());

        result.Should().Be(nint.Zero);
      }
      
      
      [Fact]
      public void AllocatedCorrectPointer_ForValidStrings()
      {
        var list = new[] { "A=12", "BC=3" };
        var expectedBytes = new byte[]
        {
          (byte)'A', (byte)'=', (byte)'1', (byte)'2', 0,
          (byte)'B', (byte)'C', (byte)'=', (byte)'3', 0, 0
        };
        
        var ptr = TmNativeUtil.AllocateDoubleNullTerminatedPointerFromStringList(list);
        
        ptr.Should().NotBe(nint.Zero);

        try
        {
          var actualBytes = new byte[expectedBytes.Length];
          Marshal.Copy(ptr, actualBytes, 0, expectedBytes.Length);

          actualBytes.Should().BeEquivalentTo(expectedBytes, options => options.WithStrictOrdering());
        }
        finally
        {
          if (ptr != nint.Zero)
          {
            Marshal.FreeHGlobal(ptr);
          }
        }
      }
    }
    
    
    public class GetStringListFromDoubleNullTerminatedPointer
    {
      [Fact]
      public void ReturnsEmptyCollection_WhenPointerIsZero()
      {
        var result = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(nint.Zero, 1024);

        result.Should().BeEmpty();
      }

      [Fact]
      public void ReturnsCorrectStrings_ForValidPointer()
      {
        var sourceBytes = new byte[]
        {
          (byte)'A', (byte)'=', (byte)'1', (byte)'2', 0,
          (byte)'B', (byte)'C', (byte)'=', (byte)'3', 0, 0
        };

        var ptr = Marshal.AllocHGlobal(sourceBytes.Length);
        Marshal.Copy(sourceBytes, 0, ptr, sourceBytes.Length);

        try
        {
          var result = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptr, sourceBytes.Length);

          result.Should().Equal("A=12", "BC=3");
        }
        finally
        {
          if (ptr != nint.Zero)
          {
            Marshal.FreeHGlobal(ptr);
          }
        }
      }
    }
    
    
    public class GetBytes
    {
      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct TestDummy
      {
        public byte Id;    // 1 байт
        public int  Value; // 4 байта
      }

      [Fact]
      public void GetBytes_ReturnsArrayWithCorrectSize()
      {
        var dummy        = new TestDummy { Id = 1, Value = 100 };
        var expectedSize = Marshal.SizeOf<TestDummy>(); // ожидаем 5 байт

        var result = TmNativeUtil.GetBytes(dummy);

        result.Should().HaveCount(expectedSize);
      }

      [Fact]
      public void GetBytes_ReturnsCorrectBytes()
      {
        var dummy         = new TestDummy { Id = 0xAA, Value = 0x000000BB };
        var expectedBytes = new byte[] { 0xAA, 0xBB, 0x00, 0x00, 0x00 }; // little-endian

        var result = TmNativeUtil.GetBytes(dummy);

        result.Should().BeEquivalentTo(expectedBytes, options => options.WithStrictOrdering());
      }
    }
    
    
    public class BytesToString
    {
      public BytesToString()
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      }
      
      [Fact]
      public void ReturnsEmptyString_WhenSpanIsEmpty()
      {
        var result = TmNativeUtil.BytesToString(Span<byte>.Empty);

        result.Should().BeEmpty();
      }

      [Fact]
      public void ReturnsFullString_WhenNoNullTerminatorFound()
      {
        var sourceBytes = new[] { (byte)'T', (byte)'e', (byte)'s', (byte)'t' };

        var result = TmNativeUtil.BytesToString(sourceBytes);

        result.Should().Be("Test");
      }

      [Fact]
      public void CutsStringAtFirstNullTerminator_WhenNullExists()
      {
        var sourceBytes = new byte[]
        {
          (byte)'T', (byte)'e', (byte)'s', (byte)'t',
          0,
          (byte)'T', (byte)'r', (byte)'a', (byte)'s', (byte)'h' // мусор
        };

        var result = TmNativeUtil.BytesToString(sourceBytes);

        result.Should().Be("Test");
      }

      [Fact]
      public void ReturnsEmptyString_WhenFirstByteIsNull()
      {
        var sourceBytes = new byte[] { 0, (byte)'A', (byte)'B' };

        var result = TmNativeUtil.BytesToString(sourceBytes);

        result.Should().BeEmpty();
      }

      [Fact]
      public void UsesSpecifiedEncoding_WhenPassedExplicitly()
      {
        var encoding1251 = Encoding.GetEncoding(1251);
        var sourceBytes  = encoding1251.GetBytes("Привет\0");

        var result = TmNativeUtil.BytesToString(sourceBytes, encoding1251);

        result.Should().Be("Привет");
      }
    }
    
    
    public class StringToBytes
    {
      public StringToBytes()
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      }
      
      [Fact]
      public void ReturnsEmptyArray_WhenStringIsNullOrEmpty()
      {
        var resultNull  = TmNativeUtil.StringToBytes(null);
        var resultEmpty = TmNativeUtil.StringToBytes("");

        resultNull.Should().BeEmpty();
        resultEmpty.Should().BeEmpty();
      }

      [Fact]
      public void ReturnsCorrectBytes_ForUtf8String()
      {
        var input    = "Test";
        var expected = new[] { (byte)'T', (byte)'e', (byte)'s', (byte)'t' };

        var result = TmNativeUtil.StringToBytes(input);

        result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
      }

      [Fact]
      public void UsesSpecifiedEncoding_WhenPassedExplicitly()
      {
        var input        = "Тест";
        var encoding1251 = Encoding.GetEncoding(1251);
        var expected     = encoding1251.GetBytes(input);

        var result = TmNativeUtil.StringToBytes(input, encoding1251);

        result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
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
      public void ReturnsEmptyListForEmptyChars()
      {
        var chars = new char[0];

        var result = TmNativeUtil.GetStringListFromDoubleNullTerminatedChars(chars);

        result.Should().BeEmpty();
      }
      
      [Fact]
      public void ReturnsEmptyListForSingleNullChar()
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


    public class GetCStringFromIntPtr
    {
      [Fact]
      public void ReturnsEmptyString_WhenPointerIsZero()
      {
        var result = TmNativeUtil.GetCStringFromIntPtr(nint.Zero);

        result.Should().BeEmpty();
      }

      [Fact]
      public void ReturnsEmptyString_WhenFirstByteIsNull()
      {
        var ptr = Marshal.AllocHGlobal(4);
        Marshal.WriteByte(ptr, 0, 0);

        try
        {
          var result = TmNativeUtil.GetCStringFromIntPtr(ptr);

          result.Should().BeEmpty();
        }
        finally
        {
          Marshal.FreeHGlobal(ptr);
        }
      }

      [Fact]
      public void ReturnsCorrectString_ForValidNullTerminatedPtr()
      {
        var bytes = "Hello\0"u8.ToArray();
        var ptr   = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);

        try
        {
          var result = TmNativeUtil.GetCStringFromIntPtr(ptr);

          result.Should().Be("Hello");
        }
        finally
        {
          Marshal.FreeHGlobal(ptr);
        }
      }
    }


    public class GetUtcTimestampFromDateTime
    {
      [Fact]
      public void ReturnsZero_ForEpoch()
      {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = TmNativeUtil.GetUtcTimestampFromDateTime(epoch);

        result.Should().Be(0);
      }

      [Fact]
      public void ReturnsCorrectTimestamp_ForKnownDate()
      {
        var dt     = new DateTime(2024, 1, 15, 12, 30, 0, DateTimeKind.Utc);
        var expected = 1705321800L;

        var result = TmNativeUtil.GetUtcTimestampFromDateTime(dt);

        result.Should().Be(expected);
      }
    }


    public class StringToLpstrBytes
    {
      [Fact]
      public void ReturnsZero_WhenStringIsNull()
      {
        var buf = new byte[10];

        var result = TmNativeUtil.StringToLpstrBytes(null, buf);

        result.Should().Be(0u);
      }

      [Fact]
      public void ReturnsZero_WhenStringIsEmpty()
      {
        var buf = new byte[10];

        var result = TmNativeUtil.StringToLpstrBytes("", buf);

        result.Should().Be(0u);
      }

      [Fact]
      public void WritesStringWithNullTerminator()
      {
        var buf = new byte[10];

        var written = TmNativeUtil.StringToLpstrBytes("ABC", buf);

        written.Should().Be(4u);
        buf[0].Should().Be((byte)'A');
        buf[1].Should().Be((byte)'B');
        buf[2].Should().Be((byte)'C');
        buf[3].Should().Be(0);
      }
    }


    public class StringsToLpstrListBytes
    {
      [Fact]
      public void WritesStringsWithDoubleNullTerminator()
      {
        var buf = new byte[20];

        var written = TmNativeUtil.StringsToLpstrListBytes(new[] { "A", "BC" }, buf);

        written.Should().Be(6u);
        buf[0].Should().Be((byte)'A');
        buf[1].Should().Be(0);
        buf[2].Should().Be((byte)'B');
        buf[3].Should().Be((byte)'C');
        buf[4].Should().Be(0);
        buf[5].Should().Be(0);
      }

      [Fact]
      public void SkipsNullOrEmptyStrings()
      {
        var buf = new byte[20];

        var written = TmNativeUtil.StringsToLpstrListBytes(new[] { "A", null, "", "B" }, buf);

        written.Should().Be(5u);
        buf[0].Should().Be((byte)'A');
        buf[1].Should().Be(0);
        buf[2].Should().Be((byte)'B');
        buf[3].Should().Be(0);
        buf[4].Should().Be(0);
      }
    }


    public class GetDictionaryFromTmBytes
    {
      [Fact]
      public void ReturnsEmpty_ForEmptyBytes()
      {
        var result = TmNativeUtil.GetDictionaryFromTmBytes(Span<byte>.Empty);

        result.Should().BeEmpty();
      }

      [Fact]
      public void ParsesSingleEntry()
      {
        var bytes = new byte[]
        {
          (byte)'k', (byte)'e', (byte)'y', (byte)'=', (byte)'v', 0, 0
        };

        var result = TmNativeUtil.GetDictionaryFromTmBytes(bytes);

        result.Should().ContainSingle().Which.Should().Be(new KeyValuePair<string, string>("key", "v"));
      }

      [Fact]
      public void ParsesMultipleEntries()
      {
        var bytes = new byte[]
        {
          (byte)'a', (byte)'=', (byte)'1', 0,
          (byte)'b', (byte)'=', (byte)'2', 0, 0
        };

        var result = TmNativeUtil.GetDictionaryFromTmBytes(bytes);

        result.Should().BeEquivalentTo(new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" });
      }

      [Fact]
      public void StopsParsing_WhenLineHasNoEquals()
      {
        var bytes = new byte[]
        {
          (byte)'a', (byte)'=', (byte)'1', 0,
          (byte)'n', (byte)'o', (byte)'e', (byte)'q', 0, 0
        };

        var result = TmNativeUtil.GetDictionaryFromTmBytes(bytes);

        result.Should().ContainSingle().Which.Should().Be(new KeyValuePair<string, string>("a", "1"));
      }
    }


    public class TryFindValueByKey
    {
      [Fact]
      public void ReturnsFalse_WhenSourceIsNull()
      {
        var found = TmNativeUtil.TryFindValueByKey(null, "key", ';', '=', out var value);

        found.Should().BeFalse();
        value.Should().BeEmpty();
      }

      [Fact]
      public void ReturnsFalse_WhenKeyIsNull()
      {
        var found = TmNativeUtil.TryFindValueByKey("a=1;b=2", null, ';', '=', out var value);

        found.Should().BeFalse();
        value.Should().BeEmpty();
      }

      [Fact]
      public void ReturnsFalse_WhenKeyNotFound()
      {
        var found = TmNativeUtil.TryFindValueByKey("a=1;b=2", "c", ';', '=', out var value);

        found.Should().BeFalse();
        value.Should().BeEmpty();
      }

      [Fact]
      public void ReturnsValue_WhenKeyFound()
      {
        var found = TmNativeUtil.TryFindValueByKey("a=1;b=2", "a", ';', '=', out var value);

        found.Should().BeTrue();
        value.Should().Be("1");
      }

      [Fact]
      public void ReturnsValue_ForKeyAtEndWithoutSeparator()
      {
        var found = TmNativeUtil.TryFindValueByKey("a=1;b=2", "b", ';', '=', out var value);

        found.Should().BeTrue();
        value.Should().Be("2");
      }
    }


    public class GetEventAddData
    {
      [Fact]
      public void Throws_WhenBytesIsEmpty()
      {
        Action act = () => TmNativeUtil.GetEventAddData(Span<byte>.Empty);

        act.Should().Throw<ArgumentException>();
      }
    }


    public class IpAddrToNativeDword
    {
      [Fact]
      public void ReturnsCorrectDword_ForValidIp()
      {
        var result = TmNativeUtil.IpAddrToNativeDword("192.168.1.1");

        result.Should().Be(0x0101a8c0u);
      }

      [Fact]
      public void ReturnsZero_ForInvalidIp()
      {
        var result = TmNativeUtil.IpAddrToNativeDword("not.an.ip");

        result.Should().Be(0u);
      }

      [Fact]
      public void ReturnsZero_WhenOctetOutOfRange()
      {
        var result = TmNativeUtil.IpAddrToNativeDword(300, 1, 1, 1);

        result.Should().Be(0u);
      }

      [Fact]
      public void ReturnsZero_ForZeroIp()
      {
        var result = TmNativeUtil.IpAddrToNativeDword(0, 0, 0, 0);

        result.Should().Be(0u);
      }

      [Fact]
      public void ReturnsZero_ForBroadcastIp()
      {
        var result = TmNativeUtil.IpAddrToNativeDword(255, 255, 255, 255);

        result.Should().Be(0u);
      }
    }


    public class IntPtrToByteSpan
    {
      [Fact]
      public void ReturnsSpanWithCorrectData()
      {
        var source = new byte[] { 1, 2, 3 };
        var ptr    = Marshal.AllocHGlobal(source.Length);
        Marshal.Copy(source, 0, ptr, source.Length);

        try
        {
          var span = TmNativeUtil.IntPtrToByteSpan(ptr, source.Length);

          span.ToArray().Should().BeEquivalentTo(source, options => options.WithStrictOrdering());
        }
        finally
        {
          Marshal.FreeHGlobal(ptr);
        }
      }
    }


    public class PointerValueIsNull
    {
      [Fact]
      public void Throws_WhenPointerIsZero()
      {
        Action act = () => TmNativeUtil.PointerValueIsNull(nint.Zero);

        act.Should().Throw<ArgumentException>();
      }

      [Fact]
      public void ReturnsTrue_WhenFirstByteIsNull()
      {
        var ptr = Marshal.AllocHGlobal(4);
        Marshal.WriteByte(ptr, 0, 0);

        try
        {
          var result = TmNativeUtil.PointerValueIsNull(ptr);

          result.Should().BeTrue();
        }
        finally
        {
          Marshal.FreeHGlobal(ptr);
        }
      }

      [Fact]
      public void ReturnsFalse_WhenFirstByteIsNotNull()
      {
        var ptr = Marshal.AllocHGlobal(4);
        Marshal.WriteByte(ptr, 0, 1);

        try
        {
          var result = TmNativeUtil.PointerValueIsNull(ptr);

          result.Should().BeFalse();
        }
        finally
        {
          Marshal.FreeHGlobal(ptr);
        }
      }
    }


    public class FreeAllocatedPointer
    {
      [Fact]
      public void DoesNotThrow_WhenPointerIsValid()
      {
        var ptr = Marshal.AllocHGlobal(10);

        Action act = () => TmNativeUtil.FreeAllocatedPointer(ptr);

        act.Should().NotThrow();
      }
    }


    public class GetStringsListFromIntPtr
    {
      [Fact]
      public void ReturnsEmpty_WhenPointerIsZero()
      {
        var result = TmNativeUtil.GetStringsListFromIntPtr(nint.Zero);

        result.Should().BeEmpty();
      }

      [Fact]
      public void ReturnsStrings_ForValidPointer()
      {
        var bytes = new byte[] { (byte)'A', 0, (byte)'B', 0, 0 };
        var ptr   = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);

        try
        {
          var result = TmNativeUtil.GetStringsListFromIntPtr(ptr);

          result.Should().Equal("A", "B");
        }
        finally
        {
          Marshal.FreeHGlobal(ptr);
        }
      }

      [Fact]
      public void RespectsLimit()
      {
        var bytes = new byte[] { (byte)'A', 0, (byte)'B', 0, (byte)'C', 0, 0 };
        var ptr   = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);

        try
        {
          var result = TmNativeUtil.GetStringsListFromIntPtr(ptr, limit: 2);

          result.Should().Equal("A", "B");
        }
        finally
        {
          Marshal.FreeHGlobal(ptr);
        }
      }
    }


    public class GetStringsListFromBytes
    {
      [Fact]
      public void ReturnsStrings_ForValidBytes()
      {
        var bytes = new byte[] { (byte)'X', 0, (byte)'Y', 0, 0 };

        var result = TmNativeUtil.GetStringsListFromBytes(bytes);

        result.Should().Equal("X", "Y");
      }
    }


    public class GetStringsListWithOffsetPointer
    {
      [Fact]
      public void ReturnsEmpty_WhenPointerIsZero()
      {
        var (strings, next) = TmNativeUtil.GetStringsListWithOffsetPointer(nint.Zero);

        strings.Should().BeEmpty();
        next.Should().Be(nint.Zero);
      }

      [Fact]
      public void ReturnsStringsAndNextPointer_ForValidPointer()
      {
        var bytes = new byte[] { (byte)'A', 0, 0, (byte)'X', 0, 0 };
        var ptr   = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);

        try
        {
          var (strings, next) = TmNativeUtil.GetStringsListWithOffsetPointer(ptr);

          strings.Should().Equal("A");
          next.Should().NotBe(nint.Zero);
          next.Should().NotBe(ptr);
        }
        finally
        {
          Marshal.FreeHGlobal(ptr);
        }
      }
    }


    public class ParseMqttMessageDatagram
    {
      [Fact]
      public void ReturnsEmpty_WhenDatagramTooShort()
      {
        var bytes = new byte[] { 0 };

        var result = TmNativeUtil.ParseMqttMessageDatagram(bytes);

        result.Headers.Should().BeEmpty();
        result.Payload.ToArray().Should().BeEmpty();
      }

      [Fact]
      public void ReturnsEmpty_WhenDatagramPrefixMismatch()
      {
        var bytes = new[] { (byte)'x', (byte)'y' };

        var result = TmNativeUtil.ParseMqttMessageDatagram(bytes);

        result.Headers.Should().BeEmpty();
        result.Payload.ToArray().Should().BeEmpty();
      }

      [Fact]
      public void ReturnsEmpty_WhenNoDoubleNullSeparator()
      {
        var bytes = new byte[] { (byte)'p', (byte)'o', (byte)'a', 0, (byte)'b', 0 };

        var result = TmNativeUtil.ParseMqttMessageDatagram(bytes);

        result.Headers.Should().BeEmpty();
        result.Payload.ToArray().Should().BeEmpty();
      }

      [Fact]
      public void ParsesHeaders_WhenDoubleNullSeparatorExists()
      {
        var bytes = new byte[]
        {
          (byte)'p', (byte)'o',
          (byte)'k', (byte)'e', (byte)'y', (byte)'=', (byte)'v', 0,
          0,
          (byte)'p', (byte)'a', (byte)'y'
        };

        var result = TmNativeUtil.ParseMqttMessageDatagram(bytes);

        result.Headers.Should().ContainSingle().Which.Should().Be(new KeyValuePair<string, string>("key", "v"));
        
        result.Payload.ToArray().Should().BeEquivalentTo(new[] { (byte)'p', (byte)'a', (byte)'y' }, 
                                                         options => options.WithStrictOrdering());
      }

      [Fact]
      public void ParsesHeaders_WithEmptyPayload()
      {
        var bytes = new byte[]
        {
          (byte)'p', (byte)'o',
          (byte)'a', (byte)'=', (byte)'1', 0,
          0
        };

        var result = TmNativeUtil.ParseMqttMessageDatagram(bytes);

        result.Headers.Should().ContainSingle().Which.Should().Be(new KeyValuePair<string, string>("a", "1"));
        
        result.Payload.ToArray().Should().BeEmpty();
      }
    }
  }
}
