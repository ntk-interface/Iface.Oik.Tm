using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmServer : TmNotifyPropertyChanged, ITmServerTraceable
  {
    private readonly int _hashCode;

    public string    Name            { get; private set; }
    public string    Comment         { get; private set; }
    public uint      Signature       { get; private set; }
    public uint      Unique          { get; private set; }
    public uint      ProcessId       { get; private set; }
    public uint      ThreadId        { get; } = 0xffffffff;
    public uint      ParentProcessId { get; private set; }
    public uint      Flags           { get; private set; } //В текущей версии libif_cfs не используется
    public uint      DbgCnt          { get; private set; } //В текущей версии libif_cfs не используется
    public uint      LoudCnt         { get; private set; } //В текущей версии libif_cfs не используется
    public ulong     BytesIn         { get; private set; } //В текущей версии libif_cfs не используется
    public ulong     BytesOut        { get; private set; } //В текущей версии libif_cfs не используется
    public uint      State           { get; private set; }
    public DateTime? CreationTime    { get; private set; }
    public uint      ResState        { get; private set; }

    public string                         DisplayName => $"{Name} {Comment}";
    public ObservableCollection<TmServer> Children    { get; private set; }
    public TmServer                       Parent      { get; set; }
    public ObservableCollection<TmUser>   Users       { get; private set; }

    public TmServer(int hashCode)
    {
      Children  = new ObservableCollection<TmServer>();
      Users     = new ObservableCollection<TmUser>();
      _hashCode = hashCode;
    }

    public static TmServer CreateFromIfaceServer(TmNativeDefs.IfaceServer ifaceServer)
    {
      var name    = EncodingUtil.Win1251BytesToUtf8(ifaceServer.Name);
      var comment = EncodingUtil.Win1251BytesToUtf8(ifaceServer.Comment);

      var tmServer = new TmServer((name, comment, ifaceServer.Signature, ifaceServer.Unique).ToTuple().GetHashCode())
                     {
                       Name            = name,
                       Comment         = comment,
                       Signature       = ifaceServer.Signature,
                       Unique          = ifaceServer.Unique,
                       ProcessId       = ifaceServer.Pid,
                       ParentProcessId = ifaceServer.Ppid,
                       Flags           = ifaceServer.Flags,
                       DbgCnt          = ifaceServer.DbgCnt,
                       LoudCnt         = ifaceServer.LoudCnt,
                       BytesIn         = ifaceServer.BytesIn,
                       BytesOut        = ifaceServer.BytesOut,
                       State           = ifaceServer.State,
                       CreationTime    = DateUtil.GetDateTimeFromTimestamp(ifaceServer.CreationTime),
                       ResState        = ifaceServer.ResState
                     };


      return tmServer;
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as TmServer);
    }


    public bool Equals(TmServer comparison)
    {
      if (ReferenceEquals(comparison, null))
      {
        return false;
      }

      if (ReferenceEquals(this, comparison))
      {
        return true;
      }

      var usersHashSet          = new HashSet<TmUser>(Users);
      var comparisonUserHashSet = new HashSet<TmUser>(comparison.Users);

      var childrenHashSet           = new HashSet<TmServer>(Children);
      var comparisonChildrenHashSet = new HashSet<TmServer>(comparison.Children);

      return Name            == comparison.Name
          && Comment         == comparison.Comment
          && Signature       == comparison.Signature
          && Unique          == comparison.Unique
          && ProcessId       == comparison.ProcessId
          && ParentProcessId == comparison.ParentProcessId
          && Flags           == comparison.Flags
          && DbgCnt          == comparison.DbgCnt
          && LoudCnt         == comparison.LoudCnt
          && BytesIn         == comparison.BytesIn
          && BytesOut        == comparison.BytesOut
          && State           == comparison.State
          && CreationTime    == comparison.CreationTime
          && ResState        == comparison.ResState
          && usersHashSet.SetEquals(comparisonUserHashSet)
          && childrenHashSet.SetEquals(comparisonChildrenHashSet);
    }


    public static bool operator ==(TmServer left, TmServer right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }

      return left.Equals(right);
    }

    public static bool operator !=(TmServer left, TmServer right)
    {
      return !(left == right);
    }

    public override int GetHashCode()
    {
      return _hashCode;
    }
  }
}