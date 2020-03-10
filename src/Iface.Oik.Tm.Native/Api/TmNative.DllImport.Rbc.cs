using System;
using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    [DllImport(Tmconn, CallingConvention = CallingConvention.Cdecl)]
    public static extern UInt16 rbcIpgStartRedirector(Int32  cid,
                                                      UInt16 portIdx);


    [DllImport(Tmconn, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool rbcIpgStopRedirector(Int32  cid,
                                                   UInt16 portIdx);


    [DllImport(Tmconn, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 rbcGetSecurity(Int32      cid,
                                              out bool   pAdmin,
                                              out UInt32 pAccessMask);
  }
}