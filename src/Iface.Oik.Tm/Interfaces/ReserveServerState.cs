namespace Iface.Oik.Tm.Interfaces
{
  public class ReserveServerState
  {
    public string                   RemotePipeName { get; set; } = string.Empty;
    public BroadcastServerSignature Signature      { get; set; }
    public bool                     IsWorking      { get; set; }
  }
}