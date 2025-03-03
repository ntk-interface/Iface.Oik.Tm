using System.ComponentModel;

namespace Iface.Oik.Tm.Interfaces
{
  public enum ModusElement : ushort
  {
    [Description("Выключатель")]                  Breaker                    = 41,
    [Description("Выключатель нагрузки")]         LoadBreaker                = 42,
    [Description("Выдвижной выключатель")]        DrawoutBreaker             = 43,
    [Description("Рубильник")]                    DoublePoleSwitch           = 44,
    [Description("Разъединитель")]                OldDisconnector            = 45, // Модус 5.1
    [Description("Разъединитель")]                OldDisconnector2           = 46, // Модус 5.1
    [Description("Выдвижной разъединитель")]      DrawoutDisconnector        = 49,
    [Description("Выдвижной отделитель")]         DrawoutSeparator           = 50,
    [Description("Ячейка КРУ без оборудования")]  Chassis                    = 51,
    [Description("Заземляющий нож")]              GroundDisconnector         = 54,
    [Description("Пускатель")]                    Actuator                   = 76,
    [Description("Табло")]                        Board                      = 102,
    [Description("Автоматика")]                   Automation                 = 103,
    [Description("Лампа")]                        Lamp                       = 106,
    [Description("Блинкер")]                      Blinker                    = 115,
    [Description("Ручка")]                        Handle                     = 118, // орган управления
    [Description("Кнопка")]                       Button                     = 119,
    [Description("Разъединитель")]                Disconnector               = 162, // Модус 5.2
    [Description("Короткозамыкатель без земли")]  ShortCircuitorWithoutEarth = 163, // Модус 5.2
    [Description("Отделитель")]                   Separator                  = 164, // Модус 5.2
    [Description("Разъединитель-предохранитель")] FuseDisconnector           = 166,
    [Description("Короткозамыкатель")]            ShortCircuitor             = 398,
    [Description("Силовой автомат")]              MoldedCircuitBreaker       = 399,

    [Description("АПВ")]      ReclosingDevice = 89, // выглядит как задвижка, кто-то использует...
    [Description("Насос")]    Pump            = 263,
    [Description("Задвижка")] Shutter         = 270,
    [Description("Клапан")]   Valve           = 289,

    [Description("Бустер")]                    Booster                           = 6,
    [Description("Точка")]                     Point                             = 7,
    [Description("Непересечение")]             NonIntersection                   = 14,
    [Description("Крепление")]                 Fastening                         = 18,
    [Description("Вилка")]                     Fork                              = 26,
    [Description("ОПН")]                       OvervoltageSuppressor             = 29,
    [Description("Заземление")]                Earthing                          = 31,
    [Description("Муфта")]                     Muft                              = 32,
    [Description("Катушка")]                   Coil                              = 33,
    [Description("Трансформатор тока")]        CurrentTransformer                = 34,
    [Description("Разрядник")]                 Discharger                        = 35,
    [Description("Генератор/СК/БСК")]          OldGenerator                      = 36, // Модус 5.1
    [Description("Реактор")]                   Reactance                         = 37,
    [Description("Двигатель")]                 Motor                             = 39,
    [Description("Трансформатор")]             Transformer                       = 47,
    [Description("Полушасси")]                 DrawoutContact                    = 52,
    [Description("Трансформатор напряжения")]  VoltageTransformer                = 55,
    [Description("Электроопора")]              Bearing                           = 146,
    [Description("Предохранитель на тележке")] DrawoutFuse                       = 154,
    [Description("ОПН с землей")]              OvervoltageSuppressorWithEarthing = 168, // ОПН1
    [Description("Трансформатор")]             TransformerUncommon               = 169, // нестаднартный_трансформатор
    [Description("БСК")]                       CapacitorBank                     = 172,
    [Description("Генератор")]                 Generator                         = 173,
    [Description("Синхронный компенсатор")]    Compensator                       = 174,
    [Description("Предохранитель")]            Fuse                              = 203,
    [Description("Заглушка трубопровода")]     PipePlug                          = 266,
    [Description("Соединитель трубопроводов")] PipeConnector                     = 281,
    [Description("Подстанция")]                Substation                        = 360,
    [Description("КТП")]                       Ktp                               = 385,
    [Description("ЗТП")]                       Ztp                               = 386,
    [Description("Конденсатор")]               Capacitor                         = 388,
    [Description("Заградительный фильтр")]     Filter                            = 389,
    [Description("Шунтирующий реактор")]       ReactanceShunting                 = 397,

    [Description("Линия")]               Line              = 1,
    [Description("Стрелка")]             Arrow             = 2,
    [Description("Прямоугольник")]       Rectangle         = 3,
    [Description("Круг")]                Circle            = 4,
    [Description("Дуга")]                Arc               = 9,
    [Description("Подложка (из файла)")] Substrate         = 11,
    [Description("Картинка (из файла)")] Image             = 12,
    [Description("Ошиновка")]            Junction          = 21,
    [Description("Воздушная линия")]     AirLine           = 22,
    [Description("Кабельная линия")]     CableLine         = 23,
    [Description("Шина")]                BusBar            = 24,
    [Description("Трубопровод")]         PipeLine          = 25,
    [Description("Линия связи")]         CommunicationLine = 27,
    [Description("Связь с объектом")]    Link              = 28,
    [Description("Контейнер")]           Container         = 310,

    [Description("Текст")]           Text           = 5,
    [Description("Метка")]           Label          = 17,
    [Description("Цифровой прибор")] DigitalDevice  = 133, // Модус 4
    [Description("Цифровой прибор")] DigitalDevice2 = 134, // Модус 5
    [Description("Таблица")]         Table          = 313,
    
    [Description("Линия связи генеральной схемы")]       GeneralSchemeLine      = 65002, // генеральная схема CIM-модели
    [Description("Источник мощности генеральной схемы")] GeneralSchemeGenerator = 65010, // генеральная схема CIM-модели
  }
}