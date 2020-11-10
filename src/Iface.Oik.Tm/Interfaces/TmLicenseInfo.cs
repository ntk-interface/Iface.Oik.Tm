using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmLicenseInfo : TmNotifyPropertyChanged
  {
    private string _activeKeyId;

    private readonly Regex _dataStringRegex = new
      Regex(@".*=.*", RegexOptions.Compiled);

    public LicenseDataItem Error     { get; } = new LicenseDataItem();
    public LicenseDataItem ErrorAdd1 { get; } = new LicenseDataItem();
    public LicenseDataItem ErrorAdd2 { get; } = new LicenseDataItem();
    public TmLicenseKey    ActiveKey { get; }


    public List<LicenseKeyType>  AvailableKeysTypes { get; } = new List<LicenseKeyType>();
    public List<LicenseDataItem> LicenseDataItems   { get; } = new List<LicenseDataItem>();


    public string ActiveKeyId => $"Активный ключ {(_activeKeyId.IsNullOrEmpty() ? "???" : _activeKeyId)}";

    public TmLicenseInfo(TmLicenseKey                key,
                         IReadOnlyCollection<string> availableKeysStrings,
                         IDictionary<string, string> licenseKeyDataDictionary)
    {
      ActiveKey = key;

      AvailableKeysTypes.AddRange(availableKeysStrings.Select(x =>
                                                              {
                                                                var keyParts = x.Split(new[] {". "},
                                                                  StringSplitOptions.None);
                                                                return GetLicenseKeyType(keyParts.First());
                                                              }));

      foreach (var item in licenseKeyDataDictionary)
      {
        if (!_dataStringRegex.IsMatch(item.Value)) continue;

        var dataItem = item.Value.Split('=');
        var name     = $"{dataItem.First()}:";
        var value    = dataItem.Last();
        switch (item.Key)
        {
          case ".1":
            _activeKeyId = value;
            break;
          case ".2":
            Error.Name  = name;
            Error.Value = value;
            break;
          case ".3":
            ErrorAdd1.Name  = name;
            ErrorAdd1.Value = value;
            break;
          case ".4":
            ErrorAdd2.Name  = name;
            ErrorAdd2.Value = value;
            break;
          case "@Company":
            break;
          default:
            LicenseDataItems.Add(new LicenseDataItem
            {
              Name  = name,
              Value = value
            });
            break;
        }
      }
    }

    private static LicenseKeyType GetLicenseKeyType(string typeString)
    {
      switch (typeString)
      {
        case "4":
          return LicenseKeyType.TypeFour;
        case "5":
          return LicenseKeyType.Software;
        case "6":
          return LicenseKeyType.UsbHidSsd;
        case "7":
          return LicenseKeyType.Network;
        default:
          return LicenseKeyType.Unknown;
      }
    }
  }

  public class LicenseDataItem
  {
    public string Name  { get; set; }
    public string Value { get; set; }

    public bool IsEmpty => Name.IsNullOrEmpty() || Value.IsNullOrEmpty();
  }
}