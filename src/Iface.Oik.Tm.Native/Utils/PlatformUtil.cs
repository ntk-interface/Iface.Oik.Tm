using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Utils
{
  public static class PlatformUtil
  {
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsLinux   => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static bool IsMacOS   => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
  }
}