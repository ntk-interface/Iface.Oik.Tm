using System;
using System.Buffers;
using System.Collections.Generic;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static IReadOnlyCollection<TServer> GetTmServers<TServer, TUser>(nint cfCid)
    where TServer : TmServerBase<TUser>, new()
    where TUser : TmUserBase, new()
  {
    var serverIdsList = GetIfaceServerIds(cfCid);
    var tmUsers       = GetTmUsers<TUser>(cfCid);

    var tmServersList = new List<TServer>();

    foreach (var serverId in serverIdsList)
    {
      var serverData = GetIfaceServerData(cfCid, serverId);

      var tmServer = tmUsers.TryGetValue(serverData.Pid, out var users)
                       ? TmServerBase<TUser>.Create<TServer>(serverData, users)
                       : TmServerBase<TUser>.Create<TServer>(serverData);

      tmServersList.Add(tmServer);
    }

    return tmServersList;
  }


  public static IReadOnlyCollection<string> GetIfaceServerIds(nint cfCid)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var ptr = TmNative.cfsTraceEnumServers(cfCid,
                                             out var errCode,
                                             errBuf,
                                             TmNativeDefsUnsafe.ErrorBufSize);
      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      var ids = TmNativeUtil.GetStringsListFromIntPtr(ptr);

      TmNative.cfsFreeMemory(ptr);

      return ids;
    }
    finally
    {
      pool.Return(errBuf);
    }
  }


  public static Dictionary<uint, List<T>> GetTmUsers<T>(nint cfCid)
    where T : TmUserBase, new()
  {
    var result = new Dictionary<uint, List<T>>();

    foreach (var id in GetTmUsersIds(cfCid))
    {
      var user = TmUserBase.Create<T>(GetIfaceUserData(cfCid, id));

      if (result.TryGetValue(user.ProcessId, out var list))
      {
        list.Add(user);
        continue;
      }

      result.Add(user.ProcessId, new List<T> { user });
    }

    return result;
  }


  public static void RegisterTmServerTracer(nint cfCid,
                                            uint processId,
                                            uint threadId,
                                            bool debug,
                                            int  pause)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var result = TmNative.cfsTraceBeginTraceEx(cfCid,
                                                 processId,
                                                 threadId,
                                                 debug,
                                                 (uint)pause,
                                                 out var errCode,
                                                 errBuf,
                                                 TmNativeDefsUnsafe.ErrorBufSize);
      if (!result)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  public static IReadOnlyCollection<T> TraceTmServerLogRecords<T>(nint cfCid)
    where T : TmServerLogRecordBase, new()
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    var records = new List<T>();

    try
    {
      while (true)
      {
        var logRecordPtr = TmNative.cfsTraceGetMessage(cfCid,
                                                       out var errCode,
                                                       errBuf,
                                                       TmNativeDefsUnsafe.ErrorBufSize);
        if (logRecordPtr == nint.Zero)
        {
          break;
        }

        if (errCode != 0)
        {
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
        }

        var record = TmServerLogRecordBase.Create<T>(logRecordPtr);
        TmNative.cfsFreeMemory(logRecordPtr);

        records.Add(record);
      }

      return records;
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  public static void StopTmServerTrace(nint cfCid)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var result = TmNative.cfsTraceEndTrace(cfCid,
                                             out var errCode,
                                             errBuf,
                                             TmNativeDefsUnsafe.ErrorBufSize);
      if (!result)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      pool.Return(errBuf);
    }
  }



  internal static TmNativeDefsUnsafe.IfaceServer GetIfaceServerData(nint   cfCid,
                                                                    string serverId)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    var server = new TmNativeDefsUnsafe.IfaceServer();

    try
    {
      TmNative.cfsTraceGetServerData(cfCid,
                                     serverId,
                                     ref server,
                                     out var errCode,
                                     errBuf,
                                     TmNativeDefsUnsafe.ErrorBufSize);

      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      return server;
    }
    finally
    {
      pool.Return(errBuf);
    }
  }


  internal static IReadOnlyCollection<string> GetTmUsersIds(nint cfCid)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      var ptr = TmNative.cfsTraceEnumUsers(cfCid,
                                           out var errCode,
                                           errBuf,
                                           TmNativeDefsUnsafe.ErrorBufSize);
      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      var ids = TmNativeUtil.GetStringsListFromIntPtr(ptr);

      TmNative.cfsFreeMemory(ptr);

      return ids;
    }
    finally
    {
      pool.Return(errBuf);
    }
  }

  internal static TmNativeDefsUnsafe.IfaceUser GetIfaceUserData(nint   cfCid,
                                                                string userId)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    var user = new TmNativeDefsUnsafe.IfaceUser();

    try
    {
      TmNative.cfsTraceGetUserData(cfCid,
                                   userId,
                                   ref user,
                                   out var errCode,
                                   errBuf,
                                   TmNativeDefsUnsafe.ErrorBufSize);

      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }

      return user;
    }
    finally
    {
      pool.Return(errBuf);
    }
  }
}