using System;
using System.IO;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    public string GetOikTaskExecutable(string origin)
    {
      DPrintF(origin);
      if (Path.GetExtension(origin) == ".dll")
      {
        var executableExtension = (PlatformUtil.IsWindows) ? ".exe" : "";
        var executable          = Path.ChangeExtension(origin, executableExtension);
        if (File.Exists(executable))
        {
          return executable;
        }
      }
      return origin;
    }


    public bool PlatformSetEvent(UInt32 hEvent)
    {
      return (PlatformUtil.IsWindows)
        ? SetEventWindows(hEvent)
        : SetEventLinux(hEvent);
    }


    public UInt32 PlatformWaitForSingleObject(UInt32 hHandle,
                                              UInt32 dwMilliseconds)
    {
      return (PlatformUtil.IsWindows)
        ? WaitForSingleObjectWindows(hHandle, dwMilliseconds)
        : WaitForSingleObjectLinux(hHandle, dwMilliseconds);
    }
  }
}