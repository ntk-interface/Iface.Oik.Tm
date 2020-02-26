using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmTagTest
  {
    public class NameProperty
    {
      [Theory, AutoNSubstituteData]
      public void ReturnsNameField(TmStatus status)
      {
        var sb = new StringBuilder("Name=Выключатель");
        status.SetTmcObjectProperties(sb);

        status.Name.Should().Be("Выключатель");
      }


      [Fact]
      public void ReturnsAddrForNotInit()
      {
        var addr   = new TmAddr(0, 1, 1);
        var status = new TmStatus(addr);

        status.Name.Should().Be(addr.ToString());
      }
    }


    public class PropertiesProperty
    {
      [Theory, AutoNSubstituteData]
      public void ReturnsNullForNotInit(TmStatus status)
      {
        status.Properties.Should().BeNull();
      }
    }


    public class ClassDataProperty
    {
      [Theory, AutoNSubstituteData]
      public void ReturnsNullForNotInit(TmStatus status)
      {
        status.ClassData.Should().BeNull();
      }
    }


    public class SetTmcObjectPropertiesMethod
    {
      [Theory, AutoNSubstituteData]
      public void SetsCorrectValuesToStatus(TmStatus status)
      {
        var sb = new StringBuilder("Key1=Value1\r\nKey2=0\r\nName=Выключатель");

        status.SetTmcObjectProperties(sb);

        status.Name.Should().Be("Выключатель");
        status.Properties.Should().Equal(new Dictionary<string, string>
        {
          {"Key1", "Value1"},
          {"Key2", "0"},
          {"Name", "Выключатель"},
        });
      }


      [Theory]
      [InlineAutoNSubstituteData(0)]
      [InlineAutoNSubstituteData(1)]
      [InlineAutoNSubstituteData(2)]
      [InlineAutoNSubstituteData(3)]
      public void SetsCorrectImportanceToStatus(short    importance,
                                                TmStatus status)
      {
        var sb = new StringBuilder($"Importance={importance}");

        status.SetTmcObjectProperties(sb);

        status.Importance.Should().Be(importance);
      }


      [Theory]
      [InlineAutoNSubstituteData(0,    0)]
      [InlineAutoNSubstituteData(1,    1)]
      [InlineAutoNSubstituteData(-1,   -1)]
      [InlineAutoNSubstituteData(1337, -1)]
      public void SetsCorrectNormalStatusToStatus(short    normalStatus, short expectedNormalStatus,
                                                  TmStatus status)
      {
        var sb = new StringBuilder($"Normal={normalStatus}");

        status.SetTmcObjectProperties(sb);

        status.NormalStatus.Should().Be(expectedNormalStatus);
      }


      [Theory, AutoNSubstituteData]
      public void SetsCorrectValuesToAnalog(TmAnalog analog)
      {
        var sb = new StringBuilder("Key1=Value1\r\nKey2=0\r\nName=Мощность\r\nUnits=МВт");

        analog.SetTmcObjectProperties(sb);

        analog.Name.Should().Be("Мощность");
        analog.Unit.Should().Be("МВт");
        analog.Properties.Should().Equal(new Dictionary<string, string>
        {
          {"Key1", "Value1"},
          {"Key2", "0"},
          {"Name", "Мощность"},
          {"Units", "МВт"},
        });
      }


      [Theory]
      [InlineAutoNSubstituteData("7.3", 7, 3)]
      [InlineAutoNSubstituteData("4.1", 4, 1)]
      public void SetsCorrectFormatToAnalog(string   format, byte expectedWidth, byte expectedPrecision,
                                            TmAnalog analog)
      {
        var sb = new StringBuilder($"Format={format}");

        analog.SetTmcObjectProperties(sb);

        analog.Width.Should().Be(expectedWidth);
        analog.Precision.Should().Be(expectedPrecision);
      }


      [Theory]
      [InlineAutoNSubstituteData("20", TmTeleregulation.Step)]
      [InlineAutoNSubstituteData("40", TmTeleregulation.Code)]
      [InlineAutoNSubstituteData("60", TmTeleregulation.Value)]
      public void SetsCorrectTeleregulationToAnalog(string   code, TmTeleregulation expectedTeleregulation,
                                                    TmAnalog analog)
      {
        var sb = new StringBuilder($"FBFlagsC={code}");

        analog.SetTmcObjectProperties(sb);

        analog.Teleregulation.Should().Be(expectedTeleregulation);
      }
    }


    public class SetTmcClassDataMethod
    {
      [Theory, AutoNSubstituteData]
      public void SetsCorrectValues(TmStatus status)
      {
        var str = "0Txt=отключен\r\n1Txt=включен\r\nBTxt=обрыв";
        
        status.SetTmcClassData(str);

        status.CaptionOff.Should().Be("отключен");
        status.CaptionOn.Should().Be("включен");
        status.ClassData.Should().Equal(new Dictionary<string, string>
        {
          {"0Txt", "отключен"},
          {"1Txt", "включен"},
          {"BTxt", "обрыв"},
        });
      }
    }
  }
}