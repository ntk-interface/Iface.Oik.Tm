using System.Linq;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmUserInfo
  {
    public int    Id      { get; }
    public string Name    { get; }
    public string KeyId   { get; }
    public int    GroupId { get; }

    private readonly bool[] _userPermissions;


    public TmUserInfo(int    userId,
                      string userName,
                      string keyId,
                      byte   groupId,
                      byte[] permissionBytes)
    {
      Id      = userId;
      Name    = userName;
      KeyId   = keyId;
      GroupId = groupId;

      _userPermissions = new bool[permissionBytes.Length];
      permissionBytes.ForEach((b, id) => _userPermissions[id] = b > 0);
    }


    public bool HasAccess(TmUserPermissions permission)
    {
      return _userPermissions.ElementAtOrDefault((int) permission);
    }
  }
}