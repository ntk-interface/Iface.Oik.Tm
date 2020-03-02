using System;
using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    [DllImport(Cfshare, EntryPoint = "Ipos_SetEvent", CallingConvention = CallingConvention.StdCall)]
    public static extern bool SetEventLinux(UInt32 hEvent);


    [DllImport(Cfshare, EntryPoint = "Ipos_WaitForSingleObject", CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 WaitForSingleObjectLinux(UInt32 hHandle,
                                                         UInt32 dwMilliseconds);


    [DllImport("kernel32.dll", EntryPoint = "SetEvent", CallingConvention = CallingConvention.StdCall)]
    public static extern bool SetEventWindows(UInt32 hEvent);


    [DllImport("kernel32.dll", EntryPoint = "WaitForSingleObject", CallingConvention = CallingConvention.StdCall)]
    public static extern UInt32 WaitForSingleObjectWindows(UInt32 hHandle,
                                                           UInt32 dwMilliseconds);
  }
}