using FluentAssertions;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class GuidUtilTest
  {
    public class EncodeAndDecodeCimSubstation
    {
      [Theory]
      [InlineData(1)]
      [InlineData(100500)]
      [InlineData(0)]
      public void Roundtrips_Correctly(int schemeId)
      {
        var guid = GuidUtil.EncodeCimSubstation(schemeId);

        var result = GuidUtil.TryDecodeCimSubstation(guid, out var decodedSchemeId);

        result.Should().BeTrue();
        decodedSchemeId.Should().Be(schemeId);
      }
    }


    public class EncodeAndDecodeCimEquipment
    {
      [Theory]
      [InlineData(1, 100)]
      [InlineData(0, 0)]
      public void Roundtrips_Correctly(int schemeId, int objectId)
      {
        var guid = GuidUtil.EncodeCimEquipment(schemeId, objectId);

        var result = GuidUtil.TryDecodeCimEquipment(guid, out var decodedSchemeId, out var decodedObjectId);

        result.Should().BeTrue();
        decodedSchemeId.Should().Be(schemeId);
        decodedObjectId.Should().Be(objectId);
      }
    }


    public class TryDecodeCimSubstation
    {
      [Fact]
      public void ReturnsFalse_ForWrongScope()
      {
        var equipmentGuid = GuidUtil.EncodeCimEquipment(1, 1);

        var result = GuidUtil.TryDecodeCimSubstation(equipmentGuid, out _);

        result.Should().BeFalse();
      }
    }


    public class TryDecodeCimEquipment
    {
      [Fact]
      public void ReturnsFalse_ForWrongScope()
      {
        var substationGuid = GuidUtil.EncodeCimSubstation(1);

        var result = GuidUtil.TryDecodeCimEquipment(substationGuid, out _, out _);

        result.Should().BeFalse();
      }
    }
  }
}
