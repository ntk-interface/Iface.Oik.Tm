using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Iface.Oik.Tm.Interfaces
{
  public class DeltaComponent
  {
    public string              Name             { get; }
    public DeltaComponentTypes Type             { get; }
    public string              TraceChain       { get; }
    public string              ParentTraceChain { get; }


    public DeltaComponent                       Parent   { get; set; }
    public ObservableCollection<DeltaComponent> Children { get; }

    public uint[] TraceChainArray => TraceChain.Split('-')
                                               .Select(x => Convert.ToUInt32(x, 10))
                                               .ToArray();

    public uint[] ParentTraceChainArray => ParentTraceChain.Split('-')
                                                           .Select(x => Convert.ToUInt32(x, 10))
                                                           .ToArray();

    public DeltaComponent(string name, string type, string traceChain, string parentTraceChain)
    {
      Name             = name;
      Type             = ParseComponentType(type);
      TraceChain       = traceChain;
      ParentTraceChain = parentTraceChain;
      Children         = new ObservableCollection<DeltaComponent>();
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