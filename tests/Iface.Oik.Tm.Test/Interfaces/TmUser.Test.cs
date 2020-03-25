using System.Text;
using FakeItEasy;
using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmUserTest
  {
    public class CreateFromIfaceUserMethod
    {
      [Theory]
      [InlineData( new byte[] {83, 89, 83, 84, 69, 77, 32, 45, 32, 80, 67, 83, 82, 86, 58, 84, 77, 83, 58, 48},
                   new byte[] {40, 83, 89, 83, 84, 69, 77, 44, 32, 84, 77, 83, 32, 40, 68, 101, 108, 116, 97, 41, 41},
                   1585076992 ,"SYSTEM - PCSRV:TMS:0", "(SYSTEM, TMS (Delta))")]
      public void SetsCorrectValues(byte[] name,
                                    byte[] comment,
                                    int    creationTime,
                                    string expectedName,
                                    string expectedComment)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var ifaceUser = A.Dummy<TmNativeDefs.IfaceUser>();
        ifaceUser.Name = name;
        ifaceUser.Comment = comment;
        ifaceUser.CreationTime = creationTime;
        
        var tmUser = TmUser.CreateFromIfaceUser(ifaceUser);
        
        tmUser.Name.Should().Be(expectedName);
        tmUser.Comment.Should().Be(expectedComment);
        tmUser.CreationTime.Should().Be(DateUtil.GetDateTimeFromTimestamp(creationTime));
      }
    }
  }
}