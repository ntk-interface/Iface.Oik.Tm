namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static void SetRedirectorPort(nint cfCid, 
                                       string pipeName, 
                                       int portIndex, 
                                       int port)
  {
    IfpcSetBinString(cfCid, 
                     ".cfs.", 
                     $"rbs${pipeName}", 
                     $"ipg_port{portIndex}", 
                     $"{port}");
  }
}