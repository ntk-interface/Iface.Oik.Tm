using System;
using System.Linq;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class DeltaItem
  {
    private readonly int _hashCode;
    
    public DeltaItemTypes Type                   { get; }
    public DateTime?      UpdateTime             { get; }
    public TmAddr         TmAddress              { get; }
    public string         ObjectName             { get; private set; }
    public string         AddressInChannelString { get; }
    public string         AdditionalInfo         { get; }
    public string         ValueString            { get; }

    public string UpdateTimeString => UpdateTime.HasValue ? $"{UpdateTime.Value:dd.MM.yyyy H:mm:ss}" : string.Empty;
    public string TmAddressString  => TmAddress == null ? string.Empty : TmAddress.ToString();


    private DeltaItem(DeltaItemTypes type, 
                      DateTime? updateTime, 
                      TmAddr tmAddress, 
                      string addressInChannelString, 
                      string additionalInfo,
                      string valueString)
    {
      Type                   = type;
      UpdateTime             = updateTime;
      TmAddress              = tmAddress;
      AddressInChannelString = addressInChannelString;
      AdditionalInfo         = additionalInfo;
      ValueString            = valueString;

      _hashCode = (type, updateTime, tmAddress, addressInChannelString, additionalInfo, valueString).GetHashCode();
    }
    

    public static DeltaItem CreateDescriptionDeltaItem(string descriptionString)
    {
      var descArray = descriptionString.TrimStart('*').Split(':');
      return new DeltaItem(DeltaItemTypes.Description,
                           null,
                           null,
                           string.Empty,
                           descArray.LastOrDefault(), string.Empty);
    }


    public static DeltaItem CreateStatusDeltaItem(int                          addressInChannel,
                                                  int                          lastUpdateTimestamp,
                                                  TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                  int                          value,
                                                  string                       additionalInfo,
                                                  TmAddr                       tmAddr)
    {
      var addrInChannelStr = string.Empty;
      if (deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum))
      {
        addrInChannelStr = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                             ? $"0x{addressInChannel:X}"
                             : $"{addressInChannel}";
      }
      else
      {
        var addressQuotient = Math.DivRem(addressInChannel, 8, out var addressRemainder);
        addrInChannelStr = $"{addressQuotient + 1}-{addressRemainder}";
      }

      var valueString = string.Empty;
      valueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                      ? $"{value & 0xF}"
                      : $"{value & 0xF} ?";

      if (deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.DestVal))
      {
        valueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.DestReli)
                        ? $"{valueString} ({value >> 4})"
                        : $"{valueString} ({value >> 4} ?)";
      }
      
      var updateTime = lastUpdateTimestamp == 0
                     ? (DateTime?) null
                     : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      var addInfo = additionalInfo;

      if (deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.S2Break))
      {
        addInfo = $"{addInfo} <Обрыв>";
      }

      if (deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.S2Malfn))
      {
        addInfo = $"{addInfo} <Неисп>";
      }

      return new DeltaItem(DeltaItemTypes.Status, updateTime, tmAddr, addrInChannelStr, addInfo, valueString);
    }


    public static DeltaItem CreateAnalogDeltaItem(int                          addressInChannel,
                                                  int                          lastUpdateTimestamp,
                                                  TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                  int                          value,
                                                  string                       additionalInfo,
                                                  TmAddr                       tmAddr)
    {
      var enumShift = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
      var hexValue  = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Analong) ? $"{value & 0xffff:X}" : $"{value:X}";
      var updateTime = lastUpdateTimestamp == 0
                         ? (DateTime?) null
                         : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      var addrInChannelStr = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                            ? $"0x{addressInChannel:X}"
                            : $"{addressInChannel + enumShift}";
      
      var valueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                          ? $"{value} (0x{hexValue})"
                          : $"{value} (0x{hexValue}) ?";
      
      return new DeltaItem(DeltaItemTypes.Analog, updateTime, tmAddr, addrInChannelStr, additionalInfo, valueString);
    }


    public static DeltaItem CreateAnalogFloatDeltaItem(int                          addressInChannel,
                                                       int                          lastUpdateTimestamp,
                                                       TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                       float                        value,
                                                       string                       additionalInfo,
                                                       TmAddr                       tmAddr)
    {
      var enumShift = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
      var updateTime = lastUpdateTimestamp == 0
                         ? (DateTime?) null
                         : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      var addrInChannelStr = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                               ? $"0x{addressInChannel:X}"
                               : $"{addressInChannel + enumShift}";

      var valueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                          ? $"{value:N6}"
                          : $"{value:N6} ?";

      return new DeltaItem(DeltaItemTypes.Analog, updateTime, tmAddr, addrInChannelStr, additionalInfo, valueString);
    }


    public static DeltaItem CreateAccumDeltaItem(int                          addressInChannel,
                                                 int                          lastUpdateTimestamp,
                                                 TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                 int                          value,
                                                 string                       additionalInfo,
                                                 TmAddr                       tmAddr)
    {
      var enumShift = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
      var updateTime = lastUpdateTimestamp == 0
                         ? (DateTime?) null
                         : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      var addrInChannelStr = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                               ? $"0x{addressInChannel:X}"
                               : $"{addressInChannel + enumShift}";

      var valueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                          ? $"{value} "
                          : $"{value} ?";

      return new DeltaItem(DeltaItemTypes.Accum, updateTime, tmAddr, addrInChannelStr, additionalInfo, valueString);
    }


    public static DeltaItem CreateAccumFloatDeltaItem(int                          addressInChannel,
                                                      int                          lastUpdateTimestamp,
                                                      TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                      float                        value,
                                                      string                       additionalInfo,
                                                      TmAddr                       tmAddr)
    {
      var enumShift = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
      var updateTime = lastUpdateTimestamp == 0
                         ? (DateTime?) null
                         : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      var addrInChannelStr = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                               ? $"0x{addressInChannel:X}"
                               : $"{addressInChannel + enumShift}";

      var valueString = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                          ? $"{value:N6}"
                          : $"{value:N6} ?";
      
      return new DeltaItem(DeltaItemTypes.AccumFloat, updateTime, tmAddr, addrInChannelStr, additionalInfo, valueString);
    }


    public static DeltaItem CreateControlDeltaItem(int                          addressInChannel,
                                                   int                          lastUpdateTimestamp,
                                                   TmNativeDefs.DeltaItemsFlags deltaFlags,
                                                   int                          controlBlock,
                                                   int                          controlGroup,
                                                   int                          controlPoint,
                                                   string                       additionalInfo,
                                                   TmAddr                       tmAddr)
    {
      var enumShift = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
      var updateTime = lastUpdateTimestamp == 0
                         ? (DateTime?) null
                         : DateUtil.GetDateTimeFromTimestamp(lastUpdateTimestamp);

      var addrInChannelStr = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                               ? $"0x{addressInChannel:X}"
                               : $"{addressInChannel + enumShift}";

      var valueString = controlBlock != 0xffff
                          ? $"{controlBlock + enumShift}-{controlGroup + enumShift}-{controlPoint + enumShift}"
                          : string.Empty;
      
      return new DeltaItem(DeltaItemTypes.Control, updateTime, tmAddr, addrInChannelStr, additionalInfo, valueString);
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
      
      var addrInChannelStr = deltaFlags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                               ? $"0x{addressInChannel:X}"
                               : $"{addressInChannel + enumShift}";

      return new DeltaItem(DeltaItemTypes.StrVal, updateTime, tmAddr, addrInChannelStr, additionalInfo, valueString);
    }

    public override int GetHashCode()
    {
      return _hashCode;
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as DeltaItem);
    }

    public bool Equals(DeltaItem comparison)
    {
      if (ReferenceEquals(comparison, null))
      {
        return false;
      }

      if (ReferenceEquals(this, comparison))
      {
        return true;
      }

      return comparison.GetHashCode() == GetHashCode();
    }
    
    public static bool operator ==(DeltaItem left, DeltaItem right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }

      return left.Equals(right);
    }

    public static bool operator !=(DeltaItem left, DeltaItem right)
    {
      return !(left == right);
    }

    public void SetObjectName(string name)
    {
      ObjectName = name;
    }
  }
}