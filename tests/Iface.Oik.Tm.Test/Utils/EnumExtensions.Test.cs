using System.ComponentModel;
using FluentAssertions;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class EnumExtensionsTest
  {
    public class GetDescriptionMethod
    {
      public enum DummyEnum
      {
        [Description("Desc A")]  ValueA,
        [Description("Descr B")] ValueB,
        ValueC,
      }


      [Theory]
      [InlineData(DummyEnum.ValueA, "Desc A")]
      [InlineData(DummyEnum.ValueB, "Descr B")]
      public void ReturnsCorrectDescription(DummyEnum value, string expected)
      {
        var result = value.GetDescription();

        result.Should().Be(expected);
      }


      [Fact]
      public void ReturnsValueWhenDescriptionIsMissing()
      {
        var result = DummyEnum.ValueC.GetDescription();

        result.Should().Be(DummyEnum.ValueC.ToString());
      }
    }
  }
}