using System;
using System.Text;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public partial interface ITmNative
  {
    void CftNodeFreeTree(IntPtr id);
    
    
    IntPtr CftNodeEnum(IntPtr id,
                       Int32  count);

    
    IntPtr CftNodeGetName(IntPtr            id,
                          ref StringBuilder buf,
                          UInt32            count);

    
    IntPtr CftNPropEnum(IntPtr            id, 
                        Int32             idx, 
                        ref StringBuilder buf, 
                        UInt32            count);

    
    IntPtr CftNPropGetText(IntPtr            id, 
                           string            name, 
                           ref StringBuilder buf, 
                           UInt32            count);

    
    IntPtr CftNodeNewTree();

    
    IntPtr CftNodeInsertAfter(IntPtr id,
                              string nodeTag);

    
    IntPtr CftNodeInsertDown(IntPtr id,
                             string nodeTag);

    
    bool CftNPropSet(IntPtr id,
                     string propName,
                     string propText);
  }
}