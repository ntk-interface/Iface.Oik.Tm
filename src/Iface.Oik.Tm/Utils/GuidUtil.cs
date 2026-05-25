using System;
using System.Buffers.Binary;
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
      Span<byte> bytes = stackalloc byte[16];
    
      bytes[0] = (byte) scope;

      BinaryPrimitives.WriteInt32LittleEndian(bytes[1..5], value1);
      BinaryPrimitives.WriteInt32LittleEndian(bytes[5..9], value2);
      BinaryPrimitives.WriteInt32LittleEndian(bytes[9..13], value3);

      return new Guid(bytes);
    }


    public static bool TryDecodeCimEquipment(Guid id, out int schemeId, out int objectId)
    {
      return TryDecode(CimGuidScope.Equipment, id, out schemeId, out objectId, out _);
    }


    public static bool TryDecodeCimSubstation(Guid id, out int schemeId)
    {
      return TryDecode(CimGuidScope.Substation, id, out schemeId, out _, out _);
    }


    private static bool TryDecode(CimGuidScope scope, Guid id, out int value1, out int value2, out int value3)
    {
      value1 = value2 = value3 = 0;

      Span<byte> bytes = stackalloc byte[16];
      id.TryWriteBytes(bytes);
      
      if (bytes[0] != (byte)scope)
      {
        return false;
      }
      value1 = BinaryPrimitives.ReadInt32LittleEndian(bytes[1..5]);
      value2 = BinaryPrimitives.ReadInt32LittleEndian(bytes[5..9]);
      value3 = BinaryPrimitives.ReadInt32LittleEndian(bytes[9..13]);
      
      return true;
    }
  }
}