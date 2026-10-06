using System;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Interfaces;

public class MonitoringData : MonitoringDataBase
{
  public Dictionary<string, string> TagsDictionary { get; private set; } = new();

  public string    SoftwareVersion    { get; private set; } = string.Empty;
  public string    BuildDate          { get; private set; } = string.Empty;
  public bool?     ServerIsRunning    { get; private set; }
  public string    LicenseId          { get; private set; } = string.Empty;
  public DateTime? LocalTime          { get; private set; }
  public DateTime? ServerTime         { get; private set; }
  public DateTime? ElixTime           { get; private set; }
  public TimeSpan? SysUpTime          { get; private set; }
  public TimeSpan? TmsUpTime          { get; private set; }
  public decimal?  MemoryTotalMb      { get; private set; }
  public decimal?  MemoryUsedMb       { get; private set; }
  public decimal?  MemoryServerUsedMb { get; private set; }
  public int?      LogErrorsCount     { get; private set; }
  public decimal?  DiskSpaceTotalMb   { get; private set; }
  public decimal?  DiskSpaceFreeMb    { get; private set; }
  public int?      DiskHealthPercents { get; private set; }

  public int?     TmsUsersMax         { get; private set; }
  public int?     TmsUsersUsed        { get; private set; }
  public decimal? CpuLoadPercent      { get; private set; }
  public decimal? StatusPerSecond     { get; private set; }
  public decimal? AnalogPerSecond     { get; private set; }
  public decimal? TeleparamsPerSecond { get; private set; }


  protected override void Initialize(InitializeDto dto)
  {
    TagsDictionary      = dto.TagsDictionaty;
    SoftwareVersion     = dto.SoftwareVersion;
    BuildDate           = dto.BuildDate;
    ServerIsRunning     = dto.ServerIsRunning;
    LicenseId           = dto.LicenseId;
    LocalTime           = dto.LocalTime;
    ServerTime          = dto.ServerTime;
    ElixTime            = dto.ElixTime;
    SysUpTime           = dto.SysUpTime;
    TmsUpTime           = dto.TmsUpTime;
    MemoryTotalMb       = dto.MemoryTotalMb;
    MemoryUsedMb        = dto.MemoryUsedMb;
    MemoryServerUsedMb  = dto.MemoryServerUsedMb;
    LogErrorsCount      = dto.LogErrorsCount;
    DiskSpaceTotalMb    = dto.DiskSpaceTotalMb;
    DiskSpaceFreeMb     = dto.DiskSpaceFreeMb;
    DiskHealthPercents  = dto.DiskHealthPercents;
    TmsUsersMax         = dto.TmsUsersMax;
    TmsUsersUsed        = dto.TmsUsersUsed;
    CpuLoadPercent      = dto.CpuLoadPercent;
    StatusPerSecond     = dto.StatusPerSecond;
    AnalogPerSecond     = dto.AnalogPerSecond;
    TeleparamsPerSecond = dto.TeleparamsPerSecond;
  }
}