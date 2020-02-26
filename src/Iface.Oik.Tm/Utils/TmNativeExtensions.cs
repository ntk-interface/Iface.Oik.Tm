using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Utils
{
  public static class TmNativeExtensions
  {
    public static TmType ToTmType(this TmNativeDefs.TmDataTypes nativeType)
    {
      switch (nativeType)
      {
        case TmNativeDefs.TmDataTypes.Status:
          return TmType.Status;
        case TmNativeDefs.TmDataTypes.Analog:
          return TmType.Analog;
        case TmNativeDefs.TmDataTypes.Accum:
          return TmType.Accum;
        default:
          return TmType.Unknown;
      }
    }
  }
}