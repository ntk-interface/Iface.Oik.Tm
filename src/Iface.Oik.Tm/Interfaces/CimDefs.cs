using System;

namespace Iface.Oik.Tm.Interfaces
{
  public enum CimClassId
  {
    Unknown = 0,

    // EquipmentContainer
    Substation = 1,
    VoltageLevel = 2,
    Bay = 3,
    Line = 4,

    //
    Name = 10,
    NameType = 11,
    BaseVoltage = 12,
    Voltage = 13,
    UnitSymbol = 14,
    UnitMultiplier = 15,
    ConnectivityNode = 16,
    Terminal = 17,
    PowerTransformerEnd = 20,

    // Conducting Equipment
    ACLineSegment = 100,
    Breaker,
    BusbarSection,
    Cut,
    Disconnector,
    ExternalNetworkInjection,
    Fuse,
    GroundDisconnector,
    GroundingImpedance,
    Jumper,
    Junction,
    PetersenCoil,
    PowerTransformer,
    EnergySource,
    EnergyConsumer, // abstract?
    StationSupply,
    NonConformLoad,
    ConformLoad,
    MktEnergyConsumer,
    MktConductingEquipment,
    FlowSensor,
    MktPowerTransformer,
    EquivalentEquipment, // abstract?
    EquivalentShunt,
    EquivalentInjection,
    EquivalentBranch,
    RegulatingCondEq, // abstract?
    StaticVarCompensator,
    ShuntCompensator,
    NonlinearShuntCompensator,
    SVC,
    MktShuntCompensator,
    LinearShuntCompensator,
    RotatingMachine,
    SynchronousMachine,
    AsynchronousMachine,
    FrequencyConverter,
    Switch, // abstract?
    ProtectedSwitch, // abstract?
    Recloser,
    LoadBreakSwitch,
    MktSwitch,
    Sectionaliser,
    EarthFaultCompensator,
    SeriesCompensator,
    MktSeriesCompensator,
    Ground,
    Conductor, // abstract? 
    MktACLineSegment,
    Connector, // abstract?
    CsConverter,
    VsConverter,
    Clamp,
    GroundSwitch, // [me:], no [cim:] !!!
    FuseSwitchDisconnector, // [me:], no [cim:] !!!

    // Auxiliary Equipment
    CurrentTransformer = 200,
    PotentialTransformer,
    SurgeArrester,
    WaveTrap,
    FaultIndicator,
    PostLineSensor,

    // Protection Equipment
    ProtectionEquipment = 300, // abstract?
    SynchrocheckRelay,
    CurrentRelay,

    // 
    ConnectDisconnectFunction = 400,
    CurrentLimit,
    Meter,
    OperationalLimitSet,
    RatioTapChanger,
    ServiceLocation,
    TapChangerControl,
    TransformerMeshImpedance,
    UsagePoint,
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