using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmAlarmTest
  {
    public class FullNameProperty
    {
      [Theory]
      [UseCulture("ru-RU")]
      [InlineData(1, "Уставка1", 13,  0, "МВт", 0, "Уставка1 > 13 МВт")]
      [InlineData(2, "Уставка2", 220, 1, "В",   2, "Уставка2 < 220,00 В")]
      public void ReturnsCorrectValues(int    id,
                                       string name,
                                       float  compareValue,
                                       short  compareSign,
                                       string unit,
                                       byte   precision,
                                       string expected)
      {
        var tmAnalog = new TmAnalog(id, 1, 1) {Unit = unit, Precision = precision};
        var tmAlarm  = new TmAlarmValue(TmAlarmType.Value, 1, name, 0, 1, true, tmAnalog, compareValue, compareSign);

        var result = tmAlarm.FullName;

        result.Should().Be(expected);
      }
    }
  }
}