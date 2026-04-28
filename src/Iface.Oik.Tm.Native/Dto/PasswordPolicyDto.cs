namespace Iface.Oik.Tm.Native.Dto;

public record PasswordPolicyDto
{
  public bool AdminPasswordChange  { get; init; }
  public bool EnforcePasswordCheck { get; init; }

  public int MinPasswordLength { get; init; }
  public int PasswordTtl       { get; init; }

  public bool CharsUpper         { get; init; }
  public bool CharsDigits        { get; init; }
  public bool CharsSpecial       { get; init; }
  public bool CharsNoRepeat      { get; init; }
  public bool CharsNonSequential { get; init; }
  public bool CheckDictionary    { get; init; }
  public bool CheckOldPasswords  { get; init; }
}