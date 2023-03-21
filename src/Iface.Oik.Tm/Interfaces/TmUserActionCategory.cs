using System.ComponentModel;

namespace Iface.Oik.Tm.Interfaces
{
  // Не менять порядок ни в коем случае!
  public enum TmUserActionCategory
  {
    [Description("")]                   None,
    [Description("Аудит")]              Audit,
    [Description("Каталог документов")] Documents,
    [Description("Плакаты")]            Placards,
    [Description("Привязки")]           Bindings,
    [Description("Настройки")]          Settings,
  }
}