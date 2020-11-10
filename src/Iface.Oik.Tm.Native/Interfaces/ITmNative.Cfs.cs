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


    bool CfsGetComputerInfo(IntPtr                         cfCid,
                            ref TmNativeDefs.ComputerInfoS cis,
                            out UInt32                     errCode,
                            ref StringBuilder              errString,
                            UInt32                         maxErrs);


    bool CfsDirEnum(IntPtr            cfCid,
                    string            path,
                    ref char[]        buf,
                    uint              bufLength,
                    out uint          errCode,
                    ref StringBuilder errString,
                    uint              maxErrs);


    bool CfsFileGet(IntPtr            cfCid,
                    string            remotePath,
                    string            localPath,
                    UInt32            timeout,
                    IntPtr            fileTime,
                    out UInt32        errCode,
                    ref StringBuilder errString,
                    UInt32            maxErrs);


    bool CfsFileGetPropreties(IntPtr                             cfCid,
                              string                             fileName,
                              ref TmNativeDefs.CfsFileProperties pProps,
                              out UInt32                         errCode,
                              ref StringBuilder                  errString,
                              UInt32                             maxErrs);


    bool CfsFilePut(IntPtr            connId,
                    string            remoteFileName,
                    string            localFileName,
                    UInt32            timeout,
                    out UInt32        errCode,
                    ref StringBuilder errString,
                    UInt32            maxErrs);


    bool CfsFileDelete(IntPtr            connId,
                       string            remoteFileName,
                       out UInt32        errCode,
                       ref StringBuilder errString,
                       UInt32            maxErrs);
    

    bool CfsCheckInstallationIntegrity(IntPtr            connId,
                                       UInt32            kind,
                                       out IntPtr        pSig,
                                       out IntPtr        pErrs,
                                       out UInt32        errCode,
                                       ref StringBuilder errString,
                                       UInt32            maxErrs);


    Int64 UxGmTime2UxTime(Int64 time);


    IntPtr CfsConnect(string            serverName,
                      out UInt32        errCode,
                      ref StringBuilder errString,
                      UInt32            maxErrs);


    void CfsDisconnect(IntPtr connId);


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
                               out UInt32                   errCode,
                               ref StringBuilder            errString,
                               UInt32                       maxErrs);


    IntPtr CfsTraceEnumUsers(IntPtr            connId,
                             out UInt32        errCode,
                             ref StringBuilder errString,
                             UInt32            maxErrs);


    bool CfsTraceGetUserData(IntPtr                     connId,
                             string                     userId,
                             ref TmNativeDefs.IfaceUser ifaceServer,
                             out UInt32                 errCode,
                             ref StringBuilder          errString,
                             uint                       maxErrs);


    bool CfsTraceStopProcess(IntPtr            connId,
                             UInt32            processId,
                             out UInt32        errCode,
                             ref StringBuilder errString,
                             UInt32            maxErrs);


    bool CfsTraceRestartProcess(IntPtr            connId,
                                UInt32            processId,
                                out UInt32        errCode,
                                ref StringBuilder errString,
                                UInt32            maxErrs);


    bool CfsTraceBeginTraceEx(IntPtr            connId,
                              UInt32            processId,
                              UInt32            threadId,
                              bool              debug,
                              UInt32            pause,
                              out UInt32        errCode,
                              ref StringBuilder errString,
                              UInt32            maxErrs);


    bool CfsTraceEndTrace(IntPtr            connId,
                          out UInt32        errCode,
                          ref StringBuilder errString,
                          UInt32            maxErrs);


    IntPtr CfsTraceGetMessage(IntPtr            connId,
                              out UInt32        errCode,
                              ref StringBuilder errString,
                              UInt32            maxErrs);


    void CfsFreeMemory(IntPtr memory);


    bool CfsIfpcGetLogonToken(IntPtr            cfCid,
                              ref StringBuilder tokUname,
                              ref StringBuilder tokToken,
                              out UInt32        errCode,
                              ref StringBuilder errString,
                              UInt32            maxErrs);


    bool CfsLogOpen(IntPtr            connId,
                    out UInt32        errCode,
                    ref StringBuilder errString,
                    UInt32            maxErrs);


    bool CfsLogClose(IntPtr            connId,
                     out UInt32        errCode,
                     ref StringBuilder errString,
                     UInt32            maxErrs);


    IntPtr CfsLogGetRecord(IntPtr            connId,
                           bool              fFirst,
                           out UInt32        errCode,
                           ref StringBuilder errString,
                           UInt32            maxErrs);


    IntPtr CfsEnumThreads(IntPtr            connId,
                          out UInt32        errCode,
                          ref StringBuilder errString,
                          UInt32            maxErrs);


    bool CfsGetIniString(IntPtr            connId,
                         string            path,
                         string            section,
                         string            key,
                         string            def,
                         ref StringBuilder value,
                         out UInt32        pcbValue,
                         out UInt32        errCode,
                         ref StringBuilder errString,
                         UInt32            maxErrs);

    bool CfsSetIniString(IntPtr            connId,
                         string            path,
                         string            section,
                         string            key,
                         string            value,
                         out UInt32        errCode,
                         ref StringBuilder errString,
                         UInt32            maxErrs);


    bool СfsGetBasePath(IntPtr            connId,
                        ref StringBuilder path,
                        UInt32            cbPath,
                        out UInt32        errCode,
                        ref StringBuilder errString,
                        UInt32            maxErrs);


    void DPrintF(string message);
    void MPrintF(string message);
    void EPrintF(string message);


    IntPtr LfParseMessage(IntPtr            stringPtrToParse,
                          ref StringBuilder time,
                          ref StringBuilder date,
                          ref StringBuilder name,
                          ref StringBuilder type,
                          ref StringBuilder msgType,
                          ref StringBuilder thid);
  }
}