using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Utils
{
  public static class TmNativeExtensions
  {
    public static TmType ToTmType(this TmNativeDefs.TmDataTypes nativeType)
    {
      return nativeType switch
             {
               TmNativeDefs.TmDataTypes.Status      => TmType.Status,
               TmNativeDefs.TmDataTypes.Analog      => TmType.Analog,
               TmNativeDefs.TmDataTypes.Accum       => TmType.Accum,
               TmNativeDefs.TmDataTypes.Channel     => TmType.Channel,
               TmNativeDefs.TmDataTypes.Rtu         => TmType.Rtu,
               TmNativeDefs.TmDataTypes.StatusGroup => TmType.StatusGroup,
               TmNativeDefs.TmDataTypes.AnalogGroup => TmType.AnalogGroup,
               TmNativeDefs.TmDataTypes.AccumGroup  => TmType.AccumGroup,
               TmNativeDefs.TmDataTypes.RetroStatus => TmType.RetroStatus,
               TmNativeDefs.TmDataTypes.RetroAnalog => TmType.RetroAnalog,
               TmNativeDefs.TmDataTypes.RetroAccum  => TmType.RetroAccum,
               _                                    => TmType.Unknown
             };
    }
  }
}