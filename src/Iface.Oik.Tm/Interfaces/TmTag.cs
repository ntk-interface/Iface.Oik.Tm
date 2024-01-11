using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public abstract class TmTag : TmNotifyPropertyChanged
  {
    public TmType Type   { get; }
    public TmAddr TmAddr { get; }

    private bool                       _isInit;
    private DateTime?                  _changeTime;
    private string                     _name;
    private byte?                      _classId;
    private bool                       _hasTmProvider;
    private DateTime?                  _eventBlockTime;
    private Dictionary<string, string> _classData;
    private Dictionary<string, string> _properties;

    public bool IsInit
    {
      get => _isInit;
      set => SetPropertyValueAndRefresh(ref _isInit, value);
    }

    
    public DateTime? ChangeTime
    {
      get => _changeTime;
      set => SetPropertyValueAndRefresh(ref _changeTime, value);
    }


    public string Name
    {
      get => _name;
      protected set => SetPropertyValueAndRefresh(ref _name, value);
    }

    public byte? ClassId
    {
      get => _classId;
      protected set => SetPropertyValueAndRefresh(ref _classId, value);
    }

    public bool HasTmProvider
    {
      get => _hasTmProvider;
      protected set => SetPropertyValueAndRefresh(ref _hasTmProvider, value);
    }

    public DateTime? EventBlockTime
    {
      get => _eventBlockTime;
      set => SetPropertyValueAndRefresh(ref _eventBlockTime, value);
    }

    public Dictionary<string, string> ClassData
    {
      get => _classData;
      protected set => SetPropertyValueAndRefresh(ref _classData, value);
    }

    public Dictionary<string, string> Properties
    {
      get => _properties;
      private set => SetPropertyValueAndRefresh(ref _properties, value);
    }

    public abstract bool HasProblems { get; }

    public abstract string       ValueToDisplay { get; }
    public abstract List<string> FlagsToDisplay { get; }


    public bool IsStatus => this is TmStatus;
    public bool IsAnalog => this is TmAnalog;

    public bool IsClassDataLoaded => ClassId.HasValue ||
                                     ClassData != null;

    public ushort NativeType
    {
      get
      {
        switch (Type)
        {
          case TmType.Status:
            return (ushort) TmNativeDefs.TmDataTypes.Status;
          case TmType.Analog:
            return (ushort) TmNativeDefs.TmDataTypes.Analog;
          case TmType.Accum:
            return (ushort) TmNativeDefs.TmDataTypes.Accum;
          default:
            return 0;
        }
      }
    }


    public string TypeName
    {
      get
      {
        switch (Type)
        {
          case TmType.Status:
            return "Сигнал";
          case TmType.Analog:
            return "Измерение";
          case TmType.Accum:
            return "Аналоговое измерение";
          default:
            return "???";
        }
      }
    }

    public object Reference { get; set; } // ссылка на связанный объект, например для схемы - выключатель, прибор и т.п.


    protected TmTag(TmType type, uint addr)
      : this(new TmAddr(type, addr))
    {
    }


    protected TmTag(TmType type, int ch, int rtu, int point)
      : this(new TmAddr(type, ch, rtu, point))
    {
    }


    protected TmTag(TmAddr addr)
    {
      Type   = addr.Type;
      TmAddr = addr;
      Name   = addr.ToString();
    }


    public override int GetHashCode()
    {
      return TmAddr.GetHashCode();
    }


    public static TmTag Create(TmAddr addr)
    {
      if (addr == null) return null;

      switch (addr.Type)
      {
        case TmType.Analog:
          return new TmAnalog(addr);
        case TmType.Status:
          return new TmStatus(addr);
        case TmType.Accum:
          return new TmAccum(addr);
        default:
          return null;
      }
    }


    public static TmTag CreateFromTmcCommonPoint(TmNativeDefs.TCommonPoint commonPoint)
    {
      switch (((TmNativeDefs.TmDataTypes) commonPoint.Type).ToTmType())
      {
        case TmType.Accum:
          return TmAccum.CreateFromTmcCommonPointEx(commonPoint);
        
        case TmType.Analog:
          return TmAnalog.CreateFromTmcCommonPointEx(commonPoint);;
        
        case TmType.Status:
          return TmStatus.CreateFromTmcCommonPointEx(commonPoint);
        default:
          return null;
      }
    }


    public virtual void SetTmcObjectProperties(string tmcObjectPropertiesString)
    {
      Properties     = new Dictionary<string, string>();
      HasTmProvider  = false;
      EventBlockTime = null;
      
      var props = tmcObjectPropertiesString.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
      foreach (var prop in props)
      {
        var kvp = prop.Split('=');
        if (kvp.Length != 2)
        {
          continue;
        }
        SetTmcObjectProperties(kvp[0], kvp[1]);
      }
    }


    protected virtual void SetTmcObjectProperties(string key, string value)
    {
      Properties.AddWithUniquePostfixIfNeeded(key, value);

      if (key == "Name")
      {
        Name = value;
      }
      if (key == "Provider" && !string.IsNullOrEmpty(value))
      {
        HasTmProvider = true;
      }
      if (key == "EvUnblkTime" && !string.IsNullOrEmpty(value))
      {
        EventBlockTime = DateUtil.GetDateTimeFromReversedTmString(value);
      }
    }


    public void SetTmcClassData(string tmcClassData)
    {
      ClassData = new Dictionary<string, string>();
      var props = tmcClassData.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
      foreach (var prop in props)
      {
        var kvp = prop.Split('=');
        if (kvp.Length != 2)
        {
         continue; 
        }
        
        SetTmcClassDataItem(kvp[0], kvp[1]);
      }
    }


    private void SetTmcClassDataItem(string key, string value)
    {
      ClassData.Add(key, value);

      switch (key)
      {
        case "ClassNumber":
          ClassId = byte.TryParse(value, out var classId) ? classId : (byte?) null;
          break;
      }
    }
    

    public void SetBlockedEventsData(string name, DateTime time)
    {
      Name           = name;
      EventBlockTime = time;
    }
  }
}