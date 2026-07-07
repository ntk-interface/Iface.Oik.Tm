using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmAddrTest
  {
    public class Constructor
    {
      [Theory]
      [TmInlineAutoData]
      [InlineData(0,   1,   1)]
      [InlineData(0,   1,   11)]
      [InlineData(0,   1,   65535)]
      [InlineData(0,   2,   1)]
      [InlineData(0,   255, 65535)]
      [InlineData(1,   1,   1)]
      [InlineData(254, 255, 65535)]
      public void FromChRtuPointCorrectly(ushort ch, ushort rtu, ushort point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        tmAddr.Type.Should().Be(TmType.Unknown);
        tmAddr.Ch.Should().Be(ch);
        tmAddr.Rtu.Should().Be(rtu);
        tmAddr.Point.Should().Be(point);
      }


      [Theory]
      [TmInlineAutoData(TmType.Status)]
      [TmInlineAutoData(TmType.Analog)]
      public void FromTypeChRtuPointCorrectly(TmType type, ushort ch, ushort rtu, ushort point)
      {
        var tmAddr = new TmAddr(type, ch, rtu, point);

        tmAddr.Type.Should().Be(type);
        tmAddr.Ch.Should().Be(ch);
        tmAddr.Rtu.Should().Be(rtu);
        tmAddr.Point.Should().Be(point);
      }


      [Theory]
      [InlineData(0,   256, 1)]
      [InlineData(255, 1,   1)]
      public void ThrowsWhenChRtuPointInvalid(ushort ch, ushort rtu, ushort point)
      {
        Action act = () => TmAddr.Create(ch, rtu, point);

        act.Should().Throw<ArgumentException>();
      }


      [Theory]
      [InlineData(0,             0,   1,   1)]
      [InlineData(10,            0,   1,   11)]
      [InlineData(0xFF_FE,       0,   1,   65535)]
      [InlineData(0x01_00_00,    0,   2,   1)]
      [InlineData(0xFE_00_00,    0,   255, 1)]
      [InlineData(0x01_00_00_00, 1,   1,   1)]
      [InlineData(0xFE_00_00_00, 254, 1,   1)]
      public void FromIntegerCorrectly(uint   value,
                                       ushort expectedCh, ushort expectedRtu, ushort expectedPoint)
      {
        var tmAddr = new TmAddr(value);

        tmAddr.Type.Should().Be(TmType.Unknown);
        tmAddr.Ch.Should().Be(expectedCh);
        tmAddr.Rtu.Should().Be(expectedRtu);
        tmAddr.Point.Should().Be(expectedPoint);
      }


      [Theory]
      [InlineData(TmType.Status, 0,  0, 1, 1)]
      [InlineData(TmType.Analog, 10, 0, 1, 11)]
      public void FromTypeIntegerCorrectly(TmType type,       uint   value,
                                           ushort expectedCh, ushort expectedRtu, ushort expectedPoint)
      {
        var tmAddr = new TmAddr(type, value);

        tmAddr.Type.Should().Be(type);
        tmAddr.Ch.Should().Be(expectedCh);
        tmAddr.Rtu.Should().Be(expectedRtu);
        tmAddr.Point.Should().Be(expectedPoint);
      }
    }


    public class CreateMethod
    {
      [Theory]
      [TmInlineAutoData]
      [InlineData(16,  33,  257)]
      [InlineData(254, 255, 65535)]
      public void FromChRtuPointCorrectly(ushort ch, ushort rtu, ushort point)
      {
        var tmAddr = TmAddr.Create(ch, rtu, point);

        tmAddr.Type.Should().Be(TmType.Unknown);
        tmAddr.Ch.Should().Be(ch);
        tmAddr.Rtu.Should().Be(rtu);
        tmAddr.Point.Should().Be(point);
      }


      [Theory]
      [TmInlineAutoData(TmType.Status)]
      [TmInlineAutoData(TmType.Analog)]
      public void FromTypeChRtuPointCorrectly(TmType type, ushort ch, ushort rtu, ushort point)
      {
        var tmAddr = TmAddr.Create(type, ch, rtu, point);

        tmAddr.Type.Should().Be(type);
        tmAddr.Ch.Should().Be(ch);
        tmAddr.Rtu.Should().Be(rtu);
        tmAddr.Point.Should().Be(point);
      }


      [Theory]
      [InlineData(0,             0,   1,   1)]
      [InlineData(0x10_20_01_00, 16,  33,  257)]
      [InlineData(0xFE_FE_FF_FE, 254, 255, 65535)]
      public void FromIntegerCorrectly(uint   value,
                                       ushort expectedCh, ushort expectedRtu, ushort expectedPoint)
      {
        var tmAddr = TmAddr.Create(value);

        tmAddr.Type.Should().Be(TmType.Unknown);
        tmAddr.Ch.Should().Be(expectedCh);
        tmAddr.Rtu.Should().Be(expectedRtu);
        tmAddr.Point.Should().Be(expectedPoint);
      }


      [Theory]
      [InlineData(TmType.Status, 0,             0,  1,  1)]
      [InlineData(TmType.Analog, 0x10_20_01_00, 16, 33, 257)]
      public void FromTypeIntegerCorrectly(TmType type,       uint   value,
                                           ushort expectedCh, ushort expectedRtu, ushort expectedPoint)
      {
        var tmAddr = TmAddr.Create(type, value);

        tmAddr.Type.Should().Be(type);
        tmAddr.Ch.Should().Be(expectedCh);
        tmAddr.Rtu.Should().Be(expectedRtu);
        tmAddr.Point.Should().Be(expectedPoint);
      }


      [Theory]
      [InlineData("16",  "33",  "257")]
      [InlineData("254", "255", "65535")]
      public void FromStringsCorrectly(string ch, string rtu, string point)
      {
        var tmAddr = TmAddr.Create(ch, rtu, point);

        tmAddr.Type.Should().Be(TmType.Unknown);
        tmAddr.Ch.Should().Be(ushort.Parse(ch));
        tmAddr.Rtu.Should().Be(ushort.Parse(rtu));
        tmAddr.Point.Should().Be(ushort.Parse(point));
      }


      [Theory]
      [InlineData(TmType.Status, "16",  "33",  "257")]
      [InlineData(TmType.Analog, "254", "255", "65535")]
      public void FromStringsWithTypeCorrectly(TmType type, string ch, string rtu, string point)
      {
        var tmAddr = TmAddr.Create(type, ch, rtu, point);

        tmAddr.Type.Should().Be(type);
        tmAddr.Ch.Should().Be(ushort.Parse(ch));
        tmAddr.Rtu.Should().Be(ushort.Parse(rtu));
        tmAddr.Point.Should().Be(ushort.Parse(point));
      }


      [Theory]
      [InlineData("16",    "33",    "dummy")]
      [InlineData("16",    "dummy", "257")]
      [InlineData("dummy", "33",    "257")]
      [InlineData("256",   "33",    "257")]
      public void ReturnsNullFromIncorrectStrings(string ch, string rtu, string point)
      {
        var tmAddr = TmAddr.Create(ch, rtu, point);

        tmAddr.Should().BeNull();
      }


      [Theory]
      [InlineData(TmType.Status, "16",    "33",    "dummy")]
      [InlineData(TmType.Status, "16",    "dummy", "257")]
      [InlineData(TmType.Analog, "dummy", "33",    "257")]
      [InlineData(TmType.Analog, "256",   "33",    "257")]
      public void ReturnsNullFromIncorrectStringsWithType(TmType type, string ch, string rtu, string point)
      {
        var tmAddr = TmAddr.Create(type, ch, rtu, point);

        tmAddr.Should().BeNull();
      }
    }


    public class SetChProperty
    {
      [Theory, TmAutoData]
      public void SetsChCorrectly(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        tmAddr.Ch = 250;

        tmAddr.Should().Be(new TmAddr(250, rtu, point));
      }


      [Theory, TmAutoData]
      public void ThrowsWhenInvalidCh(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        Action act = () => tmAddr.Ch = 255;

        act.Should().Throw<ArgumentException>();
      }
    }


    public class SetRtuProperty
    {
      [Theory, TmAutoData]
      public void SetsRtuCorrectly(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        tmAddr.Rtu = 250;

        tmAddr.Should().Be(new TmAddr(ch, 250, point));
      }


      [Theory, TmAutoData]
      public void ThrowsWhenInvalidRtu(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        Action act = () => tmAddr.Rtu = 256;

        act.Should().Throw<ArgumentException>();
      }
    }


    public class SetPointProperty
    {
      [Theory, TmAutoData]
      public void SetsPointCorrectly(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        tmAddr.Point = 250;

        tmAddr.Should().Be(new TmAddr(ch, rtu, 250));
      }
    }


    public class ToIntegerMethod
    {
      [Theory]
      [InlineData(0,   1,   1,     0)]
      [InlineData(0,   1,   11,    10)]
      [InlineData(0,   1,   65535, 0xFF_FE)]
      [InlineData(0,   2,   1,     0x01_00_00)]
      [InlineData(0,   255, 1,     0xFE_00_00)]
      [InlineData(1,   1,   1,     0x01_00_00_00)]
      [InlineData(254, 1,   1,     0xFE_00_00_00)]
      [InlineData(16,  33,  257,   0x10_20_01_00)]
      public void ReturnsCorrectValue(ushort ch, ushort rtu, ushort point,
                                      uint   expected)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        var result = tmAddr.ToInteger();

        result.Should().Be(expected);
      }
    }


    public class ToTmaMethod
    {
      [Theory]
      [InlineData(0,  1,  1,   65537)]
      [InlineData(16, 33, 257, 0x10_21_01_01)]
      public void ReturnsCorrectInteger(int ch, int rtu, int point, int expected)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        var result = tmAddr.ToTma();

        result.Should().Be(expected);
      }
    }


    public class EqualsMethod
    {
      [Theory, TmAutoData]
      public void ReturnsTrue(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(TmType.Status, ch, rtu, point);
        var tmAddr2 = new TmAddr(TmType.Status, ch, rtu, point);

        var result = tmAddr1.Equals(tmAddr2);

        result.Should().BeTrue();
      }


      [Theory, TmAutoData]
      public void ReturnsTrueForSameReference(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(TmType.Status, ch, rtu, point);
        var tmAddr2 = tmAddr1;

        var result = tmAddr1.Equals(tmAddr2);

        result.Should().BeTrue();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForDifferentAddr(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(ch, rtu, point);
        var tmAddr2 = new TmAddr(ch, rtu, point + 1);

        var result = tmAddr1.Equals(tmAddr2);

        result.Should().BeFalse();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForDifferentType(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(TmType.Status, ch, rtu, point);
        var tmAddr2 = new TmAddr(TmType.Analog, ch, rtu, point);

        var result = tmAddr1.Equals(tmAddr2);

        result.Should().BeFalse();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForNull(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(ch, rtu, point);

        var result = tmAddr1.Equals(null);

        result.Should().BeFalse();
      }


      [Theory, TmAutoData]
      [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
      public void ReturnsFalseForWrongObject(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(ch, rtu, point);

        var result = tmAddr1.Equals("string, will not work");

        result.Should().BeFalse();
      }
    }


    public class EqualityOperator
    {
      [Theory, TmAutoData]
      public void ReturnsTrue(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(TmType.Status, ch, rtu, point);
        var tmAddr2 = new TmAddr(TmType.Status, ch, rtu, point);

        var result = tmAddr1 == tmAddr2;

        result.Should().BeTrue();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForDifferentAddr(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(TmType.Status, ch, rtu, point);
        var tmAddr2 = new TmAddr(TmType.Status, ch, rtu, point + 1);

        var result = tmAddr1 == tmAddr2;

        result.Should().BeFalse();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForDifferentType(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(TmType.Status, ch, rtu, point);
        var tmAddr2 = new TmAddr(TmType.Analog, ch, rtu, point);

        var result = tmAddr1 == tmAddr2;

        result.Should().BeFalse();
      }


      [Theory, TmAutoData]
      [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
      public void ReturnsFalseForNull(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(ch, rtu, point);

        var result = tmAddr1 == null;

        result.Should().BeFalse();
      }


      [Fact]
      [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
      public void ReturnsTrueForNullWhenNull()
      {
        TmAddr tmAddr1 = null;

        var result = tmAddr1 == null;

        result.Should().BeTrue();
      }
    }


    public class InequalityOperator
    {
      [Theory, TmAutoData]
      public void ReturnsTrueForDifferentAddr(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(TmType.Status, ch, rtu, point);
        var tmAddr2 = new TmAddr(TmType.Status, ch, rtu, point + 1);

        var isNotEqual = tmAddr1 != tmAddr2;

        isNotEqual.Should().BeTrue();
      }


      [Theory, TmAutoData]
      public void ReturnsTrueForDifferentType(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(TmType.Status, ch, rtu, point);
        var tmAddr2 = new TmAddr(TmType.Analog, ch, rtu, point);

        var isNotEqual = tmAddr1 != tmAddr2;

        isNotEqual.Should().BeTrue();
      }


      [Theory, TmAutoData]
      public void ReturnsFalse(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(TmType.Status, ch, rtu, point);
        var tmAddr2 = new TmAddr(TmType.Status, ch, rtu, point);

        var isNotEqual = tmAddr1 != tmAddr2;

        isNotEqual.Should().BeFalse();
      }


      [Theory, TmAutoData]
      [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
      public void ReturnsTrueForNull(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(ch, rtu, point);

        var isNotEqual = tmAddr1 != null;

        isNotEqual.Should().BeTrue();
      }
    }


    public class CompareToMethod
    {
      [Theory]
      [InlineData(5, 10, 200, 5, 10, 200, 0)]
      [InlineData(6, 10, 200, 5, 10, 200, +1)]
      [InlineData(4, 10, 200, 5, 10, 200, -1)]
      [InlineData(5, 11, 200, 5, 10, 200, +1)]
      [InlineData(5, 9,  200, 5, 10, 200, -1)]
      [InlineData(5, 10, 201, 5, 10, 200, +1)]
      [InlineData(5, 10, 199, 5, 10, 200, -1)]
      [InlineData(6, 9, 199, 5, 10, 200, +1)]
      public void ReturnsCorrectValues(int ch1, int rtu1, int point1, int ch2, int rtu2, int point2, int expected)
      {
        var tmAddr1 = new TmAddr(ch1, rtu1, point1);
        var tmAddr2 = new TmAddr(ch2, rtu2, point2);

        var result = tmAddr1.CompareTo(tmAddr2);

        result.Should().Be(expected);
      }
    }


    public class EqualsThreeIntegersMethod
    {
      [Theory, TmAutoData]
      public void ReturnsTrue(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        var result = tmAddr.Equals(ch, rtu, point);

        result.Should().BeTrue();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForDifferentAddr(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        var result = tmAddr.Equals(ch, rtu, point + 1);

        result.Should().BeFalse();
      }
    }


    public class GetTupleMethod
    {
      [Theory, TmAutoData]
      public void ReturnsCorrectValues(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        var (resultCh, resultRtu, resultPoint) = tmAddr.GetTuple();

        resultCh.Should().Be((ushort) ch);
        resultRtu.Should().Be((ushort) rtu);
        resultPoint.Should().Be((ushort) point);
      }
    }


    public class GetTupleShortMethod
    {
      [Theory, TmAutoData]
      public void ReturnsCorrectValues(short ch, short rtu, short point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        var (resultCh, resultRtu, resultPoint) = tmAddr.GetTupleShort();

        resultCh.Should().Be(ch);
        resultRtu.Should().Be(rtu);
        resultPoint.Should().Be(point);
      }
    }


    public class ToAdrTmMethod
    {
      [Theory, TmAutoData]
      public void ReturnsCorrectValues(short ch, short rtu, short point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        var adrTm = tmAddr.ToAdrTm();

        adrTm.Should().Be(new TmNativeDefs.TAdrTm {Ch = ch, RTU = rtu, Point = point});
      }
    }


    public class ToStringMethod
    {
      [Theory]
      [InlineData(TmType.Unknown, "#??16:33:257")]
      [InlineData(TmType.Status,  "#TC16:33:257")]
      [InlineData(TmType.Analog,  "#TT16:33:257")]
      [InlineData(TmType.Accum,   "#TI16:33:257")]
      public void ReturnsCorrectString(TmType type, string expected)
      {
        var tmAddr = new TmAddr(type, 16, 33, 257);

        var result = tmAddr.ToString();

        result.Should().Be(expected);
      }
    }


    public class TryParseMethod
    {
      [Theory]
      [InlineData("16:33:257",     TmType.Unknown)]
      [InlineData("#TC16:33:257",  TmType.Status)]
      [InlineData("#TC:16:33:257", TmType.Status)]
      [InlineData("#TT16:33:257",  TmType.Analog)]
      [InlineData("#TI16:33:257",  TmType.Accum)]
      [InlineData("16,33,257",     TmType.Unknown)]
      public void ParsesCorrectlyFromValidString(string s, TmType type)
      {
        var result = TmAddr.TryParse(s, out var tmAddr);

        result.Should().BeTrue();
        tmAddr.Should().Be(new TmAddr(type, 16, 33, 257));
      }


      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("  ")]
      [InlineData("ass")]
      [InlineData("0:0:0")]
      [InlineData("0:256:1")]
      [InlineData("255:1:1")]
      [InlineData("0:0:1:1:1")]
      [InlineData("0:1")]
      [InlineData("#ERROR0:1:1")]
      public void FailsFromInvalidString(string s)
      {
        var result = TmAddr.TryParse(s, out var tmAddr);

        result.Should().BeFalse();
        tmAddr.Should().BeNull();
      }
    }


    public class ParseMethod
    {
      [Theory]
      [InlineData("16:33:257",     TmType.Unknown)]
      [InlineData("#TC16:33:257",  TmType.Status)]
      [InlineData("#TC:16:33:257", TmType.Status)]
      [InlineData("#TT16:33:257",  TmType.Analog)]
      [InlineData("#TI16:33:257",  TmType.Accum)]
      [InlineData("16,33,257",     TmType.Unknown)]
      public void ReturnsCorrectTmAddrFromValidString(string s, TmType type)
      {
        var tmAddr = TmAddr.Parse(s);

        tmAddr.Should().Be(new TmAddr(type, 16, 33, 257));
      }


      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("  ")]
      [InlineData("ass")]
      [InlineData("0:0:0")]
      [InlineData("0:256:1")]
      [InlineData("255:1:1")]
      [InlineData("0:0:1:1:1")]
      [InlineData("0:1")]
      [InlineData("#ERROR0:1:1")]
      public void ThrowsFromInvalidString(string s)
      {
        Action act = () => TmAddr.Parse(s);

        act.Should().Throw<ArgumentException>();
      }
    }
    
    
    public class TypeProperty
    {
      [Fact]
      public void SetterWorks()
      {
        var tmAddr = new TmAddr(TmType.Status, 1, 1, 1);

        tmAddr.Type = TmType.Analog;

        tmAddr.Type.Should().Be(TmType.Analog);
      }
    }


    public class GetHashCodeMethod
    {
      [Theory, TmAutoData]
      public void ReturnsSameValue_ForEqualObjects(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(TmType.Status, ch, rtu, point);
        var tmAddr2 = new TmAddr(TmType.Status, ch, rtu, point);

        var hash1 = tmAddr1.GetHashCode();
        var hash2 = tmAddr2.GetHashCode();

        hash1.Should().Be(hash2);
      }


      [Theory, TmAutoData]
      public void ReturnsDifferentValue_ForDifferentAddr(int ch, int rtu, int point)
      {
        var tmAddr1 = new TmAddr(ch, rtu, point);
        var tmAddr2 = new TmAddr(ch, rtu, point + 1);

        var hash1 = tmAddr1.GetHashCode();
        var hash2 = tmAddr2.GetHashCode();

        hash1.Should().NotBe(hash2);
      }
    }


    public class TryParseTypeMethod
    {
      [Theory]
      [InlineData("#TC16:33:257", TmType.Status)]
      [InlineData("#TT16:33:257", TmType.Analog)]
      [InlineData("#TI16:33:257", TmType.Accum)]
      public void ParsesCorrectly(string s, TmType expected)
      {
        var result = TmAddr.TryParseType(s, out var type);

        result.Should().BeTrue();
        type.Should().Be(expected);
      }


      [Theory]
      [InlineData("#??16:33:257")]
      [InlineData("16:33:257")]
      [InlineData("")]
      [InlineData(null)]
      public void ReturnsFalse_ForMissingOrUnknownPrefix(string s)
      {
        var result = TmAddr.TryParseType(s, out var type, TmType.Unknown);

        result.Should().BeFalse();
        type.Should().Be(TmType.Unknown);
      }
    }


    public class CreateFromTmaMethod
    {
      [Theory]
      [InlineData(TmType.Status,  0x10_21_01_01, 16, 33, 257)]
      [InlineData(TmType.Analog,  0x00_01_00_01, 0,  1,  1)]
      public void FromTypeAndTmaCorrectly(TmType type, int tma, ushort expectedCh, ushort expectedRtu, ushort expectedPoint)
      {
        var tmAddr = TmAddr.CreateFromTma(type, tma);

        tmAddr.Ch.Should().Be(expectedCh);
        tmAddr.Rtu.Should().Be(expectedRtu);
        tmAddr.Point.Should().Be(expectedPoint);
        tmAddr.Type.Should().Be(type);
      }


      [Fact]
      public void Roundtrips_ThroughToTma()
      {
        var original = new TmAddr(TmType.Status, 16, 33, 257);
        var tma      = original.ToTma();

        var restored = TmAddr.CreateFromTma(TmType.Status, tma);

        restored.Should().Be(original);
      }


      [Fact]
      public void FromFullTma_Roundtrips()
      {
        var original = new TmAddr(TmType.Analog, 5, 10, 200);
        var fullTma  = original.ToFullTma();

        var restored = TmAddr.CreateFromFullTma(fullTma);

        restored.Should().Be(original);
      }
    }


    public class GetTmaTypeMethod
    {
      [Theory]
      [InlineData(TmType.Status,  unchecked((short)0x8000))]
      [InlineData(TmType.Analog,  unchecked((short)0x8001))]
      [InlineData(TmType.Accum,   unchecked((short)0x8002))]
      [InlineData(TmType.Unknown, 0)]
      public void ReturnsCorrectShort_ForType(TmType type, short expected)
      {
        var tmAddr = new TmAddr(type, 1, 1, 1);

        var result = tmAddr.GetTmaType();

        result.Should().Be(expected);
      }
    }


    public class ToFullTmaMethod
    {
      [Fact]
      public void EncodesTypeInHigh32Bits()
      {
        var tmAddr  = new TmAddr(TmType.Analog, 1, 1, 1);
        var fullTma = tmAddr.ToFullTma();

        var typePart = (fullTma >> 32);

        typePart.Should().Be(0x8001); // TmDataTypes.Analog = 0x8001
      }
    }


    public class ToTmaStrMethod
    {
      [Theory]
      [InlineData(TmType.Status, "#TC:0:1:1")]
      [InlineData(TmType.Analog, "#TT:16:33:257")]
      [InlineData(TmType.Accum,  "#TI:254:255:65535")]
      public void ReturnsCorrectString(TmType type, string expected)
      {
        var ch   = ushort.Parse(expected.Split(':')[1]);
        var rtu  = ushort.Parse(expected.Split(':')[2]);
        var point = ushort.Parse(expected.Split(':')[3]);

        var tmAddr = new TmAddr(type, ch, rtu, point);

        var result = tmAddr.ToTmaStr();

        result.Should().Be(expected);
      }
    }


    public class DecodeTmaMethod
    {
      [Fact]
      public void ReturnsFalse_ForZero()
      {
        var result = TmAddr.DecodeTma(0, out var ch, out var rtu, out var point);

        result.Should().BeFalse();
        ch.Should().Be(0);
        rtu.Should().Be(0);
        point.Should().Be(0);
      }


      [Theory]
      [InlineData(0x10_21_01_01, 16, 33, 257)]
      public void DecodesCorrectly(int tma, ushort expectedCh, ushort expectedRtu, ushort expectedPoint)
      {
        var result = TmAddr.DecodeTma(tma, out var ch, out var rtu, out var point);

        result.Should().BeTrue();
        ch.Should().Be(expectedCh);
        rtu.Should().Be(expectedRtu);
        point.Should().Be(expectedPoint);
      }
    }
  }
}