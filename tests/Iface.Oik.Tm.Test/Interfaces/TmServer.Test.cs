using System.Collections.Generic;
using System.Linq;
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

    
    public class EqualsMethod
    {
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnFalseForNull(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer.Equals(null);

        result.Should().Be(false);
      }

      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsTrue(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(true);
      }


      [Theory]
      [TmInlineAutoFakeItEasyMutableData(new byte[] {73, 102, 112, 99, 111, 114, 101})]
      public void ReturnsFalseForWrongName(byte[] name, TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        ifaceServer.Name = name;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmInlineAutoFakeItEasyMutableData(new byte[] {73, 102, 112})]
      public void ReturnsFalseForWrongComment(byte[] comment, TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        ifaceServer.Comment = comment;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongSignature(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        ifaceServer.Signature += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongUnique(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.Unique += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongProcessId(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.Pid += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongParentProcessId(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.Ppid += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongFlags(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.Flags += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongDbgCnt(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.DbgCnt += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongLoudCnt(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.LoudCnt += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongBytesIn(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.BytesIn += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongBytesOut(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.BytesOut += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongState(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.State += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      [Theory]
      [TmInlineAutoFakeItEasyMutableData(1585076992)]
      public void ReturnsFalseForWrongCreationTime(int creationTimeStamp, TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.CreationTime = creationTimeStamp;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongResState(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.ResState += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongUsers(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var fixture = new Fixture();

        var tmUsers1 = new List<TmUser>{fixture.Create<TmUser>(), fixture.Create<TmUser>()};
        var tmUsers2 = new List<TmUser>{fixture.Create<TmUser>()};
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        tmServer1.Users.AddRange(tmUsers1);
        tmServer2.Users.AddRange(tmUsers2);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongChildren(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var fixture = new Fixture();
        
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
               .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var children1 = new List<TmServer>{fixture.Create<TmServer>(), fixture.Create<TmServer>()};
        var children2 = new List<TmServer>{fixture.Create<TmServer>()};
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        tmServer1.Children.AddRange(children1);
        tmServer2.Children.AddRange(children2);

        var result = tmServer1.Equals(tmServer2);
        
        result.Should().Be(false);
      }
    }
    
    
    public class EqualityOperator
    {
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnFalseForNull(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer == null;

        result.Should().Be(false);
      }
      
      
      [Fact]
      public void ReturnsTrueForNullWhenNull()
      {
        TmServer tmServer = null;

        var result = tmServer == null;

        result.Should().Be(true);
      }

      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsTrue(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        var result = tmServer1 == tmServer2;
        
        result.Should().Be(true);
      }


      [Theory]
      [TmInlineAutoFakeItEasyMutableData(new byte[] {73, 102, 112, 99, 111, 114, 101})]
      public void ReturnsFalseForWrongName(byte[] name, TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        ifaceServer.Name = name;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmInlineAutoFakeItEasyMutableData(new byte[] {73, 102, 112})]
      public void ReturnsFalseForWrongComment(byte[] comment, TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        ifaceServer.Comment = comment;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongSignature(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        ifaceServer.Signature += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);
        
        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongUnique(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.Unique += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongProcessId(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.Pid += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongParentProcessId(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.Ppid += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongFlags(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.Flags += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongDbgCnt(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.DbgCnt += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongLoudCnt(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.LoudCnt += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongBytesIn(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.BytesIn += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongBytesOut(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.BytesOut += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongState(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.State += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      [Theory]
      [TmInlineAutoFakeItEasyMutableData(1585076992)]
      public void ReturnsFalseForWrongCreationTime(int creationTimeStamp, TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.CreationTime = creationTimeStamp;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongResState(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);

        ifaceServer.ResState += 1;
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongUsers(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var fixture = new Fixture();

        var tmUsers1 = new List<TmUser>{fixture.Create<TmUser>(), fixture.Create<TmUser>()};
        var tmUsers2 = new List<TmUser>{fixture.Create<TmUser>()};
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        tmServer1.Users.AddRange(tmUsers1);
        tmServer2.Users.AddRange(tmUsers2);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
      
      
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongChildren(TmNativeDefs.IfaceServer ifaceServer)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        var fixture = new Fixture();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
               .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var children1 = new List<TmServer>{fixture.Create<TmServer>(), fixture.Create<TmServer>()};
        var children2 = new List<TmServer>{fixture.Create<TmServer>()};
        
        var tmServer1 = TmServer.CreateFromIfaceServer(ifaceServer);
        var tmServer2 = TmServer.CreateFromIfaceServer(ifaceServer);

        tmServer1.Children.AddRange(children1);
        tmServer2.Children.AddRange(children2);

        var result = tmServer1 == tmServer2;
        
        result.Should().Be(false);
      }
    }
  }
}