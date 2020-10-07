namespace Iface.Oik.Tm.Interfaces
{
  public readonly struct TmServerFeatures
  {
    public bool IsComtradeEnabled       { get; }
    public bool AreMicroSeriesEnabled   { get; }
    public bool IsImpulseArchiveEnabled { get; }
    public bool AreTechObjectsEnabled   { get; }


    public static TmServerFeatures Empty => new TmServerFeatures();


    public TmServerFeatures(bool isComtradeEnabled       = false,
                            bool areMicroSeriesEnabled   = false,
                            bool isImpulseArchiveEnabled = false, 
                            bool areTechObjectsEnabled   = false)
    {
      IsComtradeEnabled       = isComtradeEnabled;
      AreMicroSeriesEnabled   = areMicroSeriesEnabled;
      IsImpulseArchiveEnabled = isImpulseArchiveEnabled;
      AreTechObjectsEnabled   = areTechObjectsEnabled;
    }
  }
}