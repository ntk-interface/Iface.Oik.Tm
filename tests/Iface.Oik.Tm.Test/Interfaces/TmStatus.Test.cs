using System;
using System.Linq;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmStatusTest
  {
    public class Constructor
    {
      [Theory, TmAutoData]
      public void ConstructsFromChRtuPointCorrectly(int ch, int rtu, int point)
      {
        var tmStatus = new TmStatus(ch, rtu, point);

        Assert.Equal(TmType.Status, tmStatus.Type);
        Assert.Equal(ch,            tmStatus.TmAddr.Ch);
        Assert.Equal(rtu,           tmStatus.TmAddr.Rtu);
        Assert.Equal(point,         tmStatus.TmAddr.Point);
      }


      [Theory, TmAutoData]
      public void ConstructsFromTmAddrCorrectly(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(TmType.Status, ch, rtu, point);

        var tmStatus = new TmStatus(tmAddr);

        Assert.Equal(TmType.Status, tmStatus.Type);
        Assert.Equal(ch,            tmStatus.TmAddr.Ch);
        Assert.Equal(rtu,           tmStatus.TmAddr.Rtu);
        Assert.Equal(point,         tmStatus.TmAddr.Point);
      }
    }


    public class StatusCaptionProperty
    {
      [Theory]
      [InlineData(false, false, 0, TmS2Flags.None,         "ОТКЛ")]
      [InlineData(false, true,  0, TmS2Flags.None,         "СНЯТ")]
      [InlineData(false, false, 1, TmS2Flags.None,         "ВКЛ")]
      [InlineData(false, true,  1, TmS2Flags.None,         "ВЗВЕДЕН")]
      [InlineData(false, false, 0, TmS2Flags.Break,        "00-Обрыв")]
      [InlineData(false, false, 0, TmS2Flags.Malfunction,  "11-Неисправность")]
      [InlineData(false, false, 0, TmS2Flags.Intermediate, "Промежуточное")]
      [InlineData(true,  false, 0, TmS2Flags.None,         "отключен")]
      [InlineData(true,  false, 1, TmS2Flags.None,         "включен")]
      [InlineData(true,  false, 0, TmS2Flags.Break,        "обрыв")]
      [InlineData(true,  false, 0, TmS2Flags.Malfunction,  "неиспр")]
      [InlineData(true,  false, 0, TmS2Flags.Intermediate, "промеж")]
      [InlineData(true,  false, 2, TmS2Flags.None,         "???")]
      public void ReturnsCorrectCaption(bool   useClassData, bool isAps, short status, TmS2Flags s2Flags,
                                        string expected)
      {
        var tmStatus = new TmStatus(0, 1, 1) {Status = status, S2Flags = s2Flags,};
        if (isAps)
        {
          tmStatus.Flags = TmFlags.StatusAps;
        }

        if (useClassData)
        {
          tmStatus.SetTmcClassData("0Txt=отключен\r\n1Txt=включен\r\nBTxt=обрыв\r\nMTxt=неиспр\r\nITxt=промеж");
        }

        var result = tmStatus.StatusCaption;

        Assert.Equal(expected, result);
      }
    }


    public class ToStringMethod
    {
      [Theory]
      [InlineData(0,  1,  1,   1, "#TC0:1:1 = ВКЛ")]
      [InlineData(16, 33, 257, 0, "#TC16:33:257 = ОТКЛ")]
      public void ReturnsCorrectString(int ch, int rtu, int point, short status, string expected)
      {
        var tmStatus = new TmStatus(ch, rtu, point) {Status = status};

        var result = tmStatus.ToString();

        Assert.Equal(expected, result);
      }
    }


    public class GetClassCaptionMethod
    {
      [Theory]
      [InlineData(false, false, 0, TmS2Flags.None,         "ОТКЛ")]
      [InlineData(false, true,  0, TmS2Flags.None,         "СНЯТ")]
      [InlineData(false, false, 1, TmS2Flags.None,         "ВКЛ")]
      [InlineData(false, true,  1, TmS2Flags.None,         "ВЗВЕДЕН")]
      [InlineData(false, false, 0, TmS2Flags.Break,        "00-Обрыв")]
      [InlineData(false, false, 0, TmS2Flags.Malfunction,  "11-Неисправность")]
      [InlineData(false, false, 0, TmS2Flags.Intermediate, "Промежуточное")]
      [InlineData(true,  false, 0, TmS2Flags.None,         "отключен")]
      [InlineData(true,  false, 1, TmS2Flags.None,         "включен")]
      [InlineData(true,  false, 0, TmS2Flags.Break,        "обрыв")]
      [InlineData(true,  false, 0, TmS2Flags.Malfunction,  "неиспр")]
      [InlineData(true,  false, 0, TmS2Flags.Intermediate, "промеж")]
      public void ReturnsCorrectCaption(bool   useClassData, bool isAps, short status, TmS2Flags s2Flags,
                                        string expected)
      {
        var tmStatus = new TmStatus(0, 1, 1);
        if (isAps)
        {
          tmStatus.Flags = TmFlags.StatusAps;
        }

        if (useClassData)
        {
          tmStatus.SetTmcClassData("0Txt=отключен\r\n1Txt=включен\r\nBTxt=обрыв\r\nMTxt=неиспр\r\nITxt=промеж");
        }

        var classCaptionFor = HelperGetClassCaptionFor(status, s2Flags);

        var result = tmStatus.GetClassCaption(classCaptionFor);

        Assert.Equal(expected, result);
      }
    }


    public class EqualsMethod
    {
      [Theory, TmAutoData]
      public void ReturnsFalseForNull(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);

        var result = tmStatus1.Equals(null);

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongType(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);
        var tmAnalog  = new TmAnalog(ch, rtu, point);

        var result = tmStatus1.Equals(tmAnalog);

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsTrue(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);
        var tmStatus2 = new TmStatus(ch, rtu, point);

        var result = tmStatus1.Equals(tmStatus2);

        Assert.True(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongAddr(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);
        var tmStatus2 = new TmStatus(ch, rtu, point + 1);

        var result = tmStatus1.Equals(tmStatus2);

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongStatus(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);
        var tmStatus2 = new TmStatus(ch, rtu, point) {Status = 1};

        var result = tmStatus1.Equals(tmStatus2);

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongFlags(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);
        var tmStatus2 = new TmStatus(ch, rtu, point) {Flags = TmFlags.Unreliable};

        var result = tmStatus1.Equals(tmStatus2);

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongS2Flags(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);
        var tmStatus2 = new TmStatus(ch, rtu, point) {S2Flags = TmS2Flags.Break};

        var result = tmStatus1.Equals(tmStatus2);

        Assert.False(result);
      }
    }


    public class EqualityOperator
    {
      [Theory, TmAutoData]
      public void ReturnsFalseForNull(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);

        var result = tmStatus1 == null;

        Assert.False(result);
      }


      [Fact]
      public void ReturnsTrueForNullWhenNull()
      {
        TmStatus tmStatus1 = null;

        var result = tmStatus1 == null;

        Assert.True(result);
      }


      [Theory, TmAutoData]
      public void ReturnsTrue(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);
        var tmStatus2 = new TmStatus(ch, rtu, point);

        var result = tmStatus1 == tmStatus2;

        Assert.True(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongAddr(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);
        var tmStatus2 = new TmStatus(ch, rtu, point + 1);

        var result = tmStatus1 == tmStatus2;

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongStatus(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);
        var tmStatus2 = new TmStatus(ch, rtu, point) {Status = 1};

        var result = tmStatus1 == tmStatus2;

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongFlags(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);
        var tmStatus2 = new TmStatus(ch, rtu, point) {Flags = TmFlags.Unreliable};

        var result = tmStatus1 == tmStatus2;

        Assert.False(result);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongS2Flags(int ch, int rtu, int point)
      {
        var tmStatus1 = new TmStatus(ch, rtu, point);
        var tmStatus2 = new TmStatus(ch, rtu, point) {S2Flags = TmS2Flags.Break};

        var result = tmStatus1 == tmStatus2;

        Assert.False(result);
      }
    }


    public class FromCommonPointMethod
    {
      [Fact]
      public void DoesNotInitWithTmFlagsInvalid()
      {
        var tmStatus = new TmStatus(0, 1, 1);
        var tmcCommonPoint = new TmNativeDefs.TCommonPoint
        {
          TM_Flags = 0xFFFF,
          Data     = new byte[] {0},
        };

        tmStatus.FromTmcCommonPoint(tmcCommonPoint);

        Assert.False(tmStatus.IsInit);
      }


      [Fact]
      public void DoesNotInitWithStatusDataNull()
      {
        var tmStatus       = new TmStatus(0, 1, 1);
        var tmcCommonPoint = new TmNativeDefs.TCommonPoint();

        tmStatus.FromTmcCommonPoint(tmcCommonPoint);

        Assert.False(tmStatus.IsInit);
      }


      [Theory]
      [InlineData(1, 0,                                      0)]
      [InlineData(0, (short) TmNativeDefs.Flags.UnreliableHdw, 0)]
      [InlineData(0,
        (short) (TmNativeDefs.Flags.UnreliableManu | TmNativeDefs.Flags.ManuallySet),
        (short) TmNativeDefs.S2Flags.Break)]
      public void SetsCorrectValues(short status, short flags, ushort s2Flags)
      {
        var tmStatus = new TmStatus(0, 1, 1);
        var tmcCommonPoint = new TmNativeDefs.TCommonPoint
        {
          TM_Flags = 1,
          tm_s2    = s2Flags,
          Data     = new short[] {status, flags}.SelectMany(BitConverter.GetBytes).ToArray(),
        };

        tmStatus.FromTmcCommonPoint(tmcCommonPoint);

        Assert.True(tmStatus.IsInit);
        Assert.Equal(status,  tmStatus.Status);
        Assert.Equal(flags,   (short) tmStatus.Flags);
        Assert.Equal(s2Flags, (ushort) tmStatus.S2Flags);
      }


      [Theory]
      [InlineData((short) TmNativeDefs.Flags.StatusClassAps)]
      [InlineData((short) (TmNativeDefs.Flags.UnreliableHdw | TmNativeDefs.Flags.StatusClassAps))]
      public void SetsApsTrue(short flags)
      {
        var tmStatus = new TmStatus(0, 1, 1);
        var tmcCommonPoint = new TmNativeDefs.TCommonPoint
        {
          TM_Flags = 1,
          Data     = new short[] {0, flags}.SelectMany(BitConverter.GetBytes).ToArray()
        };

        tmStatus.FromTmcCommonPoint(tmcCommonPoint);

        Assert.True(tmStatus.IsAps);
      }


      [Theory]
      [InlineData(0)]
      [InlineData((short) (TmNativeDefs.Flags.UnreliableHdw | TmNativeDefs.Flags.Unacked))]
      public void SetsApsFalse(short flags)
      {
        var tmStatus = new TmStatus(0, 1, 1);
        var tmcCommonPoint = new TmNativeDefs.TCommonPoint
        {
          TM_Flags = 1,
          Data     = new short[] {0, flags}.SelectMany(BitConverter.GetBytes).ToArray()
        };

        tmStatus.FromTmcCommonPoint(tmcCommonPoint);

        Assert.False(tmStatus.IsAps);
      }
    }


    public class SetSqlPropertiesAndClassData // todo importance, normalStatus
    {
      [Theory]
      [InlineData("ТС", 0, "отключен", "включен", "обрыв", "неиспр")]
      public void SetsCorrectValuesWithClass(string name, short classId, string text0, string text1, string textB,
                                             string textM)
      {
        var tmStatus = new TmStatus(0, 1, 1);

        tmStatus.SetSqlPropertiesAndClassData(name, 0, -1, classId, text0, text1, textB, textM);

        Assert.Equal(name,           tmStatus.Name);
        Assert.Equal((byte) classId, tmStatus.ClassId);
        Assert.Equal(text0,          tmStatus.GetClassCaption(TmStatus.ClassCaption.Off));
        Assert.Equal(text1,          tmStatus.GetClassCaption(TmStatus.ClassCaption.On));
        Assert.Equal(textB,          tmStatus.GetClassCaption(TmStatus.ClassCaption.Break));
        Assert.Equal(textM,          tmStatus.GetClassCaption(TmStatus.ClassCaption.Malfunction));
      }


      [Theory]
      [InlineData("ТС", -1, "отключен", "включен", "обрыв", "неиспр")]
      public void SetsCorrectValuesWithNoClass(string name, short classId, string text0, string text1, string textB,
                                               string textM)
      {
        var tmStatus = new TmStatus(0, 1, 1);

        tmStatus.SetSqlPropertiesAndClassData(name, 0, -1, classId, text0, text1, textB, textM);

        Assert.Equal(name, tmStatus.Name);
        Assert.Null(tmStatus.ClassId);
        Assert.Equal("ОТКЛ", tmStatus.GetClassCaption(TmStatus.ClassCaption.Off));
        Assert.Equal("ВКЛ",  tmStatus.GetClassCaption(TmStatus.ClassCaption.On));
      }
    }


    private static TmStatus.ClassCaption HelperGetClassCaptionFor(short status, TmS2Flags s2Flags)
    {
      if (s2Flags == TmS2Flags.Break) return TmStatus.ClassCaption.Break;
      if (s2Flags == TmS2Flags.Malfunction) return TmStatus.ClassCaption.Malfunction;
      if (s2Flags == TmS2Flags.Intermediate) return TmStatus.ClassCaption.Intermediate;
      if (status  == 0) return TmStatus.ClassCaption.Off;
      if (status  == 1) return TmStatus.ClassCaption.On;

      return 0;
    }
  }
}