using System;
using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces;

public static class TmNativeDefsUnsafe
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

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TExtendedUserInfo
  {
    public int RecNum;
    public int UserId;
    public byte Group;

    public fixed byte KeyId[16];
    public fixed byte UserName[16];
    public fixed byte UserPwd[8];
    public fixed byte Rights[250];
  }


  public const int TEventDateTimeSize = 24;
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TEventHeader
  {
    public fixed byte DateTime[TEventDateTimeSize]; // время события в формате ДД.ММ.ГГГГ ЧЧ:ММ:СС.cc

    public ushort Imp; // уровень важности
    public ushort Id;  // тип события
    public ushort Ch;
    public ushort Rtu;
    public ushort Point;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
  public unsafe struct TEvent
  {
    public fixed byte DateTime[TEventDateTimeSize]; // время события в формате ДД.ММ.ГГГГ ЧЧ:ММ:СС.cc

    public ushort Imp; // уровень важности
    public ushort Id;  // тип события
    public ushort Ch;
    public ushort Rtu;
    public ushort Point;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2048)]
    public fixed byte  Data[2048];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TEventExHeader
  {
    public nint Next;
    public uint EventSize;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TEventEx
  {
    public nint Next;
    public uint EventSize;
    public TEvent Event;
  }
  
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TTMSElix
  {
    public UInt64 R;
    public UInt64 M;
  }
  
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TTMSEventAddData
  {
    public TTMSElix Elix;
    public UInt32   AckSec;
    public UInt16   AckMs;
    
    //UserName byte[] неизвестной длинны
  }
}