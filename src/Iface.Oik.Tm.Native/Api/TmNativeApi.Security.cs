using System;
using System.Buffers;
using System.IO;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  private const string BackupDateFormat = "dd_MM_yyyy (HH.mm.ss)";

  public static PasswordPolicyDto SecGetPasswordPolicy(nint cfCid)
  {
    const string uname = ".cfs.";
    const string oname = ".";

    var flags = (TmNativeDefsUnsafe.PasswordPolicies)IfpcGetBinUint(cfCid, uname, oname, "pwd_pol_flg");

    return new PasswordPolicyDto
    {
      AdminPasswordChange  = IfpcGetBinBool(cfCid, uname, oname, "own_pch"),
      EnforcePasswordCheck = IfpcGetBinBool(cfCid, uname, oname, "pwd_pol"),
      MinPasswordLength    = IfpcGetBinInt(cfCid, uname, oname, "pwd_pol_len"),
      PasswordTtl          = IfpcGetBinInt(cfCid, uname, oname, "p_ex_days"),
      CharsUpper           = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.Upper),
      CharsDigits          = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.Digits),
      CharsSpecial         = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.Spec),
      CharsNoRepeat        = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.CheckRepeat),
      CharsNonSequential   = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.CheqSeq),
      CheckDictionary      = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.Digits),
      CheckOldPasswords    = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.CheckCache)
    };
  }
  
  public static void BackupSecurity(nint cfCid, string directory, string pwd = "")
  {
    var fileName = "";
    string snp;

    if (string.IsNullOrEmpty(pwd))
    {
      var dto = GetComputerInfoS(cfCid);
      
      snp = "\x1";

      var namePart = dto.SecType switch
                     {
                       0 => "users.ini",
                       1 => "users.01",
                       2 => "users.02",
                       _ => throw new TmNativeException("Unknown security type", 1001)
                     };

      fileName += $"{namePart} {DateTime.Now:BackupDateFormat}.bbk";
    }
    else
    {
      snp      = "\x2";
      fileName = "Security-" + DateTime.Now.ToString(BackupDateFormat) + ".sbk";
    }
    
    BackupSecurity(cfCid, snp, pwd, Path.Combine(directory, fileName));
  }

  public static void RestoreSecurity(nint cfCid, string filename, string pwd)
  {
    var          pool   = ArrayPool<byte>.Shared;
    var          errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);
    const string snp    = "\x2";

    try
    {
      TmNative.cfsIfpcRestoreSecurity(cfCid, 
                                      snp, 
                                      pwd, 
                                      filename, 
                                      out var errCode, 
                                      errBuf,
                                      TmNativeDefsUnsafe.ErrorBufSize);

      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }
  
  
  internal static void BackupSecurity(nint cfCid, 
                                      string snp, 
                                      string pwd, 
                                      string filename)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      TmNative.cfsIfpcBackupSecurity(cfCid, 
                                     snp, 
                                     pwd, 
                                     filename, 
                                     out var errCode, 
                                     errBuf,
                                     TmNativeDefsUnsafe.ErrorBufSize);

      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }
}