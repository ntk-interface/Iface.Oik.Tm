using System;
using System.Runtime.InteropServices;

namespace Iface.Oik.Tm.Native.Interfaces;

internal static partial class TmNativeDefsUnsafe
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaCommon
  {
    public byte Type;
    public byte Length;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaStatus
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public byte Value;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaStatus2
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public byte Value;
    public byte HiNum;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaAnalog
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public int Value;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaAnalog2
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public int  Value;
    public byte HiNum;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaAnalogF
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public float Value;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaAnalogF2
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public float Value;
    public byte  HiNum;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaAccum
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public int Value;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaAccum2
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public int  Value;
    public byte HiNum;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaAccumF
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public float Value;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaAccumF2
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public float Value;
    public byte  HiNum;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaControl
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public ushort CtrlBlock;
    public ushort CtrlGroup;
    public ushort CtrlPoint;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaControl2
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;

    public ushort CtrlBlock;
    public ushort CtrlGroup;
    public ushort CtrlPoint;
    public byte   HiNum;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaSliceTime
  {
    public byte  Type;
    public byte  Length;
    public ulong Current;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaDescription
  {
    public byte Type;
    public byte Length;
   
    // Text of unknown size at the end  
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DeltaStrval
  {
    public byte   Type;
    public byte   Length;
    public ushort Number;
    public int    LastUpdate;
    public ushort DeltaFlags;
    public ushort TmsChn;
    public ushort TmsRtu;
    public ushort TmsPoint;
    
    // Text of unknown size at the end  
  }
}