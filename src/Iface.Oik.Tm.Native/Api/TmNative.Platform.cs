using System;
using System.IO;
using System.Text;
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


    public bool PlatformSetEvent(IntPtr hEvent)
    {
      return (PlatformUtil.IsWindows)
        ? SetEventWindows(hEvent)
        : SetEventLinux(hEvent);
    }


    public UInt32 PlatformWaitForSingleObject(IntPtr hHandle,
                                              UInt32 dwMilliseconds)
    {
      return (PlatformUtil.IsWindows)
        ? WaitForSingleObjectWindows(hHandle, dwMilliseconds)
        : WaitForSingleObjectLinux(hHandle, dwMilliseconds);
    }

    public string PlatformWin1251BytesToUtf8(byte[] inputBuffer)
    {
      var buffer = new byte[inputBuffer.Length * 3 + 1];

      var result = xmlMBToUTF8(inputBuffer, buffer, (uint) buffer.Length);

      return result ? Encoding.UTF8.GetString(buffer).Trim('\0') : string.Empty;
    }
  }
}