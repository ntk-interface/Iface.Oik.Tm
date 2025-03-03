using System;
using System.Collections.Generic;
using System.Linq;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class CfTreeNode
  {
    public CfTreeNode       Parent   { get; set; }
    public List<CfTreeNode> Children { get; set; }

    public string Name { get; set; }

    //public bool IsDirty;
    public bool                       Disabled     { get; set; }
    public Dictionary<string, string> CfProperties { get; set; }

    public CfTreeNode(string nodeName, CfTreeNode parent = null)
    {
      Name   = nodeName;
      Parent = parent;
    }
  }

  public class MSTreeNode
  {
    public string             ProgName   { get; }
    public MSTreeNode         Parent     { get; }
    public List<MSTreeNode>   Children   { get; set; } = new List<MSTreeNode>();
    public BaseNodeProperties Properties { get; protected set; }

    public MSTreeNode(string             progName,
                      BaseNodeProperties properties,
                      MSTreeNode         parent = null)
    {
      ProgName   = progName;
      Parent     = parent;
      Properties = properties;
    }


    public MSTreeNode(CfTreeNode cftNode, MSTreeNode parent = null)
    {
      Parent   = parent;
      ProgName = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.ProgName, string.Empty);
      var pipeName = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.PipeName, string.Empty);
      var noStart  = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.NoStart,  "0").Equals("1");

      Properties = null;
      if (cftNode.Name == "Master")
      {
        Properties = new MasterNodeProperties
        {
          LogFileSize = int.Parse(cftNode.CfProperties.ValueOrDefault(MSTreeConsts.LogFileSize, "0x80000")),
          InstallationName     = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.InstallationName, String.Empty)
        };
      }
      else
      {
        switch (ProgName)
        {
          case MSTreeConsts.TmServer:
            Properties = new NewTmsNodeProperties
            {
              // Пассивный режим только для TMS под Ifpcore
              PassiveMode = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.PassiveMode, "1") == "1",
            };
            break;
          case MSTreeConsts.RBaseServer:
            Properties = new RbsNodeProperties();
            break;
          case MSTreeConsts.TmCalc:
          case MSTreeConsts.tmcalc_old:
            Properties = new TmCalcNodeProperties();
            break;
          case MSTreeConsts.delta:
          case MSTreeConsts.delta_old:
          case MSTreeConsts.toposrv:
            Properties = new ChildNodeProperties();
            break;
          case MSTreeConsts.ExternalTask:
          case MSTreeConsts.ext_task_old:
            Properties = new ExternalTaskNodeProperties
            {
              // Зачем то в пути внешней задачи пробелы замеяются на табуляции
              TaskPath = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.TaskPath, string.Empty).Replace('\t', ' '),
              TaskArguments = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.TaskArguments, string.Empty),
              ConfigurationFilePath = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.ConfFilePath, string.Empty)
            };
            break;
          case MSTreeConsts.gensrv:
            var t = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.TaskPath, string.Empty).Trim();

            switch (t)
            {
              case MSTreeConsts.pcsrv_old:
                ProgName   = t;
                Properties = new ReservedNodeProperties();
                break;
              case MSTreeConsts.rbsrv_old:
                ProgName   = t;
                Properties = new RbsNodeProperties();
                break;
            }

            break;
          case MSTreeConsts.AutoBackup:
          {
            var exHourString = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.ExecutionHour, "16");


            Properties = new AutoBackupProperties
            {
              ExecutionHour    = int.TryParse(exHourString, out var exHour) ? exHour : 16,
              BackupsDirectory = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.BackupDirectory, string.Empty),
              ExcludeArchives  = cftNode.CfProperties.ValueOrDefault(MSTreeConsts.ExcludeArchives, "0") == "1"
            };
            break;
          }

          default:
            Properties = new BaseNodeProperties();
            break;
        }
      }

      if (Properties is MSTreeNodeProperties serviceNodeProperties)
      {
        serviceNodeProperties.NoStart = noStart;
      }

      if (Properties is ChildNodeProperties childNodeProperties)
      {
        childNodeProperties.PipeName = pipeName;
      }

      if (cftNode.Children?.Count > 0)
      {
        Children.AddRange(cftNode.Children.Select(x => new MSTreeNode(x, this)));
      }
    }
  }

  public class BaseNodeProperties
  {
  }

  public class MSTreeNodeProperties : BaseNodeProperties
  {
    public bool NoStart { get; set; }
  }

  public class MasterNodeProperties : MSTreeNodeProperties
  {
    public int    LogFileSize { get; set; }
    public string InstallationName     { get; set; }
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
    public RbsNodeProperties(short redirectorPort = 0)
    {
      RedirectorPort = redirectorPort;
    }

    public string DOC_Path       { get; set; } = "";
    public string BinPath        { get; set; } = "";
    public string DataPath       { get; set; } = "";
    public short  RedirectorPort { get; set; }
    public string RBF_Directory  { get; set; } = "";
    public string JournalSQLCS   { get; set; } = "";
    public string DTMX_SQLCS     { get; set; } = "";
    public bool   HasRedirector  => RedirectorPort != 0;
  }

  public class ExternalTaskNodeProperties : ChildNodeProperties
  {
    public string TaskPath              { get; set; } = "";
    public string TaskArguments         { get; set; } = "";
    public string ConfigurationFilePath { get; set; } = "";
  }

  public class TmCalcNodeProperties : ChildNodeProperties
  {
    public bool FUnr { get; set; }
    public bool SRel { get; set; }
  }

  public class ReservedNodeProperties : ChildNodeProperties
  {
    public short  Type         { get; set; }
    public string BindAddr     { get; set; } = "";
    public string Addr         { get; set; } = "";
    public short  Port         { get; set; }
    public short  BPort        { get; set; }
    public short  AbortTO      { get; set; } = 20;
    public short  RetakeTO     { get; set; } = 20;
    public bool   CopyConfig   { get; set; }
    public bool   StopInactive { get; set; }
  }

  public class AutoBackupProperties : MSTreeNodeProperties
  {
    public int    ExecutionHour    { get; set; }
    public string BackupsDirectory { get; set; } = string.Empty;
    public bool   ExcludeArchives  { get; set; }
  }

  public static class MSTreeConsts
  {
    public const string LogFileSize      = "LogFileSize";
    public const string NoStart          = "Отмена запуска";
    public const string InstallationName = "Место установки системы";
    public const string ProgName         = "ProgName";
    public const string PipeName         = "PipeName";
    public const string PassiveMode      = "Пассивный режим";
    public const string TaskPath         = "Args";
    public const string TaskArguments    = "Аргументы";
    public const string ConfFilePath     = "Конф. файл";
    public const string Portcore         = "portcore";
    public const string master           = "_master_.exe";
    public const string TmServer         = "pcsrv";
    public const string RBaseServer      = "rbsrv";
    public const string pcsrv_old        = "tmserv.dll";
    public const string rbsrv_old        = "rbase.dll";
    public const string delta            = "delta_pc";
    public const string delta_old        = "delta_nt.exe";
    public const string TmCalc           = "tmcalc_pc";
    public const string tmcalc_old       = "tmcalc.exe";
    public const string ExternalTask     = "_ext_pc";
    public const string ext_task_old     = "_extern";
    public const string toposrv          = "ElectricTopology";
    public const string gensrv           = "_srv_.exe";
    public const string RBS_Parameters   = "Parameters";
    public const string RBS_ClientParms  = "ClientParms";
    public const string RBS_PGParms      = "PGParms";
    public const string Tmcalc_FUnr      = "##FUnr";
    public const string Tmcalc_SRel      = "##SRel";
    public const string Tmcalc_Disabled  = "Disabled";
    public const string Tmcalc_Value     = "Value";

    public const string AutoBackup      = "abku_pc";
    public const string BackupDirectory = "Каталог назначения";
    public const string ExcludeArchives = "Исключить архивы";
    public const string ExecutionHour   = "Час начала бэкапа";
  }
}