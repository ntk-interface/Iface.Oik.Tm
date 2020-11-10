using System;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    public bool CfsInitLibrary(string baseDir = null, string extArg = null)
    {
      return cfsInitLibrary(baseDir, extArg);
    }


    public void CfsSetUser(string user,
                           string password)
    {
      cfsSetUser(user, password);
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
                                            ref UInt32 phStartEvt,
                                            ref UInt32 phStopEvt)
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
                                   out UInt32                     errCode,
                                   ref StringBuilder              errString,
                                   UInt32                         maxErrs)
    {
      return cfsGetComputerInfo(cfCid, ref cis, out errCode, errString, maxErrs);
    }


    public bool CfsDirEnum(IntPtr            cfCid,
                           string            path,
                           ref char[]        buf,
                           uint              bufLength,
                           out uint          errCode,
                           ref StringBuilder errString,
                           uint              maxErrs)
    {
      return cfsDirEnum(cfCid, path, buf, bufLength, out errCode, errString, maxErrs);
    }


    public bool CfsFileGet(IntPtr            cfCid,
                           string            remotePath,
                           string            localPath,
                           UInt32            timeout,
                           IntPtr            fileTime,
                           out UInt32        errCode,
                           ref StringBuilder errString,
                           UInt32            maxErrs)
    {
      return cfsFileGet(cfCid, remotePath, localPath, timeout, fileTime, out errCode, errString, maxErrs);
    }


    public bool CfsFileGetPropreties(IntPtr                             cfCid,
                                     string                             fileName,
                                     ref TmNativeDefs.CfsFileProperties pProps,
                                     out UInt32                         errCode,
                                     ref StringBuilder                  errString,
                                     UInt32                             maxErrs)
    {
      return cfsFileGetPropreties(cfCid, fileName, ref pProps, out errCode, errString, maxErrs);
    }


    public bool CfsFilePut(IntPtr            connId,
                           string            remoteFileName,
                           string            localFileName,
                           UInt32            timeout,
                           out UInt32        errCode,
                           ref StringBuilder errString,
                           UInt32            maxErrs)
    {
      return cfsFilePut(connId, remoteFileName, localFileName, timeout, out errCode, errString, maxErrs);
    }


    public bool CfsFileDelete(IntPtr            connId,
                              string            remoteFileName,
                              out UInt32        errCode,
                              ref StringBuilder errString,
                              UInt32            maxErrs)
    {
      return cfsFileDelete(connId, remoteFileName, out errCode, errString, maxErrs);
    }
    

    public IntPtr CfsConnect(string            serverName,
                             out UInt32        errCode,
                             ref StringBuilder errString,
                             UInt32            maxErrs)
    {
      return cfsConnect(serverName, out errCode, errString, maxErrs);
    }


    public void CfsDisconnect(IntPtr connId)
    {
      cfsDisconnect(connId);
    }


    public IntPtr CfsConfFileOpenCid(IntPtr                    connId,
                                     string                    serverName,
                                     string                    fileName,
                                     UInt32                    timeout,
                                     ref TmNativeDefs.FileTime fileTime,
                                     out UInt32                errCode,
                                     ref StringBuilder         errString,
                                     UInt32                    maxErrs)
    {
      return cfsConfFileOpenCid(connId, serverName, fileName, timeout, ref fileTime, out errCode, errString, maxErrs);
    }


    public bool CfsConfFileSaveAs(IntPtr                    treeHandle,
                                  string                    serverName,
                                  string                    remoteFileName,
                                  UInt32                    timeout,
                                  ref TmNativeDefs.FileTime fileTime,
                                  out UInt32                errCode,
                                  ref StringBuilder         errString,
                                  UInt32                    maxErrs)
    {
      return cfsConfFileSaveAs(treeHandle, serverName, remoteFileName, timeout, ref fileTime, out errCode, errString,
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


    public IntPtr CfsTraceEnumServers(IntPtr            connId,
                                      out UInt32        errCode,
                                      ref StringBuilder errString,
                                      UInt32            maxErrs)
    {
      return cfsTraceEnumServers(connId, out errCode, errString, maxErrs);
    }


    public bool CfsTraceGetServerData(IntPtr                       connId,
                                      string                       serverId,
                                      ref TmNativeDefs.IfaceServer ifaceServer,
                                      out UInt32                   errCode,
                                      ref StringBuilder            errString,
                                      UInt32                       maxErrs)
    {
      return cfsTraceGetServerData(connId, serverId, ref ifaceServer, out errCode, errString, maxErrs);
    }


    public IntPtr CfsTraceEnumUsers(IntPtr            connId,
                                    out UInt32        errCode,
                                    ref StringBuilder errString,
                                    UInt32            maxErrs)
    {
      return cfsTraceEnumUsers(connId, out errCode, errString, maxErrs);
    }


    public bool CfsTraceGetUserData(IntPtr                     connId,
                                    string                     userId,
                                    ref TmNativeDefs.IfaceUser ifaceServer,
                                    out UInt32                 errCode,
                                    ref StringBuilder          errString,
                                    UInt32                     maxErrs)
    {
      return cfsTraceGetUserData(connId, userId, ref ifaceServer, out errCode, errString, maxErrs);
    }


    public bool CfsTraceStopProcess(IntPtr connId, UInt32 processId, out UInt32 errCode, ref StringBuilder errString,
                                    UInt32 maxErrs)
    {
      return cfsTraceStopProcess(connId, processId, out errCode, errString, maxErrs);
    }


    public bool CfsTraceRestartProcess(IntPtr connId, UInt32 processId, out UInt32 errCode, ref StringBuilder errString,
                                       UInt32 maxErrs)
    {
      return cfsTraceRestartProcess(connId, processId, out errCode, errString, maxErrs);
    }


    public bool CfsTraceBeginTraceEx(IntPtr            connId,
                                     UInt32            processId,
                                     UInt32            threadId,
                                     bool              debug,
                                     UInt32            pause,
                                     out uint          errCode,
                                     ref StringBuilder errString,
                                     UInt32            maxErrs)
    {
      return cfsTraceBeginTraceEx(connId, processId, threadId, debug, pause, out errCode, errString, maxErrs);
    }


    public bool CfsTraceEndTrace(IntPtr            connId,
                                 out UInt32        errCode,
                                 ref StringBuilder errString,
                                 UInt32            maxErrs)
    {
      return cfsTraceEndTrace(connId, out errCode, errString, maxErrs);
    }


    public IntPtr CfsTraceGetMessage(IntPtr            connId,
                                     out UInt32        errCode,
                                     ref StringBuilder errString,
                                     UInt32            maxErrs)
    {
      return cfsTraceGetMessage(connId, out errCode, errString, maxErrs);
    }


    public void CfsFreeMemory(IntPtr memory)
    {
      cfsFreeMemory(memory);
    }


    public bool CfsIfpcGetLogonToken(IntPtr            cfCid,
                                     ref StringBuilder tokUname,
                                     ref StringBuilder tokToken,
                                     out UInt32        errCode,
                                     ref StringBuilder errString,
                                     UInt32            maxErrs)
    {
      return cfsIfpcGetLogonToken(cfCid, tokUname, tokToken, out errCode, errString, maxErrs);
    }


    public bool CfsLogOpen(IntPtr connId, out UInt32 errCode, ref StringBuilder errString, UInt32 maxErrs)
    {
      return cfsLogOpen(connId, out errCode, errString, maxErrs);
    }


    public bool CfsLogClose(IntPtr connId, out UInt32 errCode, ref StringBuilder errString, UInt32 maxErrs)
    {
      return cfsLogClose(connId, out errCode, errString, maxErrs);
    }


    public IntPtr CfsLogGetRecord(IntPtr connId, bool fFirst, out UInt32 errCode, ref StringBuilder errString,
                                  UInt32 maxErrs)
    {
      return cfsLogGetRecord(connId, fFirst, out errCode, errString, maxErrs);
    }


    public IntPtr CfsEnumThreads(IntPtr connId, out UInt32 errCode, ref StringBuilder errString, UInt32 maxErrs)
    {
      return cfsEnumThreads(connId, out errCode, errString, maxErrs);
    }


    public bool CfsGetIniString(IntPtr            connId,
                                string            path,
                                string            section,
                                string            key,
                                string            def,
                                ref StringBuilder value,
                                out UInt32        pcbValue,
                                out UInt32        errCode,
                                ref StringBuilder errString,
                                uint              maxErrs)
    {
      return cfsGetIniString(connId, path, section, key, def, value, out pcbValue, out errCode, errString, maxErrs);
    }


    public bool CfsSetIniString(IntPtr            connId,
                                string            path,
                                string            section,
                                string            key,
                                string            value,
                                out UInt32        errCode,
                                ref StringBuilder errString,
                                UInt32            maxErrs)
    {
      return cfsSetIniString(connId, path, section, key, value, out errCode, errString, maxErrs);
    }


    public bool CfsCheckInstallationIntegrity(IntPtr            connId,
                                              UInt32            kind,
                                              out IntPtr        pSig,
                                              out IntPtr        pErrs,
                                              out UInt32        errCode,
                                              ref StringBuilder errString,
                                              UInt32            maxErrs)
    {
      return cfsCheckInstallationIntegrity(connId, kind, out pSig, out pErrs, out errCode, errString, maxErrs);
    }


    public bool СfsGetBasePath(IntPtr            connId,
                               ref StringBuilder path,
                               UInt32            cbPath,
                               out UInt32        errCode,
                               ref StringBuilder errString,
                               UInt32            maxErrs)
    {
      return cfsGetBasePath(connId, path, cbPath, out errCode, errString, maxErrs);
    }


    public Int64 UxGmTime2UxTime(Int64 time)
    {
      return uxgmtime2uxtime(time);
    }


    public void DPrintF(string message)
    {
      d_printf(message);
    }


    public void MPrintF(string message)
    {
      m_printf(message);
    }


    public void EPrintF(string message)
    {
      e_printf(message);
    }


    public IntPtr LfParseMessage(IntPtr            stringPtrToParse,
                                 ref StringBuilder time,
                                 ref StringBuilder date,
                                 ref StringBuilder name,
                                 ref StringBuilder type,
                                 ref StringBuilder msgType,
                                 ref StringBuilder thid)
    {
      return lf_ParseMessage(stringPtrToParse, time, date, name, type, msgType, thid);
    }
  }
}