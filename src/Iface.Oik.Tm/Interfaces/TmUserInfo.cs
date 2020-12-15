using System;
using System.Collections.ObjectModel;
using System.Linq;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmUserInfo
  {
    private TmSecurityAccessFlags _securityAccessFlags;
    private DateTime?             _connectionTime;

    public int                   Id                   { get; }
    public string                Name                 { get; }
    public string                NtName               { get; }
    public string                AuthorizationName    { get; }
    public string                Comment              { get; }
    public string                Category             { get; }
    public string                KeyId                { get; }
    public int                   GroupId              { get; }
    public TmSecurityAccessFlags AccessFlags          { get; }
    public DateTime?             ConnectionTime       { get; }
    public string                AdditionalParametersString { get; }
    
    private readonly bool[] _userPermissions;

    public string AccessMask => $"{(uint) AccessFlags:x8}".ToUpperInvariant();

    public string ConnectionTimeString => ConnectionTime.HasValue
                                            ? $"{ConnectionTime.Value:dd.MM.yyyy H:mm:ss}"
                                            : string.Empty;

    public TmUserInfo(int                    id,
                      TmNativeDefs.TUserInfo tUserInfo,
                      string additionalParametersString)
    {
      Id                   = id;
      Name                 = EncodingUtil.Win1251BytesToUtf8(tUserInfo.UserName);
      NtName               = EncodingUtil.Win1251BytesToUtf8(tUserInfo.NtUserName);
      AuthorizationName    = EncodingUtil.Win1251BytesToUtf8(tUserInfo.OldUserName);
      Comment              = EncodingUtil.Win1251BytesToUtf8(tUserInfo.UserComment);
      AccessFlags          = (TmSecurityAccessFlags) tUserInfo.AccessMask;
      ConnectionTime       = DateUtil.GetDateTimeFromTimestampWithEpochCheck(tUserInfo.ConnectTime);
      Category             = EncodingUtil.Win1251BytesToUtf8(tUserInfo.UserCategory);
      AdditionalParametersString = additionalParametersString;
    }

    public TmUserInfo(int    id,
                      string name,
                      string category,
                      string keyId,
                      byte   groupId,
                      byte[] permissionBytes)
    {
      Id       = id;
      Name     = name;
      Category = category;
      KeyId    = keyId;
      GroupId  = groupId;

      _userPermissions = new bool[permissionBytes.Length];
      permissionBytes.ForEach((b, idx) => _userPermissions[idx] = b > 0);
    }


    public bool HasAccess(TmUserPermissions permission)
    {
      return _userPermissions.ElementAtOrDefault((int) permission);
    }
  }
}