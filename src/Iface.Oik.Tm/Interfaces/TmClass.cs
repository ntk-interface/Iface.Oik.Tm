namespace Iface.Oik.Tm.Interfaces
{
  public class TmClass
  {
    public int    Id   { get; }
    public string Name { get; }


    public TmClass(int id, string name)
    {
      Id   = id;
      Name = name;
    }
  }
}