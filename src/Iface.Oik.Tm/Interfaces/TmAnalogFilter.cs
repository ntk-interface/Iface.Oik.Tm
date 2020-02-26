namespace Iface.Oik.Tm.Interfaces
{
  public class TmAnalogFilter : TmTagFilter
  {
    private TmAnalogFilter()
    {
    }


    public static TmAnalogFilter Create()
    {
      return new TmAnalogFilter();
    }


    public static bool IsNullOrEmpty(TmAnalogFilter filter)
    {
      if (filter == null) return true;

      return TmTagFilter.IsNullOrEmpty(filter);
    }
  }
}