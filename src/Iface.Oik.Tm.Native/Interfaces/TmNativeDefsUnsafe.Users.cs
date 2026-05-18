using System;
using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces;

internal static partial class TmNativeDefsUnsafe
{
  [Flags]
  public enum PasswordPolicies : uint
  {
    Upper       = 0x00001,
    Digits      = 0x00002,
    Spec        = 0x00004,
    CheckRepeat = 0x00008,
    CheqSeq     = 0x00010,
    CheckDict   = 0x00020,
    CheckCache  = 0x10000,
    
    Undefined = 0xff_ff_ff_ff
  }

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
  public const int MaxPwdLen = 64;

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TExtendedUserInfo
  {
    public int  RecNum; // not used
    public int  UserId; // 1-255
    public byte Group;

    public fixed byte KeyId[16];
    public fixed byte UserName[16];
    public fixed byte UserPwd[8]; // not used?
    public fixed byte Rights[TExtendedUserInfoRightsSize];

    public fixed byte UserNameLong[MaxPwdLen];
    public fixed byte UserPwdLong[MaxPwdLen];
  }
}