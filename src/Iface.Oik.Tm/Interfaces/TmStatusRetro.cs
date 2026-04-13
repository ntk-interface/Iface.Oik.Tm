using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces;

public class TmStatusRetro : TmStatusRetroBase
{
  public short    Status { get; private set; }
  public TmFlags  Flags  { get; private set; }
  public DateTime Time   { get; private set; }
  
  public TmStatusRetro(){}
  
  public TmStatusRetro(short status,
                       short flags,
                       long  timestamp)
  {
    Status = status;
    Flags  = (TmFlags)flags;
    Time   = DateUtil.GetDateTimeFromTimestamp(timestamp);
  }

  protected override void Initialize(short status,
                                     short flags,
                                     long  timestamp)
  {
    Status = status;
    Flags  = (TmFlags)flags;
    Time   = DateUtil.GetDateTimeFromTimestamp(timestamp);
  }
}