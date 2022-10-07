using System;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Utils
{
  public static class GuidUtil
  {
    public static Guid EncodeCimEquipment(int schemeId, int objectId)
    {
      return Encode(CimGuidScope.Equipment, schemeId, objectId, 0);
    }


    private static Guid Encode(CimGuidScope scope, int value1, int value2, int value3)
    {
      var bytes = new byte[16];
    
      bytes[0] = (byte) scope;
      Array.Copy(BitConverter.GetBytes(value1), 0, bytes, 1, 4);
      Array.Copy(BitConverter.GetBytes(value2), 0, bytes, 5, 4);
      Array.Copy(BitConverter.GetBytes(value3), 0, bytes, 9, 4);

      return new Guid(bytes);
    }
  }
}