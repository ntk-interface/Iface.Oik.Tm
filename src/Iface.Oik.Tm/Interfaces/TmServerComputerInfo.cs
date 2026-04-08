using System;
using Iface.Oik.Tm.Native.Dto;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmServerComputerInfo
  {
    public string ComputerName { get; }

    public int VersionMajor { get; }
    public int VersionMinor { get; }

    public int WindowsNtVersionMajor { get; }
    public int WindowsNtVersionMinor { get; }
    public int WindowsNtBuildNumber  { get; }

    public long UptimeSeconds { get; }

    public string Version => $"{VersionMajor}.{VersionMinor}";

    public string WindowsNt => $"{WindowsNtVersionMajor}.{WindowsNtVersionMinor}.{WindowsNtBuildNumber}";

    public DateTime StartedAt => DateTime.Now.Subtract(TimeSpan.FromSeconds(UptimeSeconds));


    public TmServerComputerInfo(ComputerInfoDto dto)
    {
      ComputerName          = dto.ComputerName;
      VersionMajor          = dto.CfsVerMaj;
      VersionMinor          = dto.CfsVerMin;
      WindowsNtVersionMajor = dto.NtVerMaj;
      WindowsNtVersionMinor = dto.NtVerMin;
      WindowsNtBuildNumber  = dto.NtBuild;
      UptimeSeconds         = dto.Uptime;
    }
  }
}