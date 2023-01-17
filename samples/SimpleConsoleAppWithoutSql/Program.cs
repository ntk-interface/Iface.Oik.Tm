using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iface.Oik.Tm.Api;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Native.Api;

const string applicationName = "SimpleConsoleAppWithoutSql";

var commandLineArgs = Environment.GetCommandLineArgs();

var tmServer = commandLineArgs.ElementAtOrDefault(1) ?? "TMS";
var host     = commandLineArgs.ElementAtOrDefault(2) ?? ".";
var user     = commandLineArgs.ElementAtOrDefault(3) ?? "";
var password = commandLineArgs.ElementAtOrDefault(4) ?? "";

try
{
  Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // требуется для работы с кодировкой Win-1251
  
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

  var api = new TmsApi(new TmNative());
  api.SetCidAndUserInfo(tmCid, Tms.GetUserInfo(tmCid, tmServer));

  while (true)
  {
    Console.WriteLine(await api.GetSystemTimeString());
    
    await Task.Delay(1000).ConfigureAwait(false);
  }
}
catch (Exception ex)
{
  Tms.PrintError(ex.Message);
}