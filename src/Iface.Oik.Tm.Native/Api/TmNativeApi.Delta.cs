using System;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static int GetTreeChangeValue(int deltaCid)
  {
    return TmNative.tmcDntTreeChange(deltaCid);
  }

  public static (IReadOnlyCollection<T> items, string description) GetDeltaComponentsItems<T>(int deltaCid,
    uint[]                                                                                        traceChain)
    where T : DeltaItemBase, new()
  {
    var itemsListPtr = TmNative.tmcDntOpenItem(deltaCid,
                                               (uint)traceChain.Length,
                                               traceChain);

    if (itemsListPtr == nint.Zero)
    {
      return (Array.Empty<T>(), string.Empty);
    }

    var description = string.Empty;
    var items       = new List<T>();

    while (true)
    {
      var itemPtr = TmNative.tmcDntGetNextItem(itemsListPtr);
      if (itemPtr == nint.Zero)
      {
        break;
      }

      var commonStruct = TmNativeUtil.FromIntPtr<TmNativeDefsUnsafe.DeltaCommon>(itemPtr);

      switch ((TmNativeDefs.DeltaItemTypes)commonStruct.Type)
      {
        case TmNativeDefs.DeltaItemTypes.Description:
          var descString = GetDescriptionString(itemPtr);

          if (descString[0] == '*')
          {
            items.Add(DeltaItemBase.CreateDescription<T>(descString[1..]));
          }
          else
          {
            description += descString + Environment.NewLine;
          }
          
          break;
        case TmNativeDefs.DeltaItemTypes.Status:
          items.Add(DeltaItemBase.CreateStatus<T>(itemPtr));
          break;
        case TmNativeDefs.DeltaItemTypes.Analog:
          items.Add(DeltaItemBase.CreateAnalog<T>(itemPtr));
          break;
        case TmNativeDefs.DeltaItemTypes.AnalogF:
          items.Add(DeltaItemBase.CreateAnalogFloat<T>(itemPtr));
          break;
        case TmNativeDefs.DeltaItemTypes.Accum:
          items.Add(DeltaItemBase.CreateAccum<T>(itemPtr));
          break;
        case TmNativeDefs.DeltaItemTypes.AccumF:
          items.Add(DeltaItemBase.CreateAccumFloat<T>(itemPtr));
          break;
        case TmNativeDefs.DeltaItemTypes.Control:
          items.Add(DeltaItemBase.CreateControl<T>(itemPtr));
          break;
        case TmNativeDefs.DeltaItemTypes.StrVal:
          items.Add(DeltaItemBase.CreateStrVal<T>(itemPtr));
          break;
        default:
        {
          continue;
        }
      }
    }

    TmNative.tmcDntCloseItem(itemsListPtr);
    return (items, description);
  }

  internal static unsafe string GetDescriptionString(nint itemPtr)
  {
    return TmNativeUtil.GetCStringFromBytePtr((byte*)itemPtr + sizeof(TmNativeDefsUnsafe.DeltaDescription));
  }
}