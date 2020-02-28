using System;
using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public delegate void TmNativeCallback(Int32 sizeInBytes, IntPtr buffer, IntPtr param);


  public class TmNativeDefs
  {
    public const Int16 Success = 1;
    public const Int16 Failure = 0;


    public enum TmDataTypes 
    {
      Status      = 0x8000,
      Analog      = 0x8001,
      Accum       = 0x8002,
      Control     = 0x8003,
      Channel     = 0x9000,
      Rtu         = 0x9001,
      RetroStatus = 0x9010,
      RetroAnalog = 0x9011,
      RetroAccum  = 0x9012,
    }


    [Flags]
    public enum Flags
    {
      UnreliableHdw       = 0x0001,
      UnreliableManu      = 0x0002,
      Requested           = 0x0004,
      ManuallySet         = 0x0008,
      LevelA              = 0x0010,
      LevelB              = 0x0020,
      LevelC              = 0x0040,
      LevelD              = 0x0080,
      Inverted            = 0x0100,
      ResChannel          = 0x0200,
      TmCtrlPresent       = 0x0400,
      HasAlarm            = 0x0400,
      StatusClassAps      = 0x0800,
      AnalogCtrlPresent   = 0x0800,
      TmStreaming         = 0x1000,
      Abnormal            = 0x2000,
      Unacked             = 0x4000,
      IV                  = 0x8000,
    }


    [Flags]
    public enum S2Flags
    {
      Break       = 0x0001,
      Malfunction = 0x0002,
      Interim     = 0x4000,
    }


    [Flags]
    public enum EventTypes
    {
      StatusChange    = 0x0001,
      Alarm           = 0x0002,
      Control         = 0x0004,
      Acknowledge     = 0x0008,
      ManualStatusSet = 0x0010,
      ManualAnalogSet = 0x0020,
      Res1            = 0x0040,
      Res2            = 0x0080,
      ExtLink         = 0x2000, // Служебное значение - не использовать!
      ExtFileLink     = 0x4000, // Служебное значение - не использовать!
      Extended        = 0x8000, // Расширенный формат
    }


    public enum VfType
    {
      Status           = 0x10,
      AnalogCode       = 0x20,
      AnalogFloat      = 0x30,
      AccumValue       = 0x40,
      AccumCode        = 0x50,
      AccumIncrement   = 0x60,
      Telecontrol      = 0x70,
      AccumFloat       = 0x80,
      AccumDirect      = 0x90,
      AccumFloatDirect = 0xa0,
      StatusAck        = 0xb0,
      AnalogFloatcode  = 0xc0,

      FlagIgnore = 0x00,
      FlagSet    = 0x01,
      FlagClear  = 0x02,
      FlagCopy   = 0x03,

      Signed         = 0x04,
      AlwaysSetValue = 0x08,
    }


    [Flags]
    public enum TMXTimeFlags
    {
      Greenwich = 0x01,
      Curdata   = 0x02,
      IV        = 0x04,
      Zimp      = 0x08,
      User      = 0x10,
      ManRtr    = 0x20,
    }


    [Flags]
    public enum DatagramFlags
    {
      DataSource = 0x0001,
      TraceAll   = 0x0002,
      TraceDef   = 0x0004,
      TmNotify   = 0x0008,
      ExtsShowS2 = 0x0100,
      TobChange  = 0x0200,
      Calc       = 0x0400,
    }


    public enum DriverCall
    {
      QAllTs      = 0x003,
      QTit        = 0x001b,
      MakeTu      = 0x0004,
      Acknowledge = 0x001d,
      AckAnalog   = 0x0021,
      AckAccum    = 0x0022
    }


    [Flags]
    public enum ImpulseArchiveFlags
    {
      Mom = 0, // мгновенное
      Avg = 1, // среднее
      Min = 2, // минимальное
      Max = 3, // максимальное
    }


    [Flags]
    public enum ImpulseArchiveQueryFlags
    {
      Mom = 0x01, // мгновенное
      Avg = 0x02, // среднее
      Min = 0x04, // минимальное
      Max = 0x08, // максимальное
    }


    [Flags]
    public enum TmCpf // для запросов тэгов по маске флагов
    {
      Name     = 0x01,
      AllFlags = 0x02,
      St0      = 0x04,
      St1      = 0x08,
      SkipRes  = 0x10,
    }


    public enum AnalogRegulationFlag
    {
      None  = 0x00,
      Step  = 0x20,
      Code  = 0x40,
      Value = 0x60,
    }


    public enum AnalogRegulationType
    {
      Step  = 1,
      Code  = 2,
      Value = 3,
    }


    public enum TrsSrv
    {
      Starting = 0,
      Running  = 1,
      Stopping = 2,
    }


    public enum TofWr // тип занесения свойств техобъектов (tmcTechObjWriteValues)
    {
      Repl = 0, // реплика - все неуказанные свойства подтираются
      Addt = 1, // аддитивность - все неуказанные свойства остаются нетронутыми
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TAdrTm
    {
      public Int16 Ch;
      public Int16 RTU;
      public Int16 Point;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TCommonPoint
    {
      [MarshalAs(UnmanagedType.LPStr)] public string name;
      public                                  Byte   cp_flags;
      public                                  Byte   res1;
      public                                  UInt16 Type;
      public                                  UInt16 Ch;
      public                                  UInt16 RTU;
      public                                  UInt16 Point;
      public                                  UInt32 TM_Flags;
      public                                  UInt16 tm_s2;
      public                                  UInt16 tm_flags2;
      public                                  UInt32 tm_local_ut;
      public                                  UInt32 tm_remote_ut;
      public                                  UInt16 tm_local_ms;
      public                                  UInt16 tm_remote_ms;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
      public byte[] Data;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TStatusPoint
    {
      public Int16 Status;
      public Int16 Flags;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TAnalogPoint
    {
      public Single AsFloat;
      public Int16  AsCode;
      public Int16  Flags;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
      public byte[] Unit; // в tmconn 8, в последнем байте формат

      public byte Format; // вынесли формат отдельно
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TAnalogPointShort
    {
      public Single Value;
      public Int16  Flags;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TEvent
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
      public byte[] DateTime; // время события в формате ДД.ММ.ГГГГ ЧЧ:ММ:СС.cc

      public UInt16 Imp; // уровень важности
      public UInt16 Id;  // тип события
      public UInt16 Ch;
      public UInt16 Rtu;
      public UInt16 Point;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
      public byte[] Data;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TTMSElix
    {
      public UInt64 R;
      public UInt64 M;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TEventElix
    {
      public IntPtr   Next;
      public TTMSElix Elix;
      public UInt32   EventSize;
      public TEvent   Event;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TAlarm
    {
      public UInt16 Point;
      public Byte   Rtu;
      public Byte   Ch;
      public Byte   GroupId;
      public Byte   AlarmId;
      public Single Value;
      public UInt16 SignSensibilityActiveInuseImportance; // разбито по битовым полям :1 :7 :2 :2 :4
      public UInt16 Period;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
      public Byte[] DayMap;

      public Byte WeekMap;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
      public Byte[] YearMap;

      public Byte   InDirect;
      public UInt16 CountDown;
      public Double Sum;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TTechObj
    {
      public byte   TobFlg;
      public UInt32 Scheme;
      public UInt16 Type;
      public UInt32 Object;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TTechObjProps
    {
      public byte   TobFlg;
      public UInt32 Scheme;
      public UInt16 Type;
      public UInt32 Object;

      public IntPtr Props;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ControlData // параметры выданного телеуправления
    {
      public Byte   Ch;
      public Byte   Rtu;
      public UInt16 Point;
      public Byte   Cmd;    // выданная команда
      public Byte   Result; // == SUCCESS если ТУ успешно, иначе FAILURE

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] UserName; // пользователь, выдавший ТУ
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct AnalogSetData
    {
      public Single Value;
      public Byte   Cmd; // выданная команда

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] UserName;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
      public byte[] DummyBytes;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TMXTime
    {
      public UInt32 Sec;
      public UInt16 Ms;
      public UInt16 Flags; // см. TMXTimeFlags
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TValueAndFlags
    {
      public TAdrTm Adr;
      public Byte   Type; // см. VfType
      public Byte   Flags;
      public Byte   Bits;
      public UInt32 Value; // тут в библиотеке union от Byte до UInt32 и float
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TTimedValueAndFlags
    {
      public TValueAndFlags Vf;
      public TMXTime        Xt;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TMAAN_ARCH_VALUE
    {
      public Byte   Tag;
      public Byte   AavFlag;
      public UInt32 Ut;
      public UInt16 Ms;
      public UInt32 Flags;
      public Single Value;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TMAAN_SR_DATA
    {
      public UInt16 Ms;
      public Single Value;
      public UInt32 Flags;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TMAAN_MIN_DATA
    {
      public Byte   Legend;
      public Byte   MdFlg;
      public Single VMin;
      public Single VMax;
      public Single VLast;
      public UInt16 VAvg;
      public UInt32 VTmFlg;
      public UInt32 series;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TUserInfo
    {
      public Int32 RecNum;
      public Int32 UserId;
      public Byte  Group;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] KeyId;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] UserName;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      public byte[] UserPwd;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 250)]
      public byte[] Rights;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IfaceServer
    {
      public UInt32 Signature;
      public UInt32 Unique;
      public UInt32 Pid;
      public UInt32 Ppid;
      public UInt32 Flags;
      public UInt32 DbgCnt;
      public UInt32 LoudCnt;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] Name;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] Comment;


      public UInt64 BytesIn;
      public UInt64 BytesOut;
      public UInt32 State;
      public Int32  CreationTime;
      public UInt32 ResState;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct DomainInfoS
    {
      public UInt32 PrimaryDomainError;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
      public string PrimaryDomainName;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
      public byte[] PrimaryDomainSid;

      public UInt32 AccountDomainError;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
      public string AccountDomainName;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
      public byte[] AccountDomainSid;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ComputerInfoS
    {
      public UInt32 Len;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public string ComputerName;

      public UInt32 NtVerMaj;
      public UInt32 NtVerMin;
      public UInt32 NtBuild;
      public UInt32 Acp;
      public UInt64 Uptime;
      public Byte   UptimeAbs;
      public Byte   NtProductType;
      public Byte   Win64;
      public Byte   UdbType;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      public Byte[] LOctet;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public UInt32[] Reserved;

      public UInt32 CfsVerMaj;
      public UInt32 CfsVerMin;

      [MarshalAs(UnmanagedType.Struct)] // todo test
      public DomainInfoS DomInfo;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public string UserName;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public string Res2;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public string UserAddr;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public string Res3;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      public UInt32[] IpAddrs;

      public UInt32 AccessMask;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TraceItemStorage
    {
      public IntPtr hMap;
      public IntPtr pData;
    }
    
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileTime
    {
      public Int32 dwLowDateTime;
      public Int32 dwHighDateTime;
    }
  }
}