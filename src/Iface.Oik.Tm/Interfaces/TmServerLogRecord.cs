using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmServerLogRecord
  {
    private int _hashCode { get; }
    
    
    public string                Message    { get; private set; }
    public TmServerLogRecordType RecordType { get; private set; }
    public string                SourceServer     { get; private set; }
    public int                   ThreadId   { get; private set; }
    public DateTime?             DateTime   { get; private set; }

    public string Date => DateTime.HasValue ? DateTime.Value.ToString("dd.MM.yyyy") : "";
    public string Time => DateTime.HasValue ? DateTime.Value.ToString("HH:mm:ss.fff") : "";



    public TmServerLogRecord(int hashCode)
    {
      _hashCode = hashCode;
    }


    public static TmServerLogRecord CreateFromCfsLogRecord(TmNativeDefs.CfsLogRecord cfsLogRecord)
    {
      return new TmServerLogRecord((cfsLogRecord.Date, cfsLogRecord.Time, cfsLogRecord.Message).ToTuple().GetHashCode())
             {
               Message      = cfsLogRecord.Message,
               RecordType   = ParseRecordType(cfsLogRecord.MsgType),
               SourceServer = cfsLogRecord.Type,
               ThreadId     = Convert.ToInt32(cfsLogRecord.ThreadId, 16),
               DateTime     = DateUtil.GetDateTime($"{cfsLogRecord.Date} {cfsLogRecord.Time}")
             };

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