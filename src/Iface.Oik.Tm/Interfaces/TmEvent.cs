using System;
using System.ComponentModel;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
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
    public object       Reference            { get; private set; }
    public DateTime?    FixTime              { get; private set; }

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
                           dto.TmaType,
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


    public static TmEvent CreateFromTEventElix(TmNativeDefs.TEventElix       tEventElix,
                                               TmNativeDefs.TTMSEventAddData eventAddData,
                                               string                        sourceObjectName)
    {
      var tmEventElix = new TmEventElix(tEventElix.Elix.R, tEventElix.Elix.M);
      var tmEventType = (TmEventTypes) tEventElix.Event.Id;

      var tmEvent = new TmEvent(BitConverter.ToInt32(tmEventElix.ToByteArray(), 8))
      {
        Elix         = tmEventElix,
        Time         = DateUtil.GetDateTime(System.Text.Encoding.Default.GetString(tEventElix.Event.DateTime)),
        Type         = tmEventType,
        Importance   = tEventElix.Event.Imp,
        TmAddrString = GetTmAddrStringFromTEvent(tEventElix.Event),
        TmAddrType   = GetTmTypeFromTEvent(tEventElix.Event),
        Text         = sourceObjectName,
      };

      if (eventAddData.AckSec != 0 && !eventAddData.UserName.IsNullOrEmpty())
      {
        tmEvent.AckTime = DateUtil.GetDateTimeFromTimestamp(eventAddData.AckSec, eventAddData.AckMs);
        tmEvent.AckUser = eventAddData.UserName;
      }

      switch (tmEventType)
      {
        case TmEventTypes.StatusChange:
          var                  statusData = TmNativeUtil.GetStatusDataFromTEvent(tEventElix.Event);
          var                  isS2Only   = false;
          TmNativeDefs.S2Flags s2         = 0x0000;
          tmEvent.TypeString = statusData.Class == 1 ? "А.П.С." : "ТС";

          if ((statusData.ExtSig & TmNativeDefs.ExtendedDataSignature) == TmNativeDefs.ExtendedDataSignature)
          {
            var extSigFlags = (TmNativeDefs.ExtendedDataSignatureFlag) (statusData.ExtSig ^
                                                                        TmNativeDefs.ExtendedDataSignature);

            if (extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.FixTime))
            {
              tmEvent.FixTime = DateUtil.GetDateTimeFromTimestamp(statusData.FixUT, statusData.FixMS);
            }

            if (extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.TmFlags))
            {
              tmEvent.Reference = tmEvent.Reference == null
                                    ? $"F=${statusData.Flags:X8}"
                                    : $"{tmEvent.Reference} F=${statusData.Flags:X8}";
            }

            if (extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.Secondary))
            {
              tmEvent.Reference = tmEvent.Reference == null ? "(s)" : $"{tmEvent.Reference} (s)";
            }

            if (!extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.CurData))
            {
              tmEvent.Reference = tmEvent.Reference == null ? "(d)" : $"{tmEvent.Reference} (d)";
            }

            if (extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.S2))
            {
              isS2Only = (statusData.S2                        & 0x8000) != 0;
              s2       = (TmNativeDefs.S2Flags) (statusData.S2 & 0x7fff);
            }
          }

          if (isS2Only)
          {
            tmEvent.StateString = tmEvent.StateString.IsNullOrEmpty()
                                    ? "ИЗМ. АТРИБУТОВ"
                                    : $"{tmEvent.StateString} ИЗМ. АТРИБУТОВ";
            tmEvent.StateString = s2 == 0
                                    ? $"{tmEvent.StateString} - НОРМА"
                                    : $"{tmEvent.StateString} {GetS2StatusString(s2)}";
          }
          else
          {
            var stateString = statusData.State == 1 ? "ВКЛ" : "ОТКЛ";

            tmEvent.StateString = tmEvent.StateString.IsNullOrEmpty()
                                    ? $"{stateString}"
                                    : $"{tmEvent.StateString} {stateString}";
            if (s2 != 0)
            {
              tmEvent.StateString = $"{tmEvent.StateString} {GetS2StatusString(s2)}";
            }
          }

          break;

        case TmEventTypes.Alarm:
          var alarmData = TmNativeUtil.GetAlarmDataFromTEvent(tEventElix.Event);
          tmEvent.TypeString  = "УСТАВКА";
          tmEvent.StateString = $"{alarmData.Val} - {(alarmData.State == 0 ? "Снята" : "Взведена")}";

          break;

        case TmEventTypes.ManualAnalogSet:
          var analogSetData = TmNativeUtil.GetAnalogSetDataFromTEvent(tEventElix.Event);
          tmEvent.TypeString  = "РУЧ. ТИТ";
          tmEvent.Username    = EncodingUtil.Cp866BytesToUtf8String(analogSetData.UserName);
          tmEvent.StateString = $"{analogSetData.Value} - {(analogSetData.Cmd == 0 ? "Снято" : "Установлено")}";
          break;

        case TmEventTypes.ManualStatusSet: //TODO: Изучить вопрос, не стоит ли сделать как в клиенте
          var mSData = TmNativeUtil.GetControlDataFromTEvent(tEventElix.Event);
          tmEvent.TypeString  = "РУЧ. ТС";
          tmEvent.Username    = EncodingUtil.Cp866BytesToUtf8String(mSData.UserName);
          tmEvent.StateString = mSData.Cmd == 1 ? "ВКЛ" : "ОТКЛ";
          break;

        case TmEventTypes.Control:
          var controlData = TmNativeUtil.GetControlDataFromTEvent(tEventElix.Event);
          tmEvent.TypeString  = "ТУ";
          tmEvent.Username    = EncodingUtil.Cp866BytesToUtf8String(controlData.UserName);
          tmEvent.StateString = controlData.Cmd == 1 ? "ВКЛ" : "ОТКЛ";
          if (controlData.Result != TmNativeDefs.Success)
          {
            tmEvent.StateString = $"{tmEvent.StateString} ОШИБКА";
          }

          break;

        case TmEventTypes.Acknowledge:
          var ackData = TmNativeUtil.GetAcknowledgeDataFromTEvent(tEventElix.Event);
          tmEvent.TypeString = "КВИТИРОВАНИЕ";
          tmEvent.Username   = EncodingUtil.Cp866BytesToUtf8String(ackData.UserName);
          if (tEventElix.Event.Point == 0)
          {
            tmEvent.Text = "ОБЩЕЕ";
          }

          break;
        case TmEventTypes.Extended:
          var strBinData   = TmNativeUtil.GetStrBinData(tEventElix.Event);
          var extendedType = (TmNativeDefs.ExtendedEventTypes) tEventElix.Event.Ch;
          tmEvent.TypeString = GetTypeStringByExtendedTypeString(extendedType);
          tmEvent.Text       = TmNativeUtil.GetStringFromStrBinBytes(strBinData.StrBin);

          switch (extendedType)
          {
            case TmNativeDefs.ExtendedEventTypes.Message:
              if (strBinData.Source < 0x10000)
              {
                tmEvent.TmAddrString = $"Источник: {strBinData.Source}";
              }
              else
              {
                tmEvent.TmAddrString =
                  $"Ист: {strBinData.Source          & 0xffff}, " +
                  $"К: {(strBinData.Source  >> 0x24) & 0xff}, "   +
                  $"КП: {(strBinData.Source >> 0x16) & 0xff}";
              }

              break;
            case TmNativeDefs.ExtendedEventTypes.Model:
              tmEvent.TmAddrString = $"Источник: {strBinData.Source}";
              break;
            default:
              tmEvent.TmAddrString = "???";
              break;
          }

          break;
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


    private static string GetTmAddrStringFromTEvent(TmNativeDefs.TEvent tEvent)
    {
      switch ((TmEventTypes) tEvent.Id)
      {
        case TmEventTypes.StatusChange:
        case TmEventTypes.Control:
        case TmEventTypes.ManualStatusSet:
        case TmEventTypes.Acknowledge:
          return $"#TC{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";

        case TmEventTypes.Alarm:
        case TmEventTypes.ManualAnalogSet:
          return $"#ТТ{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";
        default:
          return string.Empty;
      }
    }


    private static TmType GetTmTypeFromTEvent(TmNativeDefs.TEvent tEvent)
    {
      switch ((TmEventTypes) tEvent.Id)
      {
        case TmEventTypes.StatusChange:
        case TmEventTypes.Control:
        case TmEventTypes.ManualStatusSet:
        case TmEventTypes.Acknowledge:
          return TmType.Status;

        case TmEventTypes.Alarm:
        case TmEventTypes.ManualAnalogSet:
          return TmType.Analog;
        default:
          return TmType.Unknown;
      }
    }


    private static string GetS2StatusString(TmNativeDefs.S2Flags s2Flag)
    {
      switch (s2Flag)
      {
        case TmNativeDefs.S2Flags.Break:
          return $"[{s2Flag:D}] <ОБРЫВ>";
        case TmNativeDefs.S2Flags.Malfunction:
          return $"[{s2Flag:D}] <НЕИСП.>";
        default:
          return $"[{s2Flag:D}]";
      }
    }


    private static string GetTypeStringByExtendedTypeString(TmNativeDefs.ExtendedEventTypes eventType)
    {
      switch (eventType)
      {
        case TmNativeDefs.ExtendedEventTypes.Message:
          return "СООБЩЕНИЕ";
        case TmNativeDefs.ExtendedEventTypes.Model:
          return "МОДЕЛЬ";
        default:
          return "???";
      }
    }
  }
}