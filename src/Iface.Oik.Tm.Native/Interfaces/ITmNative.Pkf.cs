using System;
using System.Collections.Generic;
using System.Text;

namespace Iface.Oik.Tm.Native.Interfaces
{
	public partial interface ITmNative
	{
		IntPtr PkfEnumPackedFiles(string pkfname, ref byte[] errBuf, uint maxErrs);
		IntPtr PkfUnPack(string pkfname, string dirname, ref byte[] errBuf, uint maxErrs);
		Boolean PkfExtractFile(string pkfname, string filename, string dirname, ref byte[] errBuf, uint maxErrs);
		void PkfFreeMemory(IntPtr ptr);
	}
}
