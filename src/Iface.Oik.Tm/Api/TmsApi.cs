using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api
{
  public partial class TmsApi : ITmsApi
  {
    private int _cid;


    public void SetCid(int cid)
    {
      _cid = cid;
    }


    public async Task<TmServerComputerInfo> GetServerComputerInfo()
    {
      var dto = await Task.Run(() => TmNativeApi.GetServerComputerInfo(_cid))
                          .ConfigureAwait(false);

      return new TmServerComputerInfo(dto);
    }


    public async Task<int> GetLastTmcError()
    {
      return await Task.Run(TmNativeApi.GetLastTmcError)
                       .ConfigureAwait(false);
    }


    public async Task<string> GetLastTmcErrorText()
    {
      return await Task.Run(() => TmNativeApi.GetLastTmcErrorText(_cid))
                       .ConfigureAwait(false);
    }


    public string GetConnectionErrorText()
    {
      return TmNativeApi.GetTmcConnectionErrorText(_cid);
    }


    public async Task<DateTime?> GetSystemTime()
    {
      return DateUtil.GetDateTimeFromTmString(await GetSystemTimeString().ConfigureAwait(false));
    }


    public async Task<string> GetSystemTimeString()
    {
      return await Task.Run(() => TmNativeApi.GetSystemTimeString(_cid))
                       .ConfigureAwait(false);
    }

    public async Task<(string host, string server)> GetCurrentServerName()
    {
      return await Task.Run(() => TmNativeApi.GetCurrentTmServerName(_cid))
                       .ConfigureAwait(false);
    }


    public async Task<(string user, string password)> GenerateTokenForExternalApp()
    {
      var cfCid = await GetCfCid().ConfigureAwait(false);

      return await Task.Run(() => TmNativeApi.GenerateTokenForExternalApp(cfCid))
                       .ConfigureAwait(false);
    }


    public async Task<nint> GetCfCid()
    {
      return await Task.Run(() => TmNativeApi.GetCfCid(_cid))
                       .ConfigureAwait(false);
    }


    public async Task StartTmAddrTracer(int channel, int rtu, int point, TmType tmType, TmTraceTypes filterTypes)
    {
      await Task.Run(() => TmNative.tmcSetTracer(_cid,
                                                 (short)channel,
                                                 (short)rtu,
                                                 (short)point,
                                                 (ushort)tmType.ToNativeType(),
                                                 (ushort)filterTypes))
                .ConfigureAwait(false);
    }


    public async Task StopTmAddrTracer(int channel, int rtu, int point, TmType tmType)
    {
      await Task.Run(() => TmNative.tmcSetTracer(_cid,
                                                 (short)channel,
                                                 (short)rtu,
                                                 (short)point,
                                                 (ushort)tmType.ToNativeType(),
                                                 (ushort)TmTraceTypes.None))
                .ConfigureAwait(false);
    }


    public async Task<TmServerInfo> GetServerInfo()
    {
      var info = new TmNativeDefs.TServerInfo();

      var result = await Task.Run(() => TmNative.tmcGetServerInfo(_cid, ref info)).ConfigureAwait(false);

      if (result != TmNativeDefs.Success)
      {
        Console.WriteLine(await GetLastTmcErrorText().ConfigureAwait(false));
        return null;
      }

      var (host, server) = await GetCurrentServerName().ConfigureAwait(false);
      return new TmServerInfo($"{host}\\{server}", info);
    }


    public async Task<IReadOnlyCollection<TmServerThread>> GetServerThreads()
    {
      return await Task.Run(() => TmNativeApi.GetTmServersThreads<TmServerThread>(_cid))
                       .ConfigureAwait(false);
    }


    public async Task<TmAccessRights> GetAccessRights()
    {
      uint access = 0;

      await Task.Run(() => TmNative.tmcGetGrantedAccess(_cid, out access)).ConfigureAwait(false);

      return (TmAccessRights)access;
    }


    public async Task<IReadOnlyCollection<TmUserInfo>> GetUsersInfo()
    {
      var usersIdPtr = await Task.Run(() => TmNative.tmcGetUserList(_cid)).ConfigureAwait(false);

      var tmUsersInfo = new List<TmUserInfo>();

      if (usersIdPtr == IntPtr.Zero)
      {
        Console.WriteLine("Ошибка получения списка пользователей ТМС");
        return tmUsersInfo;
      }

      var ptrWithOffset = usersIdPtr;

      while (true)
      {
        var id = Marshal.PtrToStructure<uint>(ptrWithOffset);
        if (id == 0)
        {
          break;
        }

        var user = await GetUserInfo(id).ConfigureAwait(false);

        if (user != null)
        {
          tmUsersInfo.Add(user);
        }

        ptrWithOffset = IntPtr.Add(ptrWithOffset, sizeof(uint));
      }

      return tmUsersInfo;
    }


    public async Task<TmUserInfo> GetUserInfo(uint userId)
    {
      var dto = await Task.Run(() => TmNativeApi.GetUserInfo(_cid, userId))
                          .ConfigureAwait(false);

      return new TmUserInfo(dto);
    }


    public async Task<TmUserInfo> GetExtendedUserInfo(int userId)
    {
      var dto = await Task.Run(() => TmNativeApi.GetExtendedUserInfo(_cid, (uint)userId))
                          .ConfigureAwait(false);

      return new TmUserInfo(dto);
    }
  }
}