using System;
using Iface.Oik.Tm.Interfaces;

namespace Iface.Oik.Tm.Utils
{
  public static class GuidUtil
  {
    public static Guid EncodeCimSubstation(int schemeId)
    {
      return Encode(CimGuidScope.Substation, schemeId, 0, 0);
    }
    
    
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


    public static bool TryDecodeCimEquipment(Guid id, out int schemeId, out int objectId)
    {
      return TryDecode(CimGuidScope.Equipment, id, out schemeId, out objectId, out _);
    }


    public static bool TryDecodeCimSubstation(Guid id, out int schemeId)
    {
      return TryDecode(CimGuidScope.Equipment, id, out schemeId, out _, out _);
    }


    private static bool TryDecode(CimGuidScope scope, Guid id, out int value1, out int value2, out int value3)
    {
      value1 = value2 = value3 = 0;

      var bytes = id.ToByteArray();
      if (bytes[0] != (byte)scope)
      {
        return false;
      }
      value1 = BitConverter.ToInt32(bytes, 1);
      value2 = BitConverter.ToInt32(bytes, 5);
      value3 = BitConverter.ToInt32(bytes, 9);
      return true;
    }
  }
}