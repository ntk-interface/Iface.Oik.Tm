using System;

namespace Iface.Oik.Tm.Interfaces
{
  public class DeltaTraceMessage
  {
    public DeltaTraceMessageTypes Type { get; }
    public string                 Text { get; }
    
    public DeltaTraceMessage(DeltaTraceMessageTypes type, string text)
    {
      Type = type;
      Text = text;
    }

    public override string ToString()
    {
      string prefix;
      switch (Type)
      {
        case DeltaTraceMessageTypes.Error:
          prefix = "Ошибка";
          break;
        case DeltaTraceMessageTypes.Message:
          prefix = "Сообщение";
          break;
        case DeltaTraceMessageTypes.Debug:
          prefix = "Отладка";
          break;
        case DeltaTraceMessageTypes.In:
          prefix = "<--";
          break;
        case DeltaTraceMessageTypes.Out:
          prefix = "-->";
          break;
        case DeltaTraceMessageTypes.TmsIn:
          prefix = "ТМС <--";
          break;
        case DeltaTraceMessageTypes.TmsOut:
          prefix = "ТМС -->";
          break;
        default:
          prefix = "?";
          break;
      }

      return $"{prefix}  {Text}";
    }
    
  }
}