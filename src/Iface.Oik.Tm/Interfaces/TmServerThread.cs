namespace Iface.Oik.Tm.Interfaces
{
  public class TmServerThread
  {
    public int    Id       { get; private set; }
    public string Name     { get; private set; }
    public int    UpTime   { get; private set; }
    public float  WorkTime { get; private set; }

    public string UpTimeString => $"{UpTime} s";
    public string WorkTimeString => $"{WorkTime:N3} s";

    public TmServerThread(int id, string name, int upTime, float workTime)
    {
      Id = id;
      Name = name;
      UpTime = upTime;
      WorkTime = workTime;
    }
  }
}