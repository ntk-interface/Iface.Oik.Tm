using System;
using System.ComponentModel;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Interfaces
{
  public enum TmType
  {
    Unknown     = 0,
    Status      = 1,
    Analog      = 2,
    Accum       = 3,
    Channel     = 4,
    Rtu         = 5,
    StatusGroup = 11,
    AnalogGroup = 12,
    AccumGroup  = 13,
    RetroStatus = 21,
    RetroAnalog = 22,
    RetroAccum  = 23,
  }


  [Flags]
  public enum TmFlags : uint
  {
    None              = 0,
    Unreliable        = TmNativeDefs.Flags.UnreliableHdw,
    ManuallyBlocked   = TmNativeDefs.Flags.UnreliableManu,
    Requested         = TmNativeDefs.Flags.Requested,
    ManuallySet       = TmNativeDefs.Flags.ManuallySet,
    LevelA            = TmNativeDefs.Flags.LevelA,
    LevelB            = TmNativeDefs.Flags.LevelB,
    LevelC            = TmNativeDefs.Flags.LevelC,
    LevelD            = TmNativeDefs.Flags.LevelD,
    Inverted          = TmNativeDefs.Flags.Inverted,
    ResChannel        = TmNativeDefs.Flags.ResChannel,
    TmCtrlPresent     = TmNativeDefs.Flags.TmCtrlPresent,
    HasAlarm          = TmNativeDefs.Flags.HasAlarm,
    StatusAps         = TmNativeDefs.Flags.StatusClassAps,
    HasTeleregulation = TmNativeDefs.Flags.AnalogCtrlPresent,
    TmStreaming       = TmNativeDefs.Flags.TmStreaming,
    Abnormal          = TmNativeDefs.Flags.Abnormal,
    Unacked           = TmNativeDefs.Flags.Unacked,
    Invalid           = TmNativeDefs.Flags.IV,

    // TODO это служебный набор флагов, не знаю, нужны ли они в клиенте
    HaveARes   = 0x00_01_00_00, // есть резервное значение
    AnUnsigned = 0x00_02_00_00, // заносить как беззнаковое
    NoRetro    = 0x00_04_00_00, // не заносить в ретроспективу или журнал событий
    NoZero     = 0x00_08_00_00, // не обнулять значение при приходе кода 0
    Format     = 0x00_10_00_00, // форматировать значение при занесении
    External   = 0x00_20_00_00, // от внешнего сервера
    Expression = 0x00_40_00_00, // вычисляется по выражению
    CanOutdate = 0x00_80_00_00, // значение может устаревать
    HaveNormal = 0x01_00_00_00, // есть нормальное значение
    Interim    = 0x02_00_00_00,
    Reserved26 = 0x04_00_00_00,
    Reserved27 = 0x08_00_00_00,
    Dtmx       = 0x10_00_00_00,
    Reserved29 = 0x20_00_00_00,
    SfConfig   = 0x40_00_00_00,
    SfInit     = 0x80_00_00_00,
  }


  [Flags]
  public enum TmS2Flags
  {
    None         = 0,
    Break        = TmNativeDefs.S2Flags.Break,
    Malfunction  = TmNativeDefs.S2Flags.Malfunction,
    Intermediate = TmNativeDefs.S2Flags.Interim,
  }


  public enum TmTopologyState
  {
    Unknown       = 0,
    IsNotVoltaged = 1,
    IsVoltaged    = 2,
    IsGrounded    = 4,
  }


  public enum TmTeleregulation
  {
    None  = 0,
    Step  = 1,
    Code  = 2,
    Value = 3,
  }


  [Flags]
  public enum TmAnalogMicroSeriesFlags
  {
    None       = 0,
    IsPresent  = 1,
    IsReliable = 2,
  }


  [Flags]
  public enum TmEventTypes : ushort
  {
    None            = 0,
    StatusChange    = TmNativeDefs.EventTypes.StatusChange,
    Alarm           = TmNativeDefs.EventTypes.Alarm,
    Control         = TmNativeDefs.EventTypes.Control,
    Acknowledge     = TmNativeDefs.EventTypes.Acknowledge,
    ManualStatusSet = TmNativeDefs.EventTypes.ManualStatusSet,
    ManualAnalogSet = TmNativeDefs.EventTypes.ManualAnalogSet,
    FlagsChange     = TmNativeDefs.EventTypes.FlagsChange,
    Res2            = TmNativeDefs.EventTypes.Res2,
    ExtLink         = TmNativeDefs.EventTypes.ExtLink,     // Служебное значение - не использовать!
    ExtFileLink     = TmNativeDefs.EventTypes.ExtFileLink, // Служебное значение - не использовать!
    Extended        = TmNativeDefs.EventTypes.Extended,    // Расширенный формат
    Any             = 0xFFFF,
  }


  [Flags]
  public enum TmEventImportances
  {
    None = 0,
    Imp0 = 1,
    Imp1 = 2,
    Imp2 = 4,
    Imp3 = 8,
    Any  = 0xFF,
    All  = Imp0 | Imp1 | Imp2 | Imp3
  }
  
  


  [Flags]
  public enum TmDatagramFlags
  {
    DataSource = TmNativeDefs.DatagramFlags.DataSource,
    TraceAll   = TmNativeDefs.DatagramFlags.TraceAll,
    TraceDef   = TmNativeDefs.DatagramFlags.TraceDef,
    TmNotify   = TmNativeDefs.DatagramFlags.TmNotify,
    ExtsShowS2 = TmNativeDefs.DatagramFlags.ExtsShowS2,
    TobChange  = TmNativeDefs.DatagramFlags.TobChange,
    Calc       = TmNativeDefs.DatagramFlags.Calc,
    NewClient  = TmNativeDefs.DatagramFlags.NewClient,
  }


  public enum TmPrintLevel
  {
    Debug   = 0,
    Message = 1,
    Error   = 2,
  }


  [Flags]
  public enum TmSecurityAccessFlags : uint
  {
    None         = 0,
    GetTm        = 0x00_00_00_01,
    EditStatuses = 0x00_00_00_02,
    EditAnalogs  = 0x00_00_00_04,
    EditAccums   = 0x00_00_00_08,
    Telecontrol  = 0x00_00_00_10,
    GetRetro     = 0x00_00_00_20,
    GetEvents    = 0x00_00_00_40,
    EditAlarms   = 0x00_00_00_80,
    GetTmSource  = 0x00_00_01_00,
    GetHardware  = 0x00_00_02_00,
    EditTob      = 0x00_00_04_00,
  }


  [Flags]
  public enum TmCommonPointFlags : byte
  {
    None                    = 0, 
    GetWithName             = TmNativeDefs.TmCpf.Name, 
    GetWhenAllFlagsAreValid = TmNativeDefs.TmCpf.AllFlags, 
    GetStatusWithValue0     = TmNativeDefs.TmCpf.St0, 
    GetStatusWithValue1     = TmNativeDefs.TmCpf.St1, 
    SkipReserved            = TmNativeDefs.TmCpf.SkipRes,
  }


  public enum TmUserPermissions
  {
    ReadScheme                  = 0,
    ReadSchemeInGroup           = 1,
    Telecontrol                 = 2,
    SwitchStatusManually        = 3,
    UpdateScheme                = 4,
    UpdateSchemeInGroup         = 5,
    DeleteScheme                = 6,
    DeleteSchemeInGroup         = 7,
    RenameGroup                 = 8,
    RenameGroupInGroup          = 37,
    DeleteGroup                 = 9,
    DeleteGroupInGroup          = 38,
    SoundAlarmStatuses          = 35,
    TelecontrolWithoutKey       = 36,
    UpdateAlarms                = 41,
    SetAnalogManually           = 42,
    BlockManually               = 43,
    TelecontrolInGroup          = 45,
    SwitchStatusManuallyInGroup = 46,
    SetAnalogManuallyInGroup    = 47,
    ReadAllSchemeCatalogs       = 48,
    Ack                         = 49,
    OverrideControlScript       = 50,
    NotExists                   = 1000,
  }


  public enum TmAlarmType
  {
    Analog     = 84,
    Value      = 86,
    Expression = 88,
    Zonal      = 90,
  }


  public enum TmPlacardAction
  {
    [Description("")]           None   = 0,
    [Description("Заземление")] Ground = 1,
    [Description("Расшиновка")] Gap    = 2,
  }


  public enum TobBindingType
  {
    None                                                                          = -1,
    [Description("УСТАРЕЛО")]                             DeprecatedTmTag         = 0,
    [Description("ТЕХ данные")]                           TechParam               = 1,
    [Description("ТЕХ документ")]                         DocumentTech            = 2,
    [Description("ОБР документ")]                         DocumentOperLock        = 3,
    [Description("Осн. сигнал")]                          MainTmStatus            = 4,
    [Description("Осн. измерение")]                       MainTmAnalog            = 5,
    [Description("Ссылка на элемент")]                    ParentTob               = 6,
    [Description("ОБР")]                                  TmOperLock              = 7,
    [Description("ИЗМ")]                                  TmMeasurement           = 8,
    [Description("ОМП")]                                  TmDamageLocation        = 9,
    [Description("ТЕХ")]                                  TmTechParam             = 10,
    [Description("РЗА")]                                  TmRelayProtection       = 11,
    [Description("РЕЖ")]                                  TmRegime                = 12,
    [Description("РЗА документ")]                         DocumentRelayProtection = 13,
    [Description("Документ - контекстное меню")]          DocumentForContextMenu  = 14,
    [Description("Документ - левая кнопка мыши")]         DocumentForClick        = 15,
    [Description("ДЕБЛОК")]                               TmEmergencyDelock       = 16,
    [Description("Запуск программы - контекстное меню")]  ProcessForContextMenu   = 17,
    [Description("Запуск программы - левая кнопка мыши")] ProcessForClick         = 18,
    [Description("ПЗ ОБР")]                               TmGroundOperLock        = 19,
    [Description("ПЗ ОБР документ")]                      DocumentGroundOperLock  = 20,
  }


  public enum TobLogAction
  {
    None    = 0,
    Placard = 1,
    Binding = 2,
  }


  public enum TmTelecontrolValidationResult
  {
    Ok                     = 0,
    StatusHasNoTelecontrol = 1,
    Forbidden              = 2,
    StatusIsUnreliable     = 3,
  }


  public enum TmTeleregulateValidationResult
  {
    Ok                        = 0,
    AnalogHasNoTeleregulation = 1,
    Forbidden                 = 2,
    AnalogIsUnreliable        = 3,
  }


  public enum TmTelecontrolResult
  {
    Success                                                                                                = 1,
    [Description("Неизвестный ТМ-адрес")]                                           InvalidAddress         = 0,
    [Description("Недостаточно ресурсов сервера")]                                  NoResources            = -1,
    [Description("Команда ТУ не может быть отправлена источником телеметрии")]      TmSourceFailed         = -2,
    [Description("Таймаут подтверждения телеуправления")]                           WaitTimeout            = -3,
    [Description("Ошибка связи с источником телеметрии")]                           CannotRedirect         = -4,
    [Description("Нет источника телеметрии")]                                       NoTmSource             = -5,
    [Description("Код ключа не был передан серверу")]                               NoKeyCode              = -6,
    [Description("Неверный код ключа ТУ")]                                          WrongKeyCode           = -7,
    [Description("Таймаут кода ключа ТУ, повторите попытку")]                       KeyCodeTimeout         = -8,
    [Description("Невозможно определить имя пользователя (windows)")]               UserNameUnknown        = -9,
    [Description("Недостаточно прав для выдачи ТУ")]                                AccessDenied           = -10,
    [Description("Функция не поддерживается")]                                      NotSupported           = -11,
    [Description("DELTA: Отсутствует связь с сервером")]                            NoTmServer             = -12,
    [Description("Найден аппаратный ключ, НЕ ЗАПРОГРАММИРОВАННЫЙ должным образом")] WrongKey               = -13,
    [Description("Скрипт запрещает выполнение ТУ")]                                 ScriptError            = -14,
    [Description("Исключение при выполнении ТУ")]                                   Except                 = -15,
    [Description("Оконечное устройство занято другой операцией")]                   Busy                   = -16,
    [Description("Отказано: нет захвата ТУ")]                                       NoControlCapture       = -17,
    [Description("Команда не была отправлена")]                                     CommandNotSentToServer = -100,
  }


  public enum TmPasswordNeedsChangeResult
  {
    Ok = 0,
    NeedsChange = 1,
    NeedsChangeByAdmin = 2,
    Error = 3,
  }


  public enum TmServerLogRecordTypes
  {
    [Description("???")]   Undefined = -1,
    [Description("Any")]   Any       = 0,
    [Description("MSG")]   Msg       = 1,
    [Description("ERROR")] Error     = 2,
    [Description("DEBUG")] Debug     = 3,
  }


  public enum LicenseErrorCodes
  {
    [Description("Неизвестная ошибка при проверке клбча защиты!")]                               Unknown       = -1,
    [Description("Не задан порт ключа защиты!")]                                                 NoComm        = 0,
    [Description("Невозможно открыть порт ключа защиты!")]                                       CannotOpenCom = 1,
    [Description("Ключ защиты не обнаружен!")]                                                   NoIButton     = 2,
    [Description("Ошибка ключа защиты!")]                                                        CannotRead    = 3,
    [Description("ID-файл не соответствует ключу защиты!")]                                      KeyMismatch   = 4,
    [Description("Невозможно создать поток для ключа защиты!")]                                  Thread        = 5,
    [Description("Таймаут освобождения порта ключа защиты!")]                                    Timeout       = 6,
    [Description("Отсутствует или плохой ID-файл ключа защиты!")]                                File          = 7,
    [Description("Недостаточно памяти для проверки ключа защиты!")]                              NoMemory      = 8,
    [Description("Внутренняя ошибка при проверке ключа защиты!")]                                Internal      = 9,
    [Description("Неверная контрольная сумма исполняемого модуля!")]                             Checksum      = 10,
    [Description("Неверная контрольная сумма исполняемого модуля!")]                             IllUpdate     = 11,
    [Description("Программа не может работать с данным ключом — истекла дата обновления П.О.!")] OldFile       = 12,
    [Description("Ключ защиты: Текущая дата на сервере меньше даты программирования ключа!")]    BadTime       = 13,
    [Description("Ключ защиты внесён в чёрный список!")]                                         Annuled       = 14,
    [Description("Сетевой ключ не поддерживает несколько клиентов!")]                            Sharing       = 15,
    [Description("Ключевой файл не разрешает работу этой версии П.О.!")]                         Version       = 16,

    [Description("Слишком много пользователей!")]                      TooManyUsers          = 101,
    [Description("Тестовый период окончен!")]                          ExpirationDate        = 102,
    [Description("Слишком много зарегестрированных телепараметров!")]  TooManyTeleparameters = 103,
    [Description("Горячее резервирование не разрешено!")]              NoReservation         = 104,
    [Description("Ошибка высокого уровня при проверке ключа защиты!")] HlNoKey               = 105,
    [Description("Работа ARIS SCADA не предусмотрена!")]               NoArisScada           = 106
  }


  [Flags]
  public enum TmTraceTypes : uint
  {
    None    = 0,
    Error   = TmNativeDefs.TmsTraceFlags.Error,
    Message = TmNativeDefs.TmsTraceFlags.Message,
    Debug   = TmNativeDefs.TmsTraceFlags.Debug,
    In      = TmNativeDefs.TmsTraceFlags.In,
    Out     = TmNativeDefs.TmsTraceFlags.Out,
  }


  [Flags]
  public enum TmAccessRights : uint
  {
    [Description("Чтение телепараметров")]
    None = 0x00_00_00_00,
    
    [Description("Безусловный доступ")]
    FullAccess = 0xFF_FF_FF_FF,
    
    [Description("Чтение телепараметров")]
    ReadTmValues     = 0x00_00_00_01,
    
    [Description("Изменение ТС")]
    ChangeStatuses   = 0x00_00_00_02,
    
    [Description("Изменение ТИТ")]
    ChangeAnalogs    = 0x00_00_00_04,
    
    [Description("Изменение ТИИ")]
    ChangeAccums     = 0x00_00_00_08,
    
    [Description("Телеуправление")]
    Telecontrol      = 0x00_00_00_10,
    
    [Description("Просмотр ретроспектив")]
    ViewRetro        = 0x00_00_00_20,
    
    [Description("Просмотр журнала событий")]
    ViewEventLog     = 0x00_00_00_40,
    
    [Description("Изменение уставок")]
    ChangeAlarms     = 0x00_00_00_80,
    
    [Description("Источник телеметрии")]
    DataSource       = 0x00_00_01_00,
    
    [Description("Доступ к аппаратуре")]
    HardwareAccess   = 0x00_00_02_00,
    
    [Description("Изменение техобъектов")]
    TechObjectChange = 0x00_00_04_00,
    
    [Description("Резервное копирование")]
    ReserveCopy      = 0x00_00_08_00,
  }
  
  public enum TmEventLogExtendedSources
  {
    Server     = TmNativeDefs.EventLogExtendedSources.Server,
    Comtrade   = TmNativeDefs.EventLogExtendedSources.Comtrade,
    Omp        = TmNativeDefs.EventLogExtendedSources.Omp,
    AutoSect   = TmNativeDefs.EventLogExtendedSources.AutoSect,
    I850       = TmNativeDefs.EventLogExtendedSources.I850,
    BlackBox   = TmNativeDefs.EventLogExtendedSources.BlackBox,
    Iec101     = TmNativeDefs.EventLogExtendedSources.Iec101,
    Aura       = TmNativeDefs.EventLogExtendedSources.Aura,
    Iec103     = TmNativeDefs.EventLogExtendedSources.Iec103,
    Spa        = TmNativeDefs.EventLogExtendedSources.Spa,
    Modbus     = TmNativeDefs.EventLogExtendedSources.Modbus,
    Dnp3       = TmNativeDefs.EventLogExtendedSources.Dnp3,
    Dlms       = TmNativeDefs.EventLogExtendedSources.Dlms,
    TmaRelated = TmNativeDefs.EventLogExtendedSources.TmaRelated
  }
}