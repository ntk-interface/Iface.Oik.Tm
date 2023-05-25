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
		public CfTreeNode Parent { get; set; }
		public List<CfTreeNode> Children { get; set; }
		public string Name { get; set; }
		//public bool IsDirty;
		public bool Disabled { get; set;  }
		public Dictionary<string, string> CfProperties { get; set; }
		public CfTreeNode(string nodeName, CfTreeNode parent = null)
		{
			Name = nodeName;
			Parent = parent;
			//Children = new List<CfTreeNode>();
		}
	}
	public class MSTreeNode
	{
		public string ProgName { get; }
		public MSTreeNode Parent { get; }
		public List<MSTreeNode> Children { get; set; } 
		public MSTreeNodeProperties Properties { get; protected set; }
		public MSTreeNode(CfTreeNode cft_node, MSTreeNode parent = null)
		{
			Parent = parent;
			ProgName = cft_node.CfProperties.ValueOrDefault( MSTreeConsts.ProgName, String.Empty);
			string pipeName = cft_node.CfProperties.ValueOrDefault(MSTreeConsts.PipeName, String.Empty);
			bool noStart = cft_node.CfProperties.ValueOrDefault(MSTreeConsts.NoStart, "0").Equals("1");

			Properties = null;
			if (cft_node.Name == "Master")
			{
				Properties = new MasterNodeProperties
				{
					LogFileSize = int.Parse(cft_node.CfProperties.ValueOrDefault(MSTreeConsts.LogFileSize, "0x80000")),
					WorkDir = cft_node.CfProperties.ValueOrDefault(MSTreeConsts.WorkDir, String.Empty)
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
							PassiveMode = cft_node.CfProperties.ValueOrDefault(MSTreeConsts.PassiveMode, "1").Equals("1"),
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
							// Зачем то в пути внешней задачи пробелы замеяются на табуляции
							TaskPath = cft_node.CfProperties.ValueOrDefault(MSTreeConsts.TaskPath, String.Empty).Replace('\t', ' '),
							TaskArguments = cft_node.CfProperties.ValueOrDefault(MSTreeConsts.TaskArguments, String.Empty),
							ConfigurationFilePath = cft_node.CfProperties.ValueOrDefault(MSTreeConsts.ConfFilePath, String.Empty)
						};
						break;
					case MSTreeConsts.gensrv:
						string t = cft_node.CfProperties.ValueOrDefault(MSTreeConsts.TaskPath, String.Empty).Trim();

						if (t.Equals(MSTreeConsts.pcsrv_old))
						{
							ProgName = t;
							Properties = new ReservedNodeProperties();
						}
						else
						{
							if (t.Equals(MSTreeConsts.rbsrv_old))
							{
								ProgName = t;
								Properties = new RbsNodeProperties();
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
			if (cft_node.Children != null && cft_node.Children.Count > 0)
			{
				Children = new List<MSTreeNode>();
				foreach (var child in cft_node.Children)
				{
					Children.Add(new MSTreeNode(child, this));
				}
			}
			else
				Children = null;
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
		public string DOC_Path { get; set; } = "";
		public string BinPath { get; set; } = "";
		public string DataPath { get; set; } = "";
		public short RedirectorPort { get; set; }
		public string RBF_Directory { get; set; } = "";
		public string JournalSQLCS { get; set; } = "";
		public string DTMX_SQLCS { get; set; } = "";
	}
	public class ExternalTaskNodeProperties : ChildNodeProperties
	{
		public string TaskPath { get; set; } = "";
		public string TaskArguments { get; set; } = "";
		public string ConfigurationFilePath { get; set; } = "";
	}
	public class ReservedNodeProperties : ChildNodeProperties
	{
		public short Type { get; set; } = 0;
		public string BindAddr { get; set; } = "";
		public string Addr { get; set; } = "";
		public short Port { get; set; }
		public short BPort { get; set; }
		public short AbortTO { get; set; } = 20;
		public short RetakeTO { get; set; } = 20;
		public bool CopyConfig { get; set; } 
		public bool StopInactive { get; set; }
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
		public const string portcore = "portcore";
		public const string master = "_master_.exe";
		public const string pcsrv = "pcsrv";
		public const string rbsrv = "rbsrv";
		public const string pcsrv_old = "tmserv.dll";
		public const string rbsrv_old = "rbase.dll";
		public const string delta = "delta_pc";
		public const string delta_old = "delta_nt.exe";
		public const string tmcalc = "tmcalc_pc";
		public const string tmcalc_old = "tmcalc.exe";
		public const string ext_task = "_ext_pc";
		public const string ext_task_old = "_extern";
		public const string toposrv = "ElectricTopology";
		public const string gensrv = "_srv_.exe";
		public const string RBS_Parameters = "Parameters";
		public const string RBS_ClientParms = "ClientParms";
		public const string RBS_PGParms = "PGParms";
	}
}