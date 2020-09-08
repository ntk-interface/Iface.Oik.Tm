using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr ini_Open([MarshalAs(UnmanagedType.LPStr)] string filePath);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void ini_Flush(IntPtr filePointer);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void ini_Close(IntPtr filePointer);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void ini_Reload(IntPtr filePointer);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 ini_ReadString(IntPtr                                    filePointer,
                                               [MarshalAs(UnmanagedType.LPStr)]   string section,
                                               [MarshalAs(UnmanagedType.LPStr)]   string key,
                                               [MarshalAs(UnmanagedType.LPStr)]   string defaultResponse,
                                               [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
                                               UInt32                                    bufSize);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Int32 ini_ReadInteger(IntPtr                                  filePointer,
                                               [MarshalAs(UnmanagedType.LPStr)] string section,
                                               [MarshalAs(UnmanagedType.LPStr)] string key,
                                               Int32                                   defaultResponse);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 ini_ReadSection(IntPtr                                    filePointer,
                                                [MarshalAs(UnmanagedType.LPStr)]   string section,
                                                [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
                                                UInt32                                    bufSize);
  }
}