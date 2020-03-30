using System;
using System.Text;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public partial interface ITmNative
  {
    bool CfsInitLibrary(string baseDir = null,
                        string extArg  = null);


    void CfsSetUser(string user,
                    string password);


    UInt32 CfsGetExtendedUserData(UInt32 cfCid,
                                  string serverType,
                                  string serverName,
                                  IntPtr buf,
                                  UInt32 bufSize);


    UInt32 CfsGetExtendedUserData(IntPtr cfCid,
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


    IntPtr CfsConnect(string            serverName,
                      out UInt32        errCode,
                      ref StringBuilder errString,
                      UInt32            maxErrs);


    IntPtr CfsConfFileOpenCid(IntPtr                    connId,
                              string                    serverName,
                              string                    fileName,
                              UInt32                    timeout,
                              ref TmNativeDefs.FileTime fileTime,
                              out UInt32                errCode,
                              ref StringBuilder         errString,
                              UInt32                    maxErrs);


    bool CfsConfFileSaveAs(IntPtr                    treeHandle,
                           string                    serverName,
                           string                    remoteFileName,
                           UInt32                    timeout,
                           ref TmNativeDefs.FileTime fileTime,
                           out UInt32                errCode,
                           ref StringBuilder         errString,
                           UInt32                    maxErrs);

    UInt32 CfsGetSoftwareType(IntPtr connId);


    UInt32 CfsIfpcMaster(IntPtr connId, Byte command);


    bool CfsIsConnected(IntPtr connId);


    IntPtr CfsTraceEnumServers(IntPtr            connId,
                               out UInt32        errCode,
                               ref StringBuilder errString,
                               UInt32            maxErrs);


    bool CfsTraceGetServerData(IntPtr                       connId,
                               string                       serverId,
                               ref TmNativeDefs.IfaceServer ifaceServer,
                               out uint                     errCode,
                               ref StringBuilder            errString,
                               uint                         maxErrs);

    IntPtr CfsTraceEnumUsers(IntPtr            connId,
                             out UInt32        errCode,
                             ref StringBuilder errString,
                             UInt32            maxErrs);


    bool CfsTraceGetUserData(IntPtr                     connId,
                             string                     userId,
                             ref TmNativeDefs.IfaceUser ifaceServer,
                             out uint                   errCode,
                             ref StringBuilder          errString,
                             uint                       maxErrs);


    void CfsFreeMemory(IntPtr memory);


    void DPrintF(string message);
    void MPrintF(string message);
    void EPrintF(string message);
  }
}