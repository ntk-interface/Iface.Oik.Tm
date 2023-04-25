using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmLicenseInfo : TmNotifyPropertyChanged
  {
    private readonly Regex _dataStringRegex = new
      Regex(@".*=.*", RegexOptions.Compiled);

    public string                ActiveKeyId      { get; } // .1
    public LicenseDataItem       Error            { get; } = new LicenseDataItem(); // .2
    public LicenseDataItem       Addition1        { get; } = new LicenseDataItem(); // .3
    public LicenseDataItem       Addition2        { get; } = new LicenseDataItem(); // .4
    public TmLicenseKey          ActiveKey        { get; }
    public List<LicenseDataItem> LicenseDataItems { get; } = new List<LicenseDataItem>();
    
    public string ActiveKeyIdString => $"Активный ключ {(ActiveKeyId.IsNullOrEmpty() ? "???" : ActiveKeyId)}";

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
          case ".1":
            ActiveKeyId = value;
            break;
          case ".2":
            Error.Name  = name;
            Error.Value = value;
            break;
          case ".3":
            Addition1.Name  = name;
            Addition1.Value = value;
            break;
          case ".4":
            Addition2.Name  = name;
            Addition2.Value = value;
            break;
          //case "@Company":
          //  break;
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
    
  }

  public class LicenseDataItem
  {
    public string Name  { get; set; }
    public string Value { get; set; }

    public bool IsEmpty => Name.IsNullOrEmpty() || Value.IsNullOrEmpty();
  }
}