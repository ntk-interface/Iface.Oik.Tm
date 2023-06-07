using System;
using System.Collections.Generic;
using System.Text;

namespace Iface.Oik.Tm.Native.Interfaces
{
	public partial interface ITmNative
	{
		IntPtr PkfEnumPackedFiles(byte[] pkfname, ref byte[] errBuf, uint maxErrs);
		IntPtr PkfUnPack(byte[] pkfname, byte[] dirname, ref byte[] errBuf, uint maxErrs);
		Boolean PkfExtractFile(byte[] pkfname, byte[] filename, byte[] dirname, ref byte[] errBuf, uint maxErrs);
		void PkfFreeMemory(IntPtr ptr);
	}
}
