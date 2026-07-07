using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmTagTest
  {
    public class CreateMethod
    {
      [Fact]
      public void ReturnsNullWhenAddrIsNull()
      {
        var result = TmTag.Create(null);

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsNullWhenAddrHasUnknownType()
      {
        var tmAddr = new TmAddr(A.Dummy<uint>());

        var result = TmTag.Create(tmAddr);

        result.Should().BeNull();
      }


      [Theory, TmInlineAutoData]
      public void ReturnsCorrectValueForTmStatusAddr(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(TmType.Status, ch, rtu, point);

        var result = TmTag.Create(tmAddr);

        result.Should().Be(new TmStatus(ch, rtu, point));
      }


      [Theory, TmInlineAutoData]
      public void ReturnsCorrectValueForTmAnalogAddr(int ch, int rtu, int point)
      {
        var tmAddr = new TmAddr(TmType.Analog, ch, rtu, point);

        var result = TmTag.Create(tmAddr);

        result.Should().Be(new TmAnalog(ch, rtu, point));
      }
    }
    
    
    public class NameProperty
    {
      [Theory, TmAutoFakeItEasyData]
      public void ReturnsNameField(TmStatus status)
      {
        var sb = "Name=Выключатель";
        status.UpdatePropertiesFromTmcObject(sb);

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


    public class HasTmProviderProperty
    {
      [Theory, TmAutoFakeItEasyData]
      public void ReturnsTrueForTmStatus(TmStatus status)
      {
        status.UpdatePropertiesFromTmcObject("Provider=123");

        status.HasTmProvider.Should().BeTrue();
      }
      
      [Theory]
      [TmInlineAutoFakeItEasyData("Provider=")]
      [TmInlineAutoFakeItEasyData("Dummy=123")]
      public void ReturnsFalseForTmStatus(string properties, TmStatus status)
      {
        status.UpdatePropertiesFromTmcObject(properties);

        status.HasTmProvider.Should().BeFalse();
      }
      
      [Theory, TmAutoFakeItEasyData]
      public void ReturnsTrueForTmAnalog(TmAnalog analog)
      {
        analog.UpdatePropertiesFromTmcObject("Provider=123");

        analog.HasTmProvider.Should().BeTrue();
      }
      
      [Theory]
      [TmInlineAutoFakeItEasyData("Provider=")]
      [TmInlineAutoFakeItEasyData("Dummy=123")]
      public void ReturnsFalseForTmAnalog(string properties, TmAnalog analog)
      {
        analog.UpdatePropertiesFromTmcObject(properties);

        analog.HasTmProvider.Should().BeFalse();
      }
    }


    public class PropertiesProperty
    {
      [Theory, TmAutoFakeItEasyData]
      public void ReturnsNullForNotInit(TmStatus status)
      {
        status.Properties.Should().BeNull();
      }
    }


    public class ClassDataProperty
    {
      [Theory, TmAutoFakeItEasyData]
      public void ReturnsNullForNotInit(TmStatus status)
      {
        status.ClassData.Should().BeNull();
        status.IsClassDataLoaded.Should().BeFalse();
      }
    }


    public class IsStatusProperty
    {
      [Theory, TmInlineAutoData]
      public void ReturnsTrueForStatus(int ch, int rtu, int point)
      {
        TmTag tag = new TmStatus(ch, rtu, point);
        
        tag.IsStatus.Should().BeTrue();
      }
      
      [Theory, TmInlineAutoData]
      public void ReturnsFalseForNonStatus(int ch, int rtu, int point)
      {
        TmTag tag = new TmAnalog(ch, rtu, point);
        
        tag.IsStatus.Should().BeFalse();
      }
    }


    public class IsAnalogProperty
    {
      [Theory, TmInlineAutoData]
      public void ReturnsTrueForAnalog(int ch, int rtu, int point)
      {
        TmTag tag = new TmAnalog(ch, rtu, point);
        
        tag.IsAnalog.Should().BeTrue();
      }
      
      [Theory, TmInlineAutoData]
      public void ReturnsFalseForNonAnalog(int ch, int rtu, int point)
      {
        TmTag tag = new TmStatus(ch, rtu, point);
        
        tag.IsAnalog.Should().BeFalse();
      }
    }


    public class SetTmcObjectPropertiesMethod
    {
      [Theory, TmAutoFakeItEasyData]
      public void SetsCorrectValuesToStatus(TmStatus status)
      {
        var sb = "Key1=Value1\r\nKey2=0\r\nName=Выключатель";

        status.UpdatePropertiesFromTmcObject(sb);

        status.Name.Should().Be("Выключатель");
        status.Properties.Should().Equal(new Dictionary<string, string>
        {
          {"Key1", "Value1"},
          {"Key2", "0"},
          {"Name", "Выключатель"},
        });
      }


      [Theory]
      [TmInlineAutoFakeItEasyData(0)]
      [TmInlineAutoFakeItEasyData(1)]
      [TmInlineAutoFakeItEasyData(2)]
      [TmInlineAutoFakeItEasyData(3)]
      public void SetsCorrectImportanceToStatus(short    importance,
                                                TmStatus status)
      {
        var sb = $"Importance={importance}";

        status.UpdatePropertiesFromTmcObject(sb);

        status.Importance.Should().Be(importance);
      }


      [Theory]
      [TmInlineAutoFakeItEasyData(0,    0)]
      [TmInlineAutoFakeItEasyData(1,    1)]
      [TmInlineAutoFakeItEasyData(-1,   -1)]
      [TmInlineAutoFakeItEasyData(1337, -1)]
      public void SetsCorrectNormalStatusToStatus(short    normalStatus, short expectedNormalStatus,
                                                  TmStatus status)
      {
        var sb = $"Normal={normalStatus}";

        status.UpdatePropertiesFromTmcObject(sb);

        status.NormalStatus.Should().Be(expectedNormalStatus);
      }


      [Theory, TmAutoFakeItEasyData]
      public void SetsCorrectValuesToAnalog(TmAnalog analog)
      {
        var sb = "Key1=Value1\r\nKey2=0\r\nName=Мощность\r\nUnits=МВт";

        analog.UpdatePropertiesFromTmcObject(sb);

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
      [TmInlineAutoFakeItEasyData("7.3", 7, 3)]
      [TmInlineAutoFakeItEasyData("4.1", 4, 1)]
      public void SetsCorrectFormatToAnalog(string   format, byte expectedWidth, byte expectedPrecision,
                                            TmAnalog analog)
      {
        var sb = $"Format={format}";

        analog.UpdatePropertiesFromTmcObject(sb);

        analog.Width.Should().Be(expectedWidth);
        analog.Precision.Should().Be(expectedPrecision);
      }


      [Theory]
      [TmInlineAutoFakeItEasyData("20", TmTeleregulation.Step)]
      [TmInlineAutoFakeItEasyData("40", TmTeleregulation.Code)]
      [TmInlineAutoFakeItEasyData("60", TmTeleregulation.Value)]
      public void SetsCorrectTeleregulationToAnalog(string   code, TmTeleregulation expectedTeleregulation,
                                                    TmAnalog analog)
      {
        var sb = $"FBFlagsC={code}";

        analog.UpdatePropertiesFromTmcObject(sb);

        analog.Teleregulation.Should().Be(expectedTeleregulation);
      }


      [Theory, TmAutoFakeItEasyData]
      public void SetsCorrectDuplicateKeys(TmStatus status)
      {
        var sb = "A=1\r\nA=2\r\nA=3";
        
        status.UpdatePropertiesFromTmcObject(sb);

        status.Properties.Should().ContainKeys("A", "A_1", "A_2");
      }
    }


    public class SetTmcClassDataMethod
    {
      [Theory, TmAutoFakeItEasyData]
      public void SetsCorrectValues(TmStatus status)
      {
        var str = "0Txt=отключен\r\n1Txt=включен\r\nBTxt=обрыв";
        
        status.UpdateClassDataFromTmcClassData(str);

        status.CaptionOff.Should().Be("отключен");
        status.CaptionOn.Should().Be("включен");
        status.IsClassDataLoaded.Should().BeTrue();
        status.ClassData.Should().Equal(new Dictionary<string, string>
        {
          {"0Txt", "отключен"},
          {"1Txt", "включен"},
          {"BTxt", "обрыв"},
        });
      }
    }
    
    
    public class CreateFromCommonPointDtoMethod
    {
      [Fact]
      public void ReturnsCorrectTmStatus()
      {
        var dto = new TCommonPointDto
        {
          Type           = (ushort) TmNativeDefs.TmDataTypes.Status,
          Ch             = 0,
          Rtu            = 1,
          Point          = 1,
          Name           = "Signal",
          TmFlags        = 1,
          StatusPointDto = new TStatusPointDto { Status = 1, Flags = 0 },
        };

        var result = TmTag.CreateFromCommonPointDto(dto);

        result.Should().BeOfType<TmStatus>();
        result.Name.Should().Be("Signal");
        ((TmStatus) result).Status.Should().Be(1);
      }


      [Fact]
      public void ReturnsCorrectTmAnalog()
      {
        var dto = new TCommonPointDto
        {
          Type           = (ushort) TmNativeDefs.TmDataTypes.Analog,
          Ch             = 0,
          Rtu            = 1,
          Point          = 1,
          Name           = "Measurement",
          TmFlags        = 1,
          AnalogPointDto = new TAnalogPointDto { AsFloat = 1.5f, Flags = 0 },
        };

        var result = TmTag.CreateFromCommonPointDto(dto);

        result.Should().BeOfType<TmAnalog>();
        result.Name.Should().Be("Measurement");
        ((TmAnalog) result).Value.Should().Be(1.5f);
      }


      [Fact]
      public void ReturnsNullForUnknownType()
      {
        var dto = new TCommonPointDto
        {
          Type  = 0xFF, // unknown type
          Ch    = 0,
          Rtu   = 1,
          Point = 1,
        };

        var result = TmTag.CreateFromCommonPointDto(dto);

        result.Should().BeNull();
      }
    }
  }
}