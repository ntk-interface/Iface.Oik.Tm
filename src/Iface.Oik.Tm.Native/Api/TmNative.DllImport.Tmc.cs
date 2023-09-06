using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Int32 tmcConnect(string                                                  server,
                                          string                                                  pipe,
                                          string                                                  user,
                                          [MarshalAs(UnmanagedType.FunctionPtr)] TmNativeCallback callback,
                                          IntPtr                                                  callbackParameter);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern void tmcDisconnect(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 tmcIsConnected(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern void tmcUpdateConnection(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcGetCurrentServer(Int32            cid,
                                                     [In, Out] byte[] machineBuf,
                                                     UInt32           cbMachine,
                                                     [In, Out] byte[] pipeBuf,
                                                     UInt32           cbPipe);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 tmcSetDgrmFlags(Int32  cid,
                                                UInt32 flags);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 tmcClrDgrmFlags(Int32  cid,
                                                UInt32 flags);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetFeedbackItems(Int32  cid,
                                                   UInt16 type,
                                                   Int16  ch,
                                                   Int16  rtu,
                                                   Int16  point,
                                                   Byte   count,
                                                   Byte   fbType,
                                                   UInt32 id);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcClrFeedback(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcGetCfsHandle(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern void tmcFreeMemory(IntPtr memory);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 tmcGetLastError();


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 tmcGetLastErrorText(Int32  cid,
                                                   IntPtr buf);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcDecodeTcError(UInt16 status);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcGetKnownxCfgPath(Int32                                   cid,
                                                    [MarshalAs(UnmanagedType.LPStr)] string appTag,
                                                    UInt32                                  index);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 tmcReconnectCount(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 tmcConnState(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 tmcGetServerCaps(Int32 cid,
                                                 [MarshalAs(UnmanagedType.LPArray)] [In, Out]
                                                 byte[] pCaps);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 tmcGetServerFeature(Int32  cid,
                                                   UInt32 dwCode);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSystemTime(Int32            cid,
                                             [In, Out] byte[] time,
                                             [In]      IntPtr tmStruct); // TODO test ref


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcGetUserInfo(Int32                                cid,
                                                UInt32                               usid,
                                                [In, Out] ref TmNativeDefs.TUserInfo userInfo);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean tmcGetUserInfoEx(Int32                                cid,
                                                  UInt32                               userId,
                                                  [In, Out] ref TmNativeDefs.TUserInfo userInfo,
                                                  [In, Out]     byte[]                 appxBuf,
                                                  uint                                 cbAppx);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcGetUserList(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcStatus(Int32 cid,
                                         Int16 ch,
                                         Int16 rtu,
                                         Int16 point);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Single tmcAnalog(Int32                                   cid,
                                          Int16                                   ch,
                                          Int16                                   rtu,
                                          Int16                                   point,
                                          [MarshalAs(UnmanagedType.LPStr)] string dateTime,
                                          Int16                                   retroNum);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcStatusFull(Int32                                   cid,
                                             Int16                                   ch,
                                             Int16                                   rtu,
                                             Int16                                   point,
                                             [In, Out] ref TmNativeDefs.TStatusPoint statusPoint);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcStatusFullEx(Int32                                   cid,
                                               Int16                                   ch,
                                               Int16                                   rtu,
                                               Int16                                   point,
                                               [In, Out] ref TmNativeDefs.TStatusPoint statusPoint,
                                               UInt32                                  time);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcAnalogFull(Int32                                                      cid,
                                             Int16                                                      ch,
                                             Int16                                                      rtu,
                                             Int16                                                      point,
                                             [In, Out] ref                    TmNativeDefs.TAnalogPoint analogPoint,
                                             [MarshalAs(UnmanagedType.LPStr)] string                    dateTime,
                                             Int16                                                      retroNum);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern void tmcStatusByList(Int32                                 cid,
                                              UInt16                                count,
                                              [In]      TmNativeDefs.TAdrTm[]       addr,
                                              [In, Out] TmNativeDefs.TStatusPoint[] status);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern void tmcAnalogByList(Int32                                 cid,
                                              UInt16                                count,
                                              [In]      TmNativeDefs.TAdrTm[]       addr,
                                              [In, Out] TmNativeDefs.TAnalogPoint[] analog,
                                              UInt32                                time,
                                              UInt16                                retroNum);

    
    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern void tmcAccumByList(Int32                                cid,
                                             UInt16                               count,
                                             [In]      TmNativeDefs.TAdrTm[]      addr,
                                             [In, Out] TmNativeDefs.TAccumPoint[] accum,
                                             UInt32                               time);
    
    
    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcTMValuesByListEx(Int32                      cid,
                                                    UInt16                     tmType,
                                                    Byte                       qFlags,
                                                    UInt32                     count,
                                                    [In] TmNativeDefs.TAdrTm[] addr);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcGetValuesByFlagMask(Int32      cid,
                                                       UInt16     tmType,
                                                       UInt32     tmFlags,
                                                       Byte       qFlags,
                                                       out UInt32 pCount);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcGetValuesEx(Int32                                   cid,
                                               UInt16                                  tmType,
                                               UInt32                                  tmFlagsSet,
                                               UInt32                                  tmFlagsClr,
                                               Byte                                    qFlags,
                                               [MarshalAs(UnmanagedType.LPStr)] string groupName,
                                               UInt32                                  dwUt,
                                               out UInt32                              pCount);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcRetroGetNamedAnalogGrpFull(Int32                                   cid,
                                                              [MarshalAs(UnmanagedType.LPStr)] string groupName,
                                                              UInt32                                  qryFlags,
                                                              UInt32                                  dwUt,
                                                              UInt32                                  dwStepBack,
                                                              UInt32                                  dwStepCnt,
                                                              IntPtr                                  pAddrs,
                                                              out UInt32                              pAddrCount);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcGetCurrentElix(Int32                               cid,
                                                   [In, Out] ref TmNativeDefs.TTMSElix elix);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcRegEvent(Int32                        cid,
                                           [In] ref TmNativeDefs.TEvent tmEvent);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcEvlogPutStrBin(Int32                                        cid,
                                                   UInt32                                       unixTime,
                                                   Byte                                         unixHund,
                                                   Byte                                         importance,
                                                   UInt32                                       sourceTag,
                                                   [In] [MarshalAs(UnmanagedType.LPStr)] string str,
                                                   [In] [MarshalAs(UnmanagedType.LPArray)]
                                                   Byte[] bin,
                                                   UInt32 cbBin);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcEventLogEx(Int32                              cid, 
                                              UInt16                             eventMask, 
                                              UInt32                             startUnixTime,
                                              UInt32                             endUnixTime,
                                              [In] TmNativeDefs.TEventExCriteria criteria);
    

    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcEventLogByElix(Int32                               cid,
                                                  [In, Out] ref TmNativeDefs.TTMSElix elix,
                                                  UInt16                              eventMask,
                                                  UInt32                              startUnixTime,
                                                  UInt32                              endUnixTime);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcAlertListRemove(Int32                                 cid,
                                                    [In, Out] TmNativeDefs.TAlertListId[] listIds);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetValues(Int32                              cid,
                                            UInt32                             count,
                                            [In] TmNativeDefs.TValueAndFlags[] values);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetValues(Int32                                   cid,
                                            UInt32                                  count,
                                            [In] TmNativeDefs.TValueAndFlagsUnion[] values);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetTimedValues(Int32                                   cid,
                                                 UInt32                                  count,
                                                 [In] TmNativeDefs.TTimedValueAndFlags[] values);

    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetTimedValues(Int32                                        cid,
                                                 UInt32                                       count,
                                                 [In] TmNativeDefs.TTimedValueAndFlagsUnion[] values);
    

    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 tmcExecuteControlScript(Int32 cid,
                                                       Int16 ch,
                                                       Int16 rtu,
                                                       Int16 point,
                                                       Int16 cmd);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcOverrideControlScript(Int32 cid,
                                                          bool  fOverride);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcControlByStatus(Int32 cid,
                                                  Int16 ch,
                                                  Int16 rtu,
                                                  Int16 point,
                                                  Int16 cmd);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcControlCmdResult(Int32  cid,
                                                   UInt32 id,
                                                   bool   fSuccess);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcRegulationByAnalog(Int32  cid,
                                                     Int16  ch,
                                                     Int16  rtu,
                                                     Int16  point,
                                                     Byte   regType,
                                                     IntPtr regData);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetStatusNormal(Int32  cid,
                                                  Int16  ch,
                                                  Int16  rtu,
                                                  Int16  point,
                                                  UInt16 nValue);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcGetStatusNormal(Int32      cid,
                                                  Int16      ch,
                                                  Int16      rtu,
                                                  Int16      point,
                                                  out UInt16 nValue);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcDriverCall(Int32  cid,
                                             UInt32 addr,
                                             Int16  qCode,
                                             Int16  command);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcEventLogAckRecords(Int32                          cid,
                                                       [In] ref TmNativeDefs.TTMSElix elix,
                                                       UInt32                         count);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcGetObjectProperties(Int32            cid,
                                                      UInt16           objectType,
                                                      Int16            ch,
                                                      Int16            rtu,
                                                      Int16            point,
                                                      [In, Out] byte[] buf,
                                                      Int32            bufSize);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetObjectProperties(Int32            cid,
                                                      UInt16           objectType,
                                                      Int16            ch,
                                                      Int16            rtu,
                                                      Int16            point,
                                                      [In, Out] byte[] propList,
                                                      out       UInt32 pMask);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcGetStatusClassData(Int32                      cid,
                                                      UInt32                     count, // max 128
                                                      [In] TmNativeDefs.TAdrTm[] statuses);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcGetAnalogClassData(Int32                      cid,
                                                      UInt32                     count, // max 128
                                                      [In] TmNativeDefs.TAdrTm[] analogs);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern void tmcTakeRetroTit(Int32                                      cid,
                                              Int16                                      ch,
                                              Int16                                      rtu,
                                              Int16                                      point,
                                              UInt32                                     uTime,
                                              UInt16                                     retroNum,
                                              UInt16                                     count,
                                              UInt16                                     step,
                                              [In, Out] TmNativeDefs.TAnalogPointShort[] analogs);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcAnalogMicroSeries(Int32                           cid,
                                                    UInt32                          cnt,
                                                    [In]      TmNativeDefs.TAdrTm[] addrList,
                                                    [In, Out] IntPtr[]              resultList); // TMSAnalogMSeries**


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcGetAnalogTechParms(Int32                                       cid,
                                                       [In] ref      TmNativeDefs.TAdrTm           addr,
                                                       [In, Out] ref TmNativeDefs.TAnalogTechParms tpr);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcSetAnalogTechParms(Int32                                  cid,
                                                       [In] ref TmNativeDefs.TAdrTm           addr,
                                                       [In] ref TmNativeDefs.TAnalogTechParms tpr);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcEvaluateExpression(Int32                                   cid,
                                                     byte[] expr,
                                                     [In, Out]                        byte[] res,
                                                     UInt32                                  cbBytes);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetStatus(Int32                                   cid,
                                            Int16                                   ch,
                                            Int16                                   rtu,
                                            Int16                                   point,
                                            Byte                                    value,
                                            [MarshalAs(UnmanagedType.LPStr)] string dateTime,
                                            Int16                                   hund);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetStatusFlags(Int32 cid,
                                                 Int16 ch,
                                                 Int16 rtu,
                                                 Int16 point,
                                                 Int16 flags);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcClrStatusFlags(Int32 cid,
                                                 Int16 ch,
                                                 Int16 rtu,
                                                 Int16 point,
                                                 Int16 flags);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetAnalog(Int32                                   cid,
                                            Int16                                   ch,
                                            Int16                                   rtu,
                                            Int16                                   point,
                                            Single                                  value,
                                            [MarshalAs(UnmanagedType.LPStr)] string dateTime);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetAnalogFlags(Int32 cid,
                                                 Int16 ch,
                                                 Int16 rtu,
                                                 Int16 point,
                                                 Int16 flags);

    
    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetAnalogByCode(Int32 cid,
                                                 Int16 ch,
                                                 Int16 rtu,
                                                 Int16 point,
                                                 Int16 code);

    

    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcClrAnalogFlags(Int32 cid,
                                                 Int16 ch,
                                                 Int16 rtu,
                                                 Int16 point,
                                                 Int16 flags);

    
    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetAccumValue(Int32                                   cid,
                                                Int16                                   ch,
												Int16                                   rtu,
												Int16                                   point,
												Single                                  value,
												[MarshalAs(UnmanagedType.LPStr)] string dateTime);

    

    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetAccumFlags(Int32 cid,
                                                Int16 ch,
                                                Int16 rtu,
                                                Int16 point,
                                                Int16 flags);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcClrAccumFlags(Int32 cid,
                                                Int16 ch,
                                                Int16 rtu,
                                                Int16 point,
                                                Int16 flags);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcPeekAlarm(Int32                   cid,
                                            Int16                   ch,
                                            Int16                   rtu,
                                            Int16                   point,
                                            Int16                   alarmId,
                                            ref TmNativeDefs.TAlarm alarm);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcPokeAlarm(Int32                   cid,
                                            Int16                   ch,
                                            Int16                   rtu,
                                            Int16                   point,
                                            Int16                   alarmId,
                                            ref TmNativeDefs.TAlarm alarm);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcAanReadArchive(Int32 cid,
                                                  UInt32 tmAddr,
                                                  UInt32 startUnixTime,
                                                  UInt32 endUnixTime,
                                                  UInt32 step,
                                                  UInt32 flags,
                                                  out                                    UInt32 count,
                                                  [MarshalAs(UnmanagedType.FunctionPtr)] TmNativeCallback progress,
                                                  IntPtr progressParam);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcAanGetStats(Int32                         cid,
                                                ref TmNativeDefs.TM_AAN_STATS stats,
                                                UInt32                        cbStats);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 String2Utime([MarshalAs(UnmanagedType.LPStr)] string dateTime);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Byte tmcEnumObjects(Int32              cid,
                                             UInt16             objectType,
                                             Byte               count,
                                             [In, Out] UInt16[] buf,
                                             Int16              ch,
                                             Int16              rtu,
                                             Int16              point);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Byte tmcGetObjectName(Int32            cid,
                                               UInt16           objectType,
                                               Int16            ch,
                                               Int16            rtu,
                                               Int16            point,
                                               [In, Out] byte[] buf,
                                               Int32            bufSize);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Byte tmcGetObjectNameEx(Int32            cid,
                                                 UInt16           objectType,
                                                 Int16            ch,
                                                 Int16            rtu,
                                                 Int16            point,
                                                 Int16            subObjectId,
                                                 [In, Out] byte[] buf,
                                                 Int32            bufSize);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcTechObjReadValues(Int32                        cid,
                                                     [In] TmNativeDefs.TTechObj[] objects,
                                                     UInt32                       count);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcTechObjEnumValues(Int32      cid,
                                                     UInt32     tobS,
                                                     UInt32     tobT,
                                                     IntPtr     props,
                                                     out UInt32 count);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcTechObjBeginUpdate(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcTechObjEndUpdate(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 tmcTechObjWriteValues(Int32                             cid,
                                                     [In] TmNativeDefs.TTechObjProps[] props,
                                                     UInt32                            count);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 tmcEventGetAdditionalRecData(UInt32 idx,
                                                             [MarshalAs(UnmanagedType.LPArray)] [In, Out]
                                                             byte[] buf,
                                                             UInt32
                                                               bufSize);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcComtradeEnumDays(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcComtradeEnumFiles(Int32                                   cid,
                                                     [MarshalAs(UnmanagedType.LPStr)] string date);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcComtradeGetFile(Int32                                   cid,
                                                    [MarshalAs(UnmanagedType.LPStr)] string fName,
                                                    [MarshalAs(UnmanagedType.LPStr)] string locDir);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcSetTracer(Int32  cid,
                                              Int16  ch,
                                              Int16  rtu,
                                              Int16  point,
                                              UInt16 tmType,
                                              UInt16 msgF);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Int32 tmcConnectEx(string                                                  server,
                                            string                                                  pipe,
                                            string                                                  user,
                                            [MarshalAs(UnmanagedType.FunctionPtr)] TmNativeCallback callback,
                                            IntPtr                                                  callbackParameter,
                                            UInt32                                                  cbProps,
                                            [MarshalAs(UnmanagedType.LPArray, SizeConst = 8)]
                                            UInt32[] pProps,
                                            [MarshalAs(UnmanagedType.LPArray, SizeConst = 8)]
                                            UInt32[] pPropValues);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean tmcDntGetConfig(Int32                                   cid,
                                                 [MarshalAs(UnmanagedType.LPStr)] string fName);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 tmcDntTreeChange(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcDntOpenItem(Int32  cid,
                                               UInt32 count,
                                               [MarshalAs(UnmanagedType.LPArray, SizeConst = 8)]
                                               UInt32[] pMask);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcDntGetNextItem(IntPtr itId);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern void tmcDntCloseItem(IntPtr itId);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean tmcDntGetObjectName(Int32            cid,
                                                     UInt16           objectType,
                                                     Int16            ch,
                                                     Int16            rtu,
                                                     Int16            point,
                                                     [In, Out] byte[] buf,
                                                     Int32            bufSize);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcDntRegisterUser(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern bool tmcDntUnRegisterUser(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcDntBeginTraceEx(Int32  cid,
                                                    UInt32 count,
                                                    [MarshalAs(UnmanagedType.LPArray, SizeConst = 8)]
                                                    UInt32[] pMask,
                                                    UInt32 traceFlags,
                                                    UInt32 res1,
                                                    UInt32 res2);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcDntStopTrace(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcDntBeginDebug(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean tmcDntStopDebug(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 tmcDntGetLiveInfo(Int32  cid,
                                                  UInt32 count,
                                                  [MarshalAs(UnmanagedType.LPArray, SizeConst = 8)]
                                                  UInt32[] pMask,
                                                  out UInt32 pData,
                                                  UInt32     cdData);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 tmcDntGetPortStats(Int32 cid,
                                                   [MarshalAs(UnmanagedType.LPArray, SizeConst = 8)]
                                                   UInt32[] pDap,
                                                   [In, Out] byte[] buf,
                                                   Int32            bufSize);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Int16 tmcGetServerInfo(Int32                        cid,
                                                ref TmNativeDefs.TServerInfo info);

    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr tmcGetServerThreads(Int32 cid);

    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool tmcGetGrantedAccess(Int32      cid,
                                                  out UInt32 pAccess);

    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetRetransInfoEx(Int32                                  cid,
                                                   UInt16                                 count,
                                                   [In]  TmNativeDefs.TRetransInfo[]      ri,
                                                   [Out] TmNativeDefs.TRetransInfoReply[] rir);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetRetransInfo(Int32                            cid,
                                                 UInt16                           count,
                                                 [In] TmNativeDefs.TRetransInfo[] ri);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcClrRetransInfo(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcTakeAPS(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr tmcTextSearch(Int32                                   cid,
                                              UInt16                                  type,
                                              [MarshalAs(UnmanagedType.LPStr)] string text,
                                              out                              UInt32 pCount);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Int16 tmcRetroInfoEx(Int32                         cid,
                                              UInt16                        id,
                                              ref TmNativeDefs.TRetroInfoEx info);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean tmcPubPublish(Int32                                   cid,
                                               [MarshalAs(UnmanagedType.LPStr)] string topic,
                                               UInt32                                  lifeTimeSec,
                                               Byte                                    qos,
                                               Byte[]                                  data,
                                               UInt32                                  cbData);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean tmcPubSubscribe(Int32                                   cid,
                                                 [MarshalAs(UnmanagedType.LPStr)] string topic,
                                                 UInt32                                  subscriptionId,
                                                 Byte                                    qos);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean tmcPubUnsubscribe(Int32                                   cid,
                                                   [MarshalAs(UnmanagedType.LPStr)] string topic,
                                                   UInt32                                  subscriptionId);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean tmcPubAck(Int32                                   cid,
                                           [MarshalAs(UnmanagedType.LPStr)] string topic,
                                           UInt32                                  subscriptionId,
                                           Byte                                    qos,
                                           UInt32                                  userId,
                                           Byte[]                                  ackData,
                                           UInt32                                  cbAckData);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean tmcPubParseDatagram([MarshalAs(UnmanagedType.LPArray)] [In] Byte[] diagram, //in
                                                     UInt32 diagramSize, //in
                                                     [In, Out] IntPtr pTag, //out
                                                     [In, Out] IntPtr ppdata, //out
                                                     out       UInt32 dataSize, //out
                                                     out       UInt32 subId, //out
                                                     out       Byte qos, //out
                                                     out       Boolean retained, //out, can be NULL
                                                     out       Byte pubFlg //out, can be NULL
    );

    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean tmcGetConnectErrorText(Int32            cid,
                                                        [In, Out] byte[] buf,
                                                        UInt32            bufSize);

	[DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	public static extern Boolean tmcBackupServerProcedure(
	                                                byte[] machine,
	                                                byte[] pipe,
	                                                byte[] directory,
	                                                ref UInt32 pbflags,
	                                                Int32 hCancel,
	                                                [MarshalAs(UnmanagedType.FunctionPtr)] TmNativeCallback prog_fn,
	                                                IntPtr prog_parm);

	[DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	public static extern Boolean tmcRestoreServer(Boolean tms_not_rbs,
												byte[] machine,
												byte[] pipe,
												byte[] filename,
												ref UInt32 pbflags,
												Int32 hCancel,
												[MarshalAs(UnmanagedType.FunctionPtr)] TmNativeCallback prog_fn,
												IntPtr prog_parm);
	}
}