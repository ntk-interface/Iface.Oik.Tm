using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
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
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Iface.Oik.Tm.Native.Interfaces.TmNativeDefs;
using static System.Runtime.CompilerServices.RuntimeHelpers;

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

		public async Task<(IntPtr, DateTime)> OpenConfigurationTree(string fileName)
		{
			var fileTime = new TmNativeDefs.FileTime();
			const int errStringLength = 1000;
			var errBuf = new byte[errStringLength];
			uint errCode = 0;

			var cfTreeRoot = await Task.Run(() => _native.CfsConfFileOpenCid(CfId,
																			 Host,
																			 fileName,
																			 30000 | TmNativeDefs.FailIfNoConnect,
																			 ref fileTime,
																			 out errCode,
																			 ref errBuf,
																			 errStringLength))
									   .ConfigureAwait(false);

			if (cfTreeRoot == IntPtr.Zero)
				throw new Exception($"Ошибка получения конфигурации: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");

			return (cfTreeRoot, GetDateTimeFromCustomFileTime(fileTime));
		}
		public async Task<(IntPtr, DateTime)> OpenMasterServiceConfiguration()
		{
			return await OpenConfigurationTree(TmNativeDefs.MasterConfFile).ConfigureAwait(false);
		}
		public async Task<(MSTreeNode, DateTime)> LoadFullMSTree()
		{
			var (handle, time) = await OpenMasterServiceConfiguration().ConfigureAwait(false);
			var tree = await GetCfTree(handle).ConfigureAwait(false);
			FreeConfigurationTreeHandle(handle);
			// считаем что дерево мастер-сервиса всегда начинается с одного элемента
			MSTreeNode msRoot = new MSTreeNode(tree.First());

			// читаем конфигурацию резервирования если есть
			List<CfTreeNode> res_tree = new List<CfTreeNode>();
			try
			{
				var (res_handle, _) = await OpenConfigurationTree(TmNativeDefs.HotStanbyConfFile).ConfigureAwait(false);
				res_tree = await GetCfTree(res_handle).ConfigureAwait(false);
				FreeConfigurationTreeHandle(res_handle);
			}
			catch { }

			// перебираем элементы на втором уровне после мастер-сервиса
			foreach (var node in msRoot.Children)
			{
				// ищем резервирование
				if (node.Properties is ReservedNodeProperties rn_p)
				{
					rn_p.AbortTO = 20;
					rn_p.RetakeTO = 20;
					foreach (var item in res_tree)
					{
						var tokens = item.Name.Split(':');
						if ((tokens.Length == 2) && tokens[1].Equals(rn_p.PipeName))
						{// нашли правильный сервер по PipeName, заполняем параметры резервирования
							short s;
							if (short.TryParse(item.CfProperties.ValueOrDefault(nameof(rn_p.Type), "0"), out s))
							{
								rn_p.Type = s;
							}
							rn_p.BindAddr = item.CfProperties.ValueOrDefault(nameof(rn_p.BindAddr), "");
							rn_p.Addr = item.CfProperties.ValueOrDefault(nameof(rn_p.Addr), "");
							if (short.TryParse(item.CfProperties.ValueOrDefault(nameof(rn_p.Port), "0"), out s))
								rn_p.Port = s;
							if (short.TryParse(item.CfProperties.ValueOrDefault(nameof(rn_p.BPort), "0"), out s))
								rn_p.BPort = s;
							if (short.TryParse(item.CfProperties.ValueOrDefault(nameof(rn_p.AbortTO), ""), out s))
								rn_p.AbortTO = s;
							if (short.TryParse(item.CfProperties.ValueOrDefault(nameof(rn_p.RetakeTO), ""), out s))
								rn_p.RetakeTO = s;
							rn_p.CopyConfig = item.CfProperties.ValueOrDefault(nameof(rn_p.CopyConfig), "0").Equals("1");
							rn_p.StopInactive = item.CfProperties.ValueOrDefault(nameof(rn_p.StopInactive), "1").Equals("1");
							break;
						}
					}
				}
				// ищем сервера БД
				if (node.Properties is RbsNodeProperties rbs_p)
				{
					if (node.ProgName.Equals(MSTreeConsts.rbsrv) || node.ProgName.Equals(MSTreeConsts.rbsrv_old))
					{
						//< Parameters RBF_Directory = "xx" />
						//< ClientParms DOC_Path = "xx" JournalSQLCS = "xx" DTMX_SQLCS = "xx" />
						//< PGParms BinPath = "xx" DataPath = "xx" />
						try
						{
							var (rbs_handle, _) = await OpenConfigurationTree($"{TmNativeDefs.RbsDirectory}\\{rbs_p.PipeName}\\{TmNativeDefs.RbsConfFile}").ConfigureAwait(false);
							var rbs_tree = await GetCfTree(rbs_handle).ConfigureAwait(false);
							FreeConfigurationTreeHandle(rbs_handle);
							if (rbs_tree != null)
							{
								foreach (var item in rbs_tree)
								{
									if (item.Name.Equals(MSTreeConsts.RBS_Parameters))
									{
										rbs_p.RBF_Directory = item.CfProperties.ValueOrDefault(nameof(rbs_p.RBF_Directory), "");
									}
									else
									if (item.Name.Equals(MSTreeConsts.RBS_ClientParms))
									{
										rbs_p.DOC_Path = item.CfProperties.ValueOrDefault(nameof(rbs_p.DOC_Path), "");
										rbs_p.JournalSQLCS = item.CfProperties.ValueOrDefault(nameof(rbs_p.JournalSQLCS), "");
										rbs_p.DTMX_SQLCS = item.CfProperties.ValueOrDefault(nameof(rbs_p.DTMX_SQLCS), "");
									}
									else
									if (item.Name.Equals(MSTreeConsts.RBS_PGParms))
									{

										rbs_p.BinPath = item.CfProperties.ValueOrDefault(nameof(rbs_p.BinPath), "");
										rbs_p.DataPath = item.CfProperties.ValueOrDefault(nameof(rbs_p.DataPath), "");
									}
								}
							}
						}
						catch { }
						//  параметры редиректора
						rbs_p.RedirectorPort = (short)await GetRedirectorPort(rbs_p.PipeName, 0).ConfigureAwait(false);
					}
				}
			}
			return (msRoot, time);
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
							case MSTreeConsts.rbsrv:
							case MSTreeConsts.rbsrv_old:
								p.PipeName = "RBS";
								break;
							case MSTreeConsts.pcsrv:
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

					var nodeHandle = _native.CftNodeInsertDown(res_handle, tag);
					SetNodeProperty(nodeHandle, nameof(p.Type), p.Type.ToString());
					SetNodeProperty(nodeHandle, nameof(p.BindAddr), p.BindAddr.Trim());
					SetNodeProperty(nodeHandle, nameof(p.Addr), p.Addr.Trim());
					SetNodeProperty(nodeHandle, nameof(p.Port), p.Port.ToString());
					SetNodeProperty(nodeHandle, nameof(p.BPort), p.BPort.ToString());
					SetNodeProperty(nodeHandle, nameof(p.AbortTO), p.AbortTO.ToString());
					SetNodeProperty(nodeHandle, nameof(p.RetakeTO), p.RetakeTO.ToString());
					SetNodeProperty(nodeHandle, nameof(p.CopyConfig), p.CopyConfig ? "1" : "0");
					SetNodeProperty(nodeHandle, nameof(p.StopInactive), p.StopInactive ? "1" : "0");
				}
			}
			await SaveConfigurationTree(res_handle, TmNativeDefs.HotStanbyConfFile).ConfigureAwait(false);
			FreeConfigurationTreeHandle(res_handle);

			// Конфигурации серверов RBS
			foreach (var server in msRoot.Children)
			{
				if (server.Properties is RbsNodeProperties rbs_p)
				{
					// общие параметры
					var rbs_handle = _native.CftNodeNewTree();
					IntPtr nodeHandle = _native.CftNodeInsertDown(rbs_handle, MSTreeConsts.RBS_Parameters);
					SetNodeProperty(nodeHandle, nameof(rbs_p.RBF_Directory), rbs_p.RBF_Directory);

					nodeHandle = _native.CftNodeInsertDown(rbs_handle, MSTreeConsts.RBS_ClientParms);
					SetNodeProperty(nodeHandle, nameof(rbs_p.DOC_Path), rbs_p.DOC_Path);
					SetNodeProperty(nodeHandle, nameof(rbs_p.DTMX_SQLCS), rbs_p.DTMX_SQLCS);
					SetNodeProperty(nodeHandle, nameof(rbs_p.JournalSQLCS), rbs_p.JournalSQLCS);

					nodeHandle = _native.CftNodeInsertDown(rbs_handle, MSTreeConsts.RBS_PGParms);
					SetNodeProperty(nodeHandle, nameof(rbs_p.BinPath), rbs_p.BinPath);
					SetNodeProperty(nodeHandle, nameof(rbs_p.DataPath), rbs_p.DataPath);
					await SaveConfigurationTree(rbs_handle, $"{TmNativeDefs.RbsDirectory}\\{rbs_p.PipeName}\\{TmNativeDefs.RbsConfFile}").ConfigureAwait(false);
					FreeConfigurationTreeHandle(rbs_handle);

					// параметры редиректора
					await SetRedirectorPort(rbs_p.PipeName, 0, (int)rbs_p.RedirectorPort).ConfigureAwait(false);
				}
			}
		}
		public async Task SaveConfigurationTree(IntPtr treeHandle, string filename)
		{
			var fileTime = new TmNativeDefs.FileTime();
			const int errStringLength = 1000;
			var errBuf = new byte[errStringLength];
			uint errCode = 0;

			var res = await Task.Run(() => _native.CfsConfFileSaveAs(treeHandle,
																  Host,
																  filename,
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
			await SaveConfigurationTree(treeHandle, TmNativeDefs.MasterConfFile).ConfigureAwait(false);
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

			for (var i = 0; ; i++)
			{
				var childHandle = _native.CftNodeEnumAll(parentHandle, i);
				if (childHandle == IntPtr.Zero)
					break;
				var nodeChild = new CfTreeNode(GetNodeName(childHandle), parent)
				{
					Disabled = !_native.CftNodeIsEnabled(childHandle),
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
			var nameBuf = new byte[nameBufLength];

			_native.CftNodeGetName(nodeHandle, ref nameBuf, nameBufLength);

			return EncodingUtil.Win1251BytesToUtf8(nameBuf);
		}

		private Dictionary<string, string> GetNodeProps(IntPtr nodeHandle)
		{
			var props = new Dictionary<string, string>();

			for (var i = 0; ; i++)
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
			var nameBuf = new byte[nameBufLength];

			_native.CftNPropEnum(nodeHandle, idx, ref nameBuf, nameBufLength);

			return EncodingUtil.Win1251BytesToUtf8(nameBuf);
		}

		private string GetPropValue(IntPtr nodeHandle, string propName)
		{
			const int valueBufLength = 200;
			var valueBuf = new byte[valueBufLength];

			_native.CftNPropGetText(nodeHandle, propName, ref valueBuf, valueBufLength);

			return EncodingUtil.Win1251BytesToUtf8(valueBuf);
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
			foreach(var node in tree)
			{
				CreateCfgNode(newTreeHandle, node);
			}
			return newTreeHandle;
		}
		private void CreateCfgNode(IntPtr parentNodeHandle, CfTreeNode node)
		{
			var nodeHandle = _native.CftNodeInsertDown(parentNodeHandle, node.Name);
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
			var tag = tagId == -1 ? "Master" : $"#{tagId:X3}";
			var nodeHandle = _native.CftNodeInsertDown(parentNodeHandle, tag);

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
			if (node.Properties.NoStart)
			{
				if (!SetNodeProperty(nodeHandle, MSTreeConsts.NoStart, "1")) return false;
			}
			switch (node.Properties)
			{
				case MasterNodeProperties _:
					if (!CreateMasterNodeProperties(nodeHandle, node)) 
						return false;
					break;
				case NewTmsNodeProperties _:
					if (!CreateNewTmsNodeProperties(nodeHandle, node)) 
						return false;
					break;
				case ExternalTaskNodeProperties _:
					if (!CreateExternalTaskNodeProperties(nodeHandle, node)) 
						return false;
					break;
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
				return false;

			if (node.ProgName.Equals(MSTreeConsts.portcore))
				if (!SetNodeProperty(nodeHandle, MSTreeConsts.WorkDir, props.WorkDir)) 
					return false;

			return true;
		}

		private bool CreateChildNodeProperties(IntPtr nodeHandle, MSTreeNode node)
		{
			var props = (ChildNodeProperties)node.Properties;

			if (!SetNodeProperty(nodeHandle, MSTreeConsts.PipeName, props.PipeName)) 
				return false;

			return true;
		}

		private bool CreateNewTmsNodeProperties(IntPtr nodeHandle, MSTreeNode node)
		{
			var props = (NewTmsNodeProperties)node.Properties;

			if (!CreateChildNodeProperties(nodeHandle, node)) 
				return false;
			if (!props.PassiveMode)
			{
				if (!SetNodeProperty(nodeHandle, MSTreeConsts.PassiveMode, Convert.ToInt32(props.PassiveMode).ToString())) 
					return false;
			}
			return true;
		}

		private bool CreateExternalTaskNodeProperties(IntPtr nodeHandle, MSTreeNode node)
		{
			var props = (ExternalTaskNodeProperties)node.Properties;

			if (!CreateChildNodeProperties(nodeHandle, node)) 
				return false;

			// Зачем то в пути внешней задачи пробелы замеяются на табуляции
			if (!SetNodeProperty(nodeHandle, MSTreeConsts.TaskPath, props.TaskPath.Replace(' ', '\t'))) 
				return false;

			if (!SetNodeProperty(nodeHandle, MSTreeConsts.TaskArguments, props.TaskArguments)) 
				return false;

			if (!SetNodeProperty(nodeHandle, MSTreeConsts.ConfFilePath, props.ConfigurationFilePath)) 
				return false;

			return true;
		}

		private bool SetNodeProperty(IntPtr nodeHandle, string propName, string propText)
		{
			var arr_propText = EncodingUtil.Utf8ToWin1251Bytes(propText);
			return _native.CftNPropSet(nodeHandle, propName, arr_propText);
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
			var errBuf = new byte[errStringLength];
			uint errCode = 0;

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

		public async Task<IReadOnlyCollection<TmServerLogRecord>> GetTmServersLog(int maxRecords, DateTime? startTime, DateTime? endTime)
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
						int.TryParse(tokens2[1].Replace('s', ' ').Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out int upTime);
						float.TryParse(tokens2[2].Replace('s', ' ').Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float workTime);
						tmServerThreadsList.Add(new TmServerThread(id, name.Trim(), upTime, workTime));
					}
				}
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
			bool result;
			(result, _, _) = await GetFile(localPath, "@dispserv.ini").ConfigureAwait(false);
			return result;
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
				var parts = keyData.Split('=');
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


		public async Task<(bool, string)> PutFile(string localFilePath,
								  string remoteFilePath,
								  uint timeout = 20000)
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
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

			if (!await Task.Run(() => _native.CfsFilePut(CfId, 
														 remoteFilePath, 
														 localFilePath, 
														 timeout | TmNativeDefs.FailIfNoConnect, 
														 out errCode,
														 ref errBuf, errBufLength)).ConfigureAwait(false))
			{
				return (false, $"Ошибка при отправке файла: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errBuf)}");
			}
			return (true, string.Empty);
		}
		public async Task<(bool, string, DateTime)> GetFile(string localFilePath, string remoteFilePath, uint timeout = 20000)
		{
			if (localFilePath.IsNullOrEmpty())
			{
				return (false, "Ошибка: не указан локальный путь до файла", DateTime.MinValue);
			}
			if (remoteFilePath.IsNullOrEmpty())
			{
				return (false, "Ошибка: не указан удалённый путь до файла", DateTime.MinValue);
			}
			var fileTime = new TmNativeDefs.FileTime();
			const int errStringLength = 1000;
			var errString = new byte[errStringLength];
			uint errCode = 0;
			if (!await Task.Run(() => _native.CfsFileGet(CfId,
														 remoteFilePath,
														 localFilePath,
														 timeout | TmNativeDefs.FailIfNoConnect,
														 ref fileTime,
														 out errCode,
														 ref errString,
														 errStringLength))
						   .ConfigureAwait(false))
			{
				return (false, $"Ошибка при скачивании файла: {errCode} - {EncodingUtil.Win1251BytesToUtf8(errString)}", DateTime.MinValue);
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
			var licenseKeyTypes = new List<LicenseKeyType>();

			var typesStrings = (await GetIniString(path, section, bufSize: bufSize).ConfigureAwait(false)).Split(new[] { ';' },
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
			const string key = "COM";
			
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
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

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


		public async Task<IReadOnlyCollection<SLogRecord>> GetSecurityLogFull(SLogReadDirection readDirection = SLogReadDirection.FromEnd)
		{
			return await GetSecurityLog(0,
										readDirection,
										readDirection == SLogReadDirection.FromEnd ? SLogIndex.Last : SLogIndex.First)
					   .ConfigureAwait(false);
		}


		public async Task<IReadOnlyCollection<SLogRecord>> GetAdministratorLogFull(SLogReadDirection readDirection = SLogReadDirection.FromEnd)
		{
			return await GetAdministratorLog(0,
										readDirection,
										readDirection == SLogReadDirection.FromEnd ? SLogIndex.Last : SLogIndex.First)
					   .ConfigureAwait(false);
		}


		public async Task<IReadOnlyCollection<SLogRecord>> GetSecurityLog(int maxRecords,
																		  SLogReadDirection readDirection = SLogReadDirection.FromEnd,
																		  uint startIndex = SLogIndex.Last,
																		  DateTime? startTime = null,
																		  DateTime? endTime = null)
		{
			return await GetSLog(SLogType.Security, readDirection, startIndex, maxRecords, startTime, endTime)
					   .ConfigureAwait(false);
		}


		public async Task<IReadOnlyCollection<SLogRecord>> GetAdministratorLog(int maxRecords,
																			   SLogReadDirection readDirection = SLogReadDirection.FromEnd,
																			   uint startIndex = SLogIndex.Last,
																			   DateTime? startTime = null,
																			   DateTime? endTime = null)
		{
			return await GetSLog(SLogType.Administrator, readDirection, startIndex, maxRecords, startTime, endTime)
					   .ConfigureAwait(false);
		}


		public async Task<IReadOnlyCollection<SLogRecord>> GetSLog(SLogType logType,
																   SLogReadDirection readDirection,
																   uint startIndex,
																   int maxRecords,
																   DateTime? startTime,
																   DateTime? endTime)
		{
			var logHandle = await OpenSLog(logType, readDirection, startIndex).ConfigureAwait(false);

			var log = new List<SLogRecord>();

			while (true)
			{
				var (logPart, shouldContinue) = await ReadSLogRecordsBatch(logHandle, readDirection, startTime, endTime).ConfigureAwait(false);

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


		public async Task<ulong> OpenSLog(SLogType logType,
										  SLogReadDirection direction,
										  uint startIndex)
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

			var sLogHandle = await Task.Run(() => _native.СfsSLogOpen(CfId,
																  (uint)logType,
																  startIndex,
																  (uint)direction,
																  out errCode,
																  ref errBuf,
																  errBufLength)).ConfigureAwait(false);

			if (sLogHandle == 0)
			{
				throw new Exception($"Ошибка открытия журнала безопасности. \nТип: ${logType}\nОшибка: {EncodingUtil.Win1251BytesToUtf8(errBuf)} Код: {errCode}");
			}

			return sLogHandle;
		}


		public async Task<(IReadOnlyCollection<SLogRecord> logPart, bool shouldContinue)> ReadSLogRecordsBatch(ulong sLogHandle,
																		   SLogReadDirection readDirection,
																		   DateTime? startTime,
																		   DateTime? endTime)
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];
			uint errCode = 0;


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
			var startTimeUtc = startTime.HasValue ? TimeZoneInfo.ConvertTimeToUtc(startTime.Value) : (DateTime?)null;
			var endTimeUtc = endTime.HasValue ? TimeZoneInfo.ConvertTimeToUtc(endTime.Value) : (DateTime?)null;

			var logPart = new List<SLogRecord>();
			var nextPtr = strPtr;

			do
			{
				var index = TmNativeUtil.GetDoubleNullTerminatorIndexFromPointer(nextPtr);
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
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

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
				return -1;
			}

			return port;
		}


		public async Task<byte[]> GetBin(string uName,
										 string oName,
										 string binName)
		{
			(var binData, uint errCode, _) = await SecGetBin(uName, oName, binName).ConfigureAwait(false);

			if (errCode == 0)
			{
				return binData;
			}
			else
			{
				return Array.Empty<byte>();
			}

		}

		public async Task<(byte[], uint, string)> SecGetBin(string uName,
										 string oName,
										 string binName)
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

			uint binLength = 0;

			var resultPtr = await Task.Run(() => _native.CfsIfpcGetBin(CfId,
																	   uName,
																	   oName,
																	   binName,
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
			_native.TmcFreeMemory(resultPtr);

			return (binData, 0, string.Empty);
		}


		public async Task<bool> SetRedirectorPort(string pipeName, int portIndex, int port)
		{
			var portStr = $"{port}";
			var binData = TmNativeUtil.GetFixedBytesWithTrailingZero(portStr,
																	 portStr.Length + 1,
																	 "windows-1251");

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
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

			var result = await Task.Run(() => _native.CfsIfpcSetBin(CfId,
															  uName,
															  oName,
															  binName,
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

		public AccessMasksDescriptor SecGetAccessDescriptor(string ProgName)
		{
			var ad = new AccessMasksDescriptor();
			Dictionary<string, string> iniSections = new Dictionary<string, string>()
				{
					{ MSTreeConsts.portcore,  "master#1.prp.Security"},
					{ MSTreeConsts.master,    "master.prp.Security"},
					{ MSTreeConsts.rbsrv,     "rbsrv#1.prp.Security"},
					{ MSTreeConsts.rbsrv_old, "serv_dll.ch.RbsSecurity"},
					{ MSTreeConsts.pcsrv,     "pcsrv#1.prp.Security"},
					{ MSTreeConsts.pcsrv_old, "serv_dll.ch.TmsSecurity"},
				};

			if (iniSections.TryGetValue(ProgName, out var section))
			{
				var sec_ptr = _native.CfsGetAccessDescriptor("s_setup.ini", section);
				if (sec_ptr == IntPtr.Zero)
					throw new Exception("GetAccessDescriptor sec_ptr error");
				var cfs_ad = Marshal.PtrToStructure<TmNativeDefs.CfsAccessDescriptor>(sec_ptr);
				_native.TmcFreeMemory(sec_ptr);

				ad.ObjTypeName["ru"] = EncodingUtil.Win1251BytesToUtf8(cfs_ad.ObjTypeName.rus).Replace("&", "");
				ad.ObjTypeName["en"] = EncodingUtil.Win1251BytesToUtf8(cfs_ad.ObjTypeName.eng).Replace("&", "");
				var pre = cfs_ad.NamePrefix.Split('$');
				if (pre.Length > 1)
					ad.NamePrefix = pre[0] + "$";
				else
					ad.NamePrefix = cfs_ad.NamePrefix;

				for (int bit = 0; bit < 32; bit++)
				{
					if (cfs_ad.Bit[bit].Mask != 0xffffffff)
					{
						var newMask = new AccessMask() { Mask = cfs_ad.Bit[bit].Mask };
						newMask.Description["ru"] = EncodingUtil.Win1251BytesToUtf8(cfs_ad.Bit[bit].rus).Replace("&", "");
						newMask.Description["en"] = EncodingUtil.Win1251BytesToUtf8(cfs_ad.Bit[bit].eng).Replace("&", "");
						ad.AccessMasks.Add(newMask);
					}
				}
				return ad;
			}
			else
				return null;
		}

		public ExtendedRightsDescriptor SecGetExtendedRightsDescriptor()
		{
			var ret = new ExtendedRightsDescriptor();
			var ext_ptr = _native.CfsGetExtendedUserRightsDescriptor("s_setup.ini", "TmsExtRights", 0);
			if (ext_ptr == IntPtr.Zero)
				return null;

			var er = Marshal.PtrToStructure<TmNativeDefs.CfsExtSrvrtDescriptor>(ext_ptr);
			_native.TmcFreeMemory(ext_ptr);

			ret.DoUserID = er.DoUserID;
			ret.DoUserPwd = er.DoUserPwd;
			ret.DoUserNick = er.DoUserNick;
			ret.MaxUserID = er.MaxUserID;
			ret.DoGroup = er.DoGroup;
			ret.DoKeyID = er.DoKeyID;
			var strRights = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(er.Rights, 10240);
			foreach (var item in strRights)
			{
				if (item.Length < 2) continue;

				var Right = new ExtendedRight();

				if (item[0] == 'B')
				{
					Right.IsHeader = true;
					var Descriptions = item.Substring(1).Split('`');
					if (Descriptions.Length == 2)
					{
						Right.Description["ru"] = Descriptions[1];
						Right.Description["en"] = Descriptions[0];
						ret.Rights.Add(Right);
					}
				}
				else
				if (item[0] == 'R')
				{
					Right.IsHeader = false;
					var BitAndDesc = item.Substring(1).Split('-');
					if (BitAndDesc.Length == 2)
					{
						if (byte.TryParse(BitAndDesc[0], out byte bn))
						{
							Right.ByteIndex = bn;
							var Descriptions = BitAndDesc[1].Split('`');
							if (Descriptions.Length == 2)
							{
								Right.Description["ru"] = Descriptions[1];
								Right.Description["en"] = Descriptions[0];
								ret.Rights.Add(Right);
							}
						}
					}
				}
			}
			return ret;
		}

		public async Task<(IReadOnlyCollection<string>, uint, string)> SecEnumUsers()
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];
			uint errCode = 0;
			var resultPtr = await Task.Run(() => _native.СfsIfpcEnumUsers(CfId, out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
			if (errCode != 0)
			{
				return (null, errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
			}
			return (TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(resultPtr, 16384), 0, string.Empty);
		}
		public async Task<(IReadOnlyCollection<string>, uint, string)> SecEnumOSUsers()
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];
			uint errCode = 0;
			// сделаем отдельное соединение чтобы не занимать долгой выборкой основное
			IntPtr temp_cfsid = await Task.Run(() => _native.CfsConnect(Host, out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
			if ((temp_cfsid == IntPtr.Zero) && (errCode != 0))
			{
				return (null, errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
			}
			var resultPtr = await Task.Run(() => _native.СfsIfpcEnumOSUsers(temp_cfsid, out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
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
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

			if ((username.Length > 0) && !username.StartsWith("*"))
			{
				return (12345, "Смена пароля допустима только у собственных пользователей");
			}

			var result = await Task.Run(() => _native.CfsIfpcSetUserPwd(CfId, username, password, out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
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
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

			var result = await Task.Run(() => _native.СfsIfpcDeleteUser(CfId, username, out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
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
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

			var result = await Task.Run(() => _native.СfsIfpcGetAccess(CfId, username, oName, out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
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
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

			var result = await Task.Run(() => _native.СfsIfpcSetAccess(CfId, username, oName, AccessMask, out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
			if (errCode != 0)
			{
				return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
			}
			else
			{
				return (0, string.Empty);
			}
		}
		public async Task<(ExtendedUserData, uint, string)> SecGetExtendedUserData(string serverType, string serverName, string username)
		{
			(var resultPtr, uint errCode, string errString) = await SecGetBin(username, serverType + serverName, "extr").ConfigureAwait(false);
			if ((errCode != 0) || (resultPtr.Length == 0))
			{
				return (null, errCode, errString);
			}
			var ui = new ExtendedUserData();
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
				else
				if (KeyValuePair.Length == 1)
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
		public async Task<(uint, string)> SecSetExtendedUserData(string serverType, string serverName, string username, ExtendedUserData extendedUserData)
		{
			var data = new List<string>()
			{
				{ $"UserID={extendedUserData.UserID}"},
				{ $"UserNick={extendedUserData.UserNick}"},
				{ $"UserPwd={extendedUserData.UserPwd}"},
				{ $"Group={extendedUserData.Group}"},
				{ $"KeyID={extendedUserData.KeyID}"},
			};
			for (int idx = 0; idx < extendedUserData.Rights.Length; idx++)
			{
				if (extendedUserData.Rights[idx] == 1)
				{
					data.Add("R" + idx);
				}
			}
			var bin = TmNativeUtil.GetDoubleNullTerminatedBytesFromStringList(data);
			(uint errCode, string errString) = await SecSetBin(username, serverType + serverName, "extr", bin).ConfigureAwait(false);
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
			uint errCode, resErrCode = 0; ;
			string errString, resErrString = string.Empty;
			var _UserPolicy = new UserPolicy();

			(bin, errCode, errString) = await SecGetBin(username, ".", "bad_logon").ConfigureAwait(false);
			if (errCode == 0)
			{
				string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
				if (Int32.TryParse(s, out int i))
				{
					_UserPolicy.BadLogonCount = i;
				}
			}
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
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
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
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
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
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
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
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
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
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
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
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
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
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
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
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
				else
				if (errCode != 2) // "No data"
				{
					resErrCode = errCode;
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
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
				resErrString += errString;
			}

			(bin, errCode, errString) = await SecGetBin(username, ".", "utmpl").ConfigureAwait(false);
			if (errCode == 0)
			{
				_UserPolicy.UserTemplate = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
			}
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
				resErrString += errString;
			}

			return (_UserPolicy, resErrCode, resErrString);
		}
		public async Task<(uint, string)> SecSetUserPolicy(string username, UserPolicy userPolicy)
		{
			byte[] bin;
			uint errCode, resErrCode = 0;
			string errString, resErrString = string.Empty;
			string enc = "windows-1251";


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
				resErrCode = errCode;
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
				resErrCode = errCode;
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
			bin = TmNativeUtil.GetFixedBytesWithTrailingZero(dt, dt.Length + 1, enc);
			(errCode, errString) = await SecSetBin(username, ".", "not_before", bin).ConfigureAwait(false);
			if (errCode != 0)
			{
				resErrCode = errCode;
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
			bin = TmNativeUtil.GetFixedBytesWithTrailingZero(dt, dt.Length + 1, enc);
			(errCode, errString) = await SecSetBin(username, ".", "not_after", bin).ConfigureAwait(false);
			if (errCode != 0)
			{
				resErrCode = errCode;
				resErrString += errString;
			}

			string n = userPolicy.BadLogonLimit.ToString();
			bin = TmNativeUtil.GetFixedBytesWithTrailingZero(n, n.Length + 1, enc);
			(errCode, errString) = await SecSetBin(username, ".", "logon_limit", bin).ConfigureAwait(false);
			if (errCode != 0)
			{
				resErrCode = errCode;
				resErrString += errString;
			}

			bin = TmNativeUtil.GetFixedBytesWithTrailingZero(userPolicy.UserCategory, userPolicy.UserCategory.Length + 1, enc);
			(errCode, errString) = await SecSetBin(username, ".", "uctgr", bin).ConfigureAwait(false);
			if (errCode != 0)
			{
				resErrCode = errCode;
				resErrString += errString;
			}

			bin = TmNativeUtil.GetFixedBytesWithTrailingZero(userPolicy.UserTemplate, userPolicy.UserTemplate.Length + 1, enc);
			(errCode, errString) = await SecSetBin(username, ".", "utmpl", bin).ConfigureAwait(false);
			if (errCode != 0)
			{
				resErrCode = errCode;
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
						byte[] MAC = new byte[6];
						bool good = true;
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
				resErrCode = errCode;
				resErrString += errString;
			}

			return (resErrCode, resErrString);
		}
		public async Task<(PasswordPolicy, uint, string)> SecGetPasswordPolicy()
		{
			byte[] bin;
			uint errCode, resErrCode = 0; ;
			string errString, resErrString = string.Empty;
			var passwordPolicy = new PasswordPolicy();

			(bin, errCode, errString) = await SecGetBin(".cfs.", ".", "own_pch").ConfigureAwait(false);
			if (errCode == 0)
			{
				string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
				if (Int32.TryParse(s, out int i))
				{
					passwordPolicy.AdminPasswordChange = (i == 0);
				}
			}
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
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
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
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
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
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
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
				resErrString += errString;
			}

			(bin, errCode, errString) = await SecGetBin(".cfs.", ".", "pwd_pol_flg").ConfigureAwait(false);
			if (errCode == 0)
			{
				string s = TmNativeUtil.GetStringFromBytesWithAdditionalPart(bin);
				if (uint.TryParse(s, out uint ui))
				{
					PWDPOL flags = (PWDPOL)ui;
					passwordPolicy.PwdChars_Upper = flags.HasFlag(PWDPOL.Upper);
					passwordPolicy.PwdChars_Digits = flags.HasFlag(PWDPOL.Digits);
					passwordPolicy.PwdChars_Special = flags.HasFlag(PWDPOL.Spec);
					passwordPolicy.PwdChars_NoRepeat = flags.HasFlag(PWDPOL.CheckRepeat);
					passwordPolicy.PwdChars_NoSequential = flags.HasFlag(PWDPOL.CheqSeq);
					passwordPolicy.PwdChars_CheckDictonary = flags.HasFlag(PWDPOL.CheckDict);
					passwordPolicy.CheckOldPasswords = flags.HasFlag(PWDPOL.CheckCache);
				}
			}
			else
			if (errCode != 2) // "No data"
			{
				resErrCode = errCode;
				resErrString += errString;
			}

			return (passwordPolicy, resErrCode, resErrString);
		}
		public async Task<(uint, string)> SecSetPasswordPolicy(PasswordPolicy passwordPolicy)
		{
			byte[] bin;
			string n;
			uint errCode, resErrCode = 0;
			string errString, resErrString = string.Empty;
			string enc = "windows-1251";


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
				resErrCode = errCode;
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
				resErrCode = errCode;
				resErrString += errString;
			}

			n = passwordPolicy.MinPasswordLength.ToString();
			bin = TmNativeUtil.GetFixedBytesWithTrailingZero(n, n.Length + 1, enc);
			(errCode, errString) = await SecSetBin(".cfs.", ".", "pwd_pol_len", bin).ConfigureAwait(false);
			if (errCode != 0)
			{
				resErrCode = errCode;
				resErrString += errString;
			}

			n = passwordPolicy.PasswordTTL_Days.ToString();
			bin = TmNativeUtil.GetFixedBytesWithTrailingZero(n, n.Length + 1, enc);
			(errCode, errString) = await SecSetBin(".cfs.", ".", "p_ex_days", bin).ConfigureAwait(false);
			if (errCode != 0)
			{
				resErrCode = errCode;
				resErrString += errString;
			}

			PWDPOL flags = (PWDPOL)0xff_ff_ff_ff;
			if (!passwordPolicy.PwdChars_Upper) flags &= ~PWDPOL.Upper;
			if (!passwordPolicy.PwdChars_Digits) flags &= ~PWDPOL.Digits;
			if (!passwordPolicy.PwdChars_Special) flags &= ~PWDPOL.Spec;
			if (!passwordPolicy.PwdChars_NoRepeat) flags &= ~PWDPOL.CheckRepeat;
			if (!passwordPolicy.PwdChars_NoSequential) flags &= ~PWDPOL.CheqSeq;
			if (!passwordPolicy.PwdChars_CheckDictonary) flags &= ~PWDPOL.CheckDict;
			if (!passwordPolicy.CheckOldPasswords) flags &= ~PWDPOL.CheckCache;
			string s_flg = flags.ToString();
			bin = TmNativeUtil.GetFixedBytesWithTrailingZero(s_flg, s_flg.Length + 1, enc);
			(errCode, errString) = await SecSetBin(".cfs.", ".", "pwd_pol_flg", bin).ConfigureAwait(false);
			if (errCode != 0)
			{
				resErrCode = errCode;
				resErrString += errString;
			}

			return (resErrCode, resErrString);
		}
		public async Task<(ComputerInfo, uint, string)> GetComputerInfo()
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

			var _computerInfoS = new ComputerInfoS()
			{
				Len = (uint)Marshal.SizeOf(typeof(ComputerInfoS))
			};
			await Task.Run(() => _native.CfsGetComputerInfo(CfId, ref _computerInfoS, out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
			if (errCode != 0)
			{
				return (null, errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
			}
			else
			{
				var computerInfo = new ComputerInfo
				{
					ComputerName = _computerInfoS.ComputerName,
					PrimaryDomainName = _computerInfoS.DomInfo.PrimaryDomainName,
					OS_ProductType = $"ostype{_computerInfoS.NtProductType}",
					OS_Version = $"{_computerInfoS.NtVerMaj}.{_computerInfoS.NtVerMin} build {_computerInfoS.NtBuild}",
					Architecture = (_computerInfoS.Win64 == 1) ? "x64" : "x86",
					Acp = _computerInfoS.Acp,

					ServerTimeGMT = DateUtil.GetDateTimeFromTimestamp(_computerInfoS.CurrentGMT, _computerInfoS.CurrentMs),
					Uptime = _computerInfoS.Uptime,

					Copyright = $"oiktype{_computerInfoS.Copyright}",
					CfsVer = $"{_computerInfoS.CfsVerMaj}.{_computerInfoS.CfsVerMin}",

					UserName = _computerInfoS.UserName,
					UserAddr = _computerInfoS.UserAddr,
					AccessMask = _computerInfoS.AccessMask,
					IpAddrs = new List<string>()
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
						computerInfo.BuildDate = await GetIniString(path, "Info", "BuildTime").ConfigureAwait(false);
						computerInfo.InstallDate = await GetIniString(path, "Info", "InstTime").ConfigureAwait(false);
					}
					else
						computerInfo.InstallDate = await GetIniString("@@", "IInfo", "InstTime").ConfigureAwait(false);
				}
				catch{}


				return (computerInfo, 0, string.Empty);
			}
		}
		private static string BackupDateFormat = "dd_MM_yyyy (HH.mm.ss)";
		public async Task<(bool, string)> SaveMachineConfig(string directory, bool full)
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];
			string fileName;
			if (full)
			{
				fileName = "FullConf-" + DateTime.Now.ToString(BackupDateFormat) + ".cfim";
			}
			else
			{
				fileName = "MasterConf-" + DateTime.Now.ToString(BackupDateFormat)+".pkf";
			}
			var result = await Task.Run(() => _native.CfsSaveMachineConfig(full, Host, Path.Combine(directory, fileName), ref errBuf, errBufLength)).ConfigureAwait(false);
			if (result != true)
			{
				return (false, EncodingUtil.Win1251BytesToUtf8(errBuf));
			}
			else
			{
				return (true, string.Empty);
			}
		}
		public async Task<(bool, string)> SaveMachineConfigEx(string directory, uint scope, 
															  TmNativeCallback callback = null,
															  IntPtr callbackParameter = default)
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];
			string fileName="undefined";
			if (scope == 0)//#define CFS_SMC_DEV		0
			{
				fileName = "MasterConf-" + DateTime.Now.ToString(BackupDateFormat) + ".pkf";
			}
			else
			if (scope == 1)//#define CFS_SMC_MEDIUM		1
			{
				fileName = "FullConf-" + DateTime.Now.ToString(BackupDateFormat) + ".cfim";
			}
			else
			if (scope == 2)//#define CFS_SMC_COMPLETE	2
			{
				fileName = "FullConfRetro-" + DateTime.Now.ToString(BackupDateFormat) + ".cfim";
			}
				var result = await Task.Run(() => _native.CfsSaveMachineConfigEx(
					Host,
					Path.Combine(directory, fileName),
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
		public async Task<(IReadOnlyCollection<string>, uint, string)> DirEnum(string Path)
		{
			const int resBufLength = 20480;
			var resBuf = new char[resBufLength];
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];
			uint errCode = 0;

			var result = await Task.Run(() => _native.CfsDirEnum(CfId, Path, ref resBuf, resBufLength, out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
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
											 TmNativeCallback callback=null,
											 IntPtr callbackParameter = default(IntPtr))
		{
			uint bflags;
			bool result = false;
			switch (progName)
			{
				case MSTreeConsts.pcsrv:
				case MSTreeConsts.pcsrv_old:
					//#define TMS_BACKUP_CONFIG	1
					//#define TMS_BACKUP_ARRAY	2
					//#define TMS_BACKUP_EVLOG	4
					//#define TMS_BACKUP_ALARMS	8
					//#define TMS_BACKUP_RETRO	0x10
					//#define TMS_BACKUP_SECURITY	0x20
					bflags =  1 | 2 | 4 | 8; if (withRetro) bflags |= 0x10;
					result = await Task.Run(() => _native.TmcBackupServerProcedure(Host, pipeName, directory, ref bflags, 0, callback, callbackParameter)).ConfigureAwait(false);
					break;
				case MSTreeConsts.rbsrv:
				case MSTreeConsts.rbsrv_old:
					//#define RBS_BACKUP_BASES	1
					//#define RBS_BACKUP_SECURITY 2
					bflags = 1;
					result = await Task.Run(() => _native.RbcBackupServerProcedure(Host, pipeName, directory, ref bflags, 0, callback, callbackParameter)).ConfigureAwait(false);
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
			bool result=false;
			switch (progName)
			{
				case MSTreeConsts.pcsrv:
				case MSTreeConsts.pcsrv_old:
					bflags = 1 | 2 | 4 | 8; if (withRetro) bflags |= 0x10;
					result = await Task.Run(() => _native.TmcRestoreServer(true, Host, pipeName, filename, ref bflags, 0, callback, callbackParameter)).ConfigureAwait(false);
					break;
				case MSTreeConsts.rbsrv:
				case MSTreeConsts.rbsrv_old:
					bflags = 1;
					result = await Task.Run(() => _native.TmcRestoreServer(false, Host, pipeName, filename, ref bflags, 0, callback, callbackParameter)).ConfigureAwait(false);
					break;
			}
			if (bflags == 0)
				return (result, "nothing restored");
			if (result)
				return (result, string.Empty);
			else
				return (result, "error");
		}
		public async Task<(uint, string)> BackupSecurity(string directory, string pwd="")
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];
			uint errCode = 0;
			string fileName = "", snp = "";

			if (String.IsNullOrEmpty(pwd))
			{
				snp = "\x1";
				var _computerInfoS = new ComputerInfoS()
				{
					Len = (uint)Marshal.SizeOf(typeof(ComputerInfoS))
				};
				await Task.Run(() => _native.CfsGetComputerInfo(CfId, ref _computerInfoS, out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
				if (errCode != 0)
				{
					return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
				}
				switch(_computerInfoS.SecType)
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
				snp = "\x2";
				fileName = "Security-" + DateTime.Now.ToString(BackupDateFormat) + ".sbk";
			}

			await Task.Run(() => _native.CfsIfpcBackupSecurity(CfId, snp, pwd, Path.Combine(directory, fileName), out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
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
			var errBuf = new byte[errBufLength];
			uint errCode = 0;
			string snp = "\x2";

			await Task.Run(() => _native.CfsIfpcRestoreSecurity(CfId, snp, pwd, filename, out errCode, ref errBuf, errBufLength)).ConfigureAwait(false);
			if (errCode != 0)
			{
				return (errCode, EncodingUtil.Win1251BytesToUtf8(errBuf));
			}
			else
			{
				return (0, string.Empty);
			}
		}
		public async Task<(IReadOnlyCollection<string>, string)> EnumPackedFiles(string pkfname)
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];

			var result = await Task.Run(() => _native.PkfEnumPackedFiles(pkfname, ref errBuf, errBufLength)).ConfigureAwait(false);
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
		public async Task<(IReadOnlyCollection<string>, string)> UnPack(string pkfname, string dirname)
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];

			var result = await Task.Run(() => _native.PkfUnPack(pkfname, dirname, ref errBuf, errBufLength)).ConfigureAwait(false);
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
		public async Task<(bool, string)> ExtractFile(string pkfname, string filename, string dirname)
		{
			const int errBufLength = 1000;
			var errBuf = new byte[errBufLength];

			var result = await Task.Run(() => _native.PkfExtractFile(pkfname, filename, dirname, ref errBuf, errBufLength)).ConfigureAwait(false);
			if (!result)
			{
				return (false, EncodingUtil.Win1251BytesToUtf8(errBuf));
			}
			else
			{
				return (true, string.Empty);
			}
		}
	}
}