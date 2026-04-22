namespace Iface.Oik.Tm.Native.Dto;

public record UserPolicyDto
{
  public int    BadLogonCount      { get; init; }
  public long   NotBeforeTimestamp { get; init; }
  public long   NotAfterTimestamp  { get; init; }
  public bool   MustChangePassword { get; init; }
  public bool   IsBlocked          { get; init; }
  public int    BadLogonLimit      { get; init; }
  public bool   Predefined         { get; init; }
  public string MacList            { get; init; }
  public bool   PasswordSet        { get; init; }
  public string UserCategory       { get; init; }
  public string UserTemplate       { get; init; }
}