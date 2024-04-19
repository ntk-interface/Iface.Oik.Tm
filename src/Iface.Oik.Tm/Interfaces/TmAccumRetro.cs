using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public readonly struct TmAccumRetro : ITmAccumRetro
  {
    private readonly bool _invalidFlags;

    public float    Value { get; }
    public float    Load  { get; }
    public int?     Code  { get; }
    public TmFlags  Flags { get; }
    public DateTime Time  { get; }

    public bool IsValid => Flags >= 0 && 
                           !Flags.HasFlag(TmFlags.Unreliable) &&
                           !Value.Equals(float.MaxValue) && 
                           !_invalidFlags;

    public bool IsUnreliable => !IsValid;


    public TmAccumRetro(float value, float load, short flags, long timestamp, short? code = null)
    {
      Value         = value;
      Load          = load;
      Flags         = (TmFlags) flags;
      Time          = DateUtil.GetDateTimeFromTimestamp(timestamp);
      Code          = code;
      _invalidFlags = (flags < 0);
    }


    public static TmAccumRetro UnreliableValue = new TmAccumRetro(float.MaxValue,
                                                                  float.MaxValue,
                                                                  (short) TmFlags.Unreliable,
                                                                  0);
  }
}