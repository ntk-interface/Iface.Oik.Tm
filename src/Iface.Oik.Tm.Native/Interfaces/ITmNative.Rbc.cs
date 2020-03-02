using System;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public partial interface ITmNative
  {
    UInt16 RbcIpgStartRedirector(Int32  cid,
                                 UInt16 portIdx);


    bool RbcIpgStopRedirector(Int32  cid,
                              UInt16 portIdx);
  }
}