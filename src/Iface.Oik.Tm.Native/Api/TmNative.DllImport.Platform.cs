using System;
using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    [DllImport(Cfshare, EntryPoint = "Ipos_SetEvent", CallingConvention = CallingConvention.StdCall)]
    public static extern bool SetEventLinux(IntPtr hEvent);


    [DllImport(Cfshare, EntryPoint = "Ipos_WaitForSingleObject", CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 WaitForSingleObjectLinux(IntPtr hHandle,
                                                         UInt32 dwMilliseconds);


    [DllImport("kernel32.dll", EntryPoint = "SetEvent", CallingConvention = CallingConvention.StdCall)]
    public static extern bool SetEventWindows(IntPtr hEvent);


    [DllImport("kernel32.dll", EntryPoint = "WaitForSingleObject", CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 WaitForSingleObjectWindows(IntPtr hHandle,
                                                           UInt32 dwMilliseconds);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern Boolean xmlMBToUTF8([In]      byte[] uText,
                                             [In, Out] byte[] uConvertedText,
                                             uint             cbuConvertedText);


    [DllImport(Cfshare, CallingConvention = CallingConvention.Cdecl)]
    public static extern Boolean xmlUTF8ToMB(byte[]           uText,
                                             [In, Out] byte[] uConvertedText,
                                             uint             cbuConvertedText);
  }
}