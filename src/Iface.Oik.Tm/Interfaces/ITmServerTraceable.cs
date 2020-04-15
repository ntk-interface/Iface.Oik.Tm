namespace Iface.Oik.Tm.Interfaces
{
  public interface ITmServerTraceable
  {
    string Name      { get; }
    string Comment   { get; }
    uint   ProcessId { get; }
    uint   ThreadId  { get; }

    string DisplayName { get; }
  }
}