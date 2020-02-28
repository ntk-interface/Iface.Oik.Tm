using System;
using System.Runtime.InteropServices;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    private const string Cfshare = "libif_cfs.dll";
    private const string Tmconn  = "libif_cfs.dll";

    #region Platform

    [DllImport(Cfshare, EntryPoint = "Ipos_SetEvent", CallingConvention = CallingConvention.StdCall)]
    public static extern bool SetEventLinux(UInt32 hEvent);


    [DllImport(Cfshare, EntryPoint = "Ipos_WaitForSingleObject", CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 WaitForSingleObjectLinux(UInt32 hHandle,
                                                         UInt32 dwMilliseconds);


    [DllImport("kernel32.dll", EntryPoint = "SetEvent", CallingConvention = CallingConvention.StdCall)]
    public static extern bool SetEventWindows(UInt32 hEvent);


    [DllImport("kernel32.dll", EntryPoint = "WaitForSingleObject", CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 WaitForSingleObjectWindows(UInt32 hHandle,
                                                           UInt32 dwMilliseconds);

    #endregion

    #region Cfs

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern bool cfsInitLibrary([MarshalAs(UnmanagedType.LPStr)] string baseDir,
                                             [MarshalAs(UnmanagedType.LPStr)] string extArg);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void cfsSetUser([MarshalAs(UnmanagedType.LPStr)] string name,
                                         [MarshalAs(UnmanagedType.LPStr)] string pwd);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 cfsGetExtendedUserData(UInt32                                  cfCid,
                                                       [MarshalAs(UnmanagedType.LPStr)] string serverType,
                                                       [MarshalAs(UnmanagedType.LPStr)] string serverName,
                                                       IntPtr                                  buf,
                                                       UInt32                                  bufSize);
    
    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 cfsGetExtendedUserData(IntPtr                                  cfCid,
                                                       [MarshalAs(UnmanagedType.LPStr)] string serverType,
                                                       [MarshalAs(UnmanagedType.LPStr)] string serverName,
                                                       IntPtr                                  buf,
                                                       UInt32                                  bufSize);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsPmonLocalRegisterProcess(Int32              argc,
                                                          [In, Out] string[] argv,
                                                          ref       UInt32   phStartEvt,
                                                          ref       UInt32   phStopEvt);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool strac_AllocServer(ref TmNativeDefs.TraceItemStorage       tis,
                                                UInt32                                  pid,
                                                UInt32                                  ppid,
                                                [MarshalAs(UnmanagedType.LPStr)] string name,
                                                [MarshalAs(UnmanagedType.LPStr)] string comment);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void strac_SetServerState(ref TmNativeDefs.TraceItemStorage tis,
                                                   UInt32                            state);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsGetComputerInfo(UInt32                                                      cfCid,
                                                 ref                              TmNativeDefs.ComputerInfoS cis,
                                                 out                              UInt32                     errCode,
                                                 [MarshalAs(UnmanagedType.LPStr)] StringBuilder              errString,
                                                 UInt32                                                      maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsDirEnum(UInt32                                  cfCid,
                                         [MarshalAs(UnmanagedType.LPStr)] string path,
                                         [MarshalAs(UnmanagedType.LPArray)] [In, Out]
                                         char[] buf,
                                         UInt32                                         bufLength,
                                         out                              UInt32        errCode,
                                         [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                         UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsFileGet(UInt32                                         cfCid,
                                         [MarshalAs(UnmanagedType.LPStr)] string        remotePath,
                                         [MarshalAs(UnmanagedType.LPStr)] string        localPath,
                                         UInt32                                         timeout,
                                         IntPtr                                         fileTime,
                                         out                              UInt32        errCode,
                                         [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                         UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsConnect([MarshalAs(UnmanagedType.LPStr)] string        serverName,
                                           out                              UInt32        errCode,
                                           [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                           UInt32                                         maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsConfFileOpenCid(IntPtr                                                 connId,
                                                   [MarshalAs(UnmanagedType.LPStr)] string                serverName,
                                                   [MarshalAs(UnmanagedType.LPStr)] string                fileName,
                                                   UInt32                                                 timeout,
                                                   [In, Out] ref                    TmNativeDefs.FileTime fileTime,
                                                   out                              UInt32                errCode,
                                                   [MarshalAs(UnmanagedType.LPStr)] StringBuilder         errString,
                                                   UInt32                                                 maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsConfFileSaveAs(IntPtr                                                 treeHandle,
                                                [MarshalAs(UnmanagedType.LPStr)] string                serverName,
                                                [MarshalAs(UnmanagedType.LPStr)] string                remoteFileName,
                                                UInt32                                                 timeout,
                                                [In, Out] ref                    TmNativeDefs.FileTime fileTime,
                                                out                              UInt32                errCode,
                                                [MarshalAs(UnmanagedType.LPStr)] StringBuilder         errString,
                                                UInt32                                                 maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 cfsGetSoftwareType(IntPtr connId);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 cfsIfpcMaster(IntPtr connId, Byte command);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern bool cfsIsConnected(IntPtr connId);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int64 uxgmtime2uxtime(Int64 time);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void e_printf([MarshalAs(UnmanagedType.LPStr)] string format);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void m_printf([MarshalAs(UnmanagedType.LPStr)] string format);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void d_printf([MarshalAs(UnmanagedType.LPStr)] string format);

    #endregion

    #region Tmc

    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Int32 tmcConnect(string                                                  server,
                                          string                                                  pipe,
                                          string                                                  user,
                                          [MarshalAs(UnmanagedType.FunctionPtr)] TmNativeCallback callback,
                                          IntPtr                                                  callbackParameter);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern void tmcDisconnect(Int32 cid);


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
    public static extern UInt32 tmcGetCfsHandle(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern void tmcFreeMemory(IntPtr memory);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 tmcGetLastError();


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 tmcGetLastErrorText(Int32  cid,
                                                   IntPtr buf);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 tmcReconnectCount(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 tmcConnState(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSystemTime(Int32                                          cid,
                                             [MarshalAs(UnmanagedType.LPStr)] StringBuilder time,
                                             [In]                             IntPtr        tmStruct); // TODO test ref


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
    public static extern void tmcStatusByList(Int32                                 cid,
                                              UInt16                                count,
                                              [In]      TmNativeDefs.TAdrTm[]       addr,
                                              [In, Out] TmNativeDefs.TStatusPoint[] status);
    
    
    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern void tmcAnalogByList(Int32                                 cid,
                                              UInt16                                count,
                                              [In]      TmNativeDefs.TAdrTm[]       addr,
                                              [In, Out] TmNativeDefs.TAnalogPoint[] analog);


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
    public static extern bool tmcGetCurrentElix(Int32                               cid,
                                                [In, Out] ref TmNativeDefs.TTMSElix elix);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcRegEvent(Int32                        cid,
                                           [In] ref TmNativeDefs.TEvent tmEvent);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern bool tmcEvlogPutStrBin(Int32                                        cid,
                                                UInt32                                       unixTime,
                                                Byte                                         unixHund,
                                                Byte                                         importance,
                                                UInt32                                       sourceTag,
                                                [In] [MarshalAs(UnmanagedType.LPStr)] string str,
                                                [In] [MarshalAs(UnmanagedType.LPArray)]
                                                Byte[] bin,
                                                UInt32 cbBin);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcEventLogByElix(Int32                               cid,
                                                  [In, Out] ref TmNativeDefs.TTMSElix elix,
                                                  UInt16                              eventMask,
                                                  UInt32                              startUnixTime,
                                                  UInt32                              endUnixTime);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcSetTimedValues(Int32                                   cid,
                                                 UInt32                                  count,
                                                 [In] TmNativeDefs.TTimedValueAndFlags[] values);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 tmcExecuteControlScript(Int32 cid,
                                                       Int16 ch,
                                                       Int16 rtu,
                                                       Int16 point,
                                                       Int16 cmd);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern bool tmcOverrideControlScript(Int32 cid,
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
    public static extern bool tmcEventLogAckRecords(Int32                          cid,
                                                    [In] ref TmNativeDefs.TTMSElix elix,
                                                    UInt32                         count);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int16 tmcGetObjectProperties(Int32                                          cid,
                                                      UInt16                                         objectType,
                                                      Int16                                          ch,
                                                      Int16                                          rtu,
                                                      Int16                                          point,
                                                      [MarshalAs(UnmanagedType.LPStr)] StringBuilder buf,
                                                      Int32                                          bufSize);


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
    public static extern Int16 tmcClrAnalogFlags(Int32 cid,
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
    public static extern IntPtr tmcAanReadArchive(Int32                                                   cid,
                                                  UInt32                                                  tmAddr,
                                                  UInt32                                                  startUnixTime,
                                                  UInt32                                                  endUnixTime,
                                                  UInt32                                                  step,
                                                  UInt32                                                  flags,
                                                  out                                    UInt32           count,
                                                  [MarshalAs(UnmanagedType.FunctionPtr)] TmNativeCallback progress,
                                                  IntPtr
                                                    progressParam);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 String2Utime([MarshalAs(UnmanagedType.LPStr)] string dateTime);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Byte tmcEnumObjects(Int32              cid,
                                             UInt16             objectType,
                                             Byte               count,
                                             [In, Out] UInt16[] buf,
                                             Int16              ch,
                                             Int16              rtu,
                                             Int16              toint);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Byte tmcGetObjectName(Int32                                          cid,
                                               UInt16                                         objectType,
                                               Int16                                          ch,
                                               Int16                                          rtu,
                                               Int16                                          toint,
                                               [MarshalAs(UnmanagedType.LPStr)] StringBuilder buf,
                                               Int32                                          bufSize);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr tmcTechObjReadValues(Int32                        cid,
                                                     [In] TmNativeDefs.TTechObj[] objects,
                                                     UInt32                       count);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern bool tmcTechObjBeginUpdate(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern bool tmcTechObjEndUpdate(Int32 cid);


    [DllImport(Tmconn, CallingConvention = CallingConvention.StdCall)]
    public static extern Int32 tmcTechObjWriteValues(Int32                             cid,
                                                     [In] TmNativeDefs.TTechObjProps[] props,
                                                     UInt32                            count);

    #endregion

    #region Rbc

    [DllImport(Tmconn, CallingConvention = CallingConvention.Cdecl)]
    public static extern UInt16 rbcIpgStartRedirector(Int32  cid,
                                                      UInt16 portIdx);


    [DllImport(Tmconn, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool rbcIpgStopRedirector(Int32  cid,
                                                   UInt16 portIdx);

    #endregion

    #region Cftree

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void cftNodeFreeTree(IntPtr id);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr cftNodeEnum(IntPtr id, Int32 idx);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNodeGetName(IntPtr                                         id,
                                               [MarshalAs(UnmanagedType.LPStr)] StringBuilder buf,
                                               UInt32                                         count);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNPropEnum(IntPtr                                         id,
                                             Int32                                          idx,
                                             [MarshalAs(UnmanagedType.LPStr)] StringBuilder buf,
                                             UInt32                                         count);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNPropGetText(IntPtr                                         id,
                                                [MarshalAs(UnmanagedType.LPStr)] string        name,
                                                [MarshalAs(UnmanagedType.LPStr)] StringBuilder buf,
                                                UInt32                                         count);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr cftNodeNewTree();


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNodeInsertAfter(IntPtr                                  id,
                                                   [MarshalAs(UnmanagedType.LPStr)] string nodeTag);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNodeInsertDown(IntPtr                                  id,
                                                  [MarshalAs(UnmanagedType.LPStr)] string nodeTag);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cftNPropSet(IntPtr                                  id,
                                          [MarshalAs(UnmanagedType.LPStr)] string propName,
                                          [MarshalAs(UnmanagedType.LPStr)] string propText);

    #endregion
  }
}