using System;

namespace Iface.Oik.Tm.Native.Interfaces;

public class TmNativeException : Exception
{
  public uint Code { get; }

  public TmNativeException(string message) : base(message)
  {
    
  }
  
  public TmNativeException(string message, uint code) : base(message)
  {
    Code = code;
  }
}

public class TmNotSupportedException : TmNativeException
{
  public TmNotSupportedException() : base("Функция не поддерживается"){}
}