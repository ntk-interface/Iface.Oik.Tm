using System;
using System.Linq;

namespace Iface.Oik.Tm.IntegrationTest.Util;

public static class CommonUtil
{
  public const string TaskName = "IFACE_OIK_TM_TEST";
  
  
  public static (string, string, string, string, string) ParseCommandLineArgs()
  {
    var args = Environment.GetCommandLineArgs();

    return (args.ElementAtOrDefault(1) ?? ".",
            args.ElementAtOrDefault(2) ?? "TMS",
            args.ElementAtOrDefault(3) ?? "RBS",
            args.ElementAtOrDefault(4) ?? "",
            args.ElementAtOrDefault(5) ?? "");
  }
}