using System;
using System.Text;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    public IntPtr CftNodeEnum(IntPtr id,
                              Int32  idx)
    {
      return cftNodeEnum(id, idx);
    }
    
    
    public IntPtr CftNodeEnumAll(IntPtr id,
                                 Int32  idx)
    {
      return cftNodeEnumAll(id, idx);
    }
    

    public void CftNodeFreeTree(IntPtr id)
    {
      cftNodeFreeTree(id);
    }

    public IntPtr CftNodeGetName(IntPtr     id,
                                 byte[] buf,
                                 uint       count)
    {
      return cftNodeGetName(id, buf, count);
    }

    public IntPtr CftNPropEnum(IntPtr     id,
                               int        idx,
                               byte[] buf,
                               uint       count)
    {
      return cftNPropEnum(id, idx, buf, count);
    }

    public IntPtr CftNPropGetText(IntPtr     id,
                                  string     name,
                                  byte[] buf,
                                  uint       count)
    {
      return cftNPropGetText(id, name, buf, count);
    }

    public IntPtr CftNodeNewTree()
    {
      return cftNodeNewTree();
    }

    public IntPtr CftNodeInsertAfter(IntPtr id,
                                     string nodeTag)
    {
      return cftNodeInsertAfter(id, nodeTag);
    }

    public IntPtr CftNodeInsertDown(IntPtr id,
                                    string nodeTag)
    {
      return cftNodeInsertDown(id, nodeTag);
    }

    public bool CftNPropSet(IntPtr id,
                            string propName,
                            byte[] propText)
    {
      return cftNPropSet(id, propName, propText);
    }

    public bool CftNodeIsEnabled(IntPtr id)
    {
      return cftNodeIsEnabled(id);
    }

    public void CftNodeEnable(IntPtr id, 
                              Boolean enable)
    {
      cftNodeEnable(id, enable);
    }
  }
}