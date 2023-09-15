using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class DeltaTraceMessage
  {
    public DeltaTraceMessageTypes Type         { get; }
    public DateTime?              Time         { get; }
    public string                 Text         { get; }
    public string                 BinaryString { get; }
    public bool                   IsDecoded    { get; }

    public string TimeString => Time.HasValue ? $"{Time.Value:yyyy.MM.dd HH:mm:ss.fff}" : string.Empty;
    public DeltaTraceMessage(DeltaTraceMessageTypes type, string dateTimeString, string text, string binaryString)
    {
      Type     = type;

      if (!dateTimeString.IsNullOrEmpty())
      {
        Time = DateUtil.GetDateTimeFromExtendedReversedTmString(dateTimeString);
      }
      Text     = text;
      BinaryString = binaryString;

      IsDecoded = text.Contains("•");
    }

    public override string ToString()
    {
      var binaryString = BinaryString.IsNullOrEmpty() ? string.Empty : $" {BinaryString}";
      var timeString = TimeString.IsNullOrEmpty() ? string.Empty : $" [{TimeString}]";
      return $"{timeString} {Text}{binaryString}";
    }
  }
}