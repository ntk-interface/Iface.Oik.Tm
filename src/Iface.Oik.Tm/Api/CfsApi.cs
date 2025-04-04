using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Iface.Oik.Tm.Native.Interfaces.TmNativeDefs;

namespace Iface.Oik.Tm.Api
{
  public class CfsApi : ICfsApi
  {
    private readonly ITmNative _native;

    private readonly Regex _cfsServerLogRecordRegex = new
      Regex(@"(\d{2}:\d{2}:\d{2}.\d{3}) (\d{2}.\d{2}.\d{4}) [\\]{3}([^\\]*)[\\]{2}([^\\]*)[\\]{2}([^\s]*)\s*- ThID=([0-9A-Fx]*) :\n([^\n]*)(\n|$)",
            RegexOptions.Compiled);

    public IntPtr CfId { get; private set; }
    public string Host { get; private set; }

    public CfsApi(ITmNative native)
    {
      _native = native;
    }

    public void SetCfIdAndHost(IntPtr cfId, string host)
    {
      CfId = cfId;
      Host = host;
    }

    public string MakeInprocCrd(string machine, string user, string pwd)
    {
      var ptr = _native.CfsMakeInprocCrd(
                                         EncodingUtil.Utf8ToWin1251Bytes(machine),
                                         EncodingUtil.Utf8ToWin1251Bytes(user),
                                         EncodingUtil.Utf8ToWin1251Bytes(pwd));
      if (ptr != IntPtr.Zero)
      {
        string res = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(ptr);
        _native.CfsFreeMemory(ptr);
        return res;
      }
      else
      {
        return string.Empty;
      }
    }

