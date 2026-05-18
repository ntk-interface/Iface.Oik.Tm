using Iface.Oik.Tm.Native.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    [LibraryImport(Tmconn, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool rbcBackupServerProcedure(string machine,
                                                        string pipe,
                                                        string directory,
                                                        ref uint pbflags,
                                                        int hCancel,
                                                        [MarshalAs(UnmanagedType.FunctionPtr)] TmNativeCallback? progFn,
                                                        nint progParm);
  }
}