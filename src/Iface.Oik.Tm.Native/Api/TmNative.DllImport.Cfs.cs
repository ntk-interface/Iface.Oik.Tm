using System;
using System.Runtime.InteropServices;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
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
    public static extern bool cfsGetComputerInfo(IntPtr                                                      cfCid,
                                                 ref                              TmNativeDefs.ComputerInfoS cis,
                                                 out                              UInt32                     errCode,
                                                 [MarshalAs(UnmanagedType.LPStr)] StringBuilder              errString,
                                                 UInt32                                                      maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsDirEnum(IntPtr                                  cfCid,
                                         [MarshalAs(UnmanagedType.LPStr)] string path,
                                         [MarshalAs(UnmanagedType.LPArray)] [In, Out]
                                         char[] buf,
                                         UInt32                                         bufLength,
                                         out                              UInt32        errCode,
                                         [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                         UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsFileGet(IntPtr                                         cfCid,
                                         [MarshalAs(UnmanagedType.LPStr)] string        remotePath,
                                         [MarshalAs(UnmanagedType.LPStr)] string        localPath,
                                         UInt32                                         timeout,
                                         IntPtr                                         fileTime,
                                         out                              UInt32        errCode,
                                         [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                         UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsFileGetPropreties(IntPtr cfCid,
                                                   [MarshalAs(UnmanagedType.LPStr)] string fileName,
                                                   [In, Out] ref TmNativeDefs.CfsFileProperties pProps,
                                                   out UInt32 errCode,
                                                   [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                   UInt32 maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsConnect([MarshalAs(UnmanagedType.LPStr)] string        serverName,
                                           out                              UInt32        errCode,
                                           [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                           UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void cfsDisconnect(IntPtr connId);


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


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsTraceEnumServers(IntPtr                                         connId,
                                                    out                              UInt32        errCode,
                                                    [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                    UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceGetServerData(IntPtr connId,
                                                    [MarshalAs(UnmanagedType.LPStr)] string serverId,
                                                    [In, Out] ref TmNativeDefs.IfaceServer ifaceServer,
                                                    out UInt32 errCode,
                                                    [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                    UInt32 maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsTraceEnumUsers(IntPtr                                         connId,
                                                  out                              UInt32        errCode,
                                                  [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                  UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceGetUserData(IntPtr                                                  connId,
                                                  [MarshalAs(UnmanagedType.LPStr)] string                 userId,
                                                  [In, Out] ref                    TmNativeDefs.IfaceUser ifaceUser,
                                                  out                              UInt32                 errCode,
                                                  [MarshalAs(UnmanagedType.LPStr)] StringBuilder          errString,
                                                  UInt32                                                  maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceStopProcess(IntPtr                                         connId,
                                                  UInt32                                         processId,
                                                  out                              UInt32        errCode,
                                                  [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                  UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceRestartProcess(IntPtr                                         connId,
                                                     UInt32                                         processId,
                                                     out                              UInt32        errCode,
                                                     [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                     UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceBeginTraceEx(IntPtr                                         connId,
                                                   UInt32                                         pid,
                                                   UInt32                                         thid,
                                                   bool                                           fDebug,
                                                   UInt32                                         pause,
                                                   out                              UInt32        errCode,
                                                   [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                   UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsTraceEndTrace(IntPtr                                         connId,
                                               out                              UInt32        errCode,
                                               [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                               UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsTraceGetMessage(IntPtr                                         connId,
                                                   out                              UInt32        errCode,
                                                   [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                   UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void cfsFreeMemory(IntPtr memory);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean cfsIfpcGetLogonToken(IntPtr                                         connId,
                                                      [MarshalAs(UnmanagedType.LPStr)] StringBuilder tokUname,
                                                      [MarshalAs(UnmanagedType.LPStr)] StringBuilder tokToken,
                                                      out                              UInt32        errCode,
                                                      [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                      UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern bool cfsLogOpen(IntPtr                                         connId,
                                         out                              UInt32        errCode,
                                         [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                         UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern bool cfsLogClose(IntPtr                                         connId,
                                          out                              UInt32        errCode,
                                          [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                          UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsLogGetRecord(IntPtr                                         connId,
                                                bool                                           fFirst,
                                                out                              UInt32        errCode,
                                                [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                uint                                           maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cfsEnumThreads(IntPtr                                         connId,
                                               out                              UInt32        errCode,
                                               [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                               UInt32                                         maxErrs);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Boolean cfsGetIniString(IntPtr                                         connId,
                                                 [MarshalAs(UnmanagedType.LPStr)] string        path,
                                                 [MarshalAs(UnmanagedType.LPStr)] string        section,
                                                 [MarshalAs(UnmanagedType.LPStr)] string        key,
                                                 [MarshalAs(UnmanagedType.LPStr)] string        def,
                                                 [MarshalAs(UnmanagedType.LPStr)] StringBuilder value,
                                                 out                              UInt32        pcbValue,
                                                 out                              UInt32        errCode,
                                                 [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                 UInt32                                         maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cfsCheckInstallationIntegrity(IntPtr                                         connId,
                                                            UInt32                                         kind,
                                                            out IntPtr                                         pSig,
                                                            out IntPtr                                         pErrs,
                                                            out                              UInt32        errCode,
                                                            [MarshalAs(UnmanagedType.LPStr)] StringBuilder errString,
                                                            UInt32                                         maxErrs);

    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int64 uxgmtime2uxtime(Int64 time);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void e_printf([MarshalAs(UnmanagedType.LPStr)] string format);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void m_printf([MarshalAs(UnmanagedType.LPStr)] string format);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void d_printf([MarshalAs(UnmanagedType.LPStr)] string format);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr lf_ParseMessage(IntPtr                                         stringPtrToParse,
                                                [MarshalAs(UnmanagedType.LPStr)] StringBuilder sTime,
                                                [MarshalAs(UnmanagedType.LPStr)] StringBuilder sDate,
                                                [MarshalAs(UnmanagedType.LPStr)] StringBuilder sName,
                                                [MarshalAs(UnmanagedType.LPStr)] StringBuilder sType,
                                                [MarshalAs(UnmanagedType.LPStr)] StringBuilder sMsgType,
                                                [MarshalAs(UnmanagedType.LPStr)] StringBuilder sThid);
  }
}