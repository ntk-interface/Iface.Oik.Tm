using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static unsafe int ExecuteTeleregulationScript(int                               cid,
                                                       short                             ch,
                                                       short                             rtu,
                                                       short                             point,
                                                       TmNativeDefs.AnalogRegulationType command)
  {
    short value = 0;

    return TmNative.tmcExecuteRegulationScript(cid,
                                               ch,
                                               rtu,
                                               point,
                                               (byte)command,
                                               (nint)(&value));
  }
  
  
  public static unsafe int TeleregulateByValue(int   cid,
                                               short ch,
                                               short rtu,
                                               short point,
                                               float value)
  {
    return TmNative.tmcRegulationByAnalog(cid,
                                          ch,
                                          rtu,
                                          point,
                                          (byte)TmNativeDefs.AnalogRegulationType.Value,
                                          (nint)(&value));
  }


  public static unsafe int TeleregulateByCode(int   cid,
                                              short ch,
                                              short rtu,
                                              short point,
                                              short code)
  {
    return TmNative.tmcRegulationByAnalog(cid,
                                          ch,
                                          rtu,
                                          point,
                                          (byte)TmNativeDefs.AnalogRegulationType.Code,
                                          (nint)(&code));
  }


  public static unsafe int TeleregulateByStepUpOrDown(int   cid,
                                                      short ch,
                                                      short rtu,
                                                      short point,
                                                      bool  isStepUp)
  {
    var stepValue = (short)(isStepUp ? 1 : -1); 
    
    return TmNative.tmcRegulationByAnalog(cid,
                                          ch,
                                          rtu,
                                          point,
                                          (byte)TmNativeDefs.AnalogRegulationType.Step,
                                          (nint)(&stepValue));
  }
}