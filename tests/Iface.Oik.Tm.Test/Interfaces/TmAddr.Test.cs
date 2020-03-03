using System;
using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Interfaces;
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


    public class CreateFromNoPaddingMethod
    {
      [Theory]
      [InlineData(0x00_01_00_01, 0,   1,   1)]
      [InlineData(0x10_20_01_00, 16,  32,  256)]
      [InlineData(0xFE_FF_FF_FF, 254, 255, 65535)]
      public void FromIntegerCorrectly(uint   value,
                                       ushort expectedCh, ushort expectedRtu, ushort expectedPoint)
      {
        var tmAddr = TmAddr.CreateFromNoPadding(value);

        tmAddr.Type.Should().Be(TmType.Unknown);
        tmAddr.Ch.Should().Be(expectedCh);
        tmAddr.Rtu.Should().Be(expectedRtu);
        tmAddr.Point.Should().Be(expectedPoint);
      }


      [Theory]
      [InlineData(TmType.Status, 0x00_01_00_01, 0,  1,  1)]
      [InlineData(TmType.Analog, 0x10_20_01_00, 16, 32, 256)]
      public void FromTypeIntegerCorrectly(TmType type,       uint   value,
                                           ushort expectedCh, ushort expectedRtu, ushort expectedPoint)
      {
        var tmAddr = TmAddr.CreateFromNoPadding(type, value);

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
    }


    public class SetChProperty
    {
      [Theory, TmAutoData]
      public void SetsChCorrectly(int ch, int rtu, int point)
      {
        var f = new Fixture();
        f.Create<int>();
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


    public class ToIntegerWithoutPaddingMethod
    {
      [Theory]
      [InlineData(0,  1,  1,   65537)]
      [InlineData(16, 33, 257, 0x10_21_01_01)]
      public void ReturnsCorrectInteger(int ch, int rtu, int point, uint expected)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        var result = tmAddr.ToIntegerWithoutPadding();

        result.Should().Be(expected);
      }
    }


    public class EqualsMethod
    {
      [Fact]
      public void ReturnsTrue()
      {
        var tmAddr1 = new TmAddr(TmType.Status, 16, 33, 257);
        var tmAddr2 = new TmAddr(TmType.Status, 0x10_20_01_00);

        var isEqual = tmAddr1.Equals(tmAddr2);

        isEqual.Should().BeTrue();
      }


      [Fact]
      public void ReturnsFalseForDifferentAddr()
      {
        var tmAddr1 = new TmAddr(16, 33, 257);
        var tmAddr2 = new TmAddr(0x10_20_01_01);

        var isEqual = tmAddr1.Equals(tmAddr2);

        isEqual.Should().BeFalse();
      }


      [Fact]
      public void ReturnsFalseForDifferentType()
      {
        var tmAddr1 = new TmAddr(TmType.Status, 16, 33, 257);
        var tmAddr2 = new TmAddr(TmType.Analog, 0x10_20_01_00);

        var isEqual = tmAddr1.Equals(tmAddr2);

        isEqual.Should().BeFalse();
      }


      [Fact]
      public void ReturnsFalseForNull()
      {
        var tmAddr1 = new TmAddr(16, 33, 257);

        var isEqual = tmAddr1.Equals(null);

        isEqual.Should().BeFalse();
      }


      [Fact]
      [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
      public void ReturnsFalseForWrongObject()
      {
        var tmAddr1 = new TmAddr(16, 33, 257);

        var isEqual = tmAddr1.Equals("string, will not work");

        isEqual.Should().BeFalse();
      }
    }


    public class EqualityOperator
    {
      [Fact]
      public void ReturnsTrue()
      {
        var tmAddr1 = new TmAddr(TmType.Status, 16, 33, 257);
        var tmAddr2 = new TmAddr(TmType.Status, 0x10_20_01_00);

        var isEqual = tmAddr1 == tmAddr2;

        isEqual.Should().BeTrue();
      }


      [Fact]
      public void ReturnsFalseForDifferentAddr()
      {
        var tmAddr1 = new TmAddr(TmType.Status, 16, 33, 257);
        var tmAddr2 = new TmAddr(TmType.Status, 0x10_20_01_01);

        var isEqual = tmAddr1 == tmAddr2;

        isEqual.Should().BeFalse();
      }


      [Fact]
      public void ReturnsFalseForDifferentType()
      {
        var tmAddr1 = new TmAddr(TmType.Status, 16, 33, 257);
        var tmAddr2 = new TmAddr(TmType.Analog, 0x10_20_01_00);

        var isEqual = tmAddr1 == tmAddr2;

        isEqual.Should().BeFalse();
      }


      [Fact]
      [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
      public void ReturnsFalseForNull()
      {
        var tmAddr1 = new TmAddr(16, 33, 257);

        var isEqual = tmAddr1 == null;

        isEqual.Should().BeFalse();
      }


      [Fact]
      [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
      public void ReturnsTrueForNullWhenNull()
      {
        TmAddr tmAddr1 = null;
        
        var isEqual = tmAddr1 == null;

        isEqual.Should().BeTrue();
      }
    }


    public class InequalityOperator
    {
      [Fact]
      public void ReturnsTrueForDifferentAddr()
      {
        var tmAddr1 = new TmAddr(TmType.Status, 16, 33, 257);
        var tmAddr2 = new TmAddr(TmType.Status, 0x10_20_01_01);

        var isNotEqual = tmAddr1 != tmAddr2;

        isNotEqual.Should().BeTrue();
      }


      [Fact]
      public void ReturnsTrueForDifferentType()
      {
        var tmAddr1 = new TmAddr(TmType.Status, 16, 33, 257);
        var tmAddr2 = new TmAddr(TmType.Analog, 0x10_20_01_00);

        var isNotEqual = tmAddr1 != tmAddr2;

        isNotEqual.Should().BeTrue();
      }


      [Fact]
      public void ReturnsFalse()
      {
        var tmAddr1 = new TmAddr(TmType.Status, 16, 33, 257);
        var tmAddr2 = new TmAddr(TmType.Status, 0x10_20_01_00);

        var isNotEqual = tmAddr1 != tmAddr2;

        isNotEqual.Should().BeFalse();
      }


      [Fact]
      [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
      public void ReturnsTrueForNull()
      {
        var tmAddr1 = new TmAddr(16, 33, 257);

        var isNotEqual = tmAddr1 != null;

        isNotEqual.Should().BeTrue();
      }
    }


    public class EqualsThreeIntegersMethod
    {
      [Theory, TmAutoData]
      public void ReturnsTrue(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(ch, rtu, point);

        var isEqual = tmAddr.Equals(ch, rtu, point);

        isEqual.Should().BeTrue();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForDifferentAddr(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(ch, rtu, point + 1);

        var isEqual = tmAddr.Equals(ch, rtu, point);

        isEqual.Should().BeFalse();
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
  }
}