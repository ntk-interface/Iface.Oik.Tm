namespace Iface.Oik.Tm.Interfaces
{
  public class TmRtu
  {
    public int    ChannelId { get; set; }
    public int    RtuId     { get; set; }
    public string Name      { get; set; }


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