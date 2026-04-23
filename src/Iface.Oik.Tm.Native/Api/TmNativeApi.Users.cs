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
      BadLogonCount      = IfpcGetBinInt(cfCid, username, ".", "bad_logon"),
      NotBeforeTimestamp = IfpcGetBinLong(cfCid, username, ".", "not_before"),
      NotAfterTimestamp  = IfpcGetBinLong(cfCid, username, ".", "not_after"),
      MustChangePassword = IfpcGetBinBool(cfCid, username, ".", "chgp"),
      IsBlocked          = IfpcGetBinBool(cfCid, username, ".", "blocked"),
      BadLogonLimit      = IfpcGetBinInt(cfCid, username, ".", "logon_limit"),
      Predefined         = IfpcGetBinBool(cfCid, username, ".", "initial"),
      MacList            = IfpcGetBinMacList(cfCid, username, ".", "mac_list"),
      PasswordSet        = username[0] != '*' || IfpcGetBinBool(cfCid, username, ".", "pwd"),
      UserCategory       = IfpcBinString(cfCid, username, ".", "uctgr"),
      UserTemplate       = IfpcBinString(cfCid, username, ".", "utmpl"),
    };
  }

  public static uint SecGetAccessMask(nint cfCid, string username, string oName)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var result = TmNative.cfsIfpcGetAccess(cfCid,
                                             username,
                                             oName,
                                             out var errCode,
                                             errBuf,
                                             TmNativeDefsUnsafe.ErrorBufSize);

      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      return result;
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
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

      var ptr = (byte*)binPtr;

      var p      = ptr;
      var length = 0;

      var rights       = new byte[256];
      var userId       = 0;
      var groupId      = 0;
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

        if (equalIndex != -1)
        {
          var encoding = TmNative.cfsIsUTF8(span) ? Encoding.UTF8 : Encoding.GetEncoding(1251);
          var key      = encoding.GetString(span[..equalIndex]);
          var value    = encoding.GetString(span[(equalIndex + 1)..]);

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

  public static unsafe T SecGetExtendedRightsDescriptor<T, TR>(string sSetupPath, Encoding? encoding = null)
    where                   T : ExtendedRightsDescriptorBase<TR>, new ()
    where TR : ExtendedRightBase, new()
  {
    var ptr = TmNative.cfsGetExtendedUserRightsDescriptor(sSetupPath,
                                                          "TmsExtRights",
                                                          0);

    if (ptr == nint.Zero)
    {
      throw new TmNativeException("Failed to get TmsExtRights");
    }

    var data = TmNativeUtil.FromIntPtr<TmNativeDefsUnsafe.CfsExtSrvrtDescriptor>(ptr);

    var rightsPtr = (byte*)data.Rights;
    var p         = rightsPtr;
    var length    = 0;
    encoding ??= Encoding.UTF8;
    
    var rights = new List<TR>();
    while (true)
    {
      while (p[length] != 0)
      {
        length++;
      }

      var span = new ReadOnlySpan<byte>(p, length);

      var separatorIndex = span.IndexOf((byte)'`');

      if (separatorIndex == -1)
      {
        continue;
      }

      switch (span[0])
      {
        case (byte)'B':
        {
          var right = new TR
          {
            IsHeader = true,
            Description = new Dictionary<string, string>
            {
              ["en"] = encoding.GetString(span[1..separatorIndex]),
              ["ru"] = encoding.GetString(span[(separatorIndex + 1)..])
            }
          };
          
          rights.Add(right);
          
          break;
        }
        case (byte)'R':
        {
          var prefixEndIndex = span.IndexOf((byte)'-');
          if (!Utf8Parser.TryParse(span[1..prefixEndIndex], out byte index, out _))
          {
            continue;
          }
          
          var right = new TR
          {
            ByteIndex = index,
            IsHeader  = false,
            Description = new Dictionary<string, string>
            {
              ["en"] = encoding.GetString(span[(prefixEndIndex + 1)..separatorIndex]),
              ["ru"] = encoding.GetString(span[(separatorIndex + 1)..])
            }
          };
          
          rights.Add(right);
          break;
        }
      }


      length++;

      if (p[length] == 0)
      {
        break;
      }

      p      += length;
      length =  0;
    }

    var descriptor = new T
    {
      DoUserId = data.DoUserID,
      DoUserPwd = data.DoUserPwd,
      DoUserNick = data.DoUserNick,
      MaxUserID = data.MaxUserID,
      DoGroup = data.DoGroup,
      DoKeyId = data.DoKeyID,
      Rights = rights
    };
    
    TmNative.cfsFreeMemory(ptr);

    return descriptor;
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
}