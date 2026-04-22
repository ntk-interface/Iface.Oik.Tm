namespace Iface.Oik.Tm.Native.Dto;

public record ExtendedUserDataDto
{
  public byte[] Rights   { get; init; } = new byte[256];
  public int    Id   { get; init; }
  public int    GroupId  { get; init; }
  public string Nickname { get; init; } = string.Empty;
  public string Password { get; init; } = string.Empty;
  public string KeyId    { get; init; } = string.Empty;
}