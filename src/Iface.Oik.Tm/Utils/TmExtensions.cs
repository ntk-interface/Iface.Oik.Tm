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
    
    
    public static TmNativeDefs.Flags ToNativeFlags(this TmFlags tmFlags)
    {
      return (TmNativeDefs.Flags)tmFlags;
    }
    
    
    public static TmNativeDefs.TmCpf ToNativeQueryFlags(this TmCommonPointFlags commonPointFlags)
    {
      return (TmNativeDefs.TmCpf)commonPointFlags;
    }
    

    public static byte ToEventLogImportanceByte(this TmEventImportances importances)
    {
      return importances switch
             {
               TmEventImportances.Imp0 => 0,
               TmEventImportances.Imp1 => 1,
               TmEventImportances.Imp2 => 2,
               TmEventImportances.Imp3 => 3,
               _                       => throw new TmNativeException("Важность не поддерживается")
             };
    }
  }
}