using System;

namespace Iface.Oik.Tm.Native.Interfaces;

internal static partial class TmNativeDefsUnsafe
{
  public const int  ErrorBufSize    = 1000;
  public const int  CftNameBufSize  = 200;
  public const uint FailIfNoConnect = 0x80000000;
  
  public static class MsTreeNodesNames
  {
    public const string Portcore         = "portcore";
    public const string Master           = "_master_.exe";
    public const string TmServer         = "pcsrv";
    public const string RBaseServer      = "rbsrv";
    public const string PcsrvOld        = "tmserv.dll";
    public const string RbsrvOld        = "rbase.dll";
    public const string Delta            = "delta_pc";
    public const string DeltaOld        = "delta_nt.exe";
    public const string TmCalc           = "tmcalc_pc";
    public const string TmCalcOld       = "tmcalc.exe";
    public const string ExternalTask     = "_ext_pc";
    public const string ExternalTaskOld     = "_extern";
    public const string TopologySrv          = "ElectricTopology";
    public const string GenSrv           = "_srv_.exe";
  }
}