using System;

namespace Iface.Oik.Tm.Interfaces
{
  public class TobBinding
  {
    public Guid           Id          { get; set; }
    public Tob            Tob         { get; set; }
    public TobBindingType BindingType { get; set; }
    public TmTag          TmTag       { get; set; }
    public Tob            ParentTob   { get; set; }
    public string         Name        { get; set; }
    public string         Value       { get; set; }
    public int            OrderIx     { get; set; }

    public string NameToDisplay => TmTag != null
      ? TmTag.TmAddr.ToString()
      : ParentTob != null
        ? ParentTob.NameOrDefault
        : Name;

    public bool IsOperLock        => BindingType == TobBindingType.TmOperLock;
    public bool IsGroundOperLock  => BindingType == TobBindingType.TmGroundOperLock;
    public bool IsEmergencyDelock => BindingType == TobBindingType.TmEmergencyDelock;

    public override string ToString() => NameToDisplay;
  }
}