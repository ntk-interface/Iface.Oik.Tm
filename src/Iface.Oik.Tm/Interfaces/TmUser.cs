using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmUser : TmNotifyPropertyChanged
  {
    public string    Name            { get; private set; }
    public string    Comment         { get; private set; }
    public uint      Signature       { get; private set; }
    public uint      Unique          { get; private set; }
    public uint      ThreadId        { get; private set; }
    public uint      ParentProcessId { get; private set; }
    public uint      Flags           { get; private set; } //В текущей версии libif_cfs не используется
    public uint      DbgCnt          { get; private set; } //В текущей версии libif_cfs не используется
    public uint      LoudCnt         { get; private set; } //В текущей версии libif_cfs не используется
    public ulong     BytesIn         { get; private set; } //В текущей версии libif_cfs не используется
    public ulong     BytesOut        { get; private set; } //В текущей версии libif_cfs не используется
    public uint      Handle          { get; private set; } //В текущей версии libif_cfs не используется
    public DateTime? CreationTime    { get; private set; }

    public static TmUser CreateFromIfaceUser(TmNativeDefs.IfaceUser ifaceUser)
    {
      var tmUser = new TmUser();

      tmUser.Name            = EncodingUtil.Win1251BytesToUft8(ifaceUser.Name);
      tmUser.Comment         = EncodingUtil.Win1251BytesToUft8(ifaceUser.Comment);
      tmUser.Signature       = ifaceUser.Signature;
      tmUser.Unique          = ifaceUser.Unique;
      tmUser.ThreadId        = ifaceUser.Thid;
      tmUser.ParentProcessId = ifaceUser.Pid;
      tmUser.Flags           = ifaceUser.Flags;
      tmUser.DbgCnt          = ifaceUser.DbgCnt;
      tmUser.LoudCnt         = ifaceUser.LoudCnt;
      tmUser.BytesIn         = ifaceUser.BytesIn;
      tmUser.BytesOut        = ifaceUser.BytesOut;
      tmUser.CreationTime    = DateUtil.GetDateTimeFromTimestamp(ifaceUser.CreationTime);
      tmUser.Handle          = ifaceUser.Handle;

      return tmUser;
    }
  }
}