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
      Status           = 0x8000,
      Analog           = 0x8001,
      Accum            = 0x8002,
      Control          = 0x8003,
      Channel          = 0x9000,
      Rtu              = 0x9001,
      RetroStatus      = 0x9010,
      RetroAnalog      = 0x9011,
      RetroAccum       = 0x9012,
      AnalogAlarm      = 0x9021,
      StatusGroup      = 0xC321,
      AnalogGroup      = 0xC322,
      AccumGroup       = 0xC323,
      StatusRetroGroup = 0xD331
    }


    [Flags]
    public enum Flags
    {
      UnreliableHdw     = 0x0001,
      UnreliableManu    = 0x0002,
      Requested         = 0x0004,
      ManuallySet       = 0x0008,
      LevelA            = 0x0010,
      LevelB            = 0x0020,
      LevelC            = 0x0040,
      LevelD            = 0x0080,
      Inverted          = 0x0100,
      ResChannel        = 0x0200,
      TmCtrlPresent     = 0x0400,
      HasAlarm          = 0x0400,
      StatusClassAps    = 0x0800,
      AnalogCtrlPresent = 0x0800,
      TmStreaming       = 0x1000,
      Abnormal          = 0x2000,
      Unacked           = 0x4000,
      IV                = 0x8000,
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
      FlagsChange     = 0x0040,
      Res2            = 0x0080,
      ExtLink         = 0x2000, // Служебное значение - не использовать!
      ExtFileLink     = 0x4000, // Служебное значение - не использовать!
      Extended        = 0x8000, // Расширенный формат
    }


    [Flags]
    public enum ExtendedEventTypes
    {
      Message = 0x0100,
      Model   = 0x0200
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
      NewClient  = 0x2000
    }

    public enum ServerCap
    {
      Seqst       = 0,
      AlrNew      = 1,
      Comtrade    = 2,
      EvLogArch   = 3,
      SFbIEx      = 4,
      FbFlags     = 5,
      Users       = 6,
      EvlExt      = 7,
      LogAudit    = 8,
      ResValEx    = 9,
      MicroSeries = 10,
      CfgT        = 120,
      Cnt         = 127,
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


    public enum EventLogExtendedKind
    {
      StrBin = 0x100,
      Model  = 0x200
    }


    public enum EventLogExtendedSources
    {
      Server     = 0,
      Comtrade   = 1,
      Omp        = 2,
      AutoSect   = 3,
      I850       = 50,
      BlackBox   = 90,
      Iec101     = 101,
      Aura       = 102,
      Iec103     = 103,
      Spa        = 105,
      Modbus     = 106,
      Dnp3       = 107,
      Dlms       = 108,
      TmaRelated = 109
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


    [Flags]
    public enum ExtendedDataSignatureFlag : UInt32
    {
      Reserve   = 0x01,
      FixTime   = 0x02,
      S2        = 0x04,
      Secondary = 0x08,
      TmFlags   = 0x10,
      CurData   = 0x20
    }


    [Flags]
    public enum TmsTraceFlags
    {
      Error   = 0x0001,
      Message = 0x0002,
      Debug   = 0x0004,
      In      = 0x0008,
      Out     = 0x0010
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


    [Flags]
    public enum ServerInfoPresenceFlags
    {
      UniqUserCount         = 0x01,
      TmValueCount          = 0x02,
      ReserveBufFill        = 0x04,
      ReserveBufMaxFill     = 0x08,
      TmTotValCnt           = 0x10,
      ReserveSentAsyncBytes = 0x20,
      ExtValCount           = 0x40,
      ReserveBufferSize     = 0x80
    }


    [Flags]
    public enum NewUserSystem
    {
      Available           = 0x0001,
      Certificate         = 0x0002,
      SpecialUser         = 0x0004,
      OwnUser             = 0x0008,
      ChangePassword      = 0x0100,
      SecurityLog         = 0x0200,
      UserTemplates       = 0x0400,
      AdminChangePassword = 0x0800,
    }


    [Flags]
    public enum PublicationQoS
    {
      AtMostOnce    = 0x00,
      AtLeastOnce   = 0x01,
      ExactlyOnce   = 0x02,
      NoLocal       = 0x04,
      RetainAsPub   = 0x08,
      RetainFirst   = 0x10,
      RetainNo      = 0x20,
      RetainDefault = 0x00
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
    public struct TAccumPoint
    {
      public Single Value;
      public Single Load;
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

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2048)]
      public byte[] Data;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TEventHeader
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
      public byte[] DateTime; // время события в формате ДД.ММ.ГГГГ ЧЧ:ММ:СС.cc

      public UInt16 Imp; // уровень важности
      public UInt16 Id;  // тип события
      public UInt16 Ch;
      public UInt16 Rtu;
      public UInt16 Point;
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
    public struct TEventElixHeader
    {
      public IntPtr   Next;
      public TTMSElix Elix;
      public UInt32   EventSize;
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
    public struct StatusData
    {
      public byte   State;
      public byte   Class;
      public UInt32 ExtSig;
      public byte   ResCh;
      public byte   ResRtu;
      public UInt16 ResPoint;
      public UInt32 FixUT;
      public UInt16 S2;
      public UInt32 Flags;
      public UInt16 FixMS;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct StatusDataEx
    {
      public byte   State;
      public byte   Class;
      public UInt32 ExtSig;
      public byte   ResCh;
      public byte   ResRtu;
      public UInt16 ResPoint;
      public UInt32 FixUT;
      public UInt16 S2;
      public UInt32 Flags;
      public UInt16 FixMS;
      public UInt32 OldFlags;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] UserName;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct AlarmData
    {
      public float  Val;
      public UInt16 AlarmID;
      public byte   State;
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

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2026)]
      public byte[] DummyBytes; // пустой массив нужен для корректного маршалинга в TEvent.Data
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct AcknowledgeData
    {
      public UInt16 TmType;
      public UInt16 Res1;
      public UInt16 Res2;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] UserName;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct AnalogSetData
    {
      public Single Value;
      public Byte   Cmd; // выданная команда

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] UserName;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2027)]
      public byte[] DummyBytes; // пустой массив нужен для корректного маршалинга в TEvent.Data
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct StrBinData
    {
      public UInt32 Source;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2044)]
      public byte[] StrBin;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct FlagsChangeData
    {
      public UInt16 TmType;
      public UInt32 OldFlags;
      public UInt32 NewFlags;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct FlagsChangeDataStatus
    {
      public UInt16 TmType;
      public UInt32 OldFlags;
      public UInt32 NewFlags;
      public Byte   State;
      public Byte   S2;
      public UInt32 Reserved;
      public UInt32 Reserved1;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] UserName;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct FlagsChangeDataAnalog
    {
      public UInt16 TmType;
      public UInt32 OldFlags;
      public UInt32 NewFlags;
      public UInt16 AsCode;
      public Single AsFloat;
      public UInt32 Reserved;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] UserName;
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
    public struct TValueAndFlagsUnion
    {
      public TAdrTm      Adr;
      public Byte        Type; // см. VfType
      public Byte        Flags;
      public Byte        Bits;
      public TValueUnion Value;
    }


    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
    public struct TValueUnion
    {
      [FieldOffset(0)] public byte   Uchar;
      [FieldOffset(0)] public char   Schar;
      [FieldOffset(0)] public UInt16 Ushort;
      [FieldOffset(0)] public Int16  Sshort;
      [FieldOffset(0)] public UInt32 Ulong;
      [FieldOffset(0)] public Int32  Slong;
      [FieldOffset(0)] public Single Flt;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TTimedValueAndFlags
    {
      public TValueAndFlags Vf;
      public TMXTime        Xt;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TTimedValueAndFlagsUnion
    {
      public TValueAndFlagsUnion Vf;
      public TMXTime             Xt;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TAlertListId
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] IData;
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
    public struct TM_AAN_STATS
    {
      public UInt32 Ok;
      public UInt32 LastMcMs;
      public UInt32 LastHcMs;
      public UInt32 LastDcMs;
      public UInt32 Megabytes;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TMSAnalogMSeries
    {
      public UInt16 Interval;
      public UInt16 Count;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
      public TMSAnalogMSeriesElement[] Elements;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TMSAnalogMSeriesElement
    {
      public Single Value;
      public UInt32 Ut;
      public Byte   SFlg; // 1 - present, 2 - unreliable
    }


    public const int TAnalogTechParmsAlarmSize     = 4;
    public const int TAnalogTechParamsReservedSize = 64 - 8;


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TAnalogTechParms
    {
      public Single Nominal;
      public Single MinVal;
      public Single MaxVal;
      public Byte   AlrPresent;
      public Byte   AlrInUse;
      public Byte   AlrId;
      public Byte   Reserved1;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = TAnalogTechParmsAlarmSize)]
      public Single[] ZoneLim;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = TAnalogTechParamsReservedSize)]
      public UInt32[] Reserved;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TUserInfo
    {
      public Boolean Valid;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] UserName;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] UserComment;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
      public byte[] NtUserName;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
      public byte[] NtUserDomain;

      public UInt32 DatagramMask;
      public UInt32 AccessMask;
      public UInt32 ConnectTime;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] UserCategory;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] OldUserName;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] Reserved;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TExtendedUserInfo
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


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IfaceUser
    {
      public UInt32 Signature;
      public UInt32 Unique;
      public UInt32 Thid;
      public UInt32 Pid;
      public UInt32 Flags;
      public UInt32 DbgCnt;
      public UInt32 LoudCnt;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] Name;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] Comment;

      public UInt64 BytesIn;
      public UInt64 BytesOut;
      public UInt32 Handle;

      public Int32 CreationTime;
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
    public struct TTMSEventAddData
    {
      public TTMSElix Elix;
      public UInt32   AckSec;
      public UInt16   AckMs;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
      public string UserName;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TServerInfo
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public string Description;

      public UInt32 DwType;
      public UInt32 DwHeapUsage;
      public UInt32 DwWSMin;
      public UInt32 DwWSMax;

      public UInt32 HandleCount;
      public UInt32 StartTime;
      public UInt32 ConfChangeTime;
      public UInt32 ThreadCount;

      public UInt32 UserCount;
      public UInt32 LogonCount;

      public UInt32 PresenceFlags;

      public UInt32 UniqUserCount;
      public UInt32 TmValueCount;

      public UInt32 ReserveBufFill;
      public UInt32 ReserveBufMaxFill;
      public UInt32 TmTotValCnt;
      public UInt32 ReserveSentAsyncBytes;
      public UInt32 TmExtValueCnt;
      public UInt32 DtmxLastCommit;
      public UInt32 DtmxBufFill;
      public UInt32 AnRW;
      public UInt32 ReserveBufSize;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 72)]
      public byte[] Reserved;
    }


    public struct TRetransInfo
    {
      public UInt32 Id;
      public UInt16 Type;
      public TAdrTm AdrTm;
    }


    public struct TRetransInfoReply
    {
      public Byte   Ok;
      public Byte   PresenceFlags;
      public UInt16 Group;
      public UInt16 Class;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public byte[] Reserved2;
    }


    public struct TRetroInfoEx
    {
      public UInt16 Type;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string Name;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
      public string Description;

      public UInt32 Period;
      public UInt32 Capacity;
      public UInt32 Start;
      public UInt32 Stop;
      public UInt32 RecCount;
      public UInt32 Flags;
      public byte   Version;
      public char   ActiveFile;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
      public byte[] BRes;

      public UInt32 AppendTicks;
      public UInt32 SizeMb;
      public UInt32 LastRecSize;
      public UInt32 MaxMb;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
      public UInt32[] Reserved;
    }

    public const Int16  RealTelemetryFlag     = unchecked((short)0x8000);
    public const UInt32 ExtendedDataSignature = 0xEEAAEE00;

    public const UInt32 ExtendedStatusChangedEventSize = 76;

    #region Cfs

    public const UInt32 FailIfNoConnect            = 0x80000000;
    public const string DefaultMasterConfFile      = "_master_";
    public const byte   MasterServiceStatusCommand = 0;
    public const byte   StartMasterServiceCommand  = 1;
    public const byte   StopMasterServiceCommand   = 2;
    public const string DefaultHotStanbyConfFile   = "reserve.cfg";

    public enum CfsIitgk : UInt32
    {
      Avail = 0,
      Exe   = 1,
      All   = 2,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileTime
    {
      public Int32 dwLowDateTime;
      public Int32 dwHighDateTime;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CfsLogRecord
    {
      public string Time;
      public string Date;
      public string Name;
      public string Type;
      public string MsgType;
      public string ThreadId;
      public string Message;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CfsFileProperties
    {
      public FileTime CreationTime;
      public FileTime ModificationTime;
      public UInt32   Attributes;
      public UInt32   Checksum;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
      public UInt32[] Reserved;
    }


    public enum SLogTag
    {
      Type            = '\x1',
      Body            = '\x2',
      User            = '\x3',
      Time            = '\x4',
      Index           = '\x5',
      Source          = '\x6',
      ThreadId        = '\x7',
      SessionId       = '\x8',
      FileIndex       = '\x9',
      InformationType = '\xa'
    }


    public enum SLogType : UInt32
    {
      Security      = 0,
      Administrator = 1
    }


    public enum SLogDirection : UInt32
    {
      FromStart = 0x00_00_00_00,
      FromEnd   = 0x7f_ff_ff_ff
    }

    #endregion


    #region Delta

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaCommon
    {
      public Byte Type;
      public Byte Length;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaStatus
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public Byte Value;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaStatus2
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public Byte Value;
      public Byte HiNum;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaAnalog
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public Int32 Value;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaAnalog2
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public Int32 Value;
      public Byte  HiNum;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaAnalogF
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public Single Value;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaAnalogF2
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public Single Value;
      public Byte   HiNum;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaAccum
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public Int32 Value;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaAccum2
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public Int32 Value;
      public Byte  HiNum;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaAccumF
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public Single Value;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaAccumF2
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public Single Value;
      public Byte   HiNum;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaControl
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public UInt16 CtrlBlock;
      public UInt16 CtrlGroup;
      public UInt16 CtrlPoint;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaControl2
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      public UInt16 CtrlBlock;
      public UInt16 CtrlGroup;
      public UInt16 CtrlPoint;
      public Byte   HiNum;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaSliceTime
    {
      public Byte   Type;
      public Byte   Length;
      public UInt64 Current;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DeltaDescription
    {
      public Byte Type;
      public Byte Length;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
      public byte[] Text;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DeltaStrval
    {
      public Byte   Type;
      public Byte   Length;
      public UInt16 Number;
      public Int32  LastUpdate;
      public UInt16 DeltaFlags;
      public UInt16 TmsChn;
      public UInt16 TmsRtu;
      public UInt16 TmsPoint;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
      public byte[] S;
    }


    public enum DeltaItemTypes
    {
      Description = 0,
      Time        = 1,
      Status      = 2,
      Analog      = 3,
      Accum       = 4,
      Control     = 5,
      AnalogF     = 6,
      AccumF      = 7,
      StrVal      = 8
    }

    [Flags]
    public enum DeltaItemsFlags
    {
      Reliable = 0x0001,
      ZeroEnum = 0x0002,
      DestReli = 0x0004,
      DestVal  = 0x0008,
      Hex      = 0x0010,
      Group8   = 0x0020,
      NPrsnt   = 0x0040,
      BinVal   = 0x0080,
      Writable = 0x0100,
      CtlValue = 0x0200,
      S2Break  = 0x0400,
      S2Malfn  = 0x0800,
      Analong  = 0x1000
    }


    [Flags]
    public enum DeltaTraceFlags : uint
    {
      Drv = 1,
      Usr = 2
    }

    public enum DntTraceMessageTypes
    {
      Error = 0,
      Msg   = 1,
      Debug = 2,
      TmIn  = 3,
      TmOut = 4,
      SIn   = 5,
      SOut  = 6,
    }

    #endregion
  }
}