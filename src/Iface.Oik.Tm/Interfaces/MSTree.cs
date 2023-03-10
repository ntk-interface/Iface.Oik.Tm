using Iface.Oik.Tm.Interfaces;
using System;
using System.Collections.Generic;

namespace Iface.Oik.Tm.Interfaces
{
	public class MSTreeNode
	{
		public ICollection<MSTreeNode> Children { get; set; }
		public string ProgName { get; }
		public MSTreeNodeProperties Properties { get; protected set; }
		public MSTreeNode Parent { get; }

		public MSTreeNode()
		{
			Children = new List<MSTreeNode>();
		}

		public MSTreeNode(string progName,
						  MSTreeNode parent = null)
		{
			ProgName = progName;
			Parent = parent;
			Children = new List<MSTreeNode>();
		}
	}

	public class MasterNode : MSTreeNode
	{
		public MasterNode(string progName,
						  int logFileSize,
						  bool noStart,
						  string workDir)
		  : base(progName)
		{
			Properties = new MasterNodeProperties
			{
				LogFileSize = logFileSize,
				NoStart = noStart,
				WorkDir = workDir
			};
		}
	}

	public class TmsNode : MSTreeNode
	{
		public TmsNode(string progName,
					   MSTreeNode parent,
					   string pipeName,
					   bool noStart = false,
					   bool passiveMode = false)
		  : base(progName, parent)
		{
			Properties = new TmsNodeProperties
			{
				NoStart = noStart,
				PassiveMode = passiveMode,
				PipeName = pipeName
			};
		}
	}

	public class RbsNode : MSTreeNode
	{
		public RbsNode(string progName,
					   MSTreeNode parent,
					   string pipeName,
					   bool noStart = false)
		  : base(progName, parent)
		{
			Properties = new ChildNodeProperties
			{
				NoStart = noStart,
				PipeName = pipeName
			};
		}
	}

	public class DeltaNode : MSTreeNode
	{
		public DeltaNode(string progName,
						 MSTreeNode parent,
						 string pipeName,
						 bool noStart = false)
		  : base(progName, parent)
		{
			Properties = new ChildNodeProperties
			{
				NoStart = noStart,
				PipeName = pipeName
			};
		}
	}

	public class TmCalcNode : MSTreeNode
	{
		public TmCalcNode(string progName,
						  MSTreeNode parent,
						  string pipeName,
						  bool noStart = false)
		  : base(progName, parent)
		{
			Properties = new ChildNodeProperties
			{
				NoStart = noStart,
				PipeName = pipeName
			};
		}
	}

	public class ExternalTaskNode : MSTreeNode
	{
		public ExternalTaskNode(string progName,
								MSTreeNode parent,
								string pipeName,
								bool noStart = false,
								string taskPath = "",
								string taskArguments = "",
								string confFilePath = "")
		  : base(progName, parent)
		{
			Properties = new ExternalTaskNodeProperties
			{
				NoStart = noStart,
				PipeName = pipeName,
				TaskPath = taskPath,
				TaskArguments = taskArguments,
				ConfigurationFilePath = confFilePath
			};
		}
	}

	public class ElectricTopologyNode : MSTreeNode
	{
		public ElectricTopologyNode(string progName,
									MSTreeNode parent,
									string pipeName,
									bool noStart = false)
		  : base(progName, parent)
		{
			Properties = new ChildNodeProperties
			{
				NoStart = noStart,
				PipeName = pipeName
			};
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

	public class TmsNodeProperties : ChildNodeProperties
	{
		public bool PassiveMode { get; set; }
	}

	public class ExternalTaskNodeProperties : ChildNodeProperties
	{
		public string TaskPath { get; set; }
		public string TaskArguments { get; set; }
		public string ConfigurationFilePath { get; set; }
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
		public const string pcsrv = "pcsrv";
		public const string rbsrv = "rbsrv";
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