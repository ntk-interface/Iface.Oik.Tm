using System;
using System.IO;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    public UInt16 RbcIpgStartRedirector(Int32  cid,
                                        UInt16 portIdx)
    {
      return rbcIpgStartRedirector(cid, portIdx);
    }


    public bool RbcIpgStopRedirector(Int32  cid,
                                     UInt16 portIdx)
    {
      return rbcIpgStopRedirector(cid, portIdx);
    }


    public Int32 RbcGetSecurity(Int32      cid,
                                out bool   pAdmin,
                                out UInt32 pAccessMask)
    {
      return rbcGetSecurity(cid, out pAdmin, out pAccessMask);
    }
  }
}