using System.ComponentModel;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Interfaces
{
  public enum DeltaComponentTypes
  {
    Driver  = 0,
    Adapter = 1,
    Port    = 2,
    Rtu     = 3,
    Array   = 4,
    Block   = 5,
    Unknown = 6,
    Info    = 7
  }

  public enum DeltaItemTypes
  {
    Description = TmNativeDefs.DeltaItemTypes.Description,
    Status      = TmNativeDefs.DeltaItemTypes.Status,
    Analog      = TmNativeDefs.DeltaItemTypes.Analog,
    Accum       = TmNativeDefs.DeltaItemTypes.Accum,
    Control     = TmNativeDefs.DeltaItemTypes.Control,
    AnalogFloat = TmNativeDefs.DeltaItemTypes.AnalogF,
    AccumFloat  = TmNativeDefs.DeltaItemTypes.AccumF,
    StrVal      = TmNativeDefs.DeltaItemTypes.StrVal
  }


  public enum DeltaTraceTypes
  {
    Protocol = 0,
    Physical = 1,
    Logical = 2
  }
}