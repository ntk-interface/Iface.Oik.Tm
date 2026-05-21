using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Iface.Oik.Tm.Native.Api;

namespace Iface.Oik.Tm.Native.Utils
{
  public static class PlatformUtil
  {
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsLinux   => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static bool IsMacOS   => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    
    
    public static string GetOikTaskExecutable(string origin)
    {
      if (Path.GetExtension(origin) == ".dll")
      {
        var executableExtension = (IsWindows) ? ".exe" : "";
        var executable          = Path.ChangeExtension(origin, executableExtension);
        if (File.Exists(executable))
        {
          return executable;
        }
      }
      return origin;
    }


    public static bool PlatformSetEvent(IntPtr hEvent)
    {
      return IsWindows 
               ? TmNative.SetEventWindows(hEvent)
               : TmNative.SetEventLinux(hEvent);
    }


    public static UInt32 PlatformWaitForSingleObject(IntPtr hHandle,
                                                     UInt32 dwMilliseconds)
    {
      return IsWindows
               ? TmNative.WaitForSingleObjectWindows(hHandle, dwMilliseconds)
               : TmNative.WaitForSingleObjectLinux(hHandle, dwMilliseconds);
    }
  }
}