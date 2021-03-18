using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Helpers
{
  public class IniManager : IDisposable
  {
    private const    uint      BuffSizeStep = 1024;
    private const    uint      BufSizeLimit = 0x200000;
    private readonly ITmNative _native;
    private          IntPtr    _filePointer;

    public IniManager(string filePath)
    {
      _native = new TmNative();

      _filePointer = _native.IniOpen(filePath);
    }


    public string GetPrivateString(string section, string key, string defaultResponse = "")
    {
      uint   bufSize = 0;
      byte[] buf;
      do
      {
        bufSize += BuffSizeStep;
        buf     =  new byte[bufSize];
        _native.IniReadString(_filePointer, section, key, defaultResponse, buf, bufSize);
      } while (buf[buf.Length - 2] != 0 && bufSize < BufSizeLimit);


      return EncodingUtil.Win1251BytesToUtf8(buf);
    }


    public Dictionary<string, string> GetPrivateSection(string section)
    {
      var  buf = new byte[BuffSizeStep];

      var returnSize = _native.IniReadSection(_filePointer, section, buf, BuffSizeStep);

      if (buf.Length < returnSize)
      {
        buf = new byte[returnSize];
        _native.IniReadSection(_filePointer, section, buf, returnSize);
      }
      
      var significantBytes = new byte[returnSize];
      Array.Copy(buf, significantBytes, returnSize);

      return EncodingUtil.Win1251BytesToUtf8(significantBytes)
                         .Split(new[] {'\0'}, StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Split('='))
                         .ToDictionary(x => x[0], x => x[1]);
    }


    public byte[] GetStruct(string section, string key, uint bufSize = 1024)
    {
      var  buf = new byte[bufSize];

      var returnSize = _native.IniReadStruct(_filePointer, section, key, buf, bufSize);
      
      if (buf.Length < returnSize)
      {
        buf = new byte[returnSize];
        _native.IniReadSection(_filePointer, section, buf, returnSize);
      }
      
      var significantBytes = new byte[returnSize];
      Array.Copy(buf, significantBytes, returnSize);

      return significantBytes;
    }


    public void Dispose()
    {
      _native.IniClose(_filePointer);
      _filePointer = IntPtr.Zero;
    }
  }
}