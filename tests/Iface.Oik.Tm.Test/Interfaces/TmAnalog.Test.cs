using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmAnalogTest
  {
    public class Constructor
    {
      [Theory, TmAutoData]
      public void ConstructsFromChRtuPointCorrectly(int ch, int rtu, int point)
      {
        var tmAnalog = new TmAnalog(ch, rtu, point);

        Assert.Equal(TmType.Analog, tmAnalog.Type);
        Assert.Equal(ch,            tmAnalog.TmAddr.Ch);
        Assert.Equal(rtu,           tmAnalog.TmAddr.Rtu);
        Assert.Equal(point,         tmAnalog.TmAddr.Point);
      }


      [Theory, TmAutoData]
      public void ConstructsFromTmAddrCorrectly(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(TmType.Analog, ch, rtu, point);

        var tmAnalog = new TmAnalog(tmAddr);

        Assert.Equal(TmType.Analog, tmAnalog.Type);
        Assert.Equal(ch,            tmAnalog.TmAddr.Ch);
        Assert.Equal(rtu,           tmAnalog.TmAddr.Rtu);
        Assert.Equal(point,         tmAnalog.TmAddr.Point);
      }
    }


    public class ToStringMethod
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(0,  1,  1,   1.337, 1, "Вт", "#TT0:1:1 = 1,3 Вт")]
      [InlineData(16, 33, 257, 0,     3, "В",  "#TT16:33:257 = 0,000 В")]
      public void ReturnsCorrectString(int    ch, int rtu, int point, float value, byte precision, string unit,
                                       string expected)
      {
        var tmAnalog = new TmAnalog(ch, rtu, point) {IsInit = true, Value = value, Precision = precision, Unit = unit};

        var result = tmAnalog.ToString();

        Assert.Equal(expected, result);
      }
    }


    public class ValueStringProperty
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(1.337, 1, "1,3")]
      [InlineData(0,     4, "0,0000")]
      public void ReturnsCorrectString(float  value, byte precision,
                                       string expected)
      {
        var tmAnalog = new TmAnalog(0, 1, 1) {IsInit = true, Value = value, Precision = precision};

        var result = tmAnalog.ValueString;

        Assert.Equal(expected, result);
      }


      [Fact]
      public void ReturnsQuestionsFromNonInitAnalog()
      {
        var tmAnalog = new TmAnalog(0, 1, 1);

        var result = tmAnalog.ValueString;

        Assert.Equal("???", result);
      }
    }


    public class ValueStringWithPrecisionMethod
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(1.337, 1, "1,3")]
      [InlineData(0,     4, "0,0000")]
      public void ReturnsCorrectString(float  value, byte precision,
                                       string expected)
      {
        var tmAnalog = new TmAnalog(0, 1, 1) {IsInit = true, Value = value};

        var result = tmAnalog.ValueStringWithPrecision(precision);

        Assert.Equal(expected, result);
      }
    }


    public class ValueWithUnitStringWithPrecisionMethod
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(1.337, 1, "МВт", "1,3 МВт")]
      [InlineData(0,     4, "кВ",  "0,0000 кВ")]
      public void ReturnsCorrectString(float  value, byte precision, string unit,
                                       string expected)
      {
        var tmAnalog = new TmAnalog(0, 1, 1) {IsInit = true, Value = value, Unit = unit};

        var result = tmAnalog.ValueWithUnitStringWithPrecision(precision);

        Assert.Equal(expected, result);
      }
    }


    public class EqualsMethod
    {
      [Theory, TmAutoData]
      public void ReturnsFalseForNull(int ch, int rtu, int point)
      {
        var tmAnalog1 = new TmAnalog(ch, rtu, point);

        var result = tmAnalog1.Equals(null);

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongType(int ch, int rtu, int point)
      {
        var tmAnalog1 = new TmAnalog(ch, rtu, point);
        var tmStatus  = new TmStatus(ch, rtu, point);

        var result = tmAnalog1.Equals(tmStatus);

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongAddr(int ch, int rtu, int point)
      {
        var tmAnalog1 = new TmAnalog(ch, rtu, point);
        var tmAnalog2 = new TmAnalog(ch, rtu, point + 1);

        var result = tmAnalog1.Equals(tmAnalog2);

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsTrue(int ch, int rtu, int point)
      {
        var tmAnalog1 = new TmAnalog(ch, rtu, point);
        var tmAnalog2 = new TmAnalog(ch, rtu, point);

        var result = tmAnalog1.Equals(tmAnalog2);

        Assert.True(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongValue(int ch, int rtu, int point)
      {
        var tmAnalog1 = new TmAnalog(ch, rtu, point);
        var tmAnalog2 = new TmAnalog(ch, rtu, point) {Value = 1};

        var result = tmAnalog1.Equals(tmAnalog2);

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongFlags(int ch, int rtu, int point)
      {
        var tmAnalog1 = new TmAnalog(ch, rtu, point);
        var tmAnalog2 = new TmAnalog(ch, rtu, point) {Flags = TmFlags.Unreliable};

        var result = tmAnalog1.Equals(tmAnalog2);

        Assert.False(result);
      }
    }


    public class EqualityOperator
    {
      [Theory, TmAutoData]
      public void ReturnsFalseForNull(int ch, int rtu, int point)
      {
        var tmAnalog1 = new TmAnalog(ch, rtu, point);

        var result = tmAnalog1 == null;

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsTrueForNullWhenNull(int ch, int rtu, int point)
      {
        TmAnalog tmAnalog1 = null;

        var result = tmAnalog1 == null;

        Assert.True(result);
      }


      [Theory, TmAutoData]
      public void ReturnsTrue(int ch, int rtu, int point)
      {
        var tmAnalog1 = new TmAnalog(ch, rtu, point);
        var tmAnalog2 = new TmAnalog(ch, rtu, point);

        var result = tmAnalog1 == tmAnalog2;

        Assert.True(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongAddr(int ch, int rtu, int point)
      {
        var tmAnalog1 = new TmAnalog(ch, rtu, point);
        var tmAnalog2 = new TmAnalog(ch, rtu, point + 1);

        var result = tmAnalog1 == tmAnalog2;

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongValue(int ch, int rtu, int point)
      {
        var tmAnalog1 = new TmAnalog(ch, rtu, point);
        var tmAnalog2 = new TmAnalog(ch, rtu, point) {Value = 1};

        var result = tmAnalog1 == tmAnalog2;

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongFlags(int ch, int rtu, int point)
      {
        var tmAnalog1 = new TmAnalog(ch, rtu, point);
        var tmAnalog2 = new TmAnalog(ch, rtu, point) {Flags = TmFlags.Unreliable};

        var result = tmAnalog1 == tmAnalog2;

        Assert.False(result);
      }
    }


    public class FromCommonPointMethod
    {
      [Fact]
      public void DoesNotInitWithTmFlagsInvalid()
      {
        var tmAnalog = new TmAnalog(0, 1, 1);
        var tmcCommonPoint = new TmNativeDefs.TCommonPoint
        {
          TM_Flags = 0xFFFF,
          Data     = new byte[] {0, 0},
        };

        tmAnalog.FromTmcCommonPoint(tmcCommonPoint);

        Assert.False(tmAnalog.IsInit);
      }


      [Fact]
      public void DoesNotInitWithStatusDataNull()
      {
        var tmAnalog       = new TmAnalog(0, 1, 1);
        var tmcCommonPoint = new TmNativeDefs.TCommonPoint();

        tmAnalog.FromTmcCommonPoint(tmcCommonPoint);

        Assert.False(tmAnalog.IsInit);
      }


      [Theory]
      [InlineData(1.337,     0,                                                                            7, 1)]
      [InlineData(0,         (short) TmNativeDefs.Flags.UnreliableHdw,                                     7, 3)]
      [InlineData(1234.5678, (short) (TmNativeDefs.Flags.UnreliableManu | TmNativeDefs.Flags.ManuallySet), 8, 4)]
      public void SetsCorrectValues(float value, short flags, byte width, byte precision)
      {
        var tmAnalog = new TmAnalog(0, 1, 1);
        var tmcCommonPoint = new TmNativeDefs.TCommonPoint
        {
          TM_Flags = 1,
          Data = TmNativeUtil.GetBytes(new TmNativeDefs.TAnalogPoint
          {
            AsFloat = value,
            Flags   = flags,
            Format  = (byte) ((precision << 4) | (width)),
          }),
        };

        tmAnalog.FromTmcCommonPoint(tmcCommonPoint);

        Assert.True(tmAnalog.IsInit);
        Assert.Equal(value,     tmAnalog.Value);
        Assert.Equal(flags,     (short) tmAnalog.Flags);
        Assert.Equal(width,     tmAnalog.Width);
        Assert.Equal(precision, tmAnalog.Precision);
      }
    }
  }


  public class UpdatePropertiesWithDtoMethod
  {
    [Theory]
    [InlineData("ТИТ1", "В",   "5.0", 1, 5, 0)]
    [InlineData("ТИТ2", "кВт", "7.1", 2, 7, 1)]
    [InlineData("ТИТ3", "А",   "7.3", 3, 7, 3)]
    public void SetsCorrectValuesWithClass(string name,          string unit, string format, short classId,
                                           byte   expectedWidth, byte   expectedPrecision)
    {
      var tmAnalog = new TmAnalog(0, 1, 1);

      tmAnalog.UpdatePropertiesWithDto(name, unit, format, classId, ""); // todo al test provider!

      Assert.Equal(name,              tmAnalog.Name);
      Assert.Equal(unit,              tmAnalog.Unit);
      Assert.Equal(expectedWidth,     tmAnalog.Width);
      Assert.Equal(expectedPrecision, tmAnalog.Precision);
      Assert.Equal((byte) classId,    tmAnalog.ClassId);
    }


    [Theory]
    [InlineData("ТИТ1", "В", "5.0", -1, 5, 0)]
    public void SetsCorrectValuesWithNoClass(string name,          string unit, string format, short classId,
                                             byte   expectedWidth, byte   expectedPrecision)
    {
      var tmAnalog = new TmAnalog(0, 1, 1);

      tmAnalog.UpdatePropertiesWithDto(name, unit, format, classId, ""); // todo al test provider!

      Assert.Equal(name,              tmAnalog.Name);
      Assert.Equal(unit,              tmAnalog.Unit);
      Assert.Equal(expectedWidth,     tmAnalog.Width);
      Assert.Equal(expectedPrecision, tmAnalog.Precision);
      Assert.Null(tmAnalog.ClassId);
    }
  }
}