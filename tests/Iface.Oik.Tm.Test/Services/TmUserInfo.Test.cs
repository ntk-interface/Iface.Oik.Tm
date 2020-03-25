using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Xunit;

namespace Iface.Oik.Tm.Test.Services
{
  public class TmUserInfoTest
  {
    public class Constructor
    {
      [Theory, TmAutoData]
      public void SetsCorrectValues(int    id, string name, string category, string keyId, byte group,
                                    byte[] permissionBytes)
      {
        var userInfo = new TmUserInfo(id, name, category, keyId, group, permissionBytes);

        userInfo.Id.Should().Be(id);
        userInfo.Name.Should().Be(name);
        userInfo.KeyId.Should().Be(keyId);
        userInfo.GroupId.Should().Be(group);
      }
    }


    public class HasAccessMethod
    {
      [Theory]
      [TmInlineAutoData(new byte[] {1, 0, 1}, (TmUserPermissions) 0,    true)]
      [TmInlineAutoData(new byte[] {1, 0, 1}, (TmUserPermissions) 1,    false)]
      [TmInlineAutoData(new byte[] {1, 0, 1}, (TmUserPermissions) 2,    true)]
      [TmInlineAutoData(new byte[] {1, 0, 1}, (TmUserPermissions) 1000, false)]
      public void ReturnsCorrectValues(byte[]            permissionBytes,
                                       TmUserPermissions permission,
                                       bool              expected,
                                       int               id,
                                       string            name,
                                       string            category,
                                       string            keyId,
                                       byte              group)
      {
        var userInfo = new TmUserInfo(id, name, category, keyId, group, permissionBytes);

        var result = userInfo.HasAccess(permission);

        result.Should().Be(expected);
      }
    }
  }
}