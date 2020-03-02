using System.ComponentModel;

namespace Iface.Oik.Tm.Interfaces
{
  public class CfsDefs
  {
    public enum InitializeConnectionResult
    {
      Ok                        = 0,

      [Description("Ошибка авторизации!")]
      InvalidLoginOrPassword      = 87,
      
      [Description("Ошибка соединения!")]
      NonSpecifiedError           = 1000,
    }
    
    public enum MasterServiceStatus
    {
      [Description("Потеряно соединение!")]
      LostConnection = 0,
      
      [Description("Мастер-сервис остановлен!")]
      Stopped        = 1,
      
      [Description("Мастер-сервис запущен!")]
      Running        = 2
    }
  }
}