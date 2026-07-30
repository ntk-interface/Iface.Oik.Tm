using System;
using System.Collections.Generic;
using System.Linq;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Helpers;

public static class Pkcs11
{
  public static IReadOnlyCollection<string> GetAvailableVendors()
  {
    var ptr = TmNative.cfsPkcs11EnumVendors();
    if (ptr == nint.Zero)
    {
      return new List<string>();
    }

    try
    {
      return TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptr, 1024);
    }
    finally
    {
      TmNative.cfsFreeMemory(ptr);
    }
  }


  public static void SetVendor(string vendor)
  {
    if (!TmNative.cfsPkcs11SetVendor(vendor))
    {
      throw new TmNativeException("Ошибка установки поставщика смарт-карты");
    }
  }


  public static IReadOnlyCollection<Pkcs11Certificate> GetAvailableCertificates()
  {
    const int  errBufSize = 1024;
    Span<byte> errBuf     = stackalloc byte[errBufSize];

    if (!TmNative.cfsPkcs11EnumCertificates(errBuf, errBufSize, out var ptrNames, out var ptrAttrs))
    {
      throw new TmNativeException(TmNativeUtil.BytesToString(errBuf));
    }

    var names = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptrNames, 8192);
    var attrs = TmNativeUtil.GetStringListFromDoubleNullTerminatedPointer(ptrAttrs, 8192);

    TmNative.cfsFreeMemory(ptrNames);
    TmNative.cfsFreeMemory(ptrAttrs);
    
    if (names.Count != attrs.Count)
    {
      throw new TmNativeException("Не совпадает количество сертификатов и атрибутов смарт-карты");
    }

    var result = new List<Pkcs11Certificate>();
    for (var i = 0; i < names.Count; i++)
    {
      result.Add(new Pkcs11Certificate(names.ElementAt(i), attrs.ElementAt(i)));
    }
    return result;
  }
}