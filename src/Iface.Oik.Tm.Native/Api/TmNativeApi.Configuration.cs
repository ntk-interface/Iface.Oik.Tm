using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static nint NodeEnumAll(nint handle, int index)
  {
    return TmNative.cftNodeEnumAll(handle, index);
  }
  
  public static nint NodeGetNextAll(nint handle)
  {
    return TmNative.cftNodeGetNextAll(handle);
  }

  public static bool NodeIsEnabled(nint handle)
  {
    return TmNative.cftNodeIsEnabled(handle);
  }
  
  public static string GetNodeName(nint nodeHandle)
  {
    Span<byte> nameBuf       = stackalloc byte[TmNativeDefsUnsafe.CftNameBufSize];
    TmNative.cftNodeGetName(nodeHandle, nameBuf, TmNativeDefsUnsafe.CftNameBufSize);

    return TmNativeUtil.BytesToString(nameBuf);
  }
  
  public static string GetNodePropertyName(nint nodeHandle, int index)
  {
    Span<byte> nameBuf       = stackalloc byte[TmNativeDefsUnsafe.CftNameBufSize];

    TmNative.cftNPropEnum(nodeHandle, index, nameBuf, TmNativeDefsUnsafe.CftNameBufSize);

    return TmNativeUtil.BytesToString(nameBuf);
  }
  
  public static string GetNodePropertyValue(nint nodeHandle, string propName)
  {
    var ptr = TmNative.cftNPropGetText(nodeHandle, propName, Span<byte>.Empty, 0);

    if (ptr == nint.Zero)
    {
      return string.Empty;
    }
    
    var value = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(ptr);
    
    TmNative.cfsFreeMemory(ptr);
    
    return value;
  }
}