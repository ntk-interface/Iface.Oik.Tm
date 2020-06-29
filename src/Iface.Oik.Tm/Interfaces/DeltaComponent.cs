using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class DeltaComponent : TmNotifyPropertyChanged
  {
    private readonly int    _hashCode;
    private          string _description;

    public string                               Name                   { get; }
    public DeltaComponentTypes                  Type                   { get; }
    public uint[]                               TraceChain             { get; }
    public string                               TraceChainString       { get; }
    public string                               ParentTraceChainString { get; }
    public DeltaComponent                       Parent                 { get; set; }
    public ObservableCollection<DeltaComponent> Children               { get; }
    public ObservableCollection<DeltaItem>      Items                  { get; }
    public int                                  Level                  { get; }
    public DeltaComponentStates                 State                  { get; set; }
    public string                               Address                { get; set; }

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
    public string FullPathName             => Parent == null ? Name : $"{Parent.FullPathName} • {Name}";


    public DeltaComponent(string name, string type, uint[] traceChain)
    {
      _hashCode        = (name, type, traceChain).ToTuple().GetHashCode();
      Name             = name;
      Type             = ParseComponentType(type);
      TraceChain       = traceChain;
      TraceChainString = string.Join("-", traceChain.Select(x => Convert.ToString(x, 10)));

      ParentTraceChainString =
        string.Join("-", traceChain.Take(traceChain.Length - 1).Select(x => Convert.ToString(x, 10)));

      Children = new ObservableCollection<DeltaComponent>();
      Items    = new ObservableCollection<DeltaItem>();
      Level    = traceChain.Length;
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

    public override bool Equals(object obj)
    {
      return Equals(obj as DeltaComponent);
    }


    public bool Equals(DeltaComponent comparison)
    {
      if (ReferenceEquals(comparison, null))
      {
        return false;
      }

      if (ReferenceEquals(this, comparison))
      {
        return true;
      }

      var childrenHashSet           = new HashSet<DeltaComponent>(Children);
      var comparisonChildrenHashSet = new HashSet<DeltaComponent>(comparison.Children);

      return Name             == comparison.Name
          && Type             == comparison.Type
          && TraceChainString == comparison.TraceChainString
          && Items            == comparison.Items
          && State            == comparison.State
          && Level            == comparison.Level
          && childrenHashSet.SetEquals(comparisonChildrenHashSet);
    }


    public static bool operator ==(DeltaComponent left, DeltaComponent right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }

      return left.Equals(right);
    }

    public static bool operator !=(DeltaComponent left, DeltaComponent right)
    {
      return !(left == right);
    }

    public override int GetHashCode()
    {
      return _hashCode;
    }
  }
}