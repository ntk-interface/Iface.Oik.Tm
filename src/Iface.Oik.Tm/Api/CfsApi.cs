using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Dto;
using static Iface.Oik.Tm.Native.Interfaces.TmNativeDefs;

namespace Iface.Oik.Tm.Api
{
  public class CfsApi : ICfsApi
  {
    public nint   CfId { get; private set; }
    public string Host { get; private set; }


    public void SetCfIdAndHost(IntPtr cfId, string host)
    {
      CfId = cfId;
      Host = host;
    }

    public string MakeInprocCrd(string machine, string user, string pwd)
    {
      var ptr = TmNative.cfsMakeInprocCrd(EncodingUtil.StringToBytes(machine),
                                          EncodingUtil.StringToBytes(user),
                                          EncodingUtil.StringToBytes(pwd));
      if (ptr != IntPtr.Zero)
      {
        string res = TmNativeUtil.GetCStringFromIntPtr(ptr);
        TmNative.cfsFreeMemory(ptr);
        return res;
      }
      else
      {
        return string.Empty;
      }
    }

    public async Task<(nint, DateTime)> OpenConfigurationTree(string fileName)
    {
      return await Task.Run(() => TmNativeApi.OpenConfigurationTree(CfId, Host, fileName)).ConfigureAwait(false);
    }


