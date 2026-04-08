using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces;

internal static partial class TmNativeDefsUnsafe
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TUserInfo
  {
    public int Valid; // BOOL (4 bytes!)

    public fixed byte UserName[16];
    public fixed byte UserComment[64];
    public fixed byte NtUserName[32];
    public fixed byte NtUserDomain[32];

    public uint DatagramMask;
    public uint AccessMask;
    public uint ConnectTime;

    public fixed byte UserCategory[64];
    public fixed byte OldUserName[16];
    public fixed byte Reserved[16];
  }

  public const int TExtendedUserInfoRightsSize = 250;
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TExtendedUserInfo
  {
    public int RecNum;
    public int UserId;
    public byte Group;

    public fixed byte KeyId[16];
    public fixed byte UserName[16];
    public fixed byte UserPwd[8];
    public fixed byte Rights[TExtendedUserInfoRightsSize];
  }
}