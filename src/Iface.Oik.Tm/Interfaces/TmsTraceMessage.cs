using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmsTraceMessage : TmNotifyPropertyChanged
  {
    public string       Text { get; }
    public TmTraceTypes Type { get; }

    public TmsTraceMessage(TmTraceTypes type, string text)
    {
      Type = type;
      Text = text;
    }
  }
}