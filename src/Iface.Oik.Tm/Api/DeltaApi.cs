using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api
{
  public class DeltaApi : IDeltaApi
  {
    private int _cid;

    private readonly ITmNative _native;

    private readonly Regex _portStatsRegex = new
      Regex(@"@=(\d*),0=(\d*),1=(\d*),2=(\d*),3=(\d*)",
            RegexOptions.Compiled);

    public DeltaApi(ITmNative native)
    {
      _native = native;
    }


    public void SetCid(int cid)
    {
      _cid = cid;
    }

    public async Task<IReadOnlyCollection<DeltaComponent>> GetComponentsTree()
    {
      var lookup     = new Dictionary<string, DeltaComponent>();
      var components = await GetDeltaComponents().ConfigureAwait(false);

      components.ForEach(x => lookup.Add(x.TraceChainString, x));
      foreach (var component in components)
      {
        if (!lookup.TryGetValue(component.ParentTraceChainString, out var proposedParent)) continue;
        component.Parent = proposedParent;
        proposedParent.Children.Add(component);
      }

      return lookup.Values.Where(x => x.Parent == null).ToList();
    }


    public async Task<int> GetTreeChangeValue()
    {
      return await Task.Run(() => _native.TmcDntTreeChange(_cid))
                       .ConfigureAwait(false);
    }


    public async Task GetComponentsItems(DeltaComponent component)
    {
      var componentItemsPtr = await Task.Run(() => _native.TmcDntOpenItem(_cid,
                                                                          (uint) component.TraceChain.Length,
                                                                          component.TraceChain))
                                        .ConfigureAwait(false);

      if (componentItemsPtr == IntPtr.Zero) return;

      component.ClearItems();

      while (true)
      {
        var itemPtr = await Task.Run(() => _native.TmcDntGetNextItem(componentItemsPtr))
                                .ConfigureAwait(false);

        if (itemPtr == IntPtr.Zero) break;

        var deltaCommonStruct = Marshal.PtrToStructure<TmNativeDefs.DeltaCommon>(itemPtr);

        switch ((TmNativeDefs.DeltaItemTypes) deltaCommonStruct.Type)
        {
          case TmNativeDefs.DeltaItemTypes.Description:
            var descriptionStruct     = Marshal.PtrToStructure<TmNativeDefs.DeltaDescription>(itemPtr);
            var descriptionStructSize = Marshal.SizeOf(descriptionStruct);

            var descriptionString =
              EncodingUtil.Win1251ToUtf8(Marshal.PtrToStringAnsi(IntPtr.Add(itemPtr, descriptionStructSize - 1)));

            if (descriptionStruct.Text[0] == '*')
            {
              component.Items.Add(DeltaItem.CreateDescriptionDeltaItem(descriptionString));
            }
            else
            {
              component.Description += descriptionString + Environment.NewLine;
            }

            break;
          case TmNativeDefs.DeltaItemTypes.Status:
            var statusStruct     = Marshal.PtrToStructure<TmNativeDefs.DeltaStatus>(itemPtr);
            var statusStructSize = Marshal.SizeOf(statusStruct);

            var numStatus = GetDeltaItemNum(itemPtr,
                                            statusStructSize,
                                            statusStruct.Length,
                                            statusStruct.Number);

            var addStringStatus = GetDeltaItemAdditionalText(itemPtr,
                                                             statusStructSize,
                                                             statusStruct.Length);

            var tmAddrStatus = statusStruct.TmsRtu == 0 || statusStruct.TmsRtu == 0
                                 ? null
                                 : new TmAddr(TmType.Status,
                                              statusStruct.TmsChn,
                                              statusStruct.TmsRtu,
                                              statusStruct.TmsPoint);

            var statusObjectName = await GetObjectName(tmAddrStatus).ConfigureAwait(false);

            component.Items.Add(DeltaItem.CreateStatusDeltaItem(numStatus,
                                                                statusStruct.LastUpdate,
                                                                (TmNativeDefs.DeltaItemsFlags) statusStruct.DeltaFlags,
                                                                statusStruct.Value,
                                                                addStringStatus,
                                                                tmAddrStatus,
                                                                statusObjectName));

            break;
          case TmNativeDefs.DeltaItemTypes.Analog:
            var analogStruct     = Marshal.PtrToStructure<TmNativeDefs.DeltaAnalog>(itemPtr);
            var analogStructSize = Marshal.SizeOf(analogStruct);

            var numAnalog = GetDeltaItemNum(itemPtr,
                                            analogStructSize,
                                            analogStruct.Length,
                                            analogStruct.Number);

            var addStringAnalog = GetDeltaItemAdditionalText(itemPtr,
                                                             analogStructSize,
                                                             analogStruct.Length);
            var tmAddrAnalog = analogStruct.TmsRtu == 0 || analogStruct.TmsRtu == 0
                                 ? null
                                 : new TmAddr(TmType.Analog,
                                              analogStruct.TmsChn,
                                              analogStruct.TmsRtu,
                                              analogStruct.TmsPoint);

            var analogObjectName = await GetObjectName(tmAddrAnalog).ConfigureAwait(false);

            component.Items.Add(DeltaItem.CreateAnalogDeltaItem(numAnalog,
                                                                analogStruct.LastUpdate,
                                                                (TmNativeDefs.DeltaItemsFlags) analogStruct.DeltaFlags,
                                                                analogStruct.Value,
                                                                addStringAnalog,
                                                                tmAddrAnalog,
                                                                analogObjectName));

            break;
          case TmNativeDefs.DeltaItemTypes.Accum:
            var accumStruct     = Marshal.PtrToStructure<TmNativeDefs.DeltaAccum>(itemPtr);
            var accumStructSize = Marshal.SizeOf(accumStruct);

            var numAccum = GetDeltaItemNum(itemPtr,
                                           accumStructSize,
                                           accumStruct.Length,
                                           accumStruct.Number);

            var addStringAccum = GetDeltaItemAdditionalText(itemPtr,
                                                            accumStructSize,
                                                            accumStruct.Length);

            var tmAddrAccum = accumStruct.TmsRtu == 0 || accumStruct.TmsRtu == 0
                                ? null
                                : new TmAddr(TmType.Accum,
                                             accumStruct.TmsChn,
                                             accumStruct.TmsRtu,
                                             accumStruct.TmsPoint);

            var accumObjectName = await GetObjectName(tmAddrAccum).ConfigureAwait(false);

            component.Items.Add(DeltaItem.CreateAccumDeltaItem(numAccum,
                                                               accumStruct.LastUpdate,
                                                               (TmNativeDefs.DeltaItemsFlags) accumStruct.DeltaFlags,
                                                               accumStruct.Value,
                                                               addStringAccum,
                                                               tmAddrAccum,
                                                               accumObjectName));

            break;
          case TmNativeDefs.DeltaItemTypes.Control:
            var controlStruct     = Marshal.PtrToStructure<TmNativeDefs.DeltaControl>(itemPtr);
            var controlStructSize = Marshal.SizeOf(controlStruct);

            var numControl = GetDeltaItemNum(itemPtr,
                                             controlStructSize,
                                             controlStruct.Length,
                                             controlStruct.Number);

            var addStringControl = GetDeltaItemAdditionalText(itemPtr,
                                                              controlStructSize,
                                                              controlStruct.Length);

            var tmAddrControl = controlStruct.TmsRtu == 0 || controlStruct.TmsRtu == 0
                                  ? null
                                  : new TmAddr(TmType.Status,
                                               controlStruct.TmsChn,
                                               controlStruct.TmsRtu,
                                               controlStruct.TmsPoint);

            var controlObjectName = await GetObjectName(tmAddrControl).ConfigureAwait(false);

            component.Items.Add(DeltaItem.CreateControlDeltaItem(numControl,
                                                                 controlStruct.LastUpdate,
                                                                 (TmNativeDefs.DeltaItemsFlags) controlStruct
                                                                   .DeltaFlags,
                                                                 controlStruct.CtrlBlock,
                                                                 controlStruct.CtrlGroup,
                                                                 controlStruct.CtrlPoint,
                                                                 addStringControl,
                                                                 tmAddrControl,
                                                                 controlObjectName));

            break;
          case TmNativeDefs.DeltaItemTypes.AnalogF:
            var analogFStruct     = Marshal.PtrToStructure<TmNativeDefs.DeltaAnalogF>(itemPtr);
            var analogFStructSize = Marshal.SizeOf(analogFStruct);

            var numAnalogF = GetDeltaItemNum(itemPtr,
                                             analogFStructSize,
                                             analogFStruct.Length,
                                             analogFStruct.Number);

            var addStringAnalogF = GetDeltaItemAdditionalText(itemPtr,
                                                              analogFStructSize,
                                                              analogFStruct.Length);
            var tmAddrAnalogF = analogFStruct.TmsRtu == 0 || analogFStruct.TmsRtu == 0
                                  ? null
                                  : new TmAddr(TmType.Analog,
                                               analogFStruct.TmsChn,
                                               analogFStruct.TmsRtu,
                                               analogFStruct.TmsPoint);

            var analogFObjectName = await GetObjectName(tmAddrAnalogF).ConfigureAwait(false);

            component.Items.Add(DeltaItem.CreateAnalogFloatDeltaItem(numAnalogF,
                                                                     analogFStruct.LastUpdate,
                                                                     (TmNativeDefs.DeltaItemsFlags) analogFStruct
                                                                       .DeltaFlags,
                                                                     analogFStruct.Value,
                                                                     addStringAnalogF,
                                                                     tmAddrAnalogF,
                                                                     analogFObjectName));
            break;
          case TmNativeDefs.DeltaItemTypes.AccumF:
            var accumFStruct     = Marshal.PtrToStructure<TmNativeDefs.DeltaAccumF>(itemPtr);
            var accumFStructSize = Marshal.SizeOf(accumFStruct);

            var numAccumF = GetDeltaItemNum(itemPtr,
                                            accumFStructSize,
                                            accumFStruct.Length,
                                            accumFStruct.Number);

            var addStringAccumF = GetDeltaItemAdditionalText(itemPtr,
                                                             accumFStructSize,
                                                             accumFStruct.Length);
            var tmAddrAccumF = accumFStruct.TmsRtu == 0 || accumFStruct.TmsRtu == 0
                                 ? null
                                 : new TmAddr(TmType.Accum,
                                              accumFStruct.TmsChn,
                                              accumFStruct.TmsRtu,
                                              accumFStruct.TmsPoint);

            var accumFObjectName = await GetObjectName(tmAddrAccumF).ConfigureAwait(false);

            component.Items.Add(DeltaItem.CreateAccumFloatDeltaItem(numAccumF,
                                                                    accumFStruct.LastUpdate,
                                                                    (TmNativeDefs.DeltaItemsFlags) accumFStruct
                                                                      .DeltaFlags,
                                                                    accumFStruct.Value,
                                                                    addStringAccumF,
                                                                    tmAddrAccumF,
                                                                    accumFObjectName));
            break;
          case TmNativeDefs.DeltaItemTypes.StrVal:
            var strValStruct     = Marshal.PtrToStructure<TmNativeDefs.DeltaStrval>(itemPtr);
            var strValStructSize = Marshal.SizeOf(strValStruct);

            var strValValueString =
              EncodingUtil.Win1251ToUtf8(Marshal.PtrToStringAnsi(IntPtr.Add(itemPtr, strValStructSize - 1)));

            var structSizeWithValueString = strValStructSize + strValValueString.Length;

            var strValDescriptionString = strValStruct.Length > structSizeWithValueString
                                            ? EncodingUtil.Win1251ToUtf8(Marshal.PtrToStringAnsi(IntPtr.Add(itemPtr,
                                                                           structSizeWithValueString)))
                                            : "";

            var tmAddrStrVal = strValStruct.TmsRtu == 0 || strValStruct.TmsRtu == 0
                                 ? null
                                 : new TmAddr(TmType.Unknown,
                                              strValStruct.TmsChn,
                                              strValStruct.TmsRtu,
                                              strValStruct.TmsPoint);

            component.Items.Add(DeltaItem.CreateStrValDeltaItem(strValStruct.Number,
                                                                strValStruct.LastUpdate,
                                                                (TmNativeDefs.DeltaItemsFlags) strValStruct.DeltaFlags,
                                                                strValValueString,
                                                                strValDescriptionString,
                                                                tmAddrStrVal));
            break;
          default:
            continue;
        }
      }

      await Task.Run(() => _native.TmcDntCloseItem(componentItemsPtr))
                .ConfigureAwait(false);
    }


    public async Task<bool> RegisterTracer()
    {
      return await Task.Run(() => _native.TmcDntRegisterUser(_cid)).ConfigureAwait(false);
    }


    public async Task UnRegisterTracer()
    {
      await Task.Run(() => _native.TmcDntUnRegisterUser(_cid)).ConfigureAwait(false);
    }


    public async Task TraceComponent(DeltaComponent  component,
                                     DeltaTraceTypes traceType,
                                     bool            showDebugMessages)
    {
      var traceFlag = traceType == DeltaTraceTypes.Protocol
                        ? TmNativeDefs.DeltaTraceFlags.Usr
                        : TmNativeDefs.DeltaTraceFlags.Drv;

      var traceChain = component.TraceChain;

      if (traceType == DeltaTraceTypes.Physical)
      {
        traceChain[0] = ~traceChain[0];
      }

      await Task.Run(() => _native.TmcDntBeginTraceEx(_cid,
                                                      (uint) traceChain.Length,
                                                      traceChain,
                                                      (uint) traceFlag,
                                                      0,
                                                      0))
                .ConfigureAwait(false);

      if (showDebugMessages)
      {
        await Task.Run(() => _native.TmcDntStopDebug(_cid))
                  .ConfigureAwait(false);
      }
    }


    public async Task StopTrace()
    {
      await Task.Run(() => _native.TmcDntStopDebug(_cid))
                .ConfigureAwait(false);
      await Task.Run(() => _native.TmcDntStopTrace(_cid)).ConfigureAwait(false);
    }


    public async Task UpdateComponentsTreeLiveInfo(IReadOnlyCollection<DeltaComponent> tree)
    {
      var flattenedTree = tree.Flatten();

      foreach (var node in flattenedTree)
      {
        switch (node.Level)
        {
          case 2:
          case 3:
            uint data = 5;
            var result = await Task.Run(() => _native.TmcDntGetLiveInfo(_cid,
                                                                        (uint) node.Level,
                                                                        node.TraceChain,
                                                                        out data,
                                                                        sizeof(uint)))
                                   .ConfigureAwait(false);
            if (result < sizeof(uint))
            {
              var lastError = await Task.Run(Tms.GetLastError)
                                        .ConfigureAwait(false);
              node.State = lastError == 120 ? DeltaComponentStates.NotSupported : DeltaComponentStates.Unknown;
            }
            else
            {
              node.State = (DeltaComponentStates) data;
            }

            break;
          default:
            node.State = DeltaComponentStates.None;
            break;
        }
      }
    }


    public async Task UpdatePortsStats(IReadOnlyCollection<DeltaComponent> components)
    {
      foreach (var node in components)
      {
        if (node.Level != 3) continue;

        await UpdateDeltaComponentPortStats(node).ConfigureAwait(false);
      }
    }


    private async Task<IReadOnlyCollection<DeltaComponent>> GetDeltaComponents()
    {
      try
      {
        var components = new List<DeltaComponent>();

        var tempConfFile = Path.GetTempFileName();

        var result = await Task.Run(() => _native.TmcDntGetConfig(_cid, tempConfFile))
                               .ConfigureAwait(false);
        if (!result)
        {
          return null;
        }

        var charsToTrim = new[] {' ', '\t'};
        using (var streamReader = new StreamReader(tempConfFile, Encoding.GetEncoding(1251)))
        {
          string line;
          while ((line = await streamReader.ReadLineAsync()
                                           .ConfigureAwait(false)) != null)
          {
            var splitedArray = EncodingUtil.Win1251ToUtf8(line).Split(';');


            var nameAndType = splitedArray[1].Split(',')
                                             .Select(x => x.Trim(charsToTrim))
                                             .ToArray();
            var traceChainArray = splitedArray[0].Split(',')
                                                 .Select(x => Convert.ToUInt32(x.Trim(charsToTrim), 10))
                                                 .ToArray();

            components.Add(new DeltaComponent(nameAndType[1], nameAndType[0], traceChainArray));
          }
        }

        File.Delete(tempConfFile);
        return components;
      }
      catch (Exception)
      {
        throw new Exception("Не удалось создать временный файл конфигурации Дельты.");
      }
    }

    private static int GetDeltaItemNum(IntPtr itemPtr, int itemStructLength, int actualLength, ushort baseNum)
    {
      var result = baseNum & ushort.MaxValue;

      if (actualLength <= itemStructLength) return result;

      var extendedNum = Marshal.ReadByte(IntPtr.Add(itemPtr, itemStructLength)) * 0x10000;


      return result + extendedNum;
    }

    private static string GetDeltaItemAdditionalText(IntPtr itemPtr, int itemStructLength, int actualLength)
    {
      if (actualLength <= itemStructLength + 1) return "";

      var result = Marshal.PtrToStringAnsi(IntPtr.Add(itemPtr, itemStructLength + 1));
      return EncodingUtil.Win1251ToUtf8(result);
    }

    private async Task<string> GetObjectName(TmAddr tmAddr)
    {
      if (tmAddr == null || tmAddr.Type == TmType.Unknown) return "";
      const int bufSize = 1024;
      var       buf     = new StringBuilder(bufSize);

      await Task.Run(() => _native.TmcDntGetObjectName(_cid,
                                                       (ushort) tmAddr.Type.ToNativeType(),
                                                       (short) tmAddr.Ch,
                                                       (short) tmAddr.Rtu,
                                                       (short) tmAddr.Point,
                                                       ref buf,
                                                       bufSize))
                .ConfigureAwait(false);
      return buf.ToString();
    }

    private async Task UpdateDeltaComponentPortStats(DeltaComponent component)
    {
      const int bufLength = 1024;
      var       buf       = new StringBuilder(bufLength);

      var result = await Task.Run(() =>
                                    _native.TmcDntGetPortStats(_cid,
                                                               component.TraceChain,
                                                               ref buf,
                                                               bufLength))
                             .ConfigureAwait(false);

      if (result == 0 || buf.ToString().IsNullOrEmpty()) return;

      var (ticks, statusCount, analogCount, accumCount, messagesCount) = ParsePortStatsString(buf.ToString());

      if (component.InitialPerformanceStats == null)
      {
        component.SetInitialPerformanceStats(ticks, statusCount, analogCount, accumCount, messagesCount);
        return;
      }

      component.UpdatePerformanceStatsAndString(ticks, statusCount, analogCount, accumCount, messagesCount);
    }

    private (long, long, long, long, long) ParsePortStatsString(string portStatsString)
    {
      var mc = _portStatsRegex.Match(portStatsString);
      return (Convert.ToInt64(mc.Groups[1].Value),
              Convert.ToInt64(mc.Groups[2].Value),
              Convert.ToInt64(mc.Groups[3].Value),
              Convert.ToInt64(mc.Groups[4].Value),
              Convert.ToInt64(mc.Groups[5].Value));
    }
  }
}