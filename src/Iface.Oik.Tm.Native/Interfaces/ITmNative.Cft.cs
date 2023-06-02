using System;
using System.Text;

namespace Iface.Oik.Tm.Native.Interfaces
{
  public partial interface ITmNative
  {
    void CftNodeFreeTree(IntPtr id);
    
    
    IntPtr CftNodeEnum(IntPtr id,
                       Int32  count);

    
    IntPtr CftNodeEnumAll(IntPtr id,
                          Int32  count);
    
    
    IntPtr CftNodeGetName(IntPtr     id,
                          byte[] buf,
                          uint       count);

    
    IntPtr CftNPropEnum(IntPtr     id,
                        int        idx,
                        byte[] buf,
                        uint       count);

    
    IntPtr CftNPropGetText(IntPtr     id,
                           string     name,
                           byte[] buf,
                           uint       count);

    
    IntPtr CftNodeNewTree();

    
    IntPtr CftNodeInsertAfter(IntPtr id,
                              string nodeTag);

    
    IntPtr CftNodeInsertDown(IntPtr id,
                             string nodeTag);

    
    bool CftNPropSet(IntPtr id,
                     string propName,
                     byte[] propText);


    bool CftNodeIsEnabled(IntPtr id);


    void CftNodeEnable(IntPtr id, 
                       Boolean enable);
  }
}