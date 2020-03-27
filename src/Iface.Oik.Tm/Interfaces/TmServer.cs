using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmServer : TmNotifyPropertyChanged
  {
    private bool _isSelected;
    
    public string Name            { get; private set; }
    public string Comment         { get; private set; }
    public uint   Signature       { get; private set; }
    public uint   Unique          { get; private set; }
    public uint   ProcessId       { get; private set; }
    public uint   ParentProcessId { get; private set; }
    public uint   Flags           { get; private set; } //В текущей версии libif_cfs не используется
    public uint   DbgCnt          { get; private set; } //В текущей версии libif_cfs не используется
    public uint   LoudCnt         { get; private set; } //В текущей версии libif_cfs не используется
    public ulong  BytesIn         { get; private set; } //В текущей версии libif_cfs не используется
    public ulong  BytesOut        { get; private set; } //В текущей версии libif_cfs не используется

    public uint State { get; private set; }

    public DateTime? CreationTime { get; private set; }

    public uint ResState { get; private set; }

    public ObservableCollection<TmServer> Children { get; private set; }
    public TmServer                       Parent   { get; set; }
    public ObservableCollection<TmUser>   Users    { get; private set; }

    public bool IsSelected
    {
      get => _isSelected;
      set
      {
        _isSelected = value;
        NotifyOfPropertyChange();
      }
    }

    public TmServer()
    {
      Children = new ObservableCollection<TmServer>();
      Users    = new ObservableCollection<TmUser>();
    }

    public static TmServer CreateFromIfaceServer(TmNativeDefs.IfaceServer ifaceServer)
    {
      var tmServer = new TmServer();

      tmServer.Name            = EncodingUtil.Win1251BytesToUft8(ifaceServer.Name);
      tmServer.Comment         = EncodingUtil.Win1251BytesToUft8(ifaceServer.Comment);
      tmServer.Signature       = ifaceServer.Signature;
      tmServer.Unique          = ifaceServer.Unique;
      tmServer.ProcessId       = ifaceServer.Pid;
      tmServer.ParentProcessId = ifaceServer.Ppid;
      tmServer.Flags           = ifaceServer.Flags;
      tmServer.DbgCnt          = ifaceServer.DbgCnt;
      tmServer.LoudCnt         = ifaceServer.LoudCnt;
      tmServer.BytesIn         = ifaceServer.BytesIn;
      tmServer.BytesOut        = ifaceServer.BytesOut;
      tmServer.State           = ifaceServer.State;
      tmServer.CreationTime    = DateUtil.GetDateTimeFromTimestamp(ifaceServer.CreationTime);
      tmServer.ResState        = ifaceServer.ResState;

      return tmServer;
    }

    public TmServer GetChildTmServer(TmServer serverToFind)
    {
      foreach (var child in Children)
      {
        if (child.Unique == serverToFind.Unique)
        {
          return child;
        }

        var nexDepthTmServer = child.GetChildTmServer(serverToFind);
        if (nexDepthTmServer != null) return nexDepthTmServer;
      }

      return null;
    }

    public void DeleteChildTmServer(TmServer serverToDelete)
    {

    }
  }
}