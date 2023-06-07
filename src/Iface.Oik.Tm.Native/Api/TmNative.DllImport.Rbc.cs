using Iface.Oik.Tm.Native.Interfaces;
using System;
using System.Collections.Generic;
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

		//TMC_IMPEX BOOL _CDECL rbcBackupServerProcedure(
		//LPSTR machine,
		//	LPSTR pipe,
		//	LPSTR directory,
		//	PDWORD pbflags,
		//	HANDLE hCancel,
		//	tmcProgressFn prog_fn,
		//	LPVOID prog_parm
		//);
	[DllImport(Tmconn, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern Boolean rbcBackupServerProcedure(
													byte[] machine,
													byte[] pipe,
													byte[] directory,
													ref UInt32 pbflags,
													Int32 hCancel,
													[MarshalAs(UnmanagedType.FunctionPtr)] TmNativeCallback prog_fn,
													IntPtr prog_parm);
	}
}