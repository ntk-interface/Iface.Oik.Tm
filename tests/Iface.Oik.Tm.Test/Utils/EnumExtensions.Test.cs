using System.ComponentModel;
using FluentAssertions;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class EnumExtensionsTest
  {
    public enum DummyEnum
    {
      [Description("Desc A")]  A,
      [Description("Descr B")] B
    }


    public class GetDescriptionMethod
    {
      [Theory]
      [InlineData(DummyEnum.A, "Desc A")]
      [InlineData(DummyEnum.B, "Descr B")]
      public void ReturnsCorrectDescription(DummyEnum value, string expected)
      {
        var result = value.GetDescription();

        result.Should().Be(expected);
      }
    }
  }
}