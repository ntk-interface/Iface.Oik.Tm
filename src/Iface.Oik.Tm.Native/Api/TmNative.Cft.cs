using System;
using System.Text;

namespace Iface.Oik.Tm.Native.Api
{
  public partial class TmNative
  {
    public IntPtr CftNodeEnum(IntPtr id,
                              Int32  count)
    {
      return cftNodeEnum(id, count);
    }

    public void CftNodeFreeTree(IntPtr id)
    {
      cftNodeFreeTree(id);
    }

    public IntPtr CftNodeGetName(IntPtr            id,
                                 ref StringBuilder buf,
                                 UInt32            count)
    {
      return cftNodeGetName(id, buf, count);
    }

    public IntPtr CftNPropEnum(IntPtr            id,
                               Int32             idx,
                               ref StringBuilder buf,
                               uint              count)
    {
      return cftNPropEnum(id, idx, buf, count);
    }

    public IntPtr CftNPropGetText(IntPtr            id,
                                  string            name,
                                  ref StringBuilder buf,
                                  uint              count)
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
                            string propText)
    {
      return cftNPropSet(id, propName, propText);
    }
  }
}