using System;
using System.ComponentModel;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Interfaces
{
  public enum TmType
  {
    Unknown = 0,
    Status  = 1,
    Analog  = 2,
    Accum   = 3,
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
  public enum TmEventTypes : ushort
  {
    None            = 0,
    StatusChange    = TmNativeDefs.EventTypes.StatusChange,
    Alarm           = TmNativeDefs.EventTypes.Alarm,
    Control         = TmNativeDefs.EventTypes.Control,
    Acknowledge     = TmNativeDefs.EventTypes.Acknowledge,
    ManualStatusSet = TmNativeDefs.EventTypes.ManualStatusSet,
    ManualAnalogSet = TmNativeDefs.EventTypes.ManualAnalogSet,
    Res1            = TmNativeDefs.EventTypes.Res1,
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
    Telecontrol  = 0x00_00_01_00,
    GetRetro     = 0x00_00_02_00,
    GetEvents    = 0x00_00_04_00,
    EditAlarms   = 0x00_00_08_00,
    GetTmSource  = 0x00_00_10_00,
    GetHardware  = 0x00_00_20_00,
    EditTob      = 0x00_00_40_00,
  }


  public enum TmUserPermissions
  {
    ViewScheme                  = 0,
    ViewSchemeInGroup           = 1,
    Telecontrol                 = 2,
    SwitchStatusManually        = 3,
    EditScheme                  = 4,
    EditSchemeInGroup           = 5,
    DeleteScheme                = 6,
    DeleteSchemeInGroup         = 7,
    RenameScheme                = 8,
    RenameSchemeInGroup         = 37,
    DeleteGroup                 = 9,
    DeleteGroupInGroup          = 38,
    SoundAlarmStatuses          = 35,
    TelecontrolWithoutKey       = 36,
    EditAlarms                  = 41,
    SetAnalogManually           = 42,
    BlockManually               = 43,
    TelecontrolInGroup          = 45,
    SwitchStatusManuallyInGroup = 46,
    SetAnalogManuallyInGroup    = 47,
    ViewAllSchemeCatalogs       = 48,
    Ack                         = 49,
    OverrideControlScript       = 50,
    NotExists                   = 1000,
  }


  public enum TmPlacardAction
  {
    [Description("")]           None   = 0,
    [Description("Заземление")] Ground = 1,
    [Description("Расшиновка")] Gap    = 2,
  }


  public enum TmTechObjectBindingsMode
  {
    [Description("")]    None            = 0,
    [Description("ИЗМ")] Measurement     = 1,
    [Description("ОМП")] DamageLocation  = 2,
    [Description("ТЕХ")] TechParams      = 3,
    [Description("РЗА")] RelayProtection = 4,
    [Description("РЕЖ")] Regime          = 5,
    [Description("ПЗ")]  PortableGround  = 6,
    [Description("ОБР")] OperLock        = 7,
  }


  public enum TmTechObjectBindingType
  {
    TmTag     = 0,
    TechParam = 1,
  }


  public enum TmTelecontrolValidationResult
  {
    Ok                     = 0,
    StatusHasNoTelecontrol = 1,
    Forbidden              = 2,
    StatusIsUnreliable     = 3,
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
    [Description("Команда не была отправлена")]                                     CommandNotSentToServer = -100,
  }
}