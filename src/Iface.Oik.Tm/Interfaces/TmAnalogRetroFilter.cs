using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAnalogRetroFilter
  {
    public DateTime StartTime { get; }
    public DateTime EndTime   { get; }
    public int      Step      { get; }


    public TmAnalogRetroFilter(DateTime startTime, DateTime endTime, int? step = null)
    {
      StartTime = startTime;
      EndTime   = endTime;

      if (step > 0)
      {
        Step = step.Value;
      }
      else
      {
        Step = TmUtil.GetRetrospectivePreferredStep(startTime, endTime);
      }
    }


    public TmAnalogRetroFilter(long startTime, long endTime, int? step = null)
      : this(DateUtil.GetDateTimeFromTimestamp(startTime),
             DateUtil.GetDateTimeFromTimestamp(endTime),
             step)
    {
    }


    public TmAnalogRetroFilter(string startTime, string endTime, int? step = null)
      : this(DateUtil.GetDateTime(startTime) ?? throw new ArgumentException(),
             DateUtil.GetDateTime(endTime)   ?? throw new ArgumentException(),
             step)
    {
    }
  }
}