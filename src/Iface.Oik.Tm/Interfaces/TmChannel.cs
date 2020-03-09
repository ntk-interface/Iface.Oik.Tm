namespace Iface.Oik.Tm.Interfaces
{
  public class TmChannel
  {
    public int    ChannelId { get; }
    public string Name      { get; }


    public TmChannel(int channelId, string name)
    {
      ChannelId = channelId;
      Name      = name;
    }


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