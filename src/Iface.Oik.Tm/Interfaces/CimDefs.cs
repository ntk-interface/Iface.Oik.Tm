using System;

namespace Iface.Oik.Tm.Interfaces
{
  public enum CimClassId
  {
    Substation         = 1,
    VoltageLevel       = 2,
    Bay                = 3,
    Line               = 4,
    Name               = 10,
    NameType           = 11,
    BaseVoltage        = 12,
    Voltage            = 13,
    ConnectivityNode   = 16,
    Terminal           = 17,
    Breaker            = 101,
    BusBarSection      = 102,
    Disconnector       = 104,
    GroundDisconnector = 107,
  }


  public enum CimSchemeItemType
  {
    GeneralGenerator = 65010,
  }


  public enum CimGuidScope : byte
  {
    Unknown          = 0,
    GlobalCounter    = 1,
    BaseVoltage      = 2,
    Substation       = 3,
    VoltageLevel     = 5,
    Bay              = 6,
    Equipment        = 7,
    Terminal         = 8,
    TransformerEnd   = 9,
    ConnectivityNode = 10,
  }
  
  [Flags]
  public enum CimTopologyStatus : byte
  {
    None       = 0,
    IsVoltaged = 0x1,
    IsGrounded = 0x2,
  }
}