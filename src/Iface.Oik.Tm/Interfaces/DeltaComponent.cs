using System;
using System.Collections.ObjectModel;
using System.Linq;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class DeltaComponent : TmNotifyPropertyChanged
  {
    private string _description;

    public string                               Name                   { get; }
    public DeltaComponentTypes                  Type                   { get; }
    public uint[]                               TraceChain             { get; }
    public string                               TraceChainString       { get; }
    public string                               ParentTraceChainString { get; }
    public DeltaComponent                       Parent                 { get; set; }
    public ObservableCollection<DeltaComponent> Children               { get; }
    public ObservableCollection<DeltaItem>      Items                  { get; }

    public string Description
    {
      get => _description;
      set
      {
        _description = value;
        Refresh();
      }
    }

    public string TraceChainLastLinkString => TraceChain.Last().ToString();


    public DeltaComponent(string name, string type, uint[] traceChain)
    {
      Name             = name;
      Type             = ParseComponentType(type);
      TraceChain       = traceChain;
      TraceChainString = string.Join("-", traceChain.Select(x => Convert.ToString(x, 10)));

      ParentTraceChainString =
        string.Join("-", traceChain.Take(traceChain.Length - 1).Select(x => Convert.ToString(x, 10)));

      Children = new ObservableCollection<DeltaComponent>();
      Items = new ObservableCollection<DeltaItem>();
    }

    public void ClearItems()
    {
      Description = string.Empty;
      Items.Clear();
    }

    private static DeltaComponentTypes ParseComponentType(string componentTypeString)
    {
      switch (componentTypeString)
      {
        case "0":
          return DeltaComponentTypes.Driver;
        case "1":
          return DeltaComponentTypes.Adapter;
        case "2":
          return DeltaComponentTypes.Port;
        case "3":
          return DeltaComponentTypes.Rtu;
        case "4":
          return DeltaComponentTypes.Array;
        case "5":
          return DeltaComponentTypes.Block;
        case "7":
          return DeltaComponentTypes.Info;
        default:
          return DeltaComponentTypes.Unknown;
      }
    }
  }
}