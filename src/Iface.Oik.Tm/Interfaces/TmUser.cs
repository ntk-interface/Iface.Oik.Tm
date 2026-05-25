using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmUser : TmUserNotifiable, ITmServerTraceable
  {
    private int _hashCode;

    public          string Name      { get; private set; }
    public          string Comment   { get; private set; }
    public          uint   Signature { get; private set; }
    public          uint   Unique    { get; private set; }
    public          uint   ThreadId  { get; private set; }
    public override uint   ProcessId { get; protected set; }

    public             uint      Flags        { get; private set; } //В текущей версии libif_cfs не используется
    public             uint      DbgCnt       { get; private set; } //В текущей версии libif_cfs не используется
    public             uint      LoudCnt      { get; private set; } //В текущей версии libif_cfs не используется
    public             ulong     BytesIn      { get; private set; } //В текущей версии libif_cfs не используется
    public             ulong     BytesOut     { get; private set; } //В текущей версии libif_cfs не используется
    public             uint      Handle       { get; private set; } //В текущей версии libif_cfs не используется
    public             DateTime? CreationTime { get; private set; }

    public string DisplayName => $"{Name} {Comment}";

    public TmUser() {}
    
    public TmUser(int hashCode)
    {
      _hashCode = hashCode;
    }

    protected override void Initialize(InitializeDto dto)
    {
      _hashCode = (dto, dto, dto.Signature, dto.Unique).ToTuple().GetHashCode();

      Name         = dto.Name;
      Comment      = dto.Comment;
      Signature    = dto.Signature;
      Unique       = dto.Unique;
      ThreadId     = dto.ThreadId;
      ProcessId    = dto.ProcessId;
      Flags        = dto.Flags;
      DbgCnt       = dto.DbgCnt;
      LoudCnt      = dto.LoudCnt;
      BytesIn      = dto.BytesIn;
      BytesOut     = dto.BytesOut;
      CreationTime = DateUtil.GetDateTimeFromTimestamp(dto.Timestamp);
      Handle       = dto.Handle;
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
             && Comment      == comparison.Comment
             && Signature    == comparison.Signature
             && Unique       == comparison.Unique
             && ThreadId     == comparison.ThreadId
             && ProcessId    == comparison.ProcessId
             && Flags        == comparison.Flags
             && DbgCnt       == comparison.DbgCnt
             && LoudCnt      == comparison.LoudCnt
             && BytesIn      == comparison.BytesIn
             && BytesOut     == comparison.BytesOut
             && CreationTime == comparison.CreationTime
             && Handle       == comparison.Handle;
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