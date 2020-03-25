using System.Text;
using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmServerTest
  {
    public class CreateFromIfaceServerMethod
    {
      [Theory]
      [InlineData(new byte[] {73, 102, 112, 99, 111, 114, 101},
                  new byte[] { },
                  1585076992, 1, "Ifpcore", "")]
      public void SetsCorrectValues(byte[] name,
                                    byte[] comment,
                                    int    creationTime,
                                    uint   state,
                                    string expectedName,
                                    string expectedComment)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var ifaceServer = A.Dummy<TmNativeDefs.IfaceServer>();
        ifaceServer.Name         = name;
        ifaceServer.Comment      = comment;
        ifaceServer.State        = state;
        ifaceServer.CreationTime = creationTime;

        var tmServer = TmServer.CreateFromIfaceServer(ifaceServer);

        tmServer.Name.Should().Be(expectedName);
        tmServer.Comment.Should().Be(expectedComment);
        tmServer.State.Should().Be(state);
        tmServer.CreationTime.Should().Be(DateUtil.GetDateTimeFromTimestamp(creationTime));
      }
    }
  }
}