using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AutoFixture.Xunit2;
using FakeItEasy;
using FluentAssertions;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Api
{
  public class TmsTest
  {
    public class RetroConst
    {
      public const int  Timezone     = DateUtil.Hour * 5;
      public const long UtcStartTime = 1514505600;
      public const int  Step         = 5;
      public const int  Count        = 3;
      public const long UtcMidTime   = UtcStartTime + Step;
      public const long UtcEndTime   = UtcStartTime + Step * 2;

      public const float Value1 = 10;
      public const float Value2 = 1337.0f;
      public const float Value3 = 0;

      public static readonly DateTime StartDateTime = new DateTime(2017, 12, 29, 0, 0, 0);
      public static readonly DateTime EndDateTime   = new DateTime(2017, 12, 29, 0, 0, 10);

      public const string StringStartTime = "29.12.2017 00:00:00";
      public const string StringEndTime   = "29.12.2017 00:00:10";

      public static readonly TmNativeDefs.TAnalogPointShort[] AnalogPointShortList =
      {
        new TmNativeDefs.TAnalogPointShort {Value = Value1, Flags = 0},
        new TmNativeDefs.TAnalogPointShort {Value = Value2, Flags = 0},
        new TmNativeDefs.TAnalogPointShort {Value = Value3, Flags = 0}
      };

      public static readonly List<TmAnalogRetro> TmAnalogRetroList = new List<TmAnalogRetro>
      {
        new TmAnalogRetro(Value1, 0, UtcStartTime),
        new TmAnalogRetro(Value2, 0, UtcMidTime),
        new TmAnalogRetro(Value3, 0, UtcEndTime),
      };

      public static readonly TmNativeDefs.TMAAN_ARCH_VALUE[] AanInstantList =
      {
        new TmNativeDefs.TMAAN_ARCH_VALUE {Value = Value1, Ut = (uint) UtcStartTime},
        new TmNativeDefs.TMAAN_ARCH_VALUE {Value = Value2, Ut = (uint) UtcMidTime},
        new TmNativeDefs.TMAAN_ARCH_VALUE {Value = Value3, Ut = (uint) UtcEndTime},
      };

      public static readonly List<TmAnalogImpulseArchiveInstant> TmAnalogImpulseArchiveInstantList =
        new List<TmAnalogImpulseArchiveInstant>
        {
          new TmAnalogImpulseArchiveInstant(Value1, 0, (uint) UtcStartTime, 0),
          new TmAnalogImpulseArchiveInstant(Value2, 0, (uint) UtcMidTime,   0),
          new TmAnalogImpulseArchiveInstant(Value3, 0, (uint) UtcEndTime,   0),
        };

      public static readonly TmNativeDefs.TMAAN_ARCH_VALUE[] AanAverageList =
      {
        new TmNativeDefs.TMAAN_ARCH_VALUE
        {
          Tag   = (byte) TmNativeDefs.ImpulseArchiveFlags.Min,
          Value = Value1 - 1,
          Ut    = (uint) UtcStartTime,
        },
        new TmNativeDefs.TMAAN_ARCH_VALUE
        {
          Tag   = (byte) TmNativeDefs.ImpulseArchiveFlags.Max,
          Value = Value1 + 1,
          Ut    = (uint) UtcStartTime,
        },
        new TmNativeDefs.TMAAN_ARCH_VALUE
        {
          Tag   = (byte) TmNativeDefs.ImpulseArchiveFlags.Avg,
          Value = Value1,
          Ut    = (uint) UtcStartTime,
        },

        new TmNativeDefs.TMAAN_ARCH_VALUE
        {
          Tag   = (byte) TmNativeDefs.ImpulseArchiveFlags.Min,
          Value = Value2 - 1,
          Ut    = (uint) UtcMidTime,
        },
        new TmNativeDefs.TMAAN_ARCH_VALUE
        {
          Tag   = (byte) TmNativeDefs.ImpulseArchiveFlags.Max,
          Value = Value2 + 1,
          Ut    = (uint) UtcMidTime,
        },
        new TmNativeDefs.TMAAN_ARCH_VALUE
        {
          Tag   = (byte) TmNativeDefs.ImpulseArchiveFlags.Avg,
          Value = Value2,
          Ut    = (uint) UtcMidTime,
        },

        new TmNativeDefs.TMAAN_ARCH_VALUE
        {
          Tag   = (byte) TmNativeDefs.ImpulseArchiveFlags.Min,
          Value = Value3 - 1,
          Ut    = (uint) UtcEndTime,
        },
        new TmNativeDefs.TMAAN_ARCH_VALUE
        {
          Tag   = (byte) TmNativeDefs.ImpulseArchiveFlags.Max,
          Value = Value3 + 1,
          Ut    = (uint) UtcEndTime,
        },
        new TmNativeDefs.TMAAN_ARCH_VALUE
        {
          Tag   = (byte) TmNativeDefs.ImpulseArchiveFlags.Avg,
          Value = Value3,
          Ut    = (uint) UtcEndTime,
        },
      };

      public static readonly List<TmAnalogImpulseArchiveAverage> TmAnalogImpulseArchiveAverageList =
        new List<TmAnalogImpulseArchiveAverage>
        {
          new TmAnalogImpulseArchiveAverage(Value1, Value1 - 1, Value1 + 1, 0, (uint) UtcMidTime,        0), // след!
          new TmAnalogImpulseArchiveAverage(Value2, Value2 - 1, Value2 + 1, 0, (uint) UtcEndTime,        0),
          new TmAnalogImpulseArchiveAverage(Value3, Value3 - 1, Value3 + 1, 0, (uint) UtcEndTime + Step, 0),
        };
    }


    public class GetSystemTimeStringMethod
    {
      [Theory, TmAutoFakeItEasyData]
      public async void ReturnsCorrectTime([Frozen] ITmNative native, TmsApi tms)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); 
        
        var fakeTime         = "12.12.2017 09:14:30";
        var anyByteBuf = new byte[80];
        A.CallTo(() => native.TmcSystemTime(A<int>._, ref anyByteBuf, A<IntPtr>._))
         .WithAnyArguments()
         .AssignsOutAndRefParameters(Encoding.GetEncoding(1251).GetBytes(fakeTime));

        var result = await tms.GetSystemTimeString();

        result.Should().Be(fakeTime);
      }
    }


    public class GetSystemTimeMethod
    {
      [Theory, TmAutoFakeItEasyData]
      public async void ReturnsCorrectTime([Frozen] ITmNative native, TmsApi tms)
      {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); 
        
        var fakeTime   = "12.12.2017 09:14:30";
        var anyByteBuf = new byte[80];
        A.CallTo(() => native.TmcSystemTime(A<int>._, ref anyByteBuf, A<IntPtr>._))
         .WithAnyArguments()
         .AssignsOutAndRefParameters(Encoding.GetEncoding(1251).GetBytes(fakeTime));

        var result = await tms.GetSystemTime();

        result.Should().Be(new DateTime(2017, 12, 12, 9, 14, 30));
      }
    }


    public class GetStatusMethod
    {
      [Theory, TmAutoFakeItEasyData]
      public async void ReturnsCorrectStatus([Frozen] ITmNative native, TmsApi tms,
                                             short              ch,     short  rtu, short point,
                                             short              expected)
      {
        A.CallTo(() => native.TmcStatus(A<int>._, ch, rtu, point))
         .Returns(expected);

        var result = await tms.GetStatus(ch, rtu, point);

        result.Should().Be(expected);
      }
    }


    public class GetAnalogMethod
    {
      [Theory, TmAutoFakeItEasyData]
      public async void ReturnsCorrectAnalogNow([Frozen] ITmNative native, TmsApi tms,
                                                short              ch,     short  rtu, short point,
                                                float              expected)
      {
        A.CallTo(() => native.TmcAnalog(A<int>._, ch, rtu, point, null, 0))
         .Returns(expected);

        var result = await tms.GetAnalog(ch, rtu, point);

        result.Should().Be(expected);
      }
    }


    // TODO al тут надо всё переделать под новые вызовы
    /*public class GetAnalogRetroMethod
    {
      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsNullForInvalidTimes(TmsApi tms)
      {
        var result = await tms.GetAnalogRetro(A.Dummy<TmAddr>(), new TmAnalogRetroFilter(1500000000,
                                                                                         1500000000));

        result.Should().BeNull();
      }


      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForNativeArgs([Frozen] ITmNative native, TmsApi tms,
                                                    short              ch,     short  rtu, short point)
      {
        var analogPointsShort = new TmNativeDefs.TAnalogPointShort[RetroConst.Count];
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcStartTime))
         .Returns(RetroConst.UtcStartTime);
        A.CallTo(() => native.TmcTakeRetroTit(0, ch, rtu, point,
                                              0, 0, 0, 0,
                                              ref analogPointsShort))
         .WithAnyArguments()
         .AssignsOutAndRefParameters(RetroConst.AnalogPointShortList);

        var result = await tms.GetAnalogRetro(new TmAddr(ch, rtu, point),
                                              RetroConst.UtcStartTime,
                                              RetroConst.Count,
                                              RetroConst.Step);

        result.Should().Equal(RetroConst.TmAnalogRetroList);
      }


      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForTimestampArgs([Frozen] ITmNative native, TmsApi tms,
                                                       short              ch,     short  rtu, short point)
      {
        var analogPointsShort = new TmNativeDefs.TAnalogPointShort[RetroConst.Count];
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcStartTime))
         .Returns(RetroConst.UtcStartTime);
        A.CallTo(() => native.TmcTakeRetroTit(0, ch, rtu, point,
                                              0, 0, 0, 0,
                                              ref analogPointsShort))
         .WithAnyArguments()
         .AssignsOutAndRefParameters(RetroConst.AnalogPointShortList);

        var result = await tms.GetAnalogRetro(new TmAddr(ch, rtu, point),
                                              new TmAnalogRetroFilter(RetroConst.UtcStartTime,
                                                                      RetroConst.UtcEndTime));

        result.Should().Equal(RetroConst.TmAnalogRetroList);
      }


      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForDateTimeArgs([Frozen] ITmNative native, TmsApi tms,
                                                      short              ch,     short  rtu, short point)
      {
        var analogPointsShort = new TmNativeDefs.TAnalogPointShort[RetroConst.Count];
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcStartTime))
         .Returns(RetroConst.UtcStartTime);
        A.CallTo(() => native.TmcTakeRetroTit(0, ch, rtu, point,
                                              0, 0, 0, 0,
                                              ref analogPointsShort))
         .WithAnyArguments()
         .AssignsOutAndRefParameters(RetroConst.AnalogPointShortList);

        var result = await tms.GetAnalogRetro(new TmAddr(ch, rtu, point),
                                              new TmAnalogRetroFilter(DateTime.SpecifyKind(RetroConst.StartDateTime,
                                                                                           DateTimeKind.Utc),
                                                                      DateTime.SpecifyKind(RetroConst.EndDateTime,
                                                                                           DateTimeKind.Utc)));

        result.Should().Equal(RetroConst.TmAnalogRetroList);
      }


      // TODO продумать с этим тестом и вообще запросами со временем и таймзонами сервера/клиента
      /*[Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForStringArgs([Frozen] ITmNative native, TmsApi tms,
                                                    short              ch,     short  rtu, short point)
      {
        var analogPointsShort = new TmNativeDefs.TAnalogPointShort[RetroConst.Count];
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcStartTime))
         .Returns(RetroConst.UtcStartTime);
        A.CallTo(() => native.TmcTakeRetroTit(0, ch, rtu, point,
                                              0, 0, 0, 0,
                                              ref analogPointsShort))
         .WithAnyArguments()
         .AssignsOutAndRefParameters(RetroConst.AnalogPointShortList);

        var result = await tms.GetAnalogRetro(new TmAddr(ch, rtu, point),
                                              RetroConst.StringStartTime,
                                              RetroConst.StringEndTime);

        result.Should().Equal(RetroConst.TmAnalogRetroList);
      }#1#
    }


    public class GetImpulseArchiveInstandMethod
    {
      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsNullForInvalidTimes(TmsApi tms)
      {
        var result = await tms.GetImpulseArchiveInstant(A.Dummy<TmAddr>(), new TmAnalogRetroFilter(1500000001,
                                                                                                   1500000000));

        result.Should().BeNull();
      }


      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForTimestampArgs([Frozen] ITmNative native, TmsApi tms,
                                                       short              ch,     short  rtu, short point)
      {
        var  pinnedAanList = GCHandle.Alloc(RetroConst.AanInstantList, GCHandleType.Pinned);
        uint aanCount;
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcStartTime))
         .Returns(RetroConst.UtcStartTime);
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcEndTime))
         .Returns(RetroConst.UtcEndTime);
        A.CallTo(() => native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                                0, 0,
                                                0, 0, out aanCount, null, IntPtr.Zero))
         .WithAnyArguments()
         .Returns(pinnedAanList.AddrOfPinnedObject())
         .AssignsOutAndRefParameters((uint) RetroConst.Count);

        pinnedAanList.Free();

        var result = await tms.GetImpulseArchiveInstant(new TmAddr(ch, rtu, point),
                                                        new TmAnalogRetroFilter(RetroConst.UtcStartTime,
                                                                                RetroConst.UtcEndTime));

        result.Should().Equal(RetroConst.TmAnalogImpulseArchiveInstantList);
      }


      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForDateTimeArgs([Frozen] ITmNative native, TmsApi tms,
                                                      short              ch,     short  rtu, short point)
      {
        var  pinnedAanList = GCHandle.Alloc(RetroConst.AanInstantList, GCHandleType.Pinned);
        uint aanCount;
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcStartTime))
         .Returns(RetroConst.UtcStartTime);
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcEndTime))
         .Returns(RetroConst.UtcEndTime);
        A.CallTo(() => native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                                0, 0,
                                                0, 0, out aanCount, null, IntPtr.Zero))
         .WithAnyArguments()
         .Returns(pinnedAanList.AddrOfPinnedObject())
         .AssignsOutAndRefParameters((uint) RetroConst.Count);

        pinnedAanList.Free();

        var result = await tms.GetImpulseArchiveInstant(new TmAddr(ch, rtu, point),
                                                        new TmAnalogRetroFilter(RetroConst.StartDateTime,
                                                                                RetroConst.EndDateTime));

        result.Should().Equal(RetroConst.TmAnalogImpulseArchiveInstantList);
      }


      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForStringArgs([Frozen] ITmNative native, TmsApi tms,
                                                    short              ch,     short  rtu, short point)
      {
        var  pinnedAanList = GCHandle.Alloc(RetroConst.AanInstantList, GCHandleType.Pinned);
        uint aanCount;
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcStartTime))
         .Returns(RetroConst.UtcStartTime);
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcEndTime))
         .Returns(RetroConst.UtcEndTime);
        A.CallTo(() => native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                                0, 0,
                                                0, 0, out aanCount, null, IntPtr.Zero))
         .WithAnyArguments()
         .Returns(pinnedAanList.AddrOfPinnedObject())
         .AssignsOutAndRefParameters((uint) RetroConst.Count);

        pinnedAanList.Free();

        var result = await tms.GetImpulseArchiveInstant(new TmAddr(ch, rtu, point),
                                                        new TmAnalogRetroFilter(RetroConst.StringStartTime,
                                                                                RetroConst.StringEndTime));

        result.Should().Equal(RetroConst.TmAnalogImpulseArchiveInstantList);
      }
    }


    public class GetImpulseArchiveAverageMethod
    {
      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsNullForInvalidTimes(TmsApi tms)
      {
        var result = await tms.GetImpulseArchiveAverage(new TmAddr(0, 1, 1), new TmAnalogRetroFilter(1500000001,
                                                                                                     1500000000));

        result.Should().BeNull();
      }


      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForTimestampArgs([Frozen] ITmNative native, TmsApi tms,
                                                       short              ch,     short  rtu, short point)
      {
        var  pinnedAanList = GCHandle.Alloc(RetroConst.AanAverageList, GCHandleType.Pinned);
        uint aanCount;
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcStartTime))
         .Returns(RetroConst.UtcStartTime);
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcEndTime))
         .Returns(RetroConst.UtcEndTime);
        A.CallTo(() => native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                                0, 0,
                                                0, 0, out aanCount, null, IntPtr.Zero))
         .WithAnyArguments()
         .Returns(pinnedAanList.AddrOfPinnedObject())
         .AssignsOutAndRefParameters((uint) RetroConst.Count * 3); // среднее+мин+макс

        pinnedAanList.Free();

        var result = await tms.GetImpulseArchiveAverage(new TmAddr(ch, rtu, point),
                                                        new TmAnalogRetroFilter(RetroConst.UtcStartTime,
                                                                                RetroConst.UtcEndTime));

        result.Should().Equal(RetroConst.TmAnalogImpulseArchiveAverageList);
      }


      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForDateTimeArgs([Frozen] ITmNative native, TmsApi tms,
                                                      short              ch,     short  rtu, short point)
      {
        var  pinnedAanList = GCHandle.Alloc(RetroConst.AanAverageList, GCHandleType.Pinned);
        uint aanCount;
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcStartTime))
         .Returns(RetroConst.UtcStartTime);
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcEndTime))
         .Returns(RetroConst.UtcEndTime);
        A.CallTo(() => native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                                0, 0,
                                                0, 0, out aanCount, null, IntPtr.Zero))
         .WithAnyArguments()
         .Returns(pinnedAanList.AddrOfPinnedObject())
         .AssignsOutAndRefParameters((uint) RetroConst.Count * 3); // среднее+мин+макс

        pinnedAanList.Free();

        var result = await tms.GetImpulseArchiveAverage(new TmAddr(ch, rtu, point),
                                                        new TmAnalogRetroFilter(RetroConst.StartDateTime,
                                                                                RetroConst.EndDateTime));

        result.Should().Equal(RetroConst.TmAnalogImpulseArchiveAverageList);
      }


      [Theory, TmAutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForStringArgs([Frozen] ITmNative native, TmsApi tms,
                                                    short              ch,     short  rtu, short point)
      {
        var  pinnedAanList = GCHandle.Alloc(RetroConst.AanAverageList, GCHandleType.Pinned);
        uint aanCount;
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcStartTime))
         .Returns(RetroConst.UtcStartTime);
        A.CallTo(() => native.UxGmTime2UxTime(RetroConst.UtcEndTime))
         .Returns(RetroConst.UtcEndTime);
        A.CallTo(() => native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                                0, 0,
                                                0, 0, out aanCount, null, IntPtr.Zero))
         .WithAnyArguments()
         .Returns(pinnedAanList.AddrOfPinnedObject())
         .AssignsOutAndRefParameters((uint) RetroConst.Count * 3); // среднее+мин+макс

        pinnedAanList.Free();

        var result = await tms.GetImpulseArchiveAverage(new TmAddr(ch, rtu, point),
                                                        new TmAnalogRetroFilter(RetroConst.StringStartTime,
                                                                                RetroConst.StringEndTime));

        result.Should().Equal(RetroConst.TmAnalogImpulseArchiveAverageList);
      }
    }*/


    public class GetFilesInDirectoryMethod
    {
      [Theory, TmAutoFakeItEasyData]
      public async void ReturnsNullWhenCfCidFails([Frozen] ITmNative native, TmsApi tms)
      {
        A.CallTo(() => native.TmcGetCfsHandle(A<int>._))
         .Returns(IntPtr.Zero);

        var result = await tms.GetFilesInDirectory(A.Dummy<string>());

        result.Should().BeNull();
      }


      [Theory, TmAutoFakeItEasyData]
      public async void ReturnsNullWhenWhenTmconnReturnsFalse([Frozen] ITmNative native, TmsApi tms)
      {
        uint error;
        var  errorBuf = new byte[80];
        uint bufLength          = 80;
        var  buf                = new char[bufLength];
        A.CallTo(() => native.TmcGetCfsHandle(A<int>._))
         .Returns(new IntPtr(1));
        A.CallTo(() => native.CfsDirEnum(new IntPtr(1), "", ref buf, bufLength, out error, ref errorBuf, 0))
         .WithAnyArguments()
         .Returns(false);

        var result = await tms.GetFilesInDirectory(A.Dummy<string>());

        result.Should().BeNull();
      }


      [Theory, TmAutoFakeItEasyData]
      public async void ReturnsCorrectList([Frozen] ITmNative native, TmsApi tms)
      {
        uint error;
        var  errorBuf = new byte[80];
        uint bufLength          = 80;
        var  buf                = new char[bufLength];
        A.CallTo(() => native.TmcGetCfsHandle(A<int>._))
         .Returns(new IntPtr(1));
        A.CallTo(() => native.CfsDirEnum(new IntPtr(1), "", ref buf, bufLength, out error, ref errorBuf, 0))
         .WithAnyArguments()
         .Returns(true)
         .AssignsOutAndRefParameters(new[]
         {
           'I', 't', 'e', 'm', '1', '\0',
           'I', 't', 'e', 'm', '2', '\0', '\0',
           'T', 'r', 'a', 's', 'h'
         }, A.Dummy<uint>(), A.Dummy<byte[]>());

        var result = await tms.GetFilesInDirectory(A.Dummy<string>());

        result.Should().Equal("Item1", "Item2");
      }
    }


    public class DownloadFileMethod
    {
      [Theory, TmAutoFakeItEasyData]
      public async void ReturnsFalseWhenCfCidFails([Frozen] ITmNative native, TmsApi tms)
      {
        A.CallTo(() => native.TmcGetCfsHandle(A<int>._))
         .Returns(IntPtr.Zero);

        var result = await tms.DownloadFile(A.Dummy<string>(), A.Dummy<string>());

        result.Should().BeFalse();
      }


      [Theory, TmAutoFakeItEasyData]
      public async void ReturnsFalseWhenTmconnReturnsFalse([Frozen] ITmNative native, TmsApi tms)
      {
        uint error;
        var  errorBuf = new byte[80];
        A.CallTo(() => native.TmcGetCfsHandle(A<int>._))
         .Returns(new IntPtr(1));
        A.CallTo(() => native.CfsFileGet(new IntPtr(1), "", "", 0, IntPtr.Zero, out error, ref errorBuf, 0))
         .WithAnyArguments()
         .Returns(false);

        var result = await tms.DownloadFile(A.Dummy<string>(), A.Dummy<string>());

        result.Should().BeFalse();
      }


      [Theory, TmAutoFakeItEasyData]
      public async void ReturnsFalseWhenNoFileFound([Frozen] ITmNative native, TmsApi tms)
      {
        uint error;
        var  errorBuf = new byte[80];
        A.CallTo(() => native.TmcGetCfsHandle(A<int>._))
         .Returns(new IntPtr(1));
        A.CallTo(() => native.CfsFileGet(new IntPtr(1), "", "", 0, IntPtr.Zero, out error, ref errorBuf, 0))
         .WithAnyArguments()
         .Returns(true);

        var result = await tms.DownloadFile(A.Dummy<string>(), A.Dummy<string>());

        result.Should().BeFalse();
      }
    }
  }
}