namespace Iface.Oik.Tm.Interfaces
{
  public class TmChannel
  {
    public int    ChannelId { get; set; }
    public string Name      { get; set; }


    public static int ToSqlTma(int channelId)
    {
      return (channelId << 24);
    }


    public static (int, int) GetSqlTmaRange(int channelId)
    {
      return (ToSqlTma(channelId) + 1, ToSqlTma(channelId + 1) - 1);
    }
  }
}