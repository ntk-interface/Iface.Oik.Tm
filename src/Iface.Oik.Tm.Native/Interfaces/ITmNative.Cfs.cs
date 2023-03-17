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


    void CfsSetUserForThread(string user,
                             string password);


    UInt32 CfsGetExtendedUserData(IntPtr cfCid,
                                  string serverType,
                                  string serverName,
                                  IntPtr buf,
                                  UInt32 bufSize);


    bool CfsPmonLocalRegisterProcess(Int32      argc,
                                     string[]   argv,
                                     ref IntPtr phStartEvt,
                                     ref IntPtr phStopEvt);


    bool StracAllocServer(ref TmNativeDefs.TraceItemStorage tis,
                          UInt32                            pid,
                          UInt32                            ppid,
                          string                            name,
                          string                            comment);


    void StracSetServerState(ref TmNativeDefs.TraceItemStorage tis,
                             UInt32                            state);


    bool CfsGetComputerInfo(IntPtr                         cfCid,
                            ref TmNativeDefs.ComputerInfoS cis,
                            out uint                       errCode,
                            ref byte[]                     errString,
                            uint                           maxErrs);


    bool CfsDirEnum(IntPtr     cfCid,
                    string     path,
                    ref char[] buf,
                    uint       bufLength,
                    out uint   errCode,
                    ref byte[] errString,
                    uint       maxErrs);


    bool CfsFileGet(IntPtr     cfCid,
                    string     remotePath,
                    string     localPath,
                    uint       timeout,
                    IntPtr     fileTime,
                    out uint   errCode,
                    ref byte[] errString,
                    uint       maxErrs);


    bool CfsFileGetPropreties(IntPtr                             cfCid,
                              string                             fileName,
                              ref TmNativeDefs.CfsFileProperties pProps,
                              out uint                           errCode,
                              ref byte[]                         errString,
                              uint                               maxErrs);


    bool CfsFilePut(IntPtr     connId,
                    string     remoteFileName,
                    string     localFileName,
                    uint       timeout,
                    out uint   errCode,
                    ref byte[] errString,
                    uint       maxErrs);


    bool CfsFileDelete(IntPtr     connId,
                       string     remoteFileName,
                       out uint   errCode,
                       ref byte[] errString,
                       uint       maxErrs);


    bool CfsCheckInstallationIntegrity(IntPtr     connId,
                                       uint       kind,
                                       out IntPtr pSig,
                                       out IntPtr pErrs,
                                       out uint   errCode,
                                       ref byte[] errBuf,
                                       uint       maxErrs);


    Int64 UxGmTime2UxTime(Int64 time);


    IntPtr CfsConnect(string     serverName,
                      out uint   errCode,
                      ref byte[] errString,
                      uint       maxErrs);


    void CfsDisconnect(IntPtr connId);


    IntPtr CfsConfFileOpenCid(IntPtr                    connId,
                              string                    serverName,
                              string                    fileName,
                              uint                      timeout,
                              ref TmNativeDefs.FileTime fileTime,
                              out uint                  errCode,
                              ref byte[]                errString,
                              uint                      maxErrs);


    bool CfsConfFileSaveAs(IntPtr                    treeHandle,
                           string                    serverName,
                           string                    remoteFileName,
                           uint                      timeout,
                           ref TmNativeDefs.FileTime fileTime,
                           out uint                  errCode,
                           ref byte[]                errString,
                           uint                      maxErrs);


    UInt32 CfsGetSoftwareType(IntPtr connId);


    UInt32 CfsIfpcMaster(IntPtr connId, Byte command);


    bool CfsIsConnected(IntPtr connId);


    IntPtr CfsTraceEnumServers(IntPtr     connId,
                               out uint   errCode,
                               ref byte[] errBuf,
                               uint       maxErrs);


    bool CfsTraceGetServerData(IntPtr                       connId,
                               string                       serverId,
                               ref TmNativeDefs.IfaceServer ifaceServer,
                               out uint                     errCode,
                               ref byte[]                   errBuf,
                               uint                         maxErrs);


    IntPtr CfsTraceEnumUsers(IntPtr     connId,
                             out uint   errCode,
                             ref byte[] errBuf,
                             uint       maxErrs);


    bool CfsTraceGetUserData(IntPtr                     connId,
                             string                     userId,
                             ref TmNativeDefs.IfaceUser ifaceServer,
                             out uint                   errCode,
                             ref byte[]                 errBuf,
                             uint                       maxErrs);


    bool CfsTraceStopProcess(IntPtr     connId,
                             uint       processId,
                             out uint   errCode,
                             ref byte[] errBuf,
                             uint       maxErrs);


    bool CfsTraceRestartProcess(IntPtr     connId,
                                uint       processId,
                                out uint   errCode,
                                ref byte[] errBuf,
                                uint       maxErrs);


    bool CfsTraceBeginTraceEx(IntPtr     connId,
                              uint       processId,
                              uint       threadId,
                              bool       debug,
                              uint       pause,
                              out uint   errCode,
                              ref byte[] errBuf,
                              uint       maxErrs);


    bool CfsTraceEndTrace(IntPtr     connId,
                          out uint   errCode,
                          ref byte[] errBuf,
                          uint       maxErrs);


    IntPtr CfsTraceGetMessage(IntPtr     connId,
                              out uint   errCode,
                              ref byte[] errBuf,
                              uint       maxErrs);


    void CfsFreeMemory(IntPtr memory);


    bool CfsIfpcGetLogonToken(IntPtr     cfCid,
                              ref byte[] tokUname,
                              ref byte[] tokToken,
                              out uint   errCode,
                              ref byte[] errBuf,
                              uint       maxErrs);


    bool CfsLogOpen(IntPtr     connId,
                    out uint   errCode,
                    ref byte[] errBuf,
                    uint       maxErrs);


    bool CfsLogClose(IntPtr     connId,
                     out uint   errCode,
                     ref byte[] errBuf,
                     uint       maxErrs);


    IntPtr CfsLogGetRecord(IntPtr     connId,
                           bool       fFirst,
                           out uint   errCode,
                           ref byte[] errBuf,
                           uint       maxErrs);


    IntPtr CfsEnumThreads(IntPtr     connId,
                          out uint   errCode,
                          ref byte[] errBuf,
                          uint       maxErrs);


    bool CfsGetIniString(IntPtr     connId,
                         string     path,
                         string     section,
                         string     key,
                         string     def,
                         ref byte[] value,
                         out uint   pcbValue,
                         out uint   errCode,
                         ref byte[] errBuf,
                         uint       maxErrs);

    bool CfsSetIniString(IntPtr     connId,
                         string     path,
                         string     section,
                         string     key,
                         string     value,
                         out uint   errCode,
                         ref byte[] errBuf,
                         uint       maxErrs);


    bool СfsGetBasePath(IntPtr     connId,
                        ref byte[] path,
                        uint       cbPath,
                        out uint   errCode,
                        ref byte[] errBuf,
                        uint       maxErrs);

    IntPtr CfsEnumTimezones(IntPtr     connId,
                            out UInt32 errCode,
                            ref byte[] errBuf,
                            UInt32     maxErrs);


    bool CfsIfpcNewUserSystemAvaliable(IntPtr     connId,
                                       out uint   flags,
                                       out uint   errCode,
                                       ref byte[] errBuf,
                                       uint       maxErrs);


    bool CfsIfpcSetUserPwd(IntPtr     connId,
                           string     username,
                           string     password,
                           out uint   errCode,
                           ref byte[] errBuf,
                           uint       maxErrs);


    void DPrintF(string message);
    void MPrintF(string message);
    void EPrintF(string message);


    IntPtr LfParseMessage(IntPtr     stringPtrToParse,
                          ref byte[] time,
                          ref byte[] date,
                          ref byte[] name,
                          ref byte[] type,
                          ref byte[] msgType,
                          ref byte[] thid);


    Int32 CfsLzDecompress(IntPtr     inBuffer,
                          UInt32     inLength,
                          IntPtr     outBuffPtr,
                          ref UInt32 outLength);


    Boolean CfsNodeFileSave(IntPtr     treeHandle,
                            string     fileName,
                            out UInt32 errCode,
                            ref byte[] errBuf,
                            uint       maxErrs);


    IntPtr CfsNodeFileLoad(string     fileName,
                           out UInt32 errCode,
                           ref byte[] errBuf,
                           UInt32     maxErrs);


    IntPtr CfsConfFileOpen(string                    serverName,
                           string                    fileName,
                           uint                      timeout,
                           ref TmNativeDefs.FileTime fileTime,
                           out uint                  errCode,
                           ref byte[]                errString,
                           uint                      maxErrs);


    ulong СfsSLogOpen(IntPtr     connId,
                      uint       logType,
                      uint       fileStartIndex,
                      uint       direction,
                      out uint   errCode,
                      ref byte[] errBuf,
                      uint       maxErrs);


    IntPtr CfsSLogReadRecords(IntPtr     connId,
                              ulong      sLogHandle,
                              out uint   errCode,
                              ref byte[] errBuf,
                              uint       maxErrs);


    bool СfsSLogClose(IntPtr     connId,
                      ulong      sLogHandle,
                      out uint   errCode,
                      ref byte[] errBuf,
                      uint       maxErrs);
  }
}