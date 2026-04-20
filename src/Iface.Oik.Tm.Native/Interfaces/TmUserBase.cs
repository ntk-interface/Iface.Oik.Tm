using System;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class TmUserBase
{
  public abstract uint ProcessId { get; protected set; }

  internal static unsafe T Create<T>(TmNativeDefsUnsafe.IfaceUser userData) where T : TmUserBase, new()
  {
    var user = new T();

    var dto = new InitializeDto
    {
      Name      = TmNativeUtil.BytePtrToString(userData.Name,    TmNativeDefsUnsafe.IfaceUserServerStringsSize),
      Comment   = TmNativeUtil.BytePtrToString(userData.Comment, TmNativeDefsUnsafe.IfaceUserServerStringsSize),
      Signature = userData.Signature,
      Unique    = userData.Unique,
      ThreadId  = userData.Thid,
      ProcessId = userData.Pid,
      Flags     = userData.Flags,
      DbgCnt    = userData.DbgCnt,
      LoudCnt   = userData.LoudCnt,
      BytesIn   = userData.BytesIn,
      BytesOut  = userData.BytesOut,
      Timestamp = userData.CreationTime,
      Handle    = userData.Handle
    };
    
    user.Initialize(dto);
    
    return user;
  }

  protected abstract void Initialize(InitializeDto dto);

  protected record InitializeDto
  {
    public string Name      { get; init; } = string.Empty;
    public string Comment   { get; init; } = string.Empty;
    public uint   Signature { get; init; }
    public uint   Unique    { get; init; }
    public uint   ThreadId  { get; init; }
    public uint   ProcessId { get; init; }
    public uint   Flags     { get; init; } //В текущей версии libif_cfs не используется
    public uint   DbgCnt    { get; init; } //В текущей версии libif_cfs не используется
    public uint   LoudCnt   { get; init; } //В текущей версии libif_cfs не используется
    public ulong  BytesIn   { get; init; } //В текущей версии libif_cfs не используется
    public ulong  BytesOut  { get; init; } //В текущей версии libif_cfs не используется
    public uint   Handle    { get; init; } //В текущей версии libif_cfs не используется
    public long   Timestamp { get; init; }
  }
}