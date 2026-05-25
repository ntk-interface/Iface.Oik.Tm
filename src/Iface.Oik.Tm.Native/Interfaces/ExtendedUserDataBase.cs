namespace Iface.Oik.Tm.Native.Interfaces;

public class ExtendedUserDataBase
{
  public int    UserId   { get; set; }
  public int    GroupId    { get; set; }
  public string UserNickname { get; set; } = string.Empty;
  public string UserPassword  { get; set; } = string.Empty;
  public string KeyId    { get; set; } = string.Empty;
  public byte[] Rights   { get; set; } = new byte[250];
}