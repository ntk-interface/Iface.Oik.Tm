using System;
using System.ComponentModel;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmEvent : TmNotifyPropertyChanged
  {
    private readonly int _hashCode;

    public TmEventElix  Elix                 { get; private set; } // may be null?
    public DateTime?    Time                 { get; private set; } // sql: update_time
    public string       Text                 { get; private set; } // sql: { name | rec_text }
    public string       StateString          { get; private set; } // sql: rec_state_text
    public TmEventTypes Type                 { get; private set; } // sql: rec_type
    public string       TypeString           { get; private set; } // sql: { rec_type_name | tm_type_name }
    public string       Username             { get; private set; }
    public int          Importance           { get; private set; }
    public int          TmClassId            { get; private set; }
    public TmType       TmAddrType           { get; private set; }
    public uint         TmAddrComplexInteger { get; private set; } // sql: tma // one-based, 0=none
    public string       TmAddrString         { get; private set; }

    private int       _num;
    private DateTime? _ackTime;
    private string    _ackUser;

    public int Num
    {
      get => _num;
      set
      {
        _num = value;
        NotifyOfPropertyChange();
      }
    }

    public DateTime? AckTime
    {
      get => _ackTime;
      set
      {
        _ackTime = value;
        NotifyOfPropertyChange();
        NotifyOfPropertyChange(nameof(IsAcked));
      }
    }

    public string AckUser
    {
      get => _ackUser;
      set
      {
        _ackUser = value;
        NotifyOfPropertyChange();
      }
    }

    public string             ImportanceAlias => ImportanceToAlias(Importance);
    public string             ImportanceName  => ImportanceToName(Importance);
    public TmEventImportances ImportanceFlag  => ImportanceToFlag(Importance);
    public bool               HasTmAddr       => TmAddrType != 0;
    public bool               HasTmStatus     => TmAddrType == TmType.Status;
    public bool               HasTmAnalog     => TmAddrType == TmType.Analog;
    public bool               IsAcked         => AckTime    != null; // TODO


    public TmEvent(int hashCode)
    {
      _hashCode = hashCode;
    }


    public static TmEvent CreateFromDto(TmEventDto dto)
    {
      return CreateFromDto(dto.Elix,
                           dto.UpdateTime,
                           dto.RecText,
                           dto.Name,
                           dto.RecStateText,
                           dto.RecType,
                           dto.RecTypeName,
                           dto.UserName,
                           dto.Importance,
                           dto.Tma,
                           dto.TmaStr,
                           dto.TmTypeName,
                           dto.TmType,
                           dto.ClassId,
                           dto.AckTime,
                           dto.AckUser);
    }


    public static TmEvent CreateFromDto(byte[]    elixBytes,
                                        DateTime? time,
                                        string    text,
                                        string    name,
                                        string    stateString,
                                        short     type,
                                        string    typeString,
                                        string    username,
                                        short     importance,
                                        int       tmAddrComplexInteger, // one-based, 0=none
                                        string    tmAddrString,
                                        string    tmTypeString,
                                        short?    tmAddrNativeType,
                                        short?    classId,
                                        DateTime? ackTime,
                                        string    ackUser)
    {
      var eventType = (TmEventTypes) type;

      var eventTypeString = (eventType == TmEventTypes.StatusChange    ||
                             eventType == TmEventTypes.ManualStatusSet ||
                             eventType == TmEventTypes.Alarm)
        ? tmTypeString
        : typeString;

      var eventText = (eventType == TmEventTypes.Extended)
        ? text
        : name;

      var tmEvent = new TmEvent(elixBytes != null ? BitConverter.ToInt32(elixBytes, 8) : 0)
      {
        Time                 = time.NullIfEpoch(),
        Text                 = eventText,
        StateString          = stateString?.Trim(),
        Type                 = eventType,
        TypeString           = eventTypeString,
        Username             = username,
        Importance           = importance,
        TmClassId            = classId ?? -1,
        TmAddrType           = ((TmNativeDefs.TmDataTypes) (ushort) (tmAddrNativeType ?? 0)).ToTmType(),
        TmAddrComplexInteger = (uint) tmAddrComplexInteger,
        AckTime              = ackTime.NullIfEpoch(),
        AckUser              = ackUser,
      };

      if (elixBytes != null)
      {
        tmEvent.Elix = TmEventElix.CreateFromByteArray(elixBytes);
      }

      if (tmAddrComplexInteger != 0)
      {
        tmEvent.TmAddrString = tmAddrString;
        if (false)
        {
          TmAddr.TryParseType(tmAddrString, out var tmType, TmType.Unknown);
          tmEvent.TmAddrType = tmType;
        }
      }
      else
      {
        tmEvent.TmAddrString = null;
        tmEvent.TmAddrType   = TmType.Unknown;
      }

      return tmEvent;
    }


    public override int GetHashCode()
    {
      return _hashCode != 0
        ? _hashCode
        : base.GetHashCode();
    }


    public static TmEventImportances ImportanceToFlag(int importance)
    {
      return (TmEventImportances) (1 << importance);
    }


    public static string ImportanceToAlias(int importance)
    {
      switch (importance)
      {
        case 0:
          return "ОС";
        case 1:
          return "ПС2";
        case 2:
          return "ПС1";
        case 3:
          return "АС";
        default:
          return string.Empty;
      }
    }


    public static string ImportanceToName(int importance)
    {
      switch (importance)
      {
        case 0:
          return "Оперативного состояния";
        case 1:
          return "Предупредительные 2";
        case 2:
          return "Предупредительные 1";
        case 3:
          return "Аварийные";
        default:
          return string.Empty;
      }
    }
  }
}