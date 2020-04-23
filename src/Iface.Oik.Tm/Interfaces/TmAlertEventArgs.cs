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
}