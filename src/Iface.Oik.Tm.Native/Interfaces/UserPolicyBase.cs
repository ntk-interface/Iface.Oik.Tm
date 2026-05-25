using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class UserPolicyBase
{
  [ReadOnly(true)] public bool Predefined { get; set; }

  [ReadOnly(true)] public bool PasswordSet { get; set; }

  [ReadOnly(true)] public int BadLogonCount { get; set; }

  public string UserTemplate { get; set; } = string.Empty;

  public bool IsBlocked { get; set; }

  public bool MustChangePassword { get; set; }

  public DateTime NotBefore { get; set; }

  public DateTime NotAfter { get; set; }

  public int BadLogonLimit { get; set; }

  [XmlArray] public string EnabledMacs { get; set; } = string.Empty;
  public string UserCategory { get; set; } = string.Empty;
}