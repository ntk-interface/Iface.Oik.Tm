namespace Iface.Oik.Tm.Interfaces
{
  public readonly struct TmServerFeatures
  {
    public bool IsComtradeEnabled     { get; }
    public bool AreMicroSeriesEnabled { get; }


    public static TmServerFeatures Empty => new TmServerFeatures(false, false);


    public TmServerFeatures(bool isComtradeEnabled,
                            bool areMicroSeriesEnabled)
    {
      IsComtradeEnabled     = isComtradeEnabled;
      AreMicroSeriesEnabled = areMicroSeriesEnabled;
    }
  }
}