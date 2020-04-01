using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmServerLogRecord
  {
    public string                Message    { get; }
    public TmServerLogRecordType RecordType { get; }
    public string                Source     { get; }
    public int                   ThreadId   { get; }
    public DateTime?             DateTime   { get; }

    public string Date => DateTime.HasValue ? DateTime.Value.ToString("dd.MM.yyyy") : "";
    public string Time => DateTime.HasValue ? DateTime.Value.ToString("HH:mm:ss.fff") : "";
    


    public TmServerLogRecord(string message,
                             string recordType,
                             string source,
                             string threadId,
                             string date,
                             string time)
    {
      Message    = message;
      RecordType = ParseRecordType(recordType);
      Source     = source;
      ThreadId   = Convert.ToInt32(threadId, 16);
      DateTime   = DateUtil.GetDateTime($"{date} {time}");
    }


    private static TmServerLogRecordType ParseRecordType(string recordTypeString)
    {
      switch (recordTypeString)
      {
        case "MSG":
          return TmServerLogRecordType.Msg;
        case "ERROR":
          return TmServerLogRecordType.Error;
        default:
          return TmServerLogRecordType.Undefined;
      }
    }
  }
}