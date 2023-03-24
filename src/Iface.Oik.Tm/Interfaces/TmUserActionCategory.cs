using System.ComponentModel;

namespace Iface.Oik.Tm.Interfaces
{
  // Не менять порядок ни в коем случае!
  public enum TmUserActionCategory
  {
    [Description("")]                   None,
    [Description("Аудит")]              Audit,
    [Description("Каталог документов")] Documents,
    [Description("Плакат")]             Placards,
    [Description("Привязка")]           Bindings,
    [Description("Настройки")]          Settings,
    [Description("Сообщение")]          Message,
  }
}