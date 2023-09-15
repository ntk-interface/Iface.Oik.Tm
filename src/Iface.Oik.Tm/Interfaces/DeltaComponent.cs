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
    private          string _description                    = string.Empty;
    private          string _statusPerformanceString        = string.Empty;
    private          string _statusInstantPerformanceString = string.Empty;
    private          long   _statusesReceived;
    private          long   _instantStatusesReceived;
    private          string _analogPerformanceString        = string.Empty;
    private          string _analogInstantPerformanceString = string.Empty;
    private          long   _analogsReceived;
    private          long   _instantAnalogsReceived;
    private          string _accumPerformanceString        = string.Empty;
    private          string _accumInstantPerformanceString = string.Empty;
    private          long   _accumsReceived;
    private          long   _instantAccumsReceived;
    private          string _messagesPerformanceString        = string.Empty;
    private          string _messagesInstantPerformanceString = string.Empty;
    private          long   _messagesReceived;
    private          long   _instantMessagesReceived;

    public string                               Name                    { get; }
    public DeltaComponentTypes                  Type                    { get; }
    public uint[]                               TraceChain              { get; }
    public string                               TraceChainString        { get; }
    public string                               ParentTraceChainString  { get; }
    public DeltaComponent                       Parent                  { get; set; }
    public ObservableCollection<DeltaComponent> Children                { get; }
    public ObservableCollection<DeltaItem>      Items                   { get; }
    public int                                  Level                   { get; }
    public DeltaComponentStates                 State                   { get; set; }
    public string                               Address                 { get; set; }
    public DeltaComponentPerformanceStats       InitialPerformanceStats { get; private set; }
    public DeltaComponentPerformanceStats       LastPerformanceStats    { get; private set; }

    public string Description
    {
      get => _description;
      set
      {
        _description = value;
        Refresh();
      }
    }

    public string StatusPerformanceString
    {
      get => _statusPerformanceString;
      set
      {
        _statusPerformanceString = value;
        Refresh();
      }
    }

    public string StatusInstantPerformanceString
    {
      get => _statusInstantPerformanceString;
      set
      {
        _statusInstantPerformanceString = value;
        Refresh();
      }
    }

    public long StatusesReceived
    {
      get => _statusesReceived;
      set
      {
        _statusesReceived = value;
        Refresh();
      }
    }
    
    public long InstantStatusesReceived
    {
      get => _instantStatusesReceived;
      set
      {
        _instantStatusesReceived = value;
        Refresh();
      }
    }

    public string AnalogPerformanceString
    {
      get => _analogPerformanceString;
      set
      {
        _analogPerformanceString = value;
        Refresh();
      }
    }

    public string AnalogInstantPerformanceString
    {
      get => _analogInstantPerformanceString;
      set
      {
        _analogInstantPerformanceString = value;
        Refresh();
      }
    }

    public long AnalogsReceived
    {
      get => _analogsReceived;
      set
      {
        _analogsReceived = value;
        Refresh();
      }
    }
    
    public long InstantAnalogsReceived
    {
      get => _instantAnalogsReceived;
      set
      {
        _instantAnalogsReceived = value;
        Refresh();
      }
    }

    public string AccumPerformanceString
    {
      get => _accumPerformanceString;
      set
      {
        _accumPerformanceString = value;
        Refresh();
      }
    }

    public string AccumInstantPerformanceString
    {
      get => _accumInstantPerformanceString;
      set
      {
        _accumInstantPerformanceString = value;
        Refresh();
      }
    }

    public long InstantAccumsReceived
    {
      get => _instantAccumsReceived;
      set
      {
        _instantAccumsReceived = value;
        Refresh();
      }
    }
    
    public long AccumsReceived
    {
      get => _accumsReceived;
      set
      {
        _accumsReceived = value;
        Refresh();
      }
    }

    public string MessagesPerformanceString
    {
      get => _messagesPerformanceString;
      set
      {
        _messagesPerformanceString = value;
        Refresh();
      }
    }

    public string MessagesInstantPerformanceString
    {
      get => _messagesInstantPerformanceString;
      set
      {
        _messagesInstantPerformanceString = value;
        Refresh();
      }
    }

    public long MessagesReceived
    {
      get => _messagesReceived;
      set
      {
        _messagesReceived = value;
        Refresh();
      }
    }
    
    public long InstantMessagesReceived
    {
      get => _instantMessagesReceived;
      set
      {
        _instantMessagesReceived = value;
        Refresh();
      }
    }

    public string TraceChainLastLinkString => TraceChain.Last().ToString();
    public string FullPathName             => Parent == null ? Name : $"{Parent.FullPathName} • {Name}";
    public bool   HasChildren              => Children.Any();


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

      Level = traceChain.Length;

      var driverNum = (traceChain[0] - 0x80000000) >> 24;

      switch (traceChain.Length)
      {
        case 2:
          Address = $"D{driverNum}:A{traceChain[1]}";
          break;
        case 3:
          Address = $"D{driverNum}:A{traceChain[1]}:P{traceChain[2]}";
          break;
      }
    }
    
    public bool TryUpdateItems(IReadOnlyCollection<DeltaItem> newItems, string newDescription)
    {
      var updated = false;
      if (newDescription != Description)
      {
        Description = newDescription;
        updated     = true;
      }
      
      if (!Items.SequenceEqual(newItems))
      {
        Items.Clear();
        Items.AddRange(newItems);
        updated = true;
      }

      return updated;
    }
    

    public void SetInitialPerformanceStats(long ticks,
                                           long statusCount,
                                           long analogCount,
                                           long accumCount,
                                           long messageCount)
    {
      InitialPerformanceStats = new DeltaComponentPerformanceStats
      {
        Ticks        = ticks,
        StatusCount  = statusCount,
        AnalogCount  = analogCount,
        AccumCount   = accumCount,
        MessageCount = messageCount
      };

      LastPerformanceStats = new DeltaComponentPerformanceStats
      {
        Ticks        = ticks,
        StatusCount  = statusCount,
        AnalogCount  = analogCount,
        AccumCount   = accumCount,
        MessageCount = messageCount
      };
    }

    public void UpdatePerformanceStatsAndString(long ticks,
                                                long statusCount,
                                                long analogCount,
                                                long accumCount,
                                                long messageCount)
    {
      StatusesReceived = statusCount  - InitialPerformanceStats.StatusCount;
      AnalogsReceived  = analogCount  - InitialPerformanceStats.AnalogCount;
      AccumsReceived   = accumCount   - InitialPerformanceStats.AccumCount;
      MessagesReceived = messageCount - InitialPerformanceStats.MessageCount;
      
      InstantStatusesReceived = statusCount  - LastPerformanceStats.StatusCount;
      InstantAnalogsReceived  = analogCount  - LastPerformanceStats.AnalogCount;
      InstantAccumsReceived   = accumCount   - LastPerformanceStats.AccumCount;
      InstantMessagesReceived = messageCount - LastPerformanceStats.MessageCount;

      StatusInstantPerformanceString = GetCalculatedPerformanceString(LastPerformanceStats.Ticks,
                                                                      ticks,
                                                                      InstantStatusesReceived );
      StatusPerformanceString = GetCalculatedPerformanceString(InitialPerformanceStats.Ticks,
                                                               ticks,
                                                               StatusesReceived);

      AnalogInstantPerformanceString = GetCalculatedPerformanceString(LastPerformanceStats.Ticks,
                                                                      ticks,
                                                                      InstantAnalogsReceived);
      AnalogPerformanceString = GetCalculatedPerformanceString(InitialPerformanceStats.Ticks,
                                                               ticks,
                                                               AnalogsReceived);
      
      AccumInstantPerformanceString = GetCalculatedPerformanceString(LastPerformanceStats.Ticks,
                                                                     ticks,
                                                                     InstantAccumsReceived);
      AccumPerformanceString = GetCalculatedPerformanceString(InitialPerformanceStats.Ticks,
                                                              ticks,
                                                              AccumsReceived);
      
      MessagesInstantPerformanceString = GetCalculatedPerformanceString(LastPerformanceStats.Ticks,
                                                                        ticks,
                                                                        InstantMessagesReceived);
      MessagesPerformanceString = GetCalculatedPerformanceString(InitialPerformanceStats.Ticks,
                                                                 ticks,
                                                                 MessagesReceived);

      StatusesReceived = statusCount - InitialPerformanceStats.StatusCount;

      LastPerformanceStats.Ticks        = ticks;
      LastPerformanceStats.StatusCount  = statusCount;
      LastPerformanceStats.AnalogCount  = analogCount;
      LastPerformanceStats.AccumCount   = accumCount;
      LastPerformanceStats.MessageCount = messageCount;
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

      return Name                == comparison.Name
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

    private static string GetCalculatedPerformanceString(long ticks1, long ticks2, long countDiff)
    {
      var result = countDiff / (float) (ticks2 - ticks1) * 1000;
      return result.ToString("N3");
    }
  }

  public class DeltaComponentPerformanceStats
  {
    public long Ticks        { get; set; }
    public long StatusCount  { get; set; }
    public long AnalogCount  { get; set; }
    public long AccumCount   { get; set; }
    public long MessageCount { get; set; }
  }
}