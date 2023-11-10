using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmServerInfo
  {
    public string    CurrentServerName       { get; }
    public string    Description             { get; }
    public DateTime? StartTime               { get; }
    public DateTime? ConfigurationChangeTime { get; }
    public uint      HeapUsage               { get; }
    public uint      WorkingSetMax           { get; }
    public uint      WorkingSetMin           { get; }
    public uint      UsersCount              { get; }
    public uint      LogonCount              { get; }
    public uint?     UniqUsersCount          { get; }
    public uint?     TmValuesCount           { get; }
    public uint?     TotalTmValuesCount      { get; }
    public uint?     ExternalTmValuesCount   { get; }
    public uint?     HandleCount             { get; }
    public uint?     ReserveBufferSize       { get; }
    public uint?     ReserveBufferFill       { get; }
    public uint?     ReserveBufferMaxFill    { get; }
    public uint?     ReserveSentAsyncBytes   { get; }
    public uint?     ReserveSentSyncBytes    { get; }
    public uint      ReserveAsyncPackets     { get; }
    public uint      ReserveSeconds          { get; }
    public uint      ReserveAsyncXPercent    { get; }
    public uint      DtmxLastCommit          { get; }
    public uint      DtmxBufferFill          { get; }
    public uint      AnRW                    { get; }
    public uint      TobSetCount             { get; }

    public byte[] KeyId { get; }

    public string KeyIdString => KeyId == null ? null : BitConverter.ToString(KeyId).Replace("-", "");
    
    public TmServerInfo(string                   serverName,
                        TmNativeDefs.TServerInfo tServerInfo)
    {
      CurrentServerName       = serverName;
      Description             = tServerInfo.Description;
      StartTime               = DateUtil.GetDateTimeFromTimestampWithEpochCheck(tServerInfo.StartTime);
      ConfigurationChangeTime = DateUtil.GetDateTimeFromTimestampWithEpochCheck(tServerInfo.StartTime);
      HeapUsage               = GetKbFromNativeValue(tServerInfo.DwHeapUsage);
      WorkingSetMax           = GetKbFromNativeValue(tServerInfo.DwWSMax);
      WorkingSetMin           = GetKbFromNativeValue(tServerInfo.DwWSMin);
      UsersCount              = tServerInfo.UserCount;
      LogonCount              = tServerInfo.LogonCount;
      HandleCount             = tServerInfo.HandleCount;
      DtmxBufferFill          = tServerInfo.DtmxBufFill;
      DtmxLastCommit          = tServerInfo.DtmxLastCommit;
      AnRW                    = tServerInfo.AnRW;
      ReserveAsyncPackets     = tServerInfo.ReserveAsyncPackets;
      ReserveSeconds          = tServerInfo.ReserveSeconds;
      TobSetCount             = tServerInfo.TobSetCount;

      if (tServerInfo.KeyId[0] != 0)
      {
        KeyId = tServerInfo.KeyId;
      }

      var flags = (TmNativeDefs.ServerInfoPresenceFlags)tServerInfo.PresenceFlags;

      if (flags.HasFlag(TmNativeDefs.ServerInfoPresenceFlags.UniqUserCount))
      {
        UniqUsersCount = tServerInfo.UniqUserCount;
      }

      if (flags.HasFlag(TmNativeDefs.ServerInfoPresenceFlags.TmValueCount))
      {
        TmValuesCount = tServerInfo.TmValueCount;
      }

      if (flags.HasFlag(TmNativeDefs.ServerInfoPresenceFlags.TmTotValCnt))
      {
        TotalTmValuesCount = tServerInfo.TmTotValCnt;
      }

      if (flags.HasFlag(TmNativeDefs.ServerInfoPresenceFlags.ExtValCount))
      {
        ExternalTmValuesCount = tServerInfo.TmExtValueCnt;
      }

      if (flags.HasFlag(TmNativeDefs.ServerInfoPresenceFlags.ReserveBufferSize))
      {
        ReserveBufferSize = tServerInfo.ReserveBufSize;
      }

      if (flags.HasFlag(TmNativeDefs.ServerInfoPresenceFlags.ReserveBufFill))
      {
        ReserveBufferFill = tServerInfo.ReserveBufFill;
      }

      if (flags.HasFlag(TmNativeDefs.ServerInfoPresenceFlags.ReserveBufMaxFill))
      {
        ReserveBufferMaxFill = tServerInfo.ReserveBufMaxFill;
      }

      if (flags.HasFlag(TmNativeDefs.ServerInfoPresenceFlags.ReserveSentAsyncBytes))
      {
        ReserveSentAsyncBytes = tServerInfo.ReserveSentAsyncBytes;
      }

      if (flags.HasFlag(TmNativeDefs.ServerInfoPresenceFlags.ReserveSentSyncBytes))
      {
        ReserveSentSyncBytes = tServerInfo.ReserveSentSyncBytes;
      }
    }

    private static uint GetKbFromNativeValue(uint nativeValue)
    {
      return (nativeValue + 1023) / 1024;
    }
  }
}