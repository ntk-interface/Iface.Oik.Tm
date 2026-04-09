using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Utils
{
  public static class TmExtensions
  {
    public static TmNativeDefs.TmDataTypes ToNativeType(this TmType tmType)
    {
      return tmType switch
             {
               TmType.Status      => TmNativeDefs.TmDataTypes.Status,
               TmType.Analog      => TmNativeDefs.TmDataTypes.Analog,
               TmType.Accum       => TmNativeDefs.TmDataTypes.Accum,
               TmType.Channel     => TmNativeDefs.TmDataTypes.Channel,
               TmType.Rtu         => TmNativeDefs.TmDataTypes.Rtu,
               TmType.StatusGroup => TmNativeDefs.TmDataTypes.StatusGroup,
               TmType.AnalogGroup => TmNativeDefs.TmDataTypes.AnalogGroup,
               TmType.AccumGroup  => TmNativeDefs.TmDataTypes.AccumGroup,
               TmType.RetroStatus => TmNativeDefs.TmDataTypes.RetroStatus,
               TmType.RetroAnalog => TmNativeDefs.TmDataTypes.RetroAnalog,
               TmType.RetroAccum  => TmNativeDefs.TmDataTypes.RetroAccum,
               _                  => 0
             };
    }
  }
}