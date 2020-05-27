using System;
using System.Text;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public partial interface ITmNative
  {
    Int32 TmcConnect(string           server,
                     string           pipe,
                     string           user,
                     TmNativeCallback callback,
                     IntPtr           callbackParameter);


    void TmcDisconnect(Int32 cid);


    void TmcFreeMemory(IntPtr memory);


    UInt32 TmcGetLastError();


    Int32 TmcGetLastErrorText(Int32  cid,
                              IntPtr buf);


    UInt32 TmcSetDgrmFlags(Int32  cid,
                           UInt32 flags);


    UInt32 TmcClrDgrmFlags(Int32  cid,
                           UInt32 flags);


    Int16 TmcSetFeedbackItems(Int32  cid,
                              UInt16 type,
                              Int16  ch,
                              Int16  rtu,
                              Int16  point,
                              Byte   count,
                              Byte   fbType,
                              UInt32 id);


    Int16 TmcClrFeedback(Int32 cid);


    UInt32 TmcGetCfsHandle(Int32 cid);


    UInt32 TmcReconnectCount(Int32 cid);


    UInt32 TmcConnState(Int32 cid);


    Int16 TmcSystemTime(Int32             cid,
                        ref StringBuilder time,
                        IntPtr            tmStruct);


    Boolean TmcGetUserInfo(Int32                      cid,
                           UInt32                     usid,
                           ref TmNativeDefs.TUserInfo userInfo);


    Int16 TmcStatus(Int32 cid,
                    Int16 ch,
                    Int16 rtu,
                    Int16 point);


    Single TmcAnalog(Int32  cid,
                     Int16  ch,
                     Int16  rtu,
                     Int16  point,
                     string dateTime,
                     Int16  retroNum);


    IntPtr TmcTmValuesByListEx(Int32                 cid,
                               UInt16                tmType,
                               Byte                  qFlags,
                               UInt32                count,
                               TmNativeDefs.TAdrTm[] addr);


    IntPtr TmcGetValuesByFlagMask(int      cid,
                                  ushort   tmType,
                                  uint     tmFlags,
                                  byte     qFlags,
                                  out uint pCount);


    Int16 TmcRegEvent(Int32               cid,
                      TmNativeDefs.TEvent tmEvent);


    Boolean TmcEvlogPutStrBin(Int32  cid,
                              UInt32 unixTime,
                              Byte   unixHund,
                              Byte   importance,
                              UInt32 sourceTag,
                              string str,
                              byte[] bin,
                              UInt32 cbBin);


    IntPtr TmcEventLogByElix(Int32                     cid,
                             ref TmNativeDefs.TTMSElix elix,
                             UInt16                    eventMask,
                             UInt32                    startUnixTime,
                             UInt32                    endUnixTime);


    Boolean TmcGetCurrentElix(Int32                     cid,
                              ref TmNativeDefs.TTMSElix elix);


    Boolean TmcAlertListRemove(Int32                       cid,
                               TmNativeDefs.TAlertListId[] listIds);


    Int16 TmcSetTimedValues(Int32                              cid,
                            UInt32                             count,
                            TmNativeDefs.TTimedValueAndFlags[] values);


    Int32 TmcExecuteControlScript(Int32 cid,
                                  Int16 ch,
                                  Int16 rtu,
                                  Int16 point,
                                  Int16 cmd);


    Boolean TmcOverrideControlScript(Int32 cid,
                                     bool  fOverride);


    Int16 TmcControlByStatus(Int32 cid,
                             Int16 ch,
                             Int16 rtu,
                             Int16 point,
                             Int16 cmd);


    Int16 TmcControlCmdResult(Int32  cid,
                              UInt32 id,
                              bool   fSuccess);


    Int16 TmcRegulationByAnalog(Int32  cid,
                                Int16  ch,
                                Int16  rtu,
                                Int16  point,
                                Byte   regType,
                                IntPtr regData);


    Int16 TmcSetStatusNormal(Int32  cid,
                             Int16  ch,
                             Int16  rtu,
                             Int16  point,
                             UInt16 nValue);


    Int16 TmcGetStatusNormal(Int32      cid,
                             Int16      ch,
                             Int16      rtu,
                             Int16      point,
                             out UInt16 nValue);


    Int16 TmcDriverCall(Int32  cid,
                        UInt32 addr,
                        Int16  qCode,
                        Int16  command);


    Boolean TmcEventLogAckRecords(Int32                     cid,
                                  ref TmNativeDefs.TTMSElix elix,
                                  UInt32                    count);


    void TmcStatusByList(Int32                       cid,
                         UInt16                      count,
                         TmNativeDefs.TAdrTm[]       addr,
                         TmNativeDefs.TStatusPoint[] statuses);


    void TmcAnalogByList(Int32                       cid,
                         UInt16                      count,
                         TmNativeDefs.TAdrTm[]       addr,
                         TmNativeDefs.TAnalogPoint[] analogs);


    Int16 TmcGetObjectProperties(Int32             cid,
                                 UInt16            objectType,
                                 Int16             ch,
                                 Int16             rtu,
                                 Int16             point,
                                 ref StringBuilder buf,
                                 Int32             bufSize);


    IntPtr TmcGetStatusClassData(Int32                 cid,
                                 UInt32                count,
                                 TmNativeDefs.TAdrTm[] statuses);


    IntPtr TmcGetAnalogClassData(Int32                 cid,
                                 UInt32                count,
                                 TmNativeDefs.TAdrTm[] analogs);


    void TmcTakeRetroTit(Int32                                cid,
                         Int16                                ch,
                         Int16                                rtu,
                         Int16                                point,
                         UInt32                               unixTime,
                         UInt16                               retroNum,
                         UInt16                               count,
                         UInt16                               step,
                         ref TmNativeDefs.TAnalogPointShort[] analogs);


    Int16 TmcSetStatus(Int32  cid,
                       Int16  ch,
                       Int16  rtu,
                       Int16  point,
                       Byte   value,
                       string dateTime,
                       Int16  hund);


    Int16 TmcSetStatusFlags(Int32 cid,
                            Int16 ch,
                            Int16 rtu,
                            Int16 point,
                            Int16 flags);


    Int16 TmcClrStatusFlags(Int32 cid,
                            Int16 ch,
                            Int16 rtu,
                            Int16 point,
                            Int16 flags);


    Int16 TmcSetAnalog(Int32  cid,
                       Int16  ch,
                       Int16  rtu,
                       Int16  point,
                       Single value,
                       string dateTime);


    Int16 TmcSetAnalogFlags(Int32 cid,
                            Int16 ch,
                            Int16 rtu,
                            Int16 point,
                            Int16 flags);


    Int16 TmcClrAnalogFlags(Int32 cid,
                            Int16 ch,
                            Int16 rtu,
                            Int16 point,
                            Int16 flags);


    Int16 TmcPeekAlarm(Int32                   cid,
                       Int16                   ch,
                       Int16                   rtu,
                       Int16                   point,
                       Int16                   alarmId,
                       ref TmNativeDefs.TAlarm alarm);


    Int16 TmcPokeAlarm(Int32                   cid,
                       Int16                   ch,
                       Int16                   rtu,
                       Int16                   point,
                       Int16                   alarmId,
                       ref TmNativeDefs.TAlarm alarm);


    IntPtr TmcAanReadArchive(Int32            cid,
                             UInt32           tmAddr,
                             UInt32           startUnixTime,
                             UInt32           endUnixTime,
                             UInt32           step,
                             UInt32           flags,
                             out UInt32       count,
                             TmNativeCallback progress,
                             IntPtr           progressParam);


    UInt32 String2Utime_(string dateTime);


    Byte TmcEnumObjects(Int32        cid,
                        UInt16       objectType,
                        Byte         count,
                        ref UInt16[] buf,
                        Int16        ch,
                        Int16        rtu,
                        Int16        point);


    Int16 TmcGetObjectName(Int32             cid,
                           UInt16            objectType,
                           Int16             ch,
                           Int16             rtu,
                           Int16             point,
                           ref StringBuilder buf,
                           Int32             bufSize);


    Int16 TmcGetObjectNameEx(Int32             cid,
                             UInt16            objectType,
                             Int16             ch,
                             Int16             rtu,
                             Int16             point,
                             Int16             subObjectId,
                             ref StringBuilder buf,
                             Int32             bufSize);


    IntPtr TmcTechObjReadValues(Int32                   cid,
                                TmNativeDefs.TTechObj[] objects,
                                UInt32                  count);


    IntPtr TmcTechObjEnumValues(Int32      cid,
                                UInt32     tobS,
                                UInt32     tobT,
                                IntPtr     props,
                                out UInt32 count);


    Boolean TmcTechObjBeginUpdate(Int32 cid);

    Boolean TmcTechObjEndUpdate(Int32 cid);


    Int32 TmcTechObjWriteValues(Int32                        cid,
                                TmNativeDefs.TTechObjProps[] props,
                                UInt32                       count);


    UInt32 TmcEventGetAdditionalRecData(UInt32     id,
                                        ref byte[] buf,
                                        UInt32     bufSize);


    IntPtr TmcComtradeEnumDays(Int32 cid);


    IntPtr TmcComtradeEnumFiles(Int32 cid, string date);


    Boolean TmcComtradeGetFile(Int32 cid, string fileName, string localDirectory);


    Boolean TmcSetTracer(Int32  cid,
                         Int16  ch,
                         Int16  rtu,
                         Int16  point,
                         UInt16 tmType,
                         UInt16 msgFilter);

    Int32 TmcConnectEx(string           server,
                       string           pipe,
                       string           user,
                       TmNativeCallback callback,
                       IntPtr           callbackParameter,
                       UInt32           propsCount,
                       UInt32[]         pProps,
                       UInt32[]         pPropValues);


    Boolean TmcDntGetConfig(Int32  cid,
                            string fileName);

    Int32 TmcDntTreeChange(Int32 cid);

    IntPtr TmcDntOpenItem(Int32    cid,
                          UInt32   traceChainLength,
                          UInt32[] traceChain);


    IntPtr TmcDntGetNextItem(IntPtr componentItemsPtr);


    void TmcDntCloseItem(IntPtr componentItemsPtr);

    Boolean TmcDntGetObjectName(Int32             cid,
                                UInt16            objectType,
                                Int16             ch,
                                Int16             rtu,
                                Int16             point,
                                ref StringBuilder buf,
                                Int32             bufSize);
  }
}