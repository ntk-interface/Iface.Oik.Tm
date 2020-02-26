using System.Collections.Generic;

namespace Iface.Oik.Tm.Interfaces
{
  public abstract class TmTagFilter
  {
    public string                 Name              { get; private set; }
    public TmFlags                Flags             { get; private set; }
    public List<int>              ClassIdList       { get; }
    public TmTagFilterTimeOption? ChangeTimeOption  { get; private set; }
    public int                    ChangeTimeMinutes { get; private set; }
    public TmTagFilterTimeOption? UpdateTimeOption  { get; private set; }
    public int                    UpdateTimeMinutes { get; private set; }


    protected TmTagFilter()
    {
      ClassIdList = new List<int>();
    }


    public TmTagFilter AddName(string name)
    {
      return this; // сейчас не работает поиск в SQL
      /*Name = name;
      return this;*/
    }


    public TmTagFilter AddFlags(IEnumerable<TmFlags> flags)
    {
      foreach (var flag in flags)
      {
        Flags |= flag;
      }
      return this;
    }


    public TmTagFilter AddClassIdList(IEnumerable<int> classIdList)
    {
      ClassIdList.AddRange(classIdList);
      return this;
    }


    public TmTagFilter AddChangeTime(TmTagFilterTimeOption? option,
                                     int                    minutes)
    {
      ChangeTimeOption  = option;
      ChangeTimeMinutes = minutes;
      return this;
    }


    public TmTagFilter AddUpdateTime(TmTagFilterTimeOption? option,
                                     int                    minutes)
    {
      UpdateTimeOption  = option;
      UpdateTimeMinutes = minutes;
      return this;
    }


    protected static bool IsNullOrEmpty(TmTagFilter filter)
    {
      if (filter == null) return true;

      return filter.Name              == null         &&
             filter.Flags             == TmFlags.None &&
             filter.ClassIdList.Count == 0            &&
             filter.ChangeTimeOption  == null         &&
             filter.UpdateTimeOption  == null;
    }
  }


  public enum TmTagFilterTimeOption
  {
    IsLaterThan   = 1,
    IsEarlierThan = 2,
  }
}