using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using static Iface.Oik.Tm.Native.Interfaces.TmNativeDefs;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    public bool CfsInitLibrary(byte[] baseDir = null, byte[] extArg = null)
    {
#if x86
      return false;
#else
      return cfsInitLibrary(baseDir, extArg ?? Encoding.GetEncoding(1251).GetBytes("nosig"));
#endif
    }


    public void CfsSetUser(byte[] user,
                           byte[] password)
    {
      cfsSetUser(user, password);
    }


    public void CfsSetUserForThread(byte[] user,
                                    byte[] password)
    {
      cfsSetUserForThread(user, password);
    }


    public bool CfsCheckUserCred(IntPtr cfCid,
                                 byte[] user,
                                 byte[] password)
    {
      return cfsCheckUserCred(cfCid, user, password);
    }


    public uint CfsGetExtendedUserData(IntPtr cfCid,
                                       byte[] serverType,
                                       byte[] serverName,
                                       IntPtr buf,
                                       uint   bufSize)
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
                                 byte[]                            name,
                                 byte[]                            comment)
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
                           byte[]     path,
                           ref char[] buf,
                           uint       bufLength,
                           out uint   errCode,
                           ref byte[] errBuf,
                           uint       maxErrs)
    {
      return cfsDirEnum(cfCid, path, buf, bufLength, out errCode, errBuf, maxErrs);
    }


    public bool CfsFileGet(IntPtr                    cfCid,
                           byte[]                    remotePath,
                           byte[]                    localPath,
                           uint                      timeout,
                           ref TmNativeDefs.FileTime fileTime,
                           out uint                  errCode,
                           ref byte[]                errBuf,
                           uint                      maxErrs)
    {
      return cfsFileGet(cfCid, remotePath, localPath, timeout, ref fileTime, out errCode, errBuf, maxErrs);
    }


    public bool CfsFileGetPropreties(IntPtr                             cfCid,
                                     byte[]                             fileName,
                                     ref TmNativeDefs.CfsFileProperties pProps,
                                     out uint                           errCode,
                                     ref byte[]                         errBuf,
                                     uint                               maxErrs)
    {
      return cfsFileGetPropreties(cfCid, fileName, ref pProps, out errCode, errBuf, maxErrs);
    }


    public bool CfsFilePut(IntPtr     connId,
                           byte[]     remoteFileName,
                           byte[]     localFileName,
                           uint       timeout,
                           out uint   errCode,
                           ref byte[] errBuf,
                           uint       maxErrs)
    {
      return cfsFilePut(connId, remoteFileName, localFileName, timeout, out errCode, errBuf, maxErrs);
    }


    public bool CfsFileDelete(IntPtr     connId,
                              byte[]     remoteFileName,
                              out uint   errCode,
                              ref byte[] errBuf,
                              uint       maxErrs)
    {
      return cfsFileDelete(connId, remoteFileName, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsConnect(byte[]     serverName,
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

    public IntPtr CfsEditGrabCid(IntPtr     connId,
                                 Boolean    bGrab,
                                 byte[]     fileName,
                                 byte[]     userName,
                                 out uint   errCode,
                                 ref byte[] errBuf,
                                 uint       maxErrs)
    {
      return cfsEditGrabCid(connId, bGrab, fileName, userName, out errCode, errBuf, maxErrs);
    }

    public IntPtr CfsConfFileOpenCid(IntPtr                    connId,
                                     byte[]                    serverName,
                                     byte[]                    fileName,
                                     uint                      timeout,
                                     ref TmNativeDefs.FileTime fileTime,
                                     out uint                  errCode,
                                     ref byte[]                errBuf,
                                     uint                      maxErrs)
    {
      return cfsConfFileOpenCid(connId, serverName, fileName, timeout, ref fileTime, out errCode, errBuf, maxErrs);
    }


    public bool CfsConfFileSaveAs(IntPtr                    treeHandle,
                                  byte[]                    serverName,
                                  byte[]                    remoteFileName,
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
                                      byte[]                       serverId,
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
                                    byte[]                     userId,
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
                                byte[]     path,
                                byte[]     section,
                                byte[]     key,
                                byte[]     def,
                                ref byte[] value,
                                out uint   pcbValue,
                                out uint   errCode,
                                ref byte[] errBuf,
                                uint       maxErrs)
    {
      return cfsGetIniString(connId, path, section, key, def, value, out pcbValue, out errCode, errBuf, maxErrs);
    }


    public bool CfsSetIniString(IntPtr     connId,
                                byte[]     path,
                                byte[]     section,
                                byte[]     key,
                                byte[]     value,
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
                                  byte[]     username,
                                  byte[]     password,
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


    public void DPrintF(byte[] message)
    {
      d_printf(new byte[3] { 37, 115, 0 }, message);
    }


    public void MPrintF(byte[] message)
    {
      m_printf(new byte[3] { 37, 115, 0 }, message);
    }


    public void EPrintF(byte[] message)
    {
      e_printf(new byte[3] { 37, 115, 0 }, message);
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
                                   byte[]     fileName,
                                   out UInt32 errCode,
                                   ref byte[] errBuf,
                                   UInt32     maxErrs)
    {
      return cfsNodeFileSave(treeHandle, fileName, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsNodeFileLoad(byte[]     fileName,
                                  out UInt32 errCode,
                                  ref byte[] errBuf,
                                  UInt32     maxErrs)
    {
      return cfsNodeFileLoad(fileName, out errCode, errBuf, maxErrs);
    }


    public IntPtr CfsConfFileOpen(byte[]                    serverName,
                                  byte[]                    fileName,
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


    public IntPtr CfsIfpcGetBin(IntPtr     connId,
                                byte[]     uName,
                                byte[]     oName,
                                byte[]     binName,
                                out uint   binLength,
                                out uint   errCode,
                                ref byte[] errBuf,
                                uint       maxErrs)
    {
      return cfsIfpcGetBin(connId, uName, oName, binName, out binLength, out errCode, errBuf, maxErrs);
    }


    public bool CfsIfpcSetBin(IntPtr     connId,
                              byte[]     uName,
                              byte[]     oName,
                              byte[]     binName,
                              byte[]     buf,
                              uint       bufLength,
                              out uint   errCode,
                              ref byte[] errBuf,
                              uint       maxErrs)
    {
      return cfsIfpcSetBin(connId, uName, oName, binName, buf, bufLength, out errCode, errBuf, maxErrs);
    }

    public IntPtr CfsGetAccessDescriptor(byte[] ini,
                                         byte[] section)
    {
      return cfsGetAccessDescriptor(ini, section);
    }

    public IntPtr CfsGetExtendedUserRightsDescriptor(byte[] ini,
                                                     byte[] section,
                                                     uint   fCheck)
    {
      return cfsGetExtendedUserRightsDescriptor(ini, section, fCheck);
    }

    public IntPtr СfsIfpcEnumUsers(IntPtr     connId,
                                   out uint   errCode,
                                   ref byte[] errBuf,
                                   uint       maxErrs)
    {
      return cfsIfpcEnumUsers(connId, out errCode, errBuf, maxErrs);
    }

    public IntPtr СfsIfpcEnumOSUsers(IntPtr     connId,
                                     out uint   errCode,
                                     ref byte[] errBuf,
                                     uint       maxErrs)
    {
      return cfsIfpcEnumOSUsers(connId, out errCode, errBuf, maxErrs);
    }

    public Boolean СfsIfpcDeleteUser(IntPtr     connId,
                                     byte[]     username,
                                     out uint   errCode,
                                     ref byte[] errBuf,
                                     uint       maxErrs)
    {
      return cfsIfpcDeleteUser(connId, username, out errCode, errBuf, maxErrs);
    }

    public uint СfsIfpcGetAccess(IntPtr     connId,
                                 byte[]     uName,
                                 byte[]     oName,
                                 out uint   errCode,
                                 ref byte[] errBuf,
                                 uint       maxErrs)
    {
      return cfsIfpcGetAccess(connId, uName, oName, out errCode, errBuf, maxErrs);
    }

    public Boolean СfsIfpcSetAccess(IntPtr     connId,
                                    byte[]     uName,
                                    byte[]     oName,
                                    uint       accessMask,
                                    out uint   errCode,
                                    ref byte[] errBuf,
                                    uint       maxErrs)
    {
      return cfsIfpcSetAccess(connId, uName, oName, accessMask, out errCode, errBuf, maxErrs);
    }

    public Boolean CfsSaveMachineConfig(Boolean    fFull,
                                        byte[]     remoteMasterMachine,
                                        byte[]     fileName,
                                        ref byte[] errBuf,
                                        uint       maxErrs)
    {
      return cfsSaveMachineConfig(fFull, remoteMasterMachine, fileName, errBuf, maxErrs);
    }

    public Boolean CfsSaveMachineConfigEx(byte[]           remoteMasterMachine,
                                          byte[]           fileName,
                                          uint             dwScope,
                                          TmNativeCallback progFn,
                                          IntPtr           progParm,
                                          ref byte[]       errBuf,
                                          uint             maxErrs)
    {
      try
      {
        return cfsSaveMachineConfigEx(remoteMasterMachine, fileName, dwScope, progFn, progParm, errBuf, maxErrs);
      }
      catch (Exception ex)
      {
        var ex_message = TmNativeUtil.GetFixedBytesWithTrailingZero(ex.Message, (int)maxErrs - 1, "windows-1251");
        ex_message.CopyTo(errBuf, 0);
        return false;
      }
    }

    public Boolean CfsExternalBackupServer(IntPtr                  connId,
                                           byte[]                  dllName,
                                           byte[]                  servName,
                                           uint                    bflags,
                                           ref CfsServerBackupData pbd,
                                           out uint                errCode,
                                           ref byte[]              errBuf,
                                           uint                    maxErrs)
    {
      return cfsExternalBackupServer(connId, dllName, servName, bflags, ref pbd, out errCode, errBuf, maxErrs);
    }

    public Boolean CfsExternalRestoreServer(IntPtr     connId,
                                            byte[]     dllName,
                                            byte[]     servName,
                                            byte[]     filename,
                                            out UInt32 pbFlags,
                                            out uint   errCode,
                                            ref byte[] errBuf,
                                            uint       maxErrs)
    {
      return cfsExternalRestoreServer(connId, dllName, servName, filename, out pbFlags, out errCode, errBuf, maxErrs);
    }

    public Boolean CfsIfpcBackupSecurity(IntPtr     connId,
                                         byte[]     snp,
                                         byte[]     pwd,
                                         byte[]     filename,
                                         out UInt32 errCode,
                                         ref byte[] errBuf,
                                         UInt32     maxErrs)
    {
      return cfsIfpcBackupSecurity(connId, snp, pwd, filename, out errCode, errBuf, maxErrs);
    }

    public Boolean CfsIfpcRestoreSecurity(IntPtr     connId,
                                          byte[]     snp,
                                          byte[]     pwd,
                                          byte[]     filename,
                                          out UInt32 errCode,
                                          ref byte[] errBuf,
                                          UInt32     maxErrs)
    {
      return cfsIfpcRestoreSecurity(connId, snp, pwd, filename, out errCode, errBuf, maxErrs);
    }

    public IntPtr CfsMakeInprocCrd(byte[] machine,
                                   byte[] user,
                                   byte[] pwd)
    {
      return cfsMakeInprocCrd(machine, user, pwd);
    }

    public Boolean CfsPrepNewConfig(IntPtr     connId,
                                    byte[]     remoteFName,
                                    out UInt32 errCode,
                                    ref byte[] errBuf,
                                    UInt32     maxErrs)
    {
      return cfsPrepNewConfig(connId, remoteFName, out errCode, errBuf, maxErrs);
    }

    public Boolean CfsIfpcTestTmcalc(IntPtr     connId,
                                     byte[]     tmsName,
                                     byte[]     clcName,
                                     UInt32     testWay,
                                     UInt32     testFlags,
                                     out UInt64 pHandle,
                                     out UInt32 pPid,
                                     out UInt32 errCode,
                                     ref byte[] errBuf,
                                     UInt32     maxErrs)
    {
      return cfsIfpcTestTmcalc(connId,
                               tmsName,
                               clcName,
                               testWay,
                               testFlags,
                               out pHandle,
                               out pPid,
                               out errCode,
                               errBuf,
                               maxErrs);
    }

    public Boolean CfsIfpcStopTestTmcalc(IntPtr     connId,
                                         UInt64     handle,
                                         UInt32     pid,
                                         out UInt32 errCode,
                                         ref byte[] errBuf,
                                         UInt32     maxErrs)
    {
      return cfsIfpcStopTestTmcalc(connId, handle, pid, out errCode, errBuf, maxErrs);
    }

    public Boolean СfsPmonCheckProcess(IntPtr     connId,
                                       byte[]     processNameArgs,
                                       out UInt32 errCode,
                                       ref byte[] errBuf,
                                       UInt32     maxErrs)
    {
      return cfsPmonCheckProcess(connId, processNameArgs, out errCode, errBuf, maxErrs);
    }

    public Boolean CfsPmonStopProcess(IntPtr     connId,
                                      byte[]     processNameArgs,
                                      out UInt32 pNumFound,
                                      out UInt32 errCode,
                                      ref byte[] errBuf,
                                      UInt32     maxErrs)
    {
      return cfsPmonStopProcess(connId, processNameArgs, out pNumFound, out errCode, errBuf, maxErrs);
    }

    public Boolean CfsPmonRestartProcess(IntPtr     connId,
                                         byte[]     processNameArgs,
                                         out UInt32 errCode,
                                         ref byte[] errBuf,
                                         UInt32     maxErrs)
    {
      return cfsPmonRestartProcess(connId, processNameArgs, out errCode, errBuf, maxErrs);
    }

    public Boolean CfsSwapFnSrvRole(byte[]     serverName,
                                    Boolean    bPre,
                                    byte[]     fnsName,
                                    out UInt32 errCode,
                                    ref byte[] errBuf,
                                    UInt32     maxErrs)
    {
      return cfsSwapFnSrvRole(serverName, bPre, fnsName, out errCode, errBuf, maxErrs);
    }

    public bool CfsIsReserveWorking(IntPtr     connId,
                                    uint       ipAddress,
                                    ushort     ipBcPort,
                                    ushort     ipPort,
                                    uint       sType,
                                    out bool   working,
                                    ref byte[] sName, // 64 
                                    out uint   errCode,
                                    ref byte[] errBuf,
                                    uint       maxErrs)
    {
      return cfsIsReserveWorking(connId,
                                 ipAddress,
                                 ipBcPort,
                                 ipPort, sType,
                                 out working,
                                 sName,
                                 out errCode,
                                 errBuf, maxErrs);
    }


    public bool CfsIfpcSetAbkParms(IntPtr     connId,
                                   byte[]     pwd,
                                   out UInt32 errCode,
                                   ref byte[] errBuf,
                                   UInt32     maxErrs)
    {
      return cfsIfpcSetAbkParms(connId, pwd, out errCode, errBuf, maxErrs);
    }
  }
}