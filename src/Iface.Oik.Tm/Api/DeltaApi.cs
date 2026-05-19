using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api
{
  public class DeltaApi : IDeltaApi
  {
    private int _cid;

    private readonly Regex _portStatsRegex = new
      Regex(@"@=(\d*),0=(\d*),1=(\d*),2=(\d*),3=(\d*)",
            RegexOptions.Compiled);


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
      return await Task.Run(() => TmNativeApi.GetTreeChangeValue(_cid))
                       .ConfigureAwait(false);
    }


    public async Task<(IReadOnlyCollection<DeltaItem>, string)> GetComponentsItems(DeltaComponent component)
    {
      return await Task.Run(() => TmNativeApi.GetDeltaComponentsItems<DeltaItem>(_cid, component.TraceChain))
                       .ConfigureAwait(false);
    }


    public async Task UpdateItemName(DeltaItem item)
    {
      switch (item.Type)
      {
        case DeltaItemTypes.Status:
        case DeltaItemTypes.Analog:
        case DeltaItemTypes.Accum:
        case DeltaItemTypes.Control:
        case DeltaItemTypes.AnalogFloat:
        case DeltaItemTypes.AccumFloat:
          item.SetObjectName(await GetObjectName(item.TmAddress).ConfigureAwait(false));
          break;
        default:
          return;
      }
    }


    public async Task<bool> RegisterTracer()
    {
      return await Task.Run(() => TmNative.tmcDntRegisterUser(_cid)).ConfigureAwait(false);
    }


    public async Task UnRegisterTracer()
    {
      await Task.Run(() => TmNative.tmcDntUnRegisterUser(_cid)).ConfigureAwait(false);
    }


    public async Task TraceComponent(DeltaComponent  component,
                                     DeltaTraceTypes traceType)
    {
      var traceFlag = traceType == DeltaTraceTypes.Protocol
                        ? TmNativeDefs.DeltaTraceFlags.Usr
                        : TmNativeDefs.DeltaTraceFlags.Drv;

      await Task.Run(() => TmNativeApi.TraceDeltaComponent(_cid,
                                                      component.TraceChain,
                                                      traceFlag, 
                                                      traceType == DeltaTraceTypes.Physical))
                .ConfigureAwait(false);
    }


    public async Task StartDebug()
    {
      await Task.Run(() => TmNative.tmcDntBeginDebug(_cid))
                .ConfigureAwait(false);
    }


    public async Task StopDebug()
    {
      await Task.Run(() => TmNative.tmcDntStopDebug(_cid))
                .ConfigureAwait(false);
    }


    public async Task StopTrace()
    {
      await Task.Run(() => TmNative.tmcDntStopTrace(_cid)).ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<DeltaComponentErrorState>> UpdateComponentsTreeLiveInfo(IEnumerable<DeltaComponent> tree)
    {
      var flattenedTree = tree.Flatten();

      var errorStatesList = new List<DeltaComponentErrorState>();
      foreach (var node in flattenedTree)
      {
        switch (node.Level)
        {
          case 2:
          case 3:
            uint data = 5;
            var result = await Task.Run(() => TmNative.tmcDntGetLiveInfo(_cid,
                                                                         (uint)node.Level,
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
              node.State = (DeltaComponentStates)data;
            }

            if (node.State is not DeltaComponentStates.Ok)
            {
              errorStatesList.Add(new DeltaComponentErrorState(node));
            } 
            
            break;
          default:
            node.State = DeltaComponentStates.Ok;
            break;
        }
      }

      return errorStatesList;
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
      return await Task.Run(() => TmNativeApi.GetDeltaComponents<DeltaComponent>(_cid))
                       .ConfigureAwait(false);
    }

    private async Task<string> GetObjectName(TmAddr tmAddr)
    {
      return await Task.Run(() => GetObjectNameSync(tmAddr))
                       .ConfigureAwait(false);
    }

    private string GetObjectNameSync(TmAddr tmAddr)
    {
      if (tmAddr == null || tmAddr.Type == TmType.Unknown)
      {
        return "";
      }

      const int  bufSize = 1024;
      Span<byte> buf     = stackalloc byte[bufSize];

      TmNative.tmcDntGetObjectName(_cid,
                                   (ushort)tmAddr.Type.ToNativeType(),
                                   (short)tmAddr.Ch,
                                   (short)tmAddr.Rtu,
                                   (short)tmAddr.Point,
                                   buf,
                                   bufSize);

      return EncodingUtil.BytesToString(buf);
    }

    private async Task UpdateDeltaComponentPortStats(DeltaComponent component)
    {
      var (result, portStatsString) = await Task.Run(() => GetPortStatsSync(component.TraceChain))
                                                .ConfigureAwait(false);

      if (result == 0 || portStatsString.IsNullOrEmpty()) return;

      var (ticks, statusCount, analogCount, accumCount, messagesCount) = ParsePortStatsString(portStatsString);

      if (component.InitialPerformanceStats == null)
      {
        component.SetInitialPerformanceStats(ticks, statusCount, analogCount, accumCount, messagesCount);
        return;
      }

      component.UpdatePerformanceStatsAndString(ticks, statusCount, analogCount, accumCount, messagesCount);
    }

    private (uint, string) GetPortStatsSync(uint[] traceChain)
    {
      const int  bufLength = 1024;
      Span<byte> buf       = stackalloc byte[bufLength];

      var result = TmNative.tmcDntGetPortStats(_cid,
                                               traceChain,
                                               buf,
                                               bufLength);

      return (result, EncodingUtil.BytesToString(buf));
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