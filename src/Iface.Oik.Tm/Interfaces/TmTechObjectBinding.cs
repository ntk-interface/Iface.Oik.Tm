using System.Collections.Generic;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmTechObjectBinding
  {
    public static readonly string MeasurementClassName     = TmTechObjectBindingsMode.Measurement.GetDescription();
    public static readonly string DamageLocationClassName  = TmTechObjectBindingsMode.DamageLocation.GetDescription();
    public static readonly string TechParamsClassName      = TmTechObjectBindingsMode.TechParams.GetDescription();
    public static readonly string RelayProtectionClassName = TmTechObjectBindingsMode.RelayProtection.GetDescription();
    public static readonly string RegimeClassName          = TmTechObjectBindingsMode.Regime.GetDescription();
    public static readonly string PortableGroundClassName  = TmTechObjectBindingsMode.PortableGround.GetDescription();
    public static readonly string OperLockClassName        = TmTechObjectBindingsMode.OperLock.GetDescription();


    public TmTechObject             TmTechObject { get; set; }
    public TmTechObjectBindingType  BindingType  { get; set; }
    public TmTag                    TmTag        { get; set; }
    public string                   Name         { get; set; }
    public string                   Value        { get; set; }
    public TmTechObjectBindingsMode Mode         { get; private set; }

    public bool IsOperLock => Mode == TmTechObjectBindingsMode.OperLock;

    public string ModeName => Mode.GetDescription();


    public void UpdateMode(IDictionary<int, TmClassStatus> statusesClasses,
                           IDictionary<int, TmClassAnalog> analogsClasses)
    {
      switch (BindingType)
      {
        case TmTechObjectBindingType.TmTag:
          switch (TmTag)
          {
            case TmStatus status:
              statusesClasses.TryGetValue(status.ClassId ?? 0,
                                          out var statusClass);
              UpdateModeIfTmStatus(statusClass?.Name);
              break;

            case TmAnalog analog:
              analogsClasses.TryGetValue(analog.ClassId ?? 0,
                                         out var analogClass);
              UpdateModeIfTmAnalog(analogClass?.Name);
              break;
          }
          break;
        
        case TmTechObjectBindingType.TechParam:
          Mode = TmTechObjectBindingsMode.TechParams;
          break;
      }
    }


    private void UpdateModeIfTmStatus(string tmClassName)
    {
      if (tmClassName == OperLockClassName)
      {
        Mode = TmTechObjectBindingsMode.OperLock;
      }
      else if (tmClassName == RelayProtectionClassName)
      {
        Mode = TmTechObjectBindingsMode.RelayProtection;
      }
      else if (tmClassName == PortableGroundClassName)
      {
        Mode = TmTechObjectBindingsMode.PortableGround;
      }
      else
      {
        Mode = TmTechObjectBindingsMode.DamageLocation;
      }
    }


    private void UpdateModeIfTmAnalog(string tmClassName)
    {
      if (tmClassName == DamageLocationClassName)
      {
        Mode = TmTechObjectBindingsMode.DamageLocation;
      }
      else if (tmClassName == TechParamsClassName)
      {
        Mode = TmTechObjectBindingsMode.TechParams;
      }
      else if (tmClassName == RelayProtectionClassName)
      {
        Mode = TmTechObjectBindingsMode.RelayProtection;
      }
      else if (tmClassName == RegimeClassName)
      {
        Mode = TmTechObjectBindingsMode.Regime;
      }
      else
      {
        Mode = TmTechObjectBindingsMode.Measurement;
      }
    }
  }
}