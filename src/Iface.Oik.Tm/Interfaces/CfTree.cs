using Iface.Oik.Tm.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
	public class CfTreeNode
	{
		public string Name { get; }
		public CfTreeNode Parent { get; }
		public Dictionary<string, string> CfProperties { get; set; }
		public List<CfTreeNode> Children { get; set; }
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
		public List<MSTreeNode> Children { get; set; } = new List<MSTreeNode>();
		public MSTreeNodeProperties Properties { get; protected set; }
		public MSTreeNode(CfTreeNode cft_node, MSTreeNode parent = null)
		{
			Parent = parent;
			ProgName = cft_node.CfProperties.GetValueOrDefault( MSTreeConsts.ProgName, String.Empty);
			string pipeName = cft_node.CfProperties.GetValueOrDefault(MSTreeConsts.PipeName, String.Empty);
			bool noStart = cft_node.CfProperties.GetValueOrDefault(MSTreeConsts.NoStart, "0").Equals("1");

			Properties = null;
			if (cft_node.Name == "Master")
			{
				Properties = new MasterNodeProperties
				{
					LogFileSize = int.Parse(cft_node.CfProperties.GetValueOrDefault(MSTreeConsts.LogFileSize, "0x80000")),
					WorkDir = cft_node.CfProperties.GetValueOrDefault(MSTreeConsts.WorkDir, String.Empty)
				};
			}
			else
			{
				switch (ProgName)
				{
					case MSTreeConsts.pcsrv:
						Properties = new NewTmsNodeProperties
						{
							// Пассивный режим только для TMS под Ifpcore
							PassiveMode = cft_node.CfProperties.GetValueOrDefault(MSTreeConsts.PassiveMode, "1").Equals("1"),
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
							TaskPath = cft_node.CfProperties.GetValueOrDefault(MSTreeConsts.TaskPath, String.Empty),
							TaskArguments = cft_node.CfProperties.GetValueOrDefault(MSTreeConsts.TaskArguments, String.Empty),
							ConfigurationFilePath = cft_node.CfProperties.GetValueOrDefault(MSTreeConsts.ConfFilePath, String.Empty)
						};
						break;
					case MSTreeConsts.gensrv:
						string t = cft_node.CfProperties.GetValueOrDefault(MSTreeConsts.TaskPath, String.Empty).Trim();

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
			}
			if (Properties == null)
				Properties = new MSTreeNodeProperties();
			Properties.NoStart = noStart;
			if (Properties is ChildNodeProperties p)
			{
				p.PipeName = pipeName;
			}
			foreach (var child in cft_node.Children)
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
		public string DOC_Path { get; set; }
		public string BinPath { get; set; }
		public string DataPath { get; set; }
		public short PostgresPort { get; set; }
		public string RBF_Directory { get; set; }
		public string JournalSQLCS { get; set; }
		public string DTMX_SQLCS { get; set; }
	}
	public class ExternalTaskNodeProperties : ChildNodeProperties
	{
		public string TaskPath { get; set; }
		public string TaskArguments { get; set; }
		public string ConfigurationFilePath { get; set; }
	}
	public class ReservedNodeProperties : ChildNodeProperties
	{
		public ReserveRoles Type { get; set; } = ReserveRoles.None;
		public string BindAddr { get; set; } = "";
		public string Addr { get; set; } = "";
		public short Port { get; set; }
		public short BPort { get; set; }
		public short AbortTO { get; set; }
		public short RetakeTO { get; set; }
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
		public const string master = "_master_.exe";
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