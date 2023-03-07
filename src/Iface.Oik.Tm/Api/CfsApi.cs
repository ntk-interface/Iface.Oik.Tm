using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
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
            var fileTime = new TmNativeDefs.FileTime();
            const int errStringLength = 1000;
            var errBuf = new byte[errStringLength];
            uint errCode = 0;


            var msTreeRoot = await Task.Run(() => _native.CfsConfFileOpenCid(CfId,
                                                                             Host,
                                                                             TmNativeDefs.DefaultMasterConfFile,
                                                                             30000 | TmNativeDefs.FailIfNoConnect,
                                                                             ref fileTime,
                                                                             out errCode,
                                                                             ref errBuf,
                                                                             errStringLength))
                                       .ConfigureAwait(false);

            if (msTreeRoot == IntPtr.Zero)
                throw new Exception($"Ошибка получения конфигурации мастер-сервиса: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");

            return (msTreeRoot, GetDateTimeFromCustomFileTime(fileTime));
        }


        public async Task<bool> SaveMasterServiceConfiguration(IntPtr treeHandle, string serverName)
        {
            var fileTime = new TmNativeDefs.FileTime();
            const int errStringLength = 1000;
            var errBuf = new byte[errStringLength];
            uint errCode = 0;

            return await Task.Run(() => _native.CfsConfFileSaveAs(treeHandle,
                                                                  serverName,
                                                                  TmNativeDefs.DefaultMasterConfFile,
                                                                  30000 | TmNativeDefs.FailIfNoConnect,
                                                                  ref fileTime,
                                                                  out errCode,
                                                                  ref errBuf,
                                                                  errStringLength))
                             .ConfigureAwait(false);
        }


        private static DateTime GetDateTimeFromCustomFileTime(TmNativeDefs.FileTime fileTime)
        {
            return DateTime.FromFileTime((long)fileTime.dwHighDateTime << 32 | (uint)fileTime.dwLowDateTime);
        }


        public async Task<List<CfTreeNode>> GetMasterServiceTree(IntPtr rootHandle)
        {
            return await GetNodeChildren(rootHandle)
                     .ConfigureAwait(false);
        }


        private async Task<List<CfTreeNode>> GetNodeChildren(IntPtr parentHandle, CfTreeNode parent = null)
        {
            var children = new List<CfTreeNode>();

            for (var i = 0; ; i++)
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
            const int nameBufLength = 200;
            var nameBuf = new byte[nameBufLength];

            await Task.Run(() =>
                             _native.CftNodeGetName(nodeHandle, ref nameBuf, nameBufLength))
                      .ConfigureAwait(false);

            return EncodingUtil.Win1251BytesToUtf8(nameBuf);
        }


        private async Task<Dictionary<string, string>> GetNodeProps(IntPtr nodeHandle)
        {
            var props = new Dictionary<string, string>();

            for (var i = 0; ; i++)
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
            const int nameBufLength = 200;
            var nameBuf = new byte[nameBufLength];

            await Task.Run(() =>
                             _native.CftNPropEnum(nodeHandle, idx, ref nameBuf, nameBufLength))
                      .ConfigureAwait(false);

            return EncodingUtil.Win1251BytesToUtf8(nameBuf);
        }


        private async Task<string> GetPropValue(IntPtr nodeHandle, string propName)
        {
            const int valueBufLength = 200;
            var valueBuf = new byte[valueBufLength];

            await Task.Run(() =>
                             _native.CftNPropGetText(nodeHandle, propName, ref valueBuf,
                                                     valueBufLength))
                      .ConfigureAwait(false);

            return EncodingUtil.Win1251BytesToUtf8(valueBuf);
        }


        private static CfTreeNode GetNode(string nodeName,
                                          IReadOnlyDictionary<string, string> properties,
                                          CfTreeNode parent)
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
                                           properties.ContainsKey("Отмена запуска") && properties["Отмена запуска"] == "1",
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
                                                        properties.ContainsKey("Отмена запуска") &&
                                                        properties["Отмена запуска"] == "1");
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
            var props = (MasterNodeProperties)node.Properties;

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
            var props = (ChildNodeProperties)node.Properties;

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
            var props = (TmsNodeProperties)node.Properties;

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
            var props = (ExternalTaskNodeProperties)node.Properties;

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

        public async Task<IReadOnlyCollection<TmServer>> GetTmServersTree()
        {
            var lookup = new Dictionary<uint, TmServer>();
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


        private async Task<IReadOnlyCollection<string>> GetIfaceServerId()
        {
            const int errStringLength = 1000;
            var errBuf = new byte[errStringLength];
            uint errCode = 0;

            var serversIdsPointer = await Task.Run(() => _native.CfsTraceEnumServers(CfId,
                                                                                     out errCode,
                                                                                     ref errBuf,
                                                                                     errStringLength))
                                              .ConfigureAwait(false);


            var serversIds = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(serversIdsPointer, 1000);

            _native.CfsFreeMemory(serversIdsPointer);
            return serversIds;
        }


        private async Task<TmNativeDefs.IfaceServer> GetIfaceServerData(string serverId)
        {
            const int errStringLength = 1000;
            var errBuf = new byte[errStringLength];
            uint errCode = 0;
            var ifaceServer = new TmNativeDefs.IfaceServer();

            await Task.Run(() => _native.CfsTraceGetServerData(CfId,
                                                               serverId,
                                                               ref
                                                               ifaceServer,
                                                               out errCode,
                                                               ref errBuf,
                                                               errStringLength))
                      .ConfigureAwait(false);

            return ifaceServer;
        }


        private async Task<IReadOnlyCollection<TmUser>> GetTmUsers()
        {
            var usersIds = await GetIfaceUsersIds().ConfigureAwait(false);
            var tmUsers = new List<TmUser>();

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
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

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
            var errBuf = new byte[errBufLength];
            uint errCode = 0;
            var ifaceUser = new TmNativeDefs.IfaceUser();

            await Task.Run(() => _native.CfsTraceGetUserData(CfId,
                                                             userId,
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
        public async Task<IReadOnlyCollection<TmServerLogRecord>> GetTmServersLog(int MaxRecords, DateTime? StartTime, DateTime? EndTime)
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
                if(EndTime != null)
                {
                    if (logRecord.DateTime > EndTime)
                        continue;
                }
				if (StartTime != null)
				{
					if (logRecord.DateTime < StartTime)
						break;
				}
				tmServersLog.Add(logRecord);
                if ((MaxRecords > 0) && (tmServersLog.Count >= MaxRecords))
                    break;
            }

            await CloseTmServerLog().ConfigureAwait(false);

            return tmServersLog;
        }


        public async Task<IReadOnlyCollection<TmServerThread>> GetTmServersThreads()
        {
            const int errBufLength = 1000;
            const int bufSize = 8192;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            var threadPtr =
              await Task.Run(() => _native.CfsEnumThreads(CfId, out errCode, ref errBuf, errCode))
                        .ConfigureAwait(false);

            if (threadPtr == IntPtr.Zero)
            {
                throw new Exception($"Ошибка получения потоков сервера: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
            }

            var threadsStringLists =
              TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(threadPtr, bufSize);

            _native.CfsFreeMemory(threadPtr);

            var tmServerThreadsList = new List<TmServerThread>();

            foreach (var threadString in threadsStringLists)
            {
                var regex = new Regex(@"([0-9]*), (.*?) • ([-+]?[0-9]*) s • ([-+]?[0-9]*\.?[0-9]+) s");
                var mc = regex.Match(threadString);
                var id = int.Parse(mc.Groups[1].Value);
                var name = mc.Groups[2].Value;
                var upTime = int.Parse(mc.Groups[3].Value);
                var workTime = float.Parse(mc.Groups[4].Value, CultureInfo.InvariantCulture);
                tmServerThreadsList.Add(new TmServerThread(id, name, upTime, workTime));
            }

            return tmServerThreadsList;
        }


        public async Task RegisterTmServerTracer(ITmServerTraceable traceTarget, bool debug, int pause)
        {
            const int errBufLength = 1000;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            var result = await Task.Run(() => _native.CfsTraceBeginTraceEx(CfId,
                                                                           traceTarget.ProcessId,
                                                                           traceTarget.ThreadId, debug,
                                                                           (uint)pause,
                                                                           out errCode,
                                                                           ref errBuf,
                                                                           errBufLength))
                                   .ConfigureAwait(false);
            Console.WriteLine($"Register result: {result}");
        }


        public async Task StopTmServerTrace()
        {
            const int errBufLength = 1000;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            var result = await Task.Run(() => _native.CfsTraceEndTrace(CfId, out errCode, ref errBuf, errBufLength))
                                   .ConfigureAwait(false);
            Console.WriteLine($"Stop result: {result}");
        }


        public async Task<IReadOnlyCollection<TmServerLogRecord>> TraceTmServerLogRecords()
        {
            const int errBufLength = 1000;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            var logRecordPtr = await Task.Run(() => _native.CfsTraceGetMessage(CfId, out errCode,
                                                                               ref errBuf,
                                                                               errBufLength))
                                         .ConfigureAwait(false);

            if (logRecordPtr == IntPtr.Zero) return null;

            if (errCode != 0)
            {
                throw new Exception($"Ошибка трассировки: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode} CfId:{CfId}");
            }

            var cfsLogRecords = ParseCfsServerLogRecordPointer(logRecordPtr, 5120);

            await Task.Run(() => _native.CfsFreeMemory(logRecordPtr)).ConfigureAwait(false);


            return cfsLogRecords.Select(TmServerLogRecord.CreateFromCfsLogRecord).ToList();
        }


        private async Task OpenTmServerLog()
        {
            const int errBufLength = 1000;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            var result = await Task.Run(() => _native.CfsLogOpen(CfId,
                                                                 out errCode,
                                                                 ref errBuf,
                                                                 errBufLength))
                                   .ConfigureAwait(false);
            if (!result)
            {
                throw new Exception($"Ошибка получения журнала сервера: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
            }
        }


        private async Task CloseTmServerLog()
        {
            const int errBufLength = 1000;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            var result = await Task.Run(() => _native.CfsLogClose(CfId,
                                                                  out errCode,
                                                                  ref errBuf,
                                                                  errBufLength))
                                   .ConfigureAwait(false);
            if (!result)
            {
                throw new Exception($"Ошибка получения журнала сервера: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
            }
        }


        private async Task<TmServerLogRecord> GetTmServersLogRecord(bool isFirst = false)
        {
            const int errBufLength = 1000;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            var logRecordPtr =
              await Task.Run(() => _native.CfsLogGetRecord(CfId, isFirst, out errCode, ref errBuf, errCode))
                        .ConfigureAwait(false);

            if (logRecordPtr == IntPtr.Zero) return null;

            var cfsLogRecord = ParseCfsServerLogRecordPointer(logRecordPtr, 1000).FirstOrDefault();

            _native.CfsFreeMemory(logRecordPtr);

            return TmServerLogRecord.CreateFromCfsLogRecord(cfsLogRecord);
        }


        private IReadOnlyCollection<TmNativeDefs.CfsLogRecord> ParseCfsServerLogRecordPointer(IntPtr ptr, int maxSize)
        {
            var strList = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptr, maxSize);

            return strList.Select(x =>
                                  {
                                      var mc = _cfsServerLogRecordRegex.Match(x);
                                      return new TmNativeDefs.CfsLogRecord
                                      {
                                          Time = mc.Groups[1].Value,
                                          Date = mc.Groups[2].Value,
                                          Name = mc.Groups[3].Value,
                                          Type = mc.Groups[4].Value,
                                          MsgType = mc.Groups[5].Value.Trim(' '),
                                          ThreadId = mc.Groups[6].Value,
                                          Message = mc.Groups[7].Value,
                                      };
                                  })
                          .ToList();
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
            const string remotePath = "@dispserv.ini";
            const int errBufLength = 512;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            if (!await Task.Run(() => _native.CfsFileGet(CfId,
                                                         remotePath,
                                                         localPath,
                                                         30000 | TmNativeDefs.FailIfNoConnect,
                                                         IntPtr.Zero,
                                                         out errCode,
                                                         ref errBuf,
                                                         errBufLength))
                           .ConfigureAwait(false))
            {
                Console.WriteLine($"Ошибка при скачивании файла: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errBuf)}");
                return false;
            }

            if (!File.Exists(localPath))
            {
                Console.WriteLine("Ошибка при сохранении файла в файловую систему");
                return false;
            }

            return true;
        }


        private async Task<string> GetInstallationInfoString(string key)
        {
            const string path = "@@";
            const string section = "IInfo";

            return await GetIniString(path, section, key).ConfigureAwait(false);
        }


        private async Task<TmNativeDefs.CfsFileProperties?> GetFileProperties(string filePath)
        {
            const int errBufLength = 1000;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;
            var fileProps = new TmNativeDefs.CfsFileProperties();

            var result =
              await Task.Run(() => _native.CfsFileGetPropreties(CfId,
                                                                filePath,
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
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            var signaturePointer = new IntPtr();
            var errorsPointer = new IntPtr();

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
                throw new Exception($"Ошибка проверки целостности сервера: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
            }

            var signature = $"Корневая сигнатура:{TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(signaturePointer)}";
            var errors = TmNativeUtil.GetStringWithUnknownLengthFromIntPtr(errorsPointer);
            _native.CfsFreeMemory(signaturePointer);
            _native.CfsFreeMemory(errorsPointer);

            return errors.IsNullOrEmpty() ? (true, signature) : (false, errors);
        }


        public async Task<TmLicenseInfo> GetLicenseInfo()
        {
            const string path = "@@";
            const string section = "FInfo";
            const uint bufSize = 1024 * 8;

            var keyDataStrings =
              (await GetIniString(path, section, bufSize: bufSize).ConfigureAwait(false)).Split(new[] { '\n' },
                StringSplitOptions.RemoveEmptyEntries);

            var keyDataDictionary = new Dictionary<string, string>();
            foreach (var keyData in keyDataStrings)
            {
                keyDataDictionary.Add(keyData.Split('=').First(),
                                      await GetLicenseKeyDataItemString(keyData).ConfigureAwait(false));
            }

            var currentLicenseKey = new TmLicenseKey(await GetCurrentLicenseKeyCom().ConfigureAwait(false));

            return new TmLicenseInfo(currentLicenseKey, keyDataDictionary);
        }


        public async Task SetLicenseKeyCom(TmLicenseKey newLicenseKey)
        {
            var path = Path.Combine(await GetBasePath().ConfigureAwait(false),
                                    "Data\\Main\\cfshare.ini");
            const string section = "IfaceSecKey";
            const string key = "COM";

            await SetIniString(path, section, key, newLicenseKey.NativeCom()).ConfigureAwait(false);
        }


        public async Task<IReadOnlyCollection<string>> GetFilesInDirectory(string path)
        {
            const uint bufLength = 8192;
            const int errBufLength = 1000;
            var buf = new char[bufLength];
            var errBuf = new byte[errBufLength];
            uint errCode = 0;


            if (!await Task.Run(() => _native.CfsDirEnum(CfId,
                                                         path,
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


        public async Task PutFile(string localFilePath,
                                  string remoteFilePath,
                                  uint timeout = 20000)
        {
            if (localFilePath.IsNullOrEmpty())
            {
                Console.WriteLine("Ошибка: не указан локальный путь до файла");
                return;
            }
            if (remoteFilePath.IsNullOrEmpty())
            {
                Console.WriteLine("Ошибка: не указан удалённый путь до файла");
                return;
            }

            const int errBufLength = 1000;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            if (!await Task.Run(() => _native.CfsFilePut(CfId, remoteFilePath, localFilePath, timeout, out errCode,
                                                        ref errBuf, errBufLength)).ConfigureAwait(false))
            {
                Console.WriteLine($"Ошибка при отправке файла: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errBuf)}");
            }
        }


        public async Task DeleteFile(string remoteFilePath)
        {
            const int errBufLength = 1000;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            if (remoteFilePath.IsNullOrEmpty())
            {
                Console.WriteLine("Ошибка: не указан удалённый путь до файла");
                return;
            }

            if (!await Task.Run(() => _native.CfsFileDelete(CfId, remoteFilePath, out errCode, ref errBuf, errBufLength))
                          .ConfigureAwait(false))
            {
                Console.WriteLine($"Ошибка при удалении файла: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errBuf)}");
            }
        }


        public async Task<IReadOnlyCollection<LicenseKeyType>> GetAvailableLicenseKeyTypes()
        {
            const string path = "@@";
            const string section = "AppKeyList";

            const uint bufSize = 1024;

            var typesStrings = (await GetIniString(path, section, bufSize: bufSize).ConfigureAwait(false)).Split(new[] { ';' },
              StringSplitOptions.RemoveEmptyEntries);

            var licenseKeyTypes = new List<LicenseKeyType>();

            foreach (var typeString in typesStrings)
            {
                var typeNumStr = typeString.Split(new[] { ". " }, StringSplitOptions.None).First();

                switch (typeNumStr)
                {
                    case "4":
                        licenseKeyTypes.Add(LicenseKeyType.TypeFour);
                        break;
                    case "5":
                        licenseKeyTypes.Add(LicenseKeyType.Software);
                        break;
                    case "6":
                        licenseKeyTypes.Add(LicenseKeyType.UsbHidSsd);
                        break;
                    case "7":
                        licenseKeyTypes.Add(LicenseKeyType.Network);
                        break;
                    default:
                        licenseKeyTypes.Add(LicenseKeyType.Unknown);
                        break;
                }
            }

            return licenseKeyTypes;
        }


        private async Task<int> GetCurrentLicenseKeyCom()
        {
            var path = Path.Combine(await GetBasePath().ConfigureAwait(false),
                                    "Data\\Main\\cfshare.ini");
            const string section = "IfaceSecKey";
            const string key = "COM";

            var currentLicenseKeyCom = await GetIniString(path, section, key, bufSize: 1024).ConfigureAwait(false);

            return currentLicenseKeyCom.IsNullOrEmpty() ? 0 : Convert.ToInt32(currentLicenseKeyCom);
        }


        private async Task<string> GetLicenseKeyDataItemString(string rawItemString)
        {
            const string path = "@@";
            const string section = "SStr";

            return await GetIniString(path, section, bufSize: 1024, def: rawItemString).ConfigureAwait(false);
        }


        private async Task<string> GetBasePath()
        {
            const int basePathBufLength = 1000;
            var basePathBuf = new byte[basePathBufLength];

            const int errBufLength = 1000;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            var result = await Task.Run(() => _native.СfsGetBasePath(CfId,
                                                                     ref basePathBuf,
                                                                     basePathBufLength,
                                                                     out errCode,
                                                                     ref errBuf,
                                                                     errBufLength))
                                   .ConfigureAwait(false);

            if (!result)
            {
                throw new Exception($"Ошибка получения базового пути сервера сервера: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
            }

            return EncodingUtil.Win1251BytesToUtf8(basePathBuf);
        }

        /* TODO // path = @@
                // section = LinkedServer
                // key = TMS (или др.) */
        private async Task<string> GetIniString(string path,
                                                string section,
                                                string key = "",
                                                string def = "",
                                                uint bufSize = 256)
        {
            var buf = new byte[bufSize];

            const int errBufLength = 1000;
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            var result = await Task.Run(() => _native.CfsGetIniString(CfId,
                                                                      path, section, key, def,
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
            var errBuf = new byte[errBufLength];
            uint errCode = 0;

            var result = await Task.Run(() => _native.CfsSetIniString(CfId,
                                                                      path, section, key, value,
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
    }
}