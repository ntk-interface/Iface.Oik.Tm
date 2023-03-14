using Iface.Oik.Tm.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Iface.Oik.Tm.Helpers;

namespace Iface.Oik.Tm.Interfaces
{
	public class CfTreeNode
	{
		public string Name { get; }
		public CfTreeNode Parent { get; }
		public IDictionary<string, string> CfProperties { get; set; }
		public ICollection<CfTreeNode> Children { get; set; }
		public CfTreeNode()
		{
			Children = new List<CfTreeNode>();
		}
		public CfTreeNode(string nodeName, CfTreeNode parent = null)
		{
			Name = nodeName;
			Parent = parent;
			Children = new List<CfTreeNode>();
		}
	}
	public class MSTreeNode
	{
		public string ProgName { get; }
		public MSTreeNode Parent { get; }
		public ICollection<MSTreeNode> Children { get; set; } = new List<MSTreeNode>();
		public MSTreeNodeProperties Properties { get; protected set; }
		public MSTreeNode(CfTreeNode cft_node, MSTreeNode parent = null)
		{
			Parent = parent;
			ProgName = Cfs.GetValueOrDefault(cft_node.CfProperties, MSTreeConsts.ProgName);
			string pipeName = Cfs.GetValueOrDefault(cft_node.CfProperties, MSTreeConsts.PipeName);
			bool noStart = Cfs.GetValueOrDefault(cft_node.CfProperties, MSTreeConsts.NoStart) == "1";

			Properties = new MSTreeNodeProperties();
			if (cft_node.Name == "Master")
			{
				Properties = new MasterNodeProperties
				{
					LogFileSize = int.Parse(Cfs.GetValueOrDefault(cft_node.CfProperties, MSTreeConsts.LogFileSize, "0x80000")),
					WorkDir = Cfs.GetValueOrDefault(cft_node.CfProperties, MSTreeConsts.WorkDir)
				};
			}
			switch (ProgName)
			{
				case MSTreeConsts.pcsrv:
					Properties = new NewTmsNodeProperties
					{
						// Пассивный режим только для TMS под Ifpcore
						PassiveMode = !(Cfs.GetValueOrDefault(cft_node.CfProperties, MSTreeConsts.PassiveMode) == "1"),
					};
					break;
				case MSTreeConsts.rbsrv:
					Properties = new RbsNodeProperties();
					break;
				case MSTreeConsts.delta:
				case MSTreeConsts.delta_old:
				case MSTreeConsts.tmcalc:
				case MSTreeConsts.tmcalc_old:
				case MSTreeConsts.toposrv:
					Properties = new ChildNodeProperties();
					break;
				case MSTreeConsts.ext_task:
				case MSTreeConsts.ext_task_old:
					Properties = new ExternalTaskNodeProperties
					{
						TaskPath = Cfs.GetValueOrDefault(cft_node.CfProperties, MSTreeConsts.TaskPath),
						TaskArguments = Cfs.GetValueOrDefault(cft_node.CfProperties, MSTreeConsts.TaskArguments),
						ConfigurationFilePath = Cfs.GetValueOrDefault(cft_node.CfProperties, MSTreeConsts.ConfFilePath)
					};
					break;
				case MSTreeConsts.gensrv:
					string t = Cfs.GetValueOrDefault(cft_node.CfProperties, MSTreeConsts.TaskPath).Trim();

					if (t.Equals("tmserv.dll"))
					{
						ProgName = t;
						Properties = new ReservedNodeProperties();
					}
					else
					{
						if (t.Equals("rbase.dll"))
						{
							ProgName = t;
							Properties = new ReservedNodeProperties();
						}
					}
					break;
			}
			Properties.NoStart = noStart;
			if(Properties is ChildNodeProperties p)
			{
				p.PipeName = pipeName;
			}
			foreach(var child in cft_node.Children)
			{
				Children.Add(new MSTreeNode(child, this));
			}
		}
	}

	public class MSTreeNodeProperties
	{
		public bool NoStart { get; set; }
	}
	public class MasterNodeProperties : MSTreeNodeProperties
	{
		public int LogFileSize { get; set; }
		public string WorkDir { get; set; }
	}
	public class ChildNodeProperties : MSTreeNodeProperties
	{
		public string PipeName { get; set; }
	}
	public class NewTmsNodeProperties : ReservedNodeProperties
	{
		public bool PassiveMode { get; set; }
	}
	public class RbsNodeProperties : ReservedNodeProperties
	{
		public string PublicDocumentsPath { get; set; }
		public string PostgresBin { get; set; }
		public string PostgresData { get; set; }
		public short PostgresPort { get; set; }
		public string RbaseData { get; set; }
		public string JournalsConnString { get; set; }
		public string DataLoggerConnString { get; set; }
	}
	public class ExternalTaskNodeProperties : ChildNodeProperties
	{
		public string TaskPath { get; set; }
		public string TaskArguments { get; set; }
		public string ConfigurationFilePath { get; set; }
	}
	public class ReservedNodeProperties : ChildNodeProperties
	{
		public ReserveRoles Role { get; set; } = ReserveRoles.None;
		public string BindAddr { get; set; } = "";
		public string Addr { get; set; } = "";
		public short Port { get; set; }
		public short BPort { get; set; }
		public int DisconnectTimeout { get; set; }
		public int ReactivationTimeout { get; set; }
		public bool CopyConfig { get; set; }
		public bool StopInactive { get; set; }
	}
	public enum ReserveRoles
	{
		None = 0,
		Master = 1,
		Standby = 2
	}
	public static class MSTreeConsts
	{
		public const string LogFileSize = "LogFileSize";
		public const string NoStart = "Отмена запуска";
		public const string WorkDir = "Рабочий каталог";
		public const string ProgName = "ProgName";
		public const string PipeName = "PipeName";
		public const string PassiveMode = "Пассивный режим";
		public const string TaskPath = "Args";
		public const string TaskArguments = "Аргументы";
		public const string ConfFilePath = "Конф. файл";
		public const string ifpcore = "ifpcore";
		public const string pcsrv = "pcsrv";
		public const string rbsrv = "rbsrv";
		public const string pcsrv_old = "tmserv.dll";
		public const string rbsrv_old = "rbase.dll";
		public const string delta = "delta_pc";
		public const string delta_old = "delta_nt.exe";
		public const string tmcalc = "tmcalc_pc";
		public const string tmcalc_old = "tmcalc.exe";
		public const string ext_task = "_extern";
		public const string ext_task_old = "_ext_pc";
		public const string toposrv = "ElectricTopology";
		public const string gensrv = "_srv_.exe";
	}
}