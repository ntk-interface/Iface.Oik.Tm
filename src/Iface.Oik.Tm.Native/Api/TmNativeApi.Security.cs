using System;
using System.Buffers;
using System.Collections.Generic;
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

  public static void SecSetUserPolicy(nint           cfCid,
                                      string         username,
                                      UserPolicyBase userPolicy)
  {
    IfpcSetBinBool(cfCid, username, ".", "blocked", userPolicy.IsBlocked);
    IfpcSetBinBool(cfCid, username, ".", "chgp",    userPolicy.MustChangePassword);
    IfpcSetBinTimestamp(cfCid, username, ".", "not_before", userPolicy.NotBefore);
    IfpcSetBinTimestamp(cfCid, username, ".", "not_after",  userPolicy.NotAfter);
    IfpcSetBinInt(cfCid, username, ".", "logon_limit", userPolicy.BadLogonLimit);
    IfpcSetBinString(cfCid, username, ".", "uctgr", userPolicy.UserCategory);
    IfpcSetBinString(cfCid, username, ".", "utmpl", userPolicy.UserTemplate);
    IfpcSetBinMacs(cfCid, username, userPolicy.EnabledMacs);
  }

  public static void SecSetExtendedUserData(nint                 cfCid,
                                            string               serverType,
                                            string               serverName,
                                            string               username,
                                            ExtendedUserDataBase extendedUserData)
  {
    var data = new List<string>
    {
      $"UserID={extendedUserData.UserId}",
      $"UserNick={extendedUserData.UserNickname}",
      $"UserPwd={extendedUserData.UserPassword}",
      $"Group={extendedUserData.GroupId}",
      $"KeyID={extendedUserData.KeyId}",
    };

    for (var idx = 0; idx < extendedUserData.Rights.Length; idx++)
    {
      if (extendedUserData.Rights[idx] == 1)
      {
        data.Add("R" + idx);
      }
    }

    IfpcSetBinStrings(cfCid, username, serverType + serverName, "extr", data);
  }

  public static void SecSetAccessMask(nint   cfCid,
                                      string username,
                                      string oName,
                                      uint   accessMask)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      TmNative.cfsIfpcSetAccess(cfCid,
                                username,
                                oName,
                                accessMask,
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

  public static void BackupSecurity(nint cfCid, string directory, string pwd = "")
  {
    var    fileName = "";
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


  internal static void BackupSecurity(nint   cfCid,
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