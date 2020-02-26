using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Iface.Oik.Tm.Native.Interfaces;

namespace Iface.Oik.Tm.Interfaces
{
  public abstract class TmTag : INotifyPropertyChanged
  {
    public bool IsInit { get; set; }

    public TmType Type      { get; }
    public TmAddr TmAddr    { get; }

    public string    Name       { get; protected set; }
    public DateTime? ChangeTime { get; set; }

    public byte?                      ClassId    { get; set; }
    public Dictionary<string, string> ClassData  { get; protected set; }
    public Dictionary<string, string> Properties { get; protected set; }

    public object Reference { get; set; } // ссылка на связанный объект, например для схемы - выключатель, прибор и т.п.

    public ushort TmcType
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
    
    
    public abstract string ValueToDisplay { get; }


    public bool IsStatus => this is TmStatus;
    public bool IsAnalog => this is TmAnalog;


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
        default:
          return null;
      }
    }


    public void SetTmcObjectProperties(StringBuilder tmcObjectProperties)
    {
      Properties = new Dictionary<string, string>();
      var props = tmcObjectProperties.ToString().Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
      foreach (var prop in props)
      {
        var kvp = prop.Split('=');
        if (kvp.Length != 2)
        {
          continue;
        }
        Properties.Add(kvp[0], kvp[1]);
        if (kvp[0] == "Name")
        {
          Name = kvp[1];
        }
        switch (this)
        {
          case TmAnalog tmAnalog:
          {
            if (kvp[0] == "Units")
            {
              tmAnalog.Unit = kvp[1];
            }
            if (kvp[0] == "Format")
            {
              var formatParts = kvp[1].Split('.');
              if (formatParts.Length > 1                        &&
                  byte.TryParse(formatParts[0], out byte width) &&
                  byte.TryParse(formatParts[1], out byte precision))
              {
                tmAnalog.Width     = width;
                tmAnalog.Precision = precision;
              }
            }
            if (kvp[0] == "FBFlagsC")
            {
              tmAnalog.Teleregulation = TmAnalog.GetRegulationFromNativeFlag(kvp[1]);
            }
            break;
          }
          case TmStatus tmStatus:
          {
            if (kvp[0] == "Normal")
            {
              if (int.TryParse(kvp[1], out var normalStatus))
              {
                tmStatus.NormalStatus = (short) ((normalStatus == 0 || normalStatus == 1) ? normalStatus : -1);
              }
            }
            if (kvp[0] == "Importance")
            {
              if (int.TryParse(kvp[1], out var importance))
              {
                tmStatus.Importance = (short) importance;
              }
            }
            break;
          }
        }
      }
    }


    public void SetTmcClassData(string tmcClassData)
    {
      ClassData = new Dictionary<string, string>();
      var props = tmcClassData.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
      foreach (var prop in props)
      {
        var kvp = prop.Split('=');
        if (kvp.Length == 2)
        {
          ClassData.Add(kvp[0], kvp[1]);
        }
      }
    }


    public event PropertyChangedEventHandler PropertyChanged;


    #region john added

    public void RefreshView()
    {
      OnPropertyChangedPrivate(new PropertyChangedEventArgs(string.Empty));
    }

    private void OnPropertyChangedPrivate(PropertyChangedEventArgs e)
    {
      var handler = PropertyChanged;
      handler?.Invoke(this, e);
    }

    public bool IsClassDataLoaded()
    {
      return ClassId.HasValue || ClassData != null;
    }

    #endregion

  }
}