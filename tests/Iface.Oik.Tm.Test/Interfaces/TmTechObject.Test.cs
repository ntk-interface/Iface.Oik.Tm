using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmTechObjectTest
  {
    private static readonly string[] Properties1 = {"$V=1", "$G=1", "vl=220", "n=Test1"};
    private static readonly string[] Properties2 = {"$G=1", "$V=1", "vl=220", "n=Test1"};
    private static readonly string[] Properties3 = {"vl=220", "n=Test1"};


    public class SetPropertiesFromTmcMethod
    {
      [Theory, TmAutoFakeItEasyData]
      public void SetsCorrectProperties(TmTechObject tob)
      {
        using (var monitor = tob.Monitor())
        {
          var result = tob.SetPropertiesFromTmc(Properties1);

          result.Should().BeTrue();
          tob.IsInit.Should().BeTrue();
          tob.Properties.Should().HaveCount(4);
          monitor.Should().Raise(nameof(tob.PropertyChanged));
        }
      }


      [Theory, TmAutoFakeItEasyData]
      public void DoesNothingAndReturnsFalseForNull(TmTechObject tob)
      {
        using (var monitor = tob.Monitor())
        {
          var result = tob.SetPropertiesFromTmc(null);

          result.Should().BeFalse();
          tob.IsInit.Should().BeFalse();
          monitor.Should().NotRaise(nameof(tob.PropertyChanged));
        }
      }


      [Theory, TmAutoFakeItEasyData]
      public void DoesNothingAndReturnsFalseForEqualProperties(TmTechObject tob)
      {
        tob.SetPropertiesFromTmc(Properties1);

        using (var monitor = tob.Monitor())
        {
          var result = tob.SetPropertiesFromTmc(Properties2);

          result.Should().BeFalse();
          monitor.Should().NotRaise(nameof(tob.PropertyChanged));
        }
      }


      [Theory, TmAutoFakeItEasyData]
      public void SetsCorrectPropertiesForNewOnes(TmTechObject tob)
      {
        tob.SetPropertiesFromTmc(Properties1);

        using (var monitor = tob.Monitor())
        {
          var result = tob.SetPropertiesFromTmc(Properties3);

          result.Should().BeTrue();
          tob.Properties.Should().HaveCount(2);
          monitor.Should().Raise(nameof(tob.PropertyChanged));
        }
      }
    }


    public class GetPropertyMethod
    {
      [Theory, TmAutoFakeItEasyData]
      public void ReturnsCorrectValues(TmTechObject tob)
      {
        tob.SetPropertiesFromTmc(Properties1);

        var resultV    = tob.GetPropertyOrDefault(TmTechObject.PropertyIsVoltaged);
        var resultG    = tob.GetPropertyOrDefault(TmTechObject.PropertyIsGrounded);
        var resultName = tob.GetPropertyOrDefault(TmTechObject.PropertyName);

        resultV.Should().Be("1");
        resultG.Should().Be("1");
        resultName.Should().Be("Test1");
      }


      [Theory, TmAutoFakeItEasyData]
      public void ReturnsNullForNull(TmTechObject tob)
      {
        tob.SetPropertiesFromTmc(Properties1);

        var result = tob.GetPropertyOrDefault(null);

        result.Should().BeNull();
      }


      [Theory, TmAutoFakeItEasyData]
      public void ReturnsNullForNotDefinedKey(TmTechObject tob)
      {
        tob.SetPropertiesFromTmc(Properties1);

        var result = tob.GetPropertyOrDefault("not-defined");

        result.Should().BeNull();
      }
    }


    public class TopologyStateProperty
    {
      [Theory, TmAutoFakeItEasyData]
      public void ReturnsUnknownForNonInit(TmTechObject tob)
      {
        var result = tob.TopologyState;

        result.Should().Be(TmTopologyState.Unknown);
      }


      [Theory]
      [TmInlineAutoFakeItEasyData(new[] {"$V=1", "$G=1"}, TmTopologyState.IsGrounded)]
      [TmInlineAutoFakeItEasyData(new[] {"$V=0", "$G=1"}, TmTopologyState.IsGrounded)]
      [TmInlineAutoFakeItEasyData(new[] {"$V=1", "$G=0"}, TmTopologyState.IsVoltaged)]
      [TmInlineAutoFakeItEasyData(new[] {"$V=0", "$G=0"}, TmTopologyState.IsNotVoltaged)]
      [TmInlineAutoFakeItEasyData(new[] {"$V=1"},         TmTopologyState.Unknown)]
      [TmInlineAutoFakeItEasyData(new[] {"$G=1"},         TmTopologyState.Unknown)]
      [TmInlineAutoFakeItEasyData(new[] {"dummy"},        TmTopologyState.Unknown)]
      public void ReturnsCorrectValues(string[]        properties,
                                       TmTopologyState expected,
                                       TmTechObject    tob)
      {
        tob.SetPropertiesFromTmc(properties);

        var result = tob.TopologyState;

        result.Should().Be(expected);
      }
    }
  }
}