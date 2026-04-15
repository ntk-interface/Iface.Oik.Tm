using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void cftNodeFreeTree(IntPtr id);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr cftNodeEnum(IntPtr id, Int32 idx);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial nint cftNodeEnumAll(nint id, int idx);

    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr cftNodeGetNext(IntPtr id);

    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial nint cftNodeGetNextAll(nint id);

    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial IntPtr cftNodeGetName(IntPtr     id,
                                                Span<byte> buf,
                                                UInt32     count);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial nint cftNPropEnum(nint       id,
                                            int        idx,
                                            Span<byte> buf,
                                            uint       count);


    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    public static partial nint cftNPropGetText(nint       id,
                                               string     name,
                                               Span<byte> buf,
                                               uint       count);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr cftNodeNewTree();


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNodeInsertAfter(IntPtr id,
                                                   byte[] nodeTag);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNodeInsertDown(IntPtr id,
                                                  byte[] nodeTag);


    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool cftNPropSet(nint   id,
                                           string propName,
                                           string propText);


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool cftNodeIsEnabled(nint id);


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall)]
    public static extern void cftNodeEnable(IntPtr id,
                                            bool   enable);
  }
}