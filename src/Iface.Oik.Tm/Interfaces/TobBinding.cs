namespace Iface.Oik.Tm.Interfaces
{
  public class TobBinding
  {
    public Tob            Tob         { get; set; }
    public TobBindingType BindingType { get; set; }
    public TmTag          TmTag       { get; set; }
    public Tob            ParentTob   { get; set; }
    public string         Name        { get; set; }
    public string         Value       { get; set; }

    public bool IsOperLock => BindingType == TobBindingType.TmOperLock;

    public bool IsTmStatus => TmTag is TmStatus;
    public bool IsTmAnalog => TmTag is TmAnalog;
  }
}