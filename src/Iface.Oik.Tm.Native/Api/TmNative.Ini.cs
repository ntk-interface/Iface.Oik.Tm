using System;
using System.Text;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    public IntPtr IniOpen(byte[] filePath)
    {
      return ini_Open(filePath);
    }


    public void IniFlush(IntPtr filePointer)
    {
      ini_Flush(filePointer);
    }
    
    
    public void IniClose(IntPtr filePointer)
    {
      ini_Close(filePointer);
    }

    public void IniReload(IntPtr filePointer)
    {
      ini_Reload(filePointer);
    }

    public UInt32 IniReadString(IntPtr filePointer,
                                byte[] section,
                                byte[] key,
                                byte[] defaultResponse,
                                byte[] buf,
                                UInt32 bufSize)
    {
      return ini_ReadString(filePointer, section, key, defaultResponse, buf, bufSize);
    }

    public Int32 IniReadInteger(IntPtr filePointer,
                                byte[] section,
                                byte[] key,
                                Int32  defaultResponse)
    {
      return ini_ReadInteger(filePointer, section, key, defaultResponse);
    }
    
    
    public uint IniReadSection(IntPtr filePointer, byte[] section, byte[] buf, uint bufSize)
    {
      return ini_ReadSection(filePointer, section, buf, bufSize);
    }


    public UInt32 IniReadStruct(IntPtr filePointer,
                                byte[] section,
                                byte[] key,
                                byte[] buf,
                                UInt32 bufSize)
    {
      return ini_ReadStruct(filePointer, section, key, buf, bufSize);
    }
  }
}