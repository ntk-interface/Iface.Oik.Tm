using System;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAlertEventArgs : EventArgs
  {
    public TmAlertEventReason Reason     { get; set; }
    public TmEventImportances Importance { get; set; }
  }
  
  
  public enum TmAlertEventReason
  {
    Unknown = 0,
    Added   = 1,
    Removed = 2,
  }


  public class TobEventArgs : EventArgs
  {
    public TobEventReason Reason { get; set; }
  }


  public enum TobEventReason
  {
    Unknown  = 0,
    Topology = 1,
    Placards = 2,
    Global   = 3,
  }
}