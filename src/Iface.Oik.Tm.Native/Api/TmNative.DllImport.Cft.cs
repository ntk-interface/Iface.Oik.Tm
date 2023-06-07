using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void cftNodeFreeTree(IntPtr id);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr cftNodeEnum(IntPtr id, Int32 idx);
    
    
    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr cftNodeEnumAll(IntPtr id, Int32 idx);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNodeGetName(IntPtr                                         id,
                                               [In, Out] byte[] buf,
                                               UInt32                                         count);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNPropEnum(IntPtr           id,
                                             Int32            idx,
                                             [In, Out] byte[] buf,
                                             UInt32           count);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNPropGetText(IntPtr                                  id,
                                                byte[] name,
                                                [In, Out]                        byte[] buf,
                                                UInt32                                  count);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr cftNodeNewTree();


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNodeInsertAfter(IntPtr                                  id,
                                                   byte[] nodeTag);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNodeInsertDown(IntPtr                                  id,
                                                  byte[] nodeTag);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern bool cftNPropSet(IntPtr                                  id,
                                          byte[] propName,
                                          byte[] propText);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern bool cftNodeIsEnabled(IntPtr id);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void cftNodeEnable(IntPtr id,
                                            bool   enable);
  }
}