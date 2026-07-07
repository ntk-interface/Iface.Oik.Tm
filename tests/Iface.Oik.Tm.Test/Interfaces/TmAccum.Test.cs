using System;
using FluentAssertions;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmAccumTest
  {
    public class Constructor
    {
      [Theory, TmAutoData]
      public void ConstructsFromChRtuPointCorrectly(int ch, int rtu, int point)
      {
        var tmAccum = new TmAccum(ch, rtu, point);

        tmAccum.Type.Should().Be(TmType.Accum);
        tmAccum.TmAddr.Ch.Should().Be((ushort)ch);
        tmAccum.TmAddr.Rtu.Should().Be((ushort)rtu);
        tmAccum.TmAddr.Point.Should().Be((ushort)point);
      }


      [Theory, TmAutoData]
      public void ConstructsFromTmAddrCorrectly(int ch, int rtu, int point)
      {
        var tmAddr  = new TmAddr(TmType.Accum, ch, rtu, point);
        var tmAccum = new TmAccum(tmAddr);

        tmAccum.Type.Should().Be(TmType.Accum);
        tmAccum.TmAddr.Ch.Should().Be((ushort)ch);
        tmAccum.TmAddr.Rtu.Should().Be((ushort)rtu);
        tmAccum.TmAddr.Point.Should().Be((ushort)point);
      }
    }


    public class ValueStringProperty
    {
      [Theory, UseCulture("ru-RU")]
      [InlineData(1.337, 1, "1,3")]
      [InlineData(0,     4, "0,0000")]
      public void ReturnsCorrectString(float value, byte precision, string expected)
      {
        var tmAccum = new TmAccum(0, 1, 1) { IsInit = true, Value = value, Precision = precision };

        var result = tmAccum.ValueString;

        result.Should().Be(expected);
      }


      [Fact]
      public void ReturnsQuestionsFromNonInitAccum()
      {
        var tmAccum = new TmAccum(0, 1, 1);

        var result = tmAccum.ValueString;

        result.Should().Be("???");
      }
    }


    public class LoadStringProperty
    {
      [Theory, UseCulture("ru-RU")]
      [InlineData(2.5, 2, "2,50")]
      [InlineData(0,   1, "0,0")]
      public void ReturnsCorrectString(float load, byte loadPrecision, string expected)
      {
        var tmAccum = new TmAccum(0, 1, 1) { IsInit = true, Load = load, LoadPrecision = loadPrecision };

        var result = tmAccum.LoadString;

        result.Should().Be(expected);
      }


      [Fact]
      public void ReturnsQuestionsFromNonInitAccum()
      {
        var tmAccum = new TmAccum(0, 1, 1);

        var result = tmAccum.LoadString;

        result.Should().Be("???");
      }
    }


    public class ValueToDisplayProperty
    {
      [Fact, UseCulture("ru-RU")]
      public void ReturnsFormattedString()
      {
        var tmAccum = new TmAccum(0, 1, 1)
        {
          IsInit = true, Value = 100.5f, Precision = 1, Load = 2.33f, LoadPrecision = 2, Unit = "кВт",
        };

        var result = tmAccum.ValueToDisplay;

        result.Should().Be("100,5 (2,33) кВт");
      }
    }


    public class ToStringMethod
    {
      [Fact, UseCulture("ru-RU")]
      public void ReturnsCorrectString()
      {
        var tmAccum = new TmAccum(0, 1, 1) { IsInit = true, Value = 50f, Precision = 2, Unit = "МВт" };

        var result = tmAccum.ToString();

        result.Should().Be("#TI0:1:1 = 50,00 МВт");
      }
    }


    public class EqualsMethod
    {
      [Theory, TmAutoData]
      public void ReturnsFalseForNull(int ch, int rtu, int point)
      {
        var tmAccum = new TmAccum(ch, rtu, point);

        var result = tmAccum.Equals(null);

        result.Should().BeFalse();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongType(int ch, int rtu, int point)
      {
        var tmAccum  = new TmAccum(ch, rtu, point);
        var tmStatus = new TmStatus(ch, rtu, point);

        var result = tmAccum.Equals(tmStatus);

        result.Should().BeFalse();
      }


      [Theory, TmAutoData]
      public void ReturnsTrue(int ch, int rtu, int point)
      {
        var tmAccum1 = new TmAccum(ch, rtu, point);
        var tmAccum2 = new TmAccum(ch, rtu, point);

        var result = tmAccum1.Equals(tmAccum2);

        result.Should().BeTrue();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongValue(int ch, int rtu, int point)
      {
        var tmAccum1 = new TmAccum(ch, rtu, point);
        var tmAccum2 = new TmAccum(ch, rtu, point) { Value = 1 };

        var result = tmAccum1.Equals(tmAccum2);

        result.Should().BeFalse();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongLoad(int ch, int rtu, int point)
      {
        var tmAccum1 = new TmAccum(ch, rtu, point);
        var tmAccum2 = new TmAccum(ch, rtu, point) { Load = 1 };

        var result = tmAccum1.Equals(tmAccum2);

        result.Should().BeFalse();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongFlags(int ch, int rtu, int point)
      {
        var tmAccum1 = new TmAccum(ch, rtu, point);
        var tmAccum2 = new TmAccum(ch, rtu, point) { Flags = TmFlags.Unreliable };

        var result = tmAccum1.Equals(tmAccum2);

        result.Should().BeFalse();
      }
    }


    public class EqualityOperator
    {
      [Theory, TmAutoData]
      public void ReturnsFalseForNull(int ch, int rtu, int point)
      {
        var tmAccum = new TmAccum(ch, rtu, point);

        var result = tmAccum == null;

        result.Should().BeFalse();
      }


      [Fact]
      public void ReturnsTrueForNullWhenNull()
      {
        TmAccum tmAccum = null;

        var result = tmAccum == null;

        result.Should().BeTrue();
      }


      [Theory, TmAutoData]
      public void ReturnsTrue(int ch, int rtu, int point)
      {
        var tmAccum1 = new TmAccum(ch, rtu, point);
        var tmAccum2 = new TmAccum(ch, rtu, point);

        var result = tmAccum1 == tmAccum2;

        result.Should().BeTrue();
      }
    }


    public class UpdateValueFromCommonPointDtoMethod
    {
      [Fact]
      public void DoesNotInitWithTmFlagsInvalid()
      {
        var tmAccum = new TmAccum(0, 1, 1);
        var dto     = new TCommonPointDto
        {
          TmFlags      = 0xFFFF,
          AccumPointDto = new TAccumPointDto(),
        };

        tmAccum.UpdateValueFromCommonPointDto(dto);

        tmAccum.IsInit.Should().BeFalse();
      }


      [Fact]
      public void DoesNotInitWithAccumDataNull()
      {
        var tmAccum = new TmAccum(0, 1, 1);
        var dto     = new TCommonPointDto();

        tmAccum.UpdateValueFromCommonPointDto(dto);

        tmAccum.IsInit.Should().BeFalse();
      }


      [Theory]
      [InlineData(1.337, 0.5, 0)]
      [InlineData(0,     0,   (short) TmNativeDefs.Flags.UnreliableHdw)]
      public void SetsCorrectValues(float value, float load, short flags)
      {
        var tmAccum = new TmAccum(0, 1, 1);
        var dto     = new TCommonPointDto
        {
          TmFlags = 1,
          AccumPointDto = new TAccumPointDto
          {
            Value = value,
            Load  = load,
            Flags = flags,
          },
        };

        tmAccum.UpdateValueFromCommonPointDto(dto);

        tmAccum.IsInit.Should().BeTrue();
        tmAccum.Value.Should().Be(value);
        tmAccum.Load.Should().Be(load);
        ((short) tmAccum.Flags).Should().Be(flags);
      }
    }


    public class UpdatePropertiesFromCommonPointDtoMethod
    {
      [Fact]
      public void SetsUnitWidthPrecision()
      {
        var tmAccum = new TmAccum(0, 1, 1);
        var dto     = new TCommonPointDto
        {
          Name = "Счетчик",
          AccumPointDto = new TAccumPointDto
          {
            Unit   = "кВт",
            Format = 0x23, // width=3, precision=2
          },
        };

        tmAccum.UpdatePropertiesFromCommonPointDto(dto);

        tmAccum.Name.Should().Be("Счетчик");
        tmAccum.Unit.Should().Be("кВт");
        tmAccum.Width.Should().Be(3);
        tmAccum.Precision.Should().Be(2);
      }


      [Fact]
      public void DoesNotSetUnitWhenAccumPointNull()
      {
        var tmAccum = new TmAccum(0, 1, 1) { Unit = "МВт" };
        var dto     = new TCommonPointDto { Name = "Счетчик" };

        tmAccum.UpdatePropertiesFromCommonPointDto(dto);

        tmAccum.Unit.Should().Be("МВт");
      }
    }


    public class UpdatePropertiesFromSqlMethod
    {
      [Theory]
      [InlineData("Счетчик", "кВт", "5.1", "4.2", "10")]
      public void SetsCorrectValuesWithProvider(string name, string unit, string format, string loadFormat,
                                                 string provider)
      {
        var tmAccum = new TmAccum(0, 1, 1);

        tmAccum.UpdatePropertiesFromSql(name, unit, format, loadFormat, provider);

        tmAccum.Name.Should().Be(name);
        tmAccum.Unit.Should().Be(unit);
        tmAccum.Width.Should().Be(5);
        tmAccum.Precision.Should().Be(1);
        tmAccum.LoadWidth.Should().Be(4);
        tmAccum.LoadPrecision.Should().Be(2);
        tmAccum.HasTmProvider.Should().BeTrue();
      }


      [Theory]
      [InlineData("Счетчик", "кВт", "5.1", "4.2", "")]
      public void SetsCorrectValuesWithNoProvider(string name, string unit, string format, string loadFormat,
                                                  string provider)
      {
        var tmAccum = new TmAccum(0, 1, 1);

        tmAccum.UpdatePropertiesFromSql(name, unit, format, loadFormat, provider);

        tmAccum.Name.Should().Be(name);
        tmAccum.HasTmProvider.Should().BeFalse();
      }
    }


    public class UpdateValueFromDtoMethod
    {
      [Fact]
      public void DoesNothingWhenDtoIsNull()
      {
        var tmAccum = new TmAccum(0, 1, 1) { IsInit = true };

        tmAccum.UpdateValueFromDto(null);

        tmAccum.IsInit.Should().BeTrue();
      }


      [Fact]
      public void SetsCorrectValues()
      {
        var tmAccum = new TmAccum(0, 1, 1);
        var dto     = new TmAccumDto
        {
          VVal       = 100.5f,
          VLoad      = 2.3f,
          Flags      = (int) TmNativeDefs.Flags.ManuallySet,
          ChangeTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
        };

        tmAccum.UpdateValueFromDto(dto);

        tmAccum.IsInit.Should().BeTrue();
        tmAccum.Value.Should().Be(100.5f);
        tmAccum.Load.Should().Be(2.3f);
        tmAccum.Flags.Should().Be(TmFlags.ManuallySet);
        tmAccum.ChangeTime.Should().Be(new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc));
      }
    }
  }
}
