using System;
using System.Text;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public interface ITmNative
  {
    #region Platform

    string GetOikTaskExecutable(string origin);


    bool PlatformSetEvent(UInt32 hEvent);


    UInt32 PlatformWaitForSingleObject(UInt32 hHandle,
                                       UInt32 dwMilliseconds);

    #endregion

    #region Cfs

    bool CfsInitLibrary(string baseDir = null,
                        string extArg  = null);


    void CfsSetUser(string user,
                    string password);


    UInt32 CfsGetExtendedUserData(UInt32 cfCid,
                                  string serverType,
                                  string serverName,
                                  IntPtr buf,
                                  UInt32 bufSize);


    bool CfsPmonLocalRegisterProcess(Int32      argc,
                                     string[]   argv,
                                     ref UInt32 phStartEvt,
                                     ref UInt32 phStopEvt);


    bool StracAllocServer(ref TmNativeDefs.TraceItemStorage tis,
                          UInt32                            pid,
                          UInt32                            ppid,
                          string                            name,
                          string                            comment);


    void StracSetServerState(ref TmNativeDefs.TraceItemStorage tis,
                             UInt32                            state);


    bool CfsGetComputerInfo(UInt32                         cfCid,
                            ref TmNativeDefs.ComputerInfoS cis,
                            out UInt32                     errCode,
                            ref StringBuilder              errString,
                            UInt32                         maxErrs);


    bool CfsDirEnum(UInt32            cfCid,
                    string            path,
                    ref char[]        buf,
                    UInt32            bufLength,
                    out UInt32        errCode,
                    ref StringBuilder errString,
                    UInt32            maxErrs);


    bool CfsFileGet(UInt32            cfCid,
                    string            remotePath,
                    string            localPath,
                    UInt32            timeout,
                    IntPtr            fileTime,
                    out UInt32        errCode,
                    ref StringBuilder errString,
                    UInt32            maxErrs);


    Int64 UxGmTime2UxTime(Int64 time);


    void DPrintF(string message);
    void MPrintF(string message);
    void EPrintF(string message);

    #endregion

    #region Tmc

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


    bool TmcEvlogPutStrBin(Int32  cid,
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


    bool TmcGetCurrentElix(Int32                     cid,
                           ref TmNativeDefs.TTMSElix elix);


    Int16 TmcSetTimedValues(Int32                              cid,
                            UInt32                             count,
                            TmNativeDefs.TTimedValueAndFlags[] values);


    Int32 TmcExecuteControlScript(Int32 cid,
                                  Int16 ch,
                                  Int16 rtu,
                                  Int16 point,
                                  Int16 cmd);


    bool TmcOverrideControlScript(Int32 cid,
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


    bool TmcEventLogAckRecords(Int32                     cid,
                               ref TmNativeDefs.TTMSElix elix,
                               UInt32                    count);


    void TmcStatusByList(Int32                       cid,
                         UInt16                      count,
                         TmNativeDefs.TAdrTm[]       addr,
                         TmNativeDefs.TStatusPoint[] statuses);


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


    IntPtr TmcTechObjReadValues(Int32                   cid,
                                TmNativeDefs.TTechObj[] objects,
                                UInt32                  count);


    bool TmcTechObjBeginUpdate(Int32 cid);

    bool TmcTechObjEndUpdate(Int32 cid);


    Int32 TmcTechObjWriteValues(Int32                        cid,
                                TmNativeDefs.TTechObjProps[] props,
                                UInt32                       count);

    #endregion

    #region Rbc

    UInt16 RbcIpgStartRedirector(Int32  cid,
                                 UInt16 portIdx);


    bool RbcIpgStopRedirector(Int32  cid,
                              UInt16 portIdx);

    #endregion
  }
}