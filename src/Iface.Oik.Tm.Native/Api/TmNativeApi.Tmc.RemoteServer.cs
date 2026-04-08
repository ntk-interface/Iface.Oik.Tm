using System.Buffers;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Api;

public static partial class TmNativeApi
{
  public static ComputerInfoDto GetServerComputerInfo(int tmCid)
  {
    var cfCid = TmNative.tmcGetCfsHandle(tmCid);

    if (cfCid == nint.Zero)
    {
      throw new TmNativeException("Не удалось получить cfsHandle");
    }
    
    var  pool    = ArrayPool<byte>.Shared;
    var  errBuf  = pool.Rent(TmNativeDefsUnsafe.ErrorBufSize);
    uint errCode = 0;

    unsafe
    {
      var cis = new TmNativeDefsUnsafe.ComputerInfoS
      {
        Len = (uint)sizeof(TmNativeDefsUnsafe.ComputerInfoS)
      };

      try
      {
        if (! TmNative.cfsGetComputerInfo(cfCid,
                                          ref cis,
                                          out errCode,
                                          errBuf,
                                          TmNativeDefsUnsafe.ErrorBufSize))
        {
          throw new TmNativeException(TmNativeUtil.BytesToString(errBuf));
        }
        
      }
      finally
      {
        ArrayPool<byte>.Shared.Return(errBuf);
      }
      
      return ComputerInfoDto.Create(cis);
    }
  }
}