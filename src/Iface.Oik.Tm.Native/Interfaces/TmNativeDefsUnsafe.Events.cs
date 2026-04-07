using System;
using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces;

public static partial class TmNativeDefsUnsafe
{
  public const int TEventDateTimeSize = 24;
  public const int TEventUserNameSize = 16;

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

    public nint DataPtr;
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
    public nint   Next;
    public uint   EventSize;
    public TEvent Event;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TTMSElix
  {
    public ulong R;
    public ulong M;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct TTMSEventAddData
  {
    public TTMSElix Elix;
    public UInt32   AckSec;
    public UInt16   AckMs;

    public fixed byte UserName[TEventUserNameSize];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct StatusData
  {
    public byte   State;
    public byte   Class;
    public uint   ExtSig;
    public byte   ResCh;
    public byte   ResRtu;
    public ushort ResPoint;
    public uint   FixUT;
    public ushort S2;
    public uint   Flags;
    public ushort FixMS;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct StatusDataEx
  {
    public byte   State;
    public byte   Class;
    public uint   ExtSig;
    public byte   ResCh;
    public byte   ResRtu;
    public ushort ResPoint;
    public uint   FixUT;
    public ushort S2;
    public uint   Flags;
    public ushort FixMS;
    public uint   OldFlags;

    public fixed byte UserName[TEventUserNameSize];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct AlarmData
  {
    public float  Val;
    public ushort AlarmID;
    public byte   State;
  }

  public const int ControlDataDummyBytes = 2026;

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct ControlData // параметры выданного телеуправления
  {
    public byte   Ch;
    public byte   Rtu;
    public ushort Point;
    public byte   Cmd;    // выданная команда
    public byte   Result; // == SUCCESS если ТУ успешно, иначе FAILURE

    public fixed byte UserName[TEventUserNameSize]; // пользователь, выдавший ТУ
  }

  [StructLayout(LayoutKind.Sequential)]
  public unsafe struct AcknowledgeData
  {
    public ushort TmType;
    public ushort Res1;
    public ushort Res2;

    public fixed byte UserName[TEventUserNameSize];
  }

  public const int AnalogSetDataDummyBytes = 2027;

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct AnalogSetData
  {
    public float Value;
    public byte  Cmd; // выданная команда

    public fixed byte UserName[16];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct StrBinData
  {
    public uint Source;

    public fixed byte StrBin[2044];
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct FlagsChangeData
  {
    public ushort TmType;
    public uint   OldFlags;
    public uint   NewFlags;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct FlagsChangeDataStatus
  {
    public ushort TmType;
    public uint   OldFlags;
    public uint   NewFlags;
    public byte   State;
    public byte   S2;
    public uint   Reserved;
    public uint   Reserved1;

    public fixed byte UserName[TEventUserNameSize];
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct FlagsChangeDataAnalog
  {
    public ushort TmType;
    public uint   OldFlags;
    public uint   NewFlags;
    public ushort AsCode;
    public float  AsFloat;
    public uint   Reserved;

    public fixed byte UserName[TEventUserNameSize];
  }
}