using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api
{
  public class DeltaApi : IDeltaApi
  {
    private int _cid;

    private readonly ITmNative _native;

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
      var lookup = new Dictionary<string, DeltaComponent>();
      var components = await GetDeltaComponents();
      
      components.ForEach(x => lookup.Add(x.TraceChainString, x));
      foreach (var component in components)
      {
        if (!lookup.TryGetValue(component.ParentTraceChainString, out var proposedParent)) continue;
        component.Parent = proposedParent;
        proposedParent.Children.Add(component);
      }
      
      return lookup.Values.Where(x => x.Parent == null).ToList();
    }

    private async Task<IReadOnlyCollection<DeltaComponent>> GetDeltaComponents()
    {
      try
      {
        var components = new List<DeltaComponent>();
        
        var tempConfFile = Path.GetTempFileName();
        
        var result       = await Task.Run(() => _native.TmcDntGetConfig(_cid, tempConfFile));
        if (!result)
        {
          throw new Exception("Не удалось прочитать конфигурацию Дельты");
        }
        
        var charsToTrim = new[] {' ', '\t'};
        using (var streamReader = new StreamReader(tempConfFile, Encoding.GetEncoding(1251)))
        {
          string line;
          while ((line = await streamReader.ReadLineAsync()) != null)
          {
            var splitedArray = EncodingUtil.Win1251ToUtf8(line).Split(';');
            
            
            var nameAndType = splitedArray[1].Split(',')
                                             .Select(x => x.Trim(charsToTrim))
                                             .ToArray();
            var traceChainArray = splitedArray[0].Split(',')
                                                .Select(x => Convert.ToUInt32(x.Trim(charsToTrim), 10))
                                                .ToArray() ;

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

    public async Task<int> GetTreeChangeValue()
    {
      return await Task.Run(() => _native.TmcDntTreeChange(_cid));
    }
  }
}