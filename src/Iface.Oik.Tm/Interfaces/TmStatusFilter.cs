namespace Iface.Oik.Tm.Interfaces
{
  public class TmStatusFilter : TmTagFilter
  {
    public int?       Status  { get; set; }
    public TmS2Flags? S2Flags { get; set; }


    private TmStatusFilter()
    {
    }


    public static TmStatusFilter Create()
    {
      return new TmStatusFilter();
    }


    public TmStatusFilter AddS2Status(int? s2Status)
    {
      switch (s2Status)
      {
        case 0b00:
          Status  = null;
          S2Flags = TmS2Flags.Break;
          break;
        case 0b01:
          Status  = 0;
          S2Flags = TmS2Flags.None;
          break;
        case 0b10:
          Status  = 1;
          S2Flags = TmS2Flags.None;
          break;
        case 0b11:
          Status  = null;
          S2Flags = TmS2Flags.Malfunction;
          break;
        default:
          Status  = null;
          S2Flags = null;
          break;
      }
      return this;
    }


    public int? GetS2Status()
    {
      if (Status == null && S2Flags == TmS2Flags.Break)
      {
        return 0b00;
      }
      if (Status == 0 && S2Flags == TmS2Flags.None)
      {
        return 0b01;
      }
      if (Status == 1 && S2Flags == TmS2Flags.None)
      {
        return 0b10;
      }
      if (Status == null && S2Flags == TmS2Flags.Malfunction)
      {
        return 0b11;
      }
      return null;
    }


    public static bool IsNullOrEmpty(TmStatusFilter filter)
    {
      if (filter == null) return true;

      return TmTagFilter.IsNullOrEmpty(filter) &&
             filter.Status  == null            &&
             filter.S2Flags == null;
    }
  }
}