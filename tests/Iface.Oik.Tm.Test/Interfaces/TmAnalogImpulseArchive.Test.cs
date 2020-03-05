using System.Globalization;
using FakeItEasy;
using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmAnalogImpulseArchiveTest
  {
    public class ImpulseArchiveInstantConstructor
    {
      [Theory]
      [InlineData(10,     0,    1514505600, 0,  TmFlags.None,                    "12/29/2017 00:00:00")]
      [InlineData(1337.0, 0x30, 1420634301, 27, TmFlags.LevelA | TmFlags.LevelB, "01/07/2015 12:38:21")]
      public void SetsCorrectValues(float   value, uint           flags, uint unixTime, ushort ms,
                                    TmFlags expectedFlags, string expectedTime)
      {
        var analog = new TmAnalogImpulseArchiveInstant(value, flags, unixTime, ms);

        analog.Value.Should().Be(value);
        analog.Flags.Should().Be(expectedFlags);
        analog.Time.ToString(CultureInfo.InvariantCulture).Should().Be(expectedTime);
        analog.Time.Millisecond.Should().Be(ms);
        analog.IsUnreliable.Should().BeFalse();
      }


      [Fact]
      public void SetsUnreliableForUnreliableFlag()
      {
        var analog = new TmAnalogImpulseArchiveInstant(A.Dummy<float>(),
                                                       0x01,
                                                       A.Dummy<uint>(), 
                                                       A.Dummy<ushort>());

        analog.IsUnreliable.Should().BeTrue();
      }
    }


    public class ImpulseArchiveAverageConstructor
    {
      [Theory]
      [InlineData(10,  10,  10,  0,    1514505600, 0,  TmFlags.None,                    "12/29/2017 00:00:00")]
      [InlineData(1.3, 2.6, 1.2, 0x30, 1420634301, 27, TmFlags.LevelA | TmFlags.LevelB, "01/07/2015 12:38:21")]
      public void SetsCorrectValues(float   avg, float            min, float max, uint flags, uint unixTime, ushort ms,
                                    TmFlags expectedFlags, string expectedTime)
      {
        var analog = new TmAnalogImpulseArchiveAverage(avg, min, max, flags, unixTime, ms);

        analog.Value.Should().Be(avg);
        analog.AvgValue.Should().Be(avg);
        analog.MinValue.Should().Be(min);
        analog.MaxValue.Should().Be(max);
        analog.Flags.Should().Be(expectedFlags);
        analog.Time.ToString(CultureInfo.InvariantCulture).Should().Be(expectedTime);
        analog.Time.Millisecond.Should().Be(ms);
        analog.IsUnreliable.Should().BeFalse();
      }


      [Fact]
      public void SetsUnreliableForUnreliableFlag()
      {
        var analog = new TmAnalogImpulseArchiveAverage(A.Dummy<float>(), 
                                                       A.Dummy<float>(), 
                                                       A.Dummy<float>(),
                                                       0x01,
                                                       A.Dummy<uint>(), 
                                                       A.Dummy<ushort>());

        analog.IsUnreliable.Should().BeTrue();
      }
    }
  }
}