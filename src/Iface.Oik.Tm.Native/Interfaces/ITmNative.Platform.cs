using System;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public partial interface ITmNative
  {
    string GetOikTaskExecutable(string origin);


    bool PlatformSetEvent(UInt32 hEvent);


    UInt32 PlatformWaitForSingleObject(UInt32 hHandle,
                                       UInt32 dwMilliseconds);
  }
}