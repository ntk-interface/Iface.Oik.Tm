using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Utils;

namespace GuiApp.AvaloniaUI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
  private readonly ICommonInfrastructure _infr;
  private readonly IOikDataApi           _api;

  [ObservableProperty] private string _host = string.Empty;

  [ObservableProperty] private string _tmServer = string.Empty;

  [ObservableProperty] private string _rbServer = string.Empty;

  [ObservableProperty] private string _user = string.Empty;

  [ObservableProperty] private string _password = string.Empty;

  [ObservableProperty] private string _channel = "0";

  [ObservableProperty] private string _rtu = "1";

  [ObservableProperty] private string _point = "1";

  [ObservableProperty] private bool _isConnected;

  public ObservableCollection<LogItem> Log { get; } = new();


  public MainWindowViewModel(ICommonInfrastructure infr,
                             IOikDataApi           api)
  {
    _infr = infr;
    _api  = api;

    var commandLineArgs = Environment.GetCommandLineArgs();
    TmServer = commandLineArgs.ElementAtOrDefault(1) ?? "TMS";
    RbServer = commandLineArgs.ElementAtOrDefault(2) ?? "RBS";
    Host     = commandLineArgs.ElementAtOrDefault(3) ?? ".";
    User     = commandLineArgs.ElementAtOrDefault(4) ?? "";
    Password = commandLineArgs.ElementAtOrDefault(5) ?? "";
  }


  [RelayCommand]
  public async Task Connect()
  {
    IsConnected = false;
    AddToLog("Идет соединение, ждите...");

    try
    {
      await Task.Run(() =>
      {
        var (tmCid, rbCid, rbPort, userInfo, serverFeatures) = Tms.Initialize(new TmInitializeOptions
        {
          ApplicationName = "GuiApp",
          Host            = Host,
          TmServer        = TmServer,
          RbServer        = RbServer,
          User            = User,
          Password        = Password,
        });
        _infr.InitializeTm(tmCid, rbCid, rbPort, userInfo, serverFeatures);
        _infr.ServerService.StartAsync();
      });

      IsConnected = true;
      AddToLog("Соединение установлено");
      await GetServerTime();
    }
    catch (Exception ex)
    {
      IsConnected = false;
      AddToLog("Ошибка: " + ex.Message);
    }
  }


  [RelayCommand]
  public void Disconnect()
  {
    _infr.ServerService.StopAsync();

    Tms.Terminate(_infr.TmCid, _infr.RbCid);
    _infr.TerminateTm();

    IsConnected = false;
    AddToLog("Соединение разорвано");
  }


  [RelayCommand]
  public async Task GetServerTime()
  {
    var serverTime = await _api.GetSystemTimeString();
    AddToLog($"Время на сервере: {serverTime}");
  }


  [RelayCommand]
  public async Task GetPresentAlarms()
  {
    AddToLog("Активные уставки:");
    var alarms = await _api.GetPresentAlarms();
    alarms?.ForEach(alarm => AddToLog($"{alarm.FullName}, {alarm.StateName}"));
  }


  [RelayCommand]
  public async Task GetPresentAps()
  {
    AddToLog("Активные АПС:");
    var aps = await _api.GetPresentAps();
    aps?.ForEach(AddToLog);
  }


  [RelayCommand]
  public async Task GetStatus()
  {
    if (!TmAddr.TryParse($"{Channel}:{Rtu}:{Point}", out var tmAddr, TmType.Status))
    {
      return;
    }

    var tmStatus = new TmStatus(tmAddr);
    await Task.WhenAll(_api.UpdateTagPropertiesAndClassData(tmStatus),
                       _api.UpdateStatus(tmStatus));
    AddToLog(tmStatus);
  }


  [RelayCommand]
  public async Task GetAnalog()
  {
    if (!TmAddr.TryParse($"{Channel}:{Rtu}:{Point}", out var tmAddr, TmType.Analog))
    {
      return;
    }

    var tmAnalog = new TmAnalog(tmAddr);
    await Task.WhenAll(_api.UpdateTagPropertiesAndClassData(tmAnalog),
                       _api.UpdateAnalog(tmAnalog));
    AddToLog(tmAnalog);
  }


  [RelayCommand]
  public void ClearLog()
  {
    Log.Clear();
  }


  private void AddToLog(object message)
  {
    Log.Add(new LogItem(message));
  }
}


public class LogItem
{
  public DateTime Time    { get; }
  public string   Message { get; }


  public LogItem(object message)
  {
    Time    = DateTime.Now;
    Message = message.ToString() ?? string.Empty;
  }
}