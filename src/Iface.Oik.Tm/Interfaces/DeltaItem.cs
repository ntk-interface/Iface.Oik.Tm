using System;
using System.Linq;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class DeltaItem
  {
    public DeltaItemTypes Type                   { get; private set; }
    public DateTime?      UpdateTime             { get; private set; }
    public TmAddr         TmAddress              { get; private set; }
    public string         ObjectName             { get; private set; }
    public string         TypeString             { get; private set; }
    public string         AddressInChannelString { get; private set; }
    public string         AdditionalInfo         { get; private set; }
    public string         ValueString            { get; private set; }

    public string UpdateTimeString => UpdateTime.HasValue ? $"{UpdateTime.Value:dd.MM.yyyy H:mm:ss}" : string.Empty;
    public string TmAddressString  => TmAddress == null ? string.Empty : TmAddress.ToString();


    public static DeltaItem CreateDescriptionDeltaItem(string descriptionString)
    {
      var descArray = descriptionString.TrimStart('*').Split(':');
      return new DeltaItem
             {
               Type           = DeltaItemTypes.Description,
               TypeString     = descArray.Length == 1 ? "" : descArray.FirstOrDefault(),
               AdditionalInfo = descArray.LastOrDefault()
             };
    }


    public static DeltaItem CreateStatusDeltaItem(int                          addressInChannel,
                                                  int                          lastUpdateTimestamp,
                                                  TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                  int                          value,
                                                  string                       additionalInfo,
                                                  TmAddr                       tmAddr,
                                                  string                       objectName)
    {
      var deltaItem = new DeltaItem
                      {
                        Type       = DeltaItemTypes.Status,
                        TypeString = "ТС",
                        TmAddress  = tmAddr,
                        ObjectName = objectName
                      };

      if (deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum))
      {
        deltaItem.AddressInChannelString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                                             ? $"0x{addressInChannel:X}"
                                             : $"{addressInChannel}";
      }
      else
      {
        var addressQuotient = Math.DivRem(addressInChannel, 8, out var addressRemainder);
        deltaItem.AddressInChannelString = $"{addressQuotient + 1}-{addressRemainder}";
      }

      deltaItem.ValueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                                ? $"{value & 0xF}"
                                : $"{value & 0xF} ?";

      if (deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.DestVal))
      {
        deltaItem.ValueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.DestReli)
                                  ? $"{deltaItem.ValueString} ({value >> 4})"
                                  : $"{deltaItem.ValueString} ({value >> 4} ?)";
      }

      deltaItem.UpdateTime = lastUpdateTimestamp == 0
                               ? (DateTime?) null
                               : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      deltaItem.AdditionalInfo = additionalInfo;

      if (deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.S2Break))
      {
        deltaItem.AdditionalInfo = $"{deltaItem.AdditionalInfo} <Обрыв>";
      }

      if (deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.S2Malfn))
      {
        deltaItem.AdditionalInfo = $"{deltaItem.AdditionalInfo} <Неисп>";
      }

      return deltaItem;
    }


    public static DeltaItem CreateAnalogDeltaItem(int                          addressInChannel,
                                                  int                          lastUpdateTimestamp,
                                                  TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                  int                          value,
                                                  string                       additionalInfo,
                                                  TmAddr                       tmAddr,
                                                  string                       objectName)
    {
      var enumShift = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
      var hexValue  = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Analong) ? $"{value & 0xffff:X}" : $"{value:X}";
      var updateTime = lastUpdateTimestamp == 0
                         ? (DateTime?) null
                         : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      return new DeltaItem
             {
               AddressInChannelString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                                          ? $"0x{addressInChannel:X}"
                                          : $"{addressInChannel + enumShift}",
               Type       = DeltaItemTypes.Analog,
               TypeString = "ТИТ",
               UpdateTime = updateTime,
               ValueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                               ? $"{value} (0x{hexValue})"
                               : $"{value} (0x{hexValue}) ?",
               AdditionalInfo = additionalInfo,
               TmAddress      = tmAddr,
               ObjectName = objectName
             };
    }


    public static DeltaItem CreateAnalogFloatDeltaItem(int                          addressInChannel,
                                                       int                          lastUpdateTimestamp,
                                                       TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                       float                        value,
                                                       string                       additionalInfo,
                                                       TmAddr                       tmAddr,
                                                       string                       objectName)
    {
      var enumShift = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
      var updateTime = lastUpdateTimestamp == 0
                         ? (DateTime?) null
                         : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      return new DeltaItem
             {
               AddressInChannelString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                                          ? $"0x{addressInChannel:X}"
                                          : $"{addressInChannel + enumShift}",
               Type       = DeltaItemTypes.Analog,
               TypeString = "ТИТ",
               UpdateTime = updateTime,
               ValueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                               ? $"{value:N6}"
                               : $"{value:N6} ?",
               AdditionalInfo = additionalInfo,
               TmAddress      = tmAddr,
               ObjectName = objectName
             };
    }


    public static DeltaItem CreateAccumDeltaItem(int                          addressInChannel,
                                                 int                          lastUpdateTimestamp,
                                                 TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                 int                          value,
                                                 string                       additionalInfo,
                                                 TmAddr                       tmAddr,
                                                 string                       objectName)
    {
      var enumShift = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
      var updateTime = lastUpdateTimestamp == 0
                         ? (DateTime?) null
                         : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      return new DeltaItem
             {
               AddressInChannelString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                                          ? $"0x{addressInChannel:X}"
                                          : $"{addressInChannel + enumShift}",
               Type       = DeltaItemTypes.Accum,
               TypeString = "ТИИ",
               UpdateTime = updateTime,
               ValueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                               ? $"{value} "
                               : $"{value} ?",
               AdditionalInfo = additionalInfo,
               TmAddress      = tmAddr,
               ObjectName = objectName
             };
    }


    public static DeltaItem CreateAccumFloatDeltaItem(int                          addressInChannel,
                                                      int                          lastUpdateTimestamp,
                                                      TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                      float                        value,
                                                      string                       additionalInfo,
                                                      TmAddr                       tmAddr,
                                                      string                       objectName)
    {
      var enumShift = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
      var updateTime = lastUpdateTimestamp == 0
                         ? (DateTime?) null
                         : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      return new DeltaItem
             {
               AddressInChannelString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                                          ? $"0x{addressInChannel:X}"
                                          : $"{addressInChannel + enumShift}",
               Type       = DeltaItemTypes.AccumFloat,
               TypeString = "ТИИ",
               UpdateTime = updateTime,
               ValueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                               ? $"{value:N6}"
                               : $"{value:N6} ?",
               AdditionalInfo = additionalInfo,
               TmAddress      = tmAddr,
               ObjectName = objectName
             };
    }


    public static DeltaItem CreateControlDeltaItem(int                          addressInChannel,
                                                   int                          lastUpdateTimestamp,
                                                   TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                   int                          controlBlock,
                                                   int                          controlGroup,
                                                   int                          controlPoint,
                                                   string                       additionalInfo,
                                                   TmAddr                       tmAddr,
                                                   string                       objectName)
    {
      var enumShift = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
      var updateTime = lastUpdateTimestamp == 0
                         ? (DateTime?) null
                         : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      return new DeltaItem
             {
               AddressInChannelString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                                          ? $"0x{addressInChannel:X}"
                                          : $"{addressInChannel + enumShift}",
               Type       = DeltaItemTypes.Control,
               TypeString = "ТУ",
               UpdateTime = updateTime,
               ValueString = controlBlock != 0xffff
                               ? $"{controlBlock + enumShift}-{controlGroup + enumShift}-{controlPoint + enumShift}"
                               : string.Empty,
               AdditionalInfo = additionalInfo,
               TmAddress      = tmAddr,
               ObjectName = objectName
             };
    }


    public static DeltaItem CreateStrValDeltaItem(int                          addressInChannel,
                                                  int                          lastUpdateTimestamp,
                                                  TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                  string                       valueString,
                                                  string                       additionalInfo,
                                                  TmAddr                       tmAddr)
    {
      var enumShift = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
      var updateTime = lastUpdateTimestamp == 0
                         ? (DateTime?) null
                         : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      return new DeltaItem
             {
               AddressInChannelString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                                          ? $"0x{addressInChannel:X}"
                                          : $"{addressInChannel + enumShift}",
               Type           = DeltaItemTypes.Analog,
               TypeString     = "СТР",
               UpdateTime     = updateTime,
               ValueString    = valueString,
               AdditionalInfo = additionalInfo,
               TmAddress      = tmAddr
             };
    }
  }
}