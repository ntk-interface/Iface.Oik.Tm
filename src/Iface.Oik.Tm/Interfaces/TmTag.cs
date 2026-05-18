using System;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Dto;
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

    public ushort NativeType => Type switch
                                {
                                  TmType.Status => (ushort)TmNativeDefs.TmDataTypes.Status,
                                  TmType.Analog => (ushort)TmNativeDefs.TmDataTypes.Analog,
                                  TmType.Accum  => (ushort)TmNativeDefs.TmDataTypes.Accum,
                                  _             => 0
                                };


    public string TypeName => Type switch
                              {
                                TmType.Status => "Сигнал",
                                TmType.Analog => "Измерение",
                                TmType.Accum  => "Интегральное измерение",
                                _             => "???"
                              };

    
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

      return addr.Type switch
             {
               TmType.Analog => new TmAnalog(addr),
               TmType.Status => new TmStatus(addr),
               TmType.Accum  => new TmAccum(addr),
               _             => null
             };
    }


    public static TmTag CreateFromCommonPointDto(TCommonPointDto dto)
    {
      var tmTag = Create(new TmAddr(((TmNativeDefs.TmDataTypes)dto.Type).ToTmType(),
                                    dto.Ch,
                                    dto.Rtu,
                                    dto.Point));
      tmTag.Name = dto.Name ?? string.Empty;
      tmTag.FromCommonPointDto(dto);
      
      return tmTag;
    }


    public static TmTag CreateFromTmcCommonPoint(TmNativeDefs.TCommonPoint commonPoint)
    {
      return ((TmNativeDefs.TmDataTypes)commonPoint.Type).ToTmType() switch
             {
               TmType.Status => TmStatus.CreateFromTmcCommonPointEx(commonPoint),
               TmType.Analog => TmAnalog.CreateFromTmcCommonPointEx(commonPoint),
               TmType.Accum  => TmAccum.CreateFromTmcCommonPointEx(commonPoint),
               _             => null,
             };
    }


    public abstract void FromCommonPointDto(TCommonPointDto dto);


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