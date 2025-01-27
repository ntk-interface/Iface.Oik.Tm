using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public partial interface ITmNative
  {
    bool CfsInitLibrary(string baseDir = null,
                        string extArg  = null);


    void CfsSetUser(byte[] user,
                    byte[] password);


    void CfsSetUserForThread(byte[] user,
                             byte[] password);


    bool CfsCheckUserCred(IntPtr cfCid,
                          byte[] user,
                          byte[] password);

    IntPtr CfsMakeInprocCrd(byte[] machine, byte[] user, byte[] pwd);

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
                          byte[]                            name,
                          byte[]                            comment);


    void StracSetServerState(ref TmNativeDefs.TraceItemStorage tis,
                             UInt32                            state);


    bool CfsGetComputerInfo(IntPtr                         cfCid,
                            ref TmNativeDefs.ComputerInfoS cis,
                            out uint                       errCode,
                            ref byte[]                     errString,
                            uint                           maxErrs);


    bool CfsDirEnum(IntPtr     cfCid,
                    byte[]     path,
                    ref char[] buf,
                    uint       bufLength,
                    out uint   errCode,
                    ref byte[] errString,
                    uint       maxErrs);


    bool CfsFileGet(IntPtr                    cfCid,
                    byte[]                    remotePath,
                    byte[]                    localPath,
                    uint                      timeout,
                    ref TmNativeDefs.FileTime fileTime,
                    out uint                  errCode,
                    ref byte[]                errString,
                    uint                      maxErrs);


    bool CfsFileGetPropreties(IntPtr                             cfCid,
                              byte[]                             fileName,
                              ref TmNativeDefs.CfsFileProperties pProps,
                              out uint                           errCode,
                              ref byte[]                         errString,
                              uint                               maxErrs);


    bool CfsFilePut(IntPtr     connId,
                    byte[]     remoteFileName,
                    byte[]     localFileName,
                    uint       timeout,
                    out uint   errCode,
                    ref byte[] errString,
                    uint       maxErrs);


    bool CfsFileDelete(IntPtr     connId,
                       byte[]     remoteFileName,
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


    IntPtr CfsConnect(byte[]     serverName,
                      out uint   errCode,
                      ref byte[] errString,
                      uint       maxErrs);


    void CfsDisconnect(IntPtr connId);

    IntPtr CfsEditGrabCid(IntPtr     connId,
                          Boolean    bGrab,
                          byte[]     fileName,
                          byte[]     userName,
                          out uint   errCode,
                          ref byte[] errBuf,
                          uint       maxErrs);

    IntPtr CfsConfFileOpenCid(IntPtr                    connId,
                              byte[]                    serverName,
                              byte[]                    fileName,
                              uint                      timeout,
                              ref TmNativeDefs.FileTime fileTime,
                              out uint                  errCode,
                              ref byte[]                errString,
                              uint                      maxErrs);


    bool CfsConfFileSaveAs(IntPtr                    treeHandle,
                           byte[]                    serverName,
                           byte[]                    remoteFileName,
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
                               byte[]                       serverId,
                               ref TmNativeDefs.IfaceServer ifaceServer,
                               out uint                     errCode,
                               ref byte[]                   errBuf,
                               uint                         maxErrs);


    IntPtr CfsTraceEnumUsers(IntPtr     connId,
                             out uint   errCode,
                             ref byte[] errBuf,
                             uint       maxErrs);


    bool CfsTraceGetUserData(IntPtr                     connId,
                             byte[]                     userId,
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
                         byte[]     path,
                         byte[]     section,
                         byte[]     key,
                         byte[]     def,
                         ref byte[] value,
                         out uint   pcbValue,
                         out uint   errCode,
                         ref byte[] errBuf,
                         uint       maxErrs);

    bool CfsSetIniString(IntPtr     connId,
                         byte[]     path,
                         byte[]     section,
                         byte[]     key,
                         byte[]     value,
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
                           byte[]     username,
                           byte[]     password,
                           out uint   errCode,
                           ref byte[] errBuf,
                           uint       maxErrs);


    void DPrintF(byte[] message);
    void MPrintF(byte[] message);
    void EPrintF(byte[] message);


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
                            byte[]     fileName,
                            out UInt32 errCode,
                            ref byte[] errBuf,
                            uint       maxErrs);


    IntPtr CfsNodeFileLoad(byte[]     fileName,
                           out UInt32 errCode,
                           ref byte[] errBuf,
                           UInt32     maxErrs);


    IntPtr CfsConfFileOpen(byte[]                    serverName,
                           byte[]                    fileName,
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


    IntPtr CfsIfpcGetBin(IntPtr     connId,
                         byte[]     uName,
                         byte[]     oName,
                         byte[]     binName,
                         out UInt32 binLength,
                         out UInt32 errCode,
                         ref byte[] errBuf,
                         UInt32     maxErrs);


    bool CfsIfpcSetBin(IntPtr     connId,
                       byte[]     uName,
                       byte[]     oName,
                       byte[]     binName,
                       byte[]     buf,
                       UInt32     bufLength,
                       out UInt32 errCode,
                       ref byte[] errBuf,
                       UInt32     maxErrs);

    IntPtr CfsGetAccessDescriptor(string ini, string section);


    IntPtr CfsGetExtendedUserRightsDescriptor(string ini, string section, uint fCheck);


    IntPtr СfsIfpcEnumUsers(IntPtr   connId,
                            out uint errCode, ref byte[] errBuf, uint maxErrs);


    IntPtr СfsIfpcEnumOSUsers(IntPtr connId, out uint errCode, ref byte[] errBuf, uint maxErrs);


    Boolean СfsIfpcDeleteUser(IntPtr     connId,
                              byte[]     username,
                              out uint   errCode,
                              ref byte[] errBuf,
                              uint       maxErrs);


    uint СfsIfpcGetAccess(IntPtr     connId,
                          byte[]     uName,
                          byte[]     oName,
                          out uint   errCode,
                          ref byte[] errBuf,
                          uint       maxErrs);


    Boolean СfsIfpcSetAccess(IntPtr     connId,
                             byte[]     uName,
                             byte[]     oName,
                             uint       accessMask,
                             out uint   errCode,
                             ref byte[] errBuf,
                             uint       maxErrs);


    Boolean CfsSaveMachineConfig(Boolean    fFull,
                                 byte[]     remoteMasterMachine,
                                 byte[]     fileName,
                                 ref byte[] errBuf,
                                 uint       maxErrs);


    Boolean CfsSaveMachineConfigEx(byte[]           remoteMasterMachine,
                                   byte[]           fileName,
                                   uint             dwScope,
                                   TmNativeCallback progFn, IntPtr progParm,
                                   ref byte[]       errBuf, uint   maxErrs);


    Boolean CfsIfpcBackupSecurity(IntPtr     connId,
                                  byte[]     snp,
                                  byte[]     pwd,
                                  byte[]     filename,
                                  out UInt32 errCode,
                                  ref byte[] errBuf,
                                  UInt32     maxErrs);


    Boolean CfsIfpcRestoreSecurity(IntPtr     connId,
                                   byte[]     snp,
                                   byte[]     pwd,
                                   byte[]     filename,
                                   out UInt32 errCode,
                                   ref byte[] errBuf,
                                   UInt32     maxErrs);

    Boolean CfsPrepNewConfig(IntPtr     connId,
                             byte[]     remoteFName,
                             out UInt32 errCode,
                             ref byte[] errBuf,
                             UInt32     maxErrs);


    Boolean CfsIfpcTestTmcalc(IntPtr     connId,
                              byte[]     tmsName,
                              byte[]     clcName,
                              UInt32     testWay,
                              UInt32     testFlags,
                              out UInt64 pHandle,
                              out UInt32 pPid,
                              out UInt32 errCode,
                              ref byte[] errBuf,
                              UInt32     maxErrs);


    Boolean CfsIfpcStopTestTmcalc(IntPtr     connId,
                                  UInt64     handle,
                                  UInt32     pid,
                                  out UInt32 errCode,
                                  ref byte[] errBuf,
                                  UInt32     maxErrs);


    Boolean СfsPmonCheckProcess(IntPtr     connId,
                                byte[]     processNameArgs,
                                out UInt32 errCode,
                                ref byte[] errBuf,
                                UInt32     maxErrs);


    Boolean CfsPmonStopProcess(IntPtr     connId,
                               byte[]     processNameArgs,
                               out UInt32 pNumFound,
                               out UInt32 errCode,
                               ref byte[] errBuf,
                               UInt32     maxErrs);


    Boolean CfsPmonRestartProcess(IntPtr     connId,
                                  byte[]     processNameArgs,
                                  out UInt32 errCode,
                                  ref byte[] errBuf,
                                  UInt32     maxErrs);


    Boolean CfsSwapFnSrvRole(byte[]     serverName,
                             Boolean    bPre,
                             byte[]     fnsName,
                             out UInt32 errCode,
                             ref byte[] errBuf,
                             UInt32     maxErrs);

    
    bool CfsIsReserveWorking(IntPtr     connId,
                             uint       ipAddress,
                             ushort     ipBcPort,
                             ushort     ipPort,
                             uint       sType,
                             out bool   working,
                             ref byte[] sName,
                             out uint   errCode,
                             ref byte[] errBuf,
                             uint       maxErrs);

    
    bool CfsIfpcSetAbkParms(IntPtr     connId,
                            byte[]     pwd,
                            out UInt32 errCode,
                            ref byte[] errBuf,
                            UInt32     maxErrs);
  }
}