using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static TUserInfoDto GetUserInfo(int cid, uint userId)
  {
    var tUserInfo = GetTUserInfo(cid, userId);
    return TUserInfoDto.Create(userId, tUserInfo);
  }

  public static TUserInfoDto GetExtendedUserInfo(int cid, uint userId)
  {
    const int bufSize   = 1024;
    var       pool      = ArrayPool<byte>.Shared;
    var       buf       = pool.Rent(bufSize);
    var       tUserInfo = new TmNativeDefsUnsafe.TUserInfo();

    try
    {
      if (!TmNative.tmcGetUserInfoEx(cid, userId, ref tUserInfo, buf, bufSize))
      {
        throw new TmNativeException($"Ошибка получения расширенной информации о пользователе {userId}");
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(buf);
    }


    return TUserInfoDto.Create(userId, tUserInfo, TmNativeUtil.BytesToString(buf));
  }

  public static TUserInfoDto GetUserInfoCfs(nint   cid,
                                            string serverName,
                                            string serverType)
  {
    var tExtendedUserInfo = GetTExtendedUserInfo(cid, serverName, serverType);
    return TUserInfoDto.Create(tExtendedUserInfo);
  }

  public static TUserInfoDto GetUserInfo(int tmCid, string severName)
  {
    var cfCid = TmNative.tmcGetCfsHandle(tmCid);

    if (cfCid == nint.Zero)
    {
      throw new TmNativeException("Не удалось получить cfsHandle");
    }

    var extendedInfo = GetTExtendedUserInfo(cfCid, severName, "tms$");
    var tUserInfo    = GetTUserInfo(tmCid, 0);

    return TUserInfoDto.Create(tUserInfo, extendedInfo);
  }

  public static string GetUserName(int tmCid, int userId)
  {
    var userInfo = GetTUserInfo(tmCid, (uint)userId);

    unsafe
    {
      return TmNativeUtil.BytePtrToString(userInfo.UserName, 16);
    }
  }

  public static IReadOnlyCollection<string> SecEnumUsers(nint cfCid)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var ptr = TmNative.cfsIfpcEnumUsers(cfCid,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);

      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      var users = TmNativeUtil.GetStringsListFromIntPtr(ptr);
      TmNative.cfsFreeMemory(ptr);

      return users;
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  public static UserPolicyDto SecGetUserPolicy(nint cfCid, string username)
  {
    return new UserPolicyDto
    {
      BadLogonCount      = SecGetBinInt(cfCid, username, ".", "bad_logon"),
      NotBeforeTimestamp = SecGetBinLong(cfCid, username, ".", "not_before"),
      NotAfterTimestamp  = SecGetBinLong(cfCid, username, ".", "not_after"),
      MustChangePassword = SecGetBinBool(cfCid, username, ".", "chgp"),
      IsBlocked          = SecGetBinBool(cfCid, username, ".", "blocked"),
      BadLogonLimit      = SecGetBinInt(cfCid, username, ".", "logon_limit"),
      Predefined         = SecGetBinBool(cfCid, username, ".", "initial"),
      MacList            = SecGetBinMacList(cfCid, username, ".", "mac_list"),
      PasswordSet        = username[0] != '*' || SecGetBinBool(cfCid, username, ".", "pwd"),
      UserCategory       = SecGetBinString(cfCid, username, ".", "uctgr"),
      UserTemplate       = SecGetBinString(cfCid, username, ".", "utmpl"),
    };
  }

  public static unsafe ExtendedUserDataDto SecGetExtendedUserData(nint   cfCid,
                                                                  string serverType,
                                                                  string serverName,
                                                                  string username)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          username,
                                          serverType + serverName,
                                          "extr",
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      var ptr    = (byte*)binPtr;

      var p      = ptr;
      var length = 0;

      var rights       = new byte[256];
      var userId       = 0;
      var groupId    = 0;
      var userNickname = string.Empty;
      var userPassword = string.Empty;
      var keyId        = string.Empty;
      
      while (true)
      {
        while (p[length] != 0)
        {
          length++;
        }

        var span       = new ReadOnlySpan<byte>(p, length);
        var equalIndex = span.IndexOf((byte)'=');

        var list = TmNativeUtil.GetStringsListFromIntPtr(binPtr);

        if (equalIndex != -1)
        {
          var encoding  = TmNative.cfsIsUTF8(span) ? Encoding.UTF8 : Encoding.GetEncoding(1251);
          var key       = encoding.GetString(span[..equalIndex]);
          var value = encoding.GetString(span[(equalIndex + 1)..]);

          switch (key)
          {
            case "UserID" when int.TryParse(value, out var id):
            {
              userId = id;
              break;
            }
            case "UserNick":
            {
              userNickname = value;
              break;
            }
            case "UserPwd":
            {
              userPassword = value;
              break;
            }
            case "Group" when int.TryParse(value, out var id):
            {
              groupId = id;
              break;
            }
            case "KeyID":
            {
              keyId = value;
              break;
            }
          }
        }
        else if (span[0] == 'R' && int.TryParse(span[1..], out var index))
        {
          rights[index] = 1;
        }

        length++;

        if (p[length] == 0)
        {
          break;
        }

        p      += length;
        length =  0;
      }

      TmNative.cfsFreeMemory(binPtr);

      return new ExtendedUserDataDto
      {
        Rights   = rights,
        GroupId  = groupId,
        Id       = userId,
        Nickname = userNickname,
        Password = userPassword,
        KeyId    = keyId
      };
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  public static PasswordPolicyDto SecGetPasswordPolicy(nint cfCid)
  {
    const string uname = ".cfs.";
    const string oname = ".";

    var flags = (TmNativeDefsUnsafe.PasswordPolicies)SecGetBinUint(cfCid, uname, oname, "pwd_pol_flg");

    return new PasswordPolicyDto
    {
      AdminPasswordChange  = SecGetBinBool(cfCid, uname, oname, "own_pch"),
      EnforcePasswordCheck = SecGetBinBool(cfCid, uname, oname, "pwd_pol"),
      MinPasswordLength    = SecGetBinInt(cfCid, uname, oname, "pwd_pol_len"),
      PasswordTtl          = SecGetBinInt(cfCid, uname, oname, "p_ex_days"),
      CharsUpper           = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.Upper),
      CharsDigits          = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.Digits),
      CharsSpecial         = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.Spec),
      CharsNoRepeat        = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.CheckRepeat),
      CharsNonSequential   = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.CheqSeq),
      CheckDictionary      = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.Digits),
      CheckOldPasswords    = flags.HasFlag(TmNativeDefsUnsafe.PasswordPolicies.CheckCache)
    };
  }
  
  internal static TmNativeDefsUnsafe.TUserInfo GetTUserInfo(int tmCid, uint userId)
  {
    var tUserInfo = new TmNativeDefsUnsafe.TUserInfo();

    if (!TmNative.tmcGetUserInfo(tmCid, userId, ref tUserInfo))
    {
      throw new TmNativeException($"Ошибка получения информации о пользователе {userId}");
    }

    return tUserInfo;
  }

  internal static unsafe TmNativeDefsUnsafe.TExtendedUserInfo GetTExtendedUserInfo(nint cfCid,
    string                                                                              serverName,
    string                                                                              serverType)
  {
    var tExtendedUserInfo = new TmNativeDefsUnsafe.TExtendedUserInfo();

    var fetchResult = TmNative.cfsGetExtendedUserData(cfCid,
                                                      serverType,
                                                      serverName,
                                                      ref tExtendedUserInfo,
                                                      (uint)sizeof(TmNativeDefsUnsafe.TExtendedUserInfo));

    if (fetchResult == 0)
    {
      throw new TmNativeException($"Ошибка получения TExtendedUserInfo. Server name: {serverName}");
    }

    return tExtendedUserInfo;
  }

  internal static unsafe int SecGetBinInt(nint   cfCid,
                                          string uName,
                                          string oName,
                                          string binName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var ptr    = (byte*)binPtr;
          var length = 0;

          while (ptr[length] != 0)
          {
            length++;
          }

          if (!Utf8Parser.TryParse(new ReadOnlySpan<byte>(ptr, length), out int value, out _))
          {
            throw new FormatException($"Wrong response format for {uName}{oName}{binName}");
          }

          TmNative.cfsFreeMemory(binPtr);

          return value;
        }
        case 2:
          return 0;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  internal static unsafe uint SecGetBinUint(nint   cfCid,
                                          string uName,
                                          string oName,
                                          string binName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var ptr    = (byte*)binPtr;
          var length = 0;

          while (ptr[length] != 0)
          {
            length++;
          }

          if (!Utf8Parser.TryParse(new ReadOnlySpan<byte>(ptr, length), out uint value, out _))
          {
            throw new FormatException($"Wrong response format for {uName}{oName}{binName}");
          }

          TmNative.cfsFreeMemory(binPtr);

          return value;
        }
        case 2:
          return 0;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  
  internal static unsafe long SecGetBinLong(nint   cfCid,
                                            string uName,
                                            string oName,
                                            string binName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var ptr    = (byte*)binPtr;
          var length = 0;

          while (ptr[length] != 0)
          {
            length++;
          }

          if (!Utf8Parser.TryParse(new ReadOnlySpan<byte>(ptr, length), out long value, out _))
          {
            throw new FormatException($"Wrong response format for {uName}{oName}{binName}");
          }

          TmNative.cfsFreeMemory(binPtr);

          return value;
        }
        case 2:
          return 0;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  internal static string SecGetBinString(nint   cfCid,
                                         string uName,
                                         string oName,
                                         string binName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var value = TmNativeUtil.GetCStringFromIntPtrAutoEncoding(binPtr);

          TmNative.cfsFreeMemory(binPtr);

          return value;
        }
        case 2:
          return string.Empty;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  internal static unsafe bool SecGetBinBool(nint   cfCid,
                                            string uName,
                                            string oName,
                                            string binName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out _,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var ptr = (byte*)binPtr;

          var value = ptr[0] == (byte)'1';

          TmNative.cfsFreeMemory(binPtr);

          return value;
        }
        case 2:
          return false;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  internal static unsafe string SecGetBinMacList(nint   cfCid,
                                                 string uName,
                                                 string oName,
                                                 string binName)
  {
    var                pool   = ArrayPool<byte>.Shared;
    var                errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);
    ReadOnlySpan<char> hex    = "0123456789ABCDEF";

    try
    {
      var binPtr = TmNative.cfsIfpcGetBin(cfCid,
                                          uName,
                                          oName,
                                          binName,
                                          out var length,
                                          out var errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize);
      switch (errCode)
      {
        case 0:
        {
          var        value  = new StringBuilder();
          Span<char> buffer = stackalloc char[18];

          for (var i = 0; i < length; i += 6)
          {
            var mac = new ReadOnlySpan<byte>((byte*)binPtr, 6);

            var pos = 0;

            for (var j = 0; j < 6; j++)
            {
              var b = mac[j];

              buffer[pos++] = hex[b >> 4];
              buffer[pos++] = hex[b & 0xF];

              if (j != 5)
              {
                buffer[pos++] = ':';
              }
            }

            buffer[pos + 1] = '\n';

            value.Append(buffer);
          }

          TmNative.cfsFreeMemory(binPtr);

          return value.ToString();
        }
        case 2:
          return string.Empty;
        default:
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }
}