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
        case TmType.Channel:
          return TmNativeDefs.TmDataTypes.Channel;
        case TmType.Rtu:
          return TmNativeDefs.TmDataTypes.Rtu;
        case TmType.StatusGroup:
          return TmNativeDefs.TmDataTypes.StatusGroup;
        case TmType.AnalogGroup:
          return TmNativeDefs.TmDataTypes.AnalogGroup;
        case TmType.AccumGroup:
          return TmNativeDefs.TmDataTypes.AccumGroup;
        case TmType.RetroStatus:
          return TmNativeDefs.TmDataTypes.RetroStatus;
        case TmType.RetroAnalog:
          return TmNativeDefs.TmDataTypes.RetroAnalog;
        case TmType.RetroAccum:
          return TmNativeDefs.TmDataTypes.RetroAccum;
        default:
          return 0;
      }
    }
  }
}