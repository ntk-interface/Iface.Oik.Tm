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
        case TmNativeDefs.TmDataTypes.Channel:
          return TmType.Channel;
        case TmNativeDefs.TmDataTypes.Rtu:
          return TmType.Rtu;
        case TmNativeDefs.TmDataTypes.StatusGroup:
          return TmType.StatusGroup;
        case TmNativeDefs.TmDataTypes.AnalogGroup:
          return TmType.AnalogGroup;
        case TmNativeDefs.TmDataTypes.AccumGroup:
          return TmType.AccumGroup;
        default:
          return TmType.Unknown;
      }
    }
  }
}