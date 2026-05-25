using System;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class DeltaItemBase
{
  internal static T CreateDescription<T>(string descriptionString)
    where T : DeltaItemBase, new()
  {
    var component = new T();

    component.InitializeDescription(descriptionString);

    return component;
  }

  internal static unsafe T CreateStatus<T>(nint itemPtr)
    where T : DeltaItemBase, new()
  {
    var component = new T();

    var ptr    = (byte*)itemPtr;
    var data   = TmNativeUtil.FromBytesPtr<TmNativeDefs.DeltaStatus>(ptr);
    var offset = sizeof(TmNativeDefs.DeltaStatus);

    var flags = (TmNativeDefs.DeltaItemsFlags)data.DeltaFlags;

    var    addressInChannel = GetItemNum(ptr, offset, data.Length, data.Number);
    string addrInChannelStr;
    if (flags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum))
    {
      addrInChannelStr = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                           ? $"0x{addressInChannel:X}"
                           : $"{addressInChannel}";
    }
    else
    {
      var addressQuotient = Math.DivRem(addressInChannel, 8, out var addressRemainder);
      addrInChannelStr = $"{addressQuotient + 1}-{addressRemainder}";
    }

    var valueString = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                        ? $"{data.Value & 0xF}"
                        : $"{data.Value & 0xF} ?";

    if (flags.HasFlag(TmNativeDefs.DeltaItemsFlags.DestVal))
    {
      valueString = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.DestReli)
                      ? $"{valueString} ({data.Value >> 4})"
                      : $"{valueString} ({data.Value >> 4} ?)";
    }

    var addInfo = GetItemAdditionalText(ptr, offset, data.Length);
    if (flags.HasFlag(TmNativeDefs.DeltaItemsFlags.S2Break))
    {
      addInfo = $"{addInfo} <Обрыв>";
    }

    if (flags.HasFlag(TmNativeDefs.DeltaItemsFlags.S2Malfn))
    {
      addInfo = $"{addInfo} <Неисп>";
    }

    var dto = new InitializeTagDto
    {
      AddressInChannelString = addrInChannelStr,
      LastUpdate             = data.LastUpdate,
      ValueString            = valueString,
      AdditionalInfo         = addInfo,
      TmAddr = data.TmsRtu is 0
                 ? null
                 : (data.TmsChn,
                    data.TmsRtu,
                    data.TmsPoint)
    };

    component.InitializeStatus(dto);

    return component;
  }

  internal static unsafe T CreateAnalog<T>(nint itemPtr)
    where T : DeltaItemBase, new()
  {
    var component = new T();

    var ptr    = (byte*)itemPtr;
    var data   = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.DeltaAnalog>(ptr);
    var offset = sizeof(TmNativeDefsUnsafe.DeltaAnalog);

    var flags = (TmNativeDefs.DeltaItemsFlags)data.DeltaFlags;

    var addressInChannel = GetItemNum(ptr, offset, data.Length, data.Number);
    var enumShift        = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
    var addrInChannelStr = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                             ? $"0x{addressInChannel:X}"
                             : $"{addressInChannel + enumShift}";

    var hexValue = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Analong)
                     ? $"{data.Value & 0xffff:X}"
                     : $"{data.Value:X}";
    var valueString = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                        ? $"{data.Value} (0x{hexValue})"
                        : $"{data.Value} (0x{hexValue}) ?";

    var additionalInfo = GetItemAdditionalText(ptr, offset, data.Length);

    var dto = new InitializeTagDto
    {
      AddressInChannelString = addrInChannelStr,
      LastUpdate             = data.LastUpdate,
      ValueString            = valueString,
      AdditionalInfo         = additionalInfo,
      TmAddr = data.TmsRtu is 0
                 ? null
                 : (data.TmsChn,
                    data.TmsRtu,
                    data.TmsPoint)
    };

    component.InitializeAnalog(dto);

    return component;
  }

  internal static unsafe T CreateAnalogFloat<T>(nint itemPtr)
    where T : DeltaItemBase, new()
  {
    var component = new T();

    var ptr    = (byte*)itemPtr;
    var data   = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.DeltaAnalogF>(ptr);
    var offset = sizeof(TmNativeDefsUnsafe.DeltaAnalogF);

    var flags = (TmNativeDefs.DeltaItemsFlags)data.DeltaFlags;

    var addressInChannel = GetItemNum(ptr, offset, data.Length, data.Number);
    var enumShift        = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
    var addrInChannelStr = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                             ? $"0x{addressInChannel:X}"
                             : $"{addressInChannel + enumShift}";

    var valueString = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                        ? $"{data.Value:N6}"
                        : $"{data.Value:N6} ?";

    var additionalInfo = GetItemAdditionalText(ptr, offset, data.Length);

    var dto = new InitializeTagDto
    {
      AddressInChannelString = addrInChannelStr,
      LastUpdate             = data.LastUpdate,
      ValueString            = valueString,
      AdditionalInfo         = additionalInfo,
      TmAddr = data.TmsRtu is 0
                 ? null
                 : (data.TmsChn,
                    data.TmsRtu,
                    data.TmsPoint)
    };

    component.InitializeAnalog(dto);

    return component;
  }

  internal static unsafe T CreateAccum<T>(nint itemPtr)
    where T : DeltaItemBase, new()
  {
    var component = new T();

    var ptr    = (byte*)itemPtr;
    var data   = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.DeltaAccum>(ptr);
    var offset = sizeof(TmNativeDefsUnsafe.DeltaAccum);

    var flags = (TmNativeDefs.DeltaItemsFlags)data.DeltaFlags;

    var addressInChannel = GetItemNum(ptr, offset, data.Length, data.Number);
    var enumShift        = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
    var addrInChannelStr = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                             ? $"0x{addressInChannel:X}"
                             : $"{addressInChannel + enumShift}";

    var valueString = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                        ? $"{data.Value}"
                        : $"{data.Value} ?";

    var additionalInfo = GetItemAdditionalText(ptr, offset, data.Length);

    var dto = new InitializeTagDto
    {
      AddressInChannelString = addrInChannelStr,
      LastUpdate             = data.LastUpdate,
      ValueString            = valueString,
      AdditionalInfo         = additionalInfo,
      TmAddr = data.TmsRtu is 0
                 ? null
                 : (data.TmsChn,
                    data.TmsRtu,
                    data.TmsPoint)
    };

    component.InitializeAccum(dto);

    return component;
  }

  internal static unsafe T CreateAccumFloat<T>(nint itemPtr)
    where T : DeltaItemBase, new()
  {
    var component = new T();

    var ptr    = (byte*)itemPtr;
    var data   = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.DeltaAccumF>(ptr);
    var offset = sizeof(TmNativeDefsUnsafe.DeltaAccumF);

    var flags = (TmNativeDefs.DeltaItemsFlags)data.DeltaFlags;

    var addressInChannel = GetItemNum(ptr, offset, data.Length, data.Number);
    var enumShift        = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
    var addrInChannelStr = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                             ? $"0x{addressInChannel:X}"
                             : $"{addressInChannel + enumShift}";

    var valueString = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Reliable)
                        ? $"{data.Value:N6}"
                        : $"{data.Value:N6} ?";

    var additionalInfo = GetItemAdditionalText(ptr, offset, data.Length);

    var dto = new InitializeTagDto
    {
      AddressInChannelString = addrInChannelStr,
      LastUpdate             = data.LastUpdate,
      ValueString            = valueString,
      AdditionalInfo         = additionalInfo,
      TmAddr = data.TmsRtu is 0
                 ? null
                 : (data.TmsChn,
                    data.TmsRtu,
                    data.TmsPoint)
    };

    component.InitializeAccum(dto);

    return component;
  }

  internal static unsafe T CreateControl<T>(nint itemPtr)
    where T : DeltaItemBase, new()
  {
    var component = new T();

    var ptr    = (byte*)itemPtr;
    var data   = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.DeltaControl>(ptr);
    var offset = sizeof(TmNativeDefsUnsafe.DeltaControl);

    var flags = (TmNativeDefs.DeltaItemsFlags)data.DeltaFlags;

    var addressInChannel = GetItemNum(ptr, offset, data.Length, data.Number);
    var enumShift        = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
    var addrInChannelStr = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                             ? $"0x{addressInChannel:X}"
                             : $"{addressInChannel + enumShift}";

    var valueString = data.CtrlBlock != 0xffff
                        ? $"{data.CtrlBlock + enumShift}-{data.CtrlGroup + enumShift}-{data.CtrlPoint + enumShift}"
                        : string.Empty;

    var additionalInfo = GetItemAdditionalText(ptr, offset, data.Length);

    var dto = new InitializeTagDto
    {
      AddressInChannelString = addrInChannelStr,
      LastUpdate             = data.LastUpdate,
      ValueString            = valueString,
      AdditionalInfo         = additionalInfo,
      TmAddr = data.TmsRtu is 0
                 ? null
                 : (data.TmsChn,
                    data.TmsRtu,
                    data.TmsPoint)
    };

    component.InitializeControl(dto);

    return component;
  }

  internal static unsafe T CreateStrVal<T>(nint itemPtr)
    where T : DeltaItemBase, new()
  {
    var component = new T();

    var ptr    = (byte*)itemPtr;
    var data   = TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.DeltaStrval>(ptr);
    var offset = sizeof(TmNativeDefsUnsafe.DeltaStrval);

    var flags = (TmNativeDefs.DeltaItemsFlags)data.DeltaFlags;

    var addressInChannel = data.Number;
    var enumShift        = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.ZeroEnum) ? 0 : 1;
    var addrInChannelStr = flags.HasFlag(TmNativeDefs.DeltaItemsFlags.Hex)
                             ? $"0x{addressInChannel:X}"
                             : $"{addressInChannel + enumShift}";

    var descriptionOffset = offset + data.Length;
    var additionalInfo = data.Length > descriptionOffset
                           ? TmNativeUtil.GetCStringFromBytePtr(ptr + descriptionOffset)
                           : string.Empty;

    var valueString = TmNativeUtil.GetCStringFromBytePtr((byte*)itemPtr + offset);

    var dto = new InitializeTagDto
    {
      AddressInChannelString = addrInChannelStr,
      LastUpdate             = data.LastUpdate,
      ValueString            = valueString,
      AdditionalInfo         = additionalInfo,
      TmAddr = data.TmsRtu is 0
                 ? null
                 : (data.TmsChn,
                    data.TmsRtu,
                    data.TmsPoint)
    };
    
    component.InitializeStrVal(dto);

    return component;
  }

  protected abstract void InitializeDescription(string       descriptionString);
  protected abstract void InitializeStatus(InitializeTagDto  dto);
  protected abstract void InitializeAnalog(InitializeTagDto  dto);
  protected abstract void InitializeAccum(InitializeTagDto   dto);
  protected abstract void InitializeControl(InitializeTagDto dto);
  protected abstract void InitializeStrVal(InitializeTagDto dto);

  internal static unsafe int GetItemNum(byte* itemPtr, int itemStructLength, int actualLength, ushort baseNum)
  {
    var result = baseNum & ushort.MaxValue;

    if (actualLength <= itemStructLength)
    {
      return result;
    }

    var offsetPtr = itemPtr + itemStructLength;

    return result + offsetPtr[0] * 0x10000;
  }

  internal static unsafe string GetItemAdditionalText(byte* ptr, int offset, int actualLength)
  {
    return actualLength <= offset + 1
             ? string.Empty
             : TmNativeUtil.GetCStringFromBytePtr(ptr + offset + 1);
  }

  protected record InitializeTagDto
  {
    public string                                 AddressInChannelString { get; init; } = string.Empty;
    public int                                    LastUpdate             { get; init; }
    public (ushort ch, ushort rtu, ushort point)? TmAddr                 { get; init; }
    public string                                 AdditionalInfo         { get; init; } = string.Empty;
    public string                                 ValueString            { get; init; } = string.Empty;
  }
}