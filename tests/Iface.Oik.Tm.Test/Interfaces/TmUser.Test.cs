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
      [InlineData(new byte[] {83, 89, 83, 84, 69, 77, 32, 45, 32, 80, 67, 83, 82, 86, 58, 84, 77, 83, 58, 48},
                  new byte[] {40, 83, 89, 83, 84, 69, 77, 44, 32, 84, 77, 83, 32, 40, 68, 101, 108, 116, 97, 41, 41},
                  1585076992, "SYSTEM - PCSRV:TMS:0", "(SYSTEM, TMS (Delta))")]
      public void SetsCorrectValues(byte[] name,
                                    byte[] comment,
                                    int    creationTime,
                                    string expectedName,
                                    string expectedComment)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var ifaceUser = A.Dummy<TmNativeDefs.IfaceUser>();
        ifaceUser.Name         = name;
        ifaceUser.Comment      = comment;
        ifaceUser.CreationTime = creationTime;

        var tmUser = TmUser.CreateFromIfaceUser(ifaceUser);

        tmUser.Name.Should().Be(expectedName);
        tmUser.Comment.Should().Be(expectedComment);
        tmUser.CreationTime.Should().Be(DateUtil.GetDateTimeFromTimestamp(creationTime));
      }
    }


    public class EqualsMethod
    {
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnFalseForNull(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser .Equals(null);

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsTrue(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(true);
      }


      [Theory]
      [TmInlineAutoFakeItEasyMutableData(new byte[]
                                         {
                                           83, 89, 83, 84, 69, 77, 32, 45, 32, 80, 67, 83, 82, 86, 58, 84,
                                           77, 83, 58, 48
                                         })]
      public void ReturnsFalseForWrongName(byte[] name, TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Name = name;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmInlineAutoFakeItEasyMutableData(new byte[]
                                         {
                                           40, 83, 89, 83, 84, 69, 77, 44, 32, 84, 77, 83, 32, 40, 68, 101,
                                           108, 116, 97, 41, 41
                                         })]
      public void ReturnsFalseForWrongComment(byte[] comment, TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Comment = comment;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongSignature(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Signature += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongUnique(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Unique += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongThreadId(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Thid += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongParentProcessId(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Pid += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongFlags(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Flags += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongDbgCnt(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.DbgCnt += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongLoudCnt(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.LoudCnt += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongBytesIn(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.BytesIn += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongBytesOut(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.BytesOut += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmInlineAutoFakeItEasyMutableData(1585076992)]
      public void ReturnsFalseForWrongCreationTime(int creationTimeStamp, TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.CreationTime = creationTimeStamp;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongHandle(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Handle += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1.Equals(tmUser2);

        result.Should().Be(false);
      }

      
      
    }
    
    
    public class EqualityOperator
    {
      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnFalseForNull(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser == null;

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsTrue(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(true);
      }

      
      [Fact]
      public void ReturnsTrueForNullWhenNull()
      {
        TmUser tmUser = null;

        var result = tmUser == null;

        result.Should().Be(true);
      }
      

      [Theory]
      [TmInlineAutoFakeItEasyMutableData(new byte[]
                                         {
                                           83, 89, 83, 84, 69, 77, 32, 45, 32, 80, 67, 83, 82, 86, 58, 84,
                                           77, 83, 58, 48
                                         })]
      public void ReturnsFalseForWrongName(byte[] name, TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Name = name;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmInlineAutoFakeItEasyMutableData(new byte[]
                                         {
                                           40, 83, 89, 83, 84, 69, 77, 44, 32, 84, 77, 83, 32, 40, 68, 101,
                                           108, 116, 97, 41, 41
                                         })]
      public void ReturnsFalseForWrongComment(byte[] comment, TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Comment = comment;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongSignature(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Signature += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongUnique(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Unique += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongThreadId(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Thid += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongParentProcessId(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Pid += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongFlags(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Flags += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongDbgCnt(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.DbgCnt += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongLoudCnt(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.LoudCnt += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongBytesIn(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.BytesIn += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongBytesOut(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.BytesOut += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmInlineAutoFakeItEasyMutableData(1585076992)]
      public void ReturnsFalseForWrongCreationTime(int creationTimeStamp, TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.CreationTime = creationTimeStamp;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }


      [Theory]
      [TmAutoFakeItEasyMutableDataAttribute]
      public void ReturnsFalseForWrongHandle(TmNativeDefs.IfaceUser ifaceUser)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var tmUser1 = TmUser.CreateFromIfaceUser(ifaceUser);

        ifaceUser.Handle += 1;
        var tmUser2 = TmUser.CreateFromIfaceUser(ifaceUser);

        var result = tmUser1 == tmUser2;

        result.Should().Be(false);
      }

      
      
    }
  }
}