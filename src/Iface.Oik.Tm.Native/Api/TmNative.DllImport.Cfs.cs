using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;
using static Iface.Oik.Tm.Native.Interfaces.TmNativeDefs;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial void cfsSetUtf8Encoding([MarshalAs(UnmanagedType.Bool)] Boolean bSet);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern bool cfsInitLibrary(byte[] baseDir,
                                             byte[] extArg);


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


    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial UInt32 cfsGetExtendedUserData(nint                                     cfCid,
                                                          string                                   serverType,
                                                          string                                   serverName,
                                                          ref TmNativeDefsUnsafe.TExtendedUserInfo userInfo,
                                                          uint                                     bufSize);


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


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool cfsGetComputerInfo(nint                                 cfCid,
                                                    ref TmNativeDefsUnsafe.ComputerInfoS cis,
                                                    out uint                             errCode,
                                                    Span<byte>                           errBuf,
                                                    uint                                 maxErrs);


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


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial IntPtr cfsConnect(byte[]     serverName,
                                            out UInt32 errCode,
                                            Span<byte> errBuf,
                                            UInt32     maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void cfsDisconnect(IntPtr connId);


    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial nint cfsConfFileOpenCid(nint                            connId,
                                                    string                          serverName,
                                                    string                          fileName,
                                                    uint                            timeout,
                                                    ref TmNativeDefsUnsafe.FileTime fileTime,
                                                    out uint                        errCode,
                                                    Span<byte>                      errBuf,
                                                    uint                            maxErrs);

    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool cfsConfFileSaveAs(nint                            treeHandle,
                                                   string                          serverName,
                                                   string                          remoteFileName,
                                                   uint                            timeout,
                                                   ref TmNativeDefsUnsafe.FileTime fileTime,
                                                   out uint                        errCode,
                                                   Span<byte>                      errBuf,
                                                   uint                            maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 cfsGetSoftwareType(IntPtr connId);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 cfsIfpcMaster(IntPtr connId, Byte command);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern bool cfsIsConnected(IntPtr connId);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial nint cfsTraceEnumServers(nint       connId,
                                                   out uint   errCode,
                                                   Span<byte> errBuf,
                                                   uint       maxErrs);


    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool cfsTraceGetServerData(nint                               connId,
                                                       string                             serverId,
                                                       ref TmNativeDefsUnsafe.IfaceServer ifaceServer,
                                                       out uint                           errCode,
                                                       Span<byte>                         errBuf,
                                                       uint                               maxErrs);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial nint cfsTraceEnumUsers(nint       connId,
                                                 out uint   errCode,
                                                 Span<byte> errBuf,
                                                 uint       maxErrs);


    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool cfsTraceGetUserData(nint                             connId,
                                                     string                           userId,
                                                     ref TmNativeDefsUnsafe.IfaceUser ifaceUser,
                                                     out uint                         errCode,
                                                     Span<byte>                       errBuf,
                                                     uint                             maxErrs);


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


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool cfsTraceBeginTraceEx(nint                                 connId,
                                                    uint                                 pid,
                                                    uint                                 thid,
                                                    [MarshalAs(UnmanagedType.Bool)] bool fDebug,
                                                    uint                                 pause,
                                                    out uint                             errCode,
                                                    Span<byte>                           errBuf,
                                                    uint                                 maxErrs);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool cfsTraceEndTrace(nint       connId,
                                                out uint   errCode,
                                                Span<byte> errBuf,
                                                uint       maxErrs);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial nint cfsTraceGetMessage(nint       connId,
                                                  out uint   errCode,
                                                  Span<byte> errBuf,
                                                  uint       maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void cfsFreeMemory(IntPtr memory);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool cfsIfpcGetLogonToken(nint       connId,
                                                    Span<byte> tokUname,
                                                    Span<byte> tokToken,
                                                    out uint   errCode,
                                                    Span<byte> errBuf,
                                                    uint       maxErrs);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool cfsLogOpen(nint       connId,
                                          out uint   errCode,
                                          Span<byte> errBuf,
                                          uint       maxErrs);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool cfsLogClose(nint       connId,
                                             out uint   errCode,
                                             Span<byte> errBuf,
                                             uint       maxErrs);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial nint cfsLogGetRecord(nint                                 connId,
                                               [MarshalAs(UnmanagedType.Bool)] bool fFirst,
                                               out                             uint errCode,
                                               Span<byte>                           errBuf,
                                               uint                                 maxErrs);

    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial nint cfsLogGetRecordEx(nint                                 connId,
                                                   [MarshalAs(UnmanagedType.Bool)] bool fFirst,
                                                   out                             uint errCode,
                                                   Span<byte>                           errBuf,
                                                   uint                                 maxErrs);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial nint cfsEnumThreads(nint       connId,
                                              out uint   errCode,
                                              Span<byte> errBuf,
                                              uint       maxErrs);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool cfsGetIniString(IntPtr     connId,
                                               byte[]     path,
                                               byte[]     section,
                                               byte[]     key,
                                               byte[]     def,
                                               Span<byte> value,
                                               out UInt32 pcbValue,
                                               out UInt32 errCode,
                                               Span<byte> errBuf,
                                               UInt32     maxErrs);


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


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool cfsGetBasePath(IntPtr     connId,
                                              Span<byte> path,
                                              UInt32     cbPath,
                                              out UInt32 errCode,
                                              Span<byte> errBuf,
                                              UInt32     maxErrs);


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


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool cfsIfpcSetUserPwd(IntPtr     connId,
                                                 byte[]     username,
                                                 byte[]     password,
                                                 out UInt32 errCode,
                                                 Span<byte> errBuf,
                                                 UInt32     maxErrs);


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


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial nint lf_ParseMessage(Span<byte> stringPtrToParse,
                                                 Span<byte> sTime,
                                                 Span<byte> sDate,
                                                 Span<byte> sName,
                                                 Span<byte> sType,
                                                 Span<byte> sMsgType,
                                                 Span<byte> sThid);


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

    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial nint cfsEditGrabCid(nint                                 connId,
                                              [MarshalAs(UnmanagedType.Bool)] bool bGrab,
                                              string                               fileName,
                                              string                               userName,
                                              out uint                             errCode,
                                              Span<byte>                           errBuf,
                                              uint                                 maxErrs);

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


    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial nint cfsIfpcGetBin(nint       connId,
                                             string     uName,
                                             string     oName,
                                             string     binName,
                                             out uint   binLength,
                                             out uint   errCode,
                                             Span<byte> errBuf,
                                             uint       maxErrs);


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
    public static extern IntPtr cfsGetAccessDescriptor(byte[] ini,
                                                       byte[] section);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsGetExtendedUserRightsDescriptor(byte[] ini,
                                                                   byte[] section,
                                                                   uint   fCheck);

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

    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool cfsIfpcBackupSecurity(nint       connId,
                                                     string     snp,
                                                     string     pwd,
                                                     string     filename,
                                                     out uint   errCode,
                                                     Span<byte> errBuf,
                                                     uint       maxErrs);

    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool cfsIfpcRestoreSecurity(IntPtr     connId,
                                                      string     snp,
                                                      string     pwd,
                                                      string     filename,
                                                      out uint   errCode,
                                                      Span<byte> errBuf,
                                                      uint       maxErr);

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


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial Boolean cfsIsReserveWorking(IntPtr                                      connId,
                                                      UInt32                                      ipAddress,
                                                      UInt16                                      ipBcPort,
                                                      UInt16                                      ipPort,
                                                      UInt32                                      sType,
                                                      [MarshalAs(UnmanagedType.Bool)] out Boolean working,
                                                      Span<byte>                                  sName, // 64 
                                                      out UInt32                                  errCode,
                                                      Span<byte>                                  errBuf,
                                                      UInt32                                      maxErrs);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial Boolean cfsIfpcSetAbkParms(IntPtr     connId,
                                                     byte[]     pwd,
                                                     out UInt32 errCode,
                                                     Span<byte> errBuf,
                                                     UInt32     maxErrs);


    [LibraryImport(Cfshare)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool cfsIsUTF8(Span<byte> text);
  }
}