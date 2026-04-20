using System;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class TmServerBase<TUser> where TUser : TmUserBase
{
  internal static unsafe T Create<T>(TmNativeDefsUnsafe.IfaceServer serverData,
                                            IReadOnlyCollection<TUser>     users)
    where T : TmServerBase<TUser>, new()
  {
    var server = new T();

    var dto = new InitializeDto
    {
      Name            = TmNativeUtil.BytePtrToString(serverData.Name,    TmNativeDefsUnsafe.IfaceUserServerStringsSize),
      Comment         = TmNativeUtil.BytePtrToString(serverData.Comment, TmNativeDefsUnsafe.IfaceUserServerStringsSize),
      Signature       = serverData.Signature,
      Unique          = serverData.Unique,
      ProcessId       = serverData.Pid,
      ParentProcessId = serverData.Ppid,
      Flags           = serverData.Flags,
      DbgCnt          = serverData.DbgCnt,
      LoudCnt         = serverData.LoudCnt,
      BytesIn         = serverData.BytesIn,
      BytesOut        = serverData.BytesOut,
      State           = serverData.State,
      Timestamp       = serverData.CreationTime,
      ResState        = serverData.ResState,
      Users           = users
    };

    server.Initialize(dto);

    return server;
  }

  internal static unsafe T Create<T>(TmNativeDefsUnsafe.IfaceServer serverData)
    where T : TmServerBase<TUser>, new()
  {
    var server = new T();

    var dto = new InitializeDto
    {
      Name            = TmNativeUtil.BytePtrToString(serverData.Name,    TmNativeDefsUnsafe.IfaceUserServerStringsSize),
      Comment         = TmNativeUtil.BytePtrToString(serverData.Comment, TmNativeDefsUnsafe.IfaceUserServerStringsSize),
      Signature       = serverData.Signature,
      Unique          = serverData.Unique,
      ProcessId       = serverData.Pid,
      ParentProcessId = serverData.Ppid,
      Flags           = serverData.Flags,
      DbgCnt          = serverData.DbgCnt,
      LoudCnt         = serverData.LoudCnt,
      BytesIn         = serverData.BytesIn,
      BytesOut        = serverData.BytesOut,
      State           = serverData.State,
      Timestamp       = serverData.CreationTime,
      ResState        = serverData.ResState
    };

    server.Initialize(dto);
    
    return server;
  }

  protected abstract void Initialize(InitializeDto dto);

  protected record InitializeDto
  {
    public string                     Name            { get; init; } = string.Empty;
    public string                     Comment         { get; init; } = string.Empty;
    public uint                       Signature       { get; init; }
    public uint                       Unique          { get; init; }
    public uint                       ProcessId       { get; init; }
    public uint                       ParentProcessId { get; init; }
    public uint                       Flags           { get; init; } //В текущей версии libif_cfs не используется
    public uint                       DbgCnt          { get; init; } //В текущей версии libif_cfs не используется
    public uint                       LoudCnt         { get; init; } //В текущей версии libif_cfs не используется
    public ulong                      BytesIn         { get; init; } //В текущей версии libif_cfs не используется
    public ulong                      BytesOut        { get; init; } //В текущей версии libif_cfs не используется
    public uint                       State           { get; init; }
    public long                       Timestamp       { get; init; }
    public uint                       ResState        { get; init; }
    public IReadOnlyCollection<TUser> Users           { get; init; } = Array.Empty<TUser>();
  }
}