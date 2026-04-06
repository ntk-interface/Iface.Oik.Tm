using System;
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
    };
  }

  protected abstract void InitializeTmEvent(InitializeTmEventDto dto);

  protected abstract void InitializeStatusChangeEvent(InitializeStatusChangeEventDto dto);

  protected abstract void InitializeAlarmEvent(InitializeAlarmEventDto dto);

  protected abstract void InitializeControlEvent(InitializeControlEventDto dto);

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

    public TagPropsAndClassData PropsAndClassData { get; init; }

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
}