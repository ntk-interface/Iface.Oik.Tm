using System;
using System.Text;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public partial interface ITmNative
  {
    IntPtr IniOpen(string filePath);


    void IniFlush(IntPtr filePointer);


    void IniClose(IntPtr filePointer);


    void IniReload(IntPtr filePointer);


    UInt32 IniReadString(IntPtr            filePointer,
                         string            section,
                         string            key,
                         string            defaultResponse,
                         byte[] buf,
                         UInt32            bufSize);


    Int32 IniReadInteger(IntPtr filePointer,
                         string section,
                         string key,
                         Int32  defaultResponse);


    UInt32 IniReadSection(IntPtr filePointer,
                          string section,
                          byte[] buf,
                          UInt32 bufSize);
    UInt32 IniReadStruct(IntPtr filePointer,
                          string section,
                          string key,
                          byte[] buf,
                          UInt32 bufSize);
  }
}