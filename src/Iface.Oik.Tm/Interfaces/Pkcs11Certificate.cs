using System.Text.RegularExpressions;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Interfaces;

public partial class Pkcs11Certificate
{
  [GeneratedRegex(@"pin=(?<pin>\d+),bio=(?<bio>\d+)", RegexOptions.IgnoreCase)]
  private static partial Regex AttrRegex(); 
  
  
  public string                   Name    { get; }
  public Pkcs11CertificatePinAttr PinAttr { get; }
  public Pkcs11CertificateBioAttr BioAttr { get; }


  public Pkcs11Certificate(string nameString, string attrString)
  {
    Name = nameString;
    
    var match = AttrRegex().Match(attrString);
    if (!match.Success)
    {
      throw new TmNativeException($"Ошибка разбора атрибутов смарт-карты: {attrString}");
    }
    PinAttr = (Pkcs11CertificatePinAttr)int.Parse(match.Groups["pin"].Value);
    BioAttr = (Pkcs11CertificateBioAttr)int.Parse(match.Groups["bio"].Value);
  }
}


public enum Pkcs11CertificatePinAttr
{
  Disabled = 0,
  Enabled  = 1,
}


public enum Pkcs11CertificateBioAttr
{
  Disabled      = 0,
  Enabled       = 2,
  Locked        = 3,
  CloseToLocked = 4,
}