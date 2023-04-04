using System.ComponentModel;

namespace Iface.Oik.Tm.Interfaces
{
  public enum TmEventSource
  {
    [Description("События и действия пользователей")] Union       = 0,
    [Description("Только события")]                   TmEvents    = 1,
    [Description("Только действия пользователей")]    UserActions = 2,
  }
}