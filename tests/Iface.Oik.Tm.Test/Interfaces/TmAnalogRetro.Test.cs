using System.Globalization;
using FakeItEasy;
using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmAnalogRetroTest
  {
    public class Constructor
    {
      [Theory]
      [InlineData(10,     0,    1514505600, TmFlags.None,                    "12/29/2017 00:00:00")]
      [InlineData(1337.0, 0x30, 1420634301, TmFlags.LevelA | TmFlags.LevelB, "01/07/2015 12:38:21")]
      public void SetsCorrectValues(float   value, short          flags, long time,
                                    TmFlags expectedFlags, string expectedTime)
      {
        var analogRetro = new TmAnalogRetro(value, flags, time);

        Assert.Equal(value,         analogRetro.Value);
        Assert.Equal(expectedFlags, analogRetro.Flags);
        Assert.Equal(expectedTime,  analogRetro.Time.ToString(CultureInfo.InvariantCulture));
        Assert.True(analogRetro.IsValid);
      }


      [Theory]
      [InlineData(float.MaxValue, 0)]
      [InlineData(10,             -1)]
      public void SetsInvalidFlagForInvalidValues(float value, short flags)
      {
        var analogRetro = new TmAnalogRetro(value, flags, A.Dummy<long>());

        analogRetro.IsValid.Should().BeFalse();
      }
    }
  }
}