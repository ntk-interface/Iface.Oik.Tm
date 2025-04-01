using System;
using System.Text;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public partial interface ITmNative
  {
    IntPtr IniOpen(Byte[] filePath);


    void IniFlush(IntPtr filePointer);


    void IniClose(IntPtr filePointer);


    void IniReload(IntPtr filePointer);


    UInt32 IniReadString(IntPtr filePointer,
                         Byte[] section,
                         Byte[] key,
                         Byte[] defaultResponse,
                         Byte[] buf,
                         UInt32 bufSize);


    Int32 IniReadInteger(IntPtr filePointer,
                         Byte[] section,
                         Byte[] key,
                         Int32  defaultResponse);


    UInt32 IniReadSection(IntPtr filePointer,
                          Byte[] section,
                          Byte[] buf,
                          UInt32 bufSize);


    UInt32 IniReadStruct(IntPtr filePointer,
                         Byte[] section,
                         Byte[] key,
                         Byte[] buf,
                         UInt32 bufSize);
  }
}