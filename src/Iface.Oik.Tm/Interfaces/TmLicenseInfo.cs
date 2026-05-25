using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces;

public partial class TmLicenseInfo : TmNotifyPropertyChanged
{
  private readonly Regex _dataStringRegex = DataStringRegex();
  
  public TmLicenseKey            ActiveKey        { get; }
  public List<ILicenseDataItem>  LicenseDataItems { get; } = new();

  public TmLicenseInfo(TmLicenseKey                key,
                       IDictionary<string, string> licenseKeyDataDictionary)
  {
    ActiveKey = key;

    foreach (var item in licenseKeyDataDictionary)
    {
      if (!_dataStringRegex.IsMatch(item.Value)) continue;

      var dataItem = item.Value.Split('=');
      var name     = $"{dataItem.First()}:";
      var value    = dataItem.Last();
      switch (item.Key)
      {
        case "22":
          LicenseDataItems.Add(new LicenseDataItem<LicensePlatforms>
          {
            ServerName = name,
            Value = Enum.TryParse<LicensePlatforms>(value, out var platforms)
                      ? platforms
                      : 0,
            Code       = item.Key,
            Deprecated = false
          });
          break;
        case "25":
          LicenseDataItems.Add(new LicenseDataItem<LicenseSecurityLevel>
          {
            ServerName = name,
            Value = Enum.TryParse<LicenseSecurityLevel>(value, out var level)
                      ? level
                      : LicenseSecurityLevel.Unknown,
            Code       = item.Key,
            Deprecated = false
          });
          break;
        case "27":
          if (!int.TryParse(value, out var rawVersion))
          {
            break;
          }

          LicenseDataItems.Add(new LicenseDataMaxVersionItem
          {
            ServerName = name,
            Major      = rawVersion / 100,
            Minor      = rawVersion % 100,
            Code       = item.Key
          });

          break;
        case "6":
        case "18":
        case "23":
        case "32":
          LicenseDataItems.Add(new LicenseDataItem<bool>
          {
            ServerName = name,
            Value = value switch
                    {
                      "да" => true,
                      _    => false
                    },
            Code       = item.Key,
            Deprecated = true
          });
          break;
        case var _ when value is "да" or "нет":
          LicenseDataItems.Add(new LicenseDataItem<bool>
          {
            ServerName = name,
            Value = value switch
                    {
                      "да" => true,
                      _    => false
                    },
            Code       = item.Key,
            Deprecated = false
          });
          break;
        default:
          LicenseDataItems.Add(new LicenseDataItem<string>
          {
            ServerName = name,
            Value      = value,
            Code       = item.Key
          });
          break;
      }
    }
  }

  [GeneratedRegex(@".*=.*", RegexOptions.Compiled)]
  private static partial Regex DataStringRegex();
}

public interface ILicenseDataItem
{
  public string ServerName { get; init; }
  public string Code       { get; }
  public bool   Deprecated { get; }
}

public class LicenseDataItem<T> : ILicenseDataItem
{
  public string ServerName { get; init; }
  public string Code       { get; set; }
  public T      Value      { get; set; }
  public bool   Deprecated { get; set; }

  public bool IsEmpty => ServerName.IsNullOrEmpty() || Value is null;
}

public class LicenseDataMaxVersionItem : ILicenseDataItem
{
  public string ServerName { get; init; }
  public int    Major      { get; init; }
  public int    Minor      { get; init; }

  public string Code       { get; init; }
  public bool   Deprecated => true;

  public string ShortString => $"{Major}.X";
  public string FullString  => $"{Major}.{Minor}";
}

[Flags]
public enum LicensePlatforms
{
  Win = 0x00010000,
  Lin = 0x00020000,

  Intel = 0x00000100,
  Arm   = 0x00000200,

  X32 = 0x00000001,
  X64 = 0x00000002,
}

public enum LicenseSecurityLevel
{
  Off    = 0,
  Base   = 1,
  Level1 = 3,
  Level2 = 7,

  Unknown = -1
}