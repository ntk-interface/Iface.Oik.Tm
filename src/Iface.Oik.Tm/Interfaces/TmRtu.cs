namespace Iface.Oik.Tm.Interfaces
{
  public class TmRtu
  {
    public int    ChannelId { get; }
    public int    RtuId     { get; }
    public string Name      { get; }


    public TmRtu(int channelId, int rtuId, string name)
    {
      ChannelId = channelId;
      RtuId     = rtuId;
      Name      = name;
    }


    public static int ToSqlTma(int channelId, int rtuId)
    {
      return (rtuId << 16) + (channelId << 24);
    }


    public static (int, int) GetSqlTmaRange(int channelId, int rtuId)
    {
      return (ToSqlTma(channelId, rtuId) + 1, ToSqlTma(channelId, rtuId + 1) - 1);
    }
  }
}