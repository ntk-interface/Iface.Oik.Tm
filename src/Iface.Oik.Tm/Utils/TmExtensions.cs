using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Utils
{
  public static class TmExtensions
  {
    public static TmNativeDefs.TmDataTypes ToNativeType(this TmType tmType)
    {
      switch (tmType)
      {
        case TmType.Status:
          return TmNativeDefs.TmDataTypes.Status;
        case TmType.Analog:
          return TmNativeDefs.TmDataTypes.Analog;
        case TmType.Accum:
          return TmNativeDefs.TmDataTypes.Accum;
        default:
          return 0;
      }
    }
  }
}