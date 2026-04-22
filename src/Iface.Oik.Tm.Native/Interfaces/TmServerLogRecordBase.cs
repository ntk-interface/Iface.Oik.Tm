using System;
using System.Buffers;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Interfaces;

public abstract class TmServerLogRecordBase
{
  public abstract DateTime? DateTime { get; protected set; }
  
  internal static T Create<T>(Span<byte> recordString) 
    where T: TmServerLogRecordBase, new()
  {
    const int bufSize = 128;
    
    var rec = new T();

    var pool = ArrayPool<byte>.Shared;
    
    var buffers = new byte[6][];

    for (var i = 0; i < 6; i++)
    {
      buffers[i] = pool.Rent(bufSize);
    }
    
    try
    {
      var msgPtr = TmNative.lf_ParseMessage(recordString,
                                            buffers[0],
                                            buffers[1],
                                            buffers[2],
                                            buffers[3],
                                            buffers[4],
                                            buffers[5]);

      if (msgPtr == nint.Zero)
      {
        throw new TmNativeException("Ошибка парсинга строки журнала событий");
      }

      var dto = new InitializeDto
      {
        Time     = TmNativeUtil.BytesToString(buffers[0]).Trim(' ', '\n'),
        Date     = TmNativeUtil.BytesToString(buffers[1]).Trim(' ', '\n'),
        Name     = TmNativeUtil.BytesToString(buffers[2]).Trim(' ', '\n'),
        Type     = TmNativeUtil.BytesToString(buffers[3]).Trim(' ', '\n'),
        MsgType  = TmNativeUtil.BytesToString(buffers[4]).Trim(' ', '\n'),
        ThreadId = TmNativeUtil.BytesToString(buffers[5]).Trim(' ', '\n'),
        Message  = TmNativeUtil.GetCStringFromIntPtr(msgPtr).Trim(' ', '\n'),
      };
      
      rec.Initialize(dto);
    }
    finally
    {
      for (var i = 0; i < 6; i++)
      {
        pool.Return(buffers[i]);
      }
    }
    

    return rec;
  }

  internal static unsafe T Create<T>(nint recordPtr)
    where T : TmServerLogRecordBase, new()
  {
    if (recordPtr == nint.Zero)
    {
      throw new ArgumentNullException(nameof(recordPtr));
    }

    var ptr    = (byte*)recordPtr;
    var length = 0;
    
    while (ptr[length] != 0)
    {
      length++;
    }

    var span = new Span<byte>(ptr, length);

    return Create<T>(span);
  }
  
  protected abstract void Initialize(InitializeDto dto);

  protected record InitializeDto
  {
    public string Time     { get; init; } = string.Empty;
    public string Date     { get; init; } = string.Empty;
    public string Name     { get; init; } = string.Empty;
    public string Type     { get; init; } = string.Empty;
    public string MsgType  { get; init; } = string.Empty;
    public string ThreadId { get; init; } = string.Empty;
    public string Message  { get; init; } = string.Empty;
  }
  
}