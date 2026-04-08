using System;
using System.Buffers;
using System.IO;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  private const string BackupDateFormat = "dd_MM_yyyy (HH.mm.ss)";

  public static void BackupSecurity(nint cfCid, string directory, string pwd = "")
  {
    var fileName = "";
    string snp;

    if (string.IsNullOrEmpty(pwd))
    {
      var dto = GetComputerInfoS(cfCid);
      
      snp = "\x1";

      var namePart = dto.SecType switch
                     {
                       0 => "users.ini",
                       1 => "users.01",
                       2 => "users.02",
                       _ => throw new TmNativeException("Unknown security type", 1001)
                     };

      fileName += $"{namePart} {DateTime.Now:BackupDateFormat}.bbk";
    }
    else
    {
      snp      = "\x2";
      fileName = "Security-" + DateTime.Now.ToString(BackupDateFormat) + ".sbk";
    }
    
    BackupSecurity(cfCid, snp, pwd, Path.Combine(directory, fileName));
  }

  internal static void BackupSecurity(nint cfCid, 
                                      string snp, 
                                      string pwd, 
                                      string filename)
  {
    var pool   = ArrayPool<byte>.Shared;
    var errBuf = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);

    try
    {
      TmNative.cfsIfpcBackupSecurity(cfCid, 
                                     snp, 
                                     pwd, 
                                     filename, 
                                     out var errCode, 
                                     errBuf,
                                     TmNativeDefsUnsafe.ErrorBufSize);

      if (errCode != 0)
      {
        throw new TmNativeException(TmNativeUtil.BytesToString(errBuf), errCode);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(errBuf);
    }
  }
}