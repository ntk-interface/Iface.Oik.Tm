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

  #region Policies

  public static T SecGetPasswordPolicy<T>(nint cfCid)
    where T : PasswordPolicyBase, new()
  {
    const string uname = ".cfs.";
    const string oname = ".";

    var flags = (TmNativeDefsUnsafe.PasswordPolicies)IfpcGetBinUint(cfCid, uname, oname, "pwd_pol_flg");

    return new T
    {
      AdminPasswordChange          = IfpcGetBinBool(cfCid, uname, oname, "own_pch"),
      EnforcePasswordCheck         = IfpcGetBinBool(cfCid, uname, oname, "pwd_pol"),
      MinPasswordLength            = IfpcGetBinInt(cfCid, uname, oname, "pwd_pol_len"),
      PasswordTtlDays              = IfpcGetBinInt(cfCid, uname, oname, "p_ex_days"),
      PasswordCharsUpper           = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.Upper),
      PasswordCharsDigits          = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.Digits),
      PasswordCharsSpecial         = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.Spec),
      PasswordCharsNoRepeat        = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.CheckRepeat),
      PasswordCharsNoSequential    = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.CheqSeq),
      PasswordCharsCheckDictionary = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.CheckDict),
      CheckOldPasswords            = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.CheckCache)
    };
  }

  public static void SecSetPasswordPolicy(nint               cfCid,
                                          PasswordPolicyBase passwordPolicy)
  {
    const string uname = ".cfs.";
    const string oname = ".";


    var flags = TmNativeDefsUnsafe.PasswordPolicies.Undefined;

    if (!passwordPolicy.PasswordCharsUpper) flags           &= ~TmNativeDefsUnsafe.PasswordPolicies.Upper;
    if (!passwordPolicy.PasswordCharsDigits) flags          &= ~TmNativeDefsUnsafe.PasswordPolicies.Digits;
    if (!passwordPolicy.PasswordCharsSpecial) flags         &= ~TmNativeDefsUnsafe.PasswordPolicies.Spec;
    if (!passwordPolicy.PasswordCharsNoRepeat) flags        &= ~TmNativeDefsUnsafe.PasswordPolicies.CheckRepeat;
    if (!passwordPolicy.PasswordCharsNoSequential) flags    &= ~TmNativeDefsUnsafe.PasswordPolicies.CheqSeq;
    if (!passwordPolicy.PasswordCharsCheckDictionary) flags &= ~TmNativeDefsUnsafe.PasswordPolicies.CheckDict;
    if (!passwordPolicy.CheckOldPasswords) flags            &= ~TmNativeDefsUnsafe.PasswordPolicies.CheckCache;

    IfpcSetBinBool(cfCid, uname, oname, "own_pch", passwordPolicy.AdminPasswordChange);
    IfpcSetBinBool(cfCid, uname, oname, "pwd_pol", passwordPolicy.EnforcePasswordCheck);
    IfpcSetBinInt(cfCid, uname, oname, "pwd_pol_len", passwordPolicy.MinPasswordLength);
    IfpcSetBinInt(cfCid, uname, oname, "p_ex_days",   passwordPolicy.PasswordTtlDays);
    IfpcSetBinUint(cfCid, uname, oname, "pwd_pol_flg", (uint)flags);
  }

  public static int SecGetStrictSessionControl(nint cfCid)
  {
    const string fileRelativePath = "Data\\Main\\cfshare.ini";
    const string section          = "Common";
    const string key              = "StrictSessionControl";

    var basePath = GetBasePath(cfCid);
    var filePath = $"{basePath}\\{fileRelativePath}";

    var valueString = GetIniString(cfCid, filePath, section, key, "0", 128);

    if (!int.TryParse(valueString, out var value))
    {
      throw new TmNativeException("Wrong session control value return format");
    }
    
    return value;
  }
  
  public static void SecSetStrictSessionControl(nint cfCid, int value)
  {
    const string fileRelativePath = "Data\\Main\\cfshare.ini";
    const string section          = "Common";
    const string key              = "StrictSessionControl";

    var basePath = GetBasePath(cfCid);
    var filePath = $"{basePath}\\{fileRelativePath}";

    SetIniString(cfCid, filePath, section, key, $"{value}");
  }

  public static void SecSetUserPolicy(nint           cfCid,
                                      string         username,
                                      UserPolicyBase userPolicy)
  {
    const string oname = ".";

    IfpcSetBinBool(cfCid, username, oname, "blocked", userPolicy.IsBlocked);
    IfpcSetBinBool(cfCid, username, oname, "chgp",    userPolicy.MustChangePassword);
    IfpcSetBinTimestamp(cfCid, username, oname, "not_before", userPolicy.NotBefore);
    IfpcSetBinTimestamp(cfCid, username, oname, "not_after",  userPolicy.NotAfter);
    IfpcSetBinInt(cfCid, username, oname, "logon_limit", userPolicy.BadLogonLimit);
    IfpcSetBinString(cfCid, username, oname, "uctgr", userPolicy.UserCategory);
    IfpcSetBinString(cfCid, username, oname, "utmpl", userPolicy.UserTemplate);
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

  #endregion

  #region Backups

  public static (bool, string) SaveMachineConfigEx(string            host,
                                                   string            directory,
                                                   uint              scope,
                                                   TmNativeCallback? callback          = null,
                                                   nint              callbackParameter = default)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    var fileName = scope switch
                   {
                     0 => "DevConf-"       + DateTime.Now.ToString(BackupDateFormat) + ".pkf",
                     1 => "FullConf-"      + DateTime.Now.ToString(BackupDateFormat) + ".cfim",
                     2 => "FullConfRetro-" + DateTime.Now.ToString(BackupDateFormat) + ".cfim",
                     _ => "undefined"
                   };

    try
    {
      var result = TmNative.cfsSaveMachineConfigEx(host,
                                                   Path.Combine(directory, fileName),
                                                   scope,
                                                   callback, callbackParameter,
                                                   errBuf, TmNativeDefsUnsafe.ErrorBufSize);

      return (result, result ? string.Empty : TmNativeUtil.BytesToString(errBuf));
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  public static bool CreateBackup(string            host,
                                  string            progName,
                                  string            pipeName,
                                  string            directory,
                                  bool              withRetro,
                                  TmNativeCallback? callback          = null,
                                  nint              callbackParameter = default)
  {
    switch (progName)
    {
      case TmNativeDefsUnsafe.MsTreeNodesNames.TmServer:
      case TmNativeDefsUnsafe.MsTreeNodesNames.PcsrvOld:
      {
        uint bFlags = 1 | 2 | 4 | 8;
        if (withRetro)
        {
          bFlags |= 0x10;
        }

        return TmNative.tmcBackupServerProcedure(host,
                                                 pipeName,
                                                 directory,
                                                 ref bFlags,
                                                 0,
                                                 callback,
                                                 callbackParameter);
      }

      case TmNativeDefsUnsafe.MsTreeNodesNames.RBaseServer:
      case TmNativeDefsUnsafe.MsTreeNodesNames.RbsrvOld:
      {
        uint bFlags = 1;

        return TmNative.rbcBackupServerProcedure(host,
                                                 pipeName,
                                                 directory,
                                                 ref bFlags,
                                                 0,
                                                 callback,
                                                 callbackParameter);
      }

      default:
        return false;
    }
  }

  public static RestoreBackupResult RestoreBackup(string            host,
                                                  string            progName,
                                                  string            pipeName,
                                                  string            filename,
                                                  bool              withRetro,
                                                  TmNativeCallback? callback          = null,
                                                  nint              callbackParameter = default)
  {
    switch (progName)
    {
      case TmNativeDefsUnsafe.MsTreeNodesNames.TmServer:
      case TmNativeDefsUnsafe.MsTreeNodesNames.PcsrvOld:
      {
        uint bFlags = 1 | 2 | 4 | 8;
        if (withRetro)
        {
          bFlags |= 0x10;
        }

        var result = TmNative.tmcRestoreServer(true,
                                               host,
                                               pipeName,
                                               filename,
                                               ref bFlags,
                                               0,
                                               callback,
                                               callbackParameter);

        if (bFlags == 0)
        {
          return RestoreBackupResult.NothingToRestore;
        }

        return result ? RestoreBackupResult.Success : RestoreBackupResult.Error;
      }

      case TmNativeDefsUnsafe.MsTreeNodesNames.RBaseServer:
      case TmNativeDefsUnsafe.MsTreeNodesNames.RbsrvOld:
      {
        uint bFlags = 1;

        var result = TmNative.tmcRestoreServer(false,
                                               host,
                                               pipeName,
                                               filename,
                                               ref bFlags,
                                               0,
                                               callback,
                                               callbackParameter);
        if (bFlags == 0)
        {
          return RestoreBackupResult.NothingToRestore;
        }

        return result ? RestoreBackupResult.Success : RestoreBackupResult.Error;
      }

      default:
      {
        return RestoreBackupResult.Error;
      }
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

  #endregion
}