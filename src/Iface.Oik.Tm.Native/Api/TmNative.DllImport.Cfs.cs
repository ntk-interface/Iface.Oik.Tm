using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;
using static Iface.Oik.Tm.Native.Interfaces.TmNativeDefs;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern bool cfsInitLibrary([MarshalAs(UnmanagedType.LPStr)] string baseDir,
                                             [MarshalAs(UnmanagedType.LPStr)] string extArg);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void cfsSetUser(byte[] name,
                                         byte[] pwd);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void cfsSetUserForThread(byte[] name,
                                                  byte[] pwd);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsCheckUserCred(IntPtr cfCid,
                                               byte[] name,
                                               byte[] pwd);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 cfsGetExtendedUserData(IntPtr                                  cfCid,
                                                       [MarshalAs(UnmanagedType.LPStr)] string serverType,
                                                       [MarshalAs(UnmanagedType.LPStr)] string serverName,
                                                       IntPtr                                  buf,
                                                       UInt32                                  bufSize);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsPmonLocalRegisterProcess(Int32              argc,
                                                          [In, Out] string[] argv,
                                                          ref       IntPtr   phStartEvt,
                                                          ref       IntPtr   phStopEvt);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool strac_AllocServer(ref TmNativeDefs.TraceItemStorage tis,
                                                UInt32                            pid,
                                                UInt32                            ppid,
                                                byte[]                            name,
                                                byte[]                            comment);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void strac_SetServerState(ref TmNativeDefs.TraceItemStorage tis,
                                                   UInt32                            state);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsGetComputerInfo(IntPtr                               cfCid,
                                                 ref       TmNativeDefs.ComputerInfoS cis,
                                                 out       UInt32                     errCode,
                                                 [In, Out] byte[]                     errBuf,
                                                 UInt32                               maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsDirEnum(IntPtr cfCid,
                                         byte[] path,
                                         [MarshalAs(UnmanagedType.LPArray)] [In, Out]
                                         char[] buf,
                                         UInt32           bufLength,
                                         out       UInt32 errCode,
                                         [In, Out] byte[] errBuf,
                                         UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsFileGet(IntPtr                              cfCid,
                                         byte[]                              remotePath,
                                         byte[]                              localPath,
                                         UInt32                              timeout,
                                         [In, Out] ref TmNativeDefs.FileTime fileTime,
                                         out           UInt32                errCode,
                                         [In, Out]     byte[]                errBuf,
                                         UInt32                              maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsFileGetPropreties(IntPtr                                       cfCid,
                                                   byte[]                                       fileName,
                                                   [In, Out] ref TmNativeDefs.CfsFileProperties pProps,
                                                   out           UInt32                         errCode,
                                                   [In, Out]     byte[]                         errBuf,
                                                   UInt32                                       maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsFilePut(IntPtr           connId,
                                         byte[]           remoteFileName,
                                         byte[]           localFileName,
                                         UInt32           timeout,
                                         out       UInt32 errCode,
                                         [In, Out] byte[] errBuf,
                                         UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsFileDelete(IntPtr           connId,
                                            byte[]           remoteFileName,
                                            out       UInt32 errCode,
                                            [In, Out] byte[] errBuf,
                                            UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsConnect(byte[]           serverName,
                                           out       UInt32 errCode,
                                           [In, Out] byte[] errBuf,
                                           UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void cfsDisconnect(IntPtr connId);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsConfFileOpenCid(IntPtr                              connId,
                                                   byte[]                              serverName,
                                                   byte[]                              fileName,
                                                   UInt32                              timeout,
                                                   [In, Out] ref TmNativeDefs.FileTime fileTime,
                                                   out           UInt32                errCode,
                                                   [In, Out]     byte[]                errBuf,
                                                   UInt32                              maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsConfFileSaveAs(IntPtr                              treeHandle,
                                                byte[]                              serverName,
                                                byte[]                              remoteFileName,
                                                UInt32                              timeout,
                                                [In, Out] ref TmNativeDefs.FileTime fileTime,
                                                out           UInt32                errCode,
                                                [In, Out]     byte[]                errBuf,
                                                UInt32                              maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 cfsGetSoftwareType(IntPtr connId);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 cfsIfpcMaster(IntPtr connId, Byte command);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern bool cfsIsConnected(IntPtr connId);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsTraceEnumServers(IntPtr           connId,
                                                    out       UInt32 errCode,
                                                    [In, Out] byte[] errBuf,
                                                    UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceGetServerData(IntPtr                                 connId,
                                                    byte[]                                 serverId,
                                                    [In, Out] ref TmNativeDefs.IfaceServer ifaceServer,
                                                    out           UInt32                   errCode,
                                                    [In, Out]     byte[]                   errBuf,
                                                    UInt32                                 maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsTraceEnumUsers(IntPtr           connId,
                                                  out       UInt32 errCode,
                                                  [In, Out] byte[] errBuf,
                                                  UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceGetUserData(IntPtr                               connId,
                                                  byte[]                               userId,
                                                  [In, Out] ref TmNativeDefs.IfaceUser ifaceUser,
                                                  out           UInt32                 errCode,
                                                  [In, Out]     byte[]                 errBuf,
                                                  UInt32                               maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceStopProcess(IntPtr           connId,
                                                  UInt32           processId,
                                                  out       UInt32 errCode,
                                                  [In, Out] byte[] errBuf,
                                                  UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceRestartProcess(IntPtr           connId,
                                                     UInt32           processId,
                                                     out       UInt32 errCode,
                                                     [In, Out] byte[] errBuf,
                                                     UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceBeginTraceEx(IntPtr           connId,
                                                   UInt32           pid,
                                                   UInt32           thid,
                                                   bool             fDebug,
                                                   UInt32           pause,
                                                   out       UInt32 errCode,
                                                   [In, Out] byte[] errBuf,
                                                   UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceEndTrace(IntPtr           connId,
                                               out       UInt32 errCode,
                                               [In, Out] byte[] errBuf,
                                               UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsTraceGetMessage(IntPtr           connId,
                                                   out       UInt32 errCode,
                                                   [In, Out] byte[] errBuf,
                                                   UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void cfsFreeMemory(IntPtr memory);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean cfsIfpcGetLogonToken(IntPtr           connId,
                                                      [In, Out] byte[] tokUname,
                                                      [In, Out] byte[] tokToken,
                                                      out       UInt32 errCode,
                                                      [In, Out] byte[] errBuf,
                                                      UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern bool cfsLogOpen(IntPtr           connId,
                                         out       UInt32 errCode,
                                         [In, Out] byte[] errBuf,
                                         UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern bool cfsLogClose(IntPtr           connId,
                                          out       UInt32 errCode,
                                          [In, Out] byte[] errBuf,
                                          UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsLogGetRecord(IntPtr           connId,
                                                bool             fFirst,
                                                out       UInt32 errCode,
                                                [In, Out] byte[] errBuf,
                                                uint             maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsEnumThreads(IntPtr           connId,
                                               out       UInt32 errCode,
                                               [In, Out] byte[] errBuf,
                                               UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsGetIniString(IntPtr           connId,
                                              byte[]           path,
                                              byte[]           section,
                                              byte[]           key,
                                              byte[]           def,
                                              [In, Out] byte[] value,
                                              out       UInt32 pcbValue,
                                              out       UInt32 errCode,
                                              [In, Out] byte[] errBuf,
                                              UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsSetIniString(IntPtr           connId,
                                              byte[]           path,
                                              byte[]           section,
                                              byte[]           key,
                                              byte[]           value,
                                              out       UInt32 errCode,
                                              [In, Out] byte[] errBuf,
                                              UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsCheckInstallationIntegrity(IntPtr           connId,
                                                            UInt32           kind,
                                                            out       IntPtr pSig,
                                                            out       IntPtr pErrs,
                                                            out       UInt32 errCode,
                                                            [In, Out] byte[] errBuf,
                                                            UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsGetBasePath(IntPtr           connId,
                                             [In, Out] byte[] path,
                                             UInt32           cbPath,
                                             out       UInt32 errCode,
                                             [In, Out] byte[] errBuf,
                                             UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsEnumTimezones(IntPtr           connId,
                                                 out       UInt32 errCode,
                                                 [In, Out] byte[] errBuf,
                                                 UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsIfpcNewUserSystemAvaliable(IntPtr           connId,
                                                            out       UInt32 flags,
                                                            out       UInt32 errCode,
                                                            [In, Out] byte[] errBuf,
                                                            UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsIfpcSetUserPwd(IntPtr           connId,
                                                byte[]           username,
                                                byte[]           password,
                                                out       UInt32 errCode,
                                                [In, Out] byte[] errBuf,
                                                UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int64 uxgmtime2uxtime(Int64 time);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void e_printf(byte[] format,
                                       byte[] message);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void m_printf(byte[] format,
                                       byte[] message);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void d_printf(byte[] format,
                                       byte[] message);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr lf_ParseMessage(IntPtr           stringPtrToParse,
                                                [In, Out] byte[] sTime,
                                                [In, Out] byte[] sDate,
                                                [In, Out] byte[] sName,
                                                [In, Out] byte[] sType,
                                                [In, Out] byte[] sMsgType,
                                                [In, Out] byte[] sThid);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Int32 cfslzDecompress(IntPtr               inBuffer,
                                               UInt32               inLength,
                                               IntPtr               outBuffPtr,
                                               [In, Out] ref UInt32 outLength);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsNodeFileSave(IntPtr           treeHandle,
                                                 byte[]           fileName,
                                                 out       UInt32 errCode,
                                                 [In, Out] byte[] errBuf,
                                                 UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsNodeFileLoad(byte[]           fileName,
                                                out       UInt32 errCode,
                                                [In, Out] byte[] errBuf,
                                                UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsEditGrabCid(IntPtr           connId,
                                               Boolean          bGrab,
                                               byte[]           fileName,
                                               byte[]           userName,
                                               out       UInt32 errCode,
                                               [In, Out] byte[] errBuf,
                                               UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsConfFileOpen(byte[]                              serverName,
                                                byte[]                              fileName,
                                                UInt32                              timeout,
                                                [In, Out] ref TmNativeDefs.FileTime fileTime,
                                                out           UInt32                errCode,
                                                [In, Out]     byte[]                errBuf,
                                                UInt32                              maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt64 cfsSlogOpen(IntPtr           connId,
                                            UInt32           logType,
                                            UInt32           fileStartIndex,
                                            UInt32           direction,
                                            out       UInt32 errCode,
                                            [In, Out] byte[] errBuf,
                                            UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsSlogReadRecords(IntPtr           connId,
                                                   UInt64           sLogHandle,
                                                   out       UInt32 errCode,
                                                   [In, Out] byte[] errBuf,
                                                   UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsSlogClose(IntPtr           connId,
                                              UInt64           sLogHandle,
                                              out       UInt32 errCode,
                                              [In, Out] byte[] errBuf,
                                              UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsIfpcGetBin(IntPtr           connId,
                                              byte[]           uName,
                                              byte[]           oName,
                                              byte[]           binName,
                                              out       UInt32 binLength,
                                              out       UInt32 errCode,
                                              [In, Out] byte[] errBuf,
                                              UInt32           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsIfpcSetBin(IntPtr           connId,
                                               byte[]           uName,
                                               byte[]           oName,
                                               byte[]           binName,
                                               [In] byte[]      buf,
                                               UInt32           bufLength,
                                               out       UInt32 errCode,
                                               [In, Out] byte[] errBuf,
                                               UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsGetAccessDescriptor([MarshalAs(UnmanagedType.LPStr)] string ini,
                                                       [MarshalAs(UnmanagedType.LPStr)] string section);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsGetExtendedUserRightsDescriptor([MarshalAs(UnmanagedType.LPStr)] string ini,
                                                                   [MarshalAs(UnmanagedType.LPStr)] string section,
                                                                   uint                                    fCheck);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsIfpcEnumUsers(IntPtr           connId,
                                                 out       UInt32 errCode,
                                                 [In, Out] byte[] errBuf,
                                                 UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsIfpcEnumOSUsers(IntPtr           connId,
                                                   out       UInt32 errCode,
                                                   [In, Out] byte[] errBuf,
                                                   UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsIfpcDeleteUser(IntPtr           connId,
                                                   byte[]           username,
                                                   out       UInt32 errCode,
                                                   [In, Out] byte[] errBuf,
                                                   UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 cfsIfpcGetAccess(IntPtr           connId,
                                                 byte[]           uName,
                                                 byte[]           oName,
                                                 out       UInt32 errCode,
                                                 [In, Out] byte[] errBuf,
                                                 UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsIfpcSetAccess(IntPtr           connId,
                                                  byte[]           uName,
                                                  byte[]           oName,
                                                  UInt32           accessMask,
                                                  out       UInt32 errCode,
                                                  [In, Out] byte[] errBuf,
                                                  UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsSaveMachineConfig(Boolean          fFull,
                                                      byte[]           remoteMasterMachine,
                                                      byte[]           fileName,
                                                      [In, Out] byte[] errBuf,
                                                      UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsSaveMachineConfigEx(byte[] remoteMasterMachine,
                                                        byte[] fileName,
                                                        uint dwScope,
                                                        [MarshalAs(UnmanagedType.FunctionPtr)] TmNativeCallback progFn,
                                                        IntPtr progParm,
                                                        [In, Out] byte[] errBuf,
                                                        UInt32 maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsExternalBackupServer(IntPtr                            connId,
                                                         byte[]                            dllName,
                                                         byte[]                            servName,
                                                         uint                              bFlags,
                                                         [In, Out] ref CfsServerBackupData pbd,
                                                         out           UInt32              errCode,
                                                         [In, Out]     byte[]              errBuf,
                                                         UInt32                            maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsExternalRestoreServer(IntPtr           connId,
                                                          byte[]           dllName,
                                                          byte[]           servName,
                                                          byte[]           filename,
                                                          out       UInt32 pbFlags,
                                                          out       UInt32 errCode,
                                                          [In, Out] byte[] errBuf,
                                                          UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsIfpcBackupSecurity(IntPtr           connId,
                                                       byte[]           snp,
                                                       byte[]           pwd,
                                                       byte[]           filename,
                                                       out       UInt32 errCode,
                                                       [In, Out] byte[] errBuf,
                                                       UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsIfpcRestoreSecurity(IntPtr           connId,
                                                        byte[]           snp,
                                                        byte[]           pwd,
                                                        byte[]           filename,
                                                        out       UInt32 errCode,
                                                        [In, Out] byte[] errBuf,
                                                        UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsMakeInprocCrd(byte[] machine, byte[] user, byte[] pwd);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsPrepNewConfig(IntPtr           connId,
                                                  byte[]           remoteFName,
                                                  out       UInt32 errCode,
                                                  [In, Out] byte[] errBuf,
                                                  UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsIfpcTestTmcalc(IntPtr           connId,
                                                   byte[]           tmsName,
                                                   byte[]           clcName,
                                                   UInt32           testWay,
                                                   UInt32           testFlags,
                                                   out       UInt64 pHandle,
                                                   out       UInt32 pPid,
                                                   out       UInt32 errCode,
                                                   [In, Out] byte[] errBuf,
                                                   UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsIfpcStopTestTmcalc(IntPtr           connId,
                                                       UInt64           handle,
                                                       UInt32           pid,
                                                       out       UInt32 errCode,
                                                       [In, Out] byte[] errBuf,
                                                       UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsPmonCheckProcess(IntPtr           connId,
                                                     byte[]           processNameArgs,
                                                     out       UInt32 errCode,
                                                     [In, Out] byte[] errBuf,
                                                     UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsPmonStopProcess(IntPtr           connId,
                                                    byte[]           processNameArgs,
                                                    out       UInt32 pNumFound,
                                                    out       UInt32 errCode,
                                                    [In, Out] byte[] errBuf,
                                                    UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsPmonRestartProcess(IntPtr           connId,
                                                       byte[]           processNameArgs,
                                                       out       UInt32 errCode,
                                                       [In, Out] byte[] errBuf,
                                                       UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsSwapFnSrvRole(byte[]           serverName,
                                                  Boolean          bPre,
                                                  byte[]           fnsName,
                                                  out       UInt32 errCode,
                                                  [In, Out] byte[] errBuf,
                                                  UInt32           maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsIsReserveWorking(IntPtr            connId,
                                                     UInt32            ipAddress,
                                                     UInt16            ipBcPort,
                                                     UInt16            ipPort,
                                                     UInt32            sType,
                                                     out       Boolean working,
                                                     [In, Out] byte[]  sName, // 64 
                                                     out       UInt32  errCode,
                                                     [In, Out] byte[]  errBuf,
                                                     UInt32            maxErrs);
  }
}