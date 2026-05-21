using System;
using System.Linq;
using System.Threading.Tasks;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;

const string applicationName = "SimpleConsoleAppWithoutSql";

var commandLineArgs = Environment.GetCommandLineArgs();

var tmServer = commandLineArgs.ElementAtOrDefault(1) ?? "TMS";
var host     = commandLineArgs.ElementAtOrDefault(2) ?? ".";
var user     = commandLineArgs.ElementAtOrDefault(3) ?? "";
var password = commandLineArgs.ElementAtOrDefault(4) ?? "";

try
{
  Tms.InitNativeLibrary();
  
  Tms.SetUserCredentials(user, password);

  var tmCid = Tms.Connect(host,
                          tmServer,
                          applicationName,
                          Tms.EmptyTmCallbackDelegate,
                          IntPtr.Zero);
  if (tmCid == 0)
  {
    throw new Exception("Нет связи с сервером!");
  }

  var api = new TmsApi();
  api.SetCid(tmCid);

  while (true)
  {
    Console.WriteLine(await api.GetSystemTimeString());
    
    await Task.Delay(1000);
  }
}
catch (Exception ex)
{
  Tms.PrintError(ex.Message);
}