    public async Task<(nint, DateTime)> OpenMasterServiceConfiguration()
    {
      return await OpenConfigurationTree(MasterConfFile).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<CfTreeNode>> GetReserveConfiguration()
    {
      var resTree = new List<CfTreeNode>();

      var (resHandle, _) = await OpenConfigurationTree(HotStanbyConfFile).ConfigureAwait(false);

      if (resHandle == nint.Zero)
      {
        return resTree;
      }

      resTree = await GetCfTree(resHandle).ConfigureAwait(false);
      TmNativeApi.FreeTreeHandle(resHandle);
      return resTree;
    }


    public async Task<ReserveServerState> ReserveServerTypeIsWorking(CfTreeNode reserveConfNode)
    {
      if (!reserveConfNode.CfProperties.TryGetValue("Type", out var type) || type.Trim() != "1")
      {
        return new ReserveServerState { IsWorking = false };
      }

      if (!reserveConfNode.CfProperties.TryGetValue("Addr", out var sAddr))
      {
        return new ReserveServerState { IsWorking = false };
      }

      var ipAddr = TmNativeUtil.IpAddrToNativeDword(sAddr);

      if (ipAddr == 0)
      {
        return new ReserveServerState { IsWorking = false };
      }


      if (!reserveConfNode.CfProperties.TryGetValue("Port", out var sPort)
          || !ushort.TryParse(sPort, out var port)
          || port == 0
          || port > 0xfff)
      {
        return new ReserveServerState { IsWorking = false };
      }

      var bPort = port;

      if (reserveConfNode.CfProperties.TryGetValue("BPort", out var sBPort)
          && ushort.TryParse(sBPort, out var nBPort))
      {
        bPort = nBPort;

        if (bPort == 0 || bPort > 0xfff)
        {
          return new ReserveServerState { IsWorking = false };
        }
      }

      var split = reserveConfNode.Name.Split(':');

      if (split.Length != 2)
      {
        return new ReserveServerState { IsWorking = false };
      }

      var bCastSignature = await GetReserveServerBroadcastSignature(split.First()).ConfigureAwait(false);

      if (bCastSignature is BroadcastServerSignature.None)
      {
        return new ReserveServerState { IsWorking = false };
      }

      return await ReserveServerTypeIsWorking(ipAddr, bPort, port, bCastSignature).ConfigureAwait(false);
    }


    public async Task<ReserveServerState> ReserveServerTypeIsWorking(uint                     ipAddrDword,
                                                                     ushort                   bPort,
                                                                     ushort                   port,
                                                                     BroadcastServerSignature bCastSignature)
    {
      return await Task.Run(() => ReserveServerTypeIsWorkingSync(ipAddrDword, bPort, port, bCastSignature))
                       .ConfigureAwait(false);
    }


    public ReserveServerState ReserveServerTypeIsWorkingSync(uint                     ipAddrDword,
                                                             ushort                   bPort,
                                                             ushort                   port,
                                                             BroadcastServerSignature bCastSignature)
    {
      const int reserveServerNameBufSize = 64;
      const int errBufSize               = 2048;

      Span<byte> reserveServerNameBuf = stackalloc byte[reserveServerNameBufSize];
      Span<byte> errBuf               = stackalloc byte[errBufSize];

      var result = TmNative.cfsIsReserveWorking(CfId,
                                                ipAddrDword,
                                                bPort,
                                                port,
                                                (uint)bCastSignature,
                                                out bool isWorking,
                                                reserveServerNameBuf,
                                                out uint errCode,
                                                errBuf,
                                                errBufSize);

      if (errCode != 0 || reserveServerNameBuf.Length == 0)
      {
        return new ReserveServerState { IsWorking = false };
      }

      return new ReserveServerState
      {
        IsWorking      = isWorking && result,
        RemotePipeName = EncodingUtil.BytesToString(reserveServerNameBuf),
        Signature      = bCastSignature
      };
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

      TmNativeApi.FreeTreeHandle(handle);

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
            || node.Children == null || node.Properties is not ChildNodeProperties tmsP)
        {
          continue;
        }

        foreach (var child in node.Children)
        {
          if (child.Properties is not TmCalcNodeProperties calcP)
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

        if (rbsHandle == nint.Zero)
        {
          return;
        }

        var rbsTree = await GetCfTree(rbsHandle).ConfigureAwait(false);

        TmNativeApi.FreeTreeHandle(rbsHandle);

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

      var (calcHandle, _) = await OpenConfigurationTree(Path.Combine(TmsDirectory, pipeName, TmCalcConfFile))
                              .ConfigureAwait(false);

      if (calcHandle == nint.Zero)
      {
        return;
      }

      var calcTree = await GetCfTree(calcHandle).ConfigureAwait(false);

      TmNativeApi.FreeTreeHandle(calcHandle);

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


    public async Task SaveFullMSTree(MSTreeNode msRoot)
    {
      // Нормализуем имена Pipe для дочерних компонент под серверами, убираем дублирование если есть
      var knownPipes = new List<string>();

      foreach (var server in msRoot.Children)
      {
        if (server.Properties is not ChildNodeProperties p)
        {
          continue;
        }

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

        var i = 1;
        while (knownPipes.Contains(p.PipeName))
        {
          p.PipeName = $"{p.PipeName}{i}";
          i++;
        }

        knownPipes.Add(p.PipeName);
        if (server.Children == null)
        {
          continue;
        }

        foreach (var child in server.Children)
        {
          if (child.Properties is ChildNodeProperties chP)
          {
            chP.PipeName = p.PipeName;
          }
        }
      }

      // Основное дерево мастер-сервиса
      var treeHandle = CreateNewMasterServiceTree(msRoot);
      await SaveMasterServiceConfiguration(treeHandle).ConfigureAwait(false);
      TmNativeApi.FreeTreeHandle(treeHandle);

      // Конфигурация резервирования, перебираем сервера на втором уровне
      var resHandle = TmNativeApi.CreateNewTree();
      foreach (var server in msRoot.Children)
      {
        if (server.Properties is not ReservedNodeProperties p)
        {
          continue;
        }

        var tag = p.PipeName;
        if (server.ProgName.Equals(MSTreeConsts.rbsrv_old) || server.ProgName.Equals(MSTreeConsts.pcsrv_old))
        {
          tag = $"{MSTreeConsts.gensrv}:{tag}";
        }
        else
        {
          tag = $"{server.ProgName}:{tag}";
        }

        var nodeHandle = TmNativeApi.CreateChildNode(resHandle, tag);

        foreach (var (name, value) in p.ReservePropertyPairs)
        {
          TmNativeApi.SetNodeProperty(nodeHandle, name, value);
        }
      }

      await SaveConfigurationTree(resHandle, HotStanbyConfFile).ConfigureAwait(false);
      TmNativeApi.FreeTreeHandle(resHandle);

      // Другие конфигурации (rb, tmcalc)
      foreach (var server in msRoot.Children)
      {
        if (server.Properties is RbsNodeProperties rbsP)
        {
          // общие параметры
          var rbsHandle  = TmNativeApi.CreateNewTree();
          var nodeHandle = TmNativeApi.CreateChildNode(rbsHandle, MSTreeConsts.RBS_Parameters);

          TmNativeApi.SetNodeProperty(nodeHandle, nameof(rbsP.RBF_Directory), rbsP.RBF_Directory);

          nodeHandle = TmNativeApi.CreateChildNode(rbsHandle, MSTreeConsts.RBS_ClientParms);

          TmNativeApi.SetNodeProperty(nodeHandle, nameof(rbsP.DOC_Path),     rbsP.DOC_Path);
          TmNativeApi.SetNodeProperty(nodeHandle, nameof(rbsP.DTMX_SQLCS),   rbsP.DTMX_SQLCS);
          TmNativeApi.SetNodeProperty(nodeHandle, nameof(rbsP.JournalSQLCS), rbsP.JournalSQLCS);

          nodeHandle = TmNativeApi.CreateChildNode(rbsHandle, MSTreeConsts.RBS_PGParms);
          TmNativeApi.SetNodeProperty(nodeHandle, nameof(rbsP.BinPath),  rbsP.BinPath);
          TmNativeApi.SetNodeProperty(nodeHandle, nameof(rbsP.DataPath), rbsP.DataPath);
          await SaveConfigurationTree(rbsHandle,
                                      $"{RbsDirectory}\\{rbsP.PipeName}\\{RbsConfFile}")
            .ConfigureAwait(false);
          TmNativeApi.FreeTreeHandle(rbsHandle);

          // параметры редиректора
          await SetRedirectorPort(rbsP.PipeName, 0, rbsP.RedirectorPort).ConfigureAwait(false);
        }
        else if (server.ProgName.Equals(MSTreeConsts.TmServer) || server.ProgName.Equals(MSTreeConsts.pcsrv_old))
        {
          if (server.Children == null || server.Properties is not ChildNodeProperties tmsP)
          {
            continue;
          }

          foreach (var child in server.Children)
          {
            if (child.Properties is not TmCalcNodeProperties calcP)
            {
              continue;
            }

            // читаем конфигурацию дорасчётчика если есть
            var fileName = $"{TmsDirectory}\\{tmsP.PipeName}\\{TmCalcConfFile}";

            var (calcHandle, _) = await OpenConfigurationTree(fileName).ConfigureAwait(false);

            if (calcHandle == nint.Zero)
            {
              return;
            }

            var calcTree = await GetCfTree(calcHandle).ConfigureAwait(false);
            TmNativeApi.FreeTreeHandle(calcHandle);

            calcTree ??= new List<CfTreeNode>();

            var fUnr = calcTree.Find(n => n.Name.Equals(MSTreeConsts.Tmcalc_FUnr));
            if (fUnr == null)
            {
              fUnr = new CfTreeNode(MSTreeConsts.Tmcalc_FUnr);
              calcTree.Add(fUnr);
            }

            fUnr.CfProperties = new Dictionary<string, string>
            {
              [MSTreeConsts.Tmcalc_Value] = calcP.FUnr ? "+" : "-"
            };

            var sRel = calcTree.Find(n => n.Name.Equals(MSTreeConsts.Tmcalc_SRel));
            if (sRel == null)
            {
              sRel = new CfTreeNode(MSTreeConsts.Tmcalc_SRel);
              calcTree.Add(sRel);
            }

            sRel.CfProperties = new Dictionary<string, string>
            {
              [MSTreeConsts.Tmcalc_Value] = calcP.SRel ? "+" : "-"
            };

            calcHandle = CreateConfigurationTree(calcTree);
            if (calcHandle != nint.Zero)
            {
              await SaveConfigurationTree(calcHandle, fileName).ConfigureAwait(false);
              TmNativeApi.FreeTreeHandle(calcHandle);
            }

            break;
          }
        }
      }
    }


    public async Task SaveConfigurationTree(nint treeHandle, string filename)
    {
      await Task.Run(() => TmNativeApi.SaveConfigurationTree(treeHandle, Host, filename))
                .ConfigureAwait(false);
    }

    public async Task SaveMasterServiceConfiguration(IntPtr treeHandle)
    {
      await SaveConfigurationTree(treeHandle, MasterConfFile).ConfigureAwait(false);
    }

    public void FreeConfigurationTreeHandle(nint handle)
    {
      TmNativeApi.FreeTreeHandle(handle);
    }

    private static DateTime GetDateTimeFromCustomFileTime(FileTime fileTime)
    {
      return DateTime.FromFileTime((long)fileTime.dwHighDateTime << 32 | (uint)fileTime.dwLowDateTime);
    }


    public async Task<List<CfTreeNode>> GetCfTree(nint rootHandle, CfTreeNode parent = null)
    {
      return await Task.Run(() => GetNodeChildren(rootHandle, parent)).ConfigureAwait(false);
    }

    private static List<CfTreeNode> GetNodeChildren(nint parentHandle, CfTreeNode parent = null)
    {
      var children = new List<CfTreeNode>();

      var childHandle = nint.Zero;

      for (var i = 0;; i++)
      {
        childHandle = i == 0
                        ? TmNativeApi.NodeEnumAll(parentHandle, 0)
                        : TmNativeApi.NodeGetNextAll(childHandle);

        if (childHandle == nint.Zero)
        {
          break;
        }

        var nodeChild = new CfTreeNode(TmNativeApi.GetNodeName(childHandle), parent)
        {
          Disabled     = !TmNativeApi.NodeIsEnabled(childHandle),
          CfProperties = GetNodeProps(childHandle),
        };

        nodeChild.Children = GetNodeChildren(childHandle, nodeChild);
        children.Add(nodeChild);
      }

      return children.Count == 0
               ? null
               : children;
    }

    private static Dictionary<string, string> GetNodeProps(nint nodeHandle)
    {
      var props = new Dictionary<string, string>();

      for (var i = 0;; i++)
      {
        var propName = TmNativeApi.GetNodePropertyName(nodeHandle, i);

        if (propName == string.Empty)
        {
          break;
        }

        var propValue = TmNativeApi.GetNodePropertyValue(nodeHandle, propName);

        props.Add(propName, propValue);
      }

      return props;
    }


    private nint CreateNewMasterServiceTree(MSTreeNode msRoot)
    {
      var newTreeHandle = TmNativeApi.CreateNewTree();

      CreateMSNode(newTreeHandle, msRoot);

      return newTreeHandle;
    }

    public nint CreateConfigurationTree(IEnumerable<CfTreeNode> tree)
    {
      var newTreeHandle = TmNativeApi.CreateNewTree();
      foreach (var node in tree)
      {
        CreateCfgNode(newTreeHandle, node);
      }

      return newTreeHandle;
    }

    private static void CreateCfgNode(nint parentNodeHandle, CfTreeNode node)
    {
      var nodeHandle = TmNativeApi.CreateChildNode(parentNodeHandle, node.Name);
      TmNativeApi.SetNodeEnabledState(nodeHandle, !node.Disabled);

      if (node.CfProperties != null && node.CfProperties.Count != 0)
      {
        foreach (var prop in node.CfProperties)
        {
          TmNativeApi.SetNodeProperty(nodeHandle, prop.Key, prop.Value);
        }
      }

      if (node.Children == null || node.Children.Count == 0)
      {
        return;
      }

      foreach (var child in node.Children)
      {
        CreateCfgNode(nodeHandle, child);
      }
    }

    private void CreateMSNode(nint parentNodeHandle, MSTreeNode node, int tagId = -1)
    {
      var tag        = tagId == -1 ? "Master" : $"#{tagId:X3}";
      var nodeHandle = TmNativeApi.CreateChildNode(parentNodeHandle, tag);

      if (!CreateMSNodeProperties(nodeHandle, node))
        throw new Exception("Ошибка заполнения дерева конфигурации");

      if (node.Children == null)
      {
        return;
      }

      var i = 0;
      foreach (var childNode in node.Children)
      {
        CreateMSNode(nodeHandle, childNode, i);
        i++;
      }
    }

    private bool CreateMSNodeProperties(nint nodeHandle, MSTreeNode node)
    {
      if (node.ProgName.Equals(MSTreeConsts.rbsrv_old) || node.ProgName.Equals(MSTreeConsts.pcsrv_old))
      {
        if (!TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.ProgName, MSTreeConsts.gensrv))
          return false;

        if (!TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.TaskPath, node.ProgName))
          return false;
      }
      else
      {
        if (!TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.ProgName, node.ProgName))
          return false;
      }

