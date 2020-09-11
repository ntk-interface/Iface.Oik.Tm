using System;
using System.Collections.Generic;
using System.Globalization;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmInstallationInfo
  {
    public string    InstallationName      { get; }
    public int       VersionMajor          { get; }
    public int       VersionMinor          { get; }
    public DateTime? BuildDate             { get; }
    public DateTime? InstallationTime      { get; }
    public DateTime? UpdateLimitDate       { get; }
    public bool      IsIntact              { get; }
    public string    IntegrityCheckMessage { get; }


    public IReadOnlyCollection<TmInstallationFileInfo> InstalledFiles { get; }

    public string VersionString   => $"{VersionMajor}.{VersionMinor}";
    public string BuildDateString => BuildDate.HasValue ? $"{BuildDate.Value:dd.MM.yyyy}" : string.Empty;

    public string InstallationTimeString =>
      InstallationTime.HasValue ? $"{InstallationTime.Value:dd.MM.yyyy H:mm:ss}" : string.Empty;

    public string UpdateLimitDateString =>
      UpdateLimitDate.HasValue ? $"{UpdateLimitDate.Value:dd.MM.yyyy}" : string.Empty;

    public TmInstallationInfo(string                                      installationName,
                              string                                      version,
                              string                                      buildDate,
                              string                                      installationTime,
                              string                                      updateLimitDate,
                              bool                                        isIntact,
                              string                                      integrityCheckMessage,
                              IReadOnlyCollection<TmInstallationFileInfo> installedFiles)
    {
      InstallationName = installationName;

      var versionArray = version.Split('.');
      if (versionArray.Length == 2)
      {
        VersionMajor = Convert.ToInt32(versionArray[0]);
        VersionMinor = Convert.ToInt32(versionArray[1]);
      }

      BuildDate             = DateUtil.GetDateTime(buildDate);
      InstallationTime      = DateUtil.GetDateTimeFromTmString(installationTime);
      UpdateLimitDate       = DateUtil.GetDateTime(updateLimitDate);
      IsIntact              = isIntact;
      IntegrityCheckMessage = integrityCheckMessage;
      InstalledFiles        = installedFiles;
    }
  }

  public class TmInstallationFileInfo
  {
    private string _directory;

    public string    Name           { get; }
    public string    Description    { get; }
    public uint      Checksum       { get; }
    public uint      ActualChecksum { get; }
    public DateTime? Time           { get; }
    public DateTime? ActualTime     { get; }

    public string Directory            => _directory.IsNullOrEmpty() ? "." : _directory;
    public string ChecksumString       => Checksum       == 0 ? "???" : Checksum.ToString("X8");
    public string ActualChecksumString => ActualChecksum == 0 ? "???" : ActualChecksum.ToString("X8");

    public string TimeString =>
      Time.HasValue ? $"{Time.Value:dd.MM.yyyy H:mm:ss.fff}" : "???";

    public string ActualTimeString =>
      ActualTime.HasValue ? $"{ActualTime.Value:dd.MM.yyyy H:mm:ss.fff}" : "???";

    public TmInstallationFileInfo(string    name,
                                  string    description,
                                  string    directory,
                                  string    checksum,
                                  uint      actualChecksum,
                                  string    time,
                                  DateTime? actualTime)
    {
      Name           = name;
      Description    = EncodingUtil.Win1251ToUtf8(description);
      _directory     = directory;
      Checksum       = ParseChecksum(checksum);
      ActualChecksum = actualChecksum;
      Time           = DateUtil.GetDateTimeFromExtendedTmString(time);
      ActualTime     = actualTime;
    }

    private static uint ParseChecksum(string checksumString)
    {
      return uint.TryParse(checksumString, NumberStyles.HexNumber, null, out var result) ? result : 0;
    }
  }
}