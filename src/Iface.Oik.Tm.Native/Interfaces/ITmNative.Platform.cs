using System;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public partial interface ITmNative
  {
    string GetOikTaskExecutable(string origin);


    bool PlatformSetEvent(IntPtr hEvent);


    UInt32 PlatformWaitForSingleObject(IntPtr hHandle,
                                       UInt32 dwMilliseconds);
  }
}