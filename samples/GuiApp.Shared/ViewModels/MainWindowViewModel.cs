using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GuiApp.Shared.Utils;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Utils;

namespace GuiApp.Shared.ViewModels
{
  public class MainWindowViewModel : TmNotifyPropertyChanged
  {
    private readonly ICommonInfrastructure _infr;
    private readonly IOikDataApi           _api;

    private string _host;

    public string Host
    {
      get => _host;
      set => SetPropertyValue(ref _host, value);
    }

    private string _tmServer;

    public string TmServer
    {
      get => _tmServer;
      set => SetPropertyValue(ref _tmServer, value);
    }

    private string _rbServer;

    public string RbServer
    {
      get => _rbServer;
      set => SetPropertyValue(ref _rbServer, value);
    }

    private string _user;

    public string User
    {
      get => _user;
      set => SetPropertyValue(ref _user, value);
    }

    private string _password;

    public string Password
    {
      get => _password;
      set => SetPropertyValue(ref _password, value);
    }

    private string _channel = "0";

    public string Channel
    {
      get => _channel;
      set => SetPropertyValue(ref _channel, value);
    }

    private string _rtu = "1";

    public string Rtu
    {
      get => _rtu;
      set => SetPropertyValue(ref _rtu, value);
    }

    private string _point = "1";

    public string Point
    {
      get => _point;
      set => SetPropertyValue(ref _point, value);
    }

    private bool _isConnected;

    public bool IsConnected
    {
      get => _isConnected;
      set => SetPropertyValue(ref _isConnected, value);
    }

    public ObservableCollection<LogItem> Log { get; } = new ObservableCollection<LogItem>();

    public ICommand ConnectCommand          { get; }
    public ICommand DisconnectCommand       { get; }
    public ICommand GetServerTimeCommand    { get; }
    public ICommand GetPresentAlarmsCommand { get; }
    public ICommand GetPresentApsCommand    { get; }
    public ICommand GetStatusCommand        { get; }
    public ICommand GetAnalogCommand        { get; }
    public ICommand ClearLogCommand         { get; }

    public event EventHandler AddedToLog = delegate { };


    // пустой конструктор для Avalonia UI
    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(ICommonInfrastructure infr,
                         IOikDataApi           api)
    {
      _infr = infr;
      _api  = api;

      ConnectCommand          = new Command(Connect);
      DisconnectCommand       = new Command(Disconnect);
      GetServerTimeCommand    = new Command(GetServerTime);
      GetPresentAlarmsCommand = new Command(GetPresentAlarms);
      GetPresentApsCommand    = new Command(GetPresentAps);
      GetStatusCommand        = new Command(GetStatus);
      GetAnalogCommand        = new Command(GetAnalog);
      ClearLogCommand         = new Command(ClearLog);

      var commandLineArgs = Environment.GetCommandLineArgs();
      TmServer = commandLineArgs.ElementAtOrDefault(1) ?? "TMS";
      RbServer = commandLineArgs.ElementAtOrDefault(2) ?? "RBS";
      Host     = commandLineArgs.ElementAtOrDefault(3) ?? ".";
      User     = commandLineArgs.ElementAtOrDefault(4) ?? "";
      Password = commandLineArgs.ElementAtOrDefault(5) ?? "";
    }


    private async void Connect()
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
        GetServerTime();
      }
      catch (Exception ex)
      {
        IsConnected = false;
        AddToLog("Ошибка: " + ex.Message);
      }
    }


    private void Disconnect()
    {
      _infr.ServerService.StopAsync();
      
      Tms.Terminate(_infr.TmCid, _infr.RbCid);
      _infr.TerminateTm();

      IsConnected = false;
      AddToLog("Соединение разорвано");
    }


    private async void GetServerTime()
    {
      var serverTime = await _api.GetSystemTimeString();
      AddToLog($"Время на сервере: {serverTime}");
    }


    private async void GetPresentAlarms()
    {
      AddToLog("Активные уставки:");
      var alarms = await _api.GetPresentAlarms();
      alarms?.ForEach(alarm => AddToLog($"{alarm.FullName}, {alarm.StateName}"));
    }


    private async void GetPresentAps()
    {
      AddToLog("Активные АПС:");
      var aps = await _api.GetPresentAps();
      aps?.ForEach(AddToLog);
    }


    private async void GetStatus()
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


    private async void GetAnalog()
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


    private void ClearLog()
    {
      Log.Clear();
    }


    private void AddToLog(object message)
    {
      Log.Add(new LogItem(message));

      AddedToLog.Invoke(this, EventArgs.Empty);
    }
  }


  public class LogItem
  {
    public DateTime Time    { get; }
    public string   Message { get; }


    public LogItem(object message)
    {
      Time    = DateTime.Now;
      Message = message.ToString();
    }
  }
}