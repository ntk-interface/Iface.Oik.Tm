using System;
using System.Text;
using System.Text.RegularExpressions;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class TmEventBase
{
  internal static unsafe T CreateStatusChangeExtendedEvent<T>(TmNativeDefsUnsafe.TEventHeader header,
                                                              TmNativeDefsUnsafe.StatusDataEx data,
                                                              string                          operatorName,
                                                              TmNativeDefs.TTMSEventAddData   ackData,
                                                              TagPropsAndClassData            propsAndClassData)
    where T : TmEventBase, new()
  {
    var evnt = new T();

    var dto = new InitializeStatusChangeEventDto
    {
      Id                = header.Id,
      Ch                = header.Ch,
      Rtu               = header.Rtu,
      Point             = header.Point,
      Imp               = header.Imp,
      PropsAndClassData = propsAndClassData,
      DateTimeStr       = TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(header.DateTime),
      AckSec            = ackData.AckSec,
      AckMs             = ackData.AckMs,
      OperatorName      = operatorName,
      AckUser           = ackData.UserName,
      State             = data.State,
      StatusClass       = data.Class,
      ExtSig            = data.ExtSig,
      FixUt             = data.FixUT,
      FixMs             = data.FixMS,
      StatusS2          = data.S2,
      StatusFlags       = data.Flags,
      StatusOldFlags    = data.OldFlags,
    };

    evnt.InitializeStatusChangeEvent(dto);

    return evnt;
  }

  internal static unsafe T CreateStatusChangeEvent<T>(TmNativeDefsUnsafe.TEventHeader header,
                                                      TmNativeDefsUnsafe.StatusData   data,
                                                      TmNativeDefs.TTMSEventAddData   ackData,
                                                      TagPropsAndClassData            propsAndClassData)
    where T : TmEventBase, new()
  {
    var evnt = new T();

    var dto = new InitializeStatusChangeEventDto
    {
      Id                = header.Id,
      Ch                = header.Ch,
      Rtu               = header.Rtu,
      Point             = header.Point,
      Imp               = header.Imp,
      PropsAndClassData = propsAndClassData,
      DateTimeStr       = TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(header.DateTime),
      AckSec            = ackData.AckSec,
      AckMs             = ackData.AckMs,
      AckUser           = ackData.UserName,
      State             = data.State,
      StatusClass       = data.Class,
      ExtSig            = data.ExtSig,
      FixUt             = data.FixUT,
      FixMs             = data.FixMS,
      StatusS2          = data.S2,
      StatusFlags       = data.Flags,
    };

    evnt.InitializeStatusChangeEvent(dto);

    return evnt;
  }

  internal static unsafe T CreateAlarmEvent<T>(TmNativeDefsUnsafe.TEventHeader header,
                                               TmNativeDefsUnsafe.AlarmData    data,
                                               TmNativeDefs.TTMSEventAddData   ackData,
                                               string                          typeName,
                                               TagPropsAndClassData            propsAndClassData)
    where T : TmEventBase, new()
  {
    var evnt = new T();

    var dto = new InitializeAlarmEventDto
    {
      Id                = header.Id,
      Ch                = header.Ch,
      Rtu               = header.Rtu,
      Point             = header.Point,
      Imp               = header.Imp,
      PropsAndClassData = propsAndClassData,
      DateTimeStr       = TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(header.DateTime),
      AckSec            = ackData.AckSec,
      AckMs             = ackData.AckMs,
      AckUser           = ackData.UserName,
      TurnedOn          = data.State > 0,
      Value             = data.Val,
      TypeName          = typeName
    };

    evnt.InitializeAlarmEvent(dto);

    return evnt;
  }

  internal static unsafe T CreateControlEvent<T>(TmNativeDefsUnsafe.TEventHeader header,
                                                 TmNativeDefsUnsafe.ControlData  data,
                                                 TmNativeDefs.TTMSEventAddData   ackData,
                                                 string                          operatorName,
                                                 TagPropsAndClassData            propsAndClassData)
    where T : TmEventBase, new()
  {
    var evnt = new T();

    var dto = new InitializeControlEventDto
    {
      Id                = header.Id,
      Ch                = header.Ch,
      Rtu               = header.Rtu,
      Point             = header.Point,
      Imp               = header.Imp,
      PropsAndClassData = propsAndClassData,
      DateTimeStr       = TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(header.DateTime),
      AckSec            = ackData.AckSec,
      AckMs             = ackData.AckMs,
      AckUser           = ackData.UserName,
      OperatorName      = operatorName,
      Result            = unchecked((sbyte)data.Result),
      Command           = data.Cmd == 1
    };

    evnt.InitializeControlEvent(dto);

    return evnt;
  }


  internal static unsafe T CreateAcknowledgeEventDto<T>(TmNativeDefsUnsafe.TEventHeader    header,
                                                        TmNativeDefsUnsafe.AcknowledgeData data,
                                                        TmNativeDefs.TTMSEventAddData      ackData,
                                                        string                             operatorName,
                                                        TagPropsAndClassData               propsAndClassData)
    where T : TmEventBase, new()
  {
    var evnt = new T();

    var dto = new InitializeAcknowledgeEventDto
    {
      Id                = header.Id,
      Ch                = header.Ch,
      Rtu               = header.Rtu,
      Point             = header.Point,
      Imp               = header.Imp,
      DateTimeStr       = TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(header.DateTime),
      AckSec            = ackData.AckSec,
      AckMs             = ackData.AckMs,
      AckUser           = ackData.UserName,
      OperatorName      = operatorName,
      PropsAndClassData = propsAndClassData,
      TargetTmType      = data.TmType
    };

    evnt.InitializeAcknowledgeEvent(dto);

    return evnt;
  }


  internal static unsafe T CreateManualStatusSetEvent<T>(TmNativeDefsUnsafe.TEventHeader header,
                                                         TmNativeDefsUnsafe.ControlData  data,
                                                         TmNativeDefs.TTMSEventAddData   ackData,
                                                         string                          operatorName,
                                                         TagPropsAndClassData            propsAndClassData)
    where T : TmEventBase, new()
  {
    var evnt = new T();

    var dto = new InitializeManualStatusSetEventDto
    {
      Id                = header.Id,
      Ch                = header.Ch,
      Rtu               = header.Rtu,
      Point             = header.Point,
      Imp               = header.Imp,
      PropsAndClassData = propsAndClassData,
      DateTimeStr       = TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(header.DateTime),
      AckSec            = ackData.AckSec,
      AckMs             = ackData.AckMs,
      AckUser           = ackData.UserName,
      OperatorName      = operatorName,
      Command           = data.Cmd == 1
    };

    evnt.InitializeManualStatusSetEvent(dto);

    return evnt;
  }

  internal static unsafe T CreateManualAnalogSetEvent<T>(TmNativeDefsUnsafe.TEventHeader  header,
                                                         TmNativeDefsUnsafe.AnalogSetData data,
                                                         TmNativeDefs.TTMSEventAddData    ackData,
                                                         string                           operatorName,
                                                         TagPropsAndClassData             propsAndClassData)
    where T : TmEventBase, new()
  {
    var evnt = new T();

    var dto = new InitializeManualAnalogSetEventDto
    {
      Id                = header.Id,
      Ch                = header.Ch,
      Rtu               = header.Rtu,
      Point             = header.Point,
      Imp               = header.Imp,
      PropsAndClassData = propsAndClassData,
      DateTimeStr       = TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(header.DateTime),
      AckSec            = ackData.AckSec,
      AckMs             = ackData.AckMs,
      AckUser           = ackData.UserName,
      OperatorName      = operatorName,
      Command           = data.Cmd == 1,
      Value             = data.Value
    };


    evnt.InitializeManualAnalogSetEvent(dto);

    return evnt;
  }

  internal static unsafe T CreateExtendedEvent<T>(TmNativeDefsUnsafe.TEventHeader header,
                                                  TmNativeDefsUnsafe.StrBinData   data,
                                                  TmNativeDefs.TTMSEventAddData   ackData)
    where T : TmEventBase, new()
  {
    var evnt = new T();

    var (text, operatorName) = GetMessageAndUserFromStrBinBytes(data.StrBin);

    var extendedType = (TmNativeDefs.ExtendedEventTypes)header.Ch;

    string reference;
    string    tmAddrString = null;

    switch (extendedType)
    {
        case TmNativeDefs.ExtendedEventTypes.Message when data.Source < 0x10000:
        {
          reference = $"Источник: {data.Source}";
          break;
        }
        case TmNativeDefs.ExtendedEventTypes.Message:
        {
          tmAddrString =
            $"#XX{(data.Source & 0xff00_0000) >> 24}:{(data.Source & 0x00ff_0000) >> 16}:0";
          reference =
            $"Ист: {data.Source & 0x0000_ffff}, "        +
            $"К: {(data.Source  & 0xff00_0000) >> 24}, " +
            $"КП: {(data.Source & 0x00ff_0000) >> 16}";
          
          break;
        }
        
        case TmNativeDefs.ExtendedEventTypes.Model:
          reference = $"Источник: {data.Source}";
          break;
        default:
          reference = "???";
          break;
    }
    
    
    var dto = new InitializeExtendedEventDto
    {
      Id    = header.Id,
      Ch    = header.Ch,
      Rtu   = header.Rtu,
      Point = header.Point,
      Imp   = header.Imp,
      PropsAndClassData = new TagPropsAndClassData
      {
        Name = text,
      },
      DateTimeStr  = TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(header.DateTime),
      AckSec       = ackData.AckSec,
      AckMs        = ackData.AckMs,
      AckUser      = ackData.UserName,
      OperatorName = operatorName,
      TypeString = extendedType switch
                   {
                     TmNativeDefs.ExtendedEventTypes.Message => "Сообщение",
                     TmNativeDefs.ExtendedEventTypes.Model => "Модель",
                     _ => "???"
                   },
      Reference = reference,
      TmAddrString = tmAddrString
    };

    evnt.InitializeExtendedEvent(dto);

    return evnt;
  }

  internal static unsafe T CreateUnknownEvent<T>(TmNativeDefsUnsafe.TEventHeader header,
                                            TmNativeDefs.TTMSEventAddData   ackData) 
    where T: TmEventBase, new()
  {
    var evnt = new T();
    
    var dto = new InitializeTmEventDto
    {
      Id    = header.Id,
      Ch    = header.Ch,
      Rtu   = header.Rtu,
      Point = header.Point,
      Imp   = header.Imp,
      PropsAndClassData = new TagPropsAndClassData
      {
        Name = "???",
      },
      DateTimeStr = TmNativeUtil.GetStringWithUnknownLengthFromBytePtr(header.DateTime),
      AckSec      = ackData.AckSec,
      AckMs       = ackData.AckMs,
      AckUser     = ackData.UserName,
    };

    evnt.InitializeTmEvent(dto);
    
    return evnt;
  }
  
  internal static string GetTagName(string propertiesString)
  {
    var span = propertiesString.AsSpan();

    while (!span.IsEmpty)
    {
      var lineEnd = span.IndexOf("\r\n");

      ReadOnlySpan<char> line;
      if (lineEnd >= 0)
      {
        line = span[..lineEnd];
        span = span[(lineEnd + 2)..];
      }
      else
      {
        line = span;
        span = default;
      }

      if (line.IsEmpty)
      {
        continue;
      }

      var eq = line.IndexOf('=');
      if (eq < 0)
      {
        continue;
      }

      var key   = line[..eq].ToString();
      var value = line[(eq + 1)..].ToString();

      if (key != "Name")
      {
        continue;
      }

      return value;
    }

    return string.Empty;
  }

  internal static TagPropsAndClassData GetStatusClassData(string classDataString)
  {
    var span = classDataString.AsSpan();

    var className          = string.Empty;
    var captionOn          = string.Empty;
    var captionOff         = string.Empty;
    var captionBreak       = string.Empty;
    var captionMalfunction = string.Empty;

    var flag1Name       = string.Empty;
    var captionFlag1On  = string.Empty;
    var captionFlag1Off = string.Empty;

    var flag2Name       = string.Empty;
    var captionFlag2On  = string.Empty;
    var captionFlag2Off = string.Empty;

    var flag3Name       = string.Empty;
    var captionFlag3On  = string.Empty;
    var captionFlag3Off = string.Empty;

    var flag4Name       = string.Empty;
    var captionFlag4On  = string.Empty;
    var captionFlag4Off = string.Empty;

    while (!span.IsEmpty)
    {
      var lineEnd = span.IndexOf("\r\n");

      ReadOnlySpan<char> line;
      if (lineEnd >= 0)
      {
        line = span[..lineEnd];
        span = span[(lineEnd + 2)..];
      }
      else
      {
        line = span;
        span = default;
      }

      if (line.IsEmpty)
      {
        continue;
      }

      var eq = line.IndexOf('=');
      if (eq < 0)
      {
        continue;
      }

      var key = line[..eq].ToString();

      switch (key)
      {
        case "ClassName":
          className = line[(eq + 1)..].ToString();
          break;
        case "0Txt":
          captionOff = line[(eq + 1)..].ToString();
          break;
        case "1Txt":
          captionOn = line[(eq + 1)..].ToString();
          break;
        case "BTxt":
          captionBreak = line[(eq + 1)..].ToString();
          break;
        case "MTxt":
          captionMalfunction = line[(eq + 1)..].ToString();
          break;

        case "F1Name":
          flag1Name = line[(eq + 1)..].ToString();
          break;
        case "F10Txt":
          captionFlag1Off = line[(eq + 1)..].ToString();
          break;
        case "F11Txt":
          captionFlag1On = line[(eq + 1)..].ToString();
          break;

        case "F2Name":
          flag2Name = line[(eq + 1)..].ToString();
          break;
        case "F20Txt":
          captionFlag2Off = line[(eq + 1)..].ToString();
          break;
        case "F21Txt":
          captionFlag2On = line[(eq + 1)..].ToString();
          break;

        case "F3Name":
          flag3Name = line[(eq + 1)..].ToString();
          break;
        case "F30Txt":
          captionFlag3Off = line[(eq + 1)..].ToString();
          break;
        case "F31Txt":
          captionFlag3On = line[(eq + 1)..].ToString();
          break;

        case "F4Name":
          flag4Name = line[(eq + 1)..].ToString();
          break;
        case "F40Txt":
          captionFlag4Off = line[(eq + 1)..].ToString();
          break;
        case "F41Txt":
          captionFlag4On = line[(eq + 1)..].ToString();
          break;

        default:
          continue;
      }
    }

    return new TagPropsAndClassData
    {
      ClassName          = className,
      CaptionOn          = captionOn,
      CaptionOff         = captionOff,
      CaptionBreak       = captionBreak,
      CaptionMalfunction = captionMalfunction,

      Flag1Name       = flag1Name,
      CaptionFlag1Off = captionFlag1Off,
      CaptionFlag1On  = captionFlag1On,

      Flag2Name       = flag2Name,
      CaptionFlag2Off = captionFlag2Off,
      CaptionFlag2On  = captionFlag2On,

      Flag3Name       = flag3Name,
      CaptionFlag3Off = captionFlag3Off,
      CaptionFlag3On  = captionFlag3On,

      Flag4Name       = flag4Name,
      CaptionFlag4Off = captionFlag4Off,
      CaptionFlag4On  = captionFlag4On
    };
  }

  private static unsafe (string, string) GetMessageAndUserFromStrBinBytes(byte* ptr, Encoding encoding = null)
  {
    var message = string.Empty;
    var user    = string.Empty;
    encoding ??= Encoding.UTF8;

    if (ptr[0] == 0)
    {
      return (message, user);
    }

    var length = 0;

    var curPtr = ptr;

    for (var i = 0; i < 2; i++)
    {
      while (curPtr[length] != 0)
      {
        length++;
      }

      switch (i)
      {
        case 0:
          message = encoding.GetString(curPtr, length);
          break;
        case 1 when length > 0:
          user = encoding.GetString(curPtr, length);
          break;
      }

      curPtr += length + 1;
      length =  0;
    }

    return (message, user);
  }

  protected abstract void InitializeTmEvent(InitializeTmEventDto dto);

  protected abstract void InitializeStatusChangeEvent(InitializeStatusChangeEventDto dto);

  protected abstract void InitializeAlarmEvent(InitializeAlarmEventDto dto);

  protected abstract void InitializeControlEvent(InitializeControlEventDto dto);

  protected abstract void InitializeAcknowledgeEvent(InitializeAcknowledgeEventDto dto);

  protected abstract void InitializeManualStatusSetEvent(InitializeManualStatusSetEventDto dto);

  protected abstract void InitializeManualAnalogSetEvent(InitializeManualAnalogSetEventDto dto);

  protected abstract void InitializeExtendedEvent(InitializeExtendedEventDto dto);

  protected record InitializeTmEventDto
  {
    public ushort Id           { get; init; }
    public ushort Ch           { get; init; }
    public ushort Rtu          { get; init; }
    public ushort Point        { get; init; }
    public ushort Imp          { get; init; }
    public string DateTimeStr  { get; init; } = string.Empty;
    public uint   AckSec       { get; init; }
    public ushort AckMs        { get; init; }
    public string OperatorName { get; init; } = string.Empty;
    public string AckUser      { get; init; }

    public TagPropsAndClassData PropsAndClassData { get; init; } = new();

    public bool NotAcked => AckSec == 0 || string.IsNullOrEmpty(OperatorName);
  }

  protected record InitializeStatusChangeEventDto : InitializeTmEventDto
  {
    public byte   State          { get; init; }
    public byte   StatusClass    { get; init; }
    public uint   ExtSig         { get; init; }
    public uint   FixUt          { get; init; }
    public ushort FixMs          { get; init; }
    public ushort StatusS2       { get; init; }
    public uint   StatusFlags    { get; init; }
    public uint?  StatusOldFlags { get; init; }
  }

  protected record InitializeAlarmEventDto : InitializeTmEventDto
  {
    public string TypeName { get; init; } = string.Empty;
    public bool   TurnedOn { get; init; }
    public float  Value    { get; init; }
  }

  protected record InitializeControlEventDto : InitializeTmEventDto
  {
    public sbyte Result  { get; init; }
    public bool  Command { get; init; }
  }

  protected record InitializeAcknowledgeEventDto : InitializeTmEventDto
  {
    public ushort TargetTmType { get; init; }
  }

  protected record InitializeManualStatusSetEventDto : InitializeTmEventDto
  {
    public bool  Command { get; init; }
    public short ACh     { get; init; }
    public short ARtu    { get; init; }
  }

  protected record InitializeManualAnalogSetEventDto : InitializeTmEventDto
  {
    public float Value   { get; init; }
    public bool  Command { get; init; }
  }

  protected record InitializeExtendedEventDto : InitializeTmEventDto
  {
    public string TypeString   { get; init; } = string.Empty;
    public string Reference    { get; init; } = string.Empty;
    public string TmAddrString { get; init; } = string.Empty;
  }
}