namespace Iface.Oik.Tm.Interfaces
{
  public class TmLicenseKey
  {
    public LicenseKeyType Type { get; }
    public int            Port { get; set; }

    public TmLicenseKey(int nativeCom)
    {
      Type = GetLicenseKeyType(nativeCom / 256);

      if (Type == LicenseKeyType.Unknown)
      {
        Port = nativeCom % 256;
      }
      else
      {
        Port = (nativeCom + 1) % 256;
      }
    }

    public TmLicenseKey(LicenseKeyType type, int port)
    {
      Type = type;
      Port = port;
    }

    public string NativeCom()
    {
      var typeValue = (int) Type;

      return $"{typeValue * 256 + (typeValue != 0 && Port != 0 ? Port - 1 : Port)}";
    }
    
    private static LicenseKeyType GetLicenseKeyType(int typeValue)
    {
      switch (typeValue)
      {
        case 4:
          return LicenseKeyType.TypeFour;
        case 5:
          return LicenseKeyType.Software;
        case 6:
          return LicenseKeyType.UsbHidSsd;
        case 7:
          return LicenseKeyType.Network;
        default:
          return LicenseKeyType.Unknown;
      }
    }
  }
}