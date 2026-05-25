using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial void cftNodeFreeTree(nint id);


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


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial nint cftNodeNewTree();


    [DllImport(Cfshare, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr cftNodeInsertAfter(IntPtr id,
                                                   byte[] nodeTag);


    [LibraryImport(Cfshare, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial nint cftNodeInsertDown(nint   id,
                                                   string nodeTag);


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


    [LibraryImport(Cfshare)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial void cftNodeEnable(nint                                 id,
                                               [MarshalAs(UnmanagedType.Bool)] bool enable);
  }
}