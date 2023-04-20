using System;

namespace Iface.Oik.Tm.Native.Api
{
	public partial class TmNative
	{
		public IntPtr PkfEnumPackedFiles(string pkfname, ref byte[] errBuf, uint maxErrs)

		{
			return pkfEnumPackedFiles(pkfname, errBuf, maxErrs);
		}
		public IntPtr PkfUnPack( string pkfname, string dirname, ref byte[] errBuf, uint maxErrs)
		{
			return pkfUnPack(pkfname, dirname, errBuf, maxErrs);
		}

		public Boolean PkfExtractFile(string pkfname, string filename, string dirname, ref byte[] errBuf, uint maxErrs)
		{
			return pkfExtractFile(pkfname, filename, dirname, errBuf, maxErrs);
		}

		public void PkfFreeMemory(IntPtr ptr)
		{
			pkfFreeMemory(ptr);
		}
	}
}
