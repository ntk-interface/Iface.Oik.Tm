namespace Iface.Oik.Tm.Interfaces
{
  public readonly struct TmClassAnalog
  {
    public int    Id   { get; }
    public string Name { get; }


    public TmClassAnalog(int id, string name) => (Id, Name) = (id, name);
  }
}