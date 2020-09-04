using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class DeltaTraceMessage
  {
    public DeltaTraceMessageTypes Type         { get; }
    public DateTime?              Time     { get; }
    public string                 Text         { get; }
    public string                 BinaryString { get; }

    public string TimeString => Time.HasValue ? $"{Time.Value:yyyy.MM.dd HH:mm:ss.fff}" : string.Empty;
    public DeltaTraceMessage(DeltaTraceMessageTypes type, string dateTimeString, string text, string binaryString)
    {
      Type     = type;

      if (!dateTimeString.IsNullOrEmpty())
      {
        Time = DateUtil.GetDateTimeFromExtendedTmString(dateTimeString);
      }
      Text     = text;
      BinaryString = binaryString;
    }

    public override string ToString()
    {
      string prefix;
      switch (Type)
      {
        case DeltaTraceMessageTypes.Error:
          prefix = "Ошибка   ";
          break;
        case DeltaTraceMessageTypes.Message:
          prefix = "Сообщение";
          break;
        case DeltaTraceMessageTypes.Debug:
          prefix = "Отладка  ";
          break;
        case DeltaTraceMessageTypes.In:
          prefix = "<--      ";
          break;
        case DeltaTraceMessageTypes.Out:
          prefix = "-->      ";
          break;
        case DeltaTraceMessageTypes.TmsIn:
          prefix = "ТМС <--  ";
          break;
        case DeltaTraceMessageTypes.TmsOut:
          prefix = "ТМС -->  ";
          break;
        default:
          prefix = "?        ";
          break;
      }

      var binaryString = BinaryString.IsNullOrEmpty() ? string.Empty : $" {BinaryString}";
      var timeString = TimeString.IsNullOrEmpty() ? string.Empty : $" [{TimeString}]";
      return $"{prefix}{timeString} {Text}{binaryString}";
    }
  }
}