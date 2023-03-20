using System;
using System.Collections.Generic;
using System.Linq;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class SLogRecord
  {
    public string    Type            { get; set; }
    public string    Body            { get; set; }
    public string    User            { get; set; }
    public DateTime? DateTime        { get; set; }
    public int       Index           { get; set; }
    public string    Source          { get; set; }
    public int       ThreadId        { get; set; }
    public int       SessionId       { get; set; }
    public int       FileIndex       { get; set; }
    public string    InformationType { get; set; }

    public string DateTimeString => DateTime.HasValue ? DateTime.Value.ToString("dd.MM.yyyy HH:mm:ss.fff") : "";
    public string DateTimeLocalString => DateTime.HasValue 
                                           ? TimeZoneInfo.ConvertTime(DateTime.Value, TimeZoneInfo.Local)
                                                         .ToString("dd.MM.yyyy HH:mm:ss.fff") 
                                           : "";

    public string IndexHexString     => Index     > 0 ? $"{Index:X}" : "";
    public string ThreadIdHexString  => ThreadId  > 0 ? $"{ThreadId:X}".ToUpper() : "";
    public string SessionIdHexString => SessionId > 0 ? $"{SessionId:X}".ToUpper() : "";
    public string FileIndexHexString => FileIndex > 0 ? $"{FileIndex:x8}".ToUpper() : "";

    public static SLogRecord CreateFromStringsList(IReadOnlyCollection<string> strings) 
    {
      var result = new SLogRecord();

      if (!strings.Any())
      {
        return null;
      }
      
      foreach (var str in strings)
      {
        var payload = str.Substring(1);
        switch (str[0])
        {
          case TmNativeDefs.SLogTag.Type:
            result.Type = payload;
            break;
          case TmNativeDefs.SLogTag.Body:
            result.Body = payload;
            break;
          case TmNativeDefs.SLogTag.User:
            result.User = payload;
            break;
          case TmNativeDefs.SLogTag.Time:
            var dateTime = DateUtil.GetDateTimeFromExtendedReversedTmString(payload);
            result.DateTime = dateTime.HasValue ? System.DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc) : (DateTime?)null;
            break;
          case TmNativeDefs.SLogTag.Index:
            result.Index = Convert.ToInt32(payload, 16);
            break;
          case TmNativeDefs.SLogTag.Source:
            result.Source = payload;
            break;
          case TmNativeDefs.SLogTag.ThreadId:
            result.ThreadId = Convert.ToInt32(payload, 16);
            break;
          case TmNativeDefs.SLogTag.SessionId:
            result.SessionId = Convert.ToInt32(payload, 16);
            break;
          case TmNativeDefs.SLogTag.FileIndex:
            result.FileIndex = Convert.ToInt32(payload, 16);
            break;
          case TmNativeDefs.SLogTag.InformationType:
            result.InformationType = payload;
            break;
        }
      }

      return result;
    }
  }
}