namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
#if x86
    private const string Cfshare = "cfshare.dll";
    private const string Tmconn  = "tmconn.dll";
#else
    private const string Cfshare = "libif_cfs";
    private const string Tmconn  = "libif_cfs";
#endif
  }
}