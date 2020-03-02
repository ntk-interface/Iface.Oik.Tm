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
  }
}