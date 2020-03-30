using System;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmUser : TmNotifyPropertyChanged
  {
    private readonly int _hashCode;
    
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


    public TmUser(int hashCode)
    {
      _hashCode = hashCode;
    }

    public static TmUser CreateFromIfaceUser(TmNativeDefs.IfaceUser ifaceUser)
    {
      
      var name = EncodingUtil.Win1251BytesToUft8(ifaceUser.Name);
      var comment = EncodingUtil.Win1251BytesToUft8(ifaceUser.Comment);

      var tmUser = new TmUser((name, comment, ifaceUser.Signature, ifaceUser.Unique).ToTuple().GetHashCode())
                   {
                     Name            = name,
                     Comment         = comment,
                     Signature       = ifaceUser.Signature,
                     Unique          = ifaceUser.Unique,
                     ThreadId        = ifaceUser.Thid,
                     ParentProcessId = ifaceUser.Pid,
                     Flags           = ifaceUser.Flags,
                     DbgCnt          = ifaceUser.DbgCnt,
                     LoudCnt         = ifaceUser.LoudCnt,
                     BytesIn         = ifaceUser.BytesIn,
                     BytesOut        = ifaceUser.BytesOut,
                     CreationTime    = DateUtil.GetDateTimeFromTimestamp(ifaceUser.CreationTime),
                     Handle          = ifaceUser.Handle
                   };


      return tmUser;
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as TmUser);
    }


    public bool Equals(TmUser comparison)
    {
      if (ReferenceEquals(comparison, null))
      {
        return false;
      }

      if (ReferenceEquals(this, comparison))
      {
        return true;
      }

      return Name            == comparison.Name
          && Comment         == comparison.Comment
          && Signature       == comparison.Signature
          && Unique          == comparison.Unique
          && ThreadId        == comparison.ThreadId
          && ParentProcessId == comparison.ParentProcessId
          && Flags           == comparison.Flags
          && DbgCnt          == comparison.DbgCnt
          && LoudCnt         == comparison.LoudCnt
          && BytesIn         == comparison.BytesIn
          && BytesOut        == comparison.BytesOut
          && CreationTime    == comparison.CreationTime
          && Handle          == comparison.Handle;
    }


    public static bool operator ==(TmUser left, TmUser right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }

      return left.Equals(right);
    }

    public static bool operator !=(TmUser left, TmUser right)
    {
      return !(left == right);
    }

    public override int GetHashCode()
    {
      return _hashCode;
    }
  }
}