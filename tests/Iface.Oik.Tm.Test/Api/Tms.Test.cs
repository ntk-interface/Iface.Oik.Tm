using System;
using System.Collections.Generic;
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

      public static readonly DateTime StartDateTime = new DateTime(2017, 12, 29, 5, 0, 0);
      public static readonly DateTime EndDateTime   = new DateTime(2017, 12, 29, 5, 1, 0);

      public const string StringStartTime = "29.12.2017 5:00:00";
      public const string StringEndTime   = "29.12.2017 5:00:10";

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
      [Theory, AutoFakeItEasyData]
      public async void ReturnsCorrectTime([Frozen] ITmNative native, TmsApi tms)
      {
        var fakeTime         = "12.12.2017 09:14:30";
        var anyStringBuilder = new StringBuilder(80);
        A.CallTo(() => native.TmcSystemTime(A<int>._, ref anyStringBuilder, A<IntPtr>._))
         .WithAnyArguments()
         .AssignsOutAndRefParameters(new StringBuilder(fakeTime));

        var result = await tms.GetSystemTimeString();

        result.Should().Be(fakeTime);
      }
    }


    public class GetSystemTimeMethod
    {
      [Theory, AutoFakeItEasyData]
      public async void ReturnsCorrectTime([Frozen] ITmNative native, TmsApi tms)
      {
        var fakeTime         = "12.12.2017 09:14:30";
        var anyStringBuilder = new StringBuilder(80);
        A.CallTo(() => native.TmcSystemTime(A<int>._, ref anyStringBuilder, A<IntPtr>._))
         .WithAnyArguments()
         .AssignsOutAndRefParameters(new StringBuilder(fakeTime));

        var result = await tms.GetSystemTime();

        result.Should().Be(new DateTime(2017, 12, 12, 9, 14, 30));
      }
    }


    public class GetStatusMethod
    {
      [Theory, AutoFakeItEasyData]
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
      [Theory, AutoFakeItEasyData]
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


    /*public class GetAnalogRetroMethod
    {
      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsNullForInvalidTimes(TmsApi tms)
      {
        var result = await tms.GetAnalogRetro(new TmAddr(0, 1, 1), 1500000000, 1500000000);

        result.Should().BeNull();
      }


      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForNativeArgs([Frozen] ITmNative native, TmsApi tms,
                                                    short              ch,     short  rtu, short point)
      {
        var analogPointsShort = new TmNativeDefs.TAnalogPointShort[RetroConst.Count];
        native.UxGmTime2UxTime(RetroConst.UtcStartTime)
              .Returns(RetroConst.UtcStartTime);
        native.WhenForAnyArgs(x => x.TmcTakeRetroTit(0, ch, rtu, point,
                                                     0, 0, 0, 0,
                                                     ref analogPointsShort))
              .Do(x => x[8] = RetroConst.AnalogPointShortList);

        var result = await tms.GetAnalogRetro(new TmAddr(ch, rtu, point),
                                              RetroConst.UtcStartTime,
                                              RetroConst.Count,
                                              RetroConst.Step);

        result.Should().Equal(RetroConst.TmAnalogRetroList);
      }


      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForTimestampArgs([Frozen] ITmNative native, TmsApi tms,
                                                       short              ch,     short  rtu, short point)
      {
        var analogPointsShort = new TmNativeDefs.TAnalogPointShort[RetroConst.Count];
        native.UxGmTime2UxTime(RetroConst.UtcStartTime)
              .Returns(RetroConst.UtcStartTime);
        native.WhenForAnyArgs(x => x.TmcTakeRetroTit(0, ch, rtu, point,
                                                     0, 0, 0, 0,
                                                     ref analogPointsShort))
              .Do(x => x[8] = RetroConst.AnalogPointShortList);

        var result = await tms.GetAnalogRetro(new TmAddr(ch, rtu, point),
                                              RetroConst.UtcStartTime,
                                              RetroConst.UtcEndTime);

        result.Should().Equal(RetroConst.TmAnalogRetroList);
      }


      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForDateTimeArgs([Frozen] ITmNative native, TmsApi tms,
                                                      short              ch,     short  rtu, short point)
      {
        var analogPointsShort = new TmNativeDefs.TAnalogPointShort[RetroConst.Count];
        native.UxGmTime2UxTime(RetroConst.UtcStartTime)
              .Returns(RetroConst.UtcStartTime);
        native.WhenForAnyArgs(x => x.TmcTakeRetroTit(0, ch, rtu, point,
                                                     0, 0, 0, 0,
                                                     ref analogPointsShort))
              .Do(x => x[8] = RetroConst.AnalogPointShortList);

        var result = await tms.GetAnalogRetro(new TmAddr(ch, rtu, point),
                                              RetroConst.StartDateTime, RetroConst.EndDateTime);

        Assert.Equal(RetroConst.TmAnalogRetroList, result);
      }


      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForStringArgs([Frozen] ITmNative native, TmsApi tms,
                                                    short              ch,     short  rtu, short point)
      {
        var analogPointsShort = new TmNativeDefs.TAnalogPointShort[RetroConst.Count];
        native.UxGmTime2UxTime(RetroConst.UtcStartTime)
              .Returns(RetroConst.UtcStartTime);
        native.WhenForAnyArgs(x => x.TmcTakeRetroTit(0, ch, rtu, point,
                                                     0, 0, 0, 0,
                                                     ref analogPointsShort))
              .Do(x => x[8] = RetroConst.AnalogPointShortList);

        var result = await tms.GetAnalogRetro(new TmAddr(ch, rtu, point),
                                              RetroConst.StringStartTime, RetroConst.StringEndTime);

        Assert.Equal(RetroConst.TmAnalogRetroList, result);
      }
    }


    public class GetImpulseArchiveInstandMethod
    {
      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsNullForInvalidTimes(TmsApi tms)
      {
        var result = await tms.GetImpulseArchiveInstant(new TmAddr(0, 1, 1), 1500000001, 1500000000);

        Assert.Null(result);
      }


      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForTimestampArgs([Frozen] ITmNative native, TmsApi tms,
                                                       short              ch,     short  rtu, short point)
      {
        GCHandle pinnedAanList = GCHandle.Alloc(RetroConst.AanInstantList, GCHandleType.Pinned);
        native.UxGmTime2UxTime(RetroConst.UtcStartTime)
              .Returns(RetroConst.UtcStartTime);
        native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                 0, 0,
                                 0, 0, out uint _, null, IntPtr.Zero)
              .ReturnsForAnyArgs(x =>
              {
                x[6] = (uint) RetroConst.Count;
                return pinnedAanList.AddrOfPinnedObject();
              });

        var result = await tms.GetImpulseArchiveInstant(new TmAddr(ch, rtu, point),
                                                        RetroConst.UtcStartTime, RetroConst.UtcEndTime);

        Assert.Equal(RetroConst.TmAnalogImpulseArchiveInstantList, result);

        pinnedAanList.Free();
      }


      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForDateTimeArgs([Frozen] ITmNative native, TmsApi tms,
                                                      short              ch,     short  rtu, short point)
      {
        GCHandle pinnedAanList = GCHandle.Alloc(RetroConst.AanInstantList, GCHandleType.Pinned);
        native.UxGmTime2UxTime(RetroConst.UtcStartTime)
              .Returns(RetroConst.UtcStartTime);
        native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                 0, 0,
                                 0, 0, out uint _, null, IntPtr.Zero)
              .ReturnsForAnyArgs(x =>
              {
                x[6] = (uint) RetroConst.Count;
                return pinnedAanList.AddrOfPinnedObject();
              });

        var result = await tms.GetImpulseArchiveInstant(new TmAddr(ch, rtu, point),
                                                        RetroConst.StartDateTime, RetroConst.EndDateTime);

        Assert.Equal(RetroConst.TmAnalogImpulseArchiveInstantList, result);

        pinnedAanList.Free();
      }


      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForStringArgs([Frozen] ITmNative native, TmsApi tms,
                                                    short              ch,     short  rtu, short point)
      {
        GCHandle pinnedAanList = GCHandle.Alloc(RetroConst.AanInstantList, GCHandleType.Pinned);
        native.UxGmTime2UxTime(RetroConst.UtcStartTime)
              .Returns(RetroConst.UtcStartTime);
        native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                 0, 0,
                                 0, 0, out uint _, null, IntPtr.Zero)
              .ReturnsForAnyArgs(x =>
              {
                x[6] = (uint) RetroConst.Count;
                return pinnedAanList.AddrOfPinnedObject();
              });

        var result = await tms.GetImpulseArchiveInstant(new TmAddr(ch, rtu, point),
                                                        RetroConst.StringStartTime, RetroConst.StringEndTime);

        Assert.Equal(RetroConst.TmAnalogImpulseArchiveInstantList, result);

        pinnedAanList.Free();
      }
    }


    public class GetImpulseArchiveAverageMethod
    {
      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsNullForInvalidTimes(TmsApi tms)
      {
        var result = await tms.GetImpulseArchiveAverage(new TmAddr(0, 1, 1), 1500000001, 1500000000);

        Assert.Null(result);
      }


      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForTimestampArgs([Frozen] ITmNative native, TmsApi tms,
                                                       short              ch,     short  rtu, short point)
      {
        GCHandle pinnedAanList = GCHandle.Alloc(RetroConst.AanAverageList, GCHandleType.Pinned);
        native.UxGmTime2UxTime(RetroConst.UtcStartTime)
              .Returns(RetroConst.UtcStartTime);
        native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                 0, 0,
                                 0, 0, out uint _, null, IntPtr.Zero)
              .ReturnsForAnyArgs(x =>
              {
                x[6] = (uint) RetroConst.Count * 3; // среднее+мин+макс
                return pinnedAanList.AddrOfPinnedObject();
              });

        var result = await tms.GetImpulseArchiveAverage(new TmAddr(ch, rtu, point),
                                                        RetroConst.UtcStartTime, RetroConst.UtcEndTime);

        Assert.Equal(RetroConst.TmAnalogImpulseArchiveAverageList, result);

        pinnedAanList.Free();
      }


      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForDateTimeArgs([Frozen] ITmNative native, TmsApi tms,
                                                      short              ch,     short  rtu, short point)
      {
        GCHandle pinnedAanList = GCHandle.Alloc(RetroConst.AanAverageList, GCHandleType.Pinned);
        native.UxGmTime2UxTime(RetroConst.UtcStartTime)
              .Returns(RetroConst.UtcStartTime);
        native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                 0, 0,
                                 0, 0, out uint _, null, IntPtr.Zero)
              .ReturnsForAnyArgs(x =>
              {
                x[6] = (uint) RetroConst.Count * 3; // среднее+мин+макс
                return pinnedAanList.AddrOfPinnedObject();
              });

        var result = await tms.GetImpulseArchiveAverage(new TmAddr(ch, rtu, point),
                                                        RetroConst.StartDateTime, RetroConst.EndDateTime);

        Assert.Equal(RetroConst.TmAnalogImpulseArchiveAverageList, result);

        pinnedAanList.Free();
      }


      [Theory, AutoFakeItEasyData]
      [UseCulture("ru-RU")]
      public async void ReturnsCorrectForStringArgs([Frozen] ITmNative native, TmsApi tms,
                                                    short              ch,     short  rtu, short point)
      {
        GCHandle pinnedAanList = GCHandle.Alloc(RetroConst.AanAverageList, GCHandleType.Pinned);
        native.UxGmTime2UxTime(RetroConst.UtcStartTime)
              .Returns(RetroConst.UtcStartTime);
        native.TmcAanReadArchive(0, new TmAddr(ch, rtu, point).ToIntegerWithoutPadding(),
                                 0, 0,
                                 0, 0, out uint _, null, IntPtr.Zero)
              .ReturnsForAnyArgs(x =>
              {
                x[6] = (uint) RetroConst.Count * 3; // среднее+мин+макс
                return pinnedAanList.AddrOfPinnedObject();
              });

        var result = await tms.GetImpulseArchiveAverage(new TmAddr(ch, rtu, point),
                                                        RetroConst.StringStartTime, RetroConst.StringEndTime);

        Assert.Equal(RetroConst.TmAnalogImpulseArchiveAverageList, result);

        pinnedAanList.Free();
      }
    }


    public class GetFilesInDirectoryMethod
    {
      [Theory, AutoFakeItEasyData]
      public async void ReturnsNullWhenCfCidFails([Frozen] ITmNative native, TmsApi tms)
      {
        native.TmcGetCfsHandle(0)
              .ReturnsForAnyArgs((uint) 0);

        var result = await tms.GetFilesInDirectory("anyString");

        Assert.Null(result);
      }


      [Theory, AutoFakeItEasyData]
      public async void ReturnsNullWhenWhenTmconnReturnsFalse([Frozen] ITmNative native, TmsApi tms)
      {
        var  anyStringBuilder = new StringBuilder(80);
        uint anyBufLength     = 80;
        var  anyBuf           = new char[anyBufLength];
        native.TmcGetCfsHandle(0)
              .ReturnsForAnyArgs((uint) 1);
        native.CfsDirEnum(1, "", ref anyBuf, anyBufLength, out uint _, ref anyStringBuilder, 0)
              .ReturnsForAnyArgs(x =>
              {
                x[4] = (uint) 13;
                x[5] = new StringBuilder("Error");
                return false;
              });

        var result = await tms.GetFilesInDirectory("anyString");

        Assert.Null(result);
      }


      [Theory, AutoFakeItEasyData]
      public async void ReturnsCorrectList([Frozen] ITmNative native, TmsApi tms)
      {
        var  anyStringBuilder = new StringBuilder(80);
        uint bufLength        = 80;
        var  buf              = new char[bufLength];
        native.TmcGetCfsHandle(0)
              .ReturnsForAnyArgs((uint) 1);
        native.CfsDirEnum(1, "", ref buf, bufLength, out uint _, ref anyStringBuilder, 0)
              .ReturnsForAnyArgs(x =>
              {
                x[2] = new[]
                {
                  'I', 't', 'e', 'm', '1', '\0',
                  'I', 't', 'e', 'm', '2', '\0', '\0',
                  'T', 'r', 'a', 's', 'h'
                };
                return true;
              });

        var result = await tms.GetFilesInDirectory("anyString");

        Assert.Equal(new List<string> {"Item1", "Item2"}, result);
      }
    }


    public class DownloadFileMethod
    {
      [Theory, AutoFakeItEasyData]
      public async void ReturnsFalseWhenCfCidFails([Frozen] ITmNative native, TmsApi tms)
      {
        native.TmcGetCfsHandle(Arg.Any<int>())
              .ReturnsForAnyArgs((uint) 0);

        var result = await tms.DownloadFile("anyString", "anyString");

        result.Should().BeFalse();
      }


      [Theory, AutoFakeItEasyData]
      public async void ReturnsFalseWhenTmconnReturnsFalse([Frozen] ITmNative native, TmsApi tms)
      {
        var anyString        = Arg.Any<string>();
        var anyStringBuilder = new StringBuilder(80);
        native.TmcGetCfsHandle(0)
              .ReturnsForAnyArgs((uint) 1);
        native.CfsFileGet(1, anyString, anyString, 0, IntPtr.Zero, out uint _, ref anyStringBuilder, 0)
              .ReturnsForAnyArgs(false);

        var result = await tms.DownloadFile("anyString", "anyString");

        result.Should().BeTrue();
      }


      [Theory, AutoFakeItEasyData]
      public async void ReturnsFalseWhenNoFileFound([Frozen] ITmNative native, TmsApi tms)
      {
        var anyString        = Arg.Any<string>();
        var anyStringBuilder = new StringBuilder(80);
        native.TmcGetCfsHandle(0)
              .ReturnsForAnyArgs((uint) 1);
        native.CfsFileGet(1, anyString, anyString, 0, IntPtr.Zero, out uint _, ref anyStringBuilder, 0)
              .ReturnsForAnyArgs(true);

        var result = await tms.DownloadFile("anyString", "anyString");

        Assert.False(result);
      }
    }*/
  }
}