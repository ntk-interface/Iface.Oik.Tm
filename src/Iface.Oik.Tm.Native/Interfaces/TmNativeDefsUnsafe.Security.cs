using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces;

internal static partial class TmNativeDefsUnsafe
{
  public const int AccessRightTextSize = 128;
  public const int AccessRightSize     = 260;
  
  [StructLayout(LayoutKind.Sequential)]
  public unsafe struct AccessRight
  {
    public       uint Mask;
    public fixed byte Eng[AccessRightTextSize];
    public fixed byte Rus[AccessRightTextSize];
  }

  [StructLayout(LayoutKind.Sequential)]
  public unsafe struct AccessDescription
  {
    public fixed byte Eng[AccessRightTextSize];
    public fixed byte Rus[AccessRightTextSize];
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct AccessMapping
  {
    public uint GenericRead;
    public uint GenericWrite;
    public uint GenericExecute;
    public uint GenericAll;
  }

  [StructLayout(LayoutKind.Sequential)]
  public unsafe struct CfsAccessDescriptor
  {
    public uint ResourceType;

    // AccessRight[32]
    public fixed byte Bit[AccessRightSize * 32];

    // AccessRight[32]
    public fixed byte Cplx[AccessRightSize * 32];

    // AccessRight[32]
    public fixed byte Audit[AccessRightSize * 32];
    
    public AccessDescription Default;
    public AccessDescription ObjTypeName;
    public AccessMapping     GenericMapping;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public fixed byte NamePrefix[AccessRightTextSize];
  }

  [StructLayout(LayoutKind.Sequential)]
  public unsafe struct CfsExtSrvrtDescriptor
  {
    public byte DoUserID;
    public uint MaxUserID;
    public byte DoGroup;
    public byte DoKeyID;
    public byte DoUserNick;
    public byte DoUserPwd;
    public nint Rights;

    public fixed uint Reserved[512];
  }
}