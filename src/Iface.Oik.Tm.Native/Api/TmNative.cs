using System;
using System.IO;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative : ITmNative
  {
    #region Platform

    public string GetOikTaskExecutable(string origin)
    {
      DPrintF(origin);
      if (Path.GetExtension(origin) == ".dll")
      {
        var executableExtension = (PlatformUtil.IsWindows) ? ".exe" : "";
        var executable          = Path.ChangeExtension(origin, executableExtension);
        if (File.Exists(executable))
        {
          return executable;
        }
      }
      return origin;
    }


    public bool PlatformSetEvent(UInt32 hEvent)
    {
      return (PlatformUtil.IsWindows)
        ? SetEventWindows(hEvent)
        : SetEventLinux(hEvent);
    }


    public UInt32 PlatformWaitForSingleObject(UInt32 hHandle,
                                              UInt32 dwMilliseconds)
    {
      return (PlatformUtil.IsWindows)
        ? WaitForSingleObjectWindows(hHandle, dwMilliseconds)
        : WaitForSingleObjectLinux(hHandle, dwMilliseconds);
    }

    #endregion

    #region Cfs

    public bool CfsInitLibrary(string baseDir = null, string extArg = null)
    {
      return cfsInitLibrary(baseDir, extArg);
    }


    public void CfsSetUser(string user,
                           string password)
    {
      cfsSetUser(user, password);
    }


    public UInt32 CfsGetExtendedUserData(UInt32 cfCid,
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


    public bool CfsGetComputerInfo(UInt32                         cfCid,
                                   ref TmNativeDefs.ComputerInfoS cis,
                                   out UInt32                     errCode,
                                   ref StringBuilder              errString,
                                   UInt32                         maxErrs)
    {
      return cfsGetComputerInfo(cfCid, ref cis, out errCode, errString, maxErrs);
    }


    public bool CfsDirEnum(UInt32            cfCid,
                           string            path,
                           ref char[]        buf,
                           UInt32            bufLength,
                           out UInt32        errCode,
                           ref StringBuilder errString,
                           UInt32            maxErrs)
    {
      return cfsDirEnum(cfCid, path, buf, bufLength, out errCode, errString, maxErrs);
    }


    public bool CfsFileGet(UInt32            cfCid,
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

    
    public IntPtr CfsConnect(string            serverName,
                             out UInt32        errCode,
                             ref StringBuilder errString,
                             UInt32            maxErrs)
    {
      return cfsConnect(serverName, out errCode, errString, maxErrs);
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

    #endregion

    #region Tmc

    public Int32 TmcConnect(string           server,
                            string           pipe,
                            string           user,
                            TmNativeCallback callback,
                            IntPtr           callbackParameter)
    {
      return tmcConnect(server, pipe, user, callback, callbackParameter);
    }


    public void TmcDisconnect(Int32 cid)
    {
      tmcDisconnect(cid);
    }


    public void TmcFreeMemory(IntPtr memory)
    {
      tmcFreeMemory(memory);
    }


    public UInt32 TmcGetLastError()
    {
      return tmcGetLastError();
    }


    public Int32 TmcGetLastErrorText(Int32  cid,
                                     IntPtr buf)
    {
      return tmcGetLastErrorText(cid, buf);
    }


    public UInt32 TmcSetDgrmFlags(Int32  cid,
                                  UInt32 flags)
    {
      return tmcSetDgrmFlags(cid, flags);
    }


    public UInt32 TmcClrDgrmFlags(Int32  cid,
                                  UInt32 flags)
    {
      return tmcClrDgrmFlags(cid, flags);
    }


    public Int16 TmcSetFeedbackItems(Int32  cid,
                                     UInt16 type,
                                     Int16  ch,
                                     Int16  rtu,
                                     Int16  point,
                                     Byte   count,
                                     Byte   fbType,
                                     UInt32 id)
    {
      return tmcSetFeedbackItems(cid, type, ch, rtu, point, count, fbType, id);
    }


    public Int16 TmcClrFeedback(Int32 cid)
    {
      return tmcClrFeedback(cid);
    }


    public UInt32 TmcGetCfsHandle(Int32 cid)
    {
      return tmcGetCfsHandle(cid);
    }


    public UInt32 TmcReconnectCount(Int32 cid)
    {
      return tmcReconnectCount(cid);
    }


    public UInt32 TmcConnState(Int32 cid)
    {
      return tmcConnState(cid);
    }


    public Int16 TmcSystemTime(Int32             cid,
                               ref StringBuilder time,
                               IntPtr            tmStruct)
    {
      return tmcSystemTime(cid, time, tmStruct);
    }


    public Int16 TmcStatus(Int32 cid,
                           Int16 ch,
                           Int16 rtu,
                           Int16 point)
    {
      return tmcStatus(cid, ch, rtu, point);
    }


    public Single TmcAnalog(Int32  cid,
                            Int16  ch,
                            Int16  rtu,
                            Int16  point,
                            string dateTime,
                            Int16  retroNum)
    {
      return tmcAnalog(cid, ch, rtu, point, dateTime, retroNum);
    }


    public IntPtr TmcTmValuesByListEx(Int32                 cid,
                                      UInt16                tmType,
                                      Byte                  qFlags,
                                      UInt32                count,
                                      TmNativeDefs.TAdrTm[] addr)
    {
      return tmcTMValuesByListEx(cid, tmType, qFlags, count, addr);
    }


    public IntPtr TmcGetValuesByFlagMask(Int32      cid,
                                         UInt16     tmType,
                                         UInt32     tmFlags,
                                         Byte       qFlags,
                                         out UInt32 pCount)
    {
      return tmcGetValuesByFlagMask(cid, tmType, tmFlags, qFlags, out pCount);
    }


    public Int16 TmcRegEvent(Int32               cid,
                             TmNativeDefs.TEvent tmEvent)
    {
      return tmcRegEvent(cid, ref tmEvent);
    }


    public bool TmcEvlogPutStrBin(Int32  cid,
                                  UInt32 unixTime,
                                  Byte   unixHund,
                                  Byte   importance,
                                  UInt32 sourceTag,
                                  string str,
                                  Byte[] bin,
                                  UInt32 cbBin)
    {
      return tmcEvlogPutStrBin(cid, unixTime, unixHund, importance, sourceTag, str, bin, cbBin);
    }


    public IntPtr TmcEventLogByElix(Int32                     cid,
                                    ref TmNativeDefs.TTMSElix elix,
                                    UInt16                    eventMask,
                                    UInt32                    startUnixTime,
                                    UInt32                    endUnixTime)
    {
      return tmcEventLogByElix(cid, ref elix, eventMask, startUnixTime, endUnixTime);
    }


    public bool TmcGetCurrentElix(Int32                     cid,
                                  ref TmNativeDefs.TTMSElix elix)
    {
      return tmcGetCurrentElix(cid, ref elix);
    }


    public Int16 TmcSetTimedValues(Int32                              cid,
                                   UInt32                             count,
                                   TmNativeDefs.TTimedValueAndFlags[] values)
    {
      return tmcSetTimedValues(cid, count, values);
    }


    public Int32 TmcExecuteControlScript(Int32 cid,
                                         Int16 ch,
                                         Int16 rtu,
                                         Int16 point,
                                         Int16 cmd)
    {
      return tmcExecuteControlScript(cid, ch, rtu, point, cmd);
    }


    public bool TmcOverrideControlScript(Int32 cid,
                                         bool  fOverride)
    {
      return tmcOverrideControlScript(cid, fOverride);
    }


    public Int16 TmcControlByStatus(Int32 cid,
                                    Int16 ch,
                                    Int16 rtu,
                                    Int16 point,
                                    Int16 cmd)
    {
      return tmcControlByStatus(cid, ch, rtu, point, cmd);
    }


    public Int16 TmcControlCmdResult(Int32  cid,
                                     UInt32 id,
                                     bool   fSuccess)
    {
      return tmcControlCmdResult(cid, id, fSuccess);
    }


    public Int16 TmcRegulationByAnalog(Int32  cid,
                                       Int16  ch,
                                       Int16  rtu,
                                       Int16  point,
                                       Byte   regType,
                                       IntPtr regData)
    {
      return tmcRegulationByAnalog(cid, ch, rtu, point, regType, regData);
    }


    public Int16 TmcSetStatusNormal(Int32  cid,
                                    Int16  ch,
                                    Int16  rtu,
                                    Int16  point,
                                    UInt16 nValue)
    {
      return tmcSetStatusNormal(cid, ch, rtu, point, nValue);
    }


    public Int16 TmcGetStatusNormal(Int32      cid,
                                    Int16      ch,
                                    Int16      rtu,
                                    Int16      point,
                                    out UInt16 nValue)
    {
      return tmcGetStatusNormal(cid, ch, rtu, point, out nValue);
    }


    public Int16 TmcDriverCall(Int32  cid,
                               UInt32 addr,
                               Int16  qCode,
                               Int16  command)
    {
      return tmcDriverCall(cid, addr, qCode, command);
    }


    public bool TmcEventLogAckRecords(Int32                     cid,
                                      ref TmNativeDefs.TTMSElix elix,
                                      UInt32                    count)
    {
      return tmcEventLogAckRecords(cid, ref elix, count);
    }


    public void TmcStatusByList(Int32                       cid,
                                UInt16                      count,
                                TmNativeDefs.TAdrTm[]       addr,
                                TmNativeDefs.TStatusPoint[] statuses)
    {
      tmcStatusByList(cid, count, addr, statuses);
    }


    public Int16 TmcGetObjectProperties(Int32             cid,
                                        UInt16            objectType,
                                        Int16             ch,
                                        Int16             rtu,
                                        Int16             point,
                                        ref StringBuilder buf,
                                        Int32             bufSize)
    {
      return tmcGetObjectProperties(cid, objectType, ch, rtu, point, buf, bufSize);
    }


    public IntPtr TmcGetStatusClassData(Int32                 cid,
                                        UInt32                count,
                                        TmNativeDefs.TAdrTm[] statuses)
    {
      return tmcGetStatusClassData(cid, count, statuses);
    }


    public IntPtr TmcGetAnalogClassData(Int32                 cid,
                                        UInt32                count,
                                        TmNativeDefs.TAdrTm[] analogs)
    {
      return tmcGetAnalogClassData(cid, count, analogs);
    }


    public void TmcTakeRetroTit(Int32                                cid,
                                Int16                                ch,
                                Int16                                rtu,
                                Int16                                point,
                                UInt32                               unixTime,
                                UInt16                               retroNum,
                                UInt16                               count,
                                UInt16                               step,
                                ref TmNativeDefs.TAnalogPointShort[] analogs)
    {
      tmcTakeRetroTit(cid, ch, rtu, point, unixTime, retroNum, count, step, analogs);
    }


    public Int16 TmcSetStatus(Int32  cid,
                              Int16  ch,
                              Int16  rtu,
                              Int16  point,
                              Byte   value,
                              string dateTime,
                              Int16  hund)
    {
      return tmcSetStatus(cid, ch, rtu, point, value, dateTime, hund);
    }


    public Int16 TmcSetAnalog(Int32  cid,
                              Int16  ch,
                              Int16  rtu,
                              Int16  point,
                              Single value,
                              string dateTime)
    {
      return tmcSetAnalog(cid, ch, rtu, point, value, dateTime);
    }


    public Int16 TmcSetAnalogFlags(Int32 cid,
                                   Int16 ch, Int16 rtu, Int16 point,
                                   Int16 flags)
    {
      return tmcSetAnalogFlags(cid, ch, rtu, point, flags);
    }


    public Int16 TmcPeekAlarm(Int32                   cid,
                              Int16                   ch,
                              Int16                   rtu,
                              Int16                   point,
                              Int16                   alarmId,
                              ref TmNativeDefs.TAlarm alarm)
    {
      return tmcPeekAlarm(cid, ch, rtu, point, alarmId, ref alarm);
    }


    public Int16 TmcPokeAlarm(Int32                   cid,
                              Int16                   ch,
                              Int16                   rtu,
                              Int16                   point,
                              Int16                   alarmId,
                              ref TmNativeDefs.TAlarm alarm)
    {
      return tmcPokeAlarm(cid, ch, rtu, point, alarmId, ref alarm);
    }


    public IntPtr TmcAanReadArchive(Int32            cid,
                                    UInt32           tmAddr,
                                    UInt32           startUnixTime,
                                    UInt32           endUnixTime,
                                    UInt32           step,
                                    UInt32           flags,
                                    out UInt32       count,
                                    TmNativeCallback progress,
                                    IntPtr           progressParam)
    {
      return tmcAanReadArchive(cid,       tmAddr,   startUnixTime, endUnixTime, step, flags,
                               out count, progress, progressParam);
    }


    public UInt32 String2Utime_(string dateTime)
    {
      return String2Utime(dateTime);
    }


    public Byte TmcEnumObjects(Int32        cid,
                               UInt16       objectType,
                               Byte         count,
                               ref UInt16[] buf,
                               Int16        ch,
                               Int16        rtu,
                               Int16        point)
    {
      return tmcEnumObjects(cid, objectType, count, buf, ch, rtu, point);
    }


    public Int16 TmcGetObjectName(Int32             cid,
                                  UInt16            objectType,
                                  Int16             ch,
                                  Int16             rtu,
                                  Int16             point,
                                  ref StringBuilder buf,
                                  Int32             bufSize)
    {
      return tmcGetObjectName(cid, objectType, ch, rtu, point, buf, bufSize);
    }


    public IntPtr TmcTechObjReadValues(Int32                   cid,
                                       TmNativeDefs.TTechObj[] objects,
                                       UInt32                  count)
    {
      return tmcTechObjReadValues(cid, objects, count);
    }


    public bool TmcTechObjBeginUpdate(Int32 cid)
    {
      return tmcTechObjBeginUpdate(cid);
    }


    public bool TmcTechObjEndUpdate(Int32 cid)
    {
      return tmcTechObjEndUpdate(cid);
    }


    public Int32 TmcTechObjWriteValues(Int32                        cid,
                                       TmNativeDefs.TTechObjProps[] props,
                                       UInt32                       count)
    {
      return tmcTechObjWriteValues(cid, props, count);
    }

    #endregion

    #region Rbc

    public UInt16 RbcIpgStartRedirector(Int32  cid,
                                        UInt16 portIdx)
    {
      return rbcIpgStartRedirector(cid, portIdx);
    }


    public bool RbcIpgStopRedirector(Int32  cid,
                                     UInt16 portIdx)
    {
      return rbcIpgStopRedirector(cid, portIdx);
    }

    #endregion
    
    #region Cftree

    public IntPtr CftNodeEnum(IntPtr id,
                              Int32  count)
    {
      return cftNodeEnum(id, count);
    }

    public void CftNodeFreeTree(IntPtr id)
    {
      cftNodeFreeTree(id);
    }

    public IntPtr CftNodeGetName(IntPtr            id,
                                 ref StringBuilder buf,
                                 UInt32            count)
    {
      return cftNodeGetName(id, buf, count);
    }

    public IntPtr CftNPropEnum(IntPtr            id,
                               Int32             idx,
                               ref StringBuilder buf,
                               uint              count)
    {
      return cftNPropEnum(id, idx, buf, count);
    }

    public IntPtr CftNPropGetText(IntPtr            id,
                                  string            name,
                                  ref StringBuilder buf,
                                  uint              count)
    {
      return cftNPropGetText(id, name, buf, count);
    }

    public IntPtr CftNodeNewTree()
    {
      return cftNodeNewTree();
    }

    public IntPtr CftNodeInsertAfter(IntPtr id,
                                     string nodeTag)
    {
      return cftNodeInsertAfter(id, nodeTag);
    }

    public IntPtr CftNodeInsertDown(IntPtr id,
                                    string nodeTag)
    {
      return cftNodeInsertDown(id, nodeTag);
    }

    public bool CftNPropSet(IntPtr id,
                            string propName,
                            string propText)
    {
      return cftNPropSet(id, propName, propText);
    }

    #endregion
  }
}