      if (node.Properties is MSTreeNodeProperties props && props.NoStart)
      {
        if (!TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.NoStart, "1")) return false;
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

    private static bool CreateMasterNodeProperties(nint nodeHandle, MSTreeNode node)
    {
      var props = (MasterNodeProperties)node.Properties;

      if (!TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.LogFileSize, props.LogFileSize.ToString()))
      {
        return false;
      }

      return !node.ProgName.Equals(MSTreeConsts.Portcore) ||
             TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.InstallationName, props.InstallationName);
    }

    private static bool CreateChildNodeProperties(nint nodeHandle, MSTreeNode node)
    {
      var props = (ChildNodeProperties)node.Properties;

      return TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.PipeName, props.PipeName);
    }

    private static bool CreateNewTmsNodeProperties(nint nodeHandle, MSTreeNode node)
    {
      var props = (NewTmsNodeProperties)node.Properties;

      if (!CreateChildNodeProperties(nodeHandle, node))
      {
        return false;
      }

      return props.PassiveMode ||
             TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.PassiveMode,
                                         Convert.ToInt32(props.PassiveMode).ToString());
    }

    private static bool CreateExternalTaskNodeProperties(nint nodeHandle, MSTreeNode node)
    {
      var props = (ExternalTaskNodeProperties)node.Properties;

      if (!CreateChildNodeProperties(nodeHandle, node))
      {
        return false;
      }

      // Зачем то в пути внешней задачи пробелы замеяются на табуляции
      if (!TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.TaskPath, props.TaskPath.Replace(' ', '\t')))
      {
        return false;
      }

      if (!TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.TaskArguments, props.TaskArguments))
      {
        return false;
      }

      if (!TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.ConfFilePath, props.ConfigurationFilePath))
      {
        return false;
      }

      return true;
    }

    private static bool CreateAutoBackupNodeProperties(nint                 nodeHandle,
                                                       AutoBackupProperties properties)
    {
      if (!TmNativeApi.SetNodeProperty(nodeHandle,
                                       MSTreeConsts.ExecutionHour,
                                       $"{properties.ExecutionHour}"))
      {
        return false;
      }

      if (!TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.BackupDirectory, properties.BackupsDirectory))
      {
        return false;
      }

      if (!TmNativeApi.SetNodeProperty(nodeHandle, MSTreeConsts.ExcludeArchives,
                                       properties.ExcludeArchives ? "1" : "0"))
      {
        return false;
      }

      return true;
    }

    public async Task<CfsDefs.SoftwareTypes> GetSoftwareType()
    {
      var result = await Task.Run(() => TmNative.cfsGetSoftwareType(CfId))
                             .ConfigureAwait(false);

      return result switch
             {
               48 => CfsDefs.SoftwareTypes.Old,
               49 => CfsDefs.SoftwareTypes.Version3,
               _  => CfsDefs.SoftwareTypes.Unknown
             };
    }

    public async Task<CfsDefs.MasterServiceStatus> MasterServiceStatus()
    {
      var result = await Task.Run(() => TmNative.cfsIfpcMaster(CfId, TmNativeDefs.MasterServiceStatusCommand))
                             .ConfigureAwait(false);

      return result switch
             {
               1 => CfsDefs.MasterServiceStatus.Stopped,
               2 => CfsDefs.MasterServiceStatus.Running,
               _ => CfsDefs.MasterServiceStatus.LostConnection
             };
    }

    public async Task StartMasterService()
    {
      await Task.Run(() => TmNative.cfsIfpcMaster(CfId, TmNativeDefs.StartMasterServiceCommand))
                .ConfigureAwait(false);
    }

    public async Task StopMasterService()
    {
      await Task.Run(() => TmNative.cfsIfpcMaster(CfId, TmNativeDefs.StopMasterServiceCommand))
                .ConfigureAwait(false);
    }

    public async Task<bool> IsConnected()
    {
      return await Task.Run(() => TmNative.cfsIsConnected(CfId))
                       .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<string>> GetTimezones()
    {
      const int errStringLength = 1000;
      var       errBuf          = new byte[errStringLength];
      uint      errCode         = 0;

      var timezonesIdsPointer = await Task.Run(() => TmNative.cfsEnumTimezones(CfId,
                                                                               out errCode,
                                                                               errBuf,
                                                                               errStringLength))
                                          .ConfigureAwait(false);

      var timezonesIds = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(timezonesIdsPointer, 1000);

      TmNative.cfsFreeMemory(timezonesIdsPointer);
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
      return await Task.Run(() => TmNativeApi.GetTmServers<TmServer, TmUser>(CfId))
                       .ConfigureAwait(false);
    }


    public async Task<IReadOnlyCollection<TmServerLogRecord>> GetTmServersLog(int       maxRecords,
                                                                              DateTime? startTime,
                                                                              DateTime? endTime)
    {
      return await Task.Run(() => TmNativeApi.GetTmServersLog<TmServerLogRecord>(CfId, startTime, endTime, maxRecords))
                       .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<TmServerThread>> GetTmServersThreads()
    {
      return await Task.Run(() => TmNativeApi.GetTmServersThreads<TmServerThread>(CfId))
                       .ConfigureAwait(false);
    }

    public async Task RegisterTmServerTracer(ITmServerTraceable traceTarget, bool debug, int pause)
    {
      await Task.Run(() => TmNativeApi.RegisterTmServerTracer(CfId,
                                                              traceTarget.ProcessId,
                                                              traceTarget.ThreadId,
                                                              debug,
                                                              pause)).ConfigureAwait(false);
    }

    public async Task StopTmServerTrace()
    {
      await Task.Run(() => TmNativeApi.StopTmServerTrace(CfId))
                .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<TmServerLogRecord>> TraceTmServerLogRecords()
    {
      return await Task.Run(() => TmNativeApi.TraceTmServerLogRecords<TmServerLogRecord>(CfId))
                       .ConfigureAwait(false);
    }


    public async Task<TmInstallationInfo> GetTmInstallationInfo()
    {
      var (isIntact, integrityCheckMessage) =
        await CheckInstallationIntegrity(CfsIitgk.Exe).ConfigureAwait(false);

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

    private async Task<CfsFileProperties?> GetFileProperties(string filePath)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      var       fileProps    = new CfsFileProperties();

      var result =
        await Task.Run(() => TmNative.cfsFileGetPropreties(CfId,
                                                           EncodingUtil.StringToBytes(filePath),
                                                           ref fileProps,
                                                           out errCode,
                                                           errBuf,
                                                           errBufLength))
                  .ConfigureAwait(false);


      if (!result) return null;

      return fileProps;
    }

    public async Task<(bool, string)> CheckInstallationIntegrity(CfsIitgk kind)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var signaturePointer = new IntPtr();
      var errorsPointer    = new IntPtr();

      var result =
        await Task.Run(() => TmNative.cfsCheckInstallationIntegrity(CfId,
                                                                    (uint)kind,
                                                                    out signaturePointer,
                                                                    out errorsPointer,
                                                                    out errCode,
                                                                    errBuf,
                                                                    errBufLength))
                  .ConfigureAwait(false);

      if (!result)
      {
        throw new
          Exception($"Ошибка проверки целостности сервера: {EncodingUtil.BytesToString(errBuf)} Код: {errCode}");
      }

      var signature = $"Корневая сигнатура:{TmNativeUtil.GetCStringFromIntPtr(signaturePointer)}";
      var errors    = TmNativeUtil.GetCStringFromIntPtr(errorsPointer);
      TmNative.cfsFreeMemory(signaturePointer);
      TmNative.cfsFreeMemory(errorsPointer);

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


      if (!await Task.Run(() => TmNative.cfsDirEnum(CfId,
                                                    EncodingUtil.StringToBytes(path),
                                                    buf,
                                                    bufLength,
                                                    out errCode,
                                                    errBuf,
                                                    errBufLength))
                     .ConfigureAwait(false))
      {
        Console.WriteLine($"Ошибка при запросе списка файлов: {errCode} - {EncodingUtil.BytesToString(errBuf)}");
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

      if (!await Task.Run(() => TmNative.cfsFilePut(CfId,
                                                    EncodingUtil.StringToBytes(remoteFilePath),
                                                    EncodingUtil.StringToBytes(localFilePath),
                                                    timeout | TmNativeDefs.FailIfNoConnect,
                                                    out errCode,
                                                    errBuf,
                                                    errBufLength)).ConfigureAwait(false))
      {
        return (false, $"Ошибка при отправке файла: {errCode} - {EncodingUtil.BytesToString(errBuf)}");
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

      var       fileTime        = new FileTime();
      const int errStringLength = 1000;
      var       errString       = new byte[errStringLength];
      uint      errCode         = 0;
      if (!await Task.Run(() => TmNative.cfsFileGet(CfId,
                                                    EncodingUtil.StringToBytes(remoteFilePath),
                                                    EncodingUtil.StringToBytes(localFilePath),
                                                    timeout | FailIfNoConnect,
                                                    ref fileTime,
                                                    out errCode,
                                                    errString,
                                                    errStringLength))
                     .ConfigureAwait(false))
      {
        return (false, $"Ошибка при скачивании файла: {errCode} - {EncodingUtil.BytesToString(errString)}",
                DateTime.MinValue);
      }

      if (File.Exists(localFilePath))
      {
        return (true, string.Empty, GetDateTimeFromCustomFileTime(fileTime));
      }

      Console.WriteLine("Ошибка при сохранении файла в файловую систему");
      return (false, "Ошибка при сохранении файла в файловую систему", DateTime.MinValue);
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

      if (!await Task.Run(() => TmNative.cfsFileDelete(CfId, EncodingUtil.StringToBytes(remoteFilePath),
                                                       out errCode, errBuf, errBufLength))
                     .ConfigureAwait(false))
      {
        Console.WriteLine($"Ошибка при удалении файла: {errCode} - {EncodingUtil.BytesToString(errBuf)}");
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
      return await Task.Run(GetBasePathSync)
                       .ConfigureAwait(false);
    }


    public string GetBasePathSync()
    {
      const int  basePathBufLength = 1000;
      Span<byte> basePathBuf       = stackalloc byte[basePathBufLength];

      const int  errBufLength = 1000;
      Span<byte> errBuf       = stackalloc byte[errBufLength];

      var result = TmNative.cfsGetBasePath(CfId,
                                           basePathBuf,
                                           basePathBufLength,
                                           out uint errCode,
                                           errBuf,
                                           errBufLength);


      if (!result)
      {
        throw new
          Exception($"Ошибка получения базового пути сервера сервера: {EncodingUtil.BytesToString(errBuf)} Код: {errCode}");
      }

      return EncodingUtil.BytesToString(basePathBuf);
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
      return await Task.Run(() => GetIniStringSync(path, section, key, def, bufSize))
                       .ConfigureAwait(false);
    }


    private string GetIniStringSync(string path,
                                    string section,
                                    string key,
                                    string def,
                                    uint   bufSize)
    {
      Span<byte> buf = stackalloc byte[(int)bufSize];

      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];

      var result = TmNative.cfsGetIniString(CfId,
                                            EncodingUtil.StringToBytes(path),
                                            EncodingUtil.StringToBytes(section),
                                            EncodingUtil.StringToBytes(key),
                                            EncodingUtil.StringToBytes(def),
                                            buf,
                                            out bufSize,
                                            out var errCode,
                                            errBuf,
                                            errBufLength);

      if (!result)
      {
        throw new
          Exception($"Ошибка получения ini-строки. \nПуть: {path}\nСекция: {section}\nКлюч: {key}\nОшибка: {EncodingUtil.BytesToString(errBuf)} Код: {errCode}");
      }


      return EncodingUtil.BytesToString(buf, TmNativeUtil.DetectEncoding(buf));
    }


    private async Task SetIniString(string path,
                                    string section,
                                    string key,
                                    string value)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = await Task.Run(() => TmNative.cfsSetIniString(CfId,
                                                                 EncodingUtil.StringToBytes(path),
                                                                 EncodingUtil.StringToBytes(section),
                                                                 EncodingUtil.StringToBytes(key),
                                                                 EncodingUtil.StringToBytes(value),
                                                                 out errCode,
                                                                 errBuf,
                                                                 errBufLength))
                             .ConfigureAwait(false);

      if (!result)
      {
        throw new
          Exception($"Ошибка записи ini-строки. \nПуть: {path}\nСекция: {section}\nКлюч: {key}\nЗначение: {value}\nОшибка: {EncodingUtil.BytesToString(errBuf)} Код: {errCode}");
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

      var sLogHandle = await Task.Run(() => TmNative.cfsSlogOpen(CfId,
                                                                 (uint)logType,
                                                                 startIndex,
                                                                 (uint)direction,
                                                                 out errCode,
                                                                 errBuf,
                                                                 errBufLength)).ConfigureAwait(false);

      if (sLogHandle == 0)
      {
        throw new
          Exception($"Ошибка открытия журнала безопасности. \nТип: ${logType}\nОшибка: {EncodingUtil.BytesToString(errBuf)} Код: {errCode}");
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


      var strPtr = await Task.Run(() => TmNative.cfsSlogReadRecords(CfId,
                                                                    sLogHandle,
                                                                    out errCode,
                                                                    errBuf,
                                                                    errBufLength)).ConfigureAwait(false);

      if (strPtr == nint.Zero)
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
        (var strings, nextPtr) = TmNativeUtil.GetStringsListWithOffsetPointer(nextPtr);

        if (strings.Count == 0)
        {
          break;
        }

        var record = SLogRecord.CreateFromStringsList(strings);


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


      TmNative.cfsFreeMemory(strPtr);

      return (logPart, shouldContinue);
    }


    public async Task<bool> CloseSLog(ulong sLogHandle)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      return await Task.Run(() => TmNative.cfsSlogClose(CfId,
                                                        sLogHandle,
                                                        out errCode,
                                                        errBuf,
                                                        errBufLength)).ConfigureAwait(false);
    }


    public async Task<int> GetRedirectorPort(string pipeName, int portIndex)
    {
      var portBinData = await GetBin(".cfs.", $"rbs${pipeName}", $"ipg_port{portIndex}")
                          .ConfigureAwait(false);

      if (!int.TryParse(EncodingUtil.BytesToString(portBinData), out var port))
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

      var stateString = EncodingUtil.BytesToString(binData);

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

      var resultPtr = await Task.Run(() => TmNative.cfsIfpcGetBin(CfId,
                                                                  uName,
                                                                  oName,
                                                                  binName,
                                                                  out binLength,
                                                                  out errCode,
                                                                  errBuf,
                                                                  errBufLength))
                                .ConfigureAwait(false);

      if (errCode != 0)
      {
        return (null, errCode, EncodingUtil.BytesToString(errBuf));
      }

      var binData = new byte[binLength];

      Marshal.Copy(resultPtr, binData, 0, binData.Length);
      // не забываем освобождать память, возвращённую из библиотеки
      TmNative.cfsFreeMemory(resultPtr);

      return (binData, 0, string.Empty);
    }


    public async Task SetRedirectorPort(string pipeName, int portIndex, int port)
    {
      await Task.Run(() => TmNativeApi.SetRedirectorPort(CfId, pipeName, portIndex, port))
                .ConfigureAwait(false);
    }

    public AccessMasksDescriptor SecGetAccessDescriptor(string sSetupPath, string progName)
    {
      return TmNativeApi.SecGetAccessDescriptor<AccessMasksDescriptor, AccessMask>(sSetupPath, progName);
    }

    public ExtendedRightsDescriptor SecGetExtendedRightsDescriptor(string sSetupPath)
    {
      return TmNativeApi.SecGetExtendedRightsDescriptor<ExtendedRightsDescriptor, ExtendedRight>(sSetupPath);
    }

    public async Task<IReadOnlyCollection<string>> GetOikUsersStrings()
    {
      return await Task.Run(() => TmNativeApi.SecEnumUsers(CfId))
                       .ConfigureAwait(false);
    }

    public async Task<(IReadOnlyCollection<string>, uint, string)> SecEnumOSUsers()
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      // сделаем отдельное соединение чтобы не занимать долгой выборкой основное
      IntPtr temp_cfsid =
        await Task.Run(() => TmNative.cfsConnect(EncodingUtil.StringToBytes(Host), out errCode, errBuf,
                                                 errBufLength)).ConfigureAwait(false);
      if ((temp_cfsid == IntPtr.Zero) && (errCode != 0))
      {
        return (null, errCode, EncodingUtil.BytesToString(errBuf));
      }

      var resultPtr =
        await Task.Run(() => TmNative.cfsIfpcEnumOSUsers(temp_cfsid, out errCode, errBuf, errBufLength))
                  .ConfigureAwait(false);
      TmNative.cfsDisconnect(temp_cfsid);
      if (errCode != 0)
      {
        return (null, errCode, EncodingUtil.BytesToString(errBuf));
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
        await Task.Run(() => TmNative.cfsIfpcSetUserPwd(CfId, EncodingUtil.StringToBytes(username),
                                                        EncodingUtil.StringToBytes(password), out errCode,
                                                        errBuf, errBufLength)).ConfigureAwait(false);
      if (errCode != 0)
      {
        return (errCode, EncodingUtil.BytesToString(errBuf));
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
        await Task.Run(() => TmNative.cfsIfpcDeleteUser(CfId, EncodingUtil.StringToBytes(username), out errCode,
                                                        errBuf, errBufLength)).ConfigureAwait(false);
      if (errCode != 0)
      {
        return (errCode, EncodingUtil.BytesToString(errBuf));
      }
      else
      {
        return (0, string.Empty);
      }
    }

    public async Task<uint> SecGetAccessMask(string username, string oName)
    {
      return await Task.Run(() => TmNativeApi.SecGetAccessMask(CfId, username, oName))
                       .ConfigureAwait(false);
    }

    public async Task SecSetAccessMask(string username, string oName, uint accessMask)
    {
      await Task.Run(() => TmNativeApi.SecSetAccessMask(CfId, username, oName, accessMask))
                .ConfigureAwait(false);
    }

    public async Task<ExtendedUserData> SecGetExtendedUserData(string serverType,
                                                               string serverName,
                                                               string username)
    {
      return await Task.Run(() => TmNativeApi.SecGetExtendedUserData<ExtendedUserData>(CfId,
                              serverType,
                              serverName,
                              username))
                       .ConfigureAwait(false);
    }

    public async Task SecSetExtendedUserData(string           serverType,
                                             string           serverName,
                                             string           username,
                                             ExtendedUserData extendedUserData)
    {
      await Task.Run(() => TmNativeApi.SecSetExtendedUserData(CfId, serverType, serverName, username, extendedUserData))
                .ConfigureAwait(false);
    }

    public async Task<UserPolicy> SecGetUserPolicy(string username)
    {
      return await Task.Run(() => TmNativeApi.SecGetUserPolicy<UserPolicy>(CfId, username))
                       .ConfigureAwait(false);
    }

    public async Task SecSetUserPolicy(string username, UserPolicy userPolicy)
    {
      await Task.Run(() => TmNativeApi.SecSetUserPolicy(CfId, username, userPolicy))
                .ConfigureAwait(false);
    }

    public async Task<PasswordPolicy> SecGetPasswordPolicy()
    {
      return await Task.Run(() => TmNativeApi.SecGetPasswordPolicy<PasswordPolicy>(CfId))
                       .ConfigureAwait(false);
    }

    public async Task SecSetPasswordPolicy(PasswordPolicy passwordPolicy)
    {
      await Task.Run(() => TmNativeApi.SecSetPasswordPolicy(CfId, passwordPolicy))
                .ConfigureAwait(false);
    }

    public async Task<ComputerInfo> GetComputerInfo()
    {
      var dto = await Task.Run(() => TmNativeApi.GetServerComputerInfo(CfId))
                          .ConfigureAwait(false);

      var computerInfo = new ComputerInfo
      {
        ComputerName      = dto.ComputerName,
        PrimaryDomainName = dto.DomainInfo.PrimaryDomainName,
        OS_ProductType    = $"ostype{dto.NtProductType}",
        OS_Version        = $"{dto.NtVerMaj}.{dto.NtVerMin} build {dto.NtBuild}",
        Architecture      = dto.Win64 ? "x64" : "x86",
        Acp               = dto.Acp,

        ServerTimeGMT = DateUtil.GetDateTimeFromTimestamp(dto.CurrentGMT, dto.CurrentMs),
        Uptime        = (ulong)dto.Uptime,

        Copyright = $"oiktype{dto.Copyright}",
        CfsVer    = $"{dto.CfsVerMaj}.{dto.CfsVerMin}",

        UserName   = dto.UserName,
        UserAddr   = dto.UserAddr,
        AccessMask = dto.AccessMask,
        IpAddrs    = new List<string>()
      };
      foreach (var addr in dto.IpAddrs)
      {
        if (addr == 0)
          break;
        computerInfo.IpAddrs.Add(new IPAddress(addr).ToString());
      }

      computerInfo.SoftwareKeyID = BitConverter.ToString(dto.SoftwareKeyOctets).Replace("-", "");

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


      return computerInfo;
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

      var result = await Task.Run(() => TmNative.cfsSaveMachineConfig(full,
                                                                      EncodingUtil.StringToBytes(Host),
                                                                      EncodingUtil.StringToBytes(Path.Combine(directory,
                                                                        fileName)),
                                                                      errBuf,
                                                                      errBufLength))
                             .ConfigureAwait(false);
      if (result != true)
      {
        return (false, EncodingUtil.BytesToString(errBuf));
      }
      else
      {
        return (true, string.Empty);
      }
    }

    public async Task<(bool, string)> SaveMachineConfigEx(string           directory,
                                                          uint             scope,
                                                          TmNativeCallback callback          = null,
                                                          nint             callbackParameter = default)
    {
      return await Task.Run(() => TmNativeApi.SaveMachineConfigEx(Host,
                                                                  directory,
                                                                  scope,
                                                                  callback,
                                                                  callbackParameter))
                       .ConfigureAwait(false);

      // const int errBufLength = 1000;
      // var       errBuf       = new byte[errBufLength];
      // string    fileName     = "undefined";
      // if (scope == 0) //#define CFS_SMC_DEV		0
      // {
      //   fileName = "MasterConf-" + DateTime.Now.ToString(BackupDateFormat) + ".pkf";
      // }
      // else if (scope == 1) //#define CFS_SMC_MEDIUM		1
      // {
      //   fileName = "FullConf-" + DateTime.Now.ToString(BackupDateFormat) + ".cfim";
      // }
      // else if (scope == 2) //#define CFS_SMC_COMPLETE	2
      // {
      //   fileName = "FullConfRetro-" + DateTime.Now.ToString(BackupDateFormat) + ".cfim";
      // }
      //
      // var result = await Task.Run(() => TmNative.cfsSaveMachineConfigEx(
      //                                                                   EncodingUtil.StringToBytes(Host),
      //                                                                   EncodingUtil
      //                                                                     .StringToBytes(Path.Combine(directory,
      //                                                                       fileName)),
      //                                                                   scope,
      //                                                                   callback, callbackParameter,
      //                                                                   errBuf, errBufLength)).ConfigureAwait(false);
      //
      // if (result != true)
      // {
      //   return (false, EncodingUtil.BytesToString(errBuf));
      // }
      // else
      // {
      //   return (true, string.Empty);
      // }
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

      var result = await Task.Run(() => TmNative.cfsPrepNewConfig(CfId,
                                                                  EncodingUtil.StringToBytes(remoteFilename),
                                                                  out errCode, errBuf, errBufLength))
                             .ConfigureAwait(false);

      if (errCode != 0)
      {
        return (false, EncodingUtil.BytesToString(errBuf));
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

      var result = await Task.Run(() => TmNative.cfsDirEnum(CfId,
                                                            EncodingUtil.StringToBytes(path),
                                                            resBuf, resBufLength,
                                                            out errCode, errBuf, errBufLength))
                             .ConfigureAwait(false);
      if (errCode != 0)
      {
        return (null, errCode, EncodingUtil.BytesToString(errBuf));
      }
      else
      {
        return (TmNativeUtil.GetStringListFromDoubleNullTerminatedChars(resBuf), 0, string.Empty);
      }
    }

    public async Task<bool> CreateBackup(string           progName,
                                         string           pipeName,
                                         string           directory,
                                         bool             withRetro,
                                         TmNativeCallback callback          = null,
                                         nint             callbackParameter = default)
    {
      return await Task.Run(() => TmNativeApi.CreateBackup(Host,
                                                           progName,
                                                           pipeName,
                                                           directory,
                                                           withRetro,
                                                           callback,
                                                           callbackParameter))
                       .ConfigureAwait(false);
    }

    public async Task<RestoreBackupResult> RestoreBackup(string           progName,
                                                         string           pipeName,
                                                         string           filename,
                                                         bool             withRetro,
                                                         TmNativeCallback callback          = null,
                                                         IntPtr           callbackParameter = default)
    {
      return await Task.Run(() => TmNativeApi.RestoreBackup(Host,
                                                            progName,
                                                            pipeName,
                                                            filename,
                                                            withRetro,
                                                            callback,
                                                            callbackParameter))
                       .ConfigureAwait(false);
    }

    public async Task BackupSecurity(string directory, string pwd = "")
    {
      await Task.Run(() => TmNativeApi.BackupSecurity(CfId, directory, pwd))
                .ConfigureAwait(false);
    }

    public async Task RestoreSecurity(string filename, string pwd)
    {
      await Task.Run(() => TmNativeApi.RestoreSecurity(CfId, filename, pwd))
                .ConfigureAwait(false);
    }

    public async Task<(IReadOnlyCollection<string>, string)> EnumPackedFiles(string pkfName)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];

      var result =
        await Task.Run(() => TmNative.pkfEnumPackedFiles(EncodingUtil.StringToBytes(pkfName), errBuf,
                                                         errBufLength)).ConfigureAwait(false);
      if (result == IntPtr.Zero)
      {
        return (null, EncodingUtil.BytesToString(errBuf));
      }
      else
      {
        var res = (TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(result, 65536));
        TmNative.pkfFreeMemory(result);
        return (res, string.Empty);
      }
    }

    public async Task<(IReadOnlyCollection<string>, string)> UnPack(string pkfName, string dirname)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];

      var result = await Task.Run(() => TmNative.pkfUnPack(EncodingUtil.StringToBytes(pkfName),
                                                           EncodingUtil.StringToBytes(dirname), errBuf,
                                                           errBufLength)).ConfigureAwait(false);
      if (result == IntPtr.Zero)
      {
        return (null, EncodingUtil.BytesToString(errBuf));
      }
      else
      {
        var res = (TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(result, 65536));
        TmNative.pkfFreeMemory(result);
        return (res, string.Empty);
      }
    }

    public async Task<(bool, string)> ExtractFile(string pkfName, string filename, string dirname)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];

      var result = await Task.Run(() => TmNative.pkfExtractFile(EncodingUtil.StringToBytes(pkfName),
                                                                EncodingUtil.StringToBytes(filename),
                                                                EncodingUtil.StringToBytes(dirname),
                                                                errBuf, errBufLength)).ConfigureAwait(false);
      if (!result)
      {
        return (false, EncodingUtil.BytesToString(errBuf));
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

      var result = await Task.Run(() => TmNative.cfsIfpcTestTmcalc(CfId,
                                                                   EncodingUtil.StringToBytes(tmsName),
                                                                   EncodingUtil.StringToBytes(clcName),
                                                                   testWay, testFlags,
                                                                   out handle, out pid,
                                                                   out errCode, errBuf, errBufLength
                                                                  )).ConfigureAwait(false);
      if (!result)
      {
        return (errCode, EncodingUtil.BytesToString(errBuf), 0, 0);
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
      var result = await Task.Run(() => TmNative.cfsIfpcStopTestTmcalc(CfId,
                                                                       handle, pid,
                                                                       out errCode, errBuf, errBufLength
                                                                      )).ConfigureAwait(false);
      if (!result)
      {
        return (errCode, EncodingUtil.BytesToString(errBuf));
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
      var result = await Task.Run(() => TmNative.cfsPmonCheckProcess(CfId,
                                                                     EncodingUtil.StringToBytes(processNameArgs),
                                                                     out errCode, errBuf, errBufLength
                                                                    )).ConfigureAwait(false);
      return (result, EncodingUtil.BytesToString(errBuf));
    }

    public async Task<(bool, string)> PmonStopProcess(string processNameArgs)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0, pnumfound = 0;
      var result = await Task.Run(() => TmNative.cfsPmonStopProcess(CfId,
                                                                    EncodingUtil.StringToBytes(processNameArgs),
                                                                    out pnumfound,
                                                                    out errCode, errBuf, errBufLength
                                                                   )).ConfigureAwait(false);
      return (result, EncodingUtil.BytesToString(errBuf));
    }

    public async Task<(bool, string)> PmonRestartProcess(string processNameArgs)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;
      var result = await Task.Run(() => TmNative.cfsPmonRestartProcess(CfId,
                                                                       EncodingUtil.StringToBytes(processNameArgs),
                                                                       out errCode,
                                                                       errBuf,
                                                                       errBufLength
                                                                      )).ConfigureAwait(false);
      return (result, EncodingUtil.BytesToString(errBuf));
    }

    public async Task<(bool, string)> SwapFnSrvRole(string encodedCredentials, string fnsName, bool dryRun)
    {
      const int errBufLength = 1000;
      var       errBuf       = new byte[errBufLength];
      uint      errCode      = 0;

      var result = await Task.Run(() => TmNative.cfsSwapFnSrvRole(EncodingUtil.StringToBytes(encodedCredentials),
                                                                  dryRun,
                                                                  EncodingUtil.StringToBytes(fnsName),
                                                                  out errCode,
                                                                  errBuf,
                                                                  errBufLength
                                                                 )).ConfigureAwait(false);

      return (result, EncodingUtil.BytesToString(errBuf));
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
      return await Task.Run(() => AddPasswordToAutoBackupDigestSync(password))
                       .ConfigureAwait(false);
    }

    public (bool, int, string) AddPasswordToAutoBackupDigestSync(string password)
    {
      const int  responseMsgBufLength = 1000;
      Span<byte> responseMsgBuf       = stackalloc byte[responseMsgBufLength];

      var result = TmNative.cfsIfpcSetAbkParms(CfId,
                                               EncodingUtil.StringToBytes(password),
                                               out uint responseCode,
                                               responseMsgBuf,
                                               responseMsgBufLength);

      return (result, (int)responseCode, EncodingUtil.BytesToString(responseMsgBuf));
    }

    public async Task<IReadOnlyCollection<string>> GrabFile(string fileName, string userName)
    {
      return await Task.Run(() => TmNativeApi.EditGrab(CfId, true, fileName, userName)).ConfigureAwait(false);
    }

    public async Task UnGrabFile(string fileName, string userName)
    {
      await Task.Run(() => TmNativeApi.EditGrab(CfId, false, fileName, userName)).ConfigureAwait(false);
    }
  }
}