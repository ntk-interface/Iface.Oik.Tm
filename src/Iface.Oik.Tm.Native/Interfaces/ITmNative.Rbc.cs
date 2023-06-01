using System;

namespace Iface.Oik.Tm.Native.Interfaces
{
	public partial interface ITmNative
	{
		UInt16 RbcIpgStartRedirector(Int32 cid,
									 UInt16 portIdx);


		bool RbcIpgStopRedirector(Int32 cid,
								  UInt16 portIdx);


		Int32 RbcGetSecurity(Int32 cid,
							 out bool pAdmin,
							 out UInt32 pAccessMask);
		Boolean RbcBackupServerProcedure(string machine, string pipe, byte[] directory,
														ref UInt32 pbflags,
														Int32 hCancel,
														TmNativeCallback prog_fn,
														IntPtr prog_parm);
	}
}