using System;
using System.IO;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative : ITmNative
  {
    public Int32 TmcConnect(string           server,
                            string           pipe,
                            string           user,
                            TmNativeCallback callback,
                            IntPtr           callbackParameter)
    {
      return tmcConnect(server, pipe, user, callback, callbackParameter);
    }


    public void TmcDisconnect(Int32 cid)
    {
      tmcDisconnect(cid);
    }


    public void TmcFreeMemory(IntPtr memory)
    {
      tmcFreeMemory(memory);
    }


    public UInt32 TmcGetLastError()
    {
      return tmcGetLastError();
    }


    public Int32 TmcGetLastErrorText(Int32  cid,
                                     IntPtr buf)
    {
      return tmcGetLastErrorText(cid, buf);
    }


    public UInt32 TmcSetDgrmFlags(Int32  cid,
                                  UInt32 flags)
    {
      return tmcSetDgrmFlags(cid, flags);
    }


    public UInt32 TmcClrDgrmFlags(Int32  cid,
                                  UInt32 flags)
    {
      return tmcClrDgrmFlags(cid, flags);
    }


    public Int16 TmcSetFeedbackItems(Int32  cid,
                                     UInt16 type,
                                     Int16  ch,
                                     Int16  rtu,
                                     Int16  point,
                                     Byte   count,
                                     Byte   fbType,
                                     UInt32 id)
    {
      return tmcSetFeedbackItems(cid, type, ch, rtu, point, count, fbType, id);
    }


    public Int16 TmcClrFeedback(Int32 cid)
    {
      return tmcClrFeedback(cid);
    }


    public UInt32 TmcGetCfsHandle(Int32 cid)
    {
      return tmcGetCfsHandle(cid);
    }


    public UInt32 TmcReconnectCount(Int32 cid)
    {
      return tmcReconnectCount(cid);
    }


    public UInt32 TmcConnState(Int32 cid)
    {
      return tmcConnState(cid);
    }


    public Int16 TmcSystemTime(Int32             cid,
                               ref StringBuilder time,
                               IntPtr            tmStruct)
    {
      return tmcSystemTime(cid, time, tmStruct);
    }


    public Boolean TmcGetUserInfo(Int32                      cid,
                                  UInt32                     usid,
                                  ref TmNativeDefs.TUserInfo userInfo)
    {
      return tmcGetUserInfo(cid, usid, ref userInfo);
    }


    public Int16 TmcStatus(Int32 cid,
                           Int16 ch,
                           Int16 rtu,
                           Int16 point)
    {
      return tmcStatus(cid, ch, rtu, point);
    }


    public Single TmcAnalog(Int32  cid,
                            Int16  ch,
                            Int16  rtu,
                            Int16  point,
                            string dateTime,
                            Int16  retroNum)
    {
      return tmcAnalog(cid, ch, rtu, point, dateTime, retroNum);
    }


    public IntPtr TmcTmValuesByListEx(Int32                 cid,
                                      UInt16                tmType,
                                      Byte                  qFlags,
                                      UInt32                count,
                                      TmNativeDefs.TAdrTm[] addr)
    {
      return tmcTMValuesByListEx(cid, tmType, qFlags, count, addr);
    }


    public IntPtr TmcGetValuesByFlagMask(Int32      cid,
                                         UInt16     tmType,
                                         UInt32     tmFlags,
                                         Byte       qFlags,
                                         out UInt32 pCount)
    {
      return tmcGetValuesByFlagMask(cid, tmType, tmFlags, qFlags, out pCount);
    }


    public Int16 TmcRegEvent(Int32               cid,
                             TmNativeDefs.TEvent tmEvent)
    {
      return tmcRegEvent(cid, ref tmEvent);
    }


    public Boolean TmcEvlogPutStrBin(Int32  cid,
                                     UInt32 unixTime,
                                     Byte   unixHund,
                                     Byte   importance,
                                     UInt32 sourceTag,
                                     string str,
                                     Byte[] bin,
                                     UInt32 cbBin)
    {
      return tmcEvlogPutStrBin(cid, unixTime, unixHund, importance, sourceTag, str, bin, cbBin);
    }


    public IntPtr TmcEventLogByElix(Int32                     cid,
                                    ref TmNativeDefs.TTMSElix elix,
                                    UInt16                    eventMask,
                                    UInt32                    startUnixTime,
                                    UInt32                    endUnixTime)
    {
      return tmcEventLogByElix(cid, ref elix, eventMask, startUnixTime, endUnixTime);
    }


    public Boolean TmcGetCurrentElix(Int32                     cid,
                                     ref TmNativeDefs.TTMSElix elix)
    {
      return tmcGetCurrentElix(cid, ref elix);
    }


    public Boolean TmcAlertListRemove(Int32                       cid,
                                      TmNativeDefs.TAlertListId[] listIds)
    {
      return tmcAlertListRemove(cid, listIds);
    }


    public Int16 TmcSetTimedValues(Int32                              cid,
                                   UInt32                             count,
                                   TmNativeDefs.TTimedValueAndFlags[] values)
    {
      return tmcSetTimedValues(cid, count, values);
    }


    public Int32 TmcExecuteControlScript(Int32 cid,
                                         Int16 ch,
                                         Int16 rtu,
                                         Int16 point,
                                         Int16 cmd)
    {
      return tmcExecuteControlScript(cid, ch, rtu, point, cmd);
    }


    public Boolean TmcOverrideControlScript(Int32 cid,
                                            bool  fOverride)
    {
      return tmcOverrideControlScript(cid, fOverride);
    }


    public Int16 TmcControlByStatus(Int32 cid,
                                    Int16 ch,
                                    Int16 rtu,
                                    Int16 point,
                                    Int16 cmd)
    {
      return tmcControlByStatus(cid, ch, rtu, point, cmd);
    }


    public Int16 TmcControlCmdResult(Int32  cid,
                                     UInt32 id,
                                     bool   fSuccess)
    {
      return tmcControlCmdResult(cid, id, fSuccess);
    }


    public Int16 TmcRegulationByAnalog(Int32  cid,
                                       Int16  ch,
                                       Int16  rtu,
                                       Int16  point,
                                       Byte   regType,
                                       IntPtr regData)
    {
      return tmcRegulationByAnalog(cid, ch, rtu, point, regType, regData);
    }


    public Int16 TmcSetStatusNormal(Int32  cid,
                                    Int16  ch,
                                    Int16  rtu,
                                    Int16  point,
                                    UInt16 nValue)
    {
      return tmcSetStatusNormal(cid, ch, rtu, point, nValue);
    }


    public Int16 TmcGetStatusNormal(Int32      cid,
                                    Int16      ch,
                                    Int16      rtu,
                                    Int16      point,
                                    out UInt16 nValue)
    {
      return tmcGetStatusNormal(cid, ch, rtu, point, out nValue);
    }


    public Int16 TmcDriverCall(Int32  cid,
                               UInt32 addr,
                               Int16  qCode,
                               Int16  command)
    {
      return tmcDriverCall(cid, addr, qCode, command);
    }


    public Boolean TmcEventLogAckRecords(Int32                     cid,
                                         ref TmNativeDefs.TTMSElix elix,
                                         UInt32                    count)
    {
      return tmcEventLogAckRecords(cid, ref elix, count);
    }


    public void TmcStatusByList(Int32                       cid,
                                UInt16                      count,
                                TmNativeDefs.TAdrTm[]       addr,
                                TmNativeDefs.TStatusPoint[] statuses)
    {
      tmcStatusByList(cid, count, addr, statuses);
    }


    public void TmcAnalogByList(Int32                       cid,
                                UInt16                      count,
                                TmNativeDefs.TAdrTm[]       addr,
                                TmNativeDefs.TAnalogPoint[] analogs)
    {
      tmcAnalogByList(cid, count, addr, analogs);
    }


    public Int16 TmcGetObjectProperties(Int32             cid,
                                        UInt16            objectType,
                                        Int16             ch,
                                        Int16             rtu,
                                        Int16             point,
                                        ref StringBuilder buf,
                                        Int32             bufSize)
    {
      return tmcGetObjectProperties(cid, objectType, ch, rtu, point, buf, bufSize);
    }


    public IntPtr TmcGetStatusClassData(Int32                 cid,
                                        UInt32                count,
                                        TmNativeDefs.TAdrTm[] statuses)
    {
      return tmcGetStatusClassData(cid, count, statuses);
    }


    public IntPtr TmcGetAnalogClassData(Int32                 cid,
                                        UInt32                count,
                                        TmNativeDefs.TAdrTm[] analogs)
    {
      return tmcGetAnalogClassData(cid, count, analogs);
    }


    public void TmcTakeRetroTit(Int32                                cid,
                                Int16                                ch,
                                Int16                                rtu,
                                Int16                                point,
                                UInt32                               unixTime,
                                UInt16                               retroNum,
                                UInt16                               count,
                                UInt16                               step,
                                ref TmNativeDefs.TAnalogPointShort[] analogs)
    {
      tmcTakeRetroTit(cid, ch, rtu, point, unixTime, retroNum, count, step, analogs);
    }


    public Int16 TmcSetStatus(Int32  cid,
                              Int16  ch,
                              Int16  rtu,
                              Int16  point,
                              Byte   value,
                              string dateTime,
                              Int16  hund)
    {
      return tmcSetStatus(cid, ch, rtu, point, value, dateTime, hund);
    }


    public Int16 TmcSetStatusFlags(Int32 cid,
                                   Int16 ch,
                                   Int16 rtu,
                                   Int16 point,
                                   Int16 flags)
    {
      return tmcSetStatusFlags(cid, ch, rtu, point, flags);
    }


    public Int16 TmcClrStatusFlags(Int32 cid,
                                   Int16 ch,
                                   Int16 rtu,
                                   Int16 point,
                                   Int16 flags)
    {
      return tmcClrStatusFlags(cid, ch, rtu, point, flags);
    }


    public Int16 TmcSetAnalog(Int32  cid,
                              Int16  ch,
                              Int16  rtu,
                              Int16  point,
                              Single value,
                              string dateTime)
    {
      return tmcSetAnalog(cid, ch, rtu, point, value, dateTime);
    }


    public Int16 TmcSetAnalogFlags(Int32 cid,
                                   Int16 ch, Int16 rtu, Int16 point,
                                   Int16 flags)
    {
      return tmcSetAnalogFlags(cid, ch, rtu, point, flags);
    }


    public Int16 TmcClrAnalogFlags(Int32 cid,
                                   Int16 ch,
                                   Int16 rtu,
                                   Int16 point,
                                   Int16 flags)
    {
      return tmcClrAnalogFlags(cid, ch, rtu, point, flags);
    }


    public Int16 TmcPeekAlarm(Int32                   cid,
                              Int16                   ch,
                              Int16                   rtu,
                              Int16                   point,
                              Int16                   alarmId,
                              ref TmNativeDefs.TAlarm alarm)
    {
      return tmcPeekAlarm(cid, ch, rtu, point, alarmId, ref alarm);
    }


    public Int16 TmcPokeAlarm(Int32                   cid,
                              Int16                   ch,
                              Int16                   rtu,
                              Int16                   point,
                              Int16                   alarmId,
                              ref TmNativeDefs.TAlarm alarm)
    {
      return tmcPokeAlarm(cid, ch, rtu, point, alarmId, ref alarm);
    }


    public IntPtr TmcAanReadArchive(Int32            cid,
                                    UInt32           tmAddr,
                                    UInt32           startUnixTime,
                                    UInt32           endUnixTime,
                                    UInt32           step,
                                    UInt32           flags,
                                    out UInt32       count,
                                    TmNativeCallback progress,
                                    IntPtr           progressParam)
    {
      return tmcAanReadArchive(cid,       tmAddr,   startUnixTime, endUnixTime, step, flags,
                               out count, progress, progressParam);
    }


    public UInt32 String2Utime_(string dateTime)
    {
      return String2Utime(dateTime);
    }


    public Byte TmcEnumObjects(Int32        cid,
                               UInt16       objectType,
                               Byte         count,
                               ref UInt16[] buf,
                               Int16        ch,
                               Int16        rtu,
                               Int16        point)
    {
      return tmcEnumObjects(cid, objectType, count, buf, ch, rtu, point);
    }


    public Int16 TmcGetObjectName(Int32             cid,
                                  UInt16            objectType,
                                  Int16             ch,
                                  Int16             rtu,
                                  Int16             point,
                                  ref StringBuilder buf,
                                  Int32             bufSize)
    {
      return tmcGetObjectName(cid, objectType, ch, rtu, point, buf, bufSize);
    }


    public IntPtr TmcTechObjReadValues(Int32                   cid,
                                       TmNativeDefs.TTechObj[] objects,
                                       UInt32                  count)
    {
      return tmcTechObjReadValues(cid, objects, count);
    }


    public Boolean TmcTechObjBeginUpdate(Int32 cid)
    {
      return tmcTechObjBeginUpdate(cid);
    }


    public Boolean TmcTechObjEndUpdate(Int32 cid)
    {
      return tmcTechObjEndUpdate(cid);
    }


    public Int32 TmcTechObjWriteValues(Int32                        cid,
                                       TmNativeDefs.TTechObjProps[] props,
                                       UInt32                       count)
    {
      return tmcTechObjWriteValues(cid, props, count);
    }


    public UInt32 TmcEventGetAdditionalRecData(UInt32     id,
                                               ref byte[] buf,
                                               UInt32     bufSize)
    {
      return tmcEventGetAdditionalRecData(id, buf, bufSize);
    }
  }
}