using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmRetroInfo
  {
    public string    Name           { get; private set; }
    public string    Description    { get; private set; }
    public uint      Period         { get; private set; }
    public uint      Capacity       { get; private set; }
    public DateTime? StartTime      { get; private set; }
    public DateTime? EndTime        { get; private set; }
    public uint      RecordsCount   { get; private set; }
    public uint      Version        { get; private set; }
    public uint      AppendSpeed    { get; private set; }
    public uint      SizeMb         { get; private set; }
    public uint      MaxSizeMb      { get; private set; }
    public uint      LastRecordSize { get; private set; }
    public TmType    TmType         { get; private set; }


    public static TmRetroInfo CreateFromTRetroInfoEx(TmNativeDefs.TRetroInfoEx info)
    {
      var tmRetroInfo = new TmRetroInfo
      {
        Name = info.Name,
        Description = info.Description,
        Period = info.Period,
        Capacity = info.Capacity,
        StartTime = DateUtil.GetDateTimeFromTimestamp(info.Start),
        EndTime = DateUtil.GetDateTimeFromTimestamp(info.Stop),
        RecordsCount = info.RecCount,
        Version = info.Version,
        AppendSpeed = info.AppendTicks,
        SizeMb = info.SizeMb,
        MaxSizeMb = info.MaxMb,
        LastRecordSize = info.LastRecSize,
        TmType = ((TmNativeDefs.TmDataTypes) info.Type).ToTmType()
      };

      return tmRetroInfo;
    }
  }
}