using System;
using System.Buffers;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static (nint, DateTime) OpenConfigurationTree(nint cfCif, string host, string fileName)
  {
    var fileTime = new TmNativeDefsUnsafe.FileTime();

    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var cfTreeRoot = TmNative.cfsConfFileOpenCid(cfCif,
                                                   host,
                                                   fileName,
                                                   30000 | TmNativeDefs.FailIfNoConnect,
                                                   ref fileTime,
                                                   out var errCode,
                                                   errBuf,
                                                   TmNativeDefsUnsafe.ErrorBufSize);

      if (cfTreeRoot == nint.Zero)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      return (cfTreeRoot, TmNativeUtil.GetTimeFromFileTime(fileTime));
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  public static bool SaveConfigurationTree(nint treeHandle, string host, string fileName)
  {
    var fileTime = new TmNativeDefsUnsafe.FileTime();

    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var result = TmNative.cfsConfFileSaveAs(treeHandle,
                                             host,
                                             fileName,
                                             30000 | TmNativeDefs.FailIfNoConnect,
                                             ref fileTime,
                                             out var errCode,
                                             errBuf,
                                             TmNativeDefsUnsafe.ErrorBufSize);

      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      return result;
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  public static void FreeTreeHandle(nint nodeHandle)
  {
    TmNative.cftNodeFreeTree(nodeHandle);
  }
  
  
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
    Span<byte> nameBuf = stackalloc byte[TmNativeDefsUnsafe.CftNameBufSize];
    TmNative.cftNodeGetName(nodeHandle, nameBuf, TmNativeDefsUnsafe.CftNameBufSize);

    return TmNativeUtil.BytesToString(nameBuf);
  }

  public static string GetNodePropertyName(nint nodeHandle, int index)
  {
    Span<byte> nameBuf = stackalloc byte[TmNativeDefsUnsafe.CftNameBufSize];

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

  public static nint CreateNewTree()
  {
    return TmNative.cftNodeNewTree();
  }

  public static nint CreateChildNode(nint   parentHandle,
                                     string nodeTag)
  {
    var childHandle = TmNative.cftNodeInsertDown(parentHandle, nodeTag);
    

    return childHandle;
  }

  public static void SetNodeEnabledState(nint nodeHandle, 
                                         bool isEnabled)
  {
    TmNative.cftNodeEnable(nodeHandle, isEnabled);
  }
  
  public static bool SetNodeProperty(nint   nodeHandle,
                                     string propertyName,
                                     string propertyText)
  {
    return TmNative.cftNPropSet(nodeHandle, propertyName, propertyText);
  }
}