using System;
using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Api
{
	//	typedef BOOL(* pfkProgressFn)(LPSTR pkf_name, LPSTR file_name, DWORD fidx, u64 total_size, u64 total_pos, u64 file_size, u64 file_pos, PVOID prog_parm);

	//  BOOL _calltype_  pkfPack(LPSTR pkfname, LPSTR fnames, LPSTR errs, DWORD errlen);
	//	BOOL _calltype_  pkfPackProgress(LPSTR pkfname, LPSTR fnames, LPSTR errs, DWORD errlen, pfkProgressFn prog, PVOID prog_parm);
	//	BOOL _calltype_  pkfPackAdd(LPSTR pkfname, LPSTR fnames, LPSTR errs, DWORD errlen);

	//!!!	LPSTR _calltype_  pkfUnPack(LPSTR pkfname, LPSTR dirname, LPSTR errs, DWORD errlen);
	//!!!!	LPSTR _calltype_  pkfEnumPackedFiles(LPSTR pkfname, LPSTR errs, DWORD errlen);
	//!!!!	BOOL _calltype_  pkfExtractFile(LPSTR pkfname, LPSTR fname, LPSTR dirname, LPSTR errs, DWORD errlen);

	//	BOOL _calltype_  pkfPack_Pwd(LPSTR pkfname, LPSTR fnames, LPSTR errs, DWORD errlen, LPSTR pwd);
	//	BOOL _calltype_  pkfPackAdd_Pwd(LPSTR pkfname, LPSTR fnames, LPSTR errs, DWORD errlen, LPSTR pwd);
	//	LPSTR _calltype_  pkfUnPack_Pwd(LPSTR pkfname, LPSTR dirname, LPSTR errs, DWORD errlen, LPSTR pwd);
	//	BOOL _calltype_  pkfExtractFile_Pwd(LPSTR pkfname, LPSTR fname, LPSTR dirname, LPSTR errs, DWORD errlen, LPSTR pwd);

	//	VOID _calltype_  pkfFreeMemory(PVOID p);

	//	INT cfsZipPack(LPSTR zipname, LPSTR fnames);
	//	LPSTR cfsZipUnpack(LPSTR zipname, LPSTR dir, INT* p_error);
	public partial class TmNative
	{
		[DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr pkfEnumPackedFiles([MarshalAs(UnmanagedType.LPStr)] string pkfname, 
													   [In, Out] byte[] errBuf,  UInt32 maxErrs);

		[DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr pkfUnPack([MarshalAs(UnmanagedType.LPStr)] string pkfname,
											   [MarshalAs(UnmanagedType.LPStr)] string dirname,
											   [In, Out] byte[] errBuf, UInt32 maxErrs);

		[DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
		public static extern Boolean pkfExtractFile([MarshalAs(UnmanagedType.LPStr)] string pkfname,
												    [MarshalAs(UnmanagedType.LPStr)] string filename,
												    [MarshalAs(UnmanagedType.LPStr)] string dirname,
													[In, Out] byte[] errBuf, UInt32 maxErrs);

		[DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
		public static extern void pkfFreeMemory(IntPtr ptr);
	}
}
