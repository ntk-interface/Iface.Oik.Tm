using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmEvent : TmEventNotifiable
  {
    private int _hashCode;

    public TmEventElix  Elix                 { get; private set; } // may be null?
    public DateTime?    Time                 { get; private set; } // sql: update_time
    public string       Text                 { get; private set; } // sql: { name | rec_text }
    public string       StateString          { get; private set; } // sql: rec_state_text
    public string       ExplicitStateString  { get; private set; }
    public TmEventTypes Type                 { get; private set; } // sql: rec_type
    public string       TypeString           { get; private set; } // sql: { rec_type_name | tm_type_name }
    public string       ExplicitTypeString   { get; private set; }
    public string       Username             { get; private set; }
    public int          Importance           { get; private set; }
    public int          TmClassId            { get; private set; }
    public TmType       TmAddrType           { get; private set; }
    public uint         TmAddrComplexInteger { get; private set; } // sql: tma // one-based, 0=none
    public string       TmAddrString         { get; private set; }
    public object       Reference            { get; private set; }
    public DateTime?    FixTime              { get; private set; }
    public bool         IsFromReserve        { get; private set; } // EVL_ST_EXTF_SECONDARY, sql: ts_add_flags[4]

    private int       _num;
    private DateTime? _ackTime;
    private string    _ackUser;

    public int Num
    {
      get => _num;
      set
      {
        _num = value;
        //NotifyOfPropertyChange();
      }
    }

    public DateTime? AckTime
    {
      get => _ackTime;
      set
      {
        _ackTime = value;
        // NotifyOfPropertyChange();
        // NotifyOfPropertyChange(nameof(IsAcked));
      }
    }

    public string AckUser
    {
      get => _ackUser;
      set
      {
        _ackUser = value;
        // NotifyOfPropertyChange();
      }
    }

    public string             ImportanceAlias => ImportanceToAlias(Importance);
    public string             ImportanceName  => ImportanceToName(Importance);
    public TmEventImportances ImportanceFlag  => ImportanceToFlag(Importance);
    public bool               HasTmAddr       => TmAddrType != 0;
    public bool               HasTmStatus     => TmAddrType == TmType.Status;
    public bool               HasTmAnalog     => TmAddrType == TmType.Analog;
    public bool               IsAcked         => AckTime    != null; // TODO

    public float? AlarmInitialValue => Type == TmEventTypes.Alarm && Reference != null
                                         ? (float?)Reference
                                         : null;

    public TmEventChangedStatus ChangedStatus => Type == TmEventTypes.StatusChange && Reference != null
                                                   ? (TmEventChangedStatus)Reference
                                                   : null;

    public TmEvent()
    {
    }

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
                           dto.AlarmActive,
                           dto.VVal,
                           dto.VCode,
                           dto.VS2,
                           dto.Flags,
                           dto.TsAddFlags,
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
                                        bool?     isAlarmActive,
                                        float?    alarmInitialValue,
                                        short?    statusCode,
                                        short?    statusS2Flags,
                                        int?      statusFlags,
                                        BitArray  tsExtraFlags,
                                        DateTime? ackTime,
                                        string    ackUser)
    {
      var eventType = (TmEventTypes)type;

      var eventTypeString = eventType == TmEventTypes.StatusChange    ||
                            eventType == TmEventTypes.ManualStatusSet ||
                            eventType == TmEventTypes.Alarm
                              ? tmTypeString
                              : typeString;

      object reference = null;
      if (eventType     == TmEventTypes.Alarm &&
          isAlarmActive == true)
      {
        reference = alarmInitialValue;
      }
      else if (eventType == TmEventTypes.StatusChange)
      {
        reference = TmEventChangedStatus.CreateFromDto(statusCode, statusFlags, statusS2Flags);
      }

      string eventText;
      string eventStateString;
      if (eventType == TmEventTypes.Extended)
      {
        if (string.IsNullOrEmpty(name))
        {
          eventText        = text;
          eventStateString = stateString?.Trim();
        }
        else
        {
          eventText        = name;
          eventStateString = text;
        }
      }
      else
      {
        eventText        = name;
        eventStateString = stateString?.Trim();
      }

      var tmEvent = new TmEvent(elixBytes != null ? BitConverter.ToInt32(elixBytes, 8) : 0)
      {
        Time                 = time.NullIfEpoch(),
        Text                 = eventText,
        StateString          = eventStateString,
        Type                 = eventType,
        TypeString           = eventTypeString,
        Username             = username,
        Importance           = importance,
        TmClassId            = classId ?? -1,
        TmAddrType           = ((TmNativeDefs.TmDataTypes)(ushort)(tmAddrNativeType ?? 0)).ToTmType(),
        TmAddrComplexInteger = (uint)tmAddrComplexInteger,
        AckTime              = ackTime.NullIfEpoch(),
        AckUser              = ackUser,
        IsFromReserve        = tsExtraFlags != null && tsExtraFlags.Count > 4 && tsExtraFlags[4] == true,
        Reference            = reference,
      };

      if (elixBytes != null)
      {
        tmEvent.Elix = TmEventElix.CreateFromByteArray(elixBytes);
      }

      if (tmAddrComplexInteger != 0)
      {
        tmEvent.TmAddrString = tmAddrString;
      }
      else
      {
        tmEvent.TmAddrString = null;
        tmEvent.TmAddrType   = TmType.Unknown;
      }

      return tmEvent;
    }


    public static TmEvent CreateAlarmTmEvent(TmNativeDefs.TEvent           tEvent,
                                             TmNativeDefs.TTMSEventAddData eventAddData,
                                             string                        alarmTypeName,
                                             TmAnalog                      sourceAnalog,
                                             TmNativeDefs.AlarmData        data,
                                             TmNativeDefs.TTMSElix?        elix = null)
    {
      var alarmEvent = CreateFromTEvent(tEvent, eventAddData, sourceAnalog.Name, elix);

      alarmEvent.TmAddrString = $"#TT{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";
      alarmEvent.TmAddrComplexInteger =
        (uint)(tEvent.Point + (tEvent.Rtu << 16) + (tEvent.Ch << 24));
      alarmEvent.TmAddrType = TmType.Analog;

      alarmEvent.TypeString         = alarmTypeName;
      alarmEvent.ExplicitTypeString = "Уставка";

      if (data.State > 0)
      {
        alarmEvent.StateString = "Взведена";
        alarmEvent.Reference   = data.Val;
      }
      else
      {
        alarmEvent.StateString = "Снята";
      }

      alarmEvent.ExplicitStateString = $"{data.Val} - {alarmEvent.StateString}";

      return alarmEvent;
    }


    protected override void InitializeAlarmEvent(InitializeAlarmEventDto dto)
    {
      InitializeTmEvent(dto);

      TmAddrString = $"#TT{dto.Ch}:{dto.Rtu}:{dto.Point}";
      TmAddrComplexInteger =
        (uint)(dto.Point + (dto.Rtu << 16) + (dto.Ch << 24));
      TmAddrType = TmType.Analog;

      TypeString                    = dto.TypeName;
      ExplicitTypeString = "Уставка";

      if (dto.TurnedOn/*dto.State > 0*/)
      {
        StateString = "Взведена";
        Reference   = dto.Value;
      }
      else
      {
        StateString = "Снята";
      }

      ExplicitStateString = $"{dto.Value} - {StateString}";
    }
    


    public static TmEvent CreateManualAnalogSetEvent(TmNativeDefs.TEvent           tEvent,
                                                     TmNativeDefs.TTMSEventAddData eventAddData,
                                                     TmAnalog                      setAnalog,
                                                     TmNativeDefs.AnalogSetData    analogSetData,
                                                     TmNativeDefs.TTMSElix?        elix = null)
    {
      var manualAnalogSetEvent = CreateFromTEvent(tEvent, eventAddData, setAnalog.Name, elix);

      manualAnalogSetEvent.TmAddrString = $"#TT{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";
      manualAnalogSetEvent.TmAddrComplexInteger =
        (uint)(tEvent.Point + (tEvent.Rtu << 16) + (tEvent.Ch << 24));
      manualAnalogSetEvent.TmAddrType = TmType.Analog;

      manualAnalogSetEvent.TypeString         = "Ручн. ТИТ";
      manualAnalogSetEvent.ExplicitTypeString = "Ручн. ТИТ";

      manualAnalogSetEvent.Username = EncodingUtil.Cp866BytesToUtf8(analogSetData.UserName);

      manualAnalogSetEvent.StateString =
        $"{analogSetData.Value.ToString($"N{setAnalog.Precision}")}{(setAnalog.Unit.IsNullOrEmpty() ? "" : $" {setAnalog.Unit}")}";
      manualAnalogSetEvent.ExplicitStateString =
        $"{analogSetData.Value} - {(analogSetData.Cmd == 0 ? "Снято" : "Установлено")}";

      return manualAnalogSetEvent;
    }


    protected override void InitializeManualAnalogSetEvent(InitializeManualAnalogSetEventDto dto)
    { 
      InitializeTmEvent(dto);
      
      TmAddrString = $"#TT{dto.Ch}:{dto.Rtu}:{dto.Point}";
      TmAddrComplexInteger =
        (uint)(dto.Point + (dto.Rtu << 16) + (dto.Ch << 24));
      TmAddrType = TmType.Analog;

      TypeString         = "Ручн. ТИТ";
      ExplicitTypeString = "Ручн. ТИТ";

      StateString =
        $"{dto.Value.ToString($"N{dto.PropsAndClassData.Precision}")}{(string.IsNullOrEmpty(dto.PropsAndClassData.Units) 
                                                                         ? string.Empty 
                                                                         : $" {dto.PropsAndClassData.Units}")}";
      ExplicitStateString =
        $"{dto.Value} - {(dto.Command ? "Установлено" : "Снято" )}";
    }


    public static TmEvent CreateManualStatusSetEvent(TmNativeDefs.TEvent           tEvent,
                                                     TmNativeDefs.TTMSEventAddData eventAddData,
                                                     TmStatus                      setStatus,
                                                     TmNativeDefs.ControlData      mSData,
                                                     TmNativeDefs.TTMSElix?        elix = null)
    {
      var manualStatusSetEvent = CreateFromTEvent(tEvent, eventAddData, setStatus.Name, elix);

      manualStatusSetEvent.TmAddrString = $"#TС{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";
      manualStatusSetEvent.TmAddrComplexInteger =
        (uint)(tEvent.Point + (tEvent.Rtu << 16) + (tEvent.Ch << 24));
      manualStatusSetEvent.TmAddrType = TmType.Status;

      manualStatusSetEvent.Username = EncodingUtil.Cp866BytesToUtf8(mSData.UserName);


      manualStatusSetEvent.ExplicitTypeString  = "Ручн. ТС";
      manualStatusSetEvent.ExplicitStateString = mSData.Cmd == 1 ? "ВКЛ" : "ОТКЛ";

      if (mSData.Ch == -1)
      {
        switch (mSData.Rtu)
        {
          case 1:
            manualStatusSetEvent.TypeString  = setStatus.Flag1Name;
            manualStatusSetEvent.StateString = mSData.Cmd == 1 ? setStatus.CaptionFlag1On : setStatus.CaptionFlag1Off;
            break;
          case 2:
            manualStatusSetEvent.TypeString  = setStatus.Flag2Name;
            manualStatusSetEvent.StateString = mSData.Cmd == 1 ? setStatus.CaptionFlag2On : setStatus.CaptionFlag2Off;
            break;
          case 3:
            manualStatusSetEvent.TypeString  = setStatus.Flag3Name;
            manualStatusSetEvent.StateString = mSData.Cmd == 1 ? setStatus.CaptionFlag3On : setStatus.CaptionFlag3Off;
            break;
          case 4:
            manualStatusSetEvent.TypeString  = setStatus.Flag4Name;
            manualStatusSetEvent.StateString = mSData.Cmd == 1 ? setStatus.CaptionFlag4On : setStatus.CaptionFlag4Off;
            break;
        }
      }
      else
      {
        manualStatusSetEvent.TypeString  = "Ручн. ТС";
        manualStatusSetEvent.StateString = mSData.Cmd == 1 ? setStatus.CaptionOn : setStatus.CaptionOff;
      }


      return manualStatusSetEvent;
    }

    protected override void InitializeManualStatusSetEvent(InitializeManualStatusSetEventDto dto)
    {
      InitializeTmEvent(dto);
      
      TmAddrString = $"#TС{dto.Ch}:{dto.Rtu}:{dto.Point}";
      TmAddrComplexInteger =
        (uint)(dto.Point + (dto.Rtu << 16) + (dto.Ch << 24));
      TmAddrType = TmType.Status;

      ExplicitTypeString  = "Ручн. ТС";
      ExplicitStateString = dto.Command ? "ВКЛ" : "ОТКЛ";

      if (dto.ACh == -1)
      {
        switch (dto.ARtu)
        {
          case 1:
            TypeString  = dto.PropsAndClassData.Flag1Name;
            StateString = dto.Command ? dto.PropsAndClassData.CaptionFlag1On : dto.PropsAndClassData.CaptionFlag1Off;
            break;
          case 2:
            TypeString  = dto.PropsAndClassData.Flag2Name;
            StateString = dto.Command ? dto.PropsAndClassData.CaptionFlag2On : dto.PropsAndClassData.CaptionFlag2Off;
            break;
          case 3:
            TypeString  = dto.PropsAndClassData.Flag3Name;
            StateString = dto.Command ? dto.PropsAndClassData.CaptionFlag3On : dto.PropsAndClassData.CaptionFlag3Off;
            break;
          case 4:
            TypeString  = dto.PropsAndClassData.Flag4Name;
            StateString = dto.Command ? dto.PropsAndClassData.CaptionFlag4On : dto.PropsAndClassData.CaptionFlag4Off;
            break;
        }
      }
      else
      {
        TypeString  = "Ручн. ТС";
        StateString = dto.Command ? dto.PropsAndClassData.CaptionOn : dto.PropsAndClassData.CaptionOff;
      }
    }
    

    public static TmEvent CreateAcknowledgeEvent(TmNativeDefs.TEvent           tEvent,
                                                 TmNativeDefs.TTMSEventAddData eventAddData,
                                                 string                        sourceObjectName,
                                                 TmNativeDefs.AcknowledgeData  acknowledgeData,
                                                 TmNativeDefs.TTMSElix?        elix = null)
    {
      var acknowledgeEvent = CreateFromTEvent(tEvent, eventAddData, sourceObjectName, elix);

      acknowledgeEvent.TmAddrComplexInteger =
        (uint)(tEvent.Point + (tEvent.Rtu << 16) + (tEvent.Ch << 24));

      acknowledgeEvent.TmAddrType = ((TmNativeDefs.TmDataTypes)acknowledgeData.TmType).ToTmType();

      switch (acknowledgeEvent.TmAddrType)
      {
        case TmType.Status:
          if (tEvent.Point == 0)
          {
            acknowledgeEvent.Text = "Общее квитирование ТС";
          }
          else
          {
            acknowledgeEvent.TmAddrString = $"#TC{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";
          }

          break;
        case TmType.Analog:

          if (tEvent.Point == 0)
          {
            acknowledgeEvent.Text = "Общее квитирование ТИ";
          }
          else
          {
            acknowledgeEvent.TmAddrString = $"#TT{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";
          }

          break;
        default:
          if (tEvent.Point == 0)
          {
            acknowledgeEvent.Text = "Общее квитирование";
          }

          break;
      }

      acknowledgeEvent.TypeString         = "Квитирование";
      acknowledgeEvent.ExplicitTypeString = "Квитирование";
      acknowledgeEvent.Username           = EncodingUtil.Cp866BytesToUtf8(acknowledgeData.UserName);

      return acknowledgeEvent;
    }

    protected override void InitializeAcknowledgeEvent(InitializeAcknowledgeEventDto dto)
    {
      InitializeTmEvent(dto);
      
      TmAddrComplexInteger =
        (uint)(dto.Point + (dto.Rtu << 16) + (dto.Ch << 24));

      TmAddrType = ((TmNativeDefs.TmDataTypes)dto.TargetTmType).ToTmType();

      switch (TmAddrType)
      {
        case TmType.Status:
          if (dto.Point == 0)
          {
            Text = "Общее квитирование ТС";
          }
          else
          {
            TmAddrString = $"#TC{dto.Ch}:{dto.Rtu}:{dto.Point}";
          }

          break;
        case TmType.Analog:

          if (dto.Point == 0)
          {
            Text = "Общее квитирование ТИ";
          }
          else
          {
            TmAddrString = $"#TT{dto.Ch}:{dto.Rtu}:{dto.Point}";
          }

          break;
        default:
          if (dto.Point == 0)
          {
            Text = "Общее квитирование";
          }

          break;
      }

      TypeString         = "Квитирование";
      ExplicitTypeString = "Квитирование";
    }


    public static TmEvent CreateControlEvent(TmNativeDefs.TEvent           tEvent,
                                             TmNativeDefs.TTMSEventAddData eventAddData,
                                             TmStatus                      controlStatus,
                                             TmNativeDefs.ControlData      controlData,
                                             TmNativeDefs.TTMSElix?        elix = null)
    {
      var controlEvent = CreateFromTEvent(tEvent, eventAddData, controlStatus.Name, elix);

      controlEvent.TmAddrString = $"#TC{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";
      controlEvent.TmAddrComplexInteger =
        (uint)(tEvent.Point + (tEvent.Rtu << 16) + (tEvent.Ch << 24));
      controlEvent.TmAddrType = TmType.Status;

      controlEvent.TypeString         = "ТУ";
      controlEvent.ExplicitTypeString = "ТУ";

      controlEvent.Username = EncodingUtil.Cp866BytesToUtf8(controlData.UserName);

      var result = (TmTelecontrolResult)unchecked((sbyte)controlData.Result);

      if (result != TmTelecontrolResult.Success)
      {
        controlEvent.ExplicitStateString = $"{(controlData.Cmd == 1 ? "ВКЛ" : "ОТКЛ")} ОШИБКА {(int)result}";
        controlEvent.StateString         = result.GetDescription();
      }
      else
      {
        controlEvent.ExplicitStateString = controlData.Cmd == 1 ? "ВКЛ" : "ОТКЛ";
        controlEvent.StateString =
          $"Команда {(controlData.Cmd == 1 ? controlStatus.CaptionOn : controlStatus.CaptionOff)}";
      }

      return controlEvent;
    }

    protected override void InitializeControlEvent(InitializeControlEventDto dto)
    { 
      InitializeTmEvent(dto);
      
      TmAddrString = $"#TC{dto.Ch}:{dto.Rtu}:{dto.Point}";
      TmAddrComplexInteger =
        (uint)(dto.Point + (dto.Rtu << 16) + (dto.Ch << 24));
      TmAddrType = TmType.Status;

      TypeString         = "ТУ";
      ExplicitTypeString = "ТУ";

      var result = (TmTelecontrolResult)dto.Result;

      if (result != TmTelecontrolResult.Success)
      {
        ExplicitStateString = $"{(dto.Command ? "ВКЛ" : "ОТКЛ")} ОШИБКА {(int)result}";
        StateString         = result.GetDescription();
      }
      else
      {
        ExplicitStateString = dto.Command ? "ВКЛ" : "ОТКЛ";
        StateString =
          $"Команда {(dto.Command ? dto.PropsAndClassData.CaptionOn : dto.PropsAndClassData.CaptionOff)}";
      }
    }


    public static TmEvent CreateExtendedEvent(TmNativeDefs.TEvent           tEvent,
                                              TmNativeDefs.TTMSEventAddData eventAddData,
                                              TmNativeDefs.StrBinData       strBinData,
                                              TmNativeDefs.TTMSElix?        elix = null)
    {
      var extendedEvent = CreateFromTEvent(tEvent, eventAddData, "", elix);

      var extendedType = (TmNativeDefs.ExtendedEventTypes)tEvent.Ch;

      extendedEvent.TmAddrString         = null;
      extendedEvent.TmAddrComplexInteger = 0;
      extendedEvent.TmAddrType           = TmType.Unknown;

      extendedEvent.TypeString                     = GetTypeStringByExtendedTypeString(extendedType);
      extendedEvent.ExplicitTypeString             = GetTypeStringByExtendedTypeString(extendedType);
      (extendedEvent.Text, extendedEvent.Username) = GetMessageAndUserFromStrBinBytes(strBinData.StrBin);

      switch (extendedType)
      {
        case TmNativeDefs.ExtendedEventTypes.Message:
          if (strBinData.Source < 0x10000)
          {
            extendedEvent.Reference = $"Источник: {strBinData.Source}";
          }
          else
          {
            extendedEvent.TmAddrString =
              $"#XX{(strBinData.Source & 0xff00_0000) >> 24}:{(strBinData.Source & 0x00ff_0000) >> 16}:0";
            extendedEvent.Reference =
              $"Ист: {strBinData.Source & 0x0000_ffff}, "        +
              $"К: {(strBinData.Source  & 0xff00_0000) >> 24}, " +
              $"КП: {(strBinData.Source & 0x00ff_0000) >> 16}";
          }

          break;
        case TmNativeDefs.ExtendedEventTypes.Model:
          extendedEvent.Reference = $"Источник: {strBinData.Source}";
          break;
        default:
          extendedEvent.Reference = "???";
          break;
      }

      return extendedEvent;
    }


    protected override void InitializeExtendedEvent(InitializeExtendedEventDto dto)
    {
      InitializeTmEvent(dto);

      TmAddrString         = dto.TmAddrString;
      TmAddrComplexInteger = 0;
      TmAddrType           = TmType.Unknown;

      TypeString                                   = dto.TypeString;
      ExplicitTypeString                           = dto.TypeString;

      Reference = dto.Reference;
    }
    
    public static TmEvent CreateStatusChangeEvent(TmNativeDefs.TEvent           tEvent,
                                                  TmNativeDefs.TTMSEventAddData eventAddData,
                                                  TmStatus                      changedStatus,
                                                  TmNativeDefs.StatusData       statusData,
                                                  TmNativeDefs.TTMSElix?        elix = null)
    {
      var statusChangeEvent = CreateStatusChangeEvent(tEvent,
                                                      eventAddData,
                                                      changedStatus,
                                                      statusData.State,
                                                      statusData.Class,
                                                      statusData.ExtSig,
                                                      statusData.FixUT,
                                                      statusData.FixMS,
                                                      statusData.S2,
                                                      statusData.Flags,
                                                      elix: elix);

      return statusChangeEvent;
    }


    public static TmEvent CreateStatusChangeExtendedEvent(TmNativeDefs.TEvent           tEvent,
                                                          TmNativeDefs.TTMSEventAddData eventAddData,
                                                          TmStatus                      changedStatus,
                                                          TmNativeDefs.StatusDataEx     statusDataEx,
                                                          TmNativeDefs.TTMSElix?        elix = null)
    {
      var statusChangeExtendedEvent = CreateStatusChangeEvent(tEvent,
                                                              eventAddData,
                                                              changedStatus,
                                                              statusDataEx.State,
                                                              statusDataEx.Class,
                                                              statusDataEx.ExtSig,
                                                              statusDataEx.FixUT,
                                                              statusDataEx.FixMS,
                                                              statusDataEx.S2,
                                                              statusDataEx.Flags,
                                                              statusDataEx.OldFlags,
                                                              elix);

      statusChangeExtendedEvent.Username = EncodingUtil.BytesToString(statusDataEx.UserName);
      return statusChangeExtendedEvent;
    }

    private static TmEvent CreateStatusChangeEvent(TmNativeDefs.TEvent           tEvent,
                                                   TmNativeDefs.TTMSEventAddData eventAddData,
                                                   TmStatus                      changedStatus,
                                                   byte                          state,
                                                   byte                          statusClass,
                                                   UInt32                        extSig,
                                                   UInt32                        fixUt,
                                                   UInt16                        fixMs,
                                                   UInt16                        s2,
                                                   UInt32                        flags,
                                                   UInt32?                       oldFlags = null,
                                                   TmNativeDefs.TTMSElix?        elix     = null)
    {
      var statusChangeEvent = CreateFromTEvent(tEvent, eventAddData, changedStatus.Name, elix);

      var                  isS2Only = false;
      TmNativeDefs.S2Flags s2Flags  = 0x0000;

      statusChangeEvent.TmAddrString = $"#TC{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";
      statusChangeEvent.TmAddrComplexInteger =
        (uint)(tEvent.Point + (tEvent.Rtu << 16) + (tEvent.Ch << 24));
      statusChangeEvent.TmAddrType = TmType.Status;

      statusChangeEvent.TypeString = changedStatus.ClassName.IsNullOrEmpty()
                                       ? $"{(changedStatus.IsAps ? "АПС" : "ТС")}"
                                       : changedStatus.ClassName;
      statusChangeEvent.ExplicitTypeString = statusClass == 1 ? "АПС" : "ТС";

      var hasFixTime = false;
      var hasTmFlags = false;
      ;
      var hasSecondary = false;
      var hasNoCurData = false;
      var hasS2        = false;
      if ((extSig & TmNativeDefs.ExtendedDataSignature) == TmNativeDefs.ExtendedDataSignature)
      {
        var extSigFlags = (TmNativeDefs.ExtendedDataSignatureFlag)(extSig ^
                                                                   TmNativeDefs.ExtendedDataSignature);
        hasFixTime   = extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.FixTime);
        hasTmFlags   = extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.TmFlags);
        hasSecondary = extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.Secondary);
        hasNoCurData = !extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.CurData);
        hasS2        = extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.S2);
      }


      if (hasFixTime)
      {
        statusChangeEvent.FixTime = DateUtil.GetDateTimeFromTimestamp(fixUt, fixMs);
      }

      if (oldFlags == null)
      {
        statusChangeEvent.Reference = hasTmFlags ? $"F=${flags:X8}" : "";
      }
      else
      {
        statusChangeEvent.Reference = oldFlags == flags ? $"F=${flags:X8}" : $"F=${oldFlags:X8} -> ${flags:X8}";
      }

      if (hasSecondary)
      {
        statusChangeEvent.Reference =
          statusChangeEvent.Reference == null ? "(s)" : $"{statusChangeEvent.Reference} (s)";
        statusChangeEvent.IsFromReserve = true;
      }

      if (hasNoCurData)
      {
        statusChangeEvent.Reference =
          statusChangeEvent.Reference == null ? "(d)" : $"{statusChangeEvent.Reference} (d)";
      }

      if (hasS2)
      {
        isS2Only = (s2                       & 0x8000) != 0;
        s2Flags  = (TmNativeDefs.S2Flags)(s2 & 0x7fff);
      }


      if (s2Flags == 0)
      {
        statusChangeEvent.StateString = state == 1 ? changedStatus.CaptionOn : changedStatus.CaptionOff;
        statusChangeEvent.ExplicitStateString = isS2Only
                                                  ? "ИЗМ. АТРИБУТОВ - НОРМА"
                                                  : $"{(state == 1 ? "ВКЛ" : "ОТКЛ")}";
      }
      else
      {
        statusChangeEvent.StateString = GetS2StatusString(s2Flags, changedStatus);
        statusChangeEvent.ExplicitStateString =
          $"{(isS2Only ? "ИЗМ. АТРИБУТОВ" : $"{(state == 1 ? "ВКЛ" : "ОТКЛ")}")} {GetS2StatusString(s2Flags)}";
      }

      return statusChangeEvent;
    }

    protected override void InitializeStatusChangeEvent(InitializeStatusChangeEventDto dto)
    {
      InitializeTmEvent(dto);

      var                  isS2Only = false;
      TmNativeDefs.S2Flags s2Flags  = 0x0000;

      TmAddrString = $"#TC{dto.Ch}:{dto.Rtu}:{dto.Point}";
      TmAddrComplexInteger =
        (uint)(dto.Point + (dto.Rtu << 16) + (dto.Ch << 24));
      TmAddrType = TmType.Status;

      TypeString = string.IsNullOrEmpty(dto.PropsAndClassData.ClassName)
                     ? $"{(dto.StatusClass == 1 ? "АПС" : "ТС")}"
                     : dto.PropsAndClassData.ClassName;
      ExplicitTypeString = dto.StatusClass == 1 ? "АПС" : "ТС";

      var hasFixTime = false;
      var hasTmFlags = false;
      ;
      var hasSecondary = false;
      var hasNoCurData = false;
      var hasS2        = false;
      if ((dto.ExtSig & TmNativeDefs.ExtendedDataSignature) == TmNativeDefs.ExtendedDataSignature)
      {
        var extSigFlags = (TmNativeDefs.ExtendedDataSignatureFlag)(dto.ExtSig ^
                                                                   TmNativeDefs.ExtendedDataSignature);
        hasFixTime   = extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.FixTime);
        hasTmFlags   = extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.TmFlags);
        hasSecondary = extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.Secondary);
        hasNoCurData = !extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.CurData);
        hasS2        = extSigFlags.HasFlag(TmNativeDefs.ExtendedDataSignatureFlag.S2);
      }


      if (hasFixTime)
      {
        FixTime = DateUtil.GetDateTimeFromTimestamp(dto.FixUt, dto.FixMs);
      }

      if (dto.StatusOldFlags == null)
      {
        Reference = hasTmFlags ? $"F=${dto.StatusFlags:X8}" : "";
      }
      else
      {
        Reference = dto.StatusOldFlags == dto.StatusFlags
                      ? $"F=${dto.StatusFlags:X8}"
                      : $"F=${dto.StatusOldFlags:X8} -> ${dto.StatusFlags:X8}";
      }

      if (hasSecondary)
      {
        Reference     = Reference == null ? "(s)" : $"{Reference} (s)";
        IsFromReserve = true;
      }

      if (hasNoCurData)
      {
        Reference = Reference == null ? "(d)" : $"{Reference} (d)";
      }

      if (hasS2)
      {
        isS2Only = (dto.StatusS2                       & 0x8000) != 0;
        s2Flags  = (TmNativeDefs.S2Flags)(dto.StatusS2 & 0x7fff);
      }


      if (s2Flags == 0)
      {
        StateString = dto.State == 1 ? dto.PropsAndClassData.CaptionOn : dto.PropsAndClassData.CaptionOff;
        ExplicitStateString = isS2Only
                                ? "ИЗМ. АТРИБУТОВ - НОРМА"
                                : $"{(dto.State == 1 ? "ВКЛ" : "ОТКЛ")}";
      }
      else
      {
        StateString = GetS2StatusString(s2Flags, dto.PropsAndClassData.CaptionBreak, dto.PropsAndClassData.CaptionBreak);
        ExplicitStateString =
          $"{(isS2Only ? "ИЗМ. АТРИБУТОВ" : $"{(dto.State == 1 ? "ВКЛ" : "ОТКЛ")}")} {GetS2StatusString(s2Flags)}";
      }
    }

    public static TmEvent CreateStatusFlagsChangeEvent(TmNativeDefs.TEvent                tEvent,
                                                       TmNativeDefs.TTMSEventAddData      eventAddData,
                                                       TmStatus                           sourceStatus,
                                                       TmNativeDefs.FlagsChangeDataStatus flagsChangeDataStatus,
                                                       TmNativeDefs.TTMSElix?             elix = null)
    {
      var flagsChangeEvent = CreateFlagsChangeEvent(tEvent,
                                                    eventAddData,
                                                    sourceStatus.Name,
                                                    TmType.Status,
                                                    flagsChangeDataStatus.OldFlags,
                                                    flagsChangeDataStatus.NewFlags,
                                                    EncodingUtil.BytesToString(flagsChangeDataStatus.UserName),
                                                    elix);

      flagsChangeEvent.TypeString =
        $"Изм. флагов {(sourceStatus.ClassName.IsNullOrEmpty() ? $"{(sourceStatus.IsAps ? "АПС" : "ТС")}" : sourceStatus.ClassName)}";


      return flagsChangeEvent;
    }


    public static TmEvent CreateAnalogFlagsChangeEvent(TmNativeDefs.TEvent                tEvent,
                                                       TmNativeDefs.TTMSEventAddData      eventAddData,
                                                       TmAnalog                           sourceAnalog,
                                                       TmNativeDefs.FlagsChangeDataAnalog flagsChangeDataAnalog,
                                                       TmNativeDefs.TTMSElix?             elix = null)
    {
      var flagsChangeEvent = CreateFlagsChangeEvent(tEvent,
                                                    eventAddData,
                                                    sourceAnalog.Name,
                                                    TmType.Analog,
                                                    flagsChangeDataAnalog.OldFlags,
                                                    flagsChangeDataAnalog.NewFlags,
                                                    EncodingUtil.BytesToString(flagsChangeDataAnalog.UserName),
                                                    elix);

      flagsChangeEvent.TypeString = "Изм. флагов ТИ";

      return flagsChangeEvent;
    }


    public static TmEvent CreateAccumFlagsChangeEvent(TmNativeDefs.TEvent           tEvent,
                                                      TmNativeDefs.TTMSEventAddData eventAddData,
                                                      string                        sourceObjectName,
                                                      TmNativeDefs.FlagsChangeData  flagsChangeData,
                                                      TmNativeDefs.TTMSElix?        elix = null)
    {
      var flagsChangeEvent = CreateFlagsChangeEvent(tEvent,
                                                    eventAddData,
                                                    sourceObjectName,
                                                    TmType.Accum,
                                                    flagsChangeData.OldFlags,
                                                    flagsChangeData.NewFlags,
                                                    "",
                                                    elix);

      flagsChangeEvent.TypeString = "Изм. флагов ТИИ";

      return flagsChangeEvent;
    }


    private static TmEvent CreateFlagsChangeEvent(TmNativeDefs.TEvent           tEvent,
                                                  TmNativeDefs.TTMSEventAddData eventAddData,
                                                  string                        sourceObjectName,
                                                  TmType                        sourceDataType,
                                                  uint                          oldFlags,
                                                  uint                          newFlags,
                                                  string                        username,
                                                  TmNativeDefs.TTMSElix?        elix = null)
    {
      var flagsChangeEvent = CreateFromTEvent(tEvent, eventAddData, sourceObjectName);

      flagsChangeEvent.TmAddrComplexInteger =
        (uint)(tEvent.Point + (tEvent.Rtu << 16) + (tEvent.Ch << 24));
      flagsChangeEvent.Username = username;

      switch (sourceDataType)
      {
        case TmType.Status:
          flagsChangeEvent.TmAddrString       = $"#TC{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";
          flagsChangeEvent.ExplicitTypeString = "Изм. флагов ТС";
          flagsChangeEvent.TmAddrType         = TmType.Status;
          break;
        case TmType.Analog:
          flagsChangeEvent.TmAddrString       = $"#TТ{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";
          flagsChangeEvent.ExplicitTypeString = "Изм. флагов ТИТ";
          flagsChangeEvent.TmAddrType         = TmType.Analog;
          break;
        case TmType.Accum:
          flagsChangeEvent.TmAddrString       = $"#TИ{tEvent.Ch}:{tEvent.Rtu}:{tEvent.Point}";
          flagsChangeEvent.ExplicitTypeString = "Изм. флагов ТИИ";
          flagsChangeEvent.TmAddrType         = TmType.Accum;
          break;
      }


      flagsChangeEvent.ExplicitStateString = $"${oldFlags:X8}X -> ${newFlags:X8}X";
      flagsChangeEvent.StateString         = $"${oldFlags:X8}X -> ${newFlags:X8}X";

      return flagsChangeEvent;
    }


    public static TmEvent CreateFromTEvent(TmNativeDefs.TEvent           tEvent,
                                           TmNativeDefs.TTMSEventAddData eventAddData,
                                           string                        sourceObjectName,
                                           TmNativeDefs.TTMSElix?        elix = null)
    {
      var tmEventType = (TmEventTypes)tEvent.Id;

      TmEventElix tmEventElix = null;

      if (elix is { } ttmsElix)
      {
        tmEventElix = new TmEventElix(ttmsElix.R, ttmsElix.M);
      }

      var hash = tmEventElix is null
                   ? (tEvent.Ch, tEvent.Rtu, tEvent.Point, tEvent.Data, tEvent.DateTime).ToTuple().GetHashCode()
                   : BitConverter.ToInt32(tmEventElix.ToByteArray(), 8);

      var dtString = TmNativeUtil.GetStringFromBytesWithAdditionalPart(tEvent.DateTime);
      var dt       = DateUtil.GetDateTimeFromExtendedTmString(dtString);

      var tmEvent = new TmEvent(hash)
      {
        Elix       = tmEventElix,
        Time       = dt,
        Type       = tmEventType,
        Importance = tEvent.Imp,
        Text       = sourceObjectName,
      };

      if (eventAddData.AckSec != 0 && !eventAddData.UserName.IsNullOrEmpty())
      {
        tmEvent.AckTime = DateUtil.GetDateTimeFromTimestamp(eventAddData.AckSec, eventAddData.AckMs);
        // сервер возвращает мусор после первого нуля в имени, нужно обрезать
        tmEvent.AckUser = eventAddData.UserName;
      }

      return tmEvent;
    }

    protected override void InitializeTmEvent(InitializeTmEventDto dto)
    {
      TmEventElix tmEventElix = null;

      /*if (elix is { } ttmsElix)
      {
        tmEventElix = new TmEventElix(ttmsElix.R, ttmsElix.M);
      }*/

      _hashCode = tmEventElix is null
                    ? (dto.Ch, dto.Rtu, dto.Point, /*tEvent.Data,*/ dto.DateTimeStr).ToTuple().GetHashCode()
                    : BitConverter.ToInt32(tmEventElix.ToByteArray(), 8);

      Elix       = tmEventElix;
      Time       = DateUtil.GetDateTimeFromExtendedTmString(dto.DateTimeStr);
      Type       = (TmEventTypes)dto.Id;
      Importance = dto.Imp;
      Username   = dto.OperatorName;
      TmAddrType = Type switch
                   {
                     TmEventTypes.StatusChange => TmType.Status,
                     _                         => TmType.Unknown
                   };
      
      Text = dto.PropsAndClassData.Name;

      if (dto.NotAcked)
      {
        return;
      }

      AckTime = DateUtil.GetDateTimeFromTimestamp(dto.AckSec, dto.AckMs);
      // сервер возвращает мусор после первого нуля в имени, нужно обрезать
      AckUser = dto.AckUser;
    }

    public override int GetHashCode()
    {
      return _hashCode != 0
               ? _hashCode
               : base.GetHashCode();
    }


    public static TmEventImportances ImportanceToFlag(int importance)
    {
      return (TmEventImportances)(1 << importance);
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


    private static string GetS2StatusString(TmNativeDefs.S2Flags s2Flag, TmStatus status = null)
    {
      if (s2Flag.HasFlag(TmNativeDefs.S2Flags.Break))
      {
        return status == null ? $"[{s2Flag:D}] <ОБРЫВ>" : status.CaptionBreak;
      }

      if (s2Flag.HasFlag(TmNativeDefs.S2Flags.Malfunction))
      {
        return status == null ? $"[{s2Flag:D}] <НЕИСП.>" : status.CaptionMalfunction;
      }

      return $"[{s2Flag:D}]";
    }

    private static string GetS2StatusString(TmNativeDefs.S2Flags s2Flag,
                                            string               captionBreak,
                                            string               captionMalfunction)
    {
      if (s2Flag.HasFlag(TmNativeDefs.S2Flags.Break))
      {
        return string.IsNullOrEmpty(captionBreak) ? $"[{s2Flag:D}] <ОБРЫВ>" : captionBreak;
      }

      if (s2Flag.HasFlag(TmNativeDefs.S2Flags.Malfunction))
      {
        return string.IsNullOrEmpty(captionMalfunction) ? $"[{s2Flag:D}] <НЕИСП.>" : captionMalfunction;
      }

      return $"[{s2Flag:D}]";
    }


    private static string GetTypeStringByExtendedTypeString(TmNativeDefs.ExtendedEventTypes eventType)
    {
      switch (eventType)
      {
        case TmNativeDefs.ExtendedEventTypes.Message:
          return "Сообщение";
        case TmNativeDefs.ExtendedEventTypes.Model:
          return "Модель";
        default:
          return "???";
      }
    }


    public static (string, string) GetMessageAndUserFromStrBinBytes(byte[] bytes)
    {
      var str = Encoding.GetEncoding(1251)
                        .GetString(bytes);

      var regex = new Regex(@"(.*?)\0(.*?)\0");
      var mc    = regex.Match(str);
      var text  = mc.Groups[1].Value;
      var user  = mc.Groups[2].Value;

      return (text, user);
    }
  }
}