    public async Task<(IntPtr, DateTime)> OpenConfigurationTree(string fileName)
    {
      var       fileTime        = new TmNativeDefs.FileTime();
      const int errStringLength = 1000;
      var       errBuf          = new byte[errStringLength];
      uint      errCode         = 0;

      var cfTreeRoot = await Task.Run(() => _native.CfsConfFileOpenCid(CfId,
                                                                       EncodingUtil.Utf8ToWin1251Bytes(Host),
                                                                       EncodingUtil.Utf8ToWin1251Bytes(fileName),
                                                                       30000 | TmNativeDefs.FailIfNoConnect,
                                                                       ref fileTime,
                                                                       out errCode,
                                                                       ref errBuf,
                                                                       errStringLength))
                                 .ConfigureAwait(false);

      if (cfTreeRoot == IntPtr.Zero)
      {
        throw new Exception($"Ошибка получения конфигурации: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
      }

      return (cfTreeRoot, GetDateTimeFromCustomFileTime(fileTime));
    }


    public async Task<(IntPtr, DateTime)> OpenMasterServiceConfiguration()
    {
      return await OpenConfigurationTree(MasterConfFile).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<CfTreeNode>> GetReserveConfiguration()
    {
      var resTree = new List<CfTreeNode>();
      try
      {
        var (resHandle, _) = await OpenConfigurationTree(HotStanbyConfFile).ConfigureAwait(false);
        resTree            = await GetCfTree(resHandle).ConfigureAwait(false);
        FreeConfigurationTreeHandle(resHandle);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      
      return resTree;
    }
    

    public async Task<ReserveServerState> ReserveServerTypeIsWorking(CfTreeNode reserveConfNode)
    {
      if (!reserveConfNode.CfProperties.TryGetValue("Type", out var type) || type.Trim() != "1")
      {
        return new ReserveServerState {IsWorking = false};
      }

      if (!reserveConfNode.CfProperties.TryGetValue("Addr", out var sAddr))
      {
        return new ReserveServerState {IsWorking = false};
      }

      var ipAddr = TmNativeUtil.IpAddrToNativeDword(sAddr);

      if (ipAddr == 0)
      {
        return new ReserveServerState {IsWorking = false};
      }
      

      if (!reserveConfNode.CfProperties.TryGetValue("Port", out var sPort) 
          || !ushort.TryParse(sPort, out var port) 
          || port == 0 
          || port > 0xfff)
      {
        return new ReserveServerState {IsWorking = false};
      }

      var bPort = port;
          
      if (reserveConfNode.CfProperties.TryGetValue("BPort", out var sBPort) 
          && ushort.TryParse(sBPort, out var nBPort) )
      {
        bPort = nBPort;
            
        if (bPort == 0 || bPort > 0xfff)
        {
          return new ReserveServerState {IsWorking = false};
        }
      }

      var split = reserveConfNode.Name.Split(':');

      if (split.Length != 2)
      {
        return new ReserveServerState {IsWorking = false};
      }

      var bCastSignature = await GetReserveServerBroadcastSignature(split.First()).ConfigureAwait(false);

      if (bCastSignature is BroadcastServerSignature.None)
      {
        return new ReserveServerState {IsWorking = false};
      }

      return await ReserveServerTypeIsWorking(ipAddr, bPort, port, bCastSignature).ConfigureAwait(false);
    }
    
    
    public async Task<ReserveServerState> ReserveServerTypeIsWorking(uint                     ipAddrDword, 
                                                                      ushort                   bPort, 
                                                                      ushort                   port,
                                                                      BroadcastServerSignature bCastSignature)
    {
      const int reserveServerNameBufSize = 64;
      const int errBufSize = 2048;
      
      var  reserveServerNameBuf = new byte[reserveServerNameBufSize];
      var  errBuf               = new byte[errBufSize];
      uint errCode              = 0;
      var  isWorking            = false;
      
      var result = await Task.Run(() => _native.CfsIsReserveWorking(CfId,
                                                                ipAddrDword,
                                                                bPort,
                                                                port,
                                                                (uint)bCastSignature,
                                                                out isWorking,
                                                                ref reserveServerNameBuf,
                                                                out errCode,
                                                                ref errBuf,
                                                                errBufSize))
                             .ConfigureAwait(false);

      if (errCode != 0 || !reserveServerNameBuf.Any() )
      {
        return new ReserveServerState {IsWorking = false};
      }

      return new ReserveServerState
      {
        IsWorking      = isWorking && result,
        RemotePipeName = EncodingUtil.Win1251BytesToUtf8(reserveServerNameBuf),
        Signature      = bCastSignature
      };;
    }


    public async Task<BroadcastServerSignature> GetReserveServerBroadcastSignature(string binName)
    {
      const string path    = "@@";
      const string section = "VPath";
      
      var basePath = await GetIniString(path, section).ConfigureAwait(false);
      
      var sectionName = $"{binName}#1.prp.Layout";

      // сигнатура сервера бродкастных сообщений
      var sSrvResType = await GetIniString($"{basePath}\\s_setup.ini", sectionName, "ResType").ConfigureAwait(false);

      switch (sSrvResType)
      {
        case "1":
          return BroadcastServerSignature.Sbr;
        case "2":
          return BroadcastServerSignature.Smt;
        default:
          return BroadcastServerSignature.None;
      }
    }


    public async Task OverrideReservePipe(string pipeName, BroadcastServerSignature signature)
    {
      const string path    = "@@";
      const string section = "VPath";
      
      var basePath = await GetIniString(path, section).ConfigureAwait(false);
      var iniPath  = string.Empty;
      
      switch (signature)
      {
        case BroadcastServerSignature.Sbr:
          iniPath = Path.Combine(basePath, "RB_SERV", pipeName, "rbsx.ini");
          break;
        case BroadcastServerSignature.Smt:
          iniPath = Path.Combine(basePath, "TM_SERV", pipeName, "tmsx.ini");
          break;
        default:
          return;
      }

      await SetIniString(iniPath, "Reserve", "SrcCheckTarget", "0").ConfigureAwait(false);
    }
    
    
    public async Task<(MSTreeNode, DateTime)> LoadFullMSTree()
    {
      var (handle, time) = await OpenMasterServiceConfiguration().ConfigureAwait(false);
      var tree = await GetCfTree(handle).ConfigureAwait(false);

      FreeConfigurationTreeHandle(handle);

      // считаем что дерево мастер-сервиса всегда начинается с одного элемента
      var msRoot = new MSTreeNode(tree.First());

      // читаем конфигурацию резервирования если есть
      var resTree = await GetReserveConfiguration().ConfigureAwait(false);

      // перебираем элементы на втором уровне после мастер-сервиса
      foreach (var node in msRoot.Children)
      {
        // ищем резервирование
        if (node.Properties is ReservedNodeProperties rnP)
        {
          var resNode = resTree.SingleOrDefault(x =>
                                                {
                                                  var split = x.Name.Split(':');
                                                  return split.Length == 2 && split.Last() == rnP.PipeName;
                                                });

          if (resNode != null)
          {
            FillReserveNodeProperties(rnP, resNode);
          }
        }

        // ищем сервера БД
        if (node.Properties is RbsNodeProperties rbsP 
            && (node.ProgName == MSTreeConsts.RBaseServer || node.ProgName == MSTreeConsts.rbsrv_old))
        {
          await FillRbsNodeProperties(rbsP).ConfigureAwait(false);
        }

        // под серверами ТМ ищем дорасчёт
        if (!node.ProgName.Equals(MSTreeConsts.TmServer) && !node.ProgName.Equals(MSTreeConsts.pcsrv_old) 
            || node.Children == null || !(node.Properties is ChildNodeProperties tmsP))
        {
          continue;
        }

        foreach (var child in node.Children)
        {
          if (!(child.Properties is TmCalcNodeProperties calcP))
          {
            continue;
          }

          await FillTmCalcProperties(calcP, tmsP.PipeName).ConfigureAwait(false);
          break;
        }
      }

      return (msRoot, time);
    }

    private static void FillReserveNodeProperties(ReservedNodeProperties properties, CfTreeNode node)
    {
      properties.AbortTO  = 20;
      properties.RetakeTO = 20;

      if (short.TryParse(node.CfProperties.ValueOrDefault(nameof(properties.Type), "0"), out var s))
      {
        properties.Type = s;
      }

      properties.BindAddr = node.CfProperties.ValueOrDefault(nameof(properties.BindAddr), "");
      properties.Addr     = node.CfProperties.ValueOrDefault(nameof(properties.Addr),     "");

      if (short.TryParse(node.CfProperties.ValueOrDefault(nameof(properties.Port), "0"), out s))
      {
        properties.Port = s;
      }

      if (short.TryParse(node.CfProperties.ValueOrDefault(nameof(properties.BPort), "0"), out s))
      {
        properties.BPort = s;
      }

      if (short.TryParse(node.CfProperties.ValueOrDefault(nameof(properties.AbortTO), ""), out s))
      {
        properties.AbortTO = s;
      }

      if (short.TryParse(node.CfProperties.ValueOrDefault(nameof(properties.RetakeTO), ""), out s))
      {
        properties.RetakeTO = s;
      }

      properties.CopyConfig   = node.CfProperties.ValueOrDefault(nameof(properties.CopyConfig),   "0").Equals("1");
      properties.StopInactive = node.CfProperties.ValueOrDefault(nameof(properties.StopInactive), "1").Equals("1");
    }


    private async Task FillRbsNodeProperties(RbsNodeProperties properties)
    {
      //< Parameters RBF_Directory = "xx" />
      //< ClientParms DOC_Path = "xx" JournalSQLCS = "xx" DTMX_SQLCS = "xx" />
      //< PGParms BinPath = "xx" DataPath = "xx" />
      try
      {
        var (rbsHandle, _) = await OpenConfigurationTree(Path.Combine(RbsDirectory, properties.PipeName, RbsConfFile))
                               .ConfigureAwait(false);

        var rbsTree = await GetCfTree(rbsHandle).ConfigureAwait(false);

        FreeConfigurationTreeHandle(rbsHandle);

        if (rbsTree != null)
        {
          foreach (var item in rbsTree)
          {
            switch (item.Name)
            {
              case MSTreeConsts.RBS_Parameters:
                properties.RBF_Directory = item.CfProperties.ValueOrDefault(nameof(properties.RBF_Directory), "");
                break;
              case MSTreeConsts.RBS_ClientParms:
                properties.DOC_Path     = item.CfProperties.ValueOrDefault(nameof(properties.DOC_Path),     "");
                properties.JournalSQLCS = item.CfProperties.ValueOrDefault(nameof(properties.JournalSQLCS), "");
                properties.DTMX_SQLCS   = item.CfProperties.ValueOrDefault(nameof(properties.DTMX_SQLCS),   "");
                break;
              case MSTreeConsts.RBS_PGParms:
              {
                properties.BinPath  = item.CfProperties.ValueOrDefault(nameof(properties.BinPath),  "");
                properties.DataPath = item.CfProperties.ValueOrDefault(nameof(properties.DataPath), "");
                break;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      //  параметры редиректора
      properties.RedirectorPort = (short)await GetRedirectorPort(properties.PipeName, 0).ConfigureAwait(false);
    }


    private async Task FillTmCalcProperties(TmCalcNodeProperties properties, string pipeName)
    {
      // читаем конфигурацию дорасчётчика
      try
      {
        var (calcHandle, _) = await OpenConfigurationTree(Path.Combine(TmsDirectory, pipeName, TmCalcConfFile))
                                .ConfigureAwait(false);

        var calcTree = await GetCfTree(calcHandle).ConfigureAwait(false);

        FreeConfigurationTreeHandle(calcHandle);

        if (calcTree != null)
        {
          foreach (var item in calcTree.Where(item => item.CfProperties != null))
          {
            switch (item.Name)
            {
              case MSTreeConsts.Tmcalc_FUnr:
                properties.FUnr = item.CfProperties.ValueOrDefault(MSTreeConsts.Tmcalc_Value, "-")
                                 .StartsWith("+");
                break;
              case MSTreeConsts.Tmcalc_SRel:
                properties.SRel = item.CfProperties.ValueOrDefault(MSTreeConsts.Tmcalc_Value, "-")
                                 .StartsWith("+");
                break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    
    public async Task SaveFullMSTree(MSTreeNode msRoot)
    {
      // Нормализуем имена Pipe для дочерних компонент под серверами, убираем дублирование если есть
      List<string> known_pipes = new List<string>();
      foreach (var server in msRoot.Children)
      {
        if (server.Properties is ChildNodeProperties p)
        {
          if (p.PipeName.Trim().IsNullOrEmpty())
          {
            switch (server.ProgName)
            {
              case MSTreeConsts.RBaseServer:
              case MSTreeConsts.rbsrv_old:
                p.PipeName = "RBS";
                break;
              case MSTreeConsts.TmServer:
              case MSTreeConsts.pcsrv_old:
                p.PipeName = "TMS";
                break;
            }
          }

          int i = 1;
          while (known_pipes.Contains(p.PipeName))
          {
            p.PipeName = $"{p.PipeName}{i}";
            i++;
          }

          known_pipes.Add(p.PipeName);
          if (server.Children != null)
          {
            foreach (var child in server.Children)
            {
              if (child.Properties is ChildNodeProperties ch_p)
              {
                ch_p.PipeName = p.PipeName;
              }
            }
          }
        }
      }

      // Основное дерево мастер-сервиса
      var tree_handle = CreateNewMasterServiceTree(msRoot);
      await SaveMasterServiceConfiguration(tree_handle).ConfigureAwait(false);
      FreeMasterServiceConfigurationHandle(tree_handle);

      // Конфигурация резервирования, перебираем сервера на втором уровне
      var res_handle = _native.CftNodeNewTree();
      foreach (var server in msRoot.Children)
      {
        if (server.Properties is ReservedNodeProperties p)
        {
          string tag = p.PipeName;
          if (server.ProgName.Equals(MSTreeConsts.rbsrv_old) || server.ProgName.Equals(MSTreeConsts.pcsrv_old))
            tag = $"{MSTreeConsts.gensrv}:{tag}";
          else
            tag = $"{server.ProgName}:{tag}";

          var nodeHandle = _native.CftNodeInsertDown(res_handle, EncodingUtil.Utf8ToWin1251Bytes(tag));
          SetNodeProperty(nodeHandle, nameof(p.Type),         p.Type.ToString());
          SetNodeProperty(nodeHandle, nameof(p.BindAddr),     p.BindAddr.Trim());
          SetNodeProperty(nodeHandle, nameof(p.Addr),         p.Addr.Trim());
          SetNodeProperty(nodeHandle, nameof(p.Port),         p.Port.ToString());
          SetNodeProperty(nodeHandle, nameof(p.BPort),        p.BPort.ToString());
          SetNodeProperty(nodeHandle, nameof(p.AbortTO),      p.AbortTO.ToString());
          SetNodeProperty(nodeHandle, nameof(p.RetakeTO),     p.RetakeTO.ToString());
          SetNodeProperty(nodeHandle, nameof(p.CopyConfig),   p.CopyConfig ? "1" : "0");
          SetNodeProperty(nodeHandle, nameof(p.StopInactive), p.StopInactive ? "1" : "0");
        }
      }

      await SaveConfigurationTree(res_handle, HotStanbyConfFile).ConfigureAwait(false);
      FreeConfigurationTreeHandle(res_handle);

      // Другие конфигурации (rb, tmcalc)
      foreach (var server in msRoot.Children)
      {
        if (server.Properties is RbsNodeProperties rbs_p)
        {
          // общие параметры
          var rbs_handle = _native.CftNodeNewTree();
          IntPtr nodeHandle =
            _native.CftNodeInsertDown(rbs_handle, EncodingUtil.Utf8ToWin1251Bytes(MSTreeConsts.RBS_Parameters));
          SetNodeProperty(nodeHandle, nameof(rbs_p.RBF_Directory), rbs_p.RBF_Directory);

          nodeHandle =
            _native.CftNodeInsertDown(rbs_handle, EncodingUtil.Utf8ToWin1251Bytes(MSTreeConsts.RBS_ClientParms));
          SetNodeProperty(nodeHandle, nameof(rbs_p.DOC_Path),     rbs_p.DOC_Path);
          SetNodeProperty(nodeHandle, nameof(rbs_p.DTMX_SQLCS),   rbs_p.DTMX_SQLCS);
          SetNodeProperty(nodeHandle, nameof(rbs_p.JournalSQLCS), rbs_p.JournalSQLCS);

          nodeHandle = _native.CftNodeInsertDown(rbs_handle, EncodingUtil.Utf8ToWin1251Bytes(MSTreeConsts.RBS_PGParms));
          SetNodeProperty(nodeHandle, nameof(rbs_p.BinPath),  rbs_p.BinPath);
          SetNodeProperty(nodeHandle, nameof(rbs_p.DataPath), rbs_p.DataPath);
          await SaveConfigurationTree(rbs_handle,
                                      $"{TmNativeDefs.RbsDirectory}\\{rbs_p.PipeName}\\{TmNativeDefs.RbsConfFile}")
            .ConfigureAwait(false);
          FreeConfigurationTreeHandle(rbs_handle);

          // параметры редиректора
          await SetRedirectorPort(rbs_p.PipeName, 0, (int)rbs_p.RedirectorPort).ConfigureAwait(false);
        }
        else if (server.ProgName.Equals(MSTreeConsts.TmServer) || server.ProgName.Equals(MSTreeConsts.pcsrv_old))
        {
          if ((server.Children != null) && (server.Properties is ChildNodeProperties tms_p))
          {
            foreach (var child in server.Children)
            {
              if (child.Properties is TmCalcNodeProperties calc_p)
              {
                // читаем конфигурацию дорасчётчика если есть
                try
                {
                  string fileName = $"{TmNativeDefs.TmsDirectory}\\{tms_p.PipeName}\\{TmNativeDefs.TmCalcConfFile}";
                  var (calc_handle, _) = await OpenConfigurationTree(fileName).ConfigureAwait(false);
                  var calc_tree = await GetCfTree(calc_handle).ConfigureAwait(false);
                  FreeConfigurationTreeHandle(calc_handle);
                  if (calc_tree == null) calc_tree = new List<CfTreeNode>();

                  var FUnr = calc_tree.Find(n => n.Name.Equals(MSTreeConsts.Tmcalc_FUnr));
                  if (FUnr == null)
                  {
                    FUnr = new CfTreeNode(MSTreeConsts.Tmcalc_FUnr);
                    calc_tree.Add(FUnr);
                  }

                  FUnr.CfProperties = new Dictionary<string, string>
                  {
                    [MSTreeConsts.Tmcalc_Value] = calc_p.FUnr ? "+" : "-"
                  };

                  var SRel = calc_tree.Find(n => n.Name.Equals(MSTreeConsts.Tmcalc_SRel));
                  if (SRel == null)
                  {
                    SRel = new CfTreeNode(MSTreeConsts.Tmcalc_SRel);
                    calc_tree.Add(SRel);
                  }

                  SRel.CfProperties = new Dictionary<string, string>
                  {
                    [MSTreeConsts.Tmcalc_Value] = calc_p.SRel ? "+" : "-"
                  };

                  calc_handle = CreateConfigurationTree(calc_tree);
                  if (calc_handle != IntPtr.Zero)
                  {
                    await SaveConfigurationTree(calc_handle, fileName).ConfigureAwait(false);
                    FreeConfigurationTreeHandle(calc_handle);
                  }
                }
                catch
                {
                }

                break;
              }
            }
          }
        }
      }
    }

    
    public async Task SaveConfigurationTree(IntPtr treeHandle, string filename)
    {
      var       fileTime        = new TmNativeDefs.FileTime();
      const int errStringLength = 1000;
      var       errBuf          = new byte[errStringLength];
      uint      errCode         = 0;

      var res = await Task.Run(() => _native.CfsConfFileSaveAs(treeHandle,
                                                               EncodingUtil.Utf8ToWin1251Bytes(Host),
                                                               EncodingUtil.Utf8ToWin1251Bytes(filename),
                                                               30000 | TmNativeDefs.FailIfNoConnect,
                                                               ref fileTime,
                                                               out errCode,
                                                               ref errBuf,
                                                               errStringLength))
                          .ConfigureAwait(false);
      if (errCode != 0)
        throw new Exception($"Ошибка записи конфигурации: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
    }

    public async Task SaveMasterServiceConfiguration(IntPtr treeHandle)
    {
      await SaveConfigurationTree(treeHandle, MasterConfFile).ConfigureAwait(false);
    }

    public void FreeMasterServiceConfigurationHandle(IntPtr handle)
    {
      _native.CftNodeFreeTree(handle);
    }

    public void FreeConfigurationTreeHandle(IntPtr handle)
    {
      _native.CftNodeFreeTree(handle);
    }

    private static DateTime GetDateTimeFromCustomFileTime(TmNativeDefs.FileTime fileTime)
    {
      return DateTime.FromFileTime((long)fileTime.dwHighDateTime << 32 | (uint)fileTime.dwLowDateTime);
    }

    public async Task<List<CfTreeNode>> GetCfTree(IntPtr rootHandle, CfTreeNode parent = null)
    {
      return await Task.Run(() => GetNodeChildren(rootHandle, parent)).ConfigureAwait(false);
    }

    private List<CfTreeNode> GetNodeChildren(IntPtr parentHandle, CfTreeNode parent = null)
    {
      var children = new List<CfTreeNode>();

      for (var i = 0;; i++)
      {
        var childHandle = _native.CftNodeEnumAll(parentHandle, i);
        if (childHandle == IntPtr.Zero)
          break;
        var nodeChild = new CfTreeNode(GetNodeName(childHandle), parent)
        {
          Disabled     = !_native.CftNodeIsEnabled(childHandle),
          CfProperties = GetNodeProps(childHandle),
        };
        nodeChild.Children = GetNodeChildren(childHandle, nodeChild);
        children.Add(nodeChild);
      }

      if (children.Count == 0)
        return null;
      else
        return children;
    }

    private string GetNodeName(IntPtr nodeHandle)
    {
      const int nameBufLength = 200;
      var       nameBuf       = new byte[nameBufLength];

      _native.CftNodeGetName(nodeHandle, nameBuf, nameBufLength);

      return EncodingUtil.Win1251BytesToUtf8(nameBuf);
    }

    private Dictionary<string, string> GetNodeProps(IntPtr nodeHandle)
    {
      var props = new Dictionary<string, string>();

      for (var i = 0;; i++)
      {
        var propName = GetPropName(nodeHandle, i);
        if (propName == "")
          break;

        var propValue = GetPropValue(nodeHandle, propName);

        props.Add(propName, propValue);
      }

      return props;
    }

    private string GetPropName(IntPtr nodeHandle, int idx)
    {
      const int nameBufLength = 200;
      var       nameBuf       = new byte[nameBufLength];

      _native.CftNPropEnum(nodeHandle, idx, nameBuf, nameBufLength);

      return EncodingUtil.Win1251BytesToUtf8(nameBuf);
    }

    private string GetPropValue(IntPtr nodeHandle, string propName)
    {
      IntPtr ptr = _native.CftNPropGetText(nodeHandle, EncodingUtil.Utf8ToWin1251Bytes(propName), null, 0);
      if (ptr != IntPtr.Zero)
      {
        string ret = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(ptr);
        _native.CfsFreeMemory(ptr);
        return ret;
      }
      else
      {
        return string.Empty;
      }
    }

    private IntPtr CreateNewMasterServiceTree(MSTreeNode msRoot)
    {
      var newTreeHandle = _native.CftNodeNewTree();

      CreateMSNode(newTreeHandle, msRoot);

      return newTreeHandle;
    }

    public IntPtr CreateConfigurationTree(IEnumerable<CfTreeNode> tree)
    {
      var newTreeHandle = _native.CftNodeNewTree();
      foreach (var node in tree)
      {
        CreateCfgNode(newTreeHandle, node);
      }

      return newTreeHandle;
    }

    private void CreateCfgNode(IntPtr parentNodeHandle, CfTreeNode node)
    {
      var nodeHandle = _native.CftNodeInsertDown(parentNodeHandle, EncodingUtil.Utf8ToWin1251Bytes(node.Name));
      _native.CftNodeEnable(nodeHandle, !node.Disabled);
      if ((node.CfProperties != null) && node.CfProperties.Any())
      {
        foreach (var prop in node.CfProperties)
        {
          SetNodeProperty(nodeHandle, prop.Key, prop.Value);
        }
      }

      if ((node.Children != null) && node.Children.Any())
      {
        foreach (var child in node.Children)
        {
          CreateCfgNode(nodeHandle, child);
        }
      }
    }

    private void CreateMSNode(IntPtr parentNodeHandle, MSTreeNode node, int tagId = -1)
    {
      var tag        = tagId == -1 ? "Master" : $"#{tagId:X3}";
      var nodeHandle = _native.CftNodeInsertDown(parentNodeHandle, EncodingUtil.Utf8ToWin1251Bytes(tag));

      if (!CreateMSNodeProperties(nodeHandle, node))
        throw new Exception("Ошибка заполнения дерева конфигурации");

      if (node.Children != null)
      {
        var i = 0;
        foreach (var childNode in node.Children)
        {
          CreateMSNode(nodeHandle, childNode as MSTreeNode, i);
          i++;
        }
      }
    }

    private bool CreateMSNodeProperties(IntPtr nodeHandle, MSTreeNode node)
    {
      if (node.ProgName.Equals(MSTreeConsts.rbsrv_old) || node.ProgName.Equals(MSTreeConsts.pcsrv_old))
      {
        if (!SetNodeProperty(nodeHandle, MSTreeConsts.ProgName, MSTreeConsts.gensrv))
          return false;

        if (!SetNodeProperty(nodeHandle, MSTreeConsts.TaskPath, node.ProgName))
          return false;
      }
      else
      {
        if (!SetNodeProperty(nodeHandle, MSTreeConsts.ProgName, node.ProgName))
          return false;
      }

      if (node.Properties is MSTreeNodeProperties props && props.NoStart)
      {
        if (!SetNodeProperty(nodeHandle, MSTreeConsts.NoStart, "1")) return false;
      }

      switch (node.Properties)
      {
        case MasterNodeProperties _:
          if (!CreateMasterNodeProperties(nodeHandle, node))
          {
            return false;
          }
          
          break;
        case NewTmsNodeProperties _:
          if (!CreateNewTmsNodeProperties(nodeHandle, node))
          {
            return false;
          }
          
          break;
        case ExternalTaskNodeProperties _:
          if (!CreateExternalTaskNodeProperties(nodeHandle, node))
          {
            return false;
          }
          break;
        case AutoBackupProperties autoBackupProperties:
        {
          if (!CreateAutoBackupNodeProperties(nodeHandle, autoBackupProperties))
          {
            return false;
          } 
          
          break;
        }
        default:
          if (!CreateChildNodeProperties(nodeHandle, node))
            return false;
          break;
      }

      return true;
    }

    private bool CreateMasterNodeProperties(IntPtr nodeHandle, MSTreeNode node)
    {
      var props = (MasterNodeProperties)node.Properties;

      if (!SetNodeProperty(nodeHandle, MSTreeConsts.LogFileSize, props.LogFileSize.ToString()))
      {
        return false;
      }

      return !node.ProgName.Equals(MSTreeConsts.Portcore) || SetNodeProperty(nodeHandle, MSTreeConsts.InstallationName, props.InstallationName);
    }

    private bool CreateChildNodeProperties(IntPtr nodeHandle, MSTreeNode node)
    {
      var props = (ChildNodeProperties)node.Properties;

      return SetNodeProperty(nodeHandle, MSTreeConsts.PipeName, props.PipeName);
    }

    private bool CreateNewTmsNodeProperties(IntPtr nodeHandle, MSTreeNode node)
    {
      var props = (NewTmsNodeProperties)node.Properties;

      if (!CreateChildNodeProperties(nodeHandle, node))
      {
        return false;
      }
      
      return props.PassiveMode || SetNodeProperty(nodeHandle, MSTreeConsts.PassiveMode, Convert.ToInt32(props.PassiveMode).ToString());
    }

    private bool CreateExternalTaskNodeProperties(IntPtr nodeHandle, MSTreeNode node)
    {
      var props = (ExternalTaskNodeProperties)node.Properties;

      if (!CreateChildNodeProperties(nodeHandle, node))
      {
        return false;
      }

      // Зачем то в пути внешней задачи пробелы замеяются на табуляции
      if (!SetNodeProperty(nodeHandle, MSTreeConsts.TaskPath, props.TaskPath.Replace(' ', '\t')))
      {
        return false;
      }

      if (!SetNodeProperty(nodeHandle, MSTreeConsts.TaskArguments, props.TaskArguments))
      {
        return false;
      }

      if (!SetNodeProperty(nodeHandle, MSTreeConsts.ConfFilePath, props.ConfigurationFilePath))
      {
        return false;
      }

      return true;
    }

    private bool CreateAutoBackupNodeProperties(IntPtr nodeHandle, 
                                                AutoBackupProperties properties)
    {
      if (!SetNodeProperty(nodeHandle, MSTreeConsts.ExecutionHour, $"{properties.ExecutionHour}"))
      {
        return false;
      }
      
      if (!SetNodeProperty(nodeHandle, MSTreeConsts.BackupDirectory, properties.BackupsDirectory))
      {
        return false;
      }
      
      if (!SetNodeProperty(nodeHandle, MSTreeConsts.ExcludeArchives, properties.ExcludeArchives ? "1" : "0"))
      {
        return false;
      }

      return true;
    }

    private bool SetNodeProperty(IntPtr nodeHandle, string propName, string propText)
    {
      var arr_propText = EncodingUtil.Utf8ToWin1251Bytes(propText);
      return _native.CftNPropSet(nodeHandle, EncodingUtil.Utf8ToWin1251Bytes(propName), arr_propText);
    }

    public async Task<CfsDefs.SoftwareTypes> GetSoftwareType()
    {
      var result = await Task.Run(() => _native.CfsGetSoftwareType(CfId))
                             .ConfigureAwait(false);

      switch (result)
      {
        case 48:
          return CfsDefs.SoftwareTypes.Old;
        case 49:
          return CfsDefs.SoftwareTypes.Version3;
        default:
          return CfsDefs.SoftwareTypes.Unknown;
      }
    }

    public async Task<CfsDefs.MasterServiceStatus> MasterServiceStatus()
    {
      var result = await Task.Run(() => _native.CfsIfpcMaster(CfId, TmNativeDefs.MasterServiceStatusCommand))
                             .ConfigureAwait(false);

      switch (result)
      {
        case 1:
          return CfsDefs.MasterServiceStatus.Stopped;
        case 2:
          return CfsDefs.MasterServiceStatus.Running;
        default:
          return CfsDefs.MasterServiceStatus.LostConnection;
      }
    }

    public async Task StartMasterService()
    {
      await Task.Run(() => _native.CfsIfpcMaster(CfId, TmNativeDefs.StartMasterServiceCommand))
                .ConfigureAwait(false);
    }

    public async Task StopMasterService()
    {
      await Task.Run(() => _native.CfsIfpcMaster(CfId, TmNativeDefs.StopMasterServiceCommand))
                .ConfigureAwait(false);
    }

    public async Task<bool> IsConnected()
    {
      return await Task.Run(() => _native.CfsIsConnected(CfId))
                       .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<string>> GetTimezones()
    {
      const int errStringLength = 1000;
      var       errBuf          = new byte[errStringLength];
      uint      errCode         = 0;

      var timezonesIdsPointer = await Task.Run(() => _native.CfsEnumTimezones(CfId,
                                                                              out errCode,
                                                                              ref errBuf,
                                                                              errStringLength))
                                          .ConfigureAwait(false);

      var timezonesIds = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(timezonesIdsPointer, 1000);

      _native.CfsFreeMemory(timezonesIdsPointer);
      return timezonesIds;
    }

    public async Task<IReadOnlyCollection<TmServer>> GetTmServersTree()
    {
      var lookup    = new Dictionary<uint, TmServer>();
      var tmServers = await GetTmServers().ConfigureAwait(false);

      // Иногда случаются дубли, лучше сделать который не даст исключений
      foreach (var server in tmServers)
      {
        lookup[server.ProcessId] = server;
      }

      foreach (var tmServer in tmServers)
      {
        if (!lookup.TryGetValue(tmServer.ParentProcessId, out var proposedParent)) continue;
        tmServer.Parent = proposedParent;
        proposedParent.Children.Add(tmServer);
      }

      return lookup.Values.Where(x => x.Parent == null).ToList();
    }

    public async Task<IReadOnlyCollection<TmServer>> GetTmServers()
    {
      var serverIdsList = await GetIfaceServerId().ConfigureAwait(false);
      var tmUsers = await GetTmUsers()
                      .ConfigureAwait(false);

      var tmServersList = new List<TmServer>();

      foreach (var serverId in serverIdsList)
      {
        var tmServer = TmServer.CreateFromIfaceServer(await GetIfaceServerData(serverId)
                                                        .ConfigureAwait(false));
        var serverUsers = tmUsers.Where(x => x.ProcessId == tmServer.ProcessId);
        tmServer.Users.AddRange(serverUsers);

        tmServersList.Add(tmServer);
      }

      return tmServersList;
    }

    public async Task<IReadOnlyCollection<string>> GetIfaceServerId()
    {
      const int errStringLength = 1000;
      var       errBuf          = new byte[errStringLength];
      uint      errCode         = 0;

      var serversIdsPointer = await Task.Run(() => _native.CfsTraceEnumServers(CfId,
                                                                               out errCode,
                                                                               ref errBuf,
                                                                               errStringLength))
                                        .ConfigureAwait(false);


      var serversIds = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(serversIdsPointer, 1000);

      _native.CfsFreeMemory(serversIdsPointer);
      return serversIds;
    }

    public async Task<TmNativeDefs.IfaceServer> GetIfaceServerData(string serverId)
    {
      const int errStringLength = 1000;
      var       errBuf          = new byte[errStringLength];
      uint      errCode         = 0;
      var       ifaceServer     = new TmNativeDefs.IfaceServer();

      await Task.Run(() => _native.CfsTraceGetServerData(CfId,
                                                         EncodingUtil.Utf8ToWin1251Bytes(serverId),
                                                         ref ifaceServer,
                                                         out errCode,
                                                         ref errBuf,
                                                         errStringLength))
                .ConfigureAwait(false);

      return ifaceServer;
    }

    private async Task<IReadOnlyCollection<TmUser>> GetTmUsers()
    {
      var usersIds = await GetIfaceUsersIds().ConfigureAwait(false);
      var tmUsers  = new List<TmUser>();

      foreach (var userId in usersIds)
      {
        var tmUser = TmUser.CreateFromIfaceUser(await GetIfaceUserData(userId).ConfigureAwait(false));
        tmUsers.Add(tmUser);
      }

      return tmUsers;
    }

    private async Task<IReadOnlyCollection<string>> GetIfaceUsersIds()
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var usersIdsPointer = await Task.Run(() => _native.CfsTraceEnumUsers(CfId,
                                                                           out errCode,
                                                                           ref errBuf,
                                                                           errCode))
                                      .ConfigureAwait(false);

      var usersIds = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(usersIdsPointer, 1000);

      _native.CfsFreeMemory(usersIdsPointer);
      return usersIds;
    }

    private async Task<TmNativeDefs.IfaceUser> GetIfaceUserData(string userId)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      var       ifaceUser    = new TmNativeDefs.IfaceUser();

      await Task.Run(() => _native.CfsTraceGetUserData(CfId,
                                                       EncodingUtil.Utf8ToWin1251Bytes(userId),
                                                       ref
                                                       ifaceUser,
                                                       out errCode,
                                                       ref errBuf,
                                                       errBufLength))
                .ConfigureAwait(false);

      return ifaceUser;
    }

    public async Task<IReadOnlyCollection<TmServerLogRecord>> GetTmServersLog()
    {
      await OpenTmServerLog().ConfigureAwait(false);
      var tmServersLog = new List<TmServerLogRecord>();

      var firstRecord = true;

      while (true)
      {
        var logRecord = await GetTmServersLogRecord(firstRecord)
                          .ConfigureAwait(false);
        firstRecord = false;

        if (logRecord == null) break;

        tmServersLog.Add(logRecord);
      }

      await CloseTmServerLog().ConfigureAwait(false);

      return tmServersLog;
    }

    public async Task<IReadOnlyCollection<TmServerLogRecord>> GetTmServersLog(
      int maxRecords, DateTime? startTime, DateTime? endTime)
    {
      await OpenTmServerLog().ConfigureAwait(false);
      var tmServersLog = new List<TmServerLogRecord>();

      var firstRecord = true;

      while (true)
      {
        var logRecord = await GetTmServersLogRecord(firstRecord)
                          .ConfigureAwait(false);
        firstRecord = false;

        if (logRecord == null) break;
        if (endTime != null)
        {
          if (logRecord.DateTime > endTime)
            continue;
        }

        if (startTime != null)
        {
          if (logRecord.DateTime < startTime)
            break;
        }

        tmServersLog.Add(logRecord);
        if ((maxRecords > 0) && (tmServersLog.Count >= maxRecords))
          break;
      }

      await CloseTmServerLog().ConfigureAwait(false);

      return tmServersLog;
    }

    public async Task<IReadOnlyCollection<TmServerThread>> GetTmServersThreads()
    {
      const int errBufLength = 1000;
      const int bufSize      = 8192;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var threadPtr =
        await Task.Run(() => _native.CfsEnumThreads(CfId, out errCode, ref errBuf, errCode))
                  .ConfigureAwait(false);

      if (threadPtr == IntPtr.Zero)
      {
        throw new
          Exception($"Ошибка получения потоков сервера: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
      }

      var threadsStringLists =
        TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(threadPtr, bufSize);

      _native.CfsFreeMemory(threadPtr);

      var tmServerThreadsList = new List<TmServerThread>();

      foreach (var threadString in threadsStringLists)
      {
        // 13.03.2023 переделал на Split для совместимости с серверами 2.x
        //var regex = new Regex(@"([0-9]*),(.*?) • ([-+]?[0-9]*) s • ([-+]?[0-9]*\.?[0-9]+) s");
        //var mc = regex.Match(threadString);
        //var id = int.Parse(mc.Groups[1].Value);
        //var name = mc.Groups[2].Value;
        //var upTime = int.Parse(mc.Groups[3].Value);
        //var workTime = float.Parse(mc.Groups[4].Value, CultureInfo.InvariantCulture);
        var tokens = threadString.Split(',');
        if (tokens.Length > 1)
        {
          int.TryParse(tokens[0], out int id);
          var tokens2 = tokens[1].Split('•');
          if (tokens2.Length > 2)
          {
            var name = tokens2[0].Trim();
            int.TryParse(tokens2[1].Replace('s', ' ').Trim(), NumberStyles.Any, CultureInfo.InvariantCulture,
                         out int upTime);
            float.TryParse(tokens2[2].Replace('s', ' ').Trim(), NumberStyles.Any, CultureInfo.InvariantCulture,
                           out float workTime);
            tmServerThreadsList.Add(new TmServerThread(id, name.Trim(), upTime, workTime));
          }
        }
      }

      return tmServerThreadsList;
    }

    public async Task RegisterTmServerTracer(ITmServerTraceable traceTarget, bool debug, int pause)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = await Task.Run(() => _native.CfsTraceBeginTraceEx(CfId,
                                                                     traceTarget.ProcessId,
                                                                     traceTarget.ThreadId, debug,
                                                                     (uint)pause,
                                                                     out errCode,
                                                                     ref errBuf,
                                                                     errBufLength))
                             .ConfigureAwait(false);
    }

    public async Task StopTmServerTrace()
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = await Task.Run(() => _native.CfsTraceEndTrace(CfId, out errCode, ref errBuf, errBufLength))
                             .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<TmServerLogRecord>> TraceTmServerLogRecords()
    {
      const int          errBufLength = 1000;
      var                errBuf       = new byte[errBufLength];
      uint               errCode      = 0;
      List<CfsLogRecord> records      = new List<CfsLogRecord>();

      while (true)
      {
        var logRecordPtr = await Task.Run(() => _native.CfsTraceGetMessage(CfId, out errCode,
                                                                           ref errBuf,
                                                                           errBufLength))
                                     .ConfigureAwait(false);

        if (logRecordPtr == IntPtr.Zero) break;

        if (errCode != 0)
        {
          throw new
            Exception($"Ошибка трассировки: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode} CfId:{CfId}");
        }

        var tmpLogRecords = ParseCfsServerLogRecordPointer(logRecordPtr);
        _native.CfsFreeMemory(logRecordPtr);
        records.AddRange(tmpLogRecords);
      }

      return records.Select(TmServerLogRecord.CreateFromCfsLogRecord).ToList();
    }

    private async Task OpenTmServerLog()
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = await Task.Run(() => _native.CfsLogOpen(CfId,
                                                           out errCode,
                                                           ref errBuf,
                                                           errBufLength))
                             .ConfigureAwait(false);
      if (!result)
      {
        throw new
          Exception($"Ошибка получения журнала сервера: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
      }
    }

    private async Task CloseTmServerLog()
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = await Task.Run(() => _native.CfsLogClose(CfId,
                                                            out errCode,
                                                            ref errBuf,
                                                            errBufLength))
                             .ConfigureAwait(false);
      if (!result)
      {
        throw new
          Exception($"Ошибка получения журнала сервера: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
      }
    }

    private async Task<TmServerLogRecord> GetTmServersLogRecord(bool isFirst = false)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var logRecordPtr =
        await Task.Run(() => _native.CfsLogGetRecord(CfId, isFirst, out errCode, ref errBuf, errCode))
                  .ConfigureAwait(false);

      if (logRecordPtr == IntPtr.Zero) return null;

      var cfsLogRecord = ParseCfsServerLogRecordPointer(logRecordPtr).FirstOrDefault();

      _native.CfsFreeMemory(logRecordPtr);

      return TmServerLogRecord.CreateFromCfsLogRecord(cfsLogRecord);
    }

    private IReadOnlyCollection<TmNativeDefs.CfsLogRecord> ParseCfsServerLogRecordPointer(IntPtr ptr)
    {
      var strList = TmNativeUtil.GetUnknownLengthStringListFromDoubleNullTerminatedPointer(ptr);

      var records = new List<CfsLogRecord>();

      foreach (var str in strList)
      {
        var mc = _cfsServerLogRecordRegex.Match(str);

        if (!mc.Success)
        {
          continue;
        }

        records.Add(new TmNativeDefs.CfsLogRecord
        {
          Time     = mc.Groups[1].Value,
          Date     = mc.Groups[2].Value,
          Name     = mc.Groups[3].Value,
          Type     = mc.Groups[4].Value,
          MsgType  = mc.Groups[5].Value.Trim(' '),
          ThreadId = mc.Groups[6].Value,
          Message  = mc.Groups[7].Value,
        });
      }

      return records;
    }

    public async Task<TmInstallationInfo> GetTmInstallationInfo()
    {
      var (isIntact, integrityCheckMessage) =
        await CheckInstallationIntegrity(TmNativeDefs.CfsIitgk.Exe).ConfigureAwait(false);

      return new TmInstallationInfo(await GetInstallationInfoString("ProductName").ConfigureAwait(false),
                                    await GetInstallationInfoString("Version").ConfigureAwait(false),
                                    await GetInstallationInfoString("BuildTime").ConfigureAwait(false),
                                    await GetInstallationInfoString("InstTime").ConfigureAwait(false),
                                    await GetInstallationInfoString("UpdateLim").ConfigureAwait(false),
                                    isIntact,
                                    integrityCheckMessage,
                                    await GetTmInstallationFilesInfo().ConfigureAwait(false));
    }

    private async Task<IReadOnlyCollection<TmInstallationFileInfo>> GetTmInstallationFilesInfo()
    {
      var tempDispServIni = Path.GetTempFileName();
      if (!await GetDispServIniFile(tempDispServIni).ConfigureAwait(false))
      {
        throw new Exception($"Ошибка получения информации о установленных файлах");
      }

      var result = new List<TmInstallationFileInfo>();
      using (var iniManager = new IniManager(tempDispServIni))
      {
        foreach (var file in iniManager.GetPrivateSection("Files"))
        {
          var fileSection = iniManager.GetPrivateSection(file.Value);

          var dir = fileSection.TryGetValue("Dir", out var dirStr) ? dirStr : "";

          var fileProps = await GetFileProperties(Path.Combine($"@{dir}", file.Value)).ConfigureAwait(false);

          result.Add(new TmInstallationFileInfo(file.Value,
                                                fileSection.TryGetValue("Desc", out var descStr)
                                                  ? descStr
                                                  : string.Empty,
                                                dir,
                                                fileSection.TryGetValue("Chks", out var checkSumStr)
                                                  ? checkSumStr
                                                  : string.Empty,
                                                fileProps?.Checksum ?? 0,
                                                fileSection.TryGetValue("Time", out var timeStr)
                                                  ? timeStr
                                                  : string.Empty,
                                                fileProps.HasValue
                                                  ? GetDateTimeFromCustomFileTime(fileProps.Value.ModificationTime)
                                                  : (DateTime?)null));
        }
      }

      File.Delete(tempDispServIni);

      return result;
    }

    private async Task<bool> GetDispServIniFile(string localPath)
    {
      bool result;
      (result, _, _) = await GetFile(localPath, "@dispserv.ini").ConfigureAwait(false);
      return result;
    }

    private async Task<string> GetInstallationInfoString(string key)
    {
      const string path    = "@@";
      const string section = "IInfo";

      return await GetIniString(path, section, key).ConfigureAwait(false);
    }

    private async Task<TmNativeDefs.CfsFileProperties?> GetFileProperties(string filePath)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      var       fileProps    = new TmNativeDefs.CfsFileProperties();

      var result =
        await Task.Run(() => _native.CfsFileGetPropreties(CfId,
                                                          EncodingUtil.Utf8ToWin1251Bytes(filePath),
                                                          ref fileProps,
                                                          out errCode,
                                                          ref errBuf,
                                                          errBufLength))
                  .ConfigureAwait(false);


      if (!result) return null;

      return fileProps;
    }

    public async Task<(bool, string)> CheckInstallationIntegrity(TmNativeDefs.CfsIitgk kind)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var signaturePointer = new IntPtr();
      var errorsPointer    = new IntPtr();

      var result =
        await Task.Run(() => _native.CfsCheckInstallationIntegrity(CfId,
                                                                   (uint)kind,
                                                                   out signaturePointer,
                                                                   out errorsPointer,
                                                                   out errCode,
                                                                   ref errBuf,
                                                                   errBufLength))
                  .ConfigureAwait(false);

      if (!result)
      {
        throw new
          Exception($"Ошибка проверки целостности сервера: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
      }

      var signature = $"Корневая сигнатура:{TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(signaturePointer)}";
      var errors    = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(errorsPointer);
      _native.CfsFreeMemory(signaturePointer);
      _native.CfsFreeMemory(errorsPointer);

      return errors.IsNullOrEmpty() ? (true, signature) : (false, errors);
    }

    public async Task<TmLicenseInfo> GetLicenseInfo()
    {
      const string path    = "@@";
      const string section = "FInfo";
      const uint   bufSize = 1024 * 8;

      var keyDataStrings =
        (await GetIniString(path, section, bufSize: bufSize).ConfigureAwait(false)).Split(new[] { '\n' },
          StringSplitOptions.RemoveEmptyEntries);

      var keyDataDictionary = new Dictionary<string, string>();
      foreach (var keyData in keyDataStrings)
      {
        var    parts      = keyData.Split('=');
        string translated = await GetLicenseKeyDataItemString(keyData).ConfigureAwait(false);
        keyDataDictionary.Add(parts.First(), translated);
      }

      var currentLicenseKey = new TmLicenseKey(await GetCurrentLicenseKeyCom().ConfigureAwait(false));

      return new TmLicenseInfo(currentLicenseKey, keyDataDictionary);
    }

    public async Task SetLicenseKeyCom(TmLicenseKey newLicenseKey)
    {
      var path = Path.Combine(await GetBasePath().ConfigureAwait(false),
                              "Data\\Main\\cfshare.ini");
      const string section = "IfaceSecKey";
      const string key     = "COM";

      await SetIniString(path, section, key, newLicenseKey.NativeCom()).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<string>> GetFilesInDirectory(string path)
    {
      const uint bufLength    = 8192;
      const int  errBufLength = 1000;
      var        buf          = new char[bufLength];
      var        errBuf       = new byte[errBufLength];
      uint       errCode      = 0;


      if (!await Task.Run(() => _native.CfsDirEnum(CfId,
                                                   EncodingUtil.Utf8ToWin1251Bytes(path),
                                                   ref buf,
                                                   bufLength,
                                                   out errCode,
                                                   ref errBuf,
                                                   errBufLength))
                     .ConfigureAwait(false))
      {
        Console.WriteLine($"Ошибка при запросе списка файлов: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errBuf)}");
        return null;
      }

      return TmNativeUtil.GetStringListFromDoubleNullTerminatedChars(buf);
    }


    public async Task<IReadOnlyCollection<string>> GetInstalledLicenseKeyFiles()
    {
      var path = $"{await GetBasePath().ConfigureAwait(false)}*.id";

      return await GetFilesInDirectory(path).ConfigureAwait(false);
    }


    public async Task PutLicenseKeyFile(string filePath)
    {
      var remotePath = Path.Combine(await GetBasePath().ConfigureAwait(false),
                                    Path.GetFileName(filePath));

      await PutFile(filePath, remotePath, 20000).ConfigureAwait(false);
    }


    public async Task DeleteLicenseKeyFile(string fileName)
    {
      var remotePath = Path.Combine(await GetBasePath().ConfigureAwait(false), fileName);

      await DeleteFile(remotePath).ConfigureAwait(false);
    }


    public async Task<(bool, string)> PutFile(string localFilePath,
                                              string remoteFilePath,
                                              uint   timeout = 20000)
    {
      if (localFilePath.IsNullOrEmpty())
      {
        return (false, "Ошибка: не указан локальный путь до файла");
      }

      if (remoteFilePath.IsNullOrEmpty())
      {
        return (false, "Ошибка: не указан удалённый путь до файла");
      }

      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      if (!await Task.Run(() => _native.CfsFilePut(CfId,
                                                   EncodingUtil.Utf8ToWin1251Bytes(remoteFilePath),
                                                   EncodingUtil.Utf8ToWin1251Bytes(localFilePath),
                                                   timeout | TmNativeDefs.FailIfNoConnect,
                                                   out errCode,
                                                   ref errBuf, errBufLength)).ConfigureAwait(false))
      {
        return (false, $"Ошибка при отправке файла: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errBuf)}");
      }

      return (true, string.Empty);
    }

    public async Task<(bool, string, DateTime)> GetFile(string localFilePath, string remoteFilePath,
                                                        uint   timeout = 20000)
    {
      if (localFilePath.IsNullOrEmpty())
      {
        return (false, "Ошибка: не указан локальный путь до файла", DateTime.MinValue);
      }

      if (remoteFilePath.IsNullOrEmpty())
      {
        return (false, "Ошибка: не указан удалённый путь до файла", DateTime.MinValue);
      }

      var       fileTime        = new TmNativeDefs.FileTime();
      const int errStringLength = 1000;
      var       errString       = new byte[errStringLength];
      uint      errCode         = 0;
      if (!await Task.Run(() => _native.CfsFileGet(CfId,
                                                   EncodingUtil.Utf8ToWin1251Bytes(remoteFilePath),
                                                   EncodingUtil.Utf8ToWin1251Bytes(localFilePath),
                                                   timeout | TmNativeDefs.FailIfNoConnect,
                                                   ref fileTime,
                                                   out errCode,
                                                   ref errString,
                                                   errStringLength))
                     .ConfigureAwait(false))
      {
        return (false, $"Ошибка при скачивании файла: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errString)}",
                DateTime.MinValue);
      }

      if (!File.Exists(localFilePath))
      {
        Console.WriteLine("Ошибка при сохранении файла в файловую систему");
        return (false, "Ошибка при сохранении файла в файловую систему", DateTime.MinValue);
      }

      return (true, string.Empty, GetDateTimeFromCustomFileTime(fileTime));
    }

    public async Task DeleteFile(string remoteFilePath)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      if (remoteFilePath.IsNullOrEmpty())
      {
        Console.WriteLine("Ошибка: не указан удалённый путь до файла");
        return;
      }

      if (!await Task.Run(() => _native.CfsFileDelete(CfId, EncodingUtil.Utf8ToWin1251Bytes(remoteFilePath),
                                                      out errCode, ref errBuf, errBufLength))
                     .ConfigureAwait(false))
      {
        Console.WriteLine($"Ошибка при удалении файла: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errBuf)}");
      }
    }


    public async Task<IReadOnlyCollection<LicenseKeyType>> GetAvailableLicenseKeyTypes()
    {
      const string path    = "@@";
      const string section = "AppKeyList";

      const uint bufSize         = 1024;
      var        licenseKeyTypes = new List<LicenseKeyType>();

      var typesStrings =
        (await GetIniString(path, section, bufSize: bufSize).ConfigureAwait(false)).Split(new[] { ';' },
          StringSplitOptions.RemoveEmptyEntries);

      foreach (var typeString in typesStrings)
      {
        var typeNumStr = typeString.Split(new[] { ". " }, StringSplitOptions.None).First();

        if (int.TryParse(typeNumStr, out int t))
        {
          licenseKeyTypes.Add((LicenseKeyType)t);
        }
      }

      return licenseKeyTypes;
    }


    public async Task<int> GetCurrentLicenseKeyCom()
    {
      var path = Path.Combine(await GetBasePath().ConfigureAwait(false),
                              "Data\\Main\\cfshare.ini");
      const string section = "IfaceSecKey";
      const string key     = "COM";

      var currentLicenseKeyCom = await GetIniString(path, section, key, bufSize: 1024).ConfigureAwait(false);

      return currentLicenseKeyCom.IsNullOrEmpty() ? 0 : Convert.ToInt32(currentLicenseKeyCom);
    }


    private async Task<string> GetLicenseKeyDataItemString(string rawItemString)
    {
      return await GetIniString("@@", "SStr", bufSize: 1024, def: rawItemString).ConfigureAwait(false);
    }


    public async Task<string> GetBasePath()
    {
      const int basePathBufLength = 1000;
      var       basePathBuf       = new byte[basePathBufLength];

      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = await Task.Run(() => _native.СfsGetBasePath(CfId,
                                                               ref basePathBuf,
                                                               basePathBufLength,
                                                               out errCode,
                                                               ref errBuf,
                                                               errBufLength))
                             .ConfigureAwait(false);

      if (!result)
      {
        throw new
          Exception($"Ошибка получения базового пути сервера сервера: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
      }

      return EncodingUtil.Win1251BytesToUtf8(basePathBuf);
    }

    /* TODO // path = @@
                // section = LinkedServer
                // key = TMS (или др.) */
    private async Task<string> GetIniString(string path,
                                            string section,
                                            string key     = "",
                                            string def     = "",
                                            uint   bufSize = 256)
    {
      var buf = new byte[bufSize];

      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = await Task.Run(() => _native.CfsGetIniString(CfId,
                                                                EncodingUtil.Utf8ToWin1251Bytes(path),
                                                                EncodingUtil.Utf8ToWin1251Bytes(section),
                                                                EncodingUtil.Utf8ToWin1251Bytes(key),
                                                                EncodingUtil.Utf8ToWin1251Bytes(def),
                                                                ref buf,
                                                                out bufSize,
                                                                out errCode,
                                                                ref errBuf,
                                                                errBufLength))
                             .ConfigureAwait(false);

      if (!result)
      {
        throw new
          Exception($"Ошибка получения ini-строки. \nПуть: {path}\nСекция: {section}\nКлюч: {key}\nОшибка: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
      }

      return EncodingUtil.Win1251BytesToUtf8(buf);
    }


    private async Task SetIniString(string path,
                                    string section,
                                    string key,
                                    string value)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = await Task.Run(() => _native.CfsSetIniString(CfId,
                                                                EncodingUtil.Utf8ToWin1251Bytes(path),
                                                                EncodingUtil.Utf8ToWin1251Bytes(section),
                                                                EncodingUtil.Utf8ToWin1251Bytes(key),
                                                                EncodingUtil.Utf8ToWin1251Bytes(value),
                                                                out errCode,
                                                                ref errBuf,
                                                                errBufLength))
                             .ConfigureAwait(false);

      if (!result)
      {
        throw new
          Exception($"Ошибка записи ini-строки. \nПуть: {path}\nСекция: {section}\nКлюч: {key}\nЗначение: {value}\nОшибка: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
      }
    }


    public async Task<IReadOnlyCollection<SLogRecord>> GetSecurityLogFull(
      SLogReadDirection readDirection = SLogReadDirection.FromEnd)
    {
      return await GetSecurityLog(0,
                                  readDirection,
                                  readDirection == SLogReadDirection.FromEnd ? SLogIndex.Last : SLogIndex.First)
               .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<SLogRecord>> GetAdministratorLogFull(
      SLogReadDirection readDirection = SLogReadDirection.FromEnd)
    {
      return await GetAdministratorLog(0,
                                       readDirection,
                                       readDirection == SLogReadDirection.FromEnd ? SLogIndex.Last : SLogIndex.First)
               .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<SLogRecord>> GetSecurityLog(int maxRecords,
                                                                      SLogReadDirection readDirection =
                                                                        SLogReadDirection.FromEnd,
                                                                      uint      startIndex = SLogIndex.Last,
                                                                      DateTime? startTime  = null,
                                                                      DateTime? endTime    = null)
    {
      return await GetSLog(SLogType.Security, readDirection, startIndex, maxRecords, startTime, endTime)
               .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<SLogRecord>> GetAdministratorLog(int maxRecords,
                                                                           SLogReadDirection readDirection =
                                                                             SLogReadDirection.FromEnd,
                                                                           uint      startIndex = SLogIndex.Last,
                                                                           DateTime? startTime  = null,
                                                                           DateTime? endTime    = null)
    {
      return await GetSLog(SLogType.Administrator, readDirection, startIndex, maxRecords, startTime, endTime)
               .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<SLogRecord>> GetSLog(SLogType          logType,
                                                               SLogReadDirection readDirection,
                                                               uint              startIndex,
                                                               int               maxRecords,
                                                               DateTime?         startTime,
                                                               DateTime?         endTime)
    {
      var logHandle = await OpenSLog(logType, readDirection, startIndex).ConfigureAwait(false);

      var log = new List<SLogRecord>();

      while (true)
      {
        var (logPart, shouldContinue) =
          await ReadSLogRecordsBatch(logHandle, readDirection, startTime, endTime).ConfigureAwait(false);

        if (logPart.IsNullOrEmpty() && !shouldContinue)
        {
          break;
        }

        log.AddRange(logPart);

        if (maxRecords > 0 && log.Count >= maxRecords)
        {
          break;
        }
      }

      await CloseSLog(logHandle).ConfigureAwait(false);

      return maxRecords > 0 ? log.Take(maxRecords).ToList() : log;
    }


    public async Task<ulong> OpenSLog(SLogType          logType,
                                      SLogReadDirection direction,
                                      uint              startIndex)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var sLogHandle = await Task.Run(() => _native.СfsSLogOpen(CfId,
                                                                (uint)logType,
                                                                startIndex,
                                                                (uint)direction,
                                                                out errCode,
                                                                ref errBuf,
                                                                errBufLength)).ConfigureAwait(false);

      if (sLogHandle == 0)
      {
        throw new
          Exception($"Ошибка открытия журнала безопасности. \nТип: ${logType}\nОшибка: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
      }

      return sLogHandle;
    }


    public async Task<(IReadOnlyCollection<SLogRecord> logPart, bool shouldContinue)> ReadSLogRecordsBatch(
      ulong             sLogHandle,
      SLogReadDirection readDirection,
      DateTime?         startTime,
      DateTime?         endTime)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;


      var strPtr = await Task.Run(() => _native.CfsSLogReadRecords(CfId,
                                                                   sLogHandle,
                                                                   out errCode,
                                                                   ref errBuf,
                                                                   errBufLength)).ConfigureAwait(false);

      if (strPtr == IntPtr.Zero)
      {
        return (null, false);
      }

      var shouldContinue = false;
      var startTimeUtc   = startTime.HasValue ? TimeZoneInfo.ConvertTimeToUtc(startTime.Value) : (DateTime?)null;
      var endTimeUtc     = endTime.HasValue ? TimeZoneInfo.ConvertTimeToUtc(endTime.Value) : (DateTime?)null;

      var logPart = new List<SLogRecord>();
      var nextPtr = strPtr;

      do
      {
        var index   = TmNativeUtil.GetDoubleNullTerminatorIndexFromPointer(nextPtr);
        var strings = TmNativeUtil.GetUnknownLengthStringListFromDoubleNullTerminatedPointer(nextPtr);

        if (!strings.Any())
        {
          break;
        }

        var record = SLogRecord.CreateFromStringsList(strings);

        nextPtr = IntPtr.Add(nextPtr, index + 1);

        if ((startTimeUtc.HasValue || endTimeUtc.HasValue) && !record.DateTime.HasValue)
        {
          shouldContinue = true;
          continue;
        }

        if (readDirection == SLogReadDirection.FromStart)
        {
          if (record.DateTime >= startTimeUtc)
          {
            // var a = 1;
          }

          if (startTimeUtc.HasValue && record.DateTime < startTimeUtc)
          {
            shouldContinue = true;
            continue;
          }

          if (endTimeUtc.HasValue && record.DateTime > endTimeUtc)
          {
            shouldContinue = false;
            break;
          }
        }
        else
        {
          if (endTimeUtc.HasValue && record.DateTime > endTimeUtc)
          {
            shouldContinue = true;
            continue;
          }

          if (startTimeUtc.HasValue && record.DateTime < startTimeUtc)
          {
            shouldContinue = false;
            break;
          }
        }

        logPart.Add(record);
      } while (!TmNativeUtil.PointerValueIsNull(nextPtr));


      _native.CfsFreeMemory(strPtr);

      return (logPart, shouldContinue);
    }


    public async Task<bool> CloseSLog(ulong sLogHandle)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      return await Task.Run(() => _native.СfsSLogClose(CfId,
                                                       sLogHandle,
                                                       out errCode,
                                                       ref errBuf,
                                                       errBufLength)).ConfigureAwait(false);
    }


    public async Task<int> GetRedirectorPort(string pipeName, int portIndex)
    {
      var portBinData = await GetBin(".cfs.", $"rbs${pipeName}", $"ipg_port{portIndex}")
                          .ConfigureAwait(false);

      if (!int.TryParse(EncodingUtil.Win1251BytesToUtf8(portBinData), out var port))
      {
        return 0;
      }

      return port;
    }

    public async Task<PasswordDigestState> GetBackupPasswordDigestState()
    {
      var binData = await GetBin(".cfs.", ".", "exp_bk")
                          .ConfigureAwait(false);

      if (!binData.Any())
      {
        return PasswordDigestState.NotSupported;
      }
      
      var stateString = EncodingUtil.Win1251BytesToUtf8(binData);
      
      return stateString == "yes" ? PasswordDigestState.Exists : PasswordDigestState.DoesNotExists;
    }

    public async Task<byte[]> GetBin(string uName,
                                     string oName,
                                     string binName)
    {
      (var binData, uint errCode, _) = await SecGetBin(uName, oName, binName).ConfigureAwait(false);

      return errCode == 0 ? binData : Array.Empty<byte>();
    }

    public async Task<(byte[], uint, string)> SecGetBin(string uName,
                                                        string oName,
                                                        string binName)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      uint binLength = 0;

      var resultPtr = await Task.Run(() => _native.CfsIfpcGetBin(CfId,
                                                                 EncodingUtil.Utf8ToWin1251Bytes(uName),
                                                                 EncodingUtil.Utf8ToWin1251Bytes(oName),
                                                                 EncodingUtil.Utf8ToWin1251Bytes(binName),
                                                                 out binLength,
                                                                 out errCode,
                                                                 ref errBuf,
                                                                 errBufLength))
                                .ConfigureAwait(false);

      if (errCode != 0)
      {
        return (null, errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }

      var binData = new byte[binLength];

      Marshal.Copy(resultPtr, binData, 0, binData.Length);
      // не забываем освобождать память, возвращённую из библиотеки
      _native.CfsFreeMemory(resultPtr);

      return (binData, 0, string.Empty);
    }


    public async Task<bool> SetRedirectorPort(string pipeName, int portIndex, int port)
    {
      var portStr = $"{port}";
      var binData = TmNativeUtil.GetFixedBytesWithTrailingZero(portStr,
                                                               portStr.Length + 1,
                                                               EncodingUtil.Cp1251);

      return await SetBin(".cfs.",
                          $"rbs${pipeName}",
                          $"ipg_port{portIndex}",
                          binData).ConfigureAwait(false);
    }

    public async Task<bool> SetBin(string uName,
                                   string oName,
                                   string binName,
                                   byte[] binData)
    {
      (uint errCode, _) = await SecSetBin(uName, oName, binName, binData).ConfigureAwait(false);
      return (errCode == 0);
    }

    public async Task<(uint, string)> SecSetBin(string uName,
                                                string oName,
                                                string binName,
                                                byte[] binData)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = await Task.Run(() => _native.CfsIfpcSetBin(CfId,
                                                              EncodingUtil.Utf8ToWin1251Bytes(uName),
                                                              EncodingUtil.Utf8ToWin1251Bytes(oName),
                                                              EncodingUtil.Utf8ToWin1251Bytes(binName),
                                                              binData,
                                                              (uint)binData.Length,
                                                              out errCode,
                                                              ref errBuf,
                                                              errBufLength)).ConfigureAwait(false);
      if (errCode != 0)
      {
        return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }

      return (0, string.Empty);
    }

    public AccessMasksDescriptor SecGetAccessDescriptor(string sSetupPath, string progName)
    {
      var accessMasksDescriptor = new AccessMasksDescriptor();
      
      var iniSections = new Dictionary<string, string>
      {
        { MSTreeConsts.Portcore, "master#1.prp.Security" },
        { MSTreeConsts.master, "master.prp.Security" },
        { MSTreeConsts.RBaseServer, "rbsrv#1.prp.Security" },
        { MSTreeConsts.rbsrv_old, "serv_dll.ch.RbsSecurity" },
        { MSTreeConsts.TmServer, "pcsrv#1.prp.Security" },
        { MSTreeConsts.pcsrv_old, "serv_dll.ch.TmsSecurity" },
      };

      if (!iniSections.TryGetValue(progName, out var section))
      {
        return null;
      }
      
      var sectionPtr = _native.CfsGetAccessDescriptor(EncodingUtil.Utf8ToWin1251Bytes(sSetupPath), 
                                                      EncodingUtil.Utf8ToWin1251Bytes(section));
      if (sectionPtr == IntPtr.Zero)
      {
        throw new Exception("GetAccessDescriptor sec_ptr error");
      }
        
        
      var cfsAccessDescriptor = Marshal.PtrToStructure<CfsAccessDescriptor>(sectionPtr);
      _native.CfsFreeMemory(sectionPtr);

      accessMasksDescriptor.ObjTypeName["ru"] = EncodingUtil.Win1251BytesToUtf8(cfsAccessDescriptor.ObjTypeName.rus)
                                                            .Replace("&", "");
      accessMasksDescriptor.ObjTypeName["en"] = EncodingUtil.Win1251BytesToUtf8(cfsAccessDescriptor.ObjTypeName.eng)
                                                            .Replace("&", "");
        
      var pre = cfsAccessDescriptor.NamePrefix.Split('$');
      if (pre.Length > 1)
      {
        accessMasksDescriptor.NamePrefix = pre[0] + "$";
      }
      else
      {
        accessMasksDescriptor.NamePrefix = cfsAccessDescriptor.NamePrefix;
      }

      for (var bit = 0; bit < 32; bit++)
      {
        if (cfsAccessDescriptor.Bit[bit].Mask == 0xffffffff)
        {
          continue;
        }
          
        var newMask = new AccessMask
        {
          Mask = cfsAccessDescriptor.Bit[bit].Mask,
          Description =
          {
            ["ru"] = EncodingUtil.Win1251BytesToUtf8(cfsAccessDescriptor.Bit[bit].rus).Replace("&", ""),
            ["en"] = EncodingUtil.Win1251BytesToUtf8(cfsAccessDescriptor.Bit[bit].eng).Replace("&", "")
          }
        };
          
        accessMasksDescriptor.AccessMasks.Add(newMask);
      }

      return accessMasksDescriptor;
    }

    public ExtendedRightsDescriptor SecGetExtendedRightsDescriptor(string sSetupPath)
    {
      var ret     = new ExtendedRightsDescriptor();
      var extendedRightsPtr = _native.CfsGetExtendedUserRightsDescriptor(EncodingUtil.Utf8ToWin1251Bytes(sSetupPath), 
                                                                         EncodingUtil.Utf8ToWin1251Bytes("TmsExtRights"), 
                                                                         0);
      
      if (extendedRightsPtr == IntPtr.Zero)
      {
        return null;
      }

      var extendedRights = Marshal.PtrToStructure<CfsExtSrvrtDescriptor>(extendedRightsPtr);
      _native.CfsFreeMemory(extendedRightsPtr);

      ret.DoUserID   = extendedRights.DoUserID;
      ret.DoUserPwd  = extendedRights.DoUserPwd;
      ret.DoUserNick = extendedRights.DoUserNick;
      ret.MaxUserID  = extendedRights.MaxUserID;
      ret.DoGroup    = extendedRights.DoGroup;
      ret.DoKeyID    = extendedRights.DoKeyID;
      
      var strRights = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(extendedRights.Rights, 
                                                                                  10240);
      
      foreach (var item in strRights)
      {
        if (item.Length < 2) continue;

        var right = new ExtendedRight();

        switch (item[0])
        {
          case 'B':
          {
            right.IsHeader = true;
            var descriptions = item.Substring(1).Split('`');
          
            if (descriptions.Length != 2)
            {
              continue;
            }
          
            right.Description["ru"] = descriptions[1];
            right.Description["en"] = descriptions[0];
          
            ret.Rights.Add(right);
            break;
          }
          case 'R':
          {
            right.IsHeader = false;
            var bitAndDesc = item.Substring(1).Split('-');
            
            if (bitAndDesc.Length != 2 || !byte.TryParse(bitAndDesc[0], out var bn))
            {
              continue;
            }

            right.ByteIndex = bn;
            var descriptions = bitAndDesc[1].Split('`');
            if (descriptions.Length != 2)
            {
              continue;
            }
            
            right.Description["ru"] = descriptions[1];
            right.Description["en"] = descriptions[0];
            
            ret.Rights.Add(right);
            
            break;
          }
        }
      }

      return ret;
    }

    public async Task<(IReadOnlyCollection<string>, uint, string)> SecEnumUsers()
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      var resultPtr = await Task.Run(() => _native.СfsIfpcEnumUsers(CfId, out errCode, ref errBuf, errBufLength))
                                .ConfigureAwait(false);
      if (errCode != 0)
      {
        return (null, errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }

      return (TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(resultPtr, 16384), 0, string.Empty);
    }

    public async Task<(IReadOnlyCollection<string>, uint, string)> SecEnumOSUsers()
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      // сделаем отдельное соединение чтобы не занимать долгой выборкой основное
      IntPtr temp_cfsid =
        await Task.Run(() => _native.CfsConnect(EncodingUtil.Utf8ToWin1251Bytes(Host), out errCode, ref errBuf,
                                                errBufLength)).ConfigureAwait(false);
      if ((temp_cfsid == IntPtr.Zero) && (errCode != 0))
      {
        return (null, errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }

      var resultPtr =
        await Task.Run(() => _native.СfsIfpcEnumOSUsers(temp_cfsid, out errCode, ref errBuf, errBufLength))
                  .ConfigureAwait(false);
      _native.CfsDisconnect(temp_cfsid);
      if (errCode != 0)
      {
        return (null, errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }

      return (TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(resultPtr, 16384), 0, string.Empty);
    }

    public async Task<(uint, string)> SecChangeUserPassword(string username, string password)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      if ((username.Length > 0) && !username.StartsWith("*"))
      {
        return (12345, "Смена пароля допустима только у собственных пользователей");
      }

      var result =
        await Task.Run(() => _native.CfsIfpcSetUserPwd(CfId, EncodingUtil.Utf8ToWin1251Bytes(username),
                                                       EncodingUtil.Utf8ToWin1251Bytes(password), out errCode,
                                                       ref errBuf, errBufLength)).ConfigureAwait(false);
      if (errCode != 0)
      {
        return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (0, string.Empty);
      }
    }

    public async Task<(uint, string)> SecDeleteUser(string username)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result =
        await Task.Run(() => _native.СfsIfpcDeleteUser(CfId, EncodingUtil.Utf8ToWin1251Bytes(username), out errCode,
                                                       ref errBuf, errBufLength)).ConfigureAwait(false);
      if (errCode != 0)
      {
        return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (0, string.Empty);
      }
    }

    public async Task<(uint, uint, string)> SecGetAccessMask(string username, string oName)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result =
        await Task.Run(() => _native.СfsIfpcGetAccess(CfId, EncodingUtil.Utf8ToWin1251Bytes(username),
                                                      EncodingUtil.Utf8ToWin1251Bytes(oName), out errCode, ref errBuf,
                                                      errBufLength)).ConfigureAwait(false);
      if (errCode != 0)
      {
        return (0, errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (result, 0, string.Empty);
      }
    }

    public async Task<(uint, string)> SecSetAccessMask(string username, string oName, uint AccessMask)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result =
        await Task.Run(() => _native.СfsIfpcSetAccess(CfId, EncodingUtil.Utf8ToWin1251Bytes(username),
                                                      EncodingUtil.Utf8ToWin1251Bytes(oName), AccessMask, out errCode,
                                                      ref errBuf, errBufLength)).ConfigureAwait(false);
      if (errCode != 0)
      {
        return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (0, string.Empty);
      }
    }

    public async Task<(ExtendedUserData, uint, string)> SecGetExtendedUserData(
      string serverType, string serverName, string username)
    {
      (var resultPtr, uint errCode, string errString) =
        await SecGetBin(username, serverType + serverName, "extr").ConfigureAwait(false);
      if ((errCode != 0) || (resultPtr.Length == 0))
      {
        return (null, errCode, errString);
      }

      var ui   = new ExtendedUserData();
      var data = TmNativeUtil.GetStringListFromDoubleNullTerminatedBytes(resultPtr);
      foreach (var item in data)
      {
        var KeyValuePair = item.Split('=');
        if (KeyValuePair.Length == 2)
        {
          switch (KeyValuePair[0])
          {
            case "UserID":
            {
              if (Int32.TryParse(KeyValuePair[1], out int i))
                ui.UserID = i;
            }
              break;
            case "UserNick":
              ui.UserNick = KeyValuePair[1];
              break;
            case "UserPwd":
              ui.UserPwd = KeyValuePair[1];
              break;
            case "Group":
            {
              if (Int32.TryParse(KeyValuePair[1], out int i))
                ui.Group = i;
            }
              break;
            case "KeyID":
              ui.KeyID = KeyValuePair[1];
              break;
          }
        }
        else if (KeyValuePair.Length == 1)
        {
          if (KeyValuePair[0].StartsWith("R"))
          {
            if (Int32.TryParse(KeyValuePair[0].Substring(1), out int idx))
            {
              if ((0 <= idx) && (idx < ui.Rights.Length))
              {
                ui.Rights[idx] = 1;
              }
            }
          }
        }
      }

      return (ui, 0, string.Empty);
    }

    public async Task<(uint, string)> SecSetExtendedUserData(string serverType, string serverName, string username,
                                                             ExtendedUserData extendedUserData)
    {
      var data = new List<string>()
      {
        { $"UserID={extendedUserData.UserID}" },
        { $"UserNick={extendedUserData.UserNick}" },
        { $"UserPwd={extendedUserData.UserPwd}" },
        { $"Group={extendedUserData.Group}" },
        { $"KeyID={extendedUserData.KeyID}" },
      };
      for (int idx = 0; idx < extendedUserData.Rights.Length; idx++)
      {
        if (extendedUserData.Rights[idx] == 1)
        {
          data.Add("R" + idx);
        }
      }

      var bin = TmNativeUtil.GetDoubleNullTerminatedBytesFromStringList(data);
      (uint errCode, string errString) =
        await SecSetBin(username, serverType + serverName, "extr", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        return (errCode, errString);
      }
      else
      {
        return (0, string.Empty);
      }
    }

    public async Task<(UserPolicy, uint, string)> SecGetUserPolicy(string username)
    {
      byte[] bin;
      uint   errCode, resErrCode = 0;
      ;
      string errString, resErrString = string.Empty;
      var    _UserPolicy             = new UserPolicy();

      (bin, errCode, errString) = await SecGetBin(username, ".", "bad_logon").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (Int32.TryParse(s, out int i))
        {
          _UserPolicy.BadLogonCount = i;
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(username, ".", "not_before").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (Int32.TryParse(s, out int ut))
        {
          _UserPolicy.NotBefore = DateUtil.GetDateTimeFromTimestamp(ut, 0);
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(username, ".", "not_after").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (Int32.TryParse(s, out int ut))
        {
          _UserPolicy.NotAfter = DateUtil.GetDateTimeFromTimestamp(ut, 0);
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(username, ".", "chgp").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (Int32.TryParse(s, out int i))
        {
          if (i == 0)
            _UserPolicy.MustChangePassword = false;
          if (i == 1)
            _UserPolicy.MustChangePassword = true;
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(username, ".", "blocked").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (Int32.TryParse(s, out int i))
        {
          if (i == 0)
            _UserPolicy.IsBlocked = false;
          if (i == 1)
            _UserPolicy.IsBlocked = true;
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(username, ".", "logon_limit").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (Int32.TryParse(s, out int i))
        {
          _UserPolicy.BadLogonLimit = i;
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(username, ".", "initial").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (Int32.TryParse(s, out int i))
        {
          if (i == 0)
            _UserPolicy.Predefined = false;
          if (i == 1)
            _UserPolicy.Predefined = true;
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(username, ".", "mac_list").ConfigureAwait(false);
      if ((errCode == 0) && (bin.Length >= 6))
      {
        _UserPolicy.EnabledMACs = string.Empty;
        for (int i = 0; i < bin.Length; i += 6)
        {
          byte[] address = bin.Skip(i).Take(6).ToArray();
          _UserPolicy.EnabledMACs += BitConverter.ToString(address).Replace('-', ':') + '\n';
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      // проверяем наличие пароля только у своих пользователей
      if (username.StartsWith("*"))
      {
        (bin, errCode, errString) = await SecGetBin(username, ".", "pwd").ConfigureAwait(false);
        if (errCode == 0)
        {
          if (bin.Length > 0)
            _UserPolicy.PasswordSet = true;
          else
            _UserPolicy.PasswordSet = false;
        }
        else if (errCode != 2) // "No data"
        {
          resErrCode   =  errCode;
          resErrString += errString;
        }
      }
      else
        _UserPolicy.PasswordSet = true;

      (bin, errCode, errString) = await SecGetBin(username, ".", "uctgr").ConfigureAwait(false);
      if (errCode == 0)
      {
        _UserPolicy.UserCategory = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(username, ".", "utmpl").ConfigureAwait(false);
      if (errCode == 0)
      {
        _UserPolicy.UserTemplate = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      return (_UserPolicy, resErrCode, resErrString);
    }

    public async Task<(uint, string)> SecSetUserPolicy(string username, UserPolicy userPolicy)
    {
      byte[] bin;
      uint   errCode,   resErrCode   = 0;
      string errString, resErrString = string.Empty;
      string enc                     = EncodingUtil.Cp1251;


      if (userPolicy.IsBlocked)
      {
        bin = TmNativeUtil.GetFixedBytesWithTrailingZero("1", 2, enc);
      }
      else
      {
        bin = TmNativeUtil.GetFixedBytesWithTrailingZero("0", 2, enc);
      }

      (errCode, errString) = await SecSetBin(username, ".", "blocked", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      if (userPolicy.MustChangePassword)
      {
        bin = TmNativeUtil.GetFixedBytesWithTrailingZero("1", 2, enc);
      }
      else
      {
        bin = TmNativeUtil.GetFixedBytesWithTrailingZero("0", 2, enc);
      }

      (errCode, errString) = await SecSetBin(username, ".", "chgp", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      string dt;
      if (userPolicy.NotBefore.Equals(DateTime.MinValue))
      {
        dt = "";
      }
      else
      {
        dt = _native.UxGmTime2UxTime(DateUtil.GetUtcTimestampFromDateTime(userPolicy.NotBefore)).ToString();
      }

      bin                  = TmNativeUtil.GetFixedBytesWithTrailingZero(dt, dt.Length + 1, enc);
      (errCode, errString) = await SecSetBin(username, ".", "not_before", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      if (userPolicy.NotAfter.Equals(DateTime.MinValue))
      {
        dt = "";
      }
      else
      {
        dt = _native.UxGmTime2UxTime(DateUtil.GetUtcTimestampFromDateTime(userPolicy.NotAfter)).ToString();
      }

      bin                  = TmNativeUtil.GetFixedBytesWithTrailingZero(dt, dt.Length + 1, enc);
      (errCode, errString) = await SecSetBin(username, ".", "not_after", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      string n = userPolicy.BadLogonLimit.ToString();
      bin                  = TmNativeUtil.GetFixedBytesWithTrailingZero(n, n.Length + 1, enc);
      (errCode, errString) = await SecSetBin(username, ".", "logon_limit", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      bin = TmNativeUtil.GetFixedBytesWithTrailingZero(userPolicy.UserCategory, userPolicy.UserCategory.Length + 1,
                                                       enc);
      (errCode, errString) = await SecSetBin(username, ".", "uctgr", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      bin = TmNativeUtil.GetFixedBytesWithTrailingZero(userPolicy.UserTemplate, userPolicy.UserTemplate.Length + 1,
                                                       enc);
      (errCode, errString) = await SecSetBin(username, ".", "utmpl", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      bin = new byte[0];
      var MACs = userPolicy.EnabledMACs.Split('\n');
      foreach (var mac in MACs)
      {
        if (mac.Length > 0)
        {
          var macbytes = mac.Split(new char[] { ':', '-' });
          if (macbytes.Length == 6)
          {
            byte[] MAC  = new byte[6];
            bool   good = true;
            for (int i = 0; i < 6; i++)
            {
              try
              {
                MAC[i] = Convert.ToByte(macbytes[i], 16);
              }
              catch
              {
                good = false;
                break;
              }
            }

            if (good)
            {
              bin = bin.Concat(MAC).ToArray();
            }
          }
        }
      }

      (errCode, errString) = await SecSetBin(username, ".", "mac_list", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      return (resErrCode, resErrString);
    }

    public async Task<(PasswordPolicy, uint, string)> SecGetPasswordPolicy()
    {
      byte[] bin;
      uint   errCode, resErrCode = 0;
      ;
      string errString, resErrString = string.Empty;
      var    passwordPolicy          = new PasswordPolicy();

      (bin, errCode, errString) = await SecGetBin(".cfs.", ".", "own_pch").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (Int32.TryParse(s, out int i))
        {
          passwordPolicy.AdminPasswordChange = (i == 0);
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(".cfs.", ".", "pwd_pol").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (Int32.TryParse(s, out int i))
        {
          passwordPolicy.EnforcePasswordCheck = (i == 1);
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(".cfs.", ".", "pwd_pol_len").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (Int32.TryParse(s, out int i))
        {
          passwordPolicy.MinPasswordLength = i;
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(".cfs.", ".", "p_ex_days").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (Int32.TryParse(s, out int i))
        {
          passwordPolicy.PasswordTTL_Days = i;
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      (bin, errCode, errString) = await SecGetBin(".cfs.", ".", "pwd_pol_flg").ConfigureAwait(false);
      if (errCode == 0)
      {
        string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
        if (uint.TryParse(s, out uint ui))
        {
          PWDPOL flags = (PWDPOL)ui;
          passwordPolicy.PwdChars_Upper          = flags.HasFlag(PWDPOL.Upper);
          passwordPolicy.PwdChars_Digits         = flags.HasFlag(PWDPOL.Digits);
          passwordPolicy.PwdChars_Special        = flags.HasFlag(PWDPOL.Spec);
          passwordPolicy.PwdChars_NoRepeat       = flags.HasFlag(PWDPOL.CheckRepeat);
          passwordPolicy.PwdChars_NoSequential   = flags.HasFlag(PWDPOL.CheqSeq);
          passwordPolicy.PwdChars_CheckDictonary = flags.HasFlag(PWDPOL.CheckDict);
          passwordPolicy.CheckOldPasswords       = flags.HasFlag(PWDPOL.CheckCache);
        }
      }
      else if (errCode != 2) // "No data"
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      return (passwordPolicy, resErrCode, resErrString);
    }

    public async Task<(uint, string)> SecSetPasswordPolicy(PasswordPolicy passwordPolicy)
    {
      byte[] bin;
      string n;
      uint   errCode,   resErrCode   = 0;
      string errString, resErrString = string.Empty;
      string enc                     = EncodingUtil.Cp1251;


      if (passwordPolicy.AdminPasswordChange)
      {
        bin = TmNativeUtil.GetFixedBytesWithTrailingZero("0", 2, enc);
      }
      else
      {
        bin = TmNativeUtil.GetFixedBytesWithTrailingZero("1", 2, enc);
      }

      (errCode, errString) = await SecSetBin(".cfs.", ".", "own_pch", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      if (passwordPolicy.EnforcePasswordCheck)
      {
        bin = TmNativeUtil.GetFixedBytesWithTrailingZero("1", 2, enc);
      }
      else
      {
        bin = TmNativeUtil.GetFixedBytesWithTrailingZero("0", 2, enc);
      }

      (errCode, errString) = await SecSetBin(".cfs.", ".", "pwd_pol", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      n                    = passwordPolicy.MinPasswordLength.ToString();
      bin                  = TmNativeUtil.GetFixedBytesWithTrailingZero(n, n.Length + 1, enc);
      (errCode, errString) = await SecSetBin(".cfs.", ".", "pwd_pol_len", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      n                    = passwordPolicy.PasswordTTL_Days.ToString();
      bin                  = TmNativeUtil.GetFixedBytesWithTrailingZero(n, n.Length + 1, enc);
      (errCode, errString) = await SecSetBin(".cfs.", ".", "p_ex_days", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      PWDPOL flags                                       = (PWDPOL)0xff_ff_ff_ff;
      if (!passwordPolicy.PwdChars_Upper) flags          &= ~PWDPOL.Upper;
      if (!passwordPolicy.PwdChars_Digits) flags         &= ~PWDPOL.Digits;
      if (!passwordPolicy.PwdChars_Special) flags        &= ~PWDPOL.Spec;
      if (!passwordPolicy.PwdChars_NoRepeat) flags       &= ~PWDPOL.CheckRepeat;
      if (!passwordPolicy.PwdChars_NoSequential) flags   &= ~PWDPOL.CheqSeq;
      if (!passwordPolicy.PwdChars_CheckDictonary) flags &= ~PWDPOL.CheckDict;
      if (!passwordPolicy.CheckOldPasswords) flags       &= ~PWDPOL.CheckCache;
      string s_flg                                       = flags.ToString();
      bin                  = TmNativeUtil.GetFixedBytesWithTrailingZero(s_flg, s_flg.Length + 1, enc);
      (errCode, errString) = await SecSetBin(".cfs.", ".", "pwd_pol_flg", bin).ConfigureAwait(false);
      if (errCode != 0)
      {
        resErrCode   =  errCode;
        resErrString += errString;
      }

      return (resErrCode, resErrString);
    }

    public async Task<(ComputerInfo, uint, string)> GetComputerInfo()
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var _computerInfoS = new ComputerInfoS()
      {
        Len = (uint)Marshal.SizeOf(typeof(ComputerInfoS))
      };
      await Task.Run(() => _native.CfsGetComputerInfo(CfId, ref _computerInfoS, out errCode, ref errBuf, errBufLength))
                .ConfigureAwait(false);
      if (errCode != 0)
      {
        return (null, errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        var computerInfo = new ComputerInfo
        {
          ComputerName      = _computerInfoS.ComputerName,
          PrimaryDomainName = _computerInfoS.DomInfo.PrimaryDomainName,
          OS_ProductType    = $"ostype{_computerInfoS.NtProductType}",
          OS_Version        = $"{_computerInfoS.NtVerMaj}.{_computerInfoS.NtVerMin} build {_computerInfoS.NtBuild}",
          Architecture      = (_computerInfoS.Win64 == 1) ? "x64" : "x86",
          Acp               = _computerInfoS.Acp,

          ServerTimeGMT = DateUtil.GetDateTimeFromTimestamp(_computerInfoS.CurrentGMT, _computerInfoS.CurrentMs),
          Uptime        = _computerInfoS.Uptime,

          Copyright = $"oiktype{_computerInfoS.Copyright}",
          CfsVer    = $"{_computerInfoS.CfsVerMaj}.{_computerInfoS.CfsVerMin}",

          UserName   = _computerInfoS.UserName,
          UserAddr   = _computerInfoS.UserAddr,
          AccessMask = _computerInfoS.AccessMask,
          IpAddrs    = new List<string>()
        };
        foreach (var addr in _computerInfoS.IpAddrs)
        {
          if (addr == 0)
            break;
          computerInfo.IpAddrs.Add(new IPAddress(addr).ToString());
        }

        computerInfo.SoftwareKeyID = BitConverter.ToString(_computerInfoS.LOctet).Replace("-", "");

        // читаем дату билда и установки отдельно
        try
        {
          computerInfo.BuildDate = await GetIniString("@@", "IInfo", "BuildTime").ConfigureAwait(false);
          if (computerInfo.BuildDate.Equals(string.Empty))
          {
            // если попали на старый сервер
            var path = Path.Combine(await GetBasePath().ConfigureAwait(false),
                                    "dispserv.ini");
            computerInfo.BuildDate   = await GetIniString(path, "Info", "BuildTime").ConfigureAwait(false);
            computerInfo.InstallDate = await GetIniString(path, "Info", "InstTime").ConfigureAwait(false);
          }
          else
            computerInfo.InstallDate = await GetIniString("@@", "IInfo", "InstTime").ConfigureAwait(false);
        }
        catch
        {
        }


        return (computerInfo, 0, string.Empty);
      }
    }

    private static readonly string BackupDateFormat = "dd_MM_yyyy (HH.mm.ss)";

    public async Task<(bool, string)> SaveMachineConfig(string directory, bool full)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      string    fileName;
      if (full)
      {
        fileName = "FullConf-" + DateTime.Now.ToString(BackupDateFormat) + ".cfim";
      }
      else
      {
        fileName = "MasterConf-" + DateTime.Now.ToString(BackupDateFormat) + ".pkf";
      }

      var result = await Task.Run(() => _native.CfsSaveMachineConfig(full,
                                                                     EncodingUtil.Utf8ToWin1251Bytes(Host),
                                                                     EncodingUtil
                                                                       .Utf8ToWin1251Bytes(Path.Combine(directory,
                                                                         fileName)), ref errBuf, errBufLength))
                             .ConfigureAwait(false);
      if (result != true)
      {
        return (false, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (true, string.Empty);
      }
    }

    public async Task<(bool, string)> SaveMachineConfigEx(string           directory, uint scope,
                                                          TmNativeCallback callback          = null,
                                                          IntPtr           callbackParameter = default)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      string    fileName     = "undefined";
      if (scope == 0) //#define CFS_SMC_DEV		0
      {
        fileName = "MasterConf-" + DateTime.Now.ToString(BackupDateFormat) + ".pkf";
      }
      else if (scope == 1) //#define CFS_SMC_MEDIUM		1
      {
        fileName = "FullConf-" + DateTime.Now.ToString(BackupDateFormat) + ".cfim";
      }
      else if (scope == 2) //#define CFS_SMC_COMPLETE	2
      {
        fileName = "FullConfRetro-" + DateTime.Now.ToString(BackupDateFormat) + ".cfim";
      }

      var result = await Task.Run(() => _native.CfsSaveMachineConfigEx(
                                                                       EncodingUtil.Utf8ToWin1251Bytes(Host),
                                                                       EncodingUtil
                                                                         .Utf8ToWin1251Bytes(Path.Combine(directory,
                                                                           fileName)),
                                                                       scope,
                                                                       callback, callbackParameter,
                                                                       ref errBuf, errBufLength)).ConfigureAwait(false);

      if (result != true)
      {
        return (false, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (true, string.Empty);
      }
    }

    public async Task<(bool, string)> RestoreMachineConfig(string filename)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      string remoteFilename = Path.GetFileName(filename);
      (bool res, string errString) = await PutFile(filename, remoteFilename).ConfigureAwait(false);
      if (!res)
        return (res, errString);

      var result = await Task.Run(() => _native.CfsPrepNewConfig(CfId,
                                                                 EncodingUtil.Utf8ToWin1251Bytes(remoteFilename),
                                                                 out errCode, ref errBuf, errBufLength))
                             .ConfigureAwait(false);

      if (errCode != 0)
      {
        return (false, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (true, string.Empty);
      }
    }

    public async Task<(IReadOnlyCollection<string>, uint, string)> DirEnum(string path)
    {
      const int resBufLength = 20480;
      var       resBuf       = new char[resBufLength];
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = await Task.Run(() => _native.CfsDirEnum(CfId,
                                                           EncodingUtil.Utf8ToWin1251Bytes(path),
                                                           ref resBuf, resBufLength,
                                                           out errCode, ref errBuf, errBufLength))
                             .ConfigureAwait(false);
      if (errCode != 0)
      {
        return (null, errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (TmNativeUtil.GetStringListFromDoubleNullTerminatedChars(resBuf), 0, string.Empty);
      }
    }

    public async Task<(bool, string)> CreateBackup(string progName, string pipeName, string directory, bool withRetro,
                                                   TmNativeCallback callback = null,
                                                   IntPtr callbackParameter = default)
    {
      uint   bflags;
      bool   result       = false;
      byte[] reserved_buf = TmNativeUtil.GetFixedBytesWithTrailingZero(directory, 260, EncodingUtil.Cp1251);

      switch (progName)
      {
        case MSTreeConsts.TmServer:
        case MSTreeConsts.pcsrv_old:
          //#define TMS_BACKUP_CONFIG	1
          //#define TMS_BACKUP_ARRAY	2
          //#define TMS_BACKUP_EVLOG	4
          //#define TMS_BACKUP_ALARMS	8
          //#define TMS_BACKUP_RETRO	0x10
          //#define TMS_BACKUP_SECURITY	0x20
          bflags = 1 | 2 | 4 | 8;
          if (withRetro) bflags |= 0x10;
          result = await Task.Run(() => _native.TmcBackupServerProcedure(EncodingUtil.Utf8ToWin1251Bytes(Host),
                                                                         EncodingUtil.Utf8ToWin1251Bytes(pipeName),
                                                                         reserved_buf, ref bflags, 0, callback,
                                                                         callbackParameter)).ConfigureAwait(false);
          break;
        case MSTreeConsts.RBaseServer:
        case MSTreeConsts.rbsrv_old:
          //#define RBS_BACKUP_BASES	1
          //#define RBS_BACKUP_SECURITY 2
          bflags = 1;
          result = await Task.Run(() => _native.RbcBackupServerProcedure(EncodingUtil.Utf8ToWin1251Bytes(Host),
                                                                         EncodingUtil.Utf8ToWin1251Bytes(pipeName),
                                                                         reserved_buf, ref bflags, 0, callback,
                                                                         callbackParameter)).ConfigureAwait(false);
          break;
      }

      if (result)
        return (result, string.Empty);
      else
        return (result, "error");
    }

    public async Task<(bool, string)> RestoreBackup(string progName, string pipeName, string filename, bool withRetro,
                                                    TmNativeCallback callback = null,
                                                    IntPtr callbackParameter = default)
    {
      uint bflags = 1;
      bool result = false;
      switch (progName)
      {
        case MSTreeConsts.TmServer:
        case MSTreeConsts.pcsrv_old:
          bflags = 1 | 2 | 4 | 8;
          if (withRetro) bflags |= 0x10;
          result = await Task.Run(() => _native.TmcRestoreServer(true, EncodingUtil.Utf8ToWin1251Bytes(Host),
                                                                 EncodingUtil.Utf8ToWin1251Bytes(pipeName),
                                                                 EncodingUtil.Utf8ToWin1251Bytes(filename), ref bflags,
                                                                 0, callback, callbackParameter)).ConfigureAwait(false);
          break;
        case MSTreeConsts.RBaseServer:
        case MSTreeConsts.rbsrv_old:
          bflags = 1;
          result = await Task.Run(() => _native.TmcRestoreServer(false, EncodingUtil.Utf8ToWin1251Bytes(Host),
                                                                 EncodingUtil.Utf8ToWin1251Bytes(pipeName),
                                                                 EncodingUtil.Utf8ToWin1251Bytes(filename), ref bflags,
                                                                 0, callback, callbackParameter)).ConfigureAwait(false);
          break;
      }

      if (bflags == 0)
        return (result, "nothing restored");
      if (result)
        return (result, string.Empty);
      else
        return (result, "error");
    }

    public async Task<(uint, string)> BackupSecurity(string directory, string pwd = "")
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      string    fileName     = "", snp = "";

      if (String.IsNullOrEmpty(pwd))
      {
        snp = "\x1";
        var _computerInfoS = new ComputerInfoS()
        {
          Len = (uint)Marshal.SizeOf(typeof(ComputerInfoS))
        };
        await Task
              .Run(() => _native.CfsGetComputerInfo(CfId, ref _computerInfoS, out errCode, ref errBuf, errBufLength))
              .ConfigureAwait(false);
        if (errCode != 0)
        {
          return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
        }

        switch (_computerInfoS.SecType)
        {
          case 0:
            fileName = "users.ini";
            break;
          case 1:
            fileName = "users.01";
            break;
          case 2:
            fileName = "users.02";
            break;
          default:
            return (1001, "Unknown security type");
        }

        fileName += " " + DateTime.Now.ToString(BackupDateFormat) + ".bbk";
      }
      else
      {
        snp      = "\x2";
        fileName = "Security-" + DateTime.Now.ToString(BackupDateFormat) + ".sbk";
      }

      await Task.Run(() => _native.CfsIfpcBackupSecurity(CfId,
                                                         EncodingUtil.Utf8ToWin1251Bytes(snp),
                                                         EncodingUtil.Utf8ToWin1251Bytes(pwd),
                                                         EncodingUtil.Utf8ToWin1251Bytes(Path.Combine(directory,
                                                           fileName)),
                                                         out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
      if (errCode != 0)
      {
        return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (0, string.Empty);
      }
    }

    public async Task<(uint, string)> RestoreSecurity(string filename, string pwd)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      string    snp          = "\x2";

      await Task.Run(() => _native.CfsIfpcRestoreSecurity(CfId,
                                                          EncodingUtil.Utf8ToWin1251Bytes(snp),
                                                          EncodingUtil.Utf8ToWin1251Bytes(pwd),
                                                          EncodingUtil.Utf8ToWin1251Bytes(filename),
                                                          out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
      if (errCode != 0)
      {
        return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (0, string.Empty);
      }
    }

    public async Task<(IReadOnlyCollection<string>, string)> EnumPackedFiles(string pkfName)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];

      var result =
        await Task.Run(() => _native.PkfEnumPackedFiles(EncodingUtil.Utf8ToWin1251Bytes(pkfName), ref errBuf,
                                                        errBufLength)).ConfigureAwait(false);
      if (result == IntPtr.Zero)
      {
        return (null, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        var res = (TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(result, 65536));
        _native.PkfFreeMemory(result);
        return (res, string.Empty);
      }
    }

    public async Task<(IReadOnlyCollection<string>, string)> UnPack(string pkfName, string dirname)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];

      var result = await Task.Run(() => _native.PkfUnPack(EncodingUtil.Utf8ToWin1251Bytes(pkfName),
                                                          EncodingUtil.Utf8ToWin1251Bytes(dirname), ref errBuf,
                                                          errBufLength)).ConfigureAwait(false);
      if (result == IntPtr.Zero)
      {
        return (null, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        var res = (TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(result, 65536));
        _native.PkfFreeMemory(result);
        return (res, string.Empty);
      }
    }

    public async Task<(bool, string)> ExtractFile(string pkfName, string filename, string dirname)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];

      var result = await Task.Run(() => _native.PkfExtractFile(EncodingUtil.Utf8ToWin1251Bytes(pkfName),
                                                               EncodingUtil.Utf8ToWin1251Bytes(filename),
                                                               EncodingUtil.Utf8ToWin1251Bytes(dirname),
                                                               ref errBuf, errBufLength)).ConfigureAwait(false);
      if (!result)
      {
        return (false, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (true, string.Empty);
      }
    }

    public async Task<(uint, string, UInt64, UInt32)> StartTestTmcalc(string tmsName, string clcName, UInt32 testWay,
                                                                      UInt32 testFlags)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      UInt64    handle       = 0;
      UInt32    pid          = 0;

      var result = await Task.Run(() => _native.CfsIfpcTestTmcalc(CfId,
                                                                  EncodingUtil.Utf8ToWin1251Bytes(tmsName),
                                                                  EncodingUtil.Utf8ToWin1251Bytes(clcName),
                                                                  testWay, testFlags,
                                                                  out handle, out pid,
                                                                  out errCode, ref errBuf, errBufLength
                                                                 )).ConfigureAwait(false);
      if (!result)
      {
        return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf), 0, 0);
      }
      else
      {
        return (0, string.Empty, handle, pid);
      }
    }

    public async Task<(uint, string)> StopTestTmcalc(UInt64 handle, UInt32 pid)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      var result = await Task.Run(() => _native.CfsIfpcStopTestTmcalc(CfId,
                                                                      handle, pid,
                                                                      out errCode, ref errBuf, errBufLength
                                                                     )).ConfigureAwait(false);
      if (!result)
      {
        return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
      }
      else
      {
        return (0, string.Empty);
      }
    }

    public async Task<(bool, string)> PmonCheckProcess(string processNameArgs)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      var result = await Task.Run(() => _native.СfsPmonCheckProcess(CfId,
                                                                    EncodingUtil.Utf8ToWin1251Bytes(processNameArgs),
                                                                    out errCode, ref errBuf, errBufLength
                                                                   )).ConfigureAwait(false);
      return (result, EncodingUtil.Win1251BytesToUtf8(errBuf));
    }

    public async Task<(bool, string)> PmonStopProcess(string processNameArgs)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0, pnumfound = 0;
      var result = await Task.Run(() => _native.CfsPmonStopProcess(CfId,
                                                                   EncodingUtil.Utf8ToWin1251Bytes(processNameArgs),
                                                                   out pnumfound,
                                                                   out errCode, ref errBuf, errBufLength
                                                                  )).ConfigureAwait(false);
      return (result, EncodingUtil.Win1251BytesToUtf8(errBuf));
    }

    public async Task<(bool, string)> PmonRestartProcess(string processNameArgs)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      var result = await Task.Run(() => _native.CfsPmonRestartProcess(CfId, 
                                                                      EncodingUtil.Utf8ToWin1251Bytes(processNameArgs),
                                                                      out errCode, 
                                                                      ref errBuf, 
                                                                      errBufLength
                                                                     )).ConfigureAwait(false);
      return (result, EncodingUtil.Win1251BytesToUtf8(errBuf));
    }

    public async Task<(bool, string)> SwapFnSrvRole(string encodedCredentials, string fnsName, bool dryRun)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      
      var result = await Task.Run(() => _native.CfsSwapFnSrvRole(EncodingUtil.Utf8ToWin1251Bytes(encodedCredentials),
                                                                 dryRun,
                                                                 EncodingUtil.Utf8ToWin1251Bytes(fnsName),
                                                                 out errCode, 
                                                                 ref errBuf, 
                                                                 errBufLength
                                                                )).ConfigureAwait(false);

      return (result, EncodingUtil.Win1251BytesToUtf8(errBuf));
    }

    public async Task<(bool, string)> ReserveRoleSwapIsAvailable(string encodedCredentials, string pipeName)
    {
      return await SwapFnSrvRole(encodedCredentials, pipeName, true).ConfigureAwait(false);
    }
   
    public async Task<(bool, string)> SwapReserveRole(string encodedCredentials, string pipeName)
    {
      return await SwapFnSrvRole(encodedCredentials, pipeName, false).ConfigureAwait(false);
    }


    public async Task<(bool, int, string)> AddPasswordToAutoBackupDigest(string password)
    {
      const int responseMsgBufLength = 1000;
      var       responseMsgBuf       = new byte[responseMsgBufLength];
      uint      responseCode      = 0;

      var result = await Task.Run(() => _native.CfsIfpcSetAbkParms(CfId,
                                                                   EncodingUtil.Utf8ToWin1251Bytes(password),
                                                                   out responseCode,
                                                                   ref responseMsgBuf,
                                                                   responseMsgBufLength)).ConfigureAwait(false);

      return (result, (int) responseCode, EncodingUtil.Win1251BytesToUtf8(responseMsgBuf));
    }
  }
}