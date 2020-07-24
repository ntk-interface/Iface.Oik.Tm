using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

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


    public async Task<(IntPtr, DateTime)> OpenMasterServiceConfiguration()
    {
      var       fileTime        = new TmNativeDefs.FileTime();
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;


      var msTreeRoot = await Task.Run(() => _native.CfsConfFileOpenCid(CfId,
                                                                       Host,
                                                                       TmNativeDefs.DefaultMasterConfFile,
                                                                       30000 | TmNativeDefs.FailIfNoConnect,
                                                                       ref fileTime,
                                                                       out errCode,
                                                                       ref errString,
                                                                       errStringLength))
                                 .ConfigureAwait(false);

      if (msTreeRoot == IntPtr.Zero)
        throw new Exception($"Ошибка получения конфигурации мастер-сервиса: {errString} Код: {errCode}");

      return (msTreeRoot, GetDateTimeFromCustomFileTime(fileTime));
    }


    public async Task<bool> SaveMasterServiceConfiguration(IntPtr treeHandle, string serverName)
    {
      var       fileTime        = new TmNativeDefs.FileTime();
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;

      return await Task.Run(() => _native.CfsConfFileSaveAs(treeHandle,
                                                            serverName,
                                                            TmNativeDefs.DefaultMasterConfFile,
                                                            30000 | TmNativeDefs.FailIfNoConnect,
                                                            ref fileTime,
                                                            out errCode,
                                                            ref errString,
                                                            errStringLength))
                       .ConfigureAwait(false);
    }


    private static DateTime GetDateTimeFromCustomFileTime(TmNativeDefs.FileTime fileTime)
    {
      return DateTime.FromFileTime((long) fileTime.dwHighDateTime << 32 | (uint) fileTime.dwLowDateTime);
    }


    public async Task<List<CfTreeNode>> GetMasterServiceTree(IntPtr rootHandle)
    {
      return await GetNodeChildren(rootHandle)
              .ConfigureAwait(false);
    }


    private async Task<List<CfTreeNode>> GetNodeChildren(IntPtr parentHandle, CfTreeNode parent = null)
    {
      var children = new List<CfTreeNode>();

      for (var i = 0;; i++)
      {
        var childHandle = await Task.Run(() => _native.CftNodeEnum(parentHandle, i))
                                    .ConfigureAwait(false);
        if (childHandle == IntPtr.Zero)
          break;

        var nodeChild = GetNode(await GetNodeName(childHandle).ConfigureAwait(false),
                                await GetNodeProps(childHandle).ConfigureAwait(false),
                                parent);
        nodeChild.Children = await GetNodeChildren(childHandle, nodeChild)
                              .ConfigureAwait(false);
        children.Add(nodeChild);
      }

      return children;
    }


    private async Task<string> GetNodeName(IntPtr nodeHandle)
    {
      const int nameStringLength = 200;
      var       nameString       = new StringBuilder(nameStringLength);

      await Task.Run(() =>
                       _native.CftNodeGetName(nodeHandle, ref nameString, nameStringLength))
                .ConfigureAwait(false);

      return nameString.ToString();
    }


    private async Task<Dictionary<string, string>> GetNodeProps(IntPtr nodeHandle)
    {
      var props = new Dictionary<string, string>();

      for (var i = 0;; i++)
      {
        var propName = await GetPropName(nodeHandle, i)
                        .ConfigureAwait(false);
        if (propName == "")
          break;

        var propValue = await GetPropValue(nodeHandle, propName)
                         .ConfigureAwait(false);

        props.Add(propName, propValue);
      }

      return props;
    }


    private async Task<string> GetPropName(IntPtr nodeHandle, int idx)
    {
      const int nameStringLength = 200;
      var       nameString       = new StringBuilder(nameStringLength);

      await Task.Run(() =>
                       _native.CftNPropEnum(nodeHandle, idx, ref nameString, nameStringLength))
                .ConfigureAwait(false);

      return nameString.ToString();
    }


    private async Task<string> GetPropValue(IntPtr nodeHandle, string propName)
    {
      const int valueStringLength = 200;
      var       valueString       = new StringBuilder(valueStringLength);

      await Task.Run(() =>
                       _native.CftNPropGetText(nodeHandle, propName, ref valueString,
                                               valueStringLength))
                .ConfigureAwait(false);

      return valueString.ToString();
    }


    private static CfTreeNode GetNode(string                              nodeName,
                                      IReadOnlyDictionary<string, string> properties,
                                      CfTreeNode                          parent)
    {
      if (nodeName == "Master")
      {
        return new MasterNode(properties.ContainsKey("LogFileSize") ? int.Parse(properties["LogFileSize"]) : 0x80000,
                              properties.ContainsKey("Отмена запуска") && properties["Отмена запуска"] == "1",
                              properties.ContainsKey("Рабочий каталог") ? properties["Рабочий каталог"] : "");
      }

      if (!properties.ContainsKey("ProgName"))
        throw new Exception("Ошибка чтения конфигурации");
      var progName = properties["ProgName"];

      switch (progName)
      {
        case "pcsrv":
        {
          return new TmsNode(properties["ProgName"],
                             parent,
                             properties.ContainsKey("PipeName") ? properties["PipeName"] : "",
                             properties.ContainsKey("Отмена запуска") && properties["Отмена запуска"]    == "1",
                             !properties.ContainsKey("Пассивный режим") || properties["Пассивный режим"] == "1");
        }
        case "rbsrv":
        {
          return new RbsNode(properties["ProgName"],
                             parent,
                             properties.ContainsKey("PipeName") ? properties["PipeName"] : "",
                             properties.ContainsKey("Отмена запуска") && properties["Отмена запуска"] == "1");
        }
        case "delta_pc":
        {
          return new DeltaNode(properties["ProgName"],
                               parent,
                               properties.ContainsKey("PipeName") ? properties["PipeName"] : "",
                               properties.ContainsKey("Отмена запуска") && properties["Отмена запуска"] == "1");
        }
        case "tmcalc_pc":
        {
          return new TmCalcNode(properties["ProgName"],
                                parent,
                                properties.ContainsKey("PipeName") ? properties["PipeName"] : "",
                                properties.ContainsKey("Отмена запуска") && properties["Отмена запуска"] == "1");
        }
        case "_ext_pc":
        {
          return new ExternalTaskNode(properties["ProgName"], parent,
                                      properties.ContainsKey("PipeName") ? properties["PipeName"] : "",
                                      properties.ContainsKey("Отмена запуска") && properties["Отмена запуска"] == "1",
                                      properties.ContainsKey("Программа") ? properties["Программа"] : "",
                                      properties.ContainsKey("Аргументы") ? properties["Аргументы"] : "",
                                      properties.ContainsKey("Конф. файл") ? properties["Конф. файл"] : ""
                                     );
        }
        case "ElectricTopology":
        {
          return new ElectricTopologyNode(properties["ProgName"], 
                                          parent, 
                                          properties.ContainsKey("PipeName") ? properties["PipeName"] : "", 
                                          properties.ContainsKey("Отмена запуска") && properties["Отмена запуска"] == "1");
        }
        default:
          return new CfTreeNode(progName,
                                parent);
      }
    }


    public void FreeMasterServiceConfigurationHandle(IntPtr handle)
    {
      _native.CftNodeFreeTree(handle);
    }


    public async Task<IntPtr> CreateNewMasterServiceTree(IEnumerable<CfTreeNode> tree)
    {
      var newTreeHandle = await Task.Run(() => _native.CftNodeNewTree())
                                    .ConfigureAwait(false);


      foreach (var node in tree)
      {
        await CreateNode(newTreeHandle, node)
         .ConfigureAwait(false);
      }

      return newTreeHandle;
    }


    private async Task CreateNode(IntPtr parentNodeHandle, CfTreeNode node, int tagId = -1)
    {
      var tag = tagId == -1 ? "Master" : $"#{tagId:X3}";
      var nodeHandle = await Task.Run(() => _native.CftNodeInsertDown(parentNodeHandle, tag))
                                 .ConfigureAwait(false);

      if (!await CreateNodeProperties(nodeHandle, node)
            .ConfigureAwait(false))
        throw new Exception("Ошибка заполнения дерева конфигурации");

      var i = 0;
      foreach (var childNode in node.Children)
      {
        await CreateNode(nodeHandle, childNode, i).ConfigureAwait(false);
        i++;
      }
    }


    private async Task<bool> CreateNodeProperties(IntPtr nodeHandle, CfTreeNode node)
    {
      if (!await CreateNodePropertyAsync(nodeHandle, "ProgName", node.ProgName)
            .ConfigureAwait(false))
        return false;

      switch (node)
      {
        case MasterNode _:
          if (!await CreateMasterNodeProperties(nodeHandle, node)
                .ConfigureAwait(false))
          {
            return false;
          }

          break;
        case TmsNode _:
          if (!await CreateTmsNodeProperties(nodeHandle, node)
                .ConfigureAwait(false))
          {
            return false;
          }

          break;
        case ExternalTaskNode _:
          if (!await CreateExternalTaskNodeProperties(nodeHandle, node)
                .ConfigureAwait(false))
          {
            return false;
          }

          break;
        default:
          if (!await CreateChildNodeProperties(nodeHandle, node)
                .ConfigureAwait(false))
          {
            return false;
          }

          break;
      }

      return true;
    }


    private async Task<bool> CreateMasterNodeProperties(IntPtr nodeHandle, CfTreeNode node)
    {
      var props = (MasterNodeProperties) node.Properties;

      if (!await CreateNodePropertyAsync(nodeHandle,
                                         "Размер лог-файла",
                                         props.LogFileSize.ToString())
            .ConfigureAwait(false))
      {
        return false;
      }

      if (!await CreateNodePropertyAsync(nodeHandle,
                                         "Отмена запуска",
                                         Convert.ToInt32(props.NoStart).ToString())
            .ConfigureAwait(false))
      {
        return false;
      }


      if (!await CreateNodePropertyAsync(nodeHandle,
                                         "Рабочий каталог",
                                         props.WorkDir)
            .ConfigureAwait(false))
      {
        return false;
      }

      return true;
    }


    private async Task<bool> CreateChildNodeProperties(IntPtr nodeHandle, CfTreeNode node)
    {
      var props = (ChildNodeProperties) node.Properties;

      if (!await CreateNodePropertyAsync(nodeHandle,
                                         "PipeName",
                                         props.PipeName)
            .ConfigureAwait(false))
      {
        return false;
      }

      if (!await CreateNodePropertyAsync(nodeHandle,
                                         "Отмена запуска",
                                         Convert.ToInt32(props.NoStart).ToString())
            .ConfigureAwait(false))
      {
        return false;
      }


      return true;
    }


    private async Task<bool> CreateTmsNodeProperties(IntPtr nodeHandle, CfTreeNode node)
    {
      var props = (TmsNodeProperties) node.Properties;

      if (!await CreateChildNodeProperties(nodeHandle, node)
            .ConfigureAwait(false))
      {
        return false;
      }

      if (!await CreateNodePropertyAsync(nodeHandle,
                                         "Пассивный режим",
                                         Convert.ToInt32(props.PassiveMode).ToString())
            .ConfigureAwait(false))
      {
        return false;
      }

      return true;
    }


    private async Task<bool> CreateExternalTaskNodeProperties(IntPtr nodeHandle, CfTreeNode node)
    {
      var props = (ExternalTaskNodeProperties) node.Properties;

      if (!await CreateChildNodeProperties(nodeHandle, node).ConfigureAwait(false))
      {
        return false;
      }

      if (!await CreateNodePropertyAsync(nodeHandle,
                                         "Программа",
                                         props.TaskPath)
            .ConfigureAwait(false))
      {
        return false;
      }

      if (!await CreateNodePropertyAsync(nodeHandle,
                                         "Программа",
                                         props.TaskArguments)
            .ConfigureAwait(false))
      {
        return false;
      }

      if (!await CreateNodePropertyAsync(nodeHandle,
                                         "Программа",
                                         props.ConfigurationFilePath)
            .ConfigureAwait(false))
      {
        return false;
      }

      return true;
    }


    private async Task<bool> CreateNodePropertyAsync(IntPtr nodeHandle, string propName, string propText)
    {
      return await Task.Run(() => _native.CftNPropSet(nodeHandle, propName, propText))
                       .ConfigureAwait(false);
    }


    public async Task<uint> GetSoftware()
    {
      return await Task.Run(() => _native.CfsGetSoftwareType(CfId))
                       .ConfigureAwait(false);
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

    public async Task<IReadOnlyCollection<TmServer>> GetTmServersTree()
    {
      var lookup    = new Dictionary<uint, TmServer>();
      var tmServers = await GetTmServers().ConfigureAwait(false);

      tmServers.ForEach(x => lookup.Add(x.ProcessId, x));
      foreach (var tmServer in tmServers)
      {
        if (!lookup.TryGetValue(tmServer.ParentProcessId, out var proposedParent)) continue;
        tmServer.Parent = proposedParent;
        proposedParent.Children.Add(tmServer);
      }

      return lookup.Values.Where(x => x.Parent == null).ToList();
    }


    private async Task<IReadOnlyCollection<TmServer>> GetTmServers()
    {
      var serverIdsList = await GetIfaceServerId().ConfigureAwait(false);
      var tmUsers       = await GetTmUsers()
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


    private async Task<IReadOnlyCollection<string>> GetIfaceServerId()
    {
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;

      var serversIdsPointer = await Task.Run(() => _native.CfsTraceEnumServers(CfId,
                                                                               out errCode,
                                                                               ref errString,
                                                                               errStringLength))
                                        .ConfigureAwait(false);


      var serversIds = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(serversIdsPointer, 1000);

      _native.CfsFreeMemory(serversIdsPointer);
      return serversIds;
    }


    private async Task<TmNativeDefs.IfaceServer> GetIfaceServerData(string serverId)
    {
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;
      var       ifaceServer     = new TmNativeDefs.IfaceServer();

      await Task.Run(() => _native.CfsTraceGetServerData(CfId,
                                                         serverId,
                                                         ref
                                                         ifaceServer,
                                                         out errCode,
                                                         ref errString,
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
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;

      var usersIdsPointer = await Task.Run(() => _native.CfsTraceEnumUsers(CfId,
                                                                           out errCode,
                                                                           ref errString,
                                                                           errCode))
                                      .ConfigureAwait(false);

      var usersIds = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(usersIdsPointer, 1000);

      _native.CfsFreeMemory(usersIdsPointer);
      return usersIds;
    }


    private async Task<TmNativeDefs.IfaceUser> GetIfaceUserData(string userId)
    {
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;
      var       ifaceUser       = new TmNativeDefs.IfaceUser();

      await Task.Run(() => _native.CfsTraceGetUserData(CfId,
                                                       userId,
                                                       ref
                                                       ifaceUser,
                                                       out errCode,
                                                       ref errString,
                                                       errStringLength))
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
    
    
    public async Task<IReadOnlyCollection<TmServerThread>> GetTmServersThreads()
    {
      const int errStringLength = 1000;
      const int bufSize         = 8192;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;
      
      var threadPtr = 
        await Task.Run(() => _native.CfsEnumThreads(CfId, out errCode, ref errString, errCode))
                  .ConfigureAwait(false);

      if (threadPtr == IntPtr.Zero)
      {
        throw new Exception($"Ошибка получения потоков сервера: {errString} Код: {errCode}");
      }
      
      var threadsStringLists = 
        TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(threadPtr, bufSize);

      _native.CfsFreeMemory(threadPtr);
      
      var tmServerThreadsList = new List<TmServerThread>();

      foreach (var threadString in threadsStringLists)
      {
        var regex    = new Regex(@"([0-9]*), (.*?) • ([-+]?[0-9]*) s • ([-+]?[0-9]*\.?[0-9]+) s");
        var mc       = regex.Match(threadString);
        var id       = int.Parse(mc.Groups[1].Value);
        var name     = mc.Groups[2].Value;
        var upTime   = int.Parse(mc.Groups[3].Value);
        var workTime = float.Parse(mc.Groups[4].Value, CultureInfo.InvariantCulture);
        tmServerThreadsList.Add(new TmServerThread(id, name, upTime, workTime));
      }
      
      return tmServerThreadsList;
    }


    public async Task RegisterTmServerTracer(ITmServerTraceable traceTarget, bool debug, int pause)
    {
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;

      var result = await Task.Run(() => _native.CfsTraceBeginTraceEx(CfId,
                                                                     traceTarget.ProcessId,
                                                                     traceTarget.ThreadId, debug,
                                                                     (uint)pause,
                                                                     out errCode,
                                                                     ref errString,
                                                                     errStringLength))
                             .ConfigureAwait(false);
      Console.WriteLine($"Register result: {result}");

    }


    public async Task StopTmServerTrace()
    {
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;

      var result = await Task.Run(() => _native.CfsTraceEndTrace(CfId, out errCode, ref errString, errStringLength))
                             .ConfigureAwait(false);
      Console.WriteLine($"Stop result: {result}");
    }
 

    public async Task<IReadOnlyCollection<TmServerLogRecord>> TraceTmServerLogRecords()
    {
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;
      
      var logRecordPtr = await Task.Run(() => _native.CfsTraceGetMessage(CfId, out errCode,
                                                                         ref errString,
                                                                         errStringLength))
                                   .ConfigureAwait(false);

      if (logRecordPtr == IntPtr.Zero) return null;
      
      if (errCode != 0)
      {
        throw new Exception($"Ошибка трассировки: {errString} Код: {errCode} CfId:{CfId}" );
      }

      var cfsLogRecords = ParseCfsServerLogRecordPointer(logRecordPtr, 5120);

      await Task.Run(() => _native.CfsFreeMemory(logRecordPtr)).ConfigureAwait(false);
      

      return cfsLogRecords.Select(TmServerLogRecord.CreateFromCfsLogRecord).ToList();
    }


    private async Task OpenTmServerLog()
    {
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;

      var result = await Task.Run(() => _native.CfsLogOpen(CfId,
                                                          out errCode,
                                                          ref errString,
                                                          errStringLength))
                             .ConfigureAwait(false);
      if (!result)
      {
        throw new Exception($"Ошибка получения журнала сервера: {errString} Код: {errCode}");
      }
    }
    
    
    private async Task CloseTmServerLog()
    {
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;

      var result = await Task.Run(() => _native.CfsLogClose(CfId,
                                                          out errCode,
                                                          ref errString,
                                                          errStringLength))
                             .ConfigureAwait(false);
      if (!result)
      {
        throw new Exception($"Ошибка получения журнала сервера: {errString} Код: {errCode}");
      }
    }


    private async Task<TmServerLogRecord> GetTmServersLogRecord(bool isFirst = false)
    {
      const int errStringLength = 1000;
      var       errString       = new StringBuilder(errStringLength);
      uint      errCode         = 0;
      
      var logRecordPtr =
        await Task.Run(() => _native.CfsLogGetRecord(CfId, isFirst, out errCode, ref errString, errCode))
                  .ConfigureAwait(false);
      
      if (logRecordPtr == IntPtr.Zero) return null;

      var cfsLogRecord = ParseCfsServerLogRecordPointer(logRecordPtr, 1000).FirstOrDefault();

      _native.CfsFreeMemory(logRecordPtr);

      return TmServerLogRecord.CreateFromCfsLogRecord(cfsLogRecord);
    }
    
    
    private IReadOnlyCollection<TmNativeDefs.CfsLogRecord> ParseCfsServerLogRecordPointer(IntPtr ptr, int maxSize)
    {
      var strList = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptr, maxSize);
      
      return strList.Select(x => {
                              var mc = _cfsServerLogRecordRegex.Match(x);
                              return new TmNativeDefs.CfsLogRecord
                                     {
                                       Time     = mc.Groups[1].Value,
                                       Date     = mc.Groups[2].Value,
                                       Name     = mc.Groups[3].Value,
                                       Type     = mc.Groups[4].Value,
                                       MsgType  = mc.Groups[5].Value.Trim(' '),
                                       ThreadId = mc.Groups[6].Value,
                                       Message  = mc.Groups[7].Value,
                                     }; })
                    .ToList();
    }
  }
}