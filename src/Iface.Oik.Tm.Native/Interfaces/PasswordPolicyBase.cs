namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class PasswordPolicyBase
{
  public bool AdminPasswordChange          { get; set; }
  public int  PasswordTtlDays              { get; set; }
  public bool EnforcePasswordCheck         { get; set; }
  public int  MinPasswordLength            { get; set; }
  public bool PasswordCharsUpper           { get; set; }
  public bool PasswordCharsDigits          { get; set; }
  public bool PasswordCharsSpecial         { get; set; }
  public bool PasswordCharsNoRepeat        { get; set; }
  public bool PasswordCharsNoSequential    { get; set; }
  public bool PasswordCharsCheckDictionary { get; set; }
  public bool CheckOldPasswords            { get; set; }
}