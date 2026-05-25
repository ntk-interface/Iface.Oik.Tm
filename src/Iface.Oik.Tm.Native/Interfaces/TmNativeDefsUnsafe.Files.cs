using System;
using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces;

internal partial class TmNativeDefsUnsafe
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct FileTime
  {
    public int dwLowDateTime;
    public int dwHighDateTime;
  }
}