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
  private static (TmNativeDefsUnsafe.TUserInfo userInfo, string additionalData) 
    GetUserInfoWithAdditionalData(int cid, uint userId)
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
      var additionalData = TmNativeUtil.BytesToString(buf.AsSpan()[..bufSize]);
      
      return (tUserInfo, additionalData);
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(buf);
    }

  }
  
  
  public static TUserInfoDto GetUserInfo(int cid, uint userId)
  {
    return TUserInfoDto.Create(userId, GetTUserInfo(cid, userId));
  }

  
  public static TUserInfoDto GetExtendedUserInfo(int cid, uint userId)
  {
    var (userInfo, additionalData) = GetUserInfoWithAdditionalData(cid, userId);

    return TUserInfoDto.Create(userId, userInfo, additionalData);
  }

  
  public static TUserInfoDto GetUserInfoCfs(nint   cid,
                                            string serverName,
                                            string serverType)
  {
    return TUserInfoDto.Create(GetTExtendedUserInfo(cid, serverName, serverType));
  }
  

  public static TUserInfoDto GetUserInfo(int tmCid, string serverName)
  {
    var cfCid = TmNative.tmcGetCfsHandle(tmCid);

    if (cfCid == nint.Zero)
    {
      throw new TmNativeException("Не удалось получить cfsHandle");
    }

    var extendedInfo = GetTExtendedUserInfo(cfCid, serverName, "tms$");
    var tUserInfo    = GetTUserInfo(tmCid, 0);

    return TUserInfoDto.Create(tUserInfo, extendedInfo);
  }

  
  public static string GetUserName(int tmCid, int userId)
  {
    var (_, additionalData) = GetUserInfoWithAdditionalData(tmCid, (uint) userId);

    if (!TmNativeUtil.TryFindValueByKey(additionalData, "UserNameLong", '\n', '=', out var userName))
    {
      return string.Empty;
    }

    return userName;
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

  public static T SecGetUserPolicy<T>(nint cfCid, string username) where T : UserPolicyBase, new()
  {
    var notBeforeTimestamp = IfpcGetBinLong(cfCid, username, ".", "not_before");
    var notAfterTimestamp  = IfpcGetBinLong(cfCid, username, ".", "not_after");

    return new T
    {
      BadLogonCount = IfpcGetBinInt(cfCid, username, ".", "bad_logon"),
      NotBefore = notBeforeTimestamp == 0
                    ? new DateTime()
                    : NativeDateUtil.GetDateTimeFromTimestamp(notBeforeTimestamp),
      NotAfter = notAfterTimestamp == 0
                   ? new DateTime()
                   : NativeDateUtil.GetDateTimeFromTimestamp(notAfterTimestamp),
      MustChangePassword = IfpcGetBinBool(cfCid, username, ".", "chgp"),
      IsBlocked          = IfpcGetBinBool(cfCid, username, ".", "blocked"),
      BadLogonLimit      = IfpcGetBinInt(cfCid, username, ".", "logon_limit"),
      Predefined         = IfpcGetBinBool(cfCid, username, ".", "initial"),
      EnabledMacs        = IfpcGetBinMacList(cfCid, username),
      PasswordSet        = username[0] != '*' || IfpcGetBinPwd(cfCid, username),
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


  public static unsafe T SecGetExtendedUserData<T>(nint   cfCid,
                                                string serverType,
                                                string serverName,
                                                string username) 
    where T: ExtendedUserDataBase, new()
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

      var exUserData = new T();

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
          var encoding = TmNativeUtil.DetectEncoding(span);
          var key      = encoding.GetString(span[..equalIndex]);
          var value    = encoding.GetString(span[(equalIndex + 1)..]);

          switch (key)
          {
            case "UserID" when int.TryParse(value, out var id):
            {
              exUserData.UserId = id;
              break;
            }
            case "UserNick":
            {
              exUserData.UserNickname = value;
              break;
            }
            case "UserPwd":
            {
              exUserData.UserPassword = value;
              break;
            }
            case "Group" when int.TryParse(value, out var id):
            {
              exUserData.GroupId = id;
              break;
            }
            case "KeyID":
            {
              exUserData.KeyId = value;
              break;
            }
          }
        }
        else if (span[0] == 'R' && int.TryParse(span[1..], out var index))
        {
          exUserData.Rights[index] = 1;
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

      return exUserData;
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }

  public static unsafe T SecGetAccessDescriptor<T, TA>(string    sSetupPath,
                                                       string    progName,
                                                       Encoding? encoding = null)
    where TA : AccessMaskBase, new()
    where T : AccessMasksDescriptorBase<TA>, new()
  {
    var iniLookup = new Dictionary<string, string>
    {
      { TmNativeDefsUnsafe.MsTreeNodesNames.Portcore, "master#1.prp.Security" },
      { TmNativeDefsUnsafe.MsTreeNodesNames.Master, "master.prp.Security" },
      { TmNativeDefsUnsafe.MsTreeNodesNames.RBaseServer, "rbsrv#1.prp.Security" },
      { TmNativeDefsUnsafe.MsTreeNodesNames.RbsrvOld, "serv_dll.ch.RbsSecurity" },
      { TmNativeDefsUnsafe.MsTreeNodesNames.TmServer, "pcsrv#1.prp.Security" },
      { TmNativeDefsUnsafe.MsTreeNodesNames.PcsrvOld, "serv_dll.ch.TmsSecurity" },
    };

    if (!iniLookup.TryGetValue(progName, out var section))
    {
      throw new TmNativeException($"Unknown master service tree node {progName}");
    }

    var ptr = TmNative.cfsGetAccessDescriptor(sSetupPath, section);

    if (ptr == nint.Zero)
    {
      throw new TmNativeException($"Failed to get section ptr for {sSetupPath}/{section}");
    }

    var data = TmNativeUtil.FromIntPtr<TmNativeDefsUnsafe.CfsAccessDescriptor>(ptr);

    var masks = new List<TA>();

    foreach (var right in new Span<TmNativeDefsUnsafe.AccessRight>(data.Bit, 32))
    {
      if (right.Mask == 0xffffffff)
      {
        continue;
      }

      masks.Add(new TA
      {
        Mask = right.Mask,
        Description = new Dictionary<string, string>
        {
          ["en"] = TmNativeUtil.GetCStringFromBytePtr(right.Eng, encoding).Replace("&", ""),
          ["ru"] = TmNativeUtil.GetCStringFromBytePtr(right.Rus, encoding).Replace("&", "")
        }
      });
    }

    var descriptor = new T
    {
      NamePrefix = GetAccessDescriptorNamePrefix(data.NamePrefix),
      ObjTypeName = new Dictionary<string, string>
      {
        ["en"] = TmNativeUtil.GetCStringFromBytePtr(data.ObjTypeName.Eng, encoding).Replace("&", ""),
        ["ru"] = TmNativeUtil.GetCStringFromBytePtr(data.ObjTypeName.Rus, encoding).Replace("&", "")
      },
      AccessMasks = masks
    };

    TmNative.cfsFreeMemory(ptr);

    return descriptor;
  }

  public static unsafe T SecGetExtendedRightsDescriptor<T, TR>(string sSetupPath, Encoding? encoding = null)
    where T : ExtendedRightsDescriptorBase<TR>, new()
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
        break;
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
      DoUserId   = data.DoUserID,
      DoUserPwd  = data.DoUserPwd,
      DoUserNick = data.DoUserNick,
      MaxUserID  = data.MaxUserID,
      DoGroup    = data.DoGroup,
      DoKeyId    = data.DoKeyID,
      Rights     = rights
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

  internal static unsafe string GetAccessDescriptorNamePrefix(byte* ptr, Encoding? encoding = null)
  {
    if (ptr[0] == 0)
    {
      return string.Empty;
    }

    var length = 0;
    encoding ??= Encoding.UTF8;

    var isDollarSign = false;
    while (ptr[length] != 0 && !isDollarSign)
    {
      isDollarSign = ptr[length] == (byte)'$';
      length++;
    }

    return encoding.GetString(ptr, length);
  }
}