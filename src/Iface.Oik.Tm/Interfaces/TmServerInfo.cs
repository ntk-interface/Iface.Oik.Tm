using System;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmServerInfo
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


    public TmServerInfo(string computerName,
                        int    versionMajor,
                        int    versionMinor,
                        int    windowsNtVersionMajor,
                        int    windowsNtVersionMinor,
                        int    windowsNtBuildNumber,
                        long   uptimeSeconds)
    {
      ComputerName          = computerName;
      VersionMajor          = versionMajor;
      VersionMinor          = versionMinor;
      WindowsNtVersionMajor = windowsNtVersionMajor;
      WindowsNtVersionMinor = windowsNtVersionMinor;
      WindowsNtBuildNumber  = windowsNtBuildNumber;
      UptimeSeconds         = uptimeSeconds;
    }
  }
}