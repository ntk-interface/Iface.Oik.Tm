using System;
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
        var dummy = new TestDummy { Id = 0xAA, Value = 0x000000BB };
        var expectedBytes = new byte[] { 0xAA, 0xBB, 0x00, 0x00, 0x00 };

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