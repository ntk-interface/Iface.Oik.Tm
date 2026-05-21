using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api;

public partial class TmsApi
{
  public async Task<IReadOnlyCollection<string>> GetFilesInDirectory(string path)
  {
    var cfCid = await GetCfCid().ConfigureAwait(false);
    if (cfCid == IntPtr.Zero)
    {
      Console.WriteLine("Ошибка при получении cfCid"); // todo
      return null;
    }

    const uint bufLength       = 8192;
    const int  errStringLength = 1000;
    var        buf             = new char[bufLength];
    var        errString       = new byte[errStringLength];
    uint       errCode         = 0;
    if (!await Task.Run(() => TmNative.cfsDirEnum(cfCid,
                                                  EncodingUtil.StringToBytes(path),
                                                  buf,
                                                  bufLength,
                                                  out errCode,
                                                  errString,
                                                  errStringLength))
                   .ConfigureAwait(false))
    {
      Console.WriteLine(
        $"Ошибка при запросе списка файлов: {errCode} - {EncodingUtil.BytesToString(errString)}");
      return null;
    }

    return TmNativeUtil.GetStringListFromDoubleNullTerminatedChars(buf);
  }


  public async Task<bool> DownloadFile(string remotePath, string localPath)
  {
    var cfCid = await GetCfCid().ConfigureAwait(false);
    if (cfCid == IntPtr.Zero)
    {
      Console.WriteLine("Ошибка при получении cfCid");
      return false;
    }

    var       fileTime        = new TmNativeDefs.FileTime();
    const int errStringLength = 1000;
    var       errString       = new byte[errStringLength];
    uint      errCode         = 0;
    if (!await Task.Run(() => TmNative.cfsFileGet(cfCid,
                                                  EncodingUtil.StringToBytes(remotePath),
                                                  EncodingUtil.StringToBytes(localPath),
                                                  60000,
                                                  ref fileTime,
                                                  out errCode,
                                                  errString,
                                                  errStringLength))
                   .ConfigureAwait(false))
    {
      Console.WriteLine($"Ошибка при скачивании файла: {errCode} - {EncodingUtil.BytesToString(errString)}");
      return false;
    }

    if (!File.Exists(localPath))
    {
      Console.WriteLine("Ошибка при сохранении файла в файловую систему");
      return false;
    }

    return true;
  }


  public async Task<IReadOnlyCollection<string>> GetComtradeDays()
  {
    var ptr = await Task.Run(() => TmNative.tmcComtradeEnumDays(_cid)).ConfigureAwait(false);

    return TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptr, 8192);
  }


  public async Task<IReadOnlyCollection<string>> GetComtradeFilesByDay(string day)
  {
    var ptr = await Task.Run(() => TmNative.tmcComtradeEnumFiles(_cid, EncodingUtil.StringToBytes(day)))
                        .ConfigureAwait(false);

    return TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptr, 8192);
  }


  public async Task<bool> DownloadComtradeFile(string filename, string localPath)
  {
    if (!await Task.Run(() => TmNative.tmcComtradeGetFile(_cid,
                                                          EncodingUtil.StringToBytes(filename),
                                                          EncodingUtil.StringToBytes(localPath)))
                   .ConfigureAwait(false))
    {
      Console.WriteLine($"Ошибка при скачивании файла: {GetLastTmcError()}");
      return false;
    }

    return true;
  }
}