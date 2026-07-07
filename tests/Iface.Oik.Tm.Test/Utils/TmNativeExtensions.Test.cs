using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class TmNativeExtensionsTest
  {
    public class ToTmTypeMethod
    {
      [Theory]
      [InlineData(TmNativeDefs.TmDataTypes.Status,      TmType.Status)]
      [InlineData(TmNativeDefs.TmDataTypes.Analog,      TmType.Analog)]
      [InlineData(TmNativeDefs.TmDataTypes.Accum,       TmType.Accum)]
      [InlineData(TmNativeDefs.TmDataTypes.Channel,     TmType.Channel)]
      [InlineData(TmNativeDefs.TmDataTypes.Rtu,         TmType.Rtu)]
      [InlineData(TmNativeDefs.TmDataTypes.StatusGroup, TmType.StatusGroup)]
      [InlineData(TmNativeDefs.TmDataTypes.AnalogGroup, TmType.AnalogGroup)]
      [InlineData(TmNativeDefs.TmDataTypes.AccumGroup,  TmType.AccumGroup)]
      [InlineData(TmNativeDefs.TmDataTypes.RetroStatus, TmType.RetroStatus)]
      [InlineData(TmNativeDefs.TmDataTypes.RetroAnalog, TmType.RetroAnalog)]
      [InlineData(TmNativeDefs.TmDataTypes.RetroAccum,  TmType.RetroAccum)]
      public void ReturnsCorrectTmType(TmNativeDefs.TmDataTypes nativeType, TmType expected)
      {
        var result = nativeType.ToTmType();

        result.Should().Be(expected);
      }


      [Fact]
      public void ReturnsUnknown_ForUnknownType()
      {
        var result = ((TmNativeDefs.TmDataTypes)255).ToTmType();

        result.Should().Be(TmType.Unknown);
      }
    }
  }
}
