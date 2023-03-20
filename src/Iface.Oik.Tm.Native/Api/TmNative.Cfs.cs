using System;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    public bool CfsInitLibrary(string baseDir = null, string extArg = null)
    {
#if x86
      return false;
#else
      return cfsInitLibrary(baseDir, extArg);
#endif
    }


    public void CfsSetUser(string user,
                           string password)
    {
      cfsSetUser(user, password);
    }


    public void CfsSetUserForThread(string user,
                                    string password)
    {
      cfsSetUserForThread(user, password);
    }


    public UInt32 CfsGetExtendedUserData(IntPtr cfCid,
                                         string serverType,
                                         string serverName,
                                         IntPtr buf,
                                         UInt32 bufSize)
    {
      return cfsGetExtendedUserData(cfCid, serverType, serverName, buf, bufSize);
    }


    public bool CfsPmonLocalRegisterProcess(Int32      argc,
                                            string[]   argv,
                                            ref IntPtr phStartEvt,
                                            ref IntPtr phStopEvt)
    {
      return cfsPmonLocalRegisterProcess(argc, argv, ref phStartEvt, ref phStopEvt);
    }


    public bool StracAllocServer(ref TmNativeDefs.TraceItemStorage tis,
                                 UInt32                            pid,
                                 UInt32                            ppid,
                                 string                            name,
                                 string                            comment)
    {
      return strac_AllocServer(ref tis, pid, ppid, name, comment);
    }


    public void StracSetServerState(ref TmNativeDefs.TraceItemStorage tis,
                                    UInt32                            state)
    {
      strac_SetServerState(ref tis, state);
    }


    public bool CfsGetComputerInfo(IntPtr                         cfCid,
                                   ref TmNativeDefs.ComputerInfoS cis,
                                   out uint                       errCode,
                                   ref byte[]                     errBuf,
                                   uint                           maxErrs)
    {
      return cfsGetComputerInfo(cfCid, ref cis, out errCode, errBuf, maxErrs);
    }


    public bool CfsDirEnum(IntPtr     cfCid,
                           string     path,
                           ref char[] buf,
                           uint       bufLength,
                           out uint   errCode,
                           ref byte[] errBuf,
                           uint       maxErrs)
    {
      return cfsDirEnum(cfCid, path, buf, bufLength, out errCode, errBuf, maxErrs);
    }


    public bool CfsFileGet(IntPtr     cfCid,
                           string     remotePath,
                           string     localPath,
                           uint       timeout,
                           IntPtr     fileTime,
                           out uint   errCode,
                           ref byte[] errBuf,
                           uint       maxErrs)
    {
      return cfsFileGet(cfCid, remotePath, localPath, timeout, fileTime, out errCode, errBuf, maxErrs);
    }


    public bool CfsFileGetPropreties(IntPtr                             cfCid,
                                     string                             fileName,
                                     ref TmNativeDefs.CfsFileProperties pProps,
                                     out uint                           errCode,
                                     ref byte[]                         errBuf,
                                     uint                               maxErrs)
    {
      return cfsFileGetPropreties(cfCid, fileName, ref pProps, out errCode, errBuf, maxErrs);
    }


    public bool CfsFilePut(IntPtr     connId,
                           string     remoteFileName,
                           string     localFileName,
                           uint       timeout,
                           out uint   errCode,
                           ref byte[] errBuf,
                           uint       maxErrs)
    {
      return cfsFilePut(connId, remoteFileName, localFileName, timeout, out errCode, errBuf, maxErrs);
    }


    public bool CfsFileDelete(IntPtr     connId,
                              string     remoteFileName,
                              out uint   errCode,
                              ref byte[] errBuf,
                              uint       maxErrs)
    {
      return cfsFileDelete(connId, remoteFileName, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsConnect(string     serverName,
                             out uint   errCode,
                             ref byte[] errBuf,
                             uint       maxErrs)
    {
      return cfsConnect(serverName, out errCode, errBuf, maxErrs);
    }


    public void CfsDisconnect(IntPtr connId)
    {
      cfsDisconnect(connId);
    }


    public IntPtr CfsConfFileOpenCid(IntPtr                    connId,
                                     string                    serverName,
                                     string                    fileName,
                                     uint                      timeout,
                                     ref TmNativeDefs.FileTime fileTime,
                                     out uint                  errCode,
                                     ref byte[]                errBuf,
                                     uint                      maxErrs)
    {
      return cfsConfFileOpenCid(connId, serverName, fileName, timeout, ref fileTime, out errCode, errBuf, maxErrs);
    }


    public bool CfsConfFileSaveAs(IntPtr                    treeHandle,
                                  string                    serverName,
                                  string                    remoteFileName,
                                  uint                      timeout,
                                  ref TmNativeDefs.FileTime fileTime,
                                  out uint                  errCode,
                                  ref byte[]                errBuf,
                                  uint                      maxErrs)
    {
      return cfsConfFileSaveAs(treeHandle, serverName, remoteFileName, timeout, ref fileTime, out errCode, errBuf,
                               maxErrs);
    }


    public UInt32 CfsGetSoftwareType(IntPtr connId)
    {
      return cfsGetSoftwareType(connId);
    }


    public UInt32 CfsIfpcMaster(IntPtr connId, Byte command)
    {
      return cfsIfpcMaster(connId, command);
    }


    public bool CfsIsConnected(IntPtr connId)
    {
      return cfsIsConnected(connId);
    }


    public IntPtr CfsTraceEnumServers(IntPtr     connId,
                                      out uint   errCode,
                                      ref byte[] errBuf,
                                      uint       maxErrs)
    {
      return cfsTraceEnumServers(connId, out errCode, errBuf, maxErrs);
    }


    public bool CfsTraceGetServerData(IntPtr                       connId,
                                      string                       serverId,
                                      ref TmNativeDefs.IfaceServer ifaceServer,
                                      out uint                     errCode,
                                      ref byte[]                   errBuf,
                                      uint                         maxErrs)
    {
      return cfsTraceGetServerData(connId, serverId, ref ifaceServer, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsTraceEnumUsers(IntPtr     connId,
                                    out uint   errCode,
                                    ref byte[] errBuf,
                                    uint       maxErrs)
    {
      return cfsTraceEnumUsers(connId, out errCode, errBuf, maxErrs);
    }


    public bool CfsTraceGetUserData(IntPtr                     connId,
                                    string                     userId,
                                    ref TmNativeDefs.IfaceUser ifaceServer,
                                    out uint                   errCode,
                                    ref byte[]                 errBuf,
                                    uint                       maxErrs)
    {
      return cfsTraceGetUserData(connId, userId, ref ifaceServer, out errCode, errBuf, maxErrs);
    }


    public bool CfsTraceStopProcess(IntPtr connId, uint processId, out uint errCode, ref byte[] errBuf,
                                    uint   maxErrs)
    {
      return cfsTraceStopProcess(connId, processId, out errCode, errBuf, maxErrs);
    }


    public bool CfsTraceRestartProcess(IntPtr connId, uint processId, out uint errCode, ref byte[] errBuf,
                                       uint   maxErrs)
    {
      return cfsTraceRestartProcess(connId, processId, out errCode, errBuf, maxErrs);
    }


    public bool CfsTraceBeginTraceEx(IntPtr     connId,
                                     uint       processId,
                                     uint       threadId,
                                     bool       debug,
                                     uint       pause,
                                     out uint   errCode,
                                     ref byte[] errBuf,
                                     uint       maxErrs)
    {
      return cfsTraceBeginTraceEx(connId, processId, threadId, debug, pause, out errCode, errBuf, maxErrs);
    }


    public bool CfsTraceEndTrace(IntPtr     connId,
                                 out uint   errCode,
                                 ref byte[] errBuf,
                                 uint       maxErrs)
    {
      return cfsTraceEndTrace(connId, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsTraceGetMessage(IntPtr     connId,
                                     out uint   errCode,
                                     ref byte[] errBuf,
                                     uint       maxErrs)
    {
      return cfsTraceGetMessage(connId, out errCode, errBuf, maxErrs);
    }


    public void CfsFreeMemory(IntPtr memory)
    {
      cfsFreeMemory(memory);
    }


    public bool CfsIfpcGetLogonToken(IntPtr     cfCid,
                                     ref byte[] tokUname,
                                     ref byte[] tokToken,
                                     out uint   errCode,
                                     ref byte[] errBuf,
                                     uint       maxErrs)
    {
      return cfsIfpcGetLogonToken(cfCid, tokUname, tokToken, out errCode, errBuf, maxErrs);
    }


    public bool CfsLogOpen(IntPtr connId, out uint errCode, ref byte[] errBuf, uint maxErrs)
    {
      return cfsLogOpen(connId, out errCode, errBuf, maxErrs);
    }


    public bool CfsLogClose(IntPtr connId, out uint errCode, ref byte[] errBuf, uint maxErrs)
    {
      return cfsLogClose(connId, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsLogGetRecord(IntPtr connId, bool fFirst, out uint errCode, ref byte[] errBuf,
                                  uint   maxErrs)
    {
      return cfsLogGetRecord(connId, fFirst, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsEnumThreads(IntPtr connId, out uint errCode, ref byte[] errBuf, uint maxErrs)
    {
      return cfsEnumThreads(connId, out errCode, errBuf, maxErrs);
    }


    public bool CfsGetIniString(IntPtr     connId,
                                string     path,
                                string     section,
                                string     key,
                                string     def,
                                ref byte[] value,
                                out uint   pcbValue,
                                out uint   errCode,
                                ref byte[] errBuf,
                                uint       maxErrs)
    {
      return cfsGetIniString(connId, path, section, key, def, value, out pcbValue, out errCode, errBuf, maxErrs);
    }


    public bool CfsSetIniString(IntPtr     connId,
                                string     path,
                                string     section,
                                string     key,
                                string     value,
                                out uint   errCode,
                                ref byte[] errBuf,
                                uint       maxErrs)
    {
      return cfsSetIniString(connId, path, section, key, value, out errCode, errBuf, maxErrs);
    }


    public bool CfsCheckInstallationIntegrity(IntPtr     connId,
                                              uint       kind,
                                              out IntPtr pSig,
                                              out IntPtr pErrs,
                                              out uint   errCode,
                                              ref byte[] errBuf,
                                              uint       maxErrs)
    {
      return cfsCheckInstallationIntegrity(connId, kind, out pSig, out pErrs, out errCode, errBuf, maxErrs);
    }


    public bool СfsGetBasePath(IntPtr     connId,
                               ref byte[] path,
                               uint       cbPath,
                               out uint   errCode,
                               ref byte[] errBuf,
                               uint       maxErrs)
    {
      return cfsGetBasePath(connId, path, cbPath, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsEnumTimezones(IntPtr     connId,
                                   out UInt32 errCode,
                                   ref byte[] errBuf,
                                   UInt32     maxErrs)
    {
      return cfsEnumTimezones(connId, out errCode, errBuf, maxErrs);
    }


    public bool CfsIfpcNewUserSystemAvaliable(IntPtr     connId,
                                              out UInt32 flags,
                                              out uint   errCode,
                                              ref byte[] errBuf,
                                              uint       maxErrs)
    {
      return cfsIfpcNewUserSystemAvaliable(connId, out flags, out errCode, errBuf, maxErrs);
    }


    public bool CfsIfpcSetUserPwd(IntPtr     connId,
                                  string     username,
                                  string     password,
                                  out uint   errCode,
                                  ref byte[] errBuf,
                                  uint       maxErrs)
    {
      return cfsIfpcSetUserPwd(connId, username, password, out errCode, errBuf, maxErrs);
    }


    public Int64 UxGmTime2UxTime(Int64 time)
    {
      return uxgmtime2uxtime(time);
    }


    public void DPrintF(string message)
    {
      d_printf("%s", message);
    }


    public void MPrintF(string message)
    {
      m_printf("%s", message);
    }


    public void EPrintF(string message)
    {
      e_printf("%s", message);
    }


    public IntPtr LfParseMessage(IntPtr     stringPtrToParse,
                                 ref byte[] time,
                                 ref byte[] date,
                                 ref byte[] name,
                                 ref byte[] type,
                                 ref byte[] msgType,
                                 ref byte[] thid)
    {
      return lf_ParseMessage(stringPtrToParse, time, date, name, type, msgType, thid);
    }


    public Int32 CfsLzDecompress(IntPtr     inBuffer,
                                 UInt32     inLength,
                                 IntPtr     outBuffPtr,
                                 ref UInt32 outLength)
    {
      return cfslzDecompress(inBuffer, inLength, outBuffPtr, ref outLength);
    }


    public Boolean CfsNodeFileSave(IntPtr     treeHandle,
                                   string     fileName,
                                   out UInt32 errCode,
                                   ref byte[] errBuf,
                                   UInt32     maxErrs)
    {
      return cfsNodeFileSave(treeHandle, fileName, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsNodeFileLoad(string     fileName,
                                  out UInt32 errCode,
                                  ref byte[] errBuf,
                                  UInt32     maxErrs)
    {
      return cfsNodeFileLoad(fileName, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsConfFileOpen(string                    serverName,
                                  string                    fileName,
                                  uint                      timeout,
                                  ref TmNativeDefs.FileTime fileTime,
                                  out uint                  errCode,
                                  ref byte[]                errBuf,
                                  uint                      maxErrs)
    {
      return cfsConfFileOpen(serverName, fileName, timeout, ref fileTime, out errCode, errBuf, maxErrs);
    }


    public ulong СfsSLogOpen(IntPtr     connId,
                             uint       logType,
                             uint       fileStartIndex,
                             uint       direction,
                             out uint   errCode,
                             ref byte[] errBuf,
                             uint       maxErrs)
    {
      return cfsSlogOpen(connId, logType, fileStartIndex, direction, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsSLogReadRecords(IntPtr     connId,
                                     ulong      sLogHandle,
                                     out uint   errCode,
                                     ref byte[] errBuf,
                                     uint       maxErrs)
    {
      return cfsSlogReadRecords(connId, sLogHandle, out errCode, errBuf, maxErrs);
    }


    public bool СfsSLogClose(IntPtr     connId,
                             ulong      sLogHandle,
                             out uint   errCode,
                             ref byte[] errBuf,
                             uint       maxErrs)
    {
      return cfsSlogClose(connId, sLogHandle, out errCode, errBuf, maxErrs);
    }
  }
}