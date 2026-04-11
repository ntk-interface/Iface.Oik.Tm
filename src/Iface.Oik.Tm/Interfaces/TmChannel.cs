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


    public static int ToTma(int channelId)
    {
      return (channelId << 24);
    }


    public static (int, int) GetTmaRange(int channelId)
    {
      return (ToTma(channelId) + 1, ToTma(channelId + 1) - 1);
    }
  }
}