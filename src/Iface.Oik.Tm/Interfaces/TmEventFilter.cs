using System;
using System.Collections.Generic;
using System.Linq;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  // the type in use with JsonConvert Serialize/Deserialize
  public class TmEventFilter
  {
    public TmEventSource Source { get; set; }
    
    public bool          AreTmEventsForbidden    => Source == TmEventSource.UserActions;
    public bool          AreUserActionsForbidden => Source == TmEventSource.TmEvents;
    
    public DateTime?          StartTime           { get; set; }
    public DateTime?          EndTime             { get; set; }
    public TmEventTypes       Types               { get; set; }
    public TmEventImportances Importances         { get; set; }
    public List<TmAddr>       TmAddrList          { get; } = new List<TmAddr>();
    public List<int>          TmStatusClassIdList { get; set; }

    // channelNum -> { null(весь канал) | коллекция rtuNum }
    public Dictionary<int, HashSet<int>> ChannelAndRtuCollection { get; set; }


    public List<TmUserActionCategory> Categories { get; } = new List<TmUserActionCategory>();
    
    public int OutputLimit { get; set; }


    public bool IsBasicOnly => (Types       == 0 || Types       == TmEventTypes.Any)                   &&
                               (Importances == 0 || Importances == TmEventImportances.Any)             &&
                               TmAddrList.Count == 0                                                   &&
                               (TmStatusClassIdList     == null || TmStatusClassIdList.Count     == 0) &&
                               (ChannelAndRtuCollection == null || ChannelAndRtuCollection.Count == 0) &&
                               Categories.Count == 0                                                   &&
                               (Source == TmEventSource.Union)                                         &&
                               OutputLimit == 0;


    public bool IsAdvanced => !IsBasicOnly;


    public TmEventFilter()
    {
      Clear();
    }


    public TmEventFilter(DateTime? startTime, DateTime? endTime)
    {
      Clear();
      StartTime = startTime;
      EndTime   = endTime;
    }


    public TmEventFilter(string startTime, string endTime)
      : this(DateUtil.GetDateTime(startTime) ?? throw new ArgumentException(),
             DateUtil.GetDateTime(endTime)   ?? throw new ArgumentException())
    {
    }


    public TmEventFilter(DateTime? startTime, DateTime? endTime, TmEventTypes types, TmEventImportances importances)
      : this(startTime, endTime)
    {
      Types       = types;
      Importances = importances;
    }


    public TmEventFilter(string startTime, string endTime, TmEventTypes types, TmEventImportances importances)
      : this(DateUtil.GetDateTime(startTime) ?? throw new ArgumentException(),
             DateUtil.GetDateTime(endTime)   ?? throw new ArgumentException(),
             types,
             importances)
    {
    }


    public List<int> EnsureTmStatusClassIdList()
    {
      return TmStatusClassIdList ?? (TmStatusClassIdList = new List<int>());
    }


    public Dictionary<int, HashSet<int>> EnsureChannelAndRtuCollection()
    {
      return ChannelAndRtuCollection ?? (ChannelAndRtuCollection = new Dictionary<int, HashSet<int>>());
    }


    public void Clear()
    {
      Source      = TmEventSource.Union;
      StartTime   = null;
      EndTime     = null;
      Types       = 0;
      Importances = 0;
      TmAddrList.Clear();
      ChannelAndRtuCollection?.Clear();
      TmStatusClassIdList?.Clear();
      Categories.Clear();
    }


    public void SetTmAddr(TmAddr tmAddr)
    {
      ResetTmAddr();
      AddTmAddr(tmAddr);
    }


    public void AddTmAddr(TmAddr tmAddr)
    {
      TmAddrList.Add(tmAddr);
    }


    public void AddTmAddrRange(IEnumerable<TmAddr> tmAddr)
    {
      TmAddrList.AddRange(tmAddr);
    }


    public void ResetTmAddr()
    {
      TmAddrList.Clear();
    }


    public void SetAnyTypeAndImportance()
    {
      Types       = TmEventTypes.Any;
      Importances = TmEventImportances.Any;
    }


    public void SetZeroTypeAndImportance()
    {
      Types       = 0;
      Importances = 0;
    }


    public bool IsConform(TmEvent ev)
    {
      if (ev == null)
      {
        return false;
      }

      if (AreTmEventsForbidden)
      {
        return false;
      }

      if (Types             != TmEventTypes.None &&
          (Types & ev.Type) == 0)
      {
        return false;
      }

      if (Importances                       != TmEventImportances.None &&
          (Importances & ev.ImportanceFlag) == 0)
      {
        return false;
      }

      if (StartTime.HasValue &&
          ev.Time < StartTime)
      {
        return false;
      }

      if (EndTime.HasValue &&
          ev.Time > EndTime)
      {
        return false;
      }

      if (ev.TmAddrType       == TmType.Status &&
          TmStatusClassIdList != null          &&
          TmStatusClassIdList.All(classId => ev.TmClassId != classId))
      {
        return false;
      }

      if (ChannelAndRtuCollection != null &&
          !IsConformTmAddrComplexInteger(ev.TmAddrComplexInteger))
      {
        return false;
      }

      if (TmAddrList.Count != 0)
      {
        if (string.IsNullOrEmpty(ev.TmAddrString)               ||
            !TmAddr.TryParse(ev.TmAddrString, out var evTmAddr) ||
            !TmAddrList.Any(addr => evTmAddr.Equals(addr)))
        {
          return false;
        }
      }

      return true;
    }


    public bool IsConform(TmUserAction userAction)
    {
      if (userAction == null)
      {
        return false;
      }

      if (AreUserActionsForbidden)
      {
        return false;
      }

      if (Categories.Count == 0)
      {
        return true;
      }
      
      return Categories.Contains(userAction.Category);
    }


    public bool IsConformTmAddrComplexInteger(uint tma)
    {
      if (ChannelAndRtuCollection.IsNullOrEmpty()) return true;

      foreach (var chAndRtu in ChannelAndRtuCollection)
      {
        var channelId = chAndRtu.Key;
        var rtuList   = chAndRtu.Value;
        if (rtuList == null)
        {
          var (tmaStart, tmaEnd) = TmChannel.GetSqlTmaRange(channelId);
          if (tma >= tmaStart && tma <= tmaEnd)
          {
            return true;
          }
        }
        else
        {
          foreach (var rtuId in rtuList)
          {
            var (tmaStart, tmaEnd) = TmRtu.GetSqlTmaRange(channelId, rtuId);
            if (tma >= tmaStart && tma <= tmaEnd)
            {
              return true;
            }
          }
        }
      }

      return false;
    }


    public bool IsConformTmStatusClassId(int id)
    {
      if (TmStatusClassIdList       == null ||
          TmStatusClassIdList.Count == 0)
      {
        return true;
      }
      return TmStatusClassIdList.Contains(id);
    }


    public bool IsConformCategory(TmUserActionCategory category)
    {
      return Categories.Count == 0 || Categories.Contains(category);
    }


    public override string ToString()
    {
      if (!StartTime.HasValue || !EndTime.HasValue)
      {
        return string.Empty;
      }
      var result               = $"от {StartTime.Value.ToTmString()} до {EndTime.Value.ToTmString()}";
      var advancedFilterString = GetAdvancedFilterString();
      
      return !string.IsNullOrEmpty(advancedFilterString)
        ? $"{result}, фильтр выборки: {advancedFilterString}"
        : result;
    }


    public string GetAdvancedFilterString()
    {
      var filters = new List<string>();

      if (Source == TmEventSource.TmEvents)
      {
        filters.Add("Только события");
      }
      else if (Source == TmEventSource.UserActions)
      {
        filters.Add("Только действия пользователей");
      }
      
      if (Types != 0 && Types != TmEventTypes.Any)
      {
        var selectedTypes = Enum.GetValues(typeof(TmEventTypes))
                                .Cast<TmEventTypes>()
                                .Where(type => type > 0 && Types.HasFlag(type))
                                .Select(type => $"= \"{type.GetDescription()}\"");
        filters.Add($"(Тип {string.Join(" ИЛИ ", selectedTypes)})");
      }
      if (Categories.Count > 0)
      {
        var selectedCategories = Enum.GetValues(typeof(TmUserActionCategory))
                                     .Cast<TmUserActionCategory>()
                                     .Where(cat => Categories.Contains(cat))
                                     .Select(cat => $"= \"{cat.GetDescription()}\"");
        filters.Add($"(Категории {string.Join(" ИЛИ ", selectedCategories)})");
      }
      if (Importances != 0 && Importances != TmEventImportances.Any)
      {
        var selectedImportances = Enum.GetValues(typeof(TmEventImportances))
                                      .Cast<TmEventImportances>()
                                      .Where(imp => imp > 0 && Importances.HasFlag(imp))
                                      .Select(imp => $"= \"{imp.GetDescription()}\"");
        filters.Add($"(Важн. {string.Join(" ИЛИ ", selectedImportances)})");
      }
      if (!TmStatusClassIdList.IsNullOrEmpty())
      {
        filters.Add($"(Классы ТС содержатся в [{string.Join(",", TmStatusClassIdList)}])");
      }
      if (!ChannelAndRtuCollection.IsNullOrEmpty())
      {
        var channels = new List<string>();
        var rtus     = new List<string>();
        foreach (var chAndRtu in ChannelAndRtuCollection)
        {
          var channelId = chAndRtu.Key;
          var rtuList   = chAndRtu.Value;
          if (rtuList == null)
          {
            channels.Add(channelId.ToString());
          }
          else
          {
            foreach (var rtuId in rtuList)
            {
              rtus.Add($"{channelId}:{rtuId}");
            }
          }
        }
        if (channels.Count > 0)
        {
          filters.Add($"(Каналы содержатся в [{string.Join(",", channels)}])");
        }
        if (rtus.Count > 0)
        {
          filters.Add($"(КП содержатся в [{string.Join(",", rtus)}])");
        }
      }
      if (!TmAddrList.IsNullOrEmpty())
      {
        filters.Add($"(ТМ-адреса содержатся в [{string.Join(",", TmAddrList)}])");
      }
      if (OutputLimit > 0)
      {
        filters.Add($"(Лимит событий = {OutputLimit})");
      }

      return string.Join(" И ", filters);
    }
  }
}