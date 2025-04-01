using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr ini_Open(byte[] filePath);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void ini_Flush(IntPtr filePointer);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void ini_Close(IntPtr filePointer);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void ini_Reload(IntPtr filePointer);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 ini_ReadString(IntPtr                                    filePointer,
                                               byte[]                                    section,
                                               byte[]                                    key,
                                               byte[]                                    defaultResponse,
                                               [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
                                               UInt32                                    bufSize);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern Int32 ini_ReadInteger(IntPtr filePointer,
                                               byte[] section,
                                               byte[] key,
                                               Int32  defaultResponse);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 ini_ReadSection(IntPtr                                    filePointer,
                                                byte[]                                    section,
                                                [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
                                                UInt32                                    bufSize);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern UInt32 ini_ReadStruct(IntPtr                                    filePointer,
                                               byte[]                                    section,
                                               byte[]                                    key,
                                               [MarshalAs(UnmanagedType.LPArray)] byte[] buf,
                                               UInt32                                    bufSize);
  }
}