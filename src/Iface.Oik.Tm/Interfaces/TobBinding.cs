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

    public string NameToDisplay => TmTag != null ? TmTag.Name : Name;

    public bool IsOperLock => BindingType == TobBindingType.TmOperLock;
  }
}