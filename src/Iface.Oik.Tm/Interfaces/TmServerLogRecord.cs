using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmServerLogRecord
  {
    private readonly int _hashCode;


    public string                Message      { get; private set; }
    public TmServerLogRecordTypes RecordTypes   { get; private set; }
    public string                SourceServer { get; private set; }
    public int                   ThreadId     { get; private set; }
    public DateTime?             DateTime     { get; private set; }

    public string RecordTypeString => RecordTypes.GetDescription();
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
               RecordTypes   = ParseRecordType(cfsLogRecord.MsgType),
               SourceServer = cfsLogRecord.Name,
               ThreadId     = Convert.ToInt32(cfsLogRecord.ThreadId, 16),
               DateTime     = DateUtil.GetDateTime($"{cfsLogRecord.Date} {cfsLogRecord.Time}")
             };
    }


    private static TmServerLogRecordTypes ParseRecordType(string recordTypeString)
    {
      switch (recordTypeString)
      {
        case "MSG":
          return TmServerLogRecordTypes.Msg;
        case "ERROR":
          return TmServerLogRecordTypes.Error;
        default:
          return TmServerLogRecordTypes.Undefined;
      }
    }

    public override int GetHashCode()
    {
      return _hashCode;
    }
  }
}