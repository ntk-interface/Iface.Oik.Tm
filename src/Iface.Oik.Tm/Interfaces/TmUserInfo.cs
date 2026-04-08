using System;
using System.Linq;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmUserInfo
  {
    private TmSecurityAccessFlags _securityAccessFlags;
    private DateTime?             _connectionTime;

    public int                   Id                         { get; }
    public string                Name                       { get; }
    public string                NtName                     { get; }
    public string                AuthorizationName          { get; }
    public string                Comment                    { get; }
    public string                Category                   { get; }
    public string                KeyId                      { get; }
    public int                   GroupId                    { get; }
    public TmSecurityAccessFlags AccessFlags                { get; }
    public DateTime?             ConnectionTime             { get; }
    public string                AdditionalParametersString { get; }

    private readonly bool[] _userPermissions;

    public string AccessMask => $"{(uint)AccessFlags:x8}".ToUpperInvariant();

    public string ConnectionTimeString => ConnectionTime.HasValue
                                            ? $"{ConnectionTime.Value:dd.MM.yyyy H:mm:ss}"
                                            : string.Empty;


    public TmUserInfo(TUserInfoDto infoDto)
    {
      Id                         = infoDto.Id;
      Name                       = infoDto.UserName;
      NtName                     = infoDto.NtUserName;
      AuthorizationName          = infoDto.OldUserName;
      Comment                    = infoDto.UserComment;
      AccessFlags                = (TmSecurityAccessFlags)infoDto.AccessMask;
      ConnectionTime             = DateUtil.GetDateTimeFromTimestampWithEpochCheck(infoDto.ConnectTime);
      Category                   = infoDto.UserCategory;
      AdditionalParametersString = infoDto.AdditionalParametersString;
      KeyId                      = infoDto.KeyId;
      GroupId                    = infoDto.GroupId;

      _userPermissions = new bool[infoDto.PermissionBytes.Length];
      infoDto.PermissionBytes.ForEach((b, idx) => _userPermissions[idx] = b > 0);
    }

    public bool HasAccess(TmUserPermissions permission)
    {
      return _userPermissions.ElementAtOrDefault((int)permission);
    }
  }
}