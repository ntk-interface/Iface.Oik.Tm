using System;

namespace Iface.Oik.Tm.Native.Api
{
	public partial class TmNative
	{
		public IntPtr PkfEnumPackedFiles(byte[] pkfname, ref byte[] errBuf, uint maxErrs)

		{
			return pkfEnumPackedFiles(pkfname, errBuf, maxErrs);
		}
		public IntPtr PkfUnPack(byte[] pkfname, byte[] dirname, ref byte[] errBuf, uint maxErrs)
		{
			return pkfUnPack(pkfname, dirname, errBuf, maxErrs);
		}

		public Boolean PkfExtractFile(byte[] pkfname, byte[] filename, byte[] dirname, ref byte[] errBuf, uint maxErrs)
		{
			return pkfExtractFile(pkfname, filename, dirname, errBuf, maxErrs);
		}

		public void PkfFreeMemory(IntPtr ptr)
		{
			pkfFreeMemory(ptr);
		}
	}
}
