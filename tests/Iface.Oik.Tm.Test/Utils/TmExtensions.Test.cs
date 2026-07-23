using System;
using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class TmExtensionsTest
  {
    public class ToNativeTypeMethod
    {
      [Theory]
      [InlineData(TmType.Status,      TmNativeDefs.TmDataTypes.Status)]
      [InlineData(TmType.Analog,      TmNativeDefs.TmDataTypes.Analog)]
      [InlineData(TmType.Accum,       TmNativeDefs.TmDataTypes.Accum)]
      [InlineData(TmType.Channel,     TmNativeDefs.TmDataTypes.Channel)]
      [InlineData(TmType.Rtu,         TmNativeDefs.TmDataTypes.Rtu)]
      [InlineData(TmType.StatusGroup, TmNativeDefs.TmDataTypes.StatusGroup)]
      [InlineData(TmType.AnalogGroup, TmNativeDefs.TmDataTypes.AnalogGroup)]
      [InlineData(TmType.AccumGroup,  TmNativeDefs.TmDataTypes.AccumGroup)]
      [InlineData(TmType.RetroStatus, TmNativeDefs.TmDataTypes.RetroStatus)]
      [InlineData(TmType.RetroAnalog, TmNativeDefs.TmDataTypes.RetroAnalog)]
      [InlineData(TmType.RetroAccum,  TmNativeDefs.TmDataTypes.RetroAccum)]
      public void ReturnsCorrectNativeType(TmType tmType, TmNativeDefs.TmDataTypes expected)
      {
        var result = tmType.ToNativeType();

        result.Should().Be(expected);
      }


      [Fact]
      public void ReturnsZero_ForUnknownType()
      {
        var result = ((TmType)255).ToNativeType();

        result.Should().Be(0);
      }
    }


    public class ToEventLogImportanceByteMethod
    {
      [Theory]
      [InlineData(TmEventImportances.Imp0, 0)]
      [InlineData(TmEventImportances.Imp1, 1)]
      [InlineData(TmEventImportances.Imp2, 2)]
      [InlineData(TmEventImportances.Imp3, 3)]
      public void ReturnsCorrectByte(TmEventImportances importance, byte expected)
      {
        var result = importance.ToEventLogImportanceByte();

        result.Should().Be(expected);
      }


      [Fact]
      public void Throws_ForUnknownImportance()
      {
        Action act = () => ((TmEventImportances)255).ToEventLogImportanceByte();

        act.Should().Throw<TmNativeException>();
      }
    }
  }